using Unity.Mathematics;
using UnityEngine;

public class LineOfView : MonoBehaviour
{
    //FOV
    [SerializeField] private Transform _Target;
    [SerializeField] private float _angle;
    [SerializeField] private float _Distance;
    [SerializeField] LayerMask _layerMask;






    public bool IsInRange(Transform target)
    {
       
      var sqrdistance =(transform.position - target.position).sqrMagnitude;

        return sqrdistance < _Distance * _Distance;
    }



    public bool IsInAngle(Transform target)
    {
        var dir = (target.position - transform.position).normalized;
         return Vector3.Angle(transform.forward, dir) <= _angle/2;


    }
    //verifica la linea sin obsatculos
    public bool HasLineOfSight(Transform target)
    {
        var dir=(target.position - transform.position).normalized;
        if(Physics.Raycast(transform.position,dir,out RaycastHit Hit, _Distance, _layerMask))
        {
            return Hit.transform == target;
        }
        return false;
    }
   
    //chequea si esta dentro del campo de vision
    public bool CanSeeTarget(Transform target)
    {
        return IsInRange(target) || IsInAngle(target) || HasLineOfSight(target);
    }

    //debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position,_Distance);


        Vector3 leftLimit = Quaternion.Euler(0, -_angle / 2f, 0) * transform.forward;
        Vector3 rightlimit = Quaternion.Euler(0, _angle / 2f,0) * transform.forward;


        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * _Distance);
        Gizmos.DrawLine(transform.position, transform.position + rightlimit * _Distance);


        if(_Target !=null && CanSeeTarget(_Target))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position,_Target.position);

        }
    }
}
