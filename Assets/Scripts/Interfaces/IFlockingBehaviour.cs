using System.Collections.Generic;
using UnityEngine;

public interface IFlockingBehaviour
{
    public Vector3 GetDir(List<Boid> boids);
}
