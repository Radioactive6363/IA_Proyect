using System.Collections.Generic;
using UnityEngine;

public interface IFlockingBehabiour
{
    public Vector3 GetDir(List<Boid> boids);
}
