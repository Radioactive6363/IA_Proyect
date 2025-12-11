using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public static FlockingManager Instance { get; private set; }

    [Header("Weights")]
    [Range(0, 5f)] public float separationWeight = 1.5f;
    [Range(0, 5f)] public float cohesionWeight = 1f;
    [Range(0, 5f)] public float alignmentWeight = 1f;

    private List<UnitBrain> allUnits = new List<UnitBrain>();
    public List<UnitBrain> AllUnits => allUnits;

    void Awake() { Instance = this; }

    public void AddUnit(UnitBrain unit)
    {
        if (!allUnits.Contains(unit)) allUnits.Add(unit);
    }
    
    public void RemoveUnit(UnitBrain unit)
    {
        if (allUnits.Contains(unit)) allUnits.Remove(unit);
    }
}