using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(FOV))]
public class SkeletonTree : BaseTree
{
    [Header("Movement Stats")] 
    [SerializeField] private float speed = 4f;
    [SerializeField] private float nodeReachedDistance = 1.1f;
    [SerializeField] private float waypointArrivalDistance = 1.5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float patrolTurnSpeed = 10f;
    [SerializeField] private float chaseTurnSpeed = 30f;

    [Header("AI Logic")]
    [SerializeField] private float awarenessDistance = 15f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 5f;
    
    [Header("Steering Settings")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float avoidanceRadius = 2f;
    private Persuit steeringPursuit;
    private ObstacleAvoidance steeringAvoidance;

    [Header("Combat")] 
    [SerializeField] private bool isAlive = true;
    [SerializeField] private GameObject attackVisual;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackCooldown = 2f;
    
    [Header("Patrol")]
    [SerializeField] Transform[] patrolPoints; 
    
    private Rigidbody rb;
    private GameObject player;
    private FOV fieldOfView;
    private bool isAttacking;
    private float idleTimerHandler;
    
    private List<PFNode> currentPath = new List<PFNode>();
    private Transform currentWaypointTarget;
    private int patrolIndex = 0;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        fieldOfView = GetComponent<FOV>();
        fieldOfView.Target = player;
        
        steeringPursuit = new Persuit(player.transform, transform, speed);
        steeringAvoidance = new ObstacleAvoidance(transform, avoidanceRadius, 90, 1f, obstacleMask);

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            GameObject wp = new GameObject("AutoWP_Fallback");
            wp.transform.position = transform.position;
            patrolPoints = new Transform[] { wp.transform };
        }
        currentWaypointTarget = patrolPoints[0];

