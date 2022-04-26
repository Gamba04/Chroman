using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParallaxLayer
{
    [SerializeField, HideInInspector] private string name;

    public GameObject obj;
    [Range(-1, 1)]
    public float distance = 0;

    [ReadOnly]
    public Vector2 startPos;
    [ReadOnly]
    public Vector2 startScale;

    /// <summary> Sets start position and start scale. </summary>
    public void SetLayer()
    {
        if (obj != null)
        {
            startPos = obj.transform.position;
            startScale = obj.transform.localScale;
        }
    }

    public void SetName()
    {
        if (obj != null)
        {
            name = $"{obj.name} :: {distance}";
        }
        else
        {
            name = $"None :: {distance}";
        }
    }
}

public class ParallaxController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Camera camera;
    [SerializeField]
    private List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    private float cameraStartSize;
    private Vector2 cameraStartPosition;

    void Start()
    {
        cameraStartSize = camera.orthographicSize;
        cameraStartPosition = transform.position;

        SetParallaxLayers();
    }

    /// <summary> Sets all layers' start position and start scale. </summary>
    private void SetParallaxLayers()
    {
        for (int i = 0; i < parallaxLayers.Count; i++)
        {
            parallaxLayers[i].SetLayer();
        }
    }

    void Update()
    {
        UpdateParallax();   
    }

    private void UpdateParallax()
    {
        for (int i = 0; i < parallaxLayers.Count; i++)
        {
            ParallaxLayer layer = parallaxLayers[i];
            if (layer.obj != null)
            {
                // Position parallax
                layer.obj.transform.position = layer.startPos + ((Vector2)camera.transform.position - cameraStartPosition) * layer.distance;

                // Zoom parallax
                layer.obj.transform.localScale = (Vector3)(layer.startScale + (layer.startScale * camera.orthographicSize / cameraStartSize - layer.startScale) * layer.distance) + Vector3.forward;
            }
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (parallaxLayers != null)
        {
            for (int i = 0; i < parallaxLayers.Count; i++)
            {
                parallaxLayers[i].SetName();
            }
        }
    }

#endif

}
