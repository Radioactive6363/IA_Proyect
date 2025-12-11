using System.Collections.Generic;
using UnityEngine;

public class SeparationBehaviour : MonoBehaviour, IFlockingBehaviour
{
    [SerializeField] float radius = 2f;

    public Vector3 GetDir(List<UnitBrain> neighbors, UnitBrain owner)
    {
        Vector3 force = Vector3.zero;
        int count = 0;
        float sqrRadius = radius * radius;
        
        Vector3 ownerPos = transform.position; 
        
        foreach (var neighbor in neighbors)
        {
            if (neighbor == null || neighbor == owner) continue;
            
            if (neighbor.teamID != owner.teamID) continue;
            
            Vector3 offset = ownerPos - neighbor.transform.position;
            float sqrDist = offset.sqrMagnitude;

            if (sqrDist < sqrRadius && sqrDist > 0)
            {
                force += offset / sqrDist; 
                count++;
            }
        }
        return count > 0 ? force.normalized : Vector3.zero;
    }
}