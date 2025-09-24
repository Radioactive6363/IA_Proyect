using UnityEngine;

public class PlayerModel : MonoBehaviour, IMove, IDetectable
{
    Rigidbody _rb;
    public Transform[] _detectablePositions;
    public float speed;
    public bool _isDetectable = true;
    
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
    
    public bool IsDetectable => _isDetectable;
    
}
