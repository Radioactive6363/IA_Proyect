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
        var path = Pathfinding.ThetaStar(start, Objective, GetNeighborsInSight, GetDistanceCost, GetDistanceHeuristic, HasLineOfSight);

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

    private List<PFNode> GetNeighbors(PFNode node)
    {
        return node.neighbors;
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
    private List<PFNode> GetNeighborsNotBlocked(PFNode node)
    {
        var inSight = node.neighbors;
        for (int i = 0; i < inSight.Count; i++)
        {
            if (inSight[i].isBlocked)
                inSight.Remove(inSight[i]);
        }
        return inSight;
    }
    private float GetSimpleCost(PFNode from, PFNode to)
    {
        return to.cost;
    }
    private float GetDistanceCost(PFNode from, PFNode to)
    {
        return Vector3.Distance(from.transform.position, to.transform.position);
    }
    private float GetManhattanHeuristic(PFNode node)
    {
        return Mathf.Abs(node.x - goal.x) + Mathf.Abs(node.y - goal.y);
    }
    private float GetMangattanHeuristicWithSomeDistance(PFNode node)
    {
        return Mathf.Abs(node.x - goal.x) + Mathf.Abs(node.y - goal.y) +
        Vector3.SqrMagnitude(node.transform.position - goal.transform.position) / 1000;
    }

    private float GetDistanceHeuristic(PFNode node)
    {
        return Vector3.Distance(node.transform.position, goal.transform.position);
    }

    private bool HasLineOfSight(PFNode a, PFNode b)
    {
        return !Physics.Linecast(a.transform.position, b.transform.position, obstacleMask);
    }

    public bool ManhattanLineOfSight(PFNode from, PFNode to, PFGrid grid)
    {
        int x0 = from.x;
        int y0 = from.y;
        int x1 = to.x;
        int y1 = to.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            PFNode node = grid.GetNodeAt(x0, y0);
            if (node != null && node.isBlocked)
                return false;

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return true;
    }
}
