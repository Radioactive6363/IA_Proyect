using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMove 
{
    void Move(Vector3 dir);
    void LookAtDir(Vector3 dir);
}
