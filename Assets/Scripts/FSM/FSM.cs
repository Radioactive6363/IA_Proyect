using System;

public class FSM<T>
{
    IState<T> current;
    public event Action<string> OnStateChanged;

    public FSM(IState<T> initial)
    {
        current = initial;
        current.Enter();
    }

    public void OnUpdate()
    {
        if(current != null) current.Execute();
    }

    public void SetState(T input)
    {
        if (current.GetState(input, out IState<T> newState))
        {
            current.Exit();
            current = newState;
            current.Enter();
            OnStateChanged?.Invoke(current.GetType().Name);
        }
    }
    
    //Debugging
    public string GetCurrentStateName()
    {
        return current != null ? current.GetType().Name : "None";
    }
}