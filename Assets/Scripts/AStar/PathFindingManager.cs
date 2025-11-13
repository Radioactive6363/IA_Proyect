using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class PathFindingManager : MonoBehaviour
{
    public static PathFindingManager instance { get; private set; }

    public PFGrid grid;
    public PFNode goal;
    public LayerMask obstacleMask;

    void Awake()
    {
        instance = this;
    }

    public PFNode Closest(Vector3 pos)
    {
        PFNode closest = null;
        float minDistance = int.MaxValue;

        var length = grid.Nodes.Length;
        var nodes = grid.Nodes;
        for (int i = 0; i < length; i++)
        {
            PFNode current = nodes[i];
            var dist = Vector3.SqrMagnitude(pos - current.transform.position);
            if (dist < minDistance)
            {
                closest = current;
                minDistance = dist;
            }
        }
        return closest;
    }

    public List<PFNode> GetPath(PFNode start)
    {
        var path = Pathfinding.AstarPS(start, Objective, GetNeighborsInSight, GetSimpleCost, GetMangattanHeuristicWithSomeDistance, HasLineOfSight);

        int length = path.Count;
        for (int i = 0; i < length; i++)
        {
            path[i].Color = Color.Lerp(Color.cyan, Color.green, (float)i / length);
        }
        return path;
    }

    private bool Objective(PFNode node)
    {
        return node == goal;
    }
    private List<PFNode> GetNeighborsInSight(PFNode node)
    {
        var inSight = new List<PFNode>();
        for (int i = 0; i < node.neighbors.Count; i++)
        {
            if (!Physics.Linecast(node.transform.position, node.neighbors[i].transform.position, obstacleMask))
                inSight.Add(node.neighbors[i]);
        }
        return inSight;
    }
    private float GetSimpleCost(PFNode from, PFNode to)
    {
        return to.cost;
    }
    private float GetMangattanHeuristicWithSomeDistance(PFNode node)
    {
        return Mathf.Abs(node.x - goal.x) + Mathf.Abs(node.y - goal.y) +
        Vector3.SqrMagnitude(node.transform.position - goal.transform.position) / 1000;
    }
    private bool HasLineOfSight(PFNode a, PFNode b)
    {
        return !Physics.Linecast(a.transform.position, b.transform.position, obstacleMask);
    }
}
