using System.Collections.Generic;
using UnityEngine;

public class PFEntity : MonoBehaviour
{
    public List<PFNode> path = new();

    private Vector3 velocity;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            var startNode = PathFindingManager.instance.Closest(transform.position);
            var endNode = PathFindingManager.instance.Closest(Vector3.zero); 
            
            path = PathFindingManager.instance.GetPath(startNode, endNode); 
        }

        if (path != null && path.Count > 0)
        {
            velocity = path[0].transform.position- transform.position;
            transform.position += velocity.normalized * 10 * Time.deltaTime;
            if (velocity.magnitude < 0.5f)
            {
                path.RemoveAt(0);
            }
        }
    }
}
