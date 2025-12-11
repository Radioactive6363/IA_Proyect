using UnityEngine;

public class FSM<T>
{
    IState<T> current;

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
        }
    }
}