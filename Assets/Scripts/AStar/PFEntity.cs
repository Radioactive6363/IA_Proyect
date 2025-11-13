using System.Collections.Generic;
using UnityEngine;

public class PFEntity : MonoBehaviour
{
    public List<PFNode> path = new();

    private Vector3 velocity;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            var closest = PathFindingManager.instance.Closest(transform.position);
            path = PathFindingManager.instance.GetPath(closest);
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
