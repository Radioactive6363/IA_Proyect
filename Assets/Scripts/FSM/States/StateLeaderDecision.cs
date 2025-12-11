using UnityEngine;

public class StateLeaderDecision : State<UnitInputs>
{
    UnitBrain _brain;
    BattleRoulette<UnitInputs> _roulette; 

    public StateLeaderDecision(UnitBrain b)
    {
        _brain = b;
        _roulette = new BattleRoulette<UnitInputs>();
        
        // Pesos iniciales
        _roulette.AddItem(UnitInputs.DecisionAttack, 0.2f);
        _roulette.AddItem(UnitInputs.DecisionRetreat, 0.1f);
    }

    public override void Execute()
    {
        int enemies = 0;
        
        foreach (var unit in FlockingManager.Instance.AllUnits)
        {
            if (unit != null && unit.teamID != _brain.teamID)
            {
                if (_brain.fov.IsInSight(unit.transform.position))
                {
                    enemies++;
                }
            }
        }
        
        if (enemies > 0)
            _roulette.UpdateWeight(UnitInputs.DecisionAttack, 0.8f);
        else
            _roulette.UpdateWeight(UnitInputs.DecisionAttack, 0.1f);

        if (_brain.currentHealth < _brain.maxHealth * 0.5f)
            _roulette.UpdateWeight(UnitInputs.DecisionRetreat, 1.0f);
        else
            _roulette.UpdateWeight(UnitInputs.DecisionRetreat, 0.0f);
        
        if (Random.value < 0.05f) 
        {
            var decision = _roulette.Run();
            
            if (decision != UnitInputs.None)
                _brain.SetState(decision); 
        }
    }
}
