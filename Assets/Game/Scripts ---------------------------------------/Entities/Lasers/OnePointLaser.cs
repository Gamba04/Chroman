using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OnePointLaser : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform laserHead;
    [SerializeField]
    private RayLaser ray;
    [Header("Settings")]
    [SerializeField]
    private bool updateRotationInRuntime;
    [SerializeField]
    private float farDistanceMultiplier = 1;
    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private bool isActiveOnDistance;

    private void Start()
    {
        UpdateLaser();
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(IsActiveOnDistanceLoop());
            SetActiveOnDistance(isActiveOnDistance);
        }
        else
        {
            SetActiveOnDistance(true);
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (isActiveOnDistance)
            {
                UpdateLaser();
            }
        }
        else
        {
            UpdateLaser();
        }
    }

    private void UpdateLaser()
    {
        if (laserHead != null && ray != null)
        {
            Vector2 pos = laserHead.position;

            Vector2 dir = laserHead.up;

            // Rotate heads
            laserHead.rotation = Quaternion.LookRotation(Vector3.forward, dir);

            // Set RayLaser
            ray.SetRay(pos, dir);

            // Update Laser for precausion
            ray.UpdatePositions();
        }
    }

    private void SetActiveOnDistance(bool value)
    {
        ray.gameObject.SetActive(value);
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            bool newActive = GameManager.IsOnActiveDistance(transform.position, 0, farDistanceMultiplier);

            if (newActive != isActiveOnDistance)
            {
                isActiveOnDistance = newActive;

                SetActiveOnDistance(newActive);
            }
        }
    }
}