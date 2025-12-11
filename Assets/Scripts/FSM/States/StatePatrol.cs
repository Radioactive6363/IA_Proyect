using UnityEngine;
using System.Collections.Generic;

public class StatePatrol : State<UnitInputs>
{
    UnitBrain _brain;
    
    // LeaderData
    List<PFNode> _path;
    int _currentNode;
    
    // SoldierData
    Arrive _arriveSteering;

    public StatePatrol(UnitBrain b) 
    { 
        _brain = b; 
    }

    public override void Enter()
    {
        if (_brain.isLeader && _brain.patrolWaypoints.Length > 0)
        {
            CalculatePathToWaypoint();
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
            ExecuteLeaderPatrol();
        }
        else
        {
            ExecuteFollowerFlocking();
        }
    }
    
    private void ExecuteLeaderPatrol()
    {
        if (_brain.patrolWaypoints.Length == 0) return; 
        
        Transform currentWP = _brain.patrolWaypoints[_brain.currentWaypointIndex];
        
        float distToWP = Vector3.Distance(_brain.transform.position, currentWP.position);
        
        if (distToWP < 1.5f)
        {
            _brain.currentWaypointIndex = (_brain.currentWaypointIndex + 1) % _brain.patrolWaypoints.Length;
            CalculatePathToWaypoint();
            return;
        }
        
        if (_path != null && _currentNode < _path.Count)
        {
            Vector3 targetNodePos = _path[_currentNode].transform.position;
            
            var seek = new Seek(null, _brain.transform, _brain.MaxSpeed);
            Vector3 dir = (targetNodePos - _brain.transform.position).normalized * _brain.MaxSpeed;
            
            _brain.ApplyMovement(dir, false);
            
            if (Vector3.Distance(_brain.transform.position, targetNodePos) < 1f)
            {
                _currentNode++;
            }
        }
        else
        {
            var arrive = new Arrive(currentWP, _brain.transform, _brain.MaxSpeed, 2f);
            _brain.ApplyMovement(arrive.GetSteerDir(_brain.Velocity), false);
        }
    }
    
    private void ExecuteFollowerFlocking()
    {
        if (_brain.myLeader == null) return;
        
        float stopDistance = 3f;
        
        var arrive = new Arrive(_brain.myLeader.transform, _brain.transform, _brain.MaxSpeed, 5f);
        Vector3 followForce = arrive.GetSteerDir(_brain.Velocity);
        
        if (Vector3.Distance(_brain.transform.position, _brain.myLeader.transform.position) < stopDistance)
        {
            followForce = Vector3.zero; 
        }
        
        _brain.ApplyMovement(followForce, true);
    }

    private void CalculatePathToWaypoint()
    {
        if(PathFindingManager.instance == null) return;

        var start = PathFindingManager.instance.Closest(_brain.transform.position);
        var end = PathFindingManager.instance.Closest(_brain.patrolWaypoints[_brain.currentWaypointIndex].position);
        
        _path = PathFindingManager.instance.GetPath(start, end);
        _currentNode = 0;
    }
}