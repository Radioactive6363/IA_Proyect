using System;
using UnityEngine;

public class NPCTree : MonoBehaviour
{
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [SerializeField, Range(0, 100)] int lowHealthThreshold;

    [SerializeField] FOV fieldOfView;
    [SerializeField] GameObject target;

    [SerializeField] Transform[] waypoints;
    [SerializeField] int currentWP;

    [SerializeField] float attackRange;
    [SerializeField] float arriveRange;

    [SerializeField] float maxSpeed;
    [SerializeField] Vector3 velocity;

    [SerializeField] float avoidanceRange;
    [SerializeField] float personalArea;
    [SerializeField] LayerMask obstacleMask;

    private ITreeNode _rootNode;

    private Seek seek;
    private Flee flee;
    private Persuit persuit;
    private Evade evade;
    private Arrive arrive;
    private ObstacleAvoidance avoidance;
    
    void Start()
    {
        CreateTree();
        seek = new Seek(waypoints[currentWP].transform, transform, maxSpeed);
        flee = new Flee(target.transform, transform, maxSpeed);
        persuit = new Persuit(target.transform, transform, maxSpeed);
        evade = new Evade(target.transform, transform, maxSpeed);
        arrive = new Arrive(waypoints[currentWP].transform, transform, maxSpeed, arriveRange);
        avoidance = new ObstacleAvoidance(transform, avoidanceRange, 40, personalArea, obstacleMask);
    }

    private void CreateTree()
    {
        ActionNode die = new(Die);
        ActionNode patrol = new(Patrol);
        ActionNode idle = new(Idle);
        ActionNode flee = new(Flee);
        ActionNode persuit = new(Persuit);
        ActionNode attack = new(Attack);


        QuestionNode inAttackRange = new(() => 
            Vector3.Distance(transform.position, target.transform.position) < attackRange, attack, persuit);
        QuestionNode arrivedAtPoint = new(() =>
            (transform.position - waypoints[currentWP].position).sqrMagnitude < 1f, idle, patrol);
        QuestionNode lowHealth = new(() => health < maxHealth * lowHealthThreshold / 100, flee, inAttackRange);
        QuestionNode isPInSight = new(fieldOfView.CheckDetection, lowHealth, arrivedAtPoint);
        QuestionNode isAlive = new(() => health <= 0, die, isPInSight);

        _rootNode = isAlive;
    }
    
    private bool IsAlive()
    {
        return health <= 0;
    }

    void Update()
    {
        _rootNode.Execute();
    }

    private void HandleMessage(string message) { }
    
    private void Die() { Debug.Log("Die"); }
    
    private void Flee() 
    { 
        Debug.Log("Flee");
        velocity = evade.GetSteerDir(velocity);
        Move();
    }
    private void Attack() { Debug.Log("Attack"); }
    private void Patrol() 
    { 
        Debug.Log("Patrol");

        velocity = arrive.GetSteerDir(velocity);
        Move();
    }
    private void Idle() 
    {
        currentWP++;
        if (currentWP >= waypoints.Length)
            currentWP = 0;
        arrive.SetTarget = waypoints[currentWP].transform;
        Debug.Log("Idle"); 
    }
    private void Persuit() 
    {
        Debug.Log("Persuit");
        velocity = persuit.GetSteerDir(velocity);
        Move();
    }

    private void Move()
    {
        velocity = avoidance.GetDir2(velocity);
        transform.position += velocity * Time.deltaTime;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);


        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, avoidanceRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, personalArea);
    }
}
