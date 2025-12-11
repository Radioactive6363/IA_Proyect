using UnityEngine;

public class StateLeaderDecision : State<UnitInputs>
{
    UnitBrain _brain;
    BattleRoulette<UnitInputs> _roulette; 

    public StateLeaderDecision(UnitBrain b)
    {
        _brain = b;
        _roulette = new BattleRoulette<UnitInputs>();
        
        _roulette.AddItem(UnitInputs.DecisionPatrol, 1.0f);
        _roulette.AddItem(UnitInputs.DecisionAttack, 0.0f);
        _roulette.AddItem(UnitInputs.DecisionRetreat, 0.0f);
        _roulette.AddItem(UnitInputs.DecisionIdle, 0.2f);
    }

    public override void Execute()
    {
        int enemies = 0;
        foreach (var unit in FlockingManager.Instance.AllUnits)
        {
            if (unit != null && unit.teamID != _brain.teamID)
            {
                if (_brain.fov.IsInSight(unit.transform.position)) enemies++;
            }
        }
        
        if (enemies > 0)
        {
            // Watching enemys : High Priority Attack
            _roulette.UpdateWeight(UnitInputs.DecisionAttack, 0.8f);
            _roulette.UpdateWeight(UnitInputs.DecisionPatrol, 0.0f); 
        }
        else
        {
            // No enemies = Patrol : High Priority Patrol
            _roulette.UpdateWeight(UnitInputs.DecisionAttack, 0.0f);
            _roulette.UpdateWeight(UnitInputs.DecisionPatrol, 1.0f);
        }

        // Survival
        if (_brain.currentHealth < _brain.maxHealth * 0.5f)
            _roulette.UpdateWeight(UnitInputs.DecisionRetreat, 1.5f);
        else
            _roulette.UpdateWeight(UnitInputs.DecisionRetreat, 0.0f);

        // Spin Wheel
        if (Random.value < 0.1f)
        {
            var decision = _roulette.Run();
            if (decision != UnitInputs.None)
                _brain.SetState(decision); 
        }
    }
}