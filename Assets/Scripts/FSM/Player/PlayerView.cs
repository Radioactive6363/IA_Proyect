using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]
    Animator _anim;
    Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
}
