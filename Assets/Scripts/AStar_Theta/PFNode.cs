using System.Collections.Generic;
using UnityEngine;

public class PFNode : MonoBehaviour
{
    public List<PFNode> neighbors = new List<PFNode>();
    public int x;
    public int y;
    public int cost = 1;
    public bool isBlocked = false;

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    public void CheckBlocking(float radius, LayerMask obstacleMask)
    {
        if (Physics.CheckSphere(transform.position, radius, obstacleMask))
        {
            isBlocked = true;
            if(TryGetComponent<Renderer>(out var r)) 
                r.enabled = false;
        }
    }

    public Color Color
    {
        set
        {
            if(TryGetComponent<Renderer>(out var r))
                r.material.color = value;
        }
    }
}