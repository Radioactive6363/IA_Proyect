using UnityEngine;
using System;
using Random = UnityEngine.Random;
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

    [Header("Combat Memory")]
    public UnitBrain lastAttacker;

    [Header("Sensors & Visuals")]
    public FOV fov;
    public ObstacleAvoidance obstacleAvoidance;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Animator _animator;
    [SerializeField] private HealthBar _healthBar;
    
    [Header("Team Visuals")]
    [SerializeField] private Renderer _modelRenderer; 
    [SerializeField] private GameObject _leaderAccessory;
    [SerializeField] private Color _redColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color _blueColor = new Color(0.2f, 0.2f, 1f);

    [Header("Flocking Settings")]
    [SerializeField] float viewRadius = 10f;
    [SerializeField] float separationRadius = 2.5f;
    [SerializeField] float cohesionRadius = 7f;
    [SerializeField] float alignmentRadius = 7f;
    
    private FSM<UnitInputs> _fsm;
    private int _originalLayer;
    [SerializeField] private string ghostLayerName = "Ghost";
    
    public event Action<string> OnStateChanged; 
    public string CurrentStateName => _fsm != null ? _fsm.GetCurrentStateName() : "Init";
    
    public Transform Transform => transform;
    
    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
        
        _originalLayer = gameObject.layer;
        
        if (FlockingManager.Instance != null) 
            FlockingManager.Instance.AddUnit(this);
        
        if (!isLeader && myLeader == null)
        {
            FindMyLeader();
        }
        
        obstacleAvoidance = new ObstacleAvoidance(transform, 5f, 60f, 1.5f, obstacleLayer);
        
        ApplyTeamColor();
        UpdateLeaderVisuals();
        if(_healthBar != null) _healthBar.UpdateHealth(currentHealth, maxHealth);
        
        InitFSM();
    }

    private void FindMyLeader()
    {
        UnitBrain[] allUnits = FindObjectsByType<UnitBrain>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.teamID == this.teamID && unit.isLeader)
            {
                myLeader = unit;
                break;
            }
        }
    }

    private void InitFSM()
    {
        var patrol = new StatePatrol(this);
        var attack = new StateAttack(this); 
        var flee = new StateFlee(this);
        var idle = new StateIdle(this);
        
        if (isLeader)
        {
            var leaderLogic = new StateLeaderDecision(this);
            
            // Roulette
            leaderLogic.AddTransition(UnitInputs.DecisionPatrol, patrol);
            leaderLogic.AddTransition(UnitInputs.DecisionAttack, attack);
            leaderLogic.AddTransition(UnitInputs.DecisionRetreat, flee);
            leaderLogic.AddTransition(UnitInputs.DecisionIdle, idle);
            
            //Emergency
            leaderLogic.AddTransition(UnitInputs.UnderAttack, attack);
            
            patrol.AddTransition(UnitInputs.EnemySpotted, attack);
            patrol.AddTransition(UnitInputs.UnderAttack, attack);
            
            //Return To "Brain"
            attack.AddTransition(UnitInputs.LostSight, leaderLogic);
            flee.AddTransition(UnitInputs.Safe, leaderLogic);
            
            //Idle
            idle.AddTransition(UnitInputs.Safe, leaderLogic);
            idle.AddTransition(UnitInputs.EnemySpotted, attack);
            idle.AddTransition(UnitInputs.UnderAttack, attack);
            
            _fsm = new FSM<UnitInputs>(leaderLogic);
        }
        else
        {
            patrol.AddTransition(UnitInputs.EnemySpotted, attack);
            patrol.AddTransition(UnitInputs.UnderAttack, attack);
            patrol.AddTransition(UnitInputs.LowHealth, flee);
            patrol.AddTransition(UnitInputs.DecisionRetreat, flee);
            attack.AddTransition(UnitInputs.LowHealth, flee);
            attack.AddTransition(UnitInputs.LostSight, patrol);
            attack.AddTransition(UnitInputs.DecisionRetreat, flee);
            flee.AddTransition(UnitInputs.Safe, patrol);

            _fsm = new FSM<UnitInputs>(patrol);
        }
        _fsm.OnStateChanged += (stateName) => OnStateChanged?.Invoke(stateName);
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
            float currentSpeed = Velocity.magnitude;
            _animator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
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
        Vector3 steeringForce = Steer(desiredVelocity);
        
        Vector3 flockingForce = Vector3.zero;
        if (useFlocking)
        {
            flockingForce = CalculateFlockingForce();
        }
        
        Vector3 avoidanceDir = obstacleAvoidance.GetDir2(desiredVelocity);
        
        if(avoidanceDir != Vector3.zero) 
        {
            totalForce += Steer(avoidanceDir * MaxSpeed) * 5.0f; 
            
            totalForce += steeringForce * 0.2f; 
        }
        else
        {
            totalForce += steeringForce;
            if (useFlocking) totalForce += flockingForce;
        }
        
        AddForce(totalForce);
    }
    
    private Vector3 CalculateFlockingForce(bool onlySeparation = false)
    {
        Vector3 sepForce = Vector3.zero;
        Vector3 aliForce = Vector3.zero;
        Vector3 cohCenter = Vector3.zero;

        int sepCount = 0;
        int aliCount = 0;
        int cohCount = 0;
        
        float sqrSep = separationRadius * separationRadius;
        float sqrCoh = cohesionRadius * cohesionRadius;
        float sqrAli = alignmentRadius * alignmentRadius;
        float sqrView = viewRadius * viewRadius;

        var neighbors = FlockingManager.Instance.AllUnits;
        Vector3 myPos = transform.position;
        
        for (int i = 0; i < neighbors.Count; i++)
        {
            var n = neighbors[i];
            if (n == null || n == this) continue;
            if (n.teamID != this.teamID) continue;

            Vector3 offset = n.transform.position - myPos;
            float sqrDist = offset.sqrMagnitude;

            if (sqrDist > sqrView) continue;
            
            //Separation
            if (sqrDist < sqrSep)
            {
                sepForce += (myPos - n.transform.position) / sqrDist; 
                sepCount++;
            }
            
            if (onlySeparation) continue;

            //Alignment
            if (sqrDist < sqrAli)
            {
                aliForce += n.Velocity;
                aliCount++;
            }
            
            //Cohesion
            if (sqrDist < sqrCoh)
            {
                cohCenter += n.transform.position;
                cohCount++;
            }
        }

        Vector3 total = Vector3.zero;
        
        float sepMultiplier = onlySeparation ? 3.0f : 1.0f; 
        if (sepCount > 0)
            total += Steer(sepForce.normalized * MaxSpeed) * (FlockingManager.Instance.separationWeight * sepMultiplier);
        
        if (onlySeparation) return total;

        if (aliCount > 0)
        {
            aliForce /= aliCount;
            total += Steer(aliForce.normalized * MaxSpeed) * FlockingManager.Instance.alignmentWeight;
        }

        if (cohCount > 0)
        {
            cohCenter /= cohCount;
            Vector3 cohDir = (cohCenter - myPos).normalized * MaxSpeed;
            total += Steer(cohDir) * FlockingManager.Instance.cohesionWeight;
        }

        return total;
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

    public void TakeDamage(float amount, UnitBrain attacker)
    {
        currentHealth -= amount;
        
        lastAttacker = attacker;
        
        if (currentHealth > maxHealth * 0.3f)
        {
            SetState(UnitInputs.UnderAttack);
        }

        if(_healthBar != null) _healthBar.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    public void TryAttack(UnitBrain target)
    {
        if (Time.time >= _nextAttackTime)
        {
            if(_animator != null) _animator.SetTrigger("Attack");
            
            target.TakeDamage(attackDamage, this);
            
            _nextAttackTime = Time.time + attackRate;
        }
    }

    private void Die()
    {
        if(_animator != null) _animator.SetTrigger("Die");
        
        if (isLeader)
        {
            ElectNewLeader();
        }

        if (FlockingManager.Instance != null) 
            FlockingManager.Instance.RemoveUnit(this);
        
        this.enabled = false;
        
        if (GetComponent<Rigidbody>() != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        GetComponent<Collider>().enabled = false;
        
        if (_healthBar != null) Destroy(_healthBar.gameObject); 
        
        Destroy(gameObject, 3f);
    }
    
    public void ClearAttacker()
    {
        lastAttacker = null;
    }
    
    public void SetGhostMode(bool active)
    {
        if (active)
        {
            int ghostLayer = LayerMask.NameToLayer(ghostLayerName);
            if (ghostLayer != -1)
            {
                gameObject.layer = ghostLayer;
            }
        }
        else
        {
            gameObject.layer = _originalLayer;
        }
    }
    
    private void ApplyTeamColor()
    {
        if (_modelRenderer == null) return;
        
        if (teamID == BattleTeams.Red)
             _modelRenderer.material.color = _redColor;
        else if (teamID == BattleTeams.Blue)
             _modelRenderer.material.color = _blueColor;
    }
    
    private void UpdateLeaderVisuals()
    {
        if (_leaderAccessory != null)
            _leaderAccessory.SetActive(isLeader);
    }
    private void ElectNewLeader()
    {
        // 1. Buscar candidatos (De mi equipo, vivos y que no sea yo)
        var allUnits = FlockingManager.Instance.AllUnits;
        UnitBrain heir = null;
        List<UnitBrain> survivors = new List<UnitBrain>();

        foreach (var unit in allUnits)
        {
            if (unit != null && unit != this && unit.currentHealth > 0 && unit.teamID == this.teamID)
            {
                survivors.Add(unit);
            }
        }
        
        if (survivors.Count == 0) return;
        heir = survivors[Random.Range(0, survivors.Count)];
        heir.PromoteToLeader();
        
        foreach (var survivor in survivors)
        {
            if (survivor != heir)
            {
                survivor.myLeader = heir;
                
                survivor.SetState(UnitInputs.DecisionRetreat);
            }
        }
    }
    
    public void PromoteToLeader()
    {
        Debug.Log($"{name} new Leader");
        //RoleChange
        isLeader = true;
        myLeader = null;
        UpdateLeaderVisuals();
        InitFSM();
        SetState(UnitInputs.DecisionRetreat);
    }
}