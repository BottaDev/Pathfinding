using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Properties")] 
    public float Speed;
    
    public GameObject target;
    public Node startingNode;
    public Node goalNode;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;

    [Header("Patrol State")]
    public float viewRadius;
    public float angleRadius;
    public Transform[] wayPoints;

    private Vector3 _velocity;
    private StateMachine _sm;

    private void Awake()
    {
        _sm = GetComponent<StateMachine>();
        _sm.AddState("PatrolState", new PatrolState(this, _sm));
        _sm.ChangeState("PatrolState");
    }

    private void Update()
    {
        _sm.OnUpdate();
    }

    public List<Node> ConstructPath()
    {
        PriorityQueue frontier = new PriorityQueue();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();

        frontier.Put(startingNode, 0);
        cameFrom.Add(startingNode, null);
        costSoFar.Add(startingNode, 0);

        while (frontier.Count() != 0)
        {
            Node current = frontier.Get();

            if (current == goalNode)
            {
                List<Node> path = new List<Node>();
                while (current != startingNode)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                return path;
            }

            foreach (Node next in current.neighbors)
            {
                if (!next.IsBlocked)
                {
                    float newCost = costSoFar[current] + next.Cost;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {

                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(next.transform.position);
                        frontier.Put(next, priority);
                        cameFrom[next] = current;
                    }
                }

            }
        }
        return null;
    }
    
    public float Heuristic(Vector3 pos)
    {
        return Mathf.Abs((goalNode.transform.position - pos).magnitude);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
