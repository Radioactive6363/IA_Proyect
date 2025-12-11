using UnityEngine;
using System.Collections.Generic;

public class StatePatrol : State<UnitInputs>
{
    UnitBrain _brain;

    List<PFNode> _path;
    int _currentNode;
    bool _isWaiting;
    float _waitTimer;
    
    public StatePatrol(UnitBrain b) 
    { 
        _brain = b; 
    }

    public override void Enter()
    {
        if (_brain.isLeader)
        {
            SetRandomDestination();
        }
    }

    public override void Execute()
    {
        if (_brain.GetNearestEnemy() != null) 
        {
            _brain.SetState(UnitInputs.EnemySpotted);
            return;
        }
        
        if (_brain.isLeader)
        {
            ExecuteLeaderWander();
        }
        else
        {
            ExecuteFollowerFlocking();
        }
    }
    
    private void ExecuteLeaderWander()
    {
        if (_isWaiting)
        {
            _brain.ApplyMovement(Vector3.zero, false);
            
            if (Time.time > _waitTimer)
            {
                _isWaiting = false;
                SetRandomDestination();
            }
            return;
        }
        
        if (_path == null || _path.Count == 0)
        {
            SetRandomDestination();
            return;
        }
        
        if (_currentNode < _path.Count)
        {
            Vector3 targetNodePos = _path[_currentNode].transform.position;

            Vector3 dir = (targetNodePos - _brain.transform.position).normalized * _brain.MaxSpeed;
            
            _brain.ApplyMovement(dir, false);
            
            if (Vector3.Distance(_brain.transform.position, targetNodePos) < 1.5f)
            {
                _currentNode++;
            }
        }
        else
        {
            _isWaiting = true;
            _waitTimer = Time.time + Random.Range(1f, 3f);
        }
    }
    
    private void ExecuteFollowerFlocking()
    {
        if (_brain.myLeader == null) return;
        
        float stopDistance = 3f;
        
        var arrive = new Arrive(_brain.myLeader.transform, _brain.transform, _brain.MaxSpeed, 5f);
        Vector3 followForce = arrive.GetSteerDir(_brain.Velocity);

        if (Vector3.Distance(_brain.transform.position, _brain.myLeader.transform.position) < stopDistance)
            followForce = Vector3.zero; 

        _brain.ApplyMovement(followForce, true);
    }

    private void SetRandomDestination()
    {
        if(PathFindingManager.instance == null) return;
        
        PFNode randomNode = GetRandomValidNode();
        
        if (randomNode != null)
        {
            var start = PathFindingManager.instance.Closest(_brain.transform.position);
            
            _path = PathFindingManager.instance.GetPath(start, randomNode);
            _currentNode = 0;
        }
    }

    private PFNode GetRandomValidNode()
    {
        var gridNodes = PathFindingManager.instance.grid.Nodes;
        
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, gridNodes.Length);
            PFNode candidate = gridNodes[randomIndex];
            
            if (candidate != null && !candidate.isBlocked)
            {
                if (Vector3.Distance(_brain.transform.position, candidate.transform.position) > 5f)
                {
                    return candidate;
                }
            }
        }
        return null;
    }
}