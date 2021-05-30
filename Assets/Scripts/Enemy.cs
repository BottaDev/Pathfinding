using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Properties")] 
    public float Speed = 1f;
    public float StoppingDistance = 0.1f;
    
    [Header("Patrol Mode")]
    public List<Transform> wayPoints;
    
    [Header("Field of View")]
    public float viewRadius;
    public float angleRadius;
    
    [Header("LayerMasks")]
    public LayerMask targetLayer;
    public LayerMask nodeLayer;
    public LayerMask obstacleLayer;

    [Header("DEBUG")]
    [HideInInspector]
    public GameObject target;
    [HideInInspector]
    public int currentWayPoint = 0;
    [HideInInspector]
    public List<Node> chasePath = new List<Node>();
    [HideInInspector]
    public Vector3 lastTargetPosition;
    
    private StateMachine _sm;
    private List<Enemy> _enemies;

    private void Awake()
    {
        _sm = GetComponent<StateMachine>();
        _sm.AddState("PatrolState", new PatrolState(this, _sm));
        _sm.AddState("ChaseState", new ChaseState(this, _sm));
        _sm.ChangeState("PatrolState");
    }

    private void Start()
    {
        _enemies = GameObject.FindObjectsOfType<Enemy>().Where(x => x != this).ToList();
    }
    
    private void Update()
    {
        _sm.OnUpdate();
    }
    
    public List<Node> ConstructPath(Node startingNode, Node goalNode)
    {
        PriorityQueue frontier = new PriorityQueue();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();

        frontier.Put(startingNode, 0);
        cameFrom.Add(startingNode, null);
        costSoFar.Add(startingNode, 0);

        int frontierCount = frontier.Count();

        while (frontierCount != 0)
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
                    float priority = newCost + Heuristic(next.transform.position, goalNode);
                    frontier.Put(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        return null;
    }
    
    public void AlertEnemies()
    {
        foreach (Enemy enemy in _enemies)
        {
            enemy.chasePath.Clear();
            enemy.target = target;
            enemy.lastTargetPosition = target.transform.position;
        }
    }
    
    public GameObject ApplyFOV(LayerMask targetMask)
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        foreach (var item in targetsInViewRadius)
        {
            GameObject target = item.gameObject;

            Vector3 dirToTarget = target.transform.position - transform.position;

            if (Vector3.Angle(transform.forward, dirToTarget) < angleRadius / 2)
            {
                if (!Physics.Raycast(transform.position, dirToTarget, dirToTarget.magnitude,
                    obstacleLayer))
                    return target;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Returns the enemy's closest node 
    /// </summary>
    /// <returns></returns>
    public Node GetNerbyNode()
    {
        GameObject nerbyNode = null;
        
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, nodeLayer);

        float distance = 999f;
        
        foreach (var item in targetsInViewRadius)
        {
            Vector3 nodeDistance = item.transform.position - transform.position;
            
            if (nodeDistance.magnitude < distance)
            {
                distance = nodeDistance.magnitude;
                nerbyNode = item.gameObject;
            }
        }
        
        return nerbyNode.GetComponent<Node>();
    }
    
    public void Move(Vector3 newPos)
    {
        float step =  Speed * Time.deltaTime;
        
        newPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, newPos, step);
        
        Rotate(newPos);
    }

    private void Rotate(Vector3 newPos)
    {
        newPos.y = transform.position.y;
        transform.LookAt(newPos);
    }
    
    private float Heuristic(Vector3 pos, Node goalNode)
    {
        return Mathf.Abs((goalNode.transform.position - pos).magnitude);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
