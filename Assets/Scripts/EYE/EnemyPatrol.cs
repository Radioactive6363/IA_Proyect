using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : BaseTree
{
    [Header("Stats")]
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 4f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] private float awarnessDistance = 20f;
    [SerializeField] private float scaredDuration = 2f;
    [SerializeField] private float rotationspeed = 5f;

    [Header("Steering")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private int currentWP = 0;
    [SerializeField] private float arriveRange = 1f;
    [SerializeField] private int autoWaypointCount = 5;
    [SerializeField] private float autoWaypointRadius = 10f;

    [Header("Avoidance")]
    [SerializeField] private float avoidanceRange;
    [SerializeField] private float avoidanceAngle;
    [SerializeField] private float personalArea;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Extras")]
    [SerializeField] private AudioClip[] squeakSounds;

    private AudioSource audioSource;
    private Rigidbody rb;
    private GameObject player;

    private Flee flee;
    private Arrive arrive;
    private ObstacleAvoidance avoidance;

    private Vector3 velocity;
    private float idleTimerHandler;
    private bool isScared;
    private float scaredTimer;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            Debug.LogError("Player not Found.");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            CreateWaypoints();
            UpdateWaypoints();
        }

        flee = new Flee(player.transform, transform, fleeSpeed);
        arrive = new Arrive(waypoints[currentWP], transform, maxSpeed, arriveRange);
        avoidance = new ObstacleAvoidance(transform, avoidanceRange, avoidanceAngle, personalArea, obstacleMask);

        base.Start();
    }

    protected override void Update()
    {
        if (isScared)
        {
            scaredTimer -= Time.deltaTime;
            velocity = flee.GetSteerDir(velocity);
            Movement();

            if (scaredTimer <= 0)
            {
                isScared = false;
            }
        }

        base.Update();
    }

    protected override void CreateTree()
    {
        ActionNode scared = new(Scared);
        ActionNode idle = new(Idle);
        ActionNode patrol = new(Patrol);

        QuestionNode arrivedAtPoint = new(
            () => Vector3.Distance(transform.position, waypoints[currentWP].position) < arriveRange,
            idle, patrol);

        QuestionNode isPlayerClose = new(
            () => Vector3.Distance(transform.position, player.transform.position) < awarnessDistance,
            scared, arrivedAtPoint);

        _rootNode = isPlayerClose;
    }

    private void Scared()
    {
        if (!isScared)
        {
            Debug.Log($"{name} is Scared!");
            scaredTimer = scaredDuration;
            idleTimerHandler = 0;
            isScared = true;
        }
    }

    private void Idle()
    {
        if (idleTimerHandler <= 0)
        {
            float chance = RandomGenerator.Range(0f, 1f);

            if (chance < 0.5f)
            {
                Debug.Log($"{name} staying idle");
                float randomTime = RandomGenerator.Range(minIdleTime, maxIdleTime);
                idleTimerHandler = randomTime;
            }
            else
            {
                Debug.Log($"{name} moving to next waypoint");
                NextWaypoint();
                idleTimerHandler = 0;
            }
        }
        else
        {
            idleTimerHandler -= Time.deltaTime;
        }
    }

    private void Movement()
    {
        velocity = avoidance.GetDir2(velocity);


        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
        Vector3 move = velocity.normalized * maxSpeed;

        Vector3 flatVel = new Vector3(velocity.x, 0, velocity.z);
        RotateTowards(flatVel);
    }

    private void Patrol()
    {
        velocity = arrive.GetSteerDir(velocity);
        Movement();


        if (Vector3.Distance(transform.position, waypoints[currentWP].position) < arriveRange)
        {
            NextWaypoint();
        }
    }

    private void NextWaypoint()
    {
        currentWP = (currentWP + 1) % waypoints.Length;
        arrive.SetTarget = waypoints[currentWP];
    }

    private void CreateWaypoints()
    {
        waypoints = new Transform[autoWaypointCount];
        for (int i = 0; i < autoWaypointCount; i++)
        {
            GameObject wp = new GameObject($"EnemyWaypoint_{i}");
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

        arrive.SetTarget = waypoints[currentWP];
    }
    private void RotateTowards(Vector3 dir)
    {
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationspeed * Time.deltaTime
            );
        }
    }
}
