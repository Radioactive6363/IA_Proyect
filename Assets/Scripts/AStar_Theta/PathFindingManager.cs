using System.Collections.Generic;
using UnityEngine;

public class PathFindingManager : MonoBehaviour
{
    public static PathFindingManager instance { get; private set; }

    public PFGrid grid;
    public LayerMask obstacleMask;

    void Awake()
    {
        instance = this;
    }

    public PFNode Closest(Vector3 pos)
    {
        PFNode closest = null;
        float minDistance = float.MaxValue;

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
    
    public List<PFNode> GetPath(PFNode start, PFNode end)
    {
        bool IsObjective(PFNode node) => node == end;
        
        float GetHeuristic(PFNode node) => Vector3.Distance(node.transform.position, end.transform.position);
        
        float GetCost(PFNode a, PFNode b) => Vector3.Distance(a.transform.position, b.transform.position) * b.cost;
        
        var path = Pathfinding.ThetaStar(
            start, 
            IsObjective, 
            GetNeighbors,
            GetCost,
            GetHeuristic,
            HasLineOfSight
        );
        
        if (path != null)
        {
            int length = path.Count;
            for (int i = 0; i < length; i++)
                path[i].Color = Color.Lerp(Color.cyan, Color.green, (float)i / length);
        }
        
        return path;
    }
    
    private List<PFNode> GetNeighbors(PFNode node)
    {
        return node.neighbors; 
    }

    private bool HasLineOfSight(PFNode a, PFNode b)
    {
        Vector3 origin = a.transform.position + Vector3.up * 0.5f;
        Vector3 dest = b.transform.position + Vector3.up * 0.5f;
        
        return !Physics.Linecast(origin, dest, obstacleMask);
    }
}