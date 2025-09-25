using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float movespeed = 3f;
    [SerializeField] private float reachdistance = 0.5f;

    private int currentWP = 0;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("no hay puntos asigandos");

        }
    }

    private void FixedUpdate()
    {
        Patrol();
    }


    private void Patrol()
    {
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