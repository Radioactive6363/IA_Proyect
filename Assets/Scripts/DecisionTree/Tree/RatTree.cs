using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RatTree : BaseTree
{
    [Header("Stats")] 
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 4f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] private float maxPersuitSpeed = 5f;
    [SerializeField] float fleeDistance = 20f;
    [SerializeField] float scaredDuration = 2f;

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
    
    [Header("Extras")]
    [SerializeField] private AudioClip[] squeakSounds;
    
    private AudioSource audioSource;
    private Rigidbody rb;
    private GameObject player;
    
    private Flee flee;
    private Arrive arrive;
    private ObstacleAvoidance avoidance;

    private Vector3 velocity;
    private float idleTimerHandler = 0f;
    private bool isWaiting = false;
    private bool isScared = false;
    private float scaredTimer = 0f;

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
        Debug.Log(
            $"{name} tick - isScared={isScared} idleTimer={idleTimerHandler} distToPlayer={(player ? Vector3.Distance(transform.position, player.transform.position) : -1)}");
        base.Update();
    }

    protected override void CreateTree()
    {
        ActionNode patrol = new(Wandering);
        ActionNode idle = new(Idle);
        ActionNode runAway = new(RunAway);
        ActionNode scared = new(Scared);
        
        QuestionNode arrivedAtPoint = new(
            () => Vector3.Distance(transform.position, waypoints[currentWP].position) < 0.3f,
            idle, patrol
        );
        
        QuestionNode isPlayerClose = new(
            () => Vector3.Distance(transform.position, player.transform.position) < fleeDistance,
            runAway, arrivedAtPoint
        );
        
        _rootNode = new QuestionNode(
            () => isScared,
            scared, isPlayerClose
        );
    }

    private void Wandering()
    {
        Debug.Log("Rat Wandering");
        velocity = arrive.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        Movement();
    }

    private void Idle()
    {
        if (idleTimerHandler <= 0)
        {
            float chance = RandomGenerator.Range(0f, 1f);

            if (chance < 0.5f)
            {
                Debug.Log("Rat staying idle");
                float randomTime = RandomGenerator.Range(minIdleTime,maxIdleTime);
                idleTimerHandler = randomTime;
            }
            else
            {
                Debug.Log("Rat generating new waypoints");
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

    private void RunAway()
    {
        Debug.Log("Rat running Away, generating waypoints");
        if (!isScared)
        {
            Scare();
        }
        velocity = flee.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        Movement();
    }

    public void Scare(float duration = -1f)
    {
        Debug.Log("Rat Scared");
        isScared = true;
        scaredTimer = (duration > 0) ? duration : scaredDuration;
        idleTimerHandler = 0;
    }

    private void Scared()
    {
        if (scaredTimer <= 0)
        {
            UpdateWaypoints();
            isScared = false;
            return;
        }

        scaredTimer -= Time.deltaTime;
        velocity = flee.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        Movement();
    }

    private void Movement()
    {
        velocity = avoidance.GetDir2(velocity);
        transform.position += velocity * Time.deltaTime;
    }
    
    private void CreateWaypoints()
    {
        waypoints = new Transform[autoWaypointCount];
        for (int i = 0; i < autoWaypointCount; i++)
        {
            GameObject wp = new GameObject($"RatWaypoint_{i}");
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