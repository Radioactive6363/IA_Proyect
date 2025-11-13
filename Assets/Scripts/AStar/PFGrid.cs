using UnityEngine;

public class PFGrid : MonoBehaviour
{
    [SerializeField] PFNode prefab;
    [SerializeField] PFNode[] nodes;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float distance;
    [SerializeField] int baseCost;

    public PFNode[] Nodes => nodes;

    private void X() =>  print("adfasdf");
    
    [ContextMenu("Instantiate Nodes")]
    public void InstantiateGrid()
    {
        nodes = new PFNode[width * height];
        int count = 0;
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                PFNode n = Instantiate(prefab,
                    transform.position +
                    new Vector3(i * distance, 0, j * distance), transform.rotation, transform);
                nodes[count] = n;
                n.Initialize(i, j);

                count++;
            }
        }
        count = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            PFNode n = nodes[i];
            if (n.x > 0)          n.neighbors.Add(nodes[n.x - 1 + n.y * width]); 
            if (n.x < width - 1)  n.neighbors.Add(nodes[n.x + 1 + n.y * width]);
            if (n.y > 0)          n.neighbors.Add(nodes[n.x + (n.y - 1) * width]);
            if (n.y < height - 1) n.neighbors.Add(nodes[n.x + (n.y + 1) * width]);
        
        }
    }
    public PFNode GetNodeAt(int x, int y)
    {
        return nodes[x + y * height];
    }

    [ContextMenu("Delete Nodes")]
    public void DeleteNodes()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            DestroyImmediate(nodes[i].gameObject);
        }
        nodes = null;
    }

    [ContextMenu("Set Cost To All Nodes")]
    public void SetCostAll()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].cost = baseCost;
        }
    }
}
