using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Properties")] 
    public float Speed;
    
    [Header("Patrol")]
    public List<Transform> wayPoints;
    public float stoppingDistance;
    [HideInInspector]
    public int currentWayPoint = 0;
    [HideInInspector]
    public List<Transform> visibleNodes;

    [Header("Variables")]
    public GameObject target;
    public Node startingNode;
    public Node goalNode;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    public LayerMask nodeLayer;

    [Header("Field of View")]
    public float viewRadius;
    public float angleRadius;

    private Vector3 _velocity;
    private StateMachine _sm;
    private List<Entity> _entities; 

    private void Awake()
    {
        _sm = GetComponent<StateMachine>();
        _sm.AddState("PatrolState", new PatrolState(this, _sm));
        _sm.ChangeState("PatrolState");
    }

    private void Start()
    {
        _entities = GameObject.FindObjectsOfType<Entity>().Where(x => x != this).ToList();
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

            foreach (var next in current.neighbors)
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
        return null;
    }

    public void AlertEntities()
    {
        foreach (Entity entity in _entities)
        {
            entity.target = target;
        }
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
