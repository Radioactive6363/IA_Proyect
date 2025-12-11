using System.Collections.Generic;
using UnityEngine;

public class CohesionBehaviour : MonoBehaviour, IFlockingBehaviour
{
    [SerializeField] float radius = 5f;

    public Vector3 GetDir(List<UnitBrain> neighbors, UnitBrain owner)
    {
        Vector3 centerOfMass = Vector3.zero;
        int count = 0;
        float sqrRadius = radius * radius;
        Vector3 ownerPos = transform.position;

        foreach (var neighbor in neighbors)
        {
            if (neighbor == null || neighbor == owner) continue;
            if (neighbor.teamID != owner.teamID) continue;

            Vector3 offset = neighbor.transform.position - ownerPos;
            
            // Chequeo optimizado
            if (offset.sqrMagnitude < sqrRadius)
            {
                centerOfMass += neighbor.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        centerOfMass /= count;
        return (centerOfMass - ownerPos).normalized;
    }
}