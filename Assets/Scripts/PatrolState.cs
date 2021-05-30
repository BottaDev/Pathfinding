using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows.Speech;

public class PatrolState : IState
{
    private StateMachine _sm;
    private readonly Entity _entity;
    private List<Node> _returnPath = new List<Node>();
    private int _currentNode = 0;

    public PatrolState(Entity entity, StateMachine sm)
    {
        _sm = sm;
        _entity = entity;
    }
    
    public void OnUpdate()
    {
        if (_entity.target != null)
        {
            _sm.ChangeState("ChaseState");
            return;
        }
        
        FindVisibleTargets();
        FindVisibleNodes();
        
        if (_entity.visibleNodes.Contains(_entity.wayPoints[_entity.currentWayPoint]))
            PatrolNodes();
        else
            MoveToNodes();
        
        if (_returnPath.Count == 0)
            _entity.Move(_entity.wayPoints[_entity.currentWayPoint].transform.position);
        else
            _entity.Move(_returnPath[_currentNode].transform.position);
    }

    private void PatrolNodes()
    {
        _returnPath.Clear();
        
        _entity.Move(_entity.wayPoints[_entity.currentWayPoint].transform.position);
        
        Vector3 pointDistance = _entity.wayPoints[_entity.currentWayPoint].transform.position - _entity.transform.position;
    
        if (pointDistance.magnitude < _entity.stoppingDistance)
        {
            _entity.currentWayPoint++;
            if (_entity.currentWayPoint > _entity.wayPoints.Count - 1)
                _entity.currentWayPoint = 0;
        }
    }

    /// <summary>
    /// Returns to the normal patrol path
    /// </summary>
    private void MoveToNodes()
    {
        if (_returnPath.Count == 0)
        {
            _entity.startingNode = _entity.GetNerbyNode();
            _entity.goalNode = _entity.wayPoints[0].gameObject.GetComponent<Node>();
            
            _returnPath = _entity.ConstructPath();
            _returnPath.Reverse();

            _currentNode = 0;
            _entity.currentWayPoint = 0;
        }
        
        _entity.Move(_returnPath[_currentNode].transform.position);
        
        Vector3 pointDistance = _returnPath[_currentNode].transform.position - _entity.transform.position;
    
        if (pointDistance.magnitude < _entity.stoppingDistance)
            _currentNode++;
    }

    public void FindVisibleTargets()
    {
        _entity.target = null;

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
                    _entity.target = target;
                    _entity.AlertEntities();
                }
            }
        }
    }
    
    public void FindVisibleNodes()
    {
        _entity.visibleNodes.Clear();

        Collider[] nodesInViewRadius = Physics.OverlapSphere(_entity.transform.position, _entity.viewRadius, _entity.nodeLayer);

        foreach (var item in nodesInViewRadius)
        {
            GameObject node = item.gameObject;

            Vector3 dirToTarget = node.transform.position - _entity.transform.position;

            if (Vector3.Angle(_entity.transform.forward, dirToTarget) < _entity.angleRadius / 2)
            {
                if (!Physics.Raycast(_entity.transform.position, dirToTarget, dirToTarget.magnitude,
                    _entity.obstacleLayer))
                    _entity.visibleNodes.Add(node.transform);
            }
        }
    }
}
