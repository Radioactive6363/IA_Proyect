using UnityEngine;

public class StateLeaderDecision : State<UnitInputs>
{
    UnitBrain _brain;
    BattleRoulette<UnitInputs> _roulette; 

    public StateLeaderDecision(UnitBrain b)
    {
        _brain = b;
        _roulette = new BattleRoulette<UnitInputs>();
        
        _roulette.AddItem(UnitInputs.DecisionAttack, 0.5f);
        _roulette.AddItem(UnitInputs.DecisionRetreat, 0.1f);
    }

    public override void Execute()
    {
        int enemies = 0; 
        if (_brain.currentHealth < _brain.maxHealth * 0.5f)
            _roulette.UpdateWeight(UnitInputs.DecisionRetreat, 0.9f);
        else
            _roulette.UpdateWeight(UnitInputs.DecisionRetreat, 0.1f);
        
        if (Random.value < 0.05f) 
        {
            var decision = _roulette.Run();
            if (decision != UnitInputs.None)
                _brain.SetState(decision); 
        }
    }
}
