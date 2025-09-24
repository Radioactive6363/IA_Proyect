public class FSM<TOwner>
{
    private StateSO<TOwner> currentState;
    private TOwner owner;

    public FSM(TOwner owner, StateSO<TOwner> initialState)
    {
        this.owner = owner;
        ChangeState(initialState);
    }

    public void Update()
    {
        foreach (var t in currentState.Transitions)
        {
            if (t.condition != null && t.condition.Evaluate(owner))
            {
                ChangeState(t.target);
                break;
            }
        }
        currentState.Execute(owner);
    }

    private void ChangeState(StateSO<TOwner> newState)
    {
        currentState?.Exit(owner);
        currentState = newState;
        currentState.Enter(owner);
    }
}