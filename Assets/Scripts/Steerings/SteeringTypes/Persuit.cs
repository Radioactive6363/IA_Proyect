using UnityEngine;

public class Persuit : ISteering
{
    private Transform npcTransform;
    private Transform target;
    private float maxSpeed;
    private float timePrediction = 0.5f;
    private Rigidbody rb;
    public Transform SetTarget
    {
        set
        {
            target = value;
        }
    }

    public Persuit(Transform target, Transform npcTransform, float maxSpeed)
    {
        this.target = target;
        rb = target.GetComponent<Rigidbody>();
        this.npcTransform = npcTransform;
        this.maxSpeed = maxSpeed;
    }

    public Vector3 GetSteerDir(Vector3 currentVelocity)
    {
        var dist = (target.position - npcTransform.position).magnitude;
        var futureTargetPosition = target.position + rb.linearVelocity * timePrediction * dist;

        var dirToPoint = futureTargetPosition - npcTransform.position;
        var dirToTarget = target.position - npcTransform.position;
        
        var dotRemaped = (Vector3.Dot(dirToPoint.normalized, dirToTarget.normalized) + 1) / 2;
        dirToPoint = Vector3.Lerp(dirToTarget, dirToPoint, dotRemaped);
        
        return dirToPoint.normalized; 
    }
}
