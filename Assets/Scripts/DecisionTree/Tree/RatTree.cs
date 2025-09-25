using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RatTree : BaseTree
{
    [Header("Stats")] [SerializeField] private float idleTime = 4f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float fleeSpeed = 10f;
    [SerializeField] float fleeDistance = 20f;
    [SerializeField] float scaredDuration = 2f;
    
    [Header("Steering")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] int currentWP = 0;
    [SerializeField] float arriveRange = 1f;
    [SerializeField] int autoWaypointCount = 5;
    [SerializeField] float autoWaypointRadius = 10f;

    private Rigidbody rb;
    private GameObject player;
    private Flee flee;
    private Arrive arrive;

    private Vector3 velocity;
    private float idleTimerHandler = 0f;
    private bool isWaiting = false;
    private bool isScared = false;
    private float scaredTimer = 0f;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        if (player == null)
        {
            Debug.LogError("Player not Found.");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
            GenerateDynamicWaypoints();

        flee = new Flee(player.transform, transform, fleeSpeed);
        arrive = new Arrive(waypoints[currentWP], transform, maxSpeed, arriveRange);

        base.Start();
    }
    protected override void Update()
    {
        Debug.Log($"{name} tick - isScared={isScared} idleTimer={idleTimerHandler} distToPlayer={(player?Vector3.Distance(transform.position, player.transform.position):-1)}");
        base.Update();
    }

    protected override void CreateTree()
    {
        ActionNode patrol = new(Patrol);
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

    private void Patrol()
    {
        Debug.Log("Rat Patrolling");
        velocity = arrive.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    private void Idle()
    {
        if (idleTimerHandler <= 0)
        {
            float chance = Random.value;

            if (chance < 0.5f)
            {
                Debug.Log("Rat staying idle");
                idleTimerHandler = idleTime; // se queda quieta cierto tiempo
            }
            else
            {
                Debug.Log("Rat generating new waypoints");
                GenerateDynamicWaypoints();
                currentWP = 0;
                arrive.SetTarget = waypoints[currentWP];

                idleTimerHandler = 0; // 👈 importante: liberar timer para no quedarse bloqueada
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
        rb.linearVelocity = velocity;
    }
    
    public void Scare(float duration = -1f)
    {
        Debug.Log("Rat Scared");
        isScared = true;
        scaredTimer = (duration > 0) ? duration : scaredDuration;
        idleTime = 0;
    }
    
    private void Scared()
    {
        if (scaredTimer <= 0)
        {
            GenerateDynamicWaypoints();
            currentWP = 0;
            isScared = false;
            return;
        }
        scaredTimer -= Time.deltaTime;
        velocity = flee.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y; 
        rb.linearVelocity = velocity;
    }
    
    private void GenerateDynamicWaypoints()
    {
        waypoints = new Transform[autoWaypointCount];
        currentWP = 0;

        for (int i = 0; i < autoWaypointCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * autoWaypointRadius;
            randomPos.y = transform.position.y;

            GameObject wp = new GameObject($"RatWaypoint_{i}");
            wp.transform.position = randomPos;
            waypoints[i] = wp.transform;
        }

        arrive = new Arrive(waypoints[currentWP], transform, maxSpeed, arriveRange);
    }
}