using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public static FlockingManager Instance { get; private set; }

    [Header("Radius")]
    [SerializeField] public float separationRadius;
    [SerializeField] public float cohesionRadius;
    [Header("Weights")]
    [SerializeField, Range(0, 3f)] public float separationWeight;
    [SerializeField, Range(0, 1f)] public float cohesionWeight;
    [SerializeField, Range(0, 1f)] public float alignmentWeight;

    private List<Boid> boids = new();

    public List<Boid> AllBoids => boids;
    void Awake()
    {
        Instance = this;
    }

    public void AddBoid(Boid boid)
    { 
        boids.Add(boid); 
    }
}
