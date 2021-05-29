using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows.Speech;

public class PatrolState : IState
{
    private StateMachine _sm;
    private readonly Entity _entity;

    public PatrolState(Entity entity, StateMachine sm)
    {
        _sm = sm;
        _entity = entity;
    }
    
    public void OnUpdate()
    {
        if (_entity.target != null)
        {
            //_sm.ChangeState("ChaseState");
            return;
        }
        
        FindVisibleTargets();
        Move();
        Rotate();
    }

    public void Move()
    {
        float step =  _entity.Speed * Time.deltaTime;
        Vector3 pos = _entity.wayPoints[_entity.currentWayPoint].transform.position;
        pos.y = _entity.transform.position.y;
        _entity.transform.position = Vector3.MoveTowards(_entity.transform.position, pos, step);
        
        Vector3 pointDistance = _entity.wayPoints[_entity.currentWayPoint].transform.position - _entity.transform.position;
        
        if (pointDistance.magnitude < _entity.stoppingDistance)
        {
            _entity.currentWayPoint++;
            if (_entity.currentWayPoint > _entity.wayPoints.Length - 1)
                _entity.currentWayPoint = 0;
        }
    }

    private void Rotate()
    {
        Vector3 pos = _entity.wayPoints[_entity.currentWayPoint].transform.position;
        pos.y = _entity.transform.position.y;
        
        _entity.transform.LookAt(pos);
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
}
