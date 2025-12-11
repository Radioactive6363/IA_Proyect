using UnityEngine;

public class Pursuit : ISteering // Corregido el nombre a Pursuit
{
    private Transform npcTransform;
    private Transform target;
    private float maxSpeed;
    private float maxPredictionTime = 1f;
    private Rigidbody rb;

    public Transform SetTarget 
    { 
        set 
        { 
            target = value; 
            if(target != null) rb = target.GetComponent<Rigidbody>();
        } 
    }

    public Pursuit(Transform target, Transform npcTransform, float maxSpeed)
    {
        this.target = target;
        this.npcTransform = npcTransform;
        this.maxSpeed = maxSpeed;
        if (target != null) rb = target.GetComponent<Rigidbody>();
    }

    public Vector3 GetSteerDir(Vector3 currentVelocity)
    {
        if (target == null) return Vector3.zero;

        float dist = (target.position - npcTransform.position).magnitude;
        
        Vector3 targetVel = Vector3.zero;
        if (rb != null)
        {
            targetVel = rb.linearVelocity; 
        }
        float lookAhead = Mathf.Clamp(dist / maxSpeed, 0, maxPredictionTime);
        
        Vector3 futurePosition = target.position + (targetVel * lookAhead);

        return (futurePosition - npcTransform.position).normalized * maxSpeed;
    }
}