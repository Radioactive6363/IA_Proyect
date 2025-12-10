using UnityEngine;

public class Evade : ISteering
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

    public Evade(Transform target, Transform npcTransform, float maxSpeed)
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

        var dirFromPoint = npcTransform.position - futureTargetPosition; // Direcci�n = Posici�nInicial - Posici�nFinal 
        var dirFromTarget = npcTransform.position - target.position; // Direcci�n = Posici�nInicial - Posici�nFinal

        //if(Vector3.Dot(dirToPoint, dirToTarget) < 0)
        //{
        //    dirToPoint = dirToTarget;
        //}
        Debug.DrawRay(npcTransform.position, dirFromPoint, Color.blue);

        var dotRemaped = (Vector3.Dot(dirFromPoint, dirFromTarget) + 1) / 2;
        dirFromPoint = Vector3.Lerp(dirFromPoint, dirFromTarget, dotRemaped);

        var desiredVelocity = dirFromPoint.normalized * maxSpeed; // velocidad deseada = direcci�n normalizada * velocidad m�xima
        Vector3 steering = desiredVelocity - currentVelocity; // correcci�n de velocidad = velocidad deseada - actual


        Debug.DrawRay(npcTransform.position, dirFromPoint, Color.magenta);
        Debug.DrawRay(npcTransform.position, dirFromTarget, Color.yellow);
        return currentVelocity += steering * Time.deltaTime; // a la velocidad actual se le suma la correcci�n (aceleraci�n) * tiempo (Time.deltaTime)


    }
}