        base.Start();
    }

    protected override void CreateTree()
    {
        ActionNode patrol = new ActionNode(Patrol);
        ActionNode idle = new ActionNode(Idle);
        ActionNode attack = new ActionNode(Attack);
        ActionNode death = new ActionNode(Death);
        ActionNode persecute = new ActionNode(Persecute);
        
        QuestionNode arrivedAtWaypoint = new QuestionNode(
            () => GetFlatDistance(transform.position, currentWaypointTarget.position) < waypointArrivalDistance,
            idle, patrol 
        );
        
        QuestionNode isAtAttackRange = new QuestionNode(
            () => GetFlatDistance(transform.position, player.transform.position) < attackDistance,
            attack, persecute
        );
        
        QuestionNode detectPlayer = new QuestionNode(
            () => GetFlatDistance(transform.position, player.transform.position) < awarenessDistance || fieldOfView.CheckDetection(),
            isAtAttackRange, arrivedAtWaypoint 
        );
        
        _rootNode = new QuestionNode(
            () => isAlive,
            detectPlayer, death
        );
    }

    // --- ACCIONES ---

    private void Patrol()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            RequestPathTo(currentWaypointTarget.position);
        }
        
        if (currentPath == null || currentPath.Count == 0)
        {
             MoveDirectlyTo(currentWaypointTarget.position);
        }
        else
        {
            MoveAlongPath();
        }
    }

    private void Persecute()
    {
        currentPath.Clear(); 
        
        Vector3 steeringDir = steeringPursuit.GetSteerDir(rb.linearVelocity);
        Vector3 velocityBeforeAvoidance = steeringDir * speed;
        Vector3 avoidedDirection = steeringAvoidance.GetDir(velocityBeforeAvoidance);
        Vector3 finalVelocity = avoidedDirection.normalized * speed;
        if (finalVelocity.sqrMagnitude < 0.1f) finalVelocity = velocityBeforeAvoidance;
        Vector3 flatVel = new Vector3(finalVelocity.x, rb.linearVelocity.y, finalVelocity.z);
        rb.linearVelocity = flatVel;
        
        RotateTowards(finalVelocity.normalized, chaseTurnSpeed); 
    }

    private void Idle()
    {
        StopMovement();
        if (idleTimerHandler <= 0)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            currentWaypointTarget = patrolPoints[patrolIndex];
            idleTimerHandler = Random.Range(minIdleTime, maxIdleTime);
        }
        else
        {
            idleTimerHandler -= Time.deltaTime;
        }
    }
    
    private void Attack()
    {
        StopMovement();
        RotateTowards((player.transform.position - transform.position).normalized, chaseTurnSpeed);

        if (!canAttack || isAttacking) return;
        StartCoroutine(DoAttack());
        StartCoroutine(AttackCooldown());
    }

    private void Death()
    {
        StopMovement();
        Destroy(gameObject, 0.1f); 
    }

    // --- LOGIC A* ---

    private void RequestPathTo(Vector3 targetPos)
    {
        PFNode startNode = PathFindingManager.instance.Closest(transform.position);
        PFNode endNode = PathFindingManager.instance.Closest(targetPos);

        if (startNode == null || endNode == null) return;

        PathFindingManager.instance.goal = endNode; 
        currentPath = PathFindingManager.instance.GetPath(startNode);
        
        if (currentPath != null && currentPath.Count > 1)
        {
            if (GetFlatDistance(transform.position, currentPath[0].transform.position) < nodeReachedDistance)
                currentPath.RemoveAt(0);
        }
    }

    private void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        if (currentPath.Count > 1)
        {
            float distToCurrent = GetFlatDistance(transform.position, currentPath[0].transform.position);
            float distToNext = GetFlatDistance(transform.position, currentPath[1].transform.position);
            if (distToNext < distToCurrent) currentPath.RemoveAt(0);
        }
        
        if (currentPath.Count == 0) return;

        PFNode targetNode = currentPath[0];

        Vector3 desiredDirection = (targetNode.transform.position - transform.position).normalized;
        Vector3 flatDesiredDir = new Vector3(desiredDirection.x, 0, desiredDirection.z).normalized;
        Vector3 avoidanceDir = steeringAvoidance.GetDir(flatDesiredDir * speed);

        Vector3 finalFlatDir = new Vector3(avoidanceDir.x, 0, avoidanceDir.z).normalized * speed;
        
        if (finalFlatDir.sqrMagnitude < 0.1f) finalFlatDir = flatDesiredDir * speed;

        rb.linearVelocity = new Vector3(finalFlatDir.x, rb.linearVelocity.y, finalFlatDir.z);
        
        RotateTowards(finalFlatDir.normalized, patrolTurnSpeed);

        if (GetFlatDistance(transform.position, targetNode.transform.position) < nodeReachedDistance)
        {
            currentPath.RemoveAt(0);
        }
    }

    // --- UTILIDADES ---

    private void MoveDirectlyTo(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Vector3 flatDir = new Vector3(direction.x, 0, direction.z).normalized;
        rb.linearVelocity = new Vector3(flatDir.x * speed, rb.linearVelocity.y, flatDir.z * speed);
        
        RotateTowards(flatDir, patrolTurnSpeed);
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    private void RotateTowards(Vector3 dir, float currentTurnSpeed)
    {
        if (dir.sqrMagnitude < 0.1f) return; 
        
        dir.Normalize();

        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));

        float angle = Quaternion.Angle(transform.rotation, lookRot);
        float dynamicSpeed = (angle > 90f) ? currentTurnSpeed * 2 : currentTurnSpeed;

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * dynamicSpeed);
    }

    private float GetFlatDistance(Vector3 a, Vector3 b)
    {
        Vector3 aFlat = new Vector3(a.x, 0, a.z);
        Vector3 bFlat = new Vector3(b.x, 0, b.z);
        return Vector3.Distance(aFlat, bFlat);
    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;
        if (attackVisual != null) Instantiate(attackVisual, transform.position + transform.forward, transform.rotation);
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }
    
    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}