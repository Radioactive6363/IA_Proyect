using UnityEngine;

public class UnitBrain : SteeringEntity, IDetectable
{
    [Header("Team Info")]
    public string teamID;
    public UnitBrain myLeader; 
    public bool isLeader;
    
    [Header("Combat Stats")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackRate = 1.0f;
    private float _nextAttackTime = 0f;

    [Header("Stats")]
    public float maxHealth = 100;
    public float currentHealth;

    [Header("Components")]
    public FOV fov;
    public ObstacleAvoidance obstacleAvoidance;
    
    private SeparationBehaviour _separation;
    private CohesionBehaviour _cohesion;
    private AlignmentBehaviour _alignment;
    
    private FSM<UnitInputs> _fsm;
    
    public Transform Transform => transform;
    public Transform[] DetectablePositions => new Transform[] { transform };
    
    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        
        FlockingManager.Instance.AddUnit(this);
        
        _separation = GetComponent<SeparationBehaviour>();
        _cohesion = GetComponent<CohesionBehaviour>();
        _alignment = GetComponent<AlignmentBehaviour>();
        
        obstacleAvoidance = new ObstacleAvoidance(transform, 3f, 45f, 1.5f, LayerMask.GetMask("Default"));

        InitFSM();
    }

    private void InitFSM()
    {
        var patrol = new StatePatrol(this);
        var attack = new StateAttack(this); 
        var flee = new StateFlee(this);
        
        patrol.AddTransition(UnitInputs.EnemySpotted, attack);
        patrol.AddTransition(UnitInputs.LowHealth, flee);
        
        attack.AddTransition(UnitInputs.LostSight, patrol);
        attack.AddTransition(UnitInputs.LowHealth, flee);
        
        flee.AddTransition(UnitInputs.Safe, patrol);
        
        if (isLeader)
        {
            var leaderLogic = new StateLeaderDecision(this);
            
            leaderLogic.AddTransition(UnitInputs.DecisionAttack, attack);
            leaderLogic.AddTransition(UnitInputs.DecisionRetreat, flee);
            
            attack.AddTransition(UnitInputs.Safe, leaderLogic);
            flee.AddTransition(UnitInputs.Safe, leaderLogic);

            _fsm = new FSM<UnitInputs>(leaderLogic);
        }
        else
        {
            _fsm = new FSM<UnitInputs>(patrol);
        }
    }
    
    public void SetState(UnitInputs input)
    {
        _fsm.SetState(input);
    }

    void Update()
    {
        if(currentHealth <= 0) 
        {
             // Opcional: Manejo de muerte simple
             Destroy(gameObject);
             return;
        }

        _fsm.OnUpdate();
        Move(); 
    }

    void OnDestroy()
    {
        if(FlockingManager.Instance != null) FlockingManager.Instance.RemoveUnit(this);
    }
    
    public void ApplyMovement(Vector3 desiredVelocity, bool useFlocking)
    {
        Vector3 totalForce = Vector3.zero;
        
        Vector3 steeringForce = Steer(desiredVelocity);
        totalForce += steeringForce;
        
        if (useFlocking)
        {
            var allUnits = FlockingManager.Instance.AllUnits;
            
            Vector3 sep = _separation.GetDir(allUnits, this) * FlockingManager.Instance.separationWeight;
            Vector3 coh = _cohesion.GetDir(allUnits, this) * FlockingManager.Instance.cohesionWeight;
            Vector3 ali = _alignment.GetDir(allUnits, this) * FlockingManager.Instance.alignmentWeight;
            
            totalForce += Steer(sep * MaxSpeed); 
            totalForce += Steer(coh * MaxSpeed);
            totalForce += Steer(ali * MaxSpeed);
        }
        Vector3 avoidanceDir = obstacleAvoidance.GetDir2(velocity);
        
        if(avoidanceDir != velocity) 
        {
            totalForce += Steer(avoidanceDir.normalized * MaxSpeed) * 2f; 
        }
        
        AddForce(totalForce);
    }
    
    public Transform GetNearestEnemy()
    {
        float minDist = 999f;
        Transform bestTarget = null;
        foreach(var unit in FlockingManager.Instance.AllUnits)
        {
            if(unit == null) continue;

            if(unit.teamID != this.teamID)
            {
                float d = Vector3.Distance(transform.position, unit.transform.position);
                if(d < minDist && fov.IsInSight(unit.transform.position))
                {
                    minDist = d;
                    bestTarget = unit.transform;
                }
            }
        }
        return bestTarget;
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        Debug.DrawRay(transform.position, Vector3.up * 2, Color.red, 0.2f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TryAttack(UnitBrain target)
    {
        if (Time.time >= _nextAttackTime)
        {
            target.TakeDamage(attackDamage);
            
            _nextAttackTime = Time.time + attackRate;
            
            Debug.DrawLine(transform.position, target.transform.position, Color.yellow, 0.1f);
            Debug.Log($"{name} atacó a {target.name} | HP Restante: {target.currentHealth}");
        }
    }
    private void Die()
    {
        if (FlockingManager.Instance != null) 
            FlockingManager.Instance.RemoveUnit(this);
            
        Destroy(gameObject);
    }
}