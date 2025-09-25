using UnityEngine;

public class BaseTree : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] int health;
    [SerializeField] int maxHealth;
    [SerializeField] FOV fieldOfView;
    [SerializeField] float attackRange;
    [SerializeField] float maxSpeed;
    [SerializeField] Vector3 velocity;
    [SerializeField, Range(0, 100)] int lowHealthThreshold;
    
    [Header("Steering")]
    [SerializeField] GameObject target;
    [SerializeField] Transform[] waypoints;
    [SerializeField] int currentWP;
    [SerializeField] float arriveRange;
    
    private ITreeNode _rootNode;
    private Seek seek;
    private Flee flee;
    private Persuit persuit;
    private Evade evade;
    private Arrive arrive;
    
    void Start()
    {
        CreateTree();
        seek = new Seek(waypoints[currentWP].transform, transform, maxSpeed);
        flee = new Flee(target.transform, transform, maxSpeed);
        persuit = new Persuit(target.transform, transform, maxSpeed);
        evade = new Evade(target.transform, transform, maxSpeed);
        arrive = new Arrive(waypoints[currentWP].transform, transform, maxSpeed, arriveRange);
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
            Vector3.Distance(transform.position, waypoints[currentWP].position) < 0.3f, idle, patrol);
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
    
    private void Die() { Debug.Log("Die"); }
    
    private void Flee() 
    { 
        Debug.Log("Flee");
        //velocity = flee.GetSteerDir(velocity);
        velocity = evade.GetSteerDir(velocity);
        transform.position += velocity * Time.deltaTime;
    }

    private void Attack()
    {
        Debug.Log("Attack");
    }
    
    private void Patrol() 
    { 
        Debug.Log("Patrol");
        //var dir = waypoints[currentWP].position - transform.position;
        //transform.position += dir.normalized * Time.deltaTime * speed;

        velocity = arrive.GetSteerDir(velocity);
        transform.position += velocity * Time.deltaTime;
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
        transform.position += velocity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
