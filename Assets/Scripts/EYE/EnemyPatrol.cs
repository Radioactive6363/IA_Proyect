using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float movespeed = 3f;
    [SerializeField] private float reachdistance = 0.5f;
    [SerializeField] private float waitTimeAtWaypoint = 2f;

    [Header("Huida")]
    [SerializeField] private float fleedistance = 5f;

    private int currentWP = 0;
    private Rigidbody rb;
    private FOV fov;
    private Transform target;
    private bool waiting= false;
    private float waitTimer = 0f;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fov= GetComponent<FOV>();
        if(fov.Target!=null)
        {
            target = fov.Target.transform;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("no hay puntos asigandos");

        }
    }

    private void FixedUpdate()
    {


        if(fov != null&& fov.CheckDetection() && target!=null) {

            FleeFromTarget();

        }
        else
        {
            Patrol();

        }
       
    }

    private void FleeFromTarget()
    {
        if (target == null) return;

        //Direccion para huir
        Vector3 fleeDir = rb.position - target.position;
        fleeDir.y = 0f;
        //Movieminto hacie atras
        fleeDir.Normalize();
        rb.MovePosition(rb.position + fleeDir * movespeed * Time.fixedDeltaTime);

        //rotacion para mirar al jugador
        Vector3 lookDir = target.position - rb.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, 5f * Time.fixedDeltaTime));
        }
    }





    private void MoveTowardsWayPoint()
    {
        Transform targetWP = waypoints[currentWP];
        Vector3 dir = targetWP.position - transform.position;
        Vector3 dirY = new Vector3(dir.x, 0f, dir.z);

        // Verificar si llegó al waypoint
        if (dirY.magnitude < reachdistance)
        {
            waiting = true;
            waitTimer = 0f;
            rb.MovePosition(rb.position); // detenerse completamente
            return;
        }

        // Movimiento
        Vector3 move = dirY.normalized * movespeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotación suave solo en eje Y
        if (dirY != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirY.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }
    }
    private void WaitAtWayPoint()
    {

        waitTimer += Time.fixedDeltaTime;

        Transform nextWP= waypoints[(currentWP+1)  % waypoints.Length];
        Vector3 dirY = new Vector3(nextWP.position.x-transform.position.y,0f, nextWP.position.z-transform.position.z).normalized;
        if(dirY != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirY);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

        rb.MovePosition(rb.position);

        if(waitTimer>= waitTimeAtWaypoint)
        {
            waiting = false;
            currentWP=(currentWP+1)%waypoints.Length;
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
        
        if (dirY.magnitude < reachdistance)
        {
            currentWP = (currentWP + 1) % waypoints.Length;
            targetWP = waypoints[currentWP];
            dir = targetWP.position - transform.position;
            dirY = new Vector3(dir.x, 0f, dir.z);
        }
        
        Vector3 move = dirY.normalized * movespeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
        
        if (dirY != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirY.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

    }
}