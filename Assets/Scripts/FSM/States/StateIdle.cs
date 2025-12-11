using UnityEngine;

public class StateIdle : State<UnitInputs>
{
    UnitBrain _brain;
    float _timer;
    float _duration;

    public StateIdle(UnitBrain b) 
    { 
        _brain = b; 
    }

    public override void Enter()
    {
        _duration = Random.Range(1f, 3f);
        _timer = 0;
        
        _brain.ApplyMovement(Vector3.zero, false);
    }

    public override void Execute()
    {
        _timer += Time.deltaTime;
        
        if (_brain.currentHealth < _brain.maxHealth)
            _brain.currentHealth += Time.deltaTime;
        
        if (_brain.GetNearestEnemy() != null)
        {
            _brain.SetState(UnitInputs.EnemySpotted);
            return;
        }
        
        if (_brain.lastAttacker != null)
        {
            _brain.SetState(UnitInputs.UnderAttack);
            return;
        }
        
        if (_timer >= _duration)
        {
            _brain.SetState(UnitInputs.Safe); 
        }
    }
}