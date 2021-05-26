using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        FindVisibleTargets();
    }

    public void Move()
    {
        
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
                if(!Physics.Raycast(_entity.transform.position, dirToTarget, dirToTarget.magnitude, _entity.obstacleLayer))
                    _entity.target = target;
            }
        }
    }
}
