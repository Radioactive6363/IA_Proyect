using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


[RequireComponent(typeof(Rigidbody),typeof(FOV))]
public class EnemyPatrol : MonoBehaviour
{

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float movespeed = 3f;
    [SerializeField] private float reachdistance = 0.5f;
    [SerializeField] private float waitTimeAtWaypoint = 2f;


    [Header("HUIDA")]
    [SerializeField] private float fleedistance = 5f;

    private int currentWP = 0;
    private Rigidbody rb;
    private FOV fov;
    private Transform Target;
    private bool waiting= false;
    private float waitTimer = 0f;
   



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fov = GetComponent<FOV>();

        if (fov.Target != null)
        {
            Target = fov.Target.transform;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("no hay puntos asigandos");

        }
    }

    private void FixedUpdate()
    {

        if(fov!= null&& fov.CheckDetection() && Target!=null)
        {
            FleeFromTarget();
        }
        else
        {
            Patrol();
        }
       
    }

    private void FleeFromTarget()
    {
        if (Target == null) return;

        // Dirección para huir
        Vector3 fleeDir = rb.position - Target.position;
        fleeDir.y = 0f;

        // Movimiento hacia atrás
        fleeDir.Normalize();
        rb.MovePosition(rb.position + fleeDir * movespeed * Time.fixedDeltaTime);

        // Rotación para mirar al jugador
        Vector3 lookDir = Target.position - rb.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, 5f * Time.fixedDeltaTime));
        }
    }



 
    private void Patrol()
    {

        if (waiting)
        {
            waitTimer += Time.fixedDeltaTime;
            if(waitTimer >= waitTimeAtWaypoint)
            {
                waiting = false;
                waitTimer = 0f;
                currentWP=(currentWP + 1) % waypoints.Length;
            }
        }
        else
        {
            Transform nextWp = waypoints[(currentWP + 1) % waypoints.Length];
            Vector3 nextDir = new Vector3((nextWp.position - transform.position).x, 0f, (nextWp.position - transform.position).z).normalized;
        }

        if (waypoints.Length == 0) return;

        Transform targetWP = waypoints[currentWP];
        Vector3 dir = targetWP.position - transform.position;
        Vector3 dirY = new Vector3(dir.x, 0f, dir.z);

        // Verificar si llegó al waypoint
        if (dirY.magnitude < reachdistance)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            targetWP = waypoints[currentWP];
            dir = targetWP.position - transform.position;
            dirY = new Vector3(dir.x, 0f, dir.z);
        }

        // Movimiento usando MovePosition
        Vector3 move = dirY.normalized * movespeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotación suave en eje Y usando MoveRotation
        if (dirY != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirY.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

    }
}