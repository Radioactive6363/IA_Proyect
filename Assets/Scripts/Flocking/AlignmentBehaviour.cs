using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehaviour : MonoBehaviour, IFlockingBehaviour
{
    [SerializeField] float radius = 5f;

    public Vector3 GetDir(List<UnitBrain> neighbors, UnitBrain owner)
    {
        Vector3 averageHeading = Vector3.zero;
        int count = 0;
        float sqrRadius = radius * radius;
        Vector3 ownerPos = transform.position;

        foreach (var neighbor in neighbors)
        {
            if (neighbor == null || neighbor == owner) continue;
            if (neighbor.teamID != owner.teamID) continue; // Solo nos alineamos con aliados

            // Chequeo optimizado
            if ((neighbor.transform.position - ownerPos).sqrMagnitude < sqrRadius)
            {
                averageHeading += neighbor.Velocity; 
                count++;
            }
        }

        // Validación extra para evitar NaN
        if (count > 0 && averageHeading.sqrMagnitude > 0.001f)
            return averageHeading.normalized;

        return Vector3.zero;
    }
}