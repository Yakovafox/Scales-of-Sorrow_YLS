using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Unity.Jobs;
using Random = UnityEngine.Random;
using WaitForSeconds = UnityEngine.WaitForSeconds;


public enum EnemyStates
{
    Idle,
    Moving,
    Fly,
    Chase,
    Attack,
    ThreatenedAttack,
    Special
}

public class EnemyStateMachine : MonoBehaviour
{



    [Tooltip ("Starting state for the AI")]
    [SerializeField] private EnemyStates currentState = EnemyStates.Idle;



    [Tooltip("Points in the world the AI should navigate to and from")]
    [SerializeField] public GameObject patrolPoints_Parent;

    [Tooltip ("Scriptable Object reference that holds all the variables for this enemy type")]
    [SerializeField] private EnemyData_ScriptableObj myData_SO;

    #region DEBUG TOGGLES
    [Header("Debug Toggles")]
    [Tooltip("Toggle on the gizmos for visualising the Sight ranges of the AI.")]
    [SerializeField]
    bool Toggle_Sight_Debug;
    [SerializeField]
    private GameObject debugSphere;
    #endregion



    #region Local Variables
    //Local Variables

    private float currentHealth = 150f;
    private int stagesLeft;

    public List<GameObject> PlayerRef;
    
    private NavMeshAgent agent;
    private float defaultAgentRadius;
    private Vector3 investigationArea;

    private bool isAITarget;

    private bool intialiseMovement = false;

    private Vector3 targetsLastSeenLocation;

    private GameObject GO_shadowCaster;
    private SpriteRenderer spriteRenderer;
    private Collider hitCollider;

    private GameObject InstantiatePosition;
    private GameObject shieldRef;




    [Header("Timer Variables")]
    private float state_Timer = 0.0f;
    private float _waitTime = 0.0f;

    private float attack_Timer = 0.0f;
    private float attack_WaitTime = 0.0f;

    private float flying_Timer = 0.0f;
    private float flying_WaitTime = 0.0f;

    private float flyCooldown_Timer = 0.0f;
    private float flyCooldown_WaitTime = 0.0f;
    
    private float ability_Timer = 0.0f;
    private float ability_WaitTime = 0.0f;

    private float abilityCooldown_Timer = 0.0f;
    private float abilityCooldown_WaitTime = 0.0f;

    private float debug_Timer = 0.0f;
    private float debug_WaitTime = 0.0f;

    private Transform groundChecker;
    private Transform sightPosition;

    private float defaultYPos;

    private Ray ray;
    private RaycastHit rayResult;
    private RaycastHit colHit;



    [Header("Boolean Variables")]

    private bool shouldWait;

    private bool stunned;

    private bool flyingToRandomPoint;

    private bool doOnce;
    private bool doOneCamShake;

    private bool initialiseSpecial;
    private bool shouldSpecial;
    private bool specialActive;
    private bool water_specialActive;
    private bool firedUp = false;

    private bool pushback_VelocityShouldReset = false;

    private bool movingRight = false;

    #endregion
    

    #region DragonEvents
    public delegate void delegate_dragonLanded();
    public static event delegate_dragonLanded OnDragonLanded;

    
    #endregion


    public virtual void Awake()
    {
        sightPosition = transform.Find("SightPoint");
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = myData_SO.walkSpeed;
        defaultAgentRadius = agent.radius;
        hitCollider = gameObject.GetComponent<Collider>();

        GO_shadowCaster = gameObject.transform.Find("shadowCaster").gameObject;
        spriteRenderer = gameObject.transform.Find("CharacterBillboard").GetComponent<SpriteRenderer>();
        InstantiatePosition = sightPosition.transform.Find("InstantiatePoint").gameObject;
        myData_SO.Target = null;
        
        StartCoroutine(searchForPlayerInScene());

        //Functionality for setting the Dragon health on startup.
        currentHealth = myData_SO.MaxHealth;
        stagesLeft = myData_SO.Stages;
    }


