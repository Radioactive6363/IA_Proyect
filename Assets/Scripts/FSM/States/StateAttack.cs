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

        Transform targetTransform = null;
        if (_brain.lastAttacker != null && _brain.lastAttacker.currentHealth > 0)
            targetTransform = _brain.lastAttacker.transform;
        else
            targetTransform = _brain.GetNearestEnemy();

        if (targetTransform == null) {
            _brain.SetState(UnitInputs.LostSight);
            return;
        }
        
        float dist = Vector3.Distance(_brain.transform.position, targetTransform.position);
        
        if (dist <= _brain.attackRange)
        {
            _brain.StopMomentum(); 
            
            Vector3 dirToEnemy = (targetTransform.position - _brain.transform.position).normalized;
            dirToEnemy.y = 0;
            
            if (dirToEnemy != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dirToEnemy);
                
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, lookRot, 15f * Time.deltaTime);
            }
            
            var enemyBrain = targetTransform.GetComponent<UnitBrain>();
            if(enemyBrain != null) _brain.TryAttack(enemyBrain);
        }
        else
        {
            _persuit.SetTarget = targetTransform;
            Vector3 dir = _persuit.GetSteerDir(_brain.Velocity);
            _brain.ApplyMovement(dir, false); 
        }
    }
}