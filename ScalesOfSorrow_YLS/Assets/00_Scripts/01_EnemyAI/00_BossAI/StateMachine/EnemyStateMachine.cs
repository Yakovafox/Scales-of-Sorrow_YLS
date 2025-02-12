using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum EnemyStates
{
    Idle,
    Moving,
    Fly,
    Chase,
    Attack,
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
    private Vector3 investigationArea;

    private bool isAITarget;

    private bool intialiseMovement = false;

    private Vector3 targetsLastSeenLocation;

    private bool shouldWait;

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

    private float stun_Timer = 0.0f;
    private float stun_WaitTime = 0.0f;

    private Transform groundChecker;
    private Transform sightPosition;

    private float defaultYPos;

    private GameObject GO_shadowCaster;
    private SpriteRenderer spriteRenderer;

    private Ray ray;
    private RaycastHit rayResult;

    private bool stunned;

    private bool flyingToRandomPoint;

    private bool doOnce;
    private bool shouldSpecial;
    private bool specialActive;

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
        GO_shadowCaster = gameObject.transform.Find("shadowCaster").gameObject;
        spriteRenderer = gameObject.transform.Find("CharacterBillboard").GetComponent<SpriteRenderer>();
        myData_SO.Target = null;
        
        //if (myData_SO.Target[0].GetType() != typeof(GameObject) || myData_SO.Target[0] == null)
        //{
             GameObject[] tempPlayerArray = GameObject.FindGameObjectsWithTag("Player");
             for (int i = 0; i < tempPlayerArray.Length; i++)
             {
                 PlayerRef.Add(tempPlayerArray[i]);
                 //Should check ID of the player and should organise this list based on player with lowest ID!!!
                 
                 //myData_SO.Target.Add(tempPlayerArray[i]); //Is broken and stops code?
                 
             }
        //PlayerRef = myData_SO.Target;
        //}

        //Functionality for setting the Dragon health on startup.
        currentHealth = myData_SO.MaxHealth;
        stagesLeft = myData_SO.Stages;
    }


    void Update() //Functions should be called from here where Update holds the logic for what should happen in each state, and the Functions hold the functionality of what happens.
    {
        state_Timer += Time.deltaTime;
        attack_Timer += Time.deltaTime;
        flyCooldown_Timer += Time.deltaTime;
        abilityCooldown_Timer += Time.deltaTime;
        stun_Timer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyStates.Idle:
                if (stunned)
                {
                    if (TimeOut(myData_SO.stunTime))
                    {
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
                else if (ReachedDestination() && flyCooldown() && randomShouldFly())
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
                    flightChase();
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

                if (AttackCooldown() && ReachedDestination()) // Need some more code here to define what to do with attack as its missing a bit to define if it should attack.
                {
                    doOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.Attack);
                    //Change state to Attack
                }
                else if (AttackCooldown() && InAttackRange(myData_SO.rangedAttackDistance))
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
                    if (shouldSpecial && shouldSpecialAttack())
                    {
                        doOnce = false;
                        //Special Attack 
                    }
                    else if (InAttackRange(myData_SO.meleeAttackDistance))
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
                if (doOnce)
                {
                    initialiseSpecialAbility();
                }
                else if (!abilityTimeOut())
                {
                    // Consistently chase down player into range and attack them.
                }
                // Any Special ability that can be done should happen here. (Example the electro dragons dash.)
                //Enter attack state and loop back to hear after attack?
                else if (abilityTimeOut())
                {
                    //Reset timer when leaving this state.
                    ChangeState(EnemyStates.Idle);
                    specialActive = false;
                    abilityCooldown_Timer = 0;
                }
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
    }

    #region Player2Functions
    void player2Joined()
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
    
    void player2Left()
    {
        if (PlayerRef.Count <= 1) { return; }
        PlayerRef.RemoveAt(1);
        //Code to adjust Health here or attack damage etc.
    }

    bool doesP2Exist()
    {
        return PlayerRef.Count > 0;
    }

    GameObject findClosestPlayer()
    {
        if (PlayerRef.Count == 1) return PlayerRef[0];
        float p1Dist = Vector3.Distance(transform.position, PlayerRef[0].transform.position);
        float p2Dist = Vector3.Distance(transform.position, PlayerRef[1].transform.position);
        if (p2Dist < p1Dist) return PlayerRef[1];
        return PlayerRef[0];
    }
    #endregion

    #region Health Functions

    public void RecieveDamage(float incomingDamage)
    {
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

    private void healthReachedZero()
    {
        //Double check that health is below zero.
        if(stagesLeft <= 0)
        {
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

    void flightChase()
    {
        targetsLastSeenLocation = findClosestPlayer().transform.position;
        agent.destination = targetsLastSeenLocation;
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
                            Debug.Log("I couldn't find player");
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
        if (GO_shadowCaster.transform.localScale.x > myData_SO.shadow_MaxSize)
        {
            GO_shadowCaster.transform.localScale += GO_shadowCaster.transform.localScale * (0.1f * Time.deltaTime);
        }
        if (doOnce)
        {
            doOnce = false;
            flyingToRandomPoint = false;
            ChangeState(EnemyStates.Fly);
            //Change State to flying.
            //Play dragon lifting off animation here.

            //Set Dragon sprite rendered to disabled.
            spriteRenderer.enabled = false;
            //The above elements must time correctly otherwise the dragon will appear back at the bottom of the screen.
            //Potentially move the sprite higher out of view as well?
            defaultYPos = transform.position.y;
            spriteRenderer.transform.position = new Vector3(spriteRenderer.transform.position.x, 15f,
                spriteRenderer.transform.position.z);
        }
    }

    void Land()
    {
        if (GO_shadowCaster.transform.localScale.x < myData_SO.shadow_DefaultSize)
        {
            GO_shadowCaster.transform.localScale -= GO_shadowCaster.transform.localScale * (0.1f * Time.deltaTime);
        }
        if (doOnce)
        {
            doOnce = false;
            //Potentially move the sprite back into view if deciding to keep sprite out of view
            spriteRenderer.transform.position = new Vector3(spriteRenderer.transform.position.x, defaultYPos, spriteRenderer.transform.position.z );
            //PLay dragon landing animation here.
            
            //Set Dragon sprite rendered to disabled.
            spriteRenderer.enabled = true;
            //The above elements must time correctly otherwise the dragon will appear back at the bottom of the screen.
            flyCooldown_Timer = 0;
            stunned = true;
            OnDragonLanded?.Invoke();
            ChangeState(EnemyStates.Idle);
        }
        //Grow Shadow Over time as animation draws to end.

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
        ability_WaitTime = myData_SO.flightTime;
        return ability_Timer > ability_WaitTime;
    }

    bool abilityCooldown()
    {
        abilityCooldown_WaitTime = myData_SO.flightCooldownTime;
        return abilityCooldown_Timer > abilityCooldown_WaitTime;
    }

    bool stunTimeOut()
    {
        stun_WaitTime = myData_SO.timeToWait;
        return stun_Timer > stun_WaitTime;
    }

    bool AttackCooldown()
    {
        attack_WaitTime = myData_SO.attackCooldown;
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
        //Cause Player Damage here or effect that can cause damage.
        ChangeState(EnemyStates.Idle);

        Debug.Log("Default Attack");
    }

    protected virtual void RangedAttack()
    { 
        Debug.Log("Ranged Attack");
        GameObject projectileInstance = Instantiate(myData_SO.rangedProjectile, sightPosition.position, Quaternion.identity); // This works but needs a prefab in it disabled for development.
        projectileInstance.GetComponent<Scr_Projectile>().Accessor_dir = GetPlayerDirection(findClosestPlayer());
        agent.isStopped = true;
        ChangeState(EnemyStates.Idle);

    }

    protected virtual void initialiseSpecialAbility()
    {
        Debug.Log("Initialising Special Ability");
        //Setup any functionality for the ability here, spawn in shield etc.
        //In base machine setup all abilities at once 

    }
    #endregion


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
