using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TwoPointLaser : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform laserA;
    [SerializeField]
    private RayLaser rayA;
    [SerializeField]
    private Transform laserB;
    [SerializeField]
    private RayLaser rayB;
    [Header("Settings")]
    [SerializeField]
    private bool updatePositionsInRuntime;
    [SerializeField]
    private float farDistanceMultiplier = 1;
    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private bool isActiveOnDistance;
    [ReadOnly, SerializeField]
    private float distanceToPlayer;
    private float lenght;

    private void Start()
    {
        lenght = (laserA.position - laserB.position).magnitude;

        UpdateLasers();
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
                if (updatePositionsInRuntime)
                {
                    UpdateLasers();
                }
            }
        }
        else
        {
            UpdateLasers();
        }
    }

    private void UpdateLasers()
    {
        if (laserA != null && laserB != null && rayA != null && rayB != null)
        {
            Vector2 posA = laserA.position;
            Vector2 posB = laserB.position;

            Vector2 dirA = (laserB.position - laserA.position);
            Vector2 dirB = (laserA.position - laserB.position);

            // Rotate heads
            laserA.rotation = Quaternion.LookRotation(Vector3.forward, dirA);
            laserB.rotation = Quaternion.LookRotation(Vector3.forward, dirB);

            // Set RayLaser
            rayA.SetRay(posA, dirA, dirA.magnitude);
            rayB.SetRay(posB, dirB, dirB.magnitude);

            // Update Lasers for precausion
            rayA.UpdatePositions();
            rayB.UpdatePositions();
        }
    }

    private void SetActiveOnDistance(bool value)
    {
        rayA.gameObject.SetActive(value);
        rayB.gameObject.SetActive(value);
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            bool newActive = GameManager.IsOnActiveDistance(transform.position, lenght, farDistanceMultiplier);
            distanceToPlayer = (GameManager.Player.transform.position - transform.position).magnitude;
            if (newActive != isActiveOnDistance)
            {
                isActiveOnDistance = newActive;

                SetActiveOnDistance(newActive);
            }
        }
    }
}
