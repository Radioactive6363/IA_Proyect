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
    
    public Vector3 GetDir2(Vector3 checkVector)
    {
        Vector3 dir = checkVector.sqrMagnitude > 0.1f ? checkVector.normalized : npcTransform.forward;
        
        Vector3 origin = npcTransform.position + Vector3.up * 1.0f;
        
        if (Physics.SphereCast(origin, _personalArea, dir, out hit, _radius, _obsMask))
        {
            Vector3 hitNormal = hit.normal;
            
            Vector3 slideDir = Vector3.ProjectOnPlane(dir, hitNormal).normalized;
            
            if (slideDir == Vector3.zero)
            {
                slideDir = Vector3.Cross(hitNormal, Vector3.up).normalized;
            }

            Debug.DrawLine(origin, hit.point, Color.red);
            Debug.DrawRay(hit.point, slideDir * 2f, Color.cyan);
            
            return slideDir; 
        }
        
        return Vector3.zero;
    }
}