using UnityEngine;
using System.Collections.Generic;

public abstract class StateSO<TOwner> : ScriptableObject, IState<TOwner>
{
    [SerializeField] private List<Transition<TOwner>> transitions;
    public List<Transition<TOwner>> Transitions => transitions;

    public abstract void Enter(TOwner owner);
    public abstract void Execute(TOwner owner);
    public abstract void Exit(TOwner owner);
}

