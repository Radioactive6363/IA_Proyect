using UnityEngine;

public class PFGrid : MonoBehaviour
{
    [SerializeField] PFNode prefab;
    [SerializeField] PFNode[] nodes;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float distance;
    [SerializeField] int baseCost;
    [SerializeField] LayerMask obstacleMask;
    
    [Header("Agent Settings")]
    [Tooltip("Collider of Unit + SecurityMargin")]
    [SerializeField] float agentRadius = 0.6f; 

    public PFNode[] Nodes => nodes;
    
    [ContextMenu("Instantiate Nodes")]
    public void InstantiateGrid()
    {
        if (nodes != null) DeleteNodes();

        nodes = new PFNode[width * height];
        int count = 0;
        
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                PFNode n = Instantiate(prefab,
                    transform.position +
                    new Vector3(i * distance, 0, j * distance), transform.rotation, transform);
                
                n.Initialize(i, j);
                n.CheckBlocking(agentRadius, obstacleMask); 
                
                nodes[count] = n;
                count++;
            }
        }
        
        count = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            PFNode n = nodes[i];
            if (n.isBlocked) continue; 
            
            TryAddNeighbor(n, n.x - 1, n.y);
            TryAddNeighbor(n, n.x + 1, n.y);
            TryAddNeighbor(n, n.x, n.y - 1);
            TryAddNeighbor(n, n.x, n.y + 1);
            
            TryAddNeighbor(n, n.x - 1, n.y - 1);
            TryAddNeighbor(n, n.x + 1, n.y + 1);
            TryAddNeighbor(n, n.x - 1, n.y + 1);
            TryAddNeighbor(n, n.x + 1, n.y - 1);
        }
    }
    
    void TryAddNeighbor(PFNode current, int x, int y)
    {
        PFNode neighbor = GetNodeAt(x, y);
        if (neighbor != null && !neighbor.isBlocked)
        {
            current.neighbors.Add(neighbor);
        }
    }

    public PFNode GetNodeAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return nodes[x + y * width];
    }

    [ContextMenu("Delete Nodes")]
    public void DeleteNodes()
    {
        if (nodes == null) return;
        for (int i = 0; i < nodes.Length; i++)
        {
            if(nodes[i] != null) DestroyImmediate(nodes[i].gameObject);
        }
        nodes = null;
    }

    [ContextMenu("Set Cost To All Nodes")]
    public void SetCostAll()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if(nodes[i] != null) nodes[i].cost = baseCost;
        }
    }
}