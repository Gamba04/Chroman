using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerCollider : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Collider2D collider;

    [Header("Settings")]
    [SerializeField]
    private UnityEvent onTrigger;
    [SerializeField]
    private bool deactivateOnCollision;
    [SerializeField]
    private LayerMask detectionLayers;

    private bool inside;

    private void Update()
    {
        CollisionDetection();
    }

    private void CollisionDetection()
    {
        if (collider.enabled)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = detectionLayers;
            filter.useLayerMask = true;

            List<Collider2D> results = new List<Collider2D>();

            if (Physics2D.OverlapCollider(collider, filter, results) > 0)
            {
                if (!inside)
                {
                    inside = true;

                    onTrigger?.Invoke();

                    if (deactivateOnCollision)
                    {
                        collider.enabled = false;
                    }
                }
            }
            else inside = false;
        }
    }
}
