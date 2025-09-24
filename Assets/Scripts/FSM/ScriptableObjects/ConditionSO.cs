using UnityEngine;

public abstract class ConditionSO<TOwner> : ScriptableObject
{
    public abstract bool Evaluate(TOwner owner);
}