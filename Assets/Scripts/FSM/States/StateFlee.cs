using UnityEngine;

public class StateFlee : State<UnitInputs>
{
    UnitBrain _brain;
    Flee _flee;

    public StateFlee(UnitBrain b) {
        _brain = b;
        _flee = new Flee(null, b.transform, b.MaxSpeed);
    }

    public override void Execute()
    {
        var enemy = _brain.GetNearestEnemy();
        
        if (enemy == null) {
            _brain.SetState(UnitInputs.Safe); 
            return;
        }

        _flee.SetTarget = enemy;
        Vector3 dir = _flee.GetSteerDir(_brain.Velocity);
        
        _brain.ApplyMovement(dir, false); 
    }
}