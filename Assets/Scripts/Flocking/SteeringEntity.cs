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

    protected virtual void Start()
    {
        
    }
    
    public Vector3 Seek(Vector3 position)
    {
        var dir = position - transform.position;
        return Steer(dir.normalized * maxSpeed);
    }
    
    public Vector3 Arrive(Vector3 targetPosition, float slowingRadius)
    {
        var dir = targetPosition - transform.position;
        float dist = dir.magnitude;
        
        float targetSpeed = maxSpeed;
        if (dist < slowingRadius)
        {
            targetSpeed = maxSpeed * (dist / slowingRadius);
        }
        
        return Steer(dir.normalized * targetSpeed);
    }

    public Vector3 Steer(Vector3 desired)
    {
        var steering = desired - velocity;
        return Vector3.ClampMagnitude(steering, maxForce); 
    }

    public void AddForce(Vector3 force)
    {
        velocity += force * Time.deltaTime; 
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }

    public void Move()
    {
        if(velocity == Vector3.zero) return;
        
        transform.position += velocity * Time.deltaTime;
        
        if(velocity.sqrMagnitude > 0.1f)
        {
            transform.forward = Vector3.Slerp(transform.forward, velocity.normalized, turnSpeed * Time.deltaTime);
        }
    }
}