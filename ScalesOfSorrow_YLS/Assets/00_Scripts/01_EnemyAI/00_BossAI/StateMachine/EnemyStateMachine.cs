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
    public GameObject PlayerRef;
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
    public bool canActivateDebug = false;

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
        
        if (myData_SO.Target == null || myData_SO.Target.GetType() != typeof(GameObject))
        {
            //PlayerRef = GameObject.Find("Player");
            myData_SO.Target = GameObject.FindGameObjectWithTag("Player");
            PlayerRef = myData_SO.Target;
        }
        isAITarget = myData_SO.Target.TryGetComponent(out NavMeshAgent PlayerNav);
    }


    void Update()
    {
        stateTimer += Time.deltaTime;
        attackTimer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyStates.Idle:
                if (TimeOut())
                {
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

            case EnemyStates.Chase:
                MoveToChase();

                if (ReachedDestination()) // Need some more code here to define what to do with attack as its missing a bit to define if it should attack.
                {
                    AttackOnce = true;

                    shouldSpecial = true;
                    ChangeState(EnemyStates.Attack);
                    //Change state to Attack
                }
                else if (InAttackRange(myData_SO.rangedAttackDistance))
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

                if (!AttackOnce)
                {
                    ChangeState(EnemyStates.Chase);
                }
                else if (AttackOnce && AttackCooldown())
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

    private Vector3 GetPlayerDirection()
    {
        return (myData_SO.Target.transform.position - sightPosition.position).normalized;
    }

    bool TimeOut()
    {
        waitTime = myData_SO.timeToWait;
        return stateTimer > waitTime;
    }

    bool SeenTarget()
    {
        bool result = false;

        float playerDistance = Vector3.Distance(transform.position, myData_SO.Target.transform.position);

        if (playerDistance < myData_SO.sightDistance)
        {
            Vector3 targetDirection = (myData_SO.Target.transform.position - sightPosition.position).normalized;
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
                            targetsLastSeenLocation = myData_SO.Target.transform.position;
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
        projectileInstance.GetComponent<Scr_Projectile>().Accessor_dir = GetPlayerDirection();
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

            if (Application.isPlaying && canActivateDebug)
            {
                Gizmos.color = Color.green;
                Vector3 tDirection = (myData_SO.Target.transform.position - sightPosition.position).normalized;
                Gizmos.DrawLine(sightPosition.position, tDirection);
            }

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
