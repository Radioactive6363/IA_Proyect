using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class RatTree : Boid 
{
    [Header("Rat Stats")] 
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 4f;
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] float awarnessDistance = 20f;
    [SerializeField] float scaredDuration = 2f;
    
    [Header("Flocking")] 
    [SerializeField] float flockingForce = 2f;
    
    [Header("Wander Settings")] 
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
    
    private Rigidbody rb; 
    private ActionNode wander; 
    private ActionNode idle;
    private ActionNode scared;
    private ActionNode runAway;
    private QuestionNode rootNode;

    private int currentWP = 0;
    private AudioSource audioSource;
    private Dictionary<AudioClip, float> _audioValues;
    private GameObject player;
    
    private Flee flee;
    private Arrive arrive;
    private ObstacleAvoidance avoidance;

    private float idleTimerHandler;
    private bool isScared;
    private float scaredTimer;
    private bool useFlocking = true;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        _audioValues = new Dictionary<AudioClip, float>();

        rb = GetComponent<Rigidbody>();
        
        GenerateAudio();
        
        if (waypoints == null || waypoints.Length == 0)
        {
            CreateWaypoints();
            UpdateWaypoints();
        }

        flee = new Flee(player.transform, transform, fleeSpeed);
        arrive = new Arrive(waypoints[currentWP], transform, maxSpeed, arriveRange);
        avoidance = new ObstacleAvoidance(transform, avoidanceRange, avoidanceAngle, personalArea, obstacleMask);

        base.Start(); 
        CreateTree();
    }

    protected override void Update()
    {
        rootNode?.Execute();
    }
    
    private void CreateTree()
    {
        wander = new ActionNode(Wandering);
        idle = new ActionNode(Idle);
        scared = new ActionNode(Scared);
        runAway = new ActionNode(RunAway);
        
        QuestionNode arrivedAtPoint = new(
            () => Vector3.Distance(transform.position, waypoints[currentWP].position) < 2f, 
            idle, wander
        );
        
        QuestionNode isPlayerClose = new(
            () => Vector3.Distance(transform.position, player.transform.position) < awarnessDistance,
            scared, arrivedAtPoint
        );
        
        rootNode = new QuestionNode(
            () => isScared,
            runAway, isPlayerClose
        );
    }

    private void Wandering()
    {
        velocity = rb.linearVelocity;
        useFlocking = true;
        
        Vector3 flockingPush = CalculateFlockingForce();
        Vector3 arrivePush = arrive.GetSteerDir(transform.position); 
        
        Vector3 flockingDir = flockingPush.magnitude > 0.01f ? flockingPush.normalized : Vector3.zero;
        Vector3 arriveDir = arrivePush.normalized;
        
        Vector3 finalDirection = (arriveDir * 1.0f) + (flockingDir * 2.0f);


        Vector3 targetVelocity = finalDirection.normalized * maxSpeed;
        
        velocity = Vector3.MoveTowards(velocity, targetVelocity, maxForce * Time.deltaTime);
        
        Movement();
    }
    
    private void Idle()
    {
        velocity = Vector3.zero; 
        if (idleTimerHandler <= 0)
        {
            if (Random.value < 0.5f)
            {
                idleTimerHandler = Random.Range(minIdleTime, maxIdleTime);
            }
            else
            {
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
            isScared = true;
            scaredTimer = scaredDuration;
            idleTimerHandler = 0;
            PlayRandomSqueak();
            useFlocking = false; 
        }
    }

    private void RunAway()
    {
        base.velocity = rb.linearVelocity;
        
        if (scaredTimer <= 0)
        {
            isScared = false;
            UpdateWaypoints(); 
            return;
        }
        Vector3 fleeForce = flee.GetSteerDir(velocity);
        Vector3 runDirection = fleeForce.normalized;
        Vector3 targetVelocity = runDirection * (maxSpeed * 1.5f); 
        velocity = Vector3.MoveTowards(velocity, targetVelocity, (maxForce * 2f) * Time.deltaTime);
        Movement();

        scaredTimer -= Time.deltaTime;
    }

    private void Movement()
    {
        Vector3 avoidanceDir = avoidance.GetDir2(velocity); 
        if(avoidanceDir != Vector3.zero)
        {
            velocity = Vector3.Lerp(velocity, avoidanceDir.normalized * maxSpeed, Time.deltaTime * 5f);
        }
        Vector3 finalVel = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        rb.linearVelocity = finalVel;
        
        if (finalVel.sqrMagnitude > 0.1f)
        {
            Vector3 lookDir = new Vector3(finalVel.x, 0, finalVel.z);
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    private Vector3 CalculateFlockingForce()
    {
        if (!useFlocking) return Vector3.zero;
        return GetFlockingForce(); 
    }

    private void GenerateAudio()
    {
        if (squeakSounds.Length > 0)
        {
            _audioValues[squeakSounds[0]] = 0.5f;
            if(squeakSounds.Length > 1) _audioValues[squeakSounds[1]] = 0.3f;
            if(squeakSounds.Length > 2) _audioValues[squeakSounds[2]] = 0.2f;
        }
    }
    
    private void PlayRandomSqueak()
    {
        if (squeakSounds.Length > 0)
            audioSource.PlayOneShot(squeakSounds[Random.Range(0, squeakSounds.Length)]);
    }
    
    private void CreateWaypoints()
    {
        waypoints = new Transform[autoWaypointCount];
        for (int i = 0; i < autoWaypointCount; i++)
        {
            GameObject wp = new GameObject($"RatWP_{i}");
            waypoints[i] = wp.transform;
        }
    }
    
    private void UpdateWaypoints()
    {
        currentWP = 0;
        Vector3 groupCenter = transform.position + Random.insideUnitSphere * autoWaypointRadius; 
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 randomPos = groupCenter + Random.insideUnitSphere * 2f; 
            randomPos.y = transform.position.y;
            waypoints[i].position = randomPos;
        }
    }
}