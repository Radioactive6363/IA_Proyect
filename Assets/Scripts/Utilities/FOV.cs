using UnityEngine;

public class FOV : MonoBehaviour
{
    [SerializeField] private float _angle = 90f;
    [SerializeField] private float _distance = 15f;
    [SerializeField] private LayerMask _obstacleMask;
    
    public float ViewRadius => _distance;
    private Vector3 Origin => transform.position;
    private Vector3 Forward => transform.forward;
    public bool IsInSight(Vector3 targetPos)
    {
        Vector3 dirToTarget = targetPos - Origin;
        if (dirToTarget.sqrMagnitude > _distance * _distance) 
            return false;
        
        if (Vector3.Angle(Forward, dirToTarget) > _angle / 2) 
            return false;
        
        if (Physics.Linecast(Origin + Vector3.up * 0.5f, targetPos + Vector3.up * 0.5f, _obstacleMask)) 
            return false;

        return true;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _distance);

        Vector3 viewAngleA = DirFromAngle(-_angle / 2, false);
        Vector3 viewAngleB = DirFromAngle(_angle / 2, false);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * _distance);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * _distance);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}