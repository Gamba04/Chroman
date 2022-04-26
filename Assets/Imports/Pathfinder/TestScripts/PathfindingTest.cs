using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> path;
    [SerializeField]
    private Vector2 a;
    [SerializeField]
    private Vector2 b;
    [SerializeField]
    [Range(0.5f,10f)]
    private float resolution = 1;

    [SerializeField]
    private GameObject ball;
    [SerializeField]
    private GameObject donchim;
    private int targetNode;
    private bool reachedDestination;
    [SerializeField]
    private bool pathUpdated;

    [SerializeField]
    private bool updatePath;

    private void Start()
    {
        StartCoroutine(Loop());
        updatePath = true;
    }
    private void Update()
    {
        if (ball != null)
        {
            if (donchim != null)
            {
                if (updatePath)
                {
                    updatePath = false;
                    path = Pathfinder.FindPath(ball.transform.position, donchim.transform.position, resolution);
                    targetNode = 0;
                    reachedDestination = false;
                    pathUpdated = true;
                }
            }
            if (path.Count > 0)
            {
                if (!reachedDestination)
                {
                    ball.transform.position += (new Vector3(path[targetNode].x, path[targetNode].y) - ball.transform.position).normalized * 0.1f;
                }
                if ((ball.transform.position - new Vector3(path[targetNode].x, path[targetNode].y)).magnitude < 0.1f)
                {
                    if (path.Count > targetNode + 1)
                    {
                        targetNode++;
                    }
                    else
                    {
                        reachedDestination = true;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.9f, 0.8f);

        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (pathUpdated)
            {
                pathUpdated = false;
                updatePath = true;
            }
        }
    }

}
