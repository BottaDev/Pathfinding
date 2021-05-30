using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chase : State
{
    private StateMachine _sm;
    private readonly Enemy _enemy;
    private int _currentChaseNode = 0;
    private bool _targetIsVisible;
    private bool _lastNodeReached;

    public Chase(Enemy enemy, StateMachine sm)
    {
        _sm = sm;
        _enemy = enemy;
    }
    
    public void OnUpdate()
    {
        Debug.Log("CHASE");

        GameObject t = _enemy.ApplyFOV(_enemy.targetLayer);
        _targetIsVisible = t; 

        if (_targetIsVisible)
        {
            _enemy.lastTargetPosition = t.transform.position;
            _enemy.AlertEnemies();
            ChaseTarget();   
        }
        else
        {
            MoveToLastPos();
        }
    }
    
    /// <summary>
    /// Chase the target without using nodes
    /// </summary>
    private void ChaseTarget()
    {
        if (_enemy.target == null)
            return;
        
        _enemy.Move(_enemy.target.transform.position);
    }
    
    /// <summary>
    /// Moves to the last position seen of the target 
    /// </summary>
    private void MoveToLastPos()
    {
        if (_lastNodeReached)
        {
            _enemy.Move(_enemy.lastTargetPosition);

            Vector3 pointDistance = _enemy.lastTargetPosition - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.StoppingDistance)
            {
                _enemy.target = null; 
                _sm.ChangeState("Patrol");
            }
        }
        else
        {
            if (_enemy.chasePath.Count == 0)
            {
                Node goalNode = GetNerbyTargetNode();
                
                // Means the target is death
                if (goalNode == null)
                {
                    _lastNodeReached = true;
                    return;
                }
                
                _enemy.chasePath = _enemy.ConstructPath(_enemy.GetNerbyNode(), goalNode);
                _enemy.chasePath.Reverse();

                _currentChaseNode = 0;

                if (_enemy.chasePath.Count == 0)
                {
                    _lastNodeReached = true;
                    return;
                }
            }
            
            _enemy.Move(_enemy.chasePath[_currentChaseNode].transform.position);

            Vector3 pointDistance = _enemy.chasePath[_currentChaseNode].transform.position - _enemy.transform.position;

            if (pointDistance.magnitude < _enemy.StoppingDistance)
            {
                _currentChaseNode++;
                if (_currentChaseNode > _enemy.chasePath.Count - 1)
                    _lastNodeReached = true;
            }
        }
    }
    
    private Node GetNerbyTargetNode()
    {
        if (_enemy.target == null)
            return null;
        
        GameObject nerbyNode = null;

        List<Node> allNodes = GameObject.FindObjectsOfType<Node>().ToList();

        float distance = 999f;
        
        foreach (var item in allNodes)
        {
            Vector3 nodeDistance = item.transform.position - _enemy.target.transform.position;

            if (nodeDistance.magnitude < distance)
            {
                distance = nodeDistance.magnitude;
                nerbyNode = item.gameObject;
            }
        }
        
        return nerbyNode.GetComponent<Node>();
    }
}
