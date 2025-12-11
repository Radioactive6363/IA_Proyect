using UnityEngine;

public class SteeringEntity : MonoBehaviour
{
    [SerializeField] protected float maxSpeed;
    [SerializeField] protected float maxForce;
    [SerializeField] float turnSpeed = 10f;
    
    public float MaxSpeed => maxSpeed;
    public float MaxForce => maxForce;
    public Vector3 Velocity => velocity; 
    
    protected Vector3 velocity;
    protected Rigidbody rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; 
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public Vector3 Seek(Vector3 position)
    {
        var dir = position - transform.position;
        dir.y = 0; 
        return Steer(dir.normalized * maxSpeed);
    }
    
    public Vector3 Arrive(Vector3 targetPosition, float slowingRadius)
    {
        var dir = targetPosition - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;
        
        float targetSpeed = maxSpeed;
        if (dist < slowingRadius)
            targetSpeed = maxSpeed * (dist / slowingRadius);
        
        return Steer(dir.normalized * targetSpeed);
    }

    public Vector3 Steer(Vector3 desired)
    {
        desired.y = 0; 
        var steering = desired - velocity;
        steering.y = 0;
        
        return Vector3.ClampMagnitude(steering, maxForce); 
    }

    public void AddForce(Vector3 force)
    {
        force.y = 0;
        velocity += force * Time.deltaTime; 
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    public void Move()
    {
        Vector3 finalVelocity = velocity;
        finalVelocity.y = rb.linearVelocity.y;
        
        rb.linearVelocity = finalVelocity;
        
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        if(horizontalVel.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVel);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
    
    public void StopMomentum()
    {
        velocity = Vector3.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
        }
    }
}