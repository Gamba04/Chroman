using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxBounds : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private BoxSpawner spawner;
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private GameObject laserRoot;
    [SerializeField]
    private Transform pointA;
    [SerializeField]
    private Transform pointB;
    [Header("Settings")]
    [SerializeField]
    private LayerMask detectionLayers;
    [SerializeField]
    private float laserLenght;
    [Space()]
    [SerializeField]
    private bool useCustomSpawnPoint;
    [SerializeField]
    private Vector2 customSpawnPoint;
    [Space()]
    [SerializeField]
    private bool useCustomDelay;
    [SerializeField]
    private Vector2 customDelay;

    // Collisions
    private List<Collider2D> targets = new List<Collider2D>();
    private ContactFilter2D filter = new ContactFilter2D();

    private void Start()
    {
        filter.layerMask = detectionLayers;
        filter.useLayerMask = true;
    }

    private void CollisionDetection()
    {
        collider.OverlapCollider(filter, targets);

        for (int i = 0; i < targets.Count; i++)
        {
            Box box = targets[i].GetComponent<Box>();

            if (box != null)
            {

            }
        }
    }
}