using UnityEngine;

public class StateFlee : State<UnitInputs>
{
    UnitBrain _brain;
    Flee _flee;

    public StateFlee(UnitBrain b) {
        _brain = b;
        _flee = new Flee(null, b.transform, b.MaxSpeed);
    }
    public override void Enter()
    {
        // 1. Activar intangibilidad (Modo Fantasma) para mí mismo
        _brain.SetGhostMode(true);

        // 2. Si soy LÍDER, ordeno la retirada general
        if (_brain.isLeader)
        {
            CommandGeneralRetreat();
        }
    }

    private void CommandGeneralRetreat()
    {
        var allUnits = FlockingManager.Instance.AllUnits;
        foreach (var unit in allUnits)
        {
            if (unit != null && unit.currentHealth > 0 &&
                unit.teamID == _brain.teamID && unit.myLeader == _brain)
            {
                unit.SetState(UnitInputs.DecisionRetreat);
            }
        }
    }
    

    public override void Exit()
    {
        _brain.SetGhostMode(false);
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