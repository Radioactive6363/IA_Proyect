using UnityEngine;

public class PlayerModel : MonoBehaviour, IMove, IDetectable
{
    [SerializeField] private float _movementSmoothness = 0.25f;
    Rigidbody _rb;
    private Vector3 desiredDir;
    private Vector3 velocity;
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
        desiredDir = dir;
    }

    private void FixedUpdate()
    {
        Vector3 targetVel = (desiredDir.x * transform.right + desiredDir.z * transform.forward).normalized * speed;
        targetVel.y = _rb.linearVelocity.y;
        
        velocity = Vector3.Lerp(_rb.linearVelocity, targetVel, _movementSmoothness * Time.fixedDeltaTime);
        _rb.linearVelocity = velocity;
    }
    
    public bool IsDetectable => _isDetectable;
    
}
