using UnityEngine;

public class StateAttack : State<UnitInputs>
{
    UnitBrain _brain;
    Pursuit _persuit;

    public StateAttack(UnitBrain b) 
    { 
        _brain = b; 
        _persuit = new Pursuit(null, b.transform, b.MaxSpeed);
    }

    public override void Execute()
    {
        if (_brain.currentHealth < _brain.maxHealth * 0.3f) {
            _brain.SetState(UnitInputs.LowHealth);
            return;
        }
        var enemyTransform = _brain.GetNearestEnemy();
        
        if (enemyTransform == null) {
            _brain.SetState(UnitInputs.LostSight);
            return;
        }
        UnitBrain enemyBrain = enemyTransform.GetComponent<UnitBrain>();
        if (enemyBrain == null) return;
        
        float dist = Vector3.Distance(_brain.transform.position, enemyTransform.position);
        
        
        if (dist <= _brain.attackRange)
        {
            _brain.ApplyMovement(Vector3.zero, false);
            Vector3 dirToEnemy = (enemyTransform.position - _brain.transform.position).normalized;
            if(dirToEnemy != Vector3.zero)
                _brain.transform.forward = Vector3.Lerp(_brain.transform.forward, dirToEnemy, 10 * Time.deltaTime);
            _brain.TryAttack(enemyBrain);
        }
        else
        {
            _persuit.SetTarget = enemyTransform;
            Vector3 dir = _persuit.GetSteerDir(_brain.Velocity);
            
            _brain.ApplyMovement(dir, false); 
        }
    }
}