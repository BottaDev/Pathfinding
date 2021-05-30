using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private StateMachine _sm;
    private readonly Enemy _enemy;
    private List<Node> _returnPath = new List<Node>();
    private int _currentReturnNode = 0;
    private List<Transform> _visibleNodes = new List<Transform>();

    public PatrolState(Enemy enemy, StateMachine sm)
    {
        _sm = sm;
        _enemy = enemy;
    }
    
    public void OnUpdate()
    {   
        if (_enemy.target != null)
        {
            _sm.ChangeState("ChaseState");
            return;
        }

        _enemy.target = _enemy.ApplyFOV(_enemy.targetLayer);

        FindVisibleNodes();
        
        if (_visibleNodes.Contains(_enemy.wayPoints[_enemy.currentWayPoint]))
            PatrolNodes();
        else
            MoveToNodes();
        
        if (_returnPath.Count == 0)
            _enemy.Move(_enemy.wayPoints[_enemy.currentWayPoint].transform.position);
        else
            _enemy.Move(_returnPath[_currentReturnNode].transform.position);
    }
    
    /// <summary>
    /// Patrols the waypoints of the enemy
    /// </summary>
    private void PatrolNodes()
    {
        _returnPath.Clear();
        
        _enemy.Move(_enemy.wayPoints[_enemy.currentWayPoint].transform.position);
        
        Vector3 pointDistance = _enemy.wayPoints[_enemy.currentWayPoint].transform.position - _enemy.transform.position;
    
        if (pointDistance.magnitude < _enemy.StoppingDistance)
        {
            _enemy.currentWayPoint++;
            if (_enemy.currentWayPoint > _enemy.wayPoints.Count - 1)
                _enemy.currentWayPoint = 0;
        }
    }

    /// <summary>
    /// Returns to the normal patrol path
    /// </summary>
    private void MoveToNodes()
    {
        if (_returnPath.Count == 0)
        {
            _returnPath = _enemy.ConstructPath(_enemy.GetNerbyNode(), _enemy.wayPoints[0].gameObject.GetComponent<Node>());
            _returnPath.Reverse();

            _currentReturnNode = 0;
            _enemy.currentWayPoint = 0;
            
            // Means the entity is already over the node
            if (_returnPath.Count == 0)
                PatrolNodes();
        }

        _enemy.Move(_returnPath[_currentReturnNode].transform.position);
        
        Vector3 pointDistance = _returnPath[_currentReturnNode].transform.position - _enemy.transform.position;
    
        if (pointDistance.magnitude < _enemy.StoppingDistance)
            _currentReturnNode++;
    }
    
    private void FindVisibleNodes()
    {
        _visibleNodes.Clear();

        Collider[] nodesInViewRadius = Physics.OverlapSphere(_enemy.transform.position, _enemy.viewRadius, _enemy.nodeLayer);

        foreach (var item in nodesInViewRadius)
        {
            GameObject node = item.gameObject;

            Vector3 dirToTarget = node.transform.position - _enemy.transform.position;

            if (Vector3.Angle(_enemy.transform.forward, dirToTarget) < _enemy.angleRadius / 2)
            {
                if (!Physics.Raycast(_enemy.transform.position, dirToTarget, dirToTarget.magnitude,
                    _enemy.obstacleLayer))
                    _visibleNodes.Add(node.transform);
            }
        }
    }
}
