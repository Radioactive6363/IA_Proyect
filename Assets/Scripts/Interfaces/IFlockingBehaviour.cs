using System.Collections.Generic;
using UnityEngine;

public interface IFlockingBehaviour
{
    Vector3 GetDir(List<UnitBrain> neighbors, UnitBrain owner);
}