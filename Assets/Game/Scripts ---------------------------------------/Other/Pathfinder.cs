using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    Discovered,
    Blocked,
    Explored
}

[ExecuteInEditMode]
public class Pathfinder : MonoBehaviour
{
    [ExecuteInEditMode]
    [System.Serializable]
    public class Node
    {
        public NodeState state = NodeState.Discovered;
        public Vector2 position;
        public float gCost;
        public float hCost;
        public float fCost;
        public Node pastNode;
        public Vector2 pastNodePosition;
        public Node() { }
        public Node(NodeState state = NodeState.Discovered, Vector2 position = new Vector2(), float gCost = 0, float hCost = 0, Node pastNode = null)
        {
            this.state = state;
            this.position = position;
            this.gCost = gCost;
            this.hCost = hCost;
            fCost = gCost + hCost;
            this.pastNode = pastNode;
        }
    }

    [Header("Main Info")]
    [SerializeField]
    private Vector2 aPos;
    [SerializeField]
    private Vector2 bPos;
    [SerializeField]
    [Range(0.5f, 10f)]
    private float resolution = 2;
    [SerializeField]
    private List<Node> evaluatedNodes = new List<Node>();
    [SerializeField]
    private List<Node> finalPath = new List<Node>();
    [SerializeField]
    private Node bestTry;
    [SerializeField]
    [Range(0,3)]
    private int debugLevel;
    [SerializeField]
    [Range(1,500)]
    private int maxIters = 200;

    private Node a;
    private Node b;

    private LayerMask mask;
    private enum Status
    {
        Incomplete,
        Successful
    }
    private Status status;

    #region Singleton

    private readonly static Pathfinder instance;

    private static Pathfinder Instance
    {
        get
        {
            if (instance == null)
            {
                Pathfinder inScene = FindObjectOfType<Pathfinder>();
                if (inScene == null)
                {
                    inScene = new GameObject("Pathfinding", typeof(Pathfinder)).GetComponent<Pathfinder>();
                }
                return inScene;
            }
            else
            {
                return instance;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion

    #region Main

    public static async Task<List<Vector2>> FindPathAsync(Vector2 a, Vector2 b, float resolution = -1, LayerMask? collisionLayers = null)
    {
        Pathfinder Instance = Pathfinder.Instance;
        List<Vector2> results = null;

        await Task.Run(() =>
        {
            Instance.mask = collisionLayers.HasValue ? (int)collisionLayers.Value : LayerMask.GetMask("Default");

            if (Instance.debugLevel >= 1) print("Operation Started..");
            List<Node> nodePath = Instance.GetNodePath(a, b, (resolution != -1) ? resolution : Instance.resolution);
            if (nodePath != null)
            {
                if (Instance.debugLevel >= 3) print("Got NodePath");
                List<Vector2> r = new List<Vector2>(nodePath.Count);

                for (int i = 0; i < nodePath.Count; i++)
                {
                    if (Instance.debugLevel >= 2) print($"Attempting to get node {i} position");
                    r.Add(nodePath[i].position);
                }

                if (Instance.debugLevel >= 1) print($"Pathfind {Instance.status}");
                results = r;
            }
        });

        return results;
    }

    public static async void FindPathAsync(Vector2 a, Vector2 b, Action<List<Vector2>> onComplete, float resolution = -1, LayerMask? collisionLayers = null)
    {
        Pathfinder Instance = Pathfinder.Instance;

        await Task.Run(() =>
        {
            Instance.mask = collisionLayers.HasValue ? (int)collisionLayers.Value : LayerMask.GetMask("Default");

            if (Instance.debugLevel >= 1) print("Operation Started..");
            List<Node> nodePath = Instance.GetNodePath(a, b, (resolution != -1) ? resolution : Instance.resolution);
            if (nodePath != null)
            {
                if (Instance.debugLevel >= 3) print("Got NodePath");
                List<Vector2> r = new List<Vector2>(nodePath.Count);

                for (int i = 0; i < nodePath.Count; i++)
                {
                    if (Instance.debugLevel >= 2) print($"Attempting to get node {i} position");
                    r.Add(nodePath[i].position);
                }

                if (Instance.debugLevel >= 1) print($"Pathfind {Instance.status}");
                onComplete?.Invoke(r);
                return;
            }
            onComplete?.Invoke(null);
        });
    }

    public static List<Vector2> FindPath(Vector2 a, Vector2 b, float resolution = -1, LayerMask? collisionLayers = null)
    {
        Instance.mask = collisionLayers.HasValue? (int)collisionLayers.Value: LayerMask.GetMask("Default");

        if(Instance.debugLevel >= 1) print("Operation Started..");
        List<Node> nodePath = Instance.GetNodePath(a, b, (resolution != -1)? resolution : Instance.resolution);
        if (nodePath != null)
        {
            if (Instance.debugLevel >= 3) print("Got NodePath");
            List<Vector2> r = new List<Vector2>(nodePath.Count);

            for (int i = 0; i < nodePath.Count; i++)
            {
                if (Instance.debugLevel >= 2) print($"Attempting to get node {i} position");
                r.Add(nodePath[i].position);
            }

            if (Instance.debugLevel >= 1) print($"Pathfind {Instance.status}");
            return r;
        }
        return null;
    }

    private List<Node> GetNodePath(Vector2 a, Vector2 b, float resolution)
    {
        if (debugLevel >= 2) print("Getting node path..");
        aPos = a;
        bPos = b;
        this.resolution = resolution;
        finalPath = null;
        if (evaluatedNodes.Count > 0)
        {
            evaluatedNodes.Clear();
            if (debugLevel >= 3) print("Evaluated nodes cleared");
        }
        int iter = maxIters;
        for (int i = 0; i < iter; i++)
        {
            if (finalPath != null)
            {
                if (debugLevel >= 1) print("Final path found!");
                return finalPath;
            }
            else
            {
                if (debugLevel >= 1) print($"Exploring next node: {i}");
                ExploreNextNode();
            }
        }
        if (debugLevel >= 1) print("Final path not found, returning best try");
        status = Status.Incomplete;
        if (bestTry != null)
        {
            return GetPath(bestTry);
        }
        else
        {
            return null;
        }
    }

    private void ExploreNextNode()
    {
        if (evaluatedNodes.Count > 0)
        {
            Node minFNode = null;
            int minIndex = 0;
            for (int i = 0; i < evaluatedNodes.Count; i++)
            {
                if (debugLevel >= 3) print($"Evaluating node {i} for minFNode");
                if (evaluatedNodes[i].state == NodeState.Discovered)
                {
                    if (minFNode != null)
                    {
                        if (evaluatedNodes[i].fCost < minFNode.fCost)
                        {
                            if (debugLevel >= 2) print($"Updating minFNode! for {i}");
                            minFNode = evaluatedNodes[i];
                            minIndex = i;
                        }
                    }
                    else
                    {
                        if (debugLevel >= 2) print("Got the first discovered node");
                        minFNode = evaluatedNodes[i];
                    }
                }
            }

            if (minFNode != null)
            {
                if (debugLevel >= 1) print($"MinFNode is {minIndex}");
                minFNode.state = NodeState.Explored;
                DiscoverNodes(minFNode);
                bestTry = minFNode;
            }
            else
            {
                if (debugLevel >= 1) Debug.LogWarning("No more discovered nodes");
            }
        }
        else
        {
            if (debugLevel >= 1) print("Exploring nodes a & b");
            b = new Node((CheckNodeCollision(bPos)) ? NodeState.Blocked : NodeState.Explored, bPos);
            evaluatedNodes.Add(b);
            if (b.state == NodeState.Blocked)
            {
                if (a == null)
                {
                    a = new Node();
                }
                a.position = aPos;
                a.pastNode = a;
                if (debugLevel >= 1) print("Invalid location");
                status = Status.Incomplete;
                finalPath = GetPath(a);
                return;
            }
            a = new Node(NodeState.Explored, aPos);
            evaluatedNodes.Add(a);
            DiscoverNodes(a);
        }
    }

    #endregion

    #region Processes

    private List<Node> GetPath(Node node)
    {
        List<Node> backwardsPath = new List<Node>(0);
        if (node != null)
        {
            backwardsPath.Add(node);
            if (debugLevel >= 3) print("First node added..");
            int iter = evaluatedNodes.Count;
            for (int i = 0; i < iter; i++)
            {
                if (i < backwardsPath.Count && backwardsPath[i].pastNode != null)
                {
                    backwardsPath.Add(backwardsPath[i].pastNode);
                    if (debugLevel >= 2) print($"adding node {i} to the path");
                    if (backwardsPath[i].pastNode == a)
                    {
                        if (debugLevel >= 1) print("Reached a! converting to forward path..");
                        List<Node> forwardPath = new List<Node>(backwardsPath.Count);

                        for (int n = backwardsPath.Count - 1; n >= 0; n--)
                        {
                            forwardPath.Add(backwardsPath[n]);
                        }
                        return forwardPath;
                    }
                }
            }
            if (debugLevel >= 1) print("Couldnt reach final path, returning backwards path");
        }
        return backwardsPath;
    }

    private void DiscoverNodes(Node node)
    {
        if (debugLevel >= 2) print($"Discovering nodes for ({node.position.x}, {node.position.y})");

        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * Vector2.up);
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * new Vector2(1,1));
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * Vector2.right);
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * new Vector2(1,-1));
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * Vector2.down);
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * new Vector2(-1,-1));
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * Vector2.left);
        if (finalPath == null) CalculateNode(node, node.position + 1f / resolution * new Vector2(-1,1));
    }

    private void CalculateNode(Node originalNode, Vector2 pos)
    {
        if (debugLevel >= 3) print($"Calculating node at: ({pos.x}, {pos.y}) from ({originalNode.position.x}, {originalNode.position.y})");
        if (debugLevel >= 3) print("Checking for existing node..");
        Node node = CheckForExistingNode(pos);
        if (node != null)
        {
            if (node == b)
            {
                if (debugLevel >= 1) print("Found b! Calculating path..");
                status = Status.Successful;
                finalPath = GetPath(node);
                return;
            }
            if (debugLevel >= 3) print("There's a node in there");
            if (node.state == NodeState.Discovered)
            {
                if (originalNode.gCost + (pos - originalNode.position).sqrMagnitude < node.gCost)
                {
                    if (debugLevel >= 3) print("Better gCost form here, updating..");
                    node.gCost = originalNode.gCost + (pos - originalNode.position).sqrMagnitude;
                    node.fCost = node.gCost + node.hCost;

                    node.pastNode = originalNode;
                    node.pastNodePosition = originalNode.position;
                }
            }
        }
        else
        {
            if (debugLevel >= 3) print("Not a node here yet, creating one..");
            node = new Node((CheckNodeCollision(pos)) ? NodeState.Blocked : NodeState.Discovered, pos, originalNode.gCost + (pos - originalNode.position).sqrMagnitude, (pos - b.position).sqrMagnitude, originalNode);
            node.pastNodePosition = originalNode.position;
            if (debugLevel >= 3) print("Adding node to evaluatedNodes..");
            evaluatedNodes.Add(node);
            if (node.hCost <= 1f / resolution)
            {
                if (debugLevel >= 1) print("Found node close to b, calculating path..");
                status = Status.Successful;
                finalPath = GetPath(node);
                return;
            }
        }
        
    }

    private bool CheckNodeCollision(Vector2 pos)
    {
        bool r = false;

        Collider2D collider = Physics2D.OverlapCircle(pos, 1f / resolution, mask);

        if (collider != null)
        {
            if (!collider.isTrigger)
            {
                r = true;
            }
        }
        return r;
    }

    private Node CheckForExistingNode(Vector2 pos)
    {
        for (int i = 0; i < evaluatedNodes.Count; i++)
        {
            if (evaluatedNodes[i].position.x == pos.x)
            {
                if (evaluatedNodes[i].position.y == pos.y)
                {
                    return evaluatedNodes[i];
                }
            }
        }
        return null;
    }

    #endregion
}
