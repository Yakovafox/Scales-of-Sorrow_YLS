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
    private NavMeshAgent agent;
    private Vector3 investigationArea;

    private bool isAITarget;

    private bool intialiseMovement = false;

    private Vector3 targetsLastSeenLocation;

    private bool shouldWait;

    private float stateTimer = 0.0f;
    private float waitTime = 0.0f;

    private Transform groundChecker;
    private Transform sightPosition;

    private Ray ray;
    private RaycastHit rayResult;

    private bool stunned;

    private bool movingRight = false;
    #endregion



    public virtual void Awake()
    {
        sightPosition = transform.Find("CharacterBillboard/SightPoint");
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (myData_SO.Target == null || myData_SO.Target.GetType() != typeof(GameObject))
        {
            myData_SO.Target = GameObject.FindGameObjectWithTag("Player");
        }
        isAITarget = myData_SO.Target.TryGetComponent(out NavMeshAgent PlayerNav);
    }


    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case EnemyStates.Idle:
                intialiseMovement = false;
                ChangeState(EnemyStates.Moving);
                break;

            case EnemyStates.Moving:
                if (!intialiseMovement)
                {
                    intialiseMovement = true;
                    RandomisedMovePoint();
                }
                else if (ReachedDestination())
                {
                    RandomisedMovePoint();
                }
                else if (myData_SO.canSee && SeenTarget())
                {
                    Debug.Log("I found the player!");
                    ChangeState(EnemyStates.Chase);
                }
                break;

            case EnemyStates.Chase:
                MoveToChase();
                Debug.Log("CHASING!!!!");

                if (ReachedDestination()) // Need some more code here to define what to do with attack as its missing a bit to define if it should attack.
                {
                    ChangeState(EnemyStates.Attack);
                    //Change state to Attack
                }
                else if (myData_SO.canSee && SeenTarget())
                {
                    ChangeState(EnemyStates.Chase);
                }
                break;

            case EnemyStates.Attack:

                if (shouldSpecialAttack())
                {
                    SpecialAttack();
                    //Special Attack
                }
                else if (InMeleeRange())
                {
                    BasicAttack();
                    //Melee Attack
                }
                else if (InRangedRange())
                {
                    RangedAttack();
                    // ShootProjectile
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
        Debug.Log("Randomising a search point");
        //Randomise x and z point in the world within a sphere starting from current location. Y position must be set to 0.
        float randXPos = Random.Range(1, myData_SO.moveRadius);
        float randZPos = Random.Range(1, myData_SO.moveRadius);
        Vector3 randomSearchPos = new Vector3(randXPos, 0, randZPos);

        //Check if the randomised point is viable. This could be done using boundaries.
        Collider[] overlapingCols = Physics.OverlapSphere(randomSearchPos, 1f);
        debugSphere.transform.position = randomSearchPos;

        for(int i = 0; i < overlapingCols.Length; i++)
        {
            if(overlapingCols[i].gameObject.layer != myData_SO.GroundLayer.value)
            {
                Debug.Log("Invalid position");
                Debug.Log(overlapingCols[i].gameObject.name);
                Debug.Log(overlapingCols[i].gameObject.layer);
                Debug.Log(myData_SO.GroundLayer.value);
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
    bool InMeleeRange()
    {
        //Distance needs to be higher than 0.
        return agent.remainingDistance < myData_SO.meleeAttackDistance && !agent.pathPending;
    }
    bool InRangedRange()
    {
        //Distance needs to be higher than 0.
        return agent.remainingDistance < myData_SO.rangedAttackDistance && !agent.pathPending;
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
            Vector3 targetDirection = (myData_SO.Target.transform.position - transform.position).normalized;
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
                    }
                }
            }
        }
        return result;
    }
    #endregion

    #region AttackFunctions
    bool shouldSpecialAttack()
    {
        float RandomNum = Random.Range(0, 100);
        return RandomNum >= myData_SO.specialAttackChance;
    }

    protected virtual void BasicAttack()
    {
        //RaycastHit hit;
        //Physics.SphereCast(transform.position, myData_SO.meleeAttackDistance, out hit);
        Debug.Log("Default Attack");
    }

    protected virtual void RangedAttack()
    {
        Debug.Log("Ranged Attack");
        Instantiate(myData_SO.rangedProjectile, sightPosition.position, Quaternion.identity);
    }

    protected virtual void SpecialAttack()
    {
        Debug.Log("Special Attack");
        ChangeState(EnemyStates.Moving);
        return;
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
