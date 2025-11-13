using System.Collections.Generic;
using UnityEngine;

public class Boid : SteeringEntity
{
    private List<IFlockingBehabiour> flockingBehabiours = new List<IFlockingBehabiour>();
    public Vector3 Velocity => velocity;
    private List<Boid> AllBoids => FlockingManager.Instance.AllBoids;
    private FlockingManager FM => FlockingManager.Instance;
    
    protected virtual void Start()
    {
        FlockingManager.Instance.AddBoid(this);
        AddForce(new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)).normalized * maxSpeed);
        flockingBehabiours.AddRange(GetComponents<IFlockingBehabiour>());
    }
    
    protected virtual void Update()
    {
        if (BoidsInRange())
            Flocking();
        
        Move();
    }

    private bool BoidsInRange()
    {
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            var dir = transform.position - boid.transform.position;
            if (dir.sqrMagnitude > FM.cohesionRadius * FM.cohesionRadius) continue;
            return true;
        }
        return false;
    }

    private void Flocking()
    {
        AddForce(Separation().NoY() * FM.separationWeight +
            Cohesion().NoY() * FM.cohesionWeight +
            Alignment().NoY() * FM.alignmentWeight);
    }
    
    public Vector3 GetFlockingForce()
    {
        if (!BoidsInRange()) return Vector3.zero;

        return Separation().NoY() * FM.separationWeight +
               Cohesion().NoY() * FM.cohesionWeight +
               Alignment().NoY() * FM.alignmentWeight;
    }

    private Vector3 Separation()
    {
        Vector3 totalDir = Vector3.zero;
        int separationCount = 0; 
        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
        
            var dir = transform.position - boid.transform.position;
            var magnitude = dir.magnitude; 
        
            if (magnitude > FM.separationRadius) continue;
            totalDir += dir.normalized / (magnitude + 0.1f);
        
            separationCount++;
        }
        if (separationCount == 0) return Vector3.zero;
        totalDir /= separationCount;
        return Steer(totalDir.normalized * maxSpeed);
    }

    private Vector3 Cohesion()
    {
        Vector3 avgPos = Vector3.zero;
        int count = 0;

        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            var dir = transform.position - boid.transform.position;
            if (dir.sqrMagnitude > FM.cohesionRadius * FM.cohesionRadius) continue;

            avgPos += boid.transform.position;
            count++;
        }
        if (count == 0) return Vector3.zero;
        avgPos /= count;
        return Seek(avgPos);
    }

    private Vector3 Alignment()
    {
        Vector3 avgVelocity = Vector3.zero;
        int count = 0;

        for (int i = 0; i < AllBoids.Count; i++)
        {
            var boid = AllBoids[i];
            if (boid == this) continue;
            var dir = transform.position - boid.transform.position;
            if (dir.sqrMagnitude > FM.cohesionRadius * FM.cohesionRadius) continue;

            avgVelocity += boid.Velocity;
            count++;
        }
        if (count == 0) return Vector3.zero;
        avgVelocity /= count;
        return Steer(avgVelocity.normalized * maxSpeed);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, FM.cohesionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, FM.separationRadius);
    }
}
