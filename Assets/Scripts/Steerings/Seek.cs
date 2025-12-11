using UnityEngine;

public class Seek : ISteering
{
    private Transform npcTransform;
    private Transform target;
    private float maxSpeed;

    public Transform SetTarget { set { target = value; } }

    public Seek(Transform target, Transform npcTransform, float maxSpeed)
    {
        this.target = target;
        this.npcTransform = npcTransform;
        this.maxSpeed = maxSpeed;
    }

    public Vector3 GetSteerDir(Vector3 currentVelocity)
    {
        if (target == null) return Vector3.zero;
        return (target.position - npcTransform.position).normalized * maxSpeed;
    }
}