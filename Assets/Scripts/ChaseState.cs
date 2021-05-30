using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChaseState : IState
{
    private StateMachine _sm;
    private readonly Entity _entity;
    private int _currentNode = 0;
    private bool _targetIsVisible;
    private bool _lastNodeReached;

    public ChaseState(Entity entity, StateMachine sm)
    {
        _sm = sm;
        _entity = entity;
    }
    
    public void OnUpdate()
    {
        FindVisibleTargets();
        
        if (_targetIsVisible)
            ChaseTarget();
        else
            MoveToLastPos();
    }

    private void MoveToLastPos()
    {
        if (_lastNodeReached)
        {
            _entity.Move(Entity.lastTargetPosition);

            Vector3 pointDistance = Entity.lastTargetPosition - _entity.transform.position;

            if (pointDistance.magnitude < _entity.stoppingDistance)
            {
                _entity.target = null; 
                _sm.ChangeState("PatrolState");
            }
        }
        else
        {
            if (_entity.chasePath.Count == 0)
            {
                _entity.startingNode = _entity.GetNerbyNode();
                _entity.goalNode = _entity.GetNerbyTargetNode();

                if (_entity.goalNode == null)
                {
                    _lastNodeReached = true;
                    return;
                }

                _entity.chasePath = _entity.ConstructPath();
                _entity.chasePath.Reverse();

                _currentNode = 0;
            }
            
            _entity.Move(_entity.chasePath[_currentNode].transform.position);

            Vector3 pointDistance = _entity.chasePath[_currentNode].transform.position - _entity.transform.position;

            if (pointDistance.magnitude < _entity.stoppingDistance)
            {
                _currentNode++;
                if (_currentNode > _entity.chasePath.Count - 1)
                    _lastNodeReached = true;
            }
        }
    }
    
    private void ChaseTarget()
    {
        if (_entity.target == null)
            return;
        
        _entity.Move(_entity.target.transform.position);
    }

    public void FindVisibleTargets()
    {
        _targetIsVisible = false;

        Collider[] targetsInViewRadius = Physics.OverlapSphere(_entity.transform.position, _entity.viewRadius, _entity.targetLayer);

        foreach (var item in targetsInViewRadius)
        {
            GameObject target = item.gameObject;

            Vector3 dirToTarget = target.transform.position - _entity.transform.position;

            if (Vector3.Angle(_entity.transform.forward, dirToTarget) < _entity.angleRadius / 2)
            {
                if (!Physics.Raycast(_entity.transform.position, dirToTarget, dirToTarget.magnitude,
                    _entity.obstacleLayer))
                {
                    _targetIsVisible = true;
                    Entity.lastTargetPosition = target.transform.position;
                }
            }
        }
    }
}
