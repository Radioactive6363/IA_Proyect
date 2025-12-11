using UnityEngine;

public class Arrive : ISteering
{
    private Transform npcTransform;
    private Transform target;
    private float maxSpeed;
    private float slowingRange;

    public Transform SetTarget { set { target = value; } }

    public Arrive(Transform target, Transform npcTransform, float maxSpeed, float slowingRange)
    {
        this.target = target;
        this.npcTransform = npcTransform;
        this.maxSpeed = maxSpeed;
        this.slowingRange = slowingRange;
    }

    public Vector3 GetSteerDir(Vector3 currentVelocity)
    {
        if (target == null) return Vector3.zero;

        Vector3 dir = target.position - npcTransform.position;
        float dist = dir.magnitude;
        
        if (dist < 0.1f) return Vector3.zero;

        float targetSpeed = maxSpeed;
        
        if (dist < slowingRange)
        {
            targetSpeed = maxSpeed * (dist / slowingRange);
        }

        return dir.normalized * targetSpeed;
    }
}