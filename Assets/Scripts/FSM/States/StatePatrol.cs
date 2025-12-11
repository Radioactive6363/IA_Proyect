using UnityEngine;
using System.Collections.Generic;

public class StatePatrol : State<UnitInputs>
{
    UnitBrain _brain;
    List<PFNode> _path;
    int _currentNode;

    public StatePatrol(UnitBrain b) { _brain = b; }

    public override void Execute()
    {
        if (_brain.GetNearestEnemy() != null) {
            _brain.SetState(UnitInputs.EnemySpotted);
            return;
        }

        Vector3 targetPos = _brain.transform.position;
        if (_brain.myLeader != null) targetPos = _brain.myLeader.transform.position;

        float dist = Vector3.Distance(_brain.transform.position, targetPos);
        
        if (dist > 20f) 
        {
            if (_path == null || _path.Count == 0)
            {
                var start = PathFindingManager.instance.Closest(_brain.transform.position);
                var end = PathFindingManager.instance.Closest(targetPos);
                _path = PathFindingManager.instance.GetPath(start, end);
                _currentNode = 0;
            }
            
            if (_path != null && _currentNode < _path.Count)
            {
                var seek = new Seek(_path[_currentNode].transform, _brain.transform, _brain.MaxSpeed);
                
                _brain.ApplyMovement(seek.GetSteerDir(_brain.Velocity), false); 
                
                if (Vector3.Distance(_brain.transform.position, _path[_currentNode].transform.position) < 1f)
                    _currentNode++;
            }
        }
        else
        {
             var arrive = new Arrive(targetPos == _brain.transform.position ? _brain.transform : _brain.myLeader.transform, 
                                     _brain.transform, _brain.MaxSpeed, 5f);
             
             _brain.ApplyMovement(arrive.GetSteerDir(_brain.Velocity), true);
        }
    }
}