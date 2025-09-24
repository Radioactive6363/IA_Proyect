using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerModel : MonoBehaviour, IMove, IDetectable
{
    Rigidbody _rb;
    public Transform[] _detectablePositions;
    public float speed;
    public bool _isDetectable = true;
    
    Action _onSpin = delegate { };
    public Transform[] DetectablePositions => _detectablePositions;
    public Transform Transform => transform;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    public void Move(Vector3 dir)
    {
        dir = dir.normalized;
        dir *= speed;
        dir.y = _rb.linearVelocity.y;
        _rb.linearVelocity = dir;
    }
    
    public void LookAtDir(Vector3 dir)
    {
        transform.forward = dir;
    }
    
    public void Spin()
    {
        _isDetectable = !_isDetectable;
        _onSpin();
    }
    
    public bool IsDetectable => _isDetectable;
    
}
