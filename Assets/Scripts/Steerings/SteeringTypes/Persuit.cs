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

        var dirToPoint = futureTargetPosition - npcTransform.position; // Direcci�n = Posici�nFinal - Posici�nInicial
        var dirToTarget = target.position - npcTransform.position; // Direcci�n = Posici�nFinal - Posici�nInicial
        
        Debug.DrawRay(npcTransform.position, dirToPoint, Color.green);

        var dotRemaped = (Vector3.Dot(dirToPoint, dirToTarget) + 1) / 2;
        dirToPoint = Vector3.Lerp(dirToTarget, dirToPoint, dotRemaped);

        var desiredVelocity = dirToPoint.normalized * maxSpeed; // velocidad deseada = direcci�n normalizada * velocidad m�xima
        Vector3 steering = desiredVelocity - currentVelocity; // correcci�n de velocidad = velocidad deseada - actual


        Debug.DrawRay(npcTransform.position, dirToPoint, Color.green);
        Debug.DrawRay(npcTransform.position, dirToTarget, Color.yellow);
        return currentVelocity += steering * Time.deltaTime; // a la velocidad actual se le suma la correcci�n (aceleraci�n) * tiempo (Time.deltaTime)


    }
}
