using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum EnemyStates
{
    Idle,
    Moving,
    Fly,
    Chase,
    Attack
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
    public List<GameObject> PlayerRef;
    private NavMeshAgent agent;
    private Vector3 investigationArea;

    private bool isAITarget;

    private bool intialiseMovement = false;

    private Vector3 targetsLastSeenLocation;

    private bool shouldWait;

    private float stateTimer = 0.0f;
    private float waitTime = 0.0f;
    private float attackTimer = 0.0f;
    private float attackWaitTime = 0.0f;  

    private Transform groundChecker;
    private Transform sightPosition;

    private Ray ray;
    private RaycastHit rayResult;

    private bool stunned;

    private bool AttackOnce;
    private bool shouldSpecial;

    private bool movingRight = false;
    #endregion



    public virtual void Awake()
    {
        sightPosition = transform.Find("SightPoint");
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        myData_SO.Target = null;
        
        //if (myData_SO.Target[0].GetType() != typeof(GameObject) || myData_SO.Target[0] == null)
        //{
             GameObject[] tempPlayerArray = GameObject.FindGameObjectsWithTag("Player");
             for (int i = 0; i < tempPlayerArray.Length; i++)
             {
                 print(tempPlayerArray[i]);
                 PlayerRef.Add(tempPlayerArray[i]);
                 //Should check ID of the player and should organise this list based on player with lowest ID!!!
                 
                 //myData_SO.Target.Add(tempPlayerArray[i]); //Is broken and stops code?
                 
             }
             //PlayerRef = myData_SO.Target;
        //}
    }


    void Update() //Functions should be called from here where Update holds the logic for what should happen in each state, and the Functions hold the functionality of what happens.
    {
        stateTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyStates.Idle:
                if (TimeOut())
                {
                    attackWaitTime = 0;
                    intialiseMovement = false;
                    agent.isStopped = false;
                    ChangeState(EnemyStates.Moving);
                }
                break;

            case EnemyStates.Moving:
                if (!intialiseMovement) // Initialise Movement is only called the first time that the enemy enters the moving state.
                {
                    intialiseMovement = true;
                    Debug.Log("Initialised Movement");
                    
                    RandomisedMovePoint();
                }
                else if (myData_SO.canSee && SeenTarget()) // AI cannot see player during this state for some reason, need to re look at logic.
                                                          // I believe this is down to the raycast aiming at the floor instead of straight forward? Seems to be if player hits into the AI then chase is called.
                {
                    ChangeState(EnemyStates.Chase);
                }
                else if (ReachedDestination() && !SeenTarget())
                {
                    RandomisedMovePoint();
                }
                break;
            
            case EnemyStates.Fly:
                //Functions for flying should go here
                break;

            case EnemyStates.Chase:
                MoveToChase();

                if (ReachedDestination()) // Need some more code here to define what to do with attack as its missing a bit to define if it should attack.
                {
                    AttackOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.Attack);
                    //Change state to Attack
                }
                else if (AttackCooldown() && InAttackRange(myData_SO.rangedAttackDistance))
                {
                    AttackOnce = true;

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
                print("Attack Once: " + AttackOnce);
                if (!AttackOnce)
                {
                    ChangeState(EnemyStates.Chase);
                }
                else if (AttackOnce)
                {
                    /*if (shouldSpecial && shouldSpecialAttack())
                    {
                        AttackOnce = false;
                        SpecialAttack();
                        //Special Attack 
                    }
                    else*/ if (InAttackRange(myData_SO.meleeAttackDistance))
                    {
                        AttackOnce = false;
                        BasicAttack();
                        //Melee Attack
                    }
                    else if (InAttackRange(myData_SO.rangedAttackDistance))
                    {
                        AttackOnce = false;
                        RangedAttack();
                        // ShootProjectile
                    }

                    AttackOnce = false;
                    attackTimer = 0;
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
        stateTimer = 0;
    }

    void player2Joined()
    {
        
    }
    
    void player2Left()
    {
        
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


    #region Movement Functions
    void MoveToChase()
    {
        agent.destination = targetsLastSeenLocation;
    }

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

    bool ReachedDestination()
    {
        //Stopping distance needs to be higher than 0.
        return agent.remainingDistance < agent.stoppingDistance && !agent.pathPending;
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

    private Vector3 GetPlayerDirection(GameObject playerToUse)
    {
        return (playerToUse.transform.position - sightPosition.position).normalized;
    }

    bool TimeOut()
    {
        waitTime = myData_SO.timeToWait;
        return stateTimer > waitTime;
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
    #endregion

    #region AttackFunctions
    bool AttackCooldown()
    {
        print("Attack Cooldown: " + attackWaitTime);
        attackWaitTime = myData_SO.attackCooldown;
        return attackTimer > attackWaitTime;
    }
    bool shouldSpecialAttack()
    {
        if (!shouldSpecial) { return false; }
        shouldSpecial = false;
        
        float RandomNum = Random.Range(0, 100);
        Debug.Log(RandomNum);
        return RandomNum <= myData_SO.specialAttackChance;
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

    protected virtual void SpecialAttack()
    {
        Debug.Log("Special Attack");
        ChangeState(EnemyStates.Idle);

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