    void Update() //Functions should be called from here where Update holds the logic for what should happen in each state, and the Functions hold the functionality of what happens.
    {
        if (PlayerRef.Count == 0) { return;}
        state_Timer += Time.deltaTime;
        attack_Timer += Time.deltaTime;
        flyCooldown_Timer += Time.deltaTime;
        abilityCooldown_Timer += Time.deltaTime;
        debug_Timer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyStates.Idle:
                if (stunned)
                {
                    if (TimeOut(myData_SO.stunTime))
                    {
                        spriteRenderer.transform.position = new Vector3(0, spriteRenderer.transform.position.y, 0);
                        attack_WaitTime = 0;
                        intialiseMovement = false;
                        agent.isStopped = false;
                        ChangeState(EnemyStates.Moving);
                        stunned = false;
                    }
                }
                else if (TimeOut(myData_SO.timeToWait))
                {
                    attack_WaitTime = 0;
                    intialiseMovement = false;
                    agent.isStopped = false;
                    ChangeState(EnemyStates.Moving);
                }
                break;

            case EnemyStates.Moving:
                if (!intialiseMovement) // Initialise Movement is only called the first time that the enemy enters the moving state.
                {
                    intialiseMovement = true;
                    
                    RandomisedMovePoint();
                }
                else if (myData_SO.canSee && SeenTarget()) 
                {
                    ChangeState(EnemyStates.Chase);
                }
                else if (ReachedDestination() && myData_SO.canFly && flyCooldown() && randomShouldFly())
                {
                    doOnce = true;
                    flying_Timer = 0;
                    TakeOff();
                }
                else if (ReachedDestination() && !SeenTarget())
                {
                    RandomisedMovePoint();
                }
                break;
            
            case EnemyStates.Fly:
                flying_Timer += Time.deltaTime;

                //Functions for flying should go here.
                if (!flyTimeOut() && !flyingToRandomPoint)
                {
                    aggresiveChase();
                }
                else if (flyTimeOut() && !flyingToRandomPoint)
                {
                    RandomisedMovePoint();
                    flyingToRandomPoint = true;
                }
                else if (flyTimeOut() && flyingToRandomPoint)
                {
                    if (ReachedDestination())
                    {
                        doOnce = true;
                        Land();
                    }
                }

                //Breakdown of what should happen in this state:
                //Chase the player around
                //if the times out, land.
                break;

            case EnemyStates.Chase:
                MoveToChase();

                
                if (debugTimeOut(1f) && movementStuck())
                {
                    Debug.LogWarning("I got stuck!");
                    ChangeState(EnemyStates.Idle);
                    //Refresh This state or move to another state.
                }
                else if (AttackCooldown(myData_SO.attackCooldown) && ReachedDestination()) // Need some more code here to define what to do with attack as its missing a bit to define if it should attack.
                {
                    doOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.Attack);
                    //Change state to Attack
                }
                else if (AttackCooldown(myData_SO.attackCooldown) && InAttackRange(myData_SO.rangedAttackDistance))
                {
                    doOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.Attack); 
                    //Change State to attack!
                }
                else if (myData_SO.canSee && SeenTarget())
                {
                    ChangeState(EnemyStates.Chase);
                }
                break;

            case EnemyStates.Attack: // Need a do once check in here to stop the code from being executed multiple times over.
                if (!doOnce)
                {
                    ChangeState(EnemyStates.Chase);
                }
                else if (doOnce)
                {
                    if (shouldSpecial && shouldSpecialAttack() && abilityCooldown())
                    {
                        doOnce = false;
                        initialiseSpecial = true;
                        ability_Timer = 0;
                        ChangeState(EnemyStates.Special);
                        break;
                        //Special Attack 
                    }
                    if (InAttackRange(myData_SO.meleeAttackDistance))
                    {
                        doOnce = false;
                        BasicAttack();
                        //Melee Attack
                    }
                    else if (InAttackRange(myData_SO.rangedAttackDistance))
                    {
                        doOnce = false;
                        RangedAttack();
                        // ShootProjectile
                    }

                    doOnce = false;
                    attack_Timer = 0;
                }

                break;

            case EnemyStates.ThreatenedAttack: //Used by special ability attacking.

                if (!doOnce)
                {
                    doOnce = false;
                    ChangeState(EnemyStates.Special);
                }
                else if (doOnce)
                {

                    if (InAttackRange(myData_SO.meleeAttackDistance))
                    {
                        doOnce = false;
                        BasicAttack();
                        //Melee Attack
                    }
                    else if (InAttackRange(myData_SO.rangedAttackDistance))
                    {
                        doOnce = false;
                        RangedAttack();
                        // ShootProjectile
                    }

                    doOnce = false;
                    attack_Timer = 0;
                }
                break;
            
            case EnemyStates.Special:
                ability_Timer += Time.deltaTime;
                specialActive = true;
                
                // Initialise special ability.
                if (initialiseSpecial)
                {
                    initialiseSpecial = false;
                    initialiseSpecialAbility();
                }
                if (abilityTimeOut())
                {
                    print("ability timed out!");
                    exitSpecialAbility();
                    //Reset timer when leaving this state.
                    ChangeState(EnemyStates.Idle);
                    specialActive = false;
                    abilityCooldown_Timer = 0;
                }
                
                // Consistently chase down player into range and attack them.
                aggresiveChase();

                if (AttackCooldown(5f) && ReachedDestination())
                {
                    doOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.ThreatenedAttack);
                    //Change state to Threatened Attack (Special version of attacking)
                }
                else if (AttackCooldown(5f) && InAttackRange(myData_SO.rangedAttackDistance))
                {
                    doOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.ThreatenedAttack);
                    //Change State to Threatened attack! (Special version of attacking.)
                }
                //Execute any special functionality that the dragon has. (Example: Dashing toward the player.)
                StartCoroutine(specialFunctionality());
                
                // Any Special ability that can be done should happen here. (Example the electro dragons dash.)
                //Enter attack state and loop back to hear after attack?
                break;
        }

        if (agent.desiredVelocity.x > 0 && agent.desiredVelocity.z < 0) { movingRight = true; }
        else movingRight = false;

        UpdateSprite();
    }


    void ChangeState(EnemyStates newState)
    {
        currentState = newState;
        state_Timer = 0;
        debug_Timer = 0;
    }

    #region Player2Functions
    public void player2Joined()
    {
        if (PlayerRef.Count > 1) { return; }
        GameObject[] tempPlayerArray = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < tempPlayerArray.Length; i++)
        {
            if (PlayerRef.Contains(tempPlayerArray[i])) { continue; }
            PlayerRef.Add(tempPlayerArray[i]);
        }
        //Code to adjust Health here or attack damage etc.
    }
    
    public void player2Left()
    {
        if (PlayerRef.Count <= 1) { return; }
        PlayerRef.RemoveAt(1);
        //Code to adjust Health here or attack damage etc.
    }

    bool doesP2Exist()
    {
        return PlayerRef.Count > 1;
    }

    GameObject findClosestPlayer()
    {
        if (!doesP2Exist()) return PlayerRef[0];
        float p1Dist = Vector3.Distance(transform.position, PlayerRef[0].transform.position);
        float p2Dist = Vector3.Distance(transform.position, PlayerRef[1].transform.position);
        if (p2Dist < p1Dist){return PlayerRef[1]; }
        return PlayerRef[0];
    }

    GameObject returnPlayerWithID(int playerID)
    {
        if (PlayerRef.Count <= 1) { return PlayerRef[0].gameObject; }
        for (int i = 0; i < PlayerRef.Count; i++)
        {
            if(PlayerRef[i].GetComponent<PlayerController>().playerID == playerID)
            {
                return PlayerRef[i].gameObject;
            }
            continue;
        }
        return PlayerRef[0].gameObject;
    }
    #endregion

    #region Health Functions

    public void ReceiveDamage(float incomingDamage, int playerID)
    {
        if(specialActive && dirOverlapsWithShield(playerID)) { return; }
        if(NHS_HealthCheckup(incomingDamage) > 0)
        {
            currentHealth -= incomingDamage;
            return;
        }
        healthReachedZero();
    }

    private float NHS_HealthCheckup(float incomingDamage)
    {
        return currentHealth - incomingDamage;
    }

    private bool dirOverlapsWithShield(int playerID)
    {

        bool result = false;
        GameObject playerContext = returnPlayerWithID(playerID);

        Vector3 playerDir = playerContext.transform.position - transform.position;
        ray = new Ray(sightPosition.position, playerDir);
        if(Physics.Raycast(ray, out colHit))
        {
            if (colHit.transform.gameObject == shieldRef)
            {
                return true;
            }
        }

        return false;
    }

    private void healthReachedZero()
    {
        //Double check that health is below zero.
        if(stagesLeft <= 0)
        {
            Debug.Log("Dragon has been defeated!!!!");
            return;
            //DefeatOfDragon
        }
        stagesLeft -= 1;
        currentHealth = myData_SO.MaxHealth;
    }

    #endregion

    #region Movement Functions
    void RandomisedMovePoint()
    {
        //Randomise x and z point in the world within a sphere starting from current location. Y position must be set to 0.
        float randXPos = Random.Range(1, myData_SO.moveRadius);
        float randZPos = Random.Range(1, myData_SO.moveRadius);
        Vector3 randomSearchPos = new Vector3(randXPos, 0, randZPos);

        //Check if the randomised point is viable. This could be done using boundaries.
        Collider[] overlapingCols = Physics.OverlapSphere(randomSearchPos, 1f);
        debugSphere.transform.position = randomSearchPos;

        for(int i = 0; i < overlapingCols.Length; i++) // There's a bug with some logic here that means Invalid position log always shows.
        {
            if (overlapingCols[i] == null) { continue; }
            if(overlapingCols[i].gameObject.layer != myData_SO.GroundLayer.value)
            {
                Debug.LogWarning("Invalid position");
            }
        }
        MoveToNextPoint(randomSearchPos);
    }

    void MoveToNextPoint(Vector3 randomPos)
    {
        agent.SetDestination(randomPos);
    }

    void MoveToSetPoint(Vector3 position)
    {
        agent.SetDestination(position); 
    }

    bool movementStuck()
    {
        if (ReachedDestination() && agent.velocity == Vector3.zero) return true;
        return false;
    }
    void UpdateSprite()
    {
        float xScale = 1;
        //^ used as a default for flipping the sprite from left to right.

        if (movingRight)
        {
            xScale = -1;
        }
        transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
    }

    bool randomShouldFly()
    {
        return myData_SO.chanceToFly > Random.Range(0f, 100f);
    }

    bool ReachedDestination()
    {
        if (agent.remainingDistance < agent.stoppingDistance && !agent.pathPending)
        {
            debug_Timer = 0;
        }
        //Stopping distance needs to be higher than 0.
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;
    }
  
    bool GroundCheck()
    {
        Collider[] SphereCastArray = new Collider[3];
        Physics.OverlapSphereNonAlloc(groundChecker.position, 0.5f, SphereCastArray, myData_SO.GroundLayer);

        for (int i = 0; i < SphereCastArray.Length; i++)
        {
            if (SphereCastArray[i].gameObject.layer == myData_SO.GroundLayer) { return true; }
        }
        return false;
    }


    #endregion
    
    #region chase Functions
    
    void MoveToChase()
    {
        agent.destination = targetsLastSeenLocation;
    }

    private Vector3 GetPlayerDirection(GameObject playerToUse)
    {
        return (playerToUse.transform.position - sightPosition.position).normalized;
    }

    void aggresiveChase()
    {
        targetsLastSeenLocation = findClosestPlayer().transform.position;
        
        agent.destination = targetsLastSeenLocation;
        Debug.Log("Aggressively chasoing!!!!");
    }

    bool SeenTarget()
    {
        bool result = false;

        GameObject closestPlayer = findClosestPlayer();
        float playerDistance = Vector3.Distance(transform.position, closestPlayer.transform.position);

        if (playerDistance < myData_SO.sightDistance)
        {
            Vector3 targetDirection = (closestPlayer.transform.position - sightPosition.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, targetDirection);

            if (angleToPlayer < myData_SO.sightAngle / 2)
            {
                Vector3 sightPos;

                sightPos = sightPosition.transform.position;

                //Crouching cast from lower would go here.

                ray = new Ray(sightPos, targetDirection);

                if (Physics.Raycast(ray, out rayResult))
                {
                    switch (rayResult.transform.gameObject.tag)
                    {
                        case "Player":
                            result = true;
                            targetsLastSeenLocation = closestPlayer.transform.position;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        return result;
    }


    bool InAttackRange(float checkDistance) //I think I need to rewrite this to use a sphere cast check instead of agent remaining distance as it just doesnt make sense.
    {
        //This could change to return distance to closest player?;
        //Raycast a sphere check in area defined by meleeAttackDistance variable. Damage Player if player is in this area.
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, checkDistance);
        //This needs to change to be an array

        //Potential further check here to see if within a set distance to do damage or just push back?
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].CompareTag("Player"))
            {
                return true;
                //Deal Damage to player;
            }
        }
        return false;
        // Original code: return agent.remainingDistance < myData_SO.meleeAttackDistance && !agent.pathPending;
    }
    
    #endregion
    
    #region Flight Functions

    void TakeOff()
    {
        StartCoroutine(TakingOffSpriteRise());
        if (doOnce)
        {
            doOnce = false;
            flyingToRandomPoint = false;
            agent.speed = myData_SO.flySpeed;
            hitCollider.enabled = false;
            //Change State to flying.
            //Play dragon lifting off animation here.

            //Set Dragon sprite rendered to disabled.
            //spriteRenderer.enabled = false;
            //The above elements must time correctly otherwise the dragon will appear back at the bottom of the screen.
            //Potentially move the sprite higher out of view as well?
            defaultYPos = spriteRenderer.gameObject.transform.position.y;
            ChangeState(EnemyStates.Fly);
        }
    }

    void Land()
    {
        StartCoroutine(LandingLerp());
        doOneCamShake = true;
        if (doOnce)
        {
            doOnce = false;
            //Potentially move the sprite back into view if deciding to keep sprite out of view
            agent.speed = myData_SO.walkSpeed;
            hitCollider.enabled = true;
            //PLay dragon landing animation here.

            //Set Dragon sprite rendered to disabled.
            //spriteRenderer.enabled = true;
            //The above elements must time correctly otherwise the dragon will appear back at the bottom of the screen.
            flyCooldown_Timer = 0;
            stunned = true;
            ChangeState(EnemyStates.Idle);
        }
        //Grow Shadow Over time as animation draws to end.

    }
    IEnumerator TakingOffSpriteRise()
    {
        while (spriteRenderer.transform.position.y <= 15f)
        {
            float step = 50f * Time.deltaTime;
            Vector3 targetHeight = new Vector3(spriteRenderer.gameObject.transform.position.x, 15f, spriteRenderer.gameObject.transform.position.z);
            spriteRenderer.gameObject.transform.position = Vector3.MoveTowards(spriteRenderer.gameObject.transform.position, targetHeight, step);

            if (GO_shadowCaster.transform.localScale.x >= myData_SO.shadow_MinSize)
            {
                GO_shadowCaster.transform.localScale -= GO_shadowCaster.transform.localScale * (3.5f * Time.deltaTime);
            }
            yield return new WaitForSeconds(0.03f);
        }
        spriteRenderer.transform.position = new Vector3(spriteRenderer.transform.position.x, 15f, spriteRenderer.transform.position.z);
        GO_shadowCaster.transform.localScale = new Vector3(myData_SO.shadow_MinSize, myData_SO.shadow_MinSize, myData_SO.shadow_MinSize);
    }

    IEnumerator LandingSpriteLand(Vector3 targetHeight)
    {
        while (spriteRenderer.transform.position.y >= defaultYPos)
        {
            float step = 50f * Time.deltaTime;
            spriteRenderer.gameObject.transform.position = Vector3.MoveTowards(spriteRenderer.gameObject.transform.position, targetHeight, step);

            if (GO_shadowCaster.transform.localScale.x < myData_SO.shadow_DefaultSize)
            {
                GO_shadowCaster.transform.localScale += GO_shadowCaster.transform.localScale * (3.5f * Time.deltaTime);
            }
            if(spriteRenderer.transform.position.y <= defaultYPos + 0.75f && spriteRenderer.transform.position.y >= defaultYPos && doOneCamShake)
            {
                doOneCamShake = false;
                OnDragonLanded?.Invoke();
                landingPushBack();
            }
            yield return new WaitForSeconds(0.03f);
        }
        spriteRenderer.transform.position = new Vector3(spriteRenderer.transform.position.x, defaultYPos, spriteRenderer.transform.position.z);
        GO_shadowCaster.transform.localScale = new Vector3(myData_SO.shadow_DefaultSize, myData_SO.shadow_DefaultSize, myData_SO.shadow_DefaultSize);
    }

    IEnumerator LandingLerp()
    {
        Vector3 targetHeight = new Vector3(spriteRenderer.gameObject.transform.position.x, defaultYPos, spriteRenderer.gameObject.transform.position.z);
        float distance = Vector3.Distance(targetHeight, spriteRenderer.gameObject.transform.position);
        float waitTime = distance / (655f * Time.deltaTime);
        print("This is the time to wait" + waitTime);
        StartCoroutine(LandingSpriteLand(targetHeight));
        yield return new WaitForSeconds(waitTime);
    }

    #endregion

    #region Timer Functions
    bool TimeOut(float timeToUse)
    {
        _waitTime = timeToUse;
        return state_Timer > _waitTime;
    }

    bool flyTimeOut()
    {
        flying_WaitTime = myData_SO.flightTime;
        return flying_Timer > flying_WaitTime;
    }
    bool flyCooldown()
    {
        flyCooldown_WaitTime = myData_SO.flightCooldownTime;
        return flyCooldown_Timer > flyCooldown_WaitTime;
    }

    bool abilityTimeOut()
    {
        ability_WaitTime = myData_SO.ability_Timer;
        return ability_Timer > ability_WaitTime;
    }

    bool abilityCooldown()
    {
        abilityCooldown_WaitTime = myData_SO.ability_cooldownTime;
        return abilityCooldown_Timer > abilityCooldown_WaitTime;
    }

    bool debugTimeOut(float timeToUse)
    {
        debug_WaitTime = timeToUse;
        return debug_Timer > debug_WaitTime;
    }

    bool AttackCooldown(float TimeToUse)
    {
        attack_WaitTime = TimeToUse;
        return attack_Timer > attack_WaitTime;
    }
    #endregion

    #region AttackFunctions

    bool shouldSpecialAttack()
    {
        if (!shouldSpecial) { return false; }
        shouldSpecial = false;
        
        return myData_SO.specialAttackChance > Random.Range(0,100);
    }

    protected virtual void BasicAttack()
    {
        float damageToDeal = 20f;
        if (firedUp)
        { damageToDeal = damageToDeal * myData_SO.fireup_DamageMultiplier; }
        //Cause Player Damage here or effect that can cause damage.
        if (specialActive)
        {
            print("From MELEE: special is active and returning to special state.");
            ChangeState(EnemyStates.Special); return;
        }

        Collider[] tempHitArray = Physics.OverlapSphere(transform.position, myData_SO.meleeAttackDistance);
        for(int i = 0; i < tempHitArray.Length; i++)
        {
            if (tempHitArray[i].gameObject.CompareTag("Player"))
            {
                tempHitArray[i].gameObject.GetComponent<PlayerController>().TakeDamage(damageToDeal);
            }
        }
        ChangeState(EnemyStates.Idle);
    }

    protected virtual void RangedAttack()
    { 
        float damageToDeal = 10f;
        if (firedUp)
        { damageToDeal = damageToDeal * myData_SO.fireup_DamageMultiplier; }
        
        GameObject projectileInstance = Instantiate(myData_SO.rangedProjectile, sightPosition.position, Quaternion.identity); // This works but needs a prefab in it disabled for development.
        projectileInstance.GetComponent<Scr_Projectile>().Accessor_dir = GetPlayerDirection(findClosestPlayer());
        projectileInstance.GetComponent<Scr_Projectile>().Accessor_damageToDeal = damageToDeal;
        agent.isStopped = true;

        if (specialActive) 
        {
            print("From RANGED: special is active and returning to special state.");
            ChangeState(EnemyStates.Special); 
            return; 
        }

        ChangeState(EnemyStates.Idle);

    }

    private List<GameObject> checkPlayersDistance()
    {
        //Check if Player2 exists.
        int activePlayers = 1;
        List<GameObject> playersToPushBack = new List<GameObject>();
        if (doesP2Exist()){ activePlayers = 2; }
        
        for (int i = 0; i < activePlayers; i++)
        {
            if (Vector3.Distance(transform.position, PlayerRef[i].transform.position) <= myData_SO.pushBackDistance)// this line is the reason only one player gets pushed back
            {
                playersToPushBack.Add(PlayerRef[i].gameObject);
            }
        }
        
        return playersToPushBack;
    }

    private void landingPushBack()
    {
        //Call function that check to see if players are in range.
        List<GameObject> playersImPushingBack = checkPlayersDistance();

        for (int i = 0; i < playersImPushingBack.Count; i++)
        {
            //Push back player
            Rigidbody rb = playersImPushingBack[i].GetComponent<Rigidbody>();

            Vector3 dir = playersImPushingBack[i].transform.position - transform.position;
            Vector3 dirAndForce = new Vector3(dir.x * myData_SO.pushBackAmount, 0, dir.z * myData_SO.pushBackAmount);
            pushback_VelocityShouldReset = true;
            StartCoroutine(UpdatePlayerVelocityZero(rb));
            rb.AddForce(dirAndForce, ForceMode.Impulse);
            pushback_VelocityShouldReset = false;
            Debug.Log("Pushing back player: " + playersImPushingBack[i].name);
        }
    }

    private IEnumerator UpdatePlayerVelocityZero(Rigidbody rb)
    {
        while (pushback_VelocityShouldReset)
        {
            rb.velocity = Vector3.zero;
            yield return new WaitForSeconds(0.01f);
        }
    }

    protected virtual void initialiseSpecialAbility()
    {
        Debug.Log("Initialising Special Ability");
        //Setup any functionality for the ability here, spawn in shield etc.
        //In base machine setup all abilities at once 
        if (!myData_SO.Shield.IsUnityNull())
        {
            shieldRef = Instantiate(myData_SO.Shield, InstantiatePosition.transform.position, Quaternion.identity);
            shieldRef.transform.parent = InstantiatePosition.transform;
        }

        if (!firedUp) StartCoroutine(specialFunctionality());
    }

    protected virtual void exitSpecialAbility()
    {
        Debug.Log("Exiting Special Ability");
        if (!shieldRef.IsUnityNull())
        {
            Destroy(shieldRef);
        }
        if(firedUp) firedUp = false;
    }

    protected virtual IEnumerator specialFunctionality()
    {
        if (!firedUp)
        {
            yield return new WaitForSeconds(myData_SO.fireup_ChargeTime);
            firedUp = true;
        }
        yield return null;

    }
    #endregion


    IEnumerator searchForPlayerInScene()
    {
        while (PlayerRef.Count == 0)
        {
            GameObject[] tempPlayerArray = GameObject.FindGameObjectsWithTag("Player");
            if (tempPlayerArray.Length > 0 && tempPlayerArray.Length <= 1)
            {
                PlayerRef.Add(tempPlayerArray[0]);
                yield return new WaitForSeconds(0.03f);
            }
            for (int i = 0; i < tempPlayerArray.Length; i++)
            {
                if(PlayerRef.Contains(tempPlayerArray[i])) continue;
                PlayerRef.Add(tempPlayerArray[i]);
                //Should check ID of the player and should organise this list based on player with lowest ID!!!
                 
                //myData_SO.Target.Add(tempPlayerArray[i]); //Is broken and stops code?
                 
            }
            yield return new WaitForSeconds(0.03f);
        }
    }

    private void OnDrawGizmos()
    {
        if (Toggle_Sight_Debug)
        {
            if (sightPosition == null) { sightPosition = transform; }

            //Sight Gizmos.
            //Forward
            Gizmos.color = Color.white;
            Gizmos.DrawLine(sightPosition.position, transform.position + (transform.forward * myData_SO.sightDistance));

            //Left Sight.
            Quaternion L_rotation = Quaternion.AngleAxis(-myData_SO.sightAngle / 2, Vector3.up);
            Vector3 DebugSightCone_L = L_rotation * transform.forward;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(sightPosition.position, transform.position + (DebugSightCone_L * myData_SO.sightDistance));

            //Right Sight.
            Quaternion R_rotation = Quaternion.AngleAxis(myData_SO.sightAngle / 2, Vector3.up);
            Vector3 DebugSightCone_R = R_rotation * transform.forward;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(sightPosition.position, transform.position + (DebugSightCone_R * myData_SO.sightDistance));
            
        }
    }
}
