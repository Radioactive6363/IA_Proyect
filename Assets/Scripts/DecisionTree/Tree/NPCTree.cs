using UnityEngine;

public class NPCTree : BaseTree
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
    private Persuit persuit;
    private Evade evade;
    private Arrive arrive;
    
    void Start()
    {
        CreateTree();
        persuit = new Persuit(target.transform, transform, maxSpeed);
        evade = new Evade(target.transform, transform, maxSpeed);
        arrive = new Arrive(waypoints[currentWP].transform, transform, maxSpeed, arriveRange);
    }

    protected override void CreateTree()
    {
        //Actions
        ActionNode die = new(Die);
        ActionNode patrol = new(Patrol);
        ActionNode idle = new(Idle);
        ActionNode flee = new(Flee);
        ActionNode persuit = new(Persuit);
        ActionNode attack = new(Attack);


        QuestionNode inAttackRange = new(() => Vector3.Distance(transform.position, target.transform.position) < attackRange, attack, persuit);
        QuestionNode arrivedAtPoint = new(() => Vector3.Distance(transform.position, waypoints[currentWP].position) < 0.3f, idle, patrol);
        QuestionNode lowHealth = new(() => health < maxHealth * lowHealthThreshold / 100, flee, inAttackRange);
        QuestionNode isPInSight = new(fieldOfView.CheckDetection, lowHealth, arrivedAtPoint);
        QuestionNode isAlive = new(() => health <= 0, die, isPInSight);

        _rootNode = isAlive;
    }
    
    void Update()
    {
        _rootNode.Execute();
    }

    private void Die()
    {
        Debug.Log("Die"); 
        
    }
    
    private void Flee() 
    { 
        Debug.Log("Flee");
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
