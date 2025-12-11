using System.Collections.Generic;

public abstract class State<T> : IState<T>
{
    protected Dictionary<T, IState<T>> transitions = new Dictionary<T, IState<T>>();

    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }

    public void AddTransition(T input, IState<T> state)
    {
        if (!transitions.ContainsKey(input))
            transitions.Add(input, state);
    }

    public bool GetState(T input, out IState<T> state)
    {
        return transitions.TryGetValue(input, out state);
    }
}
