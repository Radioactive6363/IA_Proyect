using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FOV))]
public class UnitBrain : SteeringEntity
{
    [Header("Team Info")]
    public BattleTeams teamID;
    public UnitBrain myLeader;
    public bool isLeader;

    [Header("Combat Stats")]
    public float maxHealth = 100;
    public float currentHealth;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackRate = 1.0f;
    private float _nextAttackTime = 0f;

    [Header("Sensors & Visuals")]
    public FOV fov;
    public ObstacleAvoidance obstacleAvoidance;
    [SerializeField] private Animator _animator;
    [SerializeField] private HealthBar _healthBar;
    
    [Header("Flocking Settings")]
    [SerializeField] float viewRadius = 10f;
    [SerializeField] float separationRadius = 2.5f;
    [SerializeField] float cohesionRadius = 7f;
    [SerializeField] float alignmentRadius = 7f;
    
    [Header("Team Visuals")]
    [SerializeField] private Renderer _modelRenderer; 
    [SerializeField] private GameObject _leaderAccessory;
    [SerializeField] private Color _redColor = new Color(1f, 0.2f, 0.2f); // Rojo suave
    [SerializeField] private Color _blueColor = new Color(0.2f, 0.2f, 1f); // Azul suave
    
    private FSM<UnitInputs> _fsm;
    
    public Transform Transform => transform;

    protected override void Start()
    {
        base.Start();
        
        currentHealth = maxHealth;
        if (FlockingManager.Instance != null) FlockingManager.Instance.AddUnit(this);
        obstacleAvoidance = new ObstacleAvoidance(transform, 4f, 45f, 1.5f, LayerMask.GetMask("Obstacle"));
        ApplyTeamColor();
        UpdateLeaderVisuals();
        if(_healthBar != null) _healthBar.UpdateHealth(currentHealth, maxHealth);
        InitFSM();
    }
    
    private void ApplyTeamColor()
    {
        if (_modelRenderer == null) return;
        
        if (teamID == BattleTeams.Red)
        {
            _modelRenderer.material.color = _redColor;
        }
        else if (teamID == BattleTeams.Blue)
        {
            _modelRenderer.material.color = _blueColor;
        }
    }
    
    private void UpdateLeaderVisuals()
    {
        if (_leaderAccessory != null)
        {
            _leaderAccessory.SetActive(isLeader);
        }
    }

    private void InitFSM()
    {
        var patrol = new StatePatrol(this);
        var attack = new StateAttack(this); 
        var flee = new StateFlee(this);
        
        if (isLeader)
        {
            var leaderLogic = new StateLeaderDecision(this);
            
            leaderLogic.AddTransition(UnitInputs.DecisionAttack, attack);
            leaderLogic.AddTransition(UnitInputs.DecisionRetreat, flee);
            
            attack.AddTransition(UnitInputs.LostSight, leaderLogic);
            flee.AddTransition(UnitInputs.Safe, leaderLogic);
            
            _fsm = new FSM<UnitInputs>(leaderLogic);
        }
        else
        {
            patrol.AddTransition(UnitInputs.EnemySpotted, attack);
            patrol.AddTransition(UnitInputs.LowHealth, flee);
            
            attack.AddTransition(UnitInputs.LostSight, patrol);
            attack.AddTransition(UnitInputs.LowHealth, flee);
            
            flee.AddTransition(UnitInputs.Safe, patrol);

            _fsm = new FSM<UnitInputs>(patrol);
        }
    }

    public void SetState(UnitInputs input)
    {
        _fsm.SetState(input);
    }
    
    void Update()
    {
        if(currentHealth <= 0) return;
        
        _fsm.OnUpdate();
        
        Move();
        
        if(_animator != null)
        {
            bool isMoving = Velocity.sqrMagnitude > 0.1f;
            _animator.SetBool("IsMoving", isMoving);
        }
    }

    void OnDestroy()
    {
        if(FlockingManager.Instance != null) 
            FlockingManager.Instance.RemoveUnit(this);
    }

    public void ApplyMovement(Vector3 desiredVelocity, bool useFlocking)
    {
        Vector3 totalForce = Vector3.zero;
        
        totalForce += Steer(desiredVelocity);
        
        if (useFlocking)
        {
            totalForce += CalculateFlockingForce();
        }
        
        Vector3 avoidanceDir = obstacleAvoidance.GetDir2(velocity);
        
        if(avoidanceDir != velocity) 
        {
            totalForce += Steer(avoidanceDir.normalized * MaxSpeed) * 2.5f; 
        }
        
        AddForce(totalForce);
    }
    
    private Vector3 CalculateFlockingForce()
    {
        Vector3 separationForce = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        Vector3 cohesionCenter = Vector3.zero;

        int separationCount = 0;
        int alignmentCount = 0;
        int cohesionCount = 0;

        float sqrSepRadius = separationRadius * separationRadius;
        float sqrCohRadius = cohesionRadius * cohesionRadius;
        float sqrAliRadius = alignmentRadius * alignmentRadius;
        float sqrViewRadius = viewRadius * viewRadius; 

        var neighbors = FlockingManager.Instance.AllUnits;
        Vector3 myPos = transform.position;
        
        for (int i = 0; i < neighbors.Count; i++)
        {
            var neighbor = neighbors[i];
            
            if (neighbor == null || neighbor == this) continue;
            if (neighbor.teamID != this.teamID) continue;

            Vector3 offset = neighbor.transform.position - myPos;
            float sqrDist = offset.sqrMagnitude;

            if (sqrDist > sqrViewRadius) continue;

            // Separación
            if (sqrDist < sqrSepRadius)
            {
                separationForce += (myPos - neighbor.transform.position) / sqrDist; 
                separationCount++;
            }

            // Alineación
            if (sqrDist < sqrAliRadius)
            {
                alignmentForce += neighbor.Velocity;
                alignmentCount++;
            }

            // Cohesión
            if (sqrDist < sqrCohRadius)
            {
                cohesionCenter += neighbor.transform.position;
                cohesionCount++;
            }
        }

        Vector3 totalFlock = Vector3.zero;

        if (separationCount > 0)
            totalFlock += Steer(separationForce.normalized * MaxSpeed) * FlockingManager.Instance.separationWeight;

        if (alignmentCount > 0)
        {
            alignmentForce /= alignmentCount;
            totalFlock += Steer(alignmentForce.normalized * MaxSpeed) * FlockingManager.Instance.alignmentWeight;
        }

        if (cohesionCount > 0)
        {
            cohesionCenter /= cohesionCount;
            Vector3 cohesionDir = (cohesionCenter - myPos).normalized * MaxSpeed;
            totalFlock += Steer(cohesionDir) * FlockingManager.Instance.cohesionWeight;
        }

        return totalFlock;
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
        if(_healthBar != null) 
            _healthBar.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    public void TryAttack(UnitBrain target)
    {
        if (Time.time >= _nextAttackTime)
        {
            if(_animator != null) _animator.SetTrigger("Attack");
            
            target.TakeDamage(attackDamage);
            _nextAttackTime = Time.time + attackRate;
        }
    }

    private void Die()
    {
        if(_animator != null) _animator.SetTrigger("Die");
        
        if (FlockingManager.Instance != null) 
            FlockingManager.Instance.RemoveUnit(this);
        
        this.enabled = false;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 3f);
    }
}