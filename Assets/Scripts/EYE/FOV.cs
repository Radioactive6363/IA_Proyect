using UnityEngine;

public class FOV : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private GameObject _target;
    [SerializeField] private float _angle = 90f;
    [SerializeField] private float _distance = 5f;
    [SerializeField] private LayerMask _obstacleMask;

    public GameObject Target
    {
        get => _target;
        set => _target = value;
    }

    private Vector3 Origin => transform.position;
    private Vector3 Forward => transform.forward;
    
    public bool CheckDetection()
    {
        if (_target == null) return false;

        Vector3 targetPos = _target.transform.position;
        return IsInRange(targetPos) && IsInAngle(targetPos) && IsInSight(targetPos);
    }

    public bool IsInRange(Vector3 target)
    {
        float sqrDistance = (Origin - target).sqrMagnitude;
        return sqrDistance <= _distance * _distance;
    }

    public bool IsInAngle(Vector3 target)
    {
        Vector3 dir = target - Origin;
        return Vector3.Angle(Forward, dir) <= _angle / 2f;
    }

    public bool IsInSight(Vector3 target)
    {
        return !Physics.Linecast(Origin, target, _obstacleMask);
    }
    
     private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawWireSphere(Origin, _distance);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(Origin, Quaternion.Euler(0, _angle / 2, 0) * Forward * _distance);
        Gizmos.DrawRay(Origin, Quaternion.Euler(0, -_angle / 2, 0) * Forward * _distance);

        if (_target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Origin, _target.transform.position);
        }
    }
}
