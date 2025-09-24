using UnityEngine;

public class Transition<TOwner> : MonoBehaviour
{
    public StateSO<TOwner> target;
    public ConditionSO<TOwner> condition;
}
