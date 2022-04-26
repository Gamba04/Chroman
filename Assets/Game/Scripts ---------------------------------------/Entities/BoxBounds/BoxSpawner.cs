using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private GameObject prefab;
    [Header("Settings")]
    [SerializeField]
    private Vector2 defaultSpawnPoint;
    [SerializeField]
    private float defaultSpawnDelay;

    private void Spawn(Vector2 spawnPoint, Transform parent)
    {
        if (parent.gameObject.activeInHierarchy)
        {
            Instantiate(prefab, spawnPoint, prefab.transform.rotation, parent);
        }
    }

    public void SpawnOnDelay(Transform parent, float delay = -1)
    {
        if (delay < 0)
        {
            delay = defaultSpawnDelay;
        }

        Timer.CallOnDelay(() =>
        {
            Spawn(defaultSpawnPoint, parent);
        }, delay, $"BoxSpawner: Spawning {prefab.name}");
    }

    public void SpawnOnDelay(Vector2 spawnPoint, Transform parent, float delay = -1)
    {
        if (delay < 0)
        {
            delay = defaultSpawnDelay;
        }

        Timer.CallOnDelay(() =>
        {
            Spawn(spawnPoint, parent);
        }, delay, $"BoxSpawner: Spawning {prefab.name} in {spawnPoint}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.1f);
        Gizmos.DrawWireSphere(defaultSpawnPoint, 0.5f);
    }
}