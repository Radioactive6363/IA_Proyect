using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Patrol, Alert, Flee }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FOV fov;
    [SerializeField] private NavMeshAgent agent;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointTolerance = 0.5f;
    private int wpIndex = 0;

    [Header("Communication")]
    [SerializeField] private float alertRadius = 10f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float alertDuration = 6f;

    [Header("Flee Settings")]
    [SerializeField] private float fleeDistance = 10f;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float fleeSpeed = 6f;

    private EnemyState currentState = EnemyState.Patrol;
    private float stateTimer = 0f;
    private Transform player;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (fov == null) fov = GetComponent<FOV>();

        if (fov != null && fov.Target != null)
            player = fov.Target.transform;
    }

    void Start()
    {
        EnterPatrol();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                if (fov.CheckDetection()) OnPlayerDetected();
                break;

            case EnemyState.Alert:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    EnterFlee();
                break;

            case EnemyState.Flee:
                FleeUpdate();
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    EnterPatrol();
                break;
        }
    }

    #region --- PATROL ---
    void EnterPatrol()
    {
        currentState = EnemyState.Patrol;
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[wpIndex].position);
    }

    void PatrolUpdate()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
        {
            wpIndex = (wpIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[wpIndex].position);
        }
    }
    #endregion

    #region --- ALERT ---
    void OnPlayerDetected()
    {
        if (currentState == EnemyState.Alert || currentState == EnemyState.Flee)
            return;

        EnterAlert();
    }

    void EnterAlert()
    {
        currentState = EnemyState.Alert;
        stateTimer = 1f;
        agent.isStopped = true;
        NotifyNearbyEnemies();
        // Aquí puedes agregar animación o sonido de alerta
    }

    void NotifyNearbyEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, alertRadius, enemyMask);
        foreach (var c in hits)
        {
            if (c.gameObject == gameObject) continue;
            EnemyAI other = c.GetComponent<EnemyAI>();
            if (other != null)
                other.ReceiveAlert();
        }
    }

    public void ReceiveAlert()
    {
        if (currentState != EnemyState.Flee)
            EnterFlee();
    }
    #endregion

    #region --- FLEE ---
    void EnterFlee()
    {
        currentState = EnemyState.Flee;
        agent.isStopped = false;
        agent.speed = fleeSpeed;
        stateTimer = alertDuration;
        SetFleeDestination();
    }

    void SetFleeDestination()
    {
        if (player == null) return;

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 target = transform.position + dir * fleeDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void FleeUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SetFleeDestination();
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (currentState == EnemyState.Flee)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }
    }
}
