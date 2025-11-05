using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FOV))]
public class SkeletonTree : BaseTree
{
    [Header("Stats")] 
    [SerializeField] private float minIdleTime = 5f;
    [SerializeField] private float maxIdleTime = 10f;
    [SerializeField] private float maxSpeed = 3f; 
    [SerializeField] private float persuitSpeed = 10f; 
    [SerializeField] float awarenessDistance = 15f;
    [SerializeField] float attackDistance = 2f;
    
    [Header("Steering")] 
    [SerializeField] Transform[] waypoints;
    [SerializeField] int currentWP = 0;
    [SerializeField] float arriveRange = 1f;
    [SerializeField] int autoWaypointCount = 5;
    [SerializeField] float autoWaypointRadius = 10f;

    [Header("Avoidance")] 
    [SerializeField] float avoidanceRange;
    [SerializeField] float avoidanceAngle;
    [SerializeField] float personalArea;
    [SerializeField] LayerMask obstacleMask;

    [Header("Behaviour")] 
    [SerializeField] private bool isAlive = true;
    [SerializeField] private GameObject attackVisual;
    [SerializeField] private float attackDuration;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackForce = 3f;
    [SerializeField] private float attackCooldown = 2f;
    
    private Rigidbody rb;
    private GameObject player;
    private FOV fieldOfView;
    private bool isAttacking;
        
    private Persuit persuit;
    private Arrive arrive;
    private ObstacleAvoidance avoidance;
    
    private Vector3 velocity;
    private float idleTimerHandler;
    private bool isWaiting = false;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not Found.");
            return;
        }
        rb = GetComponent<Rigidbody>();
        fieldOfView = GetComponent<FOV>();
        fieldOfView.Target = player;
        
        if (waypoints == null || waypoints.Length == 0)
        {
            CreateWaypoints();
            UpdateWaypoints();
        }
        
        persuit = new(player.transform, transform, persuitSpeed);
        arrive = new (waypoints[currentWP], transform, maxSpeed, arriveRange);
        avoidance = new (transform, avoidanceRange, avoidanceAngle, personalArea, obstacleMask);

        base.Start();
    }
    protected override void Update()
    {
        Debug.Log(
            $"{name} tick, currentWaypoint= {waypoints[currentWP]} isAlive={isAlive} idleTimer={idleTimerHandler} distToPlayer={(player ? Vector3.Distance(transform.position, player.transform.position) : -1)}");
        base.Update();
    }

    protected override void CreateTree()
    {
        ActionNode patrol = new(Patrol);
        ActionNode idle = new(Idle);
        ActionNode attack = new(Attack);
        ActionNode death = new(Death);
        ActionNode persecute = new(Persecute);
        
        QuestionNode arrivedAtPoint = new(
            () => Vector3.Distance(transform.position, waypoints[currentWP].position) < 0.3f,
            idle, patrol
        );
        
        QuestionNode isAtAttackDistance = new(
            () => Vector3.Distance(transform.position, player.transform.position) < attackDistance,
            attack, persecute
        );
        
        QuestionNode isPlayerCloseOrInSight = new(
            () => Vector3.Distance(transform.position, player.transform.position) < awarenessDistance || fieldOfView.CheckDetection(),
            isAtAttackDistance, arrivedAtPoint
        );
        
        _rootNode = new QuestionNode(
            () => isAlive,
            isPlayerCloseOrInSight, death
        );
    }

    private void Patrol()
    {
        Debug.Log($"{this} Patroling");
        velocity = arrive.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        Movement();
    }

    private void Death()
    {
        Destroy(gameObject);
    }
    private void Idle()
    {
        if (idleTimerHandler <= 0)
        {
            float chance = RandomGenerator.Range(0f, 1f);

            if (chance < 0.5f)
            {
                Debug.Log($"{this} staying idle");
                float randomTime = RandomGenerator.Range(minIdleTime,maxIdleTime);
                idleTimerHandler = randomTime;
            }
            else
            {
                Debug.Log($"{this} generating new waypoints");
                UpdateWaypoints();
                arrive.SetTarget = waypoints[currentWP];
                idleTimerHandler = 0;
            }
        }
        else
        {
            idleTimerHandler -= Time.deltaTime;
        }
    }
    
    private void Attack()
    {
        if (!canAttack || isAttacking) return;
        Debug.Log($"{this} attacks the player!");
        StartCoroutine(DoAttack());
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;
        
        if (attackVisual != null)
        {
             Instantiate(attackVisual, transform.position + transform.forward, transform.rotation);
        }
        Vector3 dir = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = dir * attackForce;

        yield return new WaitForSeconds(attackDuration);
        
        rb.linearVelocity = Vector3.zero;
        isAttacking = false;
    }
    
    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void Persecute()
    {
        Debug.Log($"{this} Persecute");
        velocity = persuit.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        Movement();
    }

    private void Movement()
    {
        velocity = avoidance.GetDir2(velocity);
        rb.linearVelocity = velocity;
    }
    
    private void CreateWaypoints()
    {
        waypoints = new Transform[autoWaypointCount];
        for (int i = 0; i < autoWaypointCount; i++)
        {
            GameObject wp = new GameObject($"{this} Waypoint_{i}");
            waypoints[i] = wp.transform;
        }
    }
    
    private void UpdateWaypoints()
    {
        currentWP = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * autoWaypointRadius;
            randomPos.y = transform.position.y;
            waypoints[i].position = randomPos;
        }
    }
}