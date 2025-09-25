using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class RatTree : BaseTree
{
    [Header("Stats")] 
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 4f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] float awarnessDistance = 20f;
    [SerializeField] float scaredDuration = 2f;

    [Header("Steering")] 
    [SerializeField] Transform[] waypoints;
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
    
    private int currentWP = 0;
    private AudioSource audioSource;
    private Dictionary<AudioClip, float> _audioValues;
    private Rigidbody rb;
    private GameObject player;
    
    private Flee flee;
    private Arrive arrive;
    private ObstacleAvoidance avoidance;

    private Vector3 velocity;
    private float idleTimerHandler;
    private bool isScared;
    private float scaredTimer;
    private bool isWaiting = false;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        _audioValues = new Dictionary<AudioClip, float>();
        rb = GetComponent<Rigidbody>();
        GenerateAudio();
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
        else
        {
            currentWP = 0;
            arrive = new Arrive(waypoints[currentWP], transform, maxSpeed, arriveRange);
        }

        flee = new Flee(player.transform, transform, fleeSpeed);
        arrive = new Arrive(waypoints[currentWP], transform, maxSpeed, arriveRange);
        avoidance = new ObstacleAvoidance(transform, avoidanceRange, avoidanceAngle, personalArea, obstacleMask);

        base.Start();
    }
    protected override void Update()
    {
        Debug.Log(
            $"{name} tick, currentWaypoint= {waypoints[currentWP]} isScared={isScared} idleTimer={idleTimerHandler} distToPlayer={(player ? Vector3.Distance(transform.position, player.transform.position) : -1)}");
        base.Update();
    }

    protected override void CreateTree()
    {
        ActionNode wander = new(Wandering);
        ActionNode idle = new(Idle);
        ActionNode scared = new(Scared);
        ActionNode runAway = new(RunAway);
        
        QuestionNode arrivedAtPoint = new(
            () => Vector3.Distance(transform.position, waypoints[currentWP].position) < 0.3f,
            idle, wander
        );
        
        QuestionNode isPlayerClose = new(
            () => Vector3.Distance(transform.position, player.transform.position) < awarnessDistance,
            scared, arrivedAtPoint
        );
        
        _rootNode = new QuestionNode(
            () => isScared,
            runAway, isPlayerClose
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

    private void Scared()
    {
        if (!isScared)
        {
            Debug.Log($"{this} Scared");
            scaredTimer = scaredDuration;
            idleTimerHandler = 0;
            PlayRandomSqueak();
            isScared = true;
        }
    }

    private void RunAway()
    {
        if (scaredTimer <= 0)
        {
            UpdateWaypoints();
            isScared = false;
            return;
        }
        velocity = flee.GetSteerDir(velocity);
        velocity.y = rb.linearVelocity.y;
        Movement();
        scaredTimer -= Time.deltaTime;
    }

    private void Movement()
    {
        velocity = avoidance.GetDir2(velocity);
        rb.linearVelocity = velocity;
    }

    private void GenerateAudio()
    {
        if (squeakSounds.Length > 0)
        {
            _audioValues[squeakSounds[0]] = 0.5f;
            _audioValues[squeakSounds[1]] = 0.3f;
            _audioValues[squeakSounds[2]] = 0.2f;
        }
    }
    
    private void PlayRandomSqueak()
    {
        AudioClip chosenClip = RandomGenerator.Roulette(_audioValues);

        if (chosenClip != null)
        {
            audioSource.PlayOneShot(chosenClip);
            Debug.Log($"{this} squeaked: {chosenClip.name}");
        }
    }
    
    private void CreateWaypoints()
    {
        waypoints = new Transform[autoWaypointCount];
        for (int i = 0; i < autoWaypointCount; i++)
        {
            GameObject wp = new GameObject($"{this}RatWaypoint_{i}");
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