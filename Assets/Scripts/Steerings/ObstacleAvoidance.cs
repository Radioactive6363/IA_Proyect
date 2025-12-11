using UnityEngine;

public class ObstacleAvoidance
{
    Transform npcTransform;
    float _radius;
    float _angle;
    float _personalArea; 
    LayerMask _obsMask;
    
    RaycastHit hit;

    public ObstacleAvoidance(Transform entity, float radius, float angle, float personalArea, LayerMask obsMask)
    {
        npcTransform = entity;
        _radius = radius;
        _angle = angle;
        _personalArea = personalArea;
        _obsMask = obsMask;
    }
    
    public Vector3 GetDir2(Vector3 currVelocity)
    {
        Vector3 dir = currVelocity.sqrMagnitude > 0 ? currVelocity.normalized : npcTransform.forward;
        
        if (Physics.SphereCast(npcTransform.position, _personalArea, dir, out hit, _radius, _obsMask))
        {
            Vector3 hitNormal = hit.normal;
            Vector3 avoidDir = Vector3.Reflect(dir, hitNormal); 
            
            Debug.DrawLine(npcTransform.position, hit.point, Color.red);
            Debug.DrawRay(hit.point, avoidDir, Color.green);
            
            return avoidDir.normalized * currVelocity.magnitude;
        }
        return currVelocity;
    }
}