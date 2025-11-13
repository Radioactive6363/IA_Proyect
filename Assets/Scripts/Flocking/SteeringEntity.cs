using System;
using UnityEngine;

public class SteeringEntity : MonoBehaviour
{
    [SerializeField] protected float maxSpeed;
    [SerializeField] protected float maxForce;
    protected Vector3 velocity;

    public Vector3 Seek(Vector3 position)
    {
        var dir = position - transform.position;
        return Steer(dir.normalized * maxSpeed);
    }
    public Vector3 Steer(Vector3 desired)
    {
        var steering = desired - velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }
    public void AddForce(Vector3 force)
    {
        velocity = velocity + force;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }
    public void Move()
    {
        if(velocity == Vector3.zero) return;
        transform.forward = velocity;
        transform.position += velocity * Time.deltaTime;
    }
   
}
