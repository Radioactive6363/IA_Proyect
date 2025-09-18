public abstract class BaseState : IState
{
    public FSM fsm => _fsm;
    private FSM _fsm;
    
    public BaseState(FSM fsm)
    {
        _fsm = fsm;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
