using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BounceTest : MonoBehaviour
{
    [SerializeField]
    private Vector2 direction;
    [SerializeField]
    private Vector2 normal;

    private Vector2 result;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,1,1);
        GambaFunctions.GizmosDrawArrow(transform.position, normal.normalized);

        Gizmos.color = new Color(1,0.5f, 0.3f);
        GambaFunctions.GizmosDrawArrow(transform.position, direction.normalized);

        Gizmos.color = Color.blue;
        GambaFunctions.GizmosDrawArrow(transform.position, result);
    }

    private void OnValidate()
    {
        result = GambaFunctions.Bounce(normal, direction);
    }
}
