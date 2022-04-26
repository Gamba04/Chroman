using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Serializable]
    public class StaticArea
    {
        [SerializeField, HideInInspector] private string name;

        public bool isActive;
        public Vector2 position;
        public Vector2 size;
        [Range(0f, 10f)]
        public float exitArea;
        [Range(1, 40)]
        public float zoom;
        [Range(0, 1)]
        public float dynamicInfluence = 0.5f;

        private Vector2 previousPosition;
        private Vector2 previousSize;

        [HideInInspector]
        public float minX;
        [HideInInspector]
        public float maxX;
        [HideInInspector]
        public float minY;
        [HideInInspector]
        public float maxY;

        public bool EntersArea(Vector2 point)
        {
            return point.x > minX && point.x < maxX && point.y > minY && point.y < maxY;
        }

        public bool ExitsArea(Vector2 point)
        {
            return point.x < minX - exitArea || point.x > maxX + exitArea || point.y < minY - exitArea || point.y > maxY + exitArea;
        }

        public void CheckChanges()
        {
            if (previousPosition != position || previousSize != size)
            {
                UpdateValues();
            }
        }

        private void UpdateValues()
        {
            previousPosition = position;
            previousSize = size;

            minX = position.x - size.x / 2f;
            maxX = position.x + size.x / 2f;
            minY = position.y - size.y / 2f;
            maxY = position.y + size.y / 2f;
        }

        public void SetName(string name)
        {
            this.name = name;
        }
    }

    [Header("Components")]
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Camera cam;
    [Header("Settings")]
    [SerializeField]
    [Range(0.1f,10)]
    private float movementSpeed;
    [SerializeField]
    [Range(0.1f,10)]
    private float zoomSpeed;
    [SerializeField]
    [Range(1,20)]
    private float zoom;
    [SerializeField]
    private List<StaticArea> areas = new List<StaticArea>();
    [GambaHeader("Shake")]
    [SerializeField]
    private AnimationCurve shakeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    [SerializeField]
    private float shakeIntensity = 0.2f;
    [SerializeField]
    private float shakeDuration = 0.1f;
    [GambaHeader("Slow Transition")]
    [SerializeField]
    private float slowTransitionDuration = 3;
    [SerializeField]
    private float slowTransitionSpeedMult = 0.1f;
    [SerializeField]
    private AnimationCurve slowTransitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1,1));
    [GambaHeader("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [GambaHeader("Info")]
    [ReadOnly, SerializeField]
    private Vector3 offset = new Vector3(0,0,-10);
    [ReadOnly, SerializeField]
    private Vector2 dynamicTarget;
    [ReadOnly, SerializeField]
    private StaticArea staticArea;
    [ReadOnly, SerializeField]
    private bool isOnArea;
    [GambaHeader("Target Values", 0.7f)]
    [ReadOnly, SerializeField]
    private Vector3 targetPos;
    [ReadOnly, SerializeField]
    private float targetZoom;
    [ReadOnly, SerializeField]
    private float targetSlowTransitionMult;

    private float slowTransitionCooldown;
    private bool previousIsOnArea;

    private Vector3 prePosition;

    private Vector2 shakeDisplacement;
    private float shakeTimer;

    private static event Action onShake;

    private void Start()
    {
        prePosition = transform.position;

        onShake -= OnShake;
        onShake += OnShake;
    }

    private void FixedUpdate()
    {
        UpdateDynamicTarget();
        UpdateStaticTarget();

        UpdateTargets();
        SlowTransitionUpdate();
        ShakeUpdate();

        CameraMovement();
        CameraZoom();
    }

    #region TargetUpdate

    private void UpdateDynamicTarget()
    {
        dynamicTarget = targetTransform.position;
    }

    private void UpdateStaticTarget()
    {
        if (isOnArea)
        {
            if (staticArea.ExitsArea(dynamicTarget) || !staticArea.isActive)
            {
                isOnArea = false;

                staticArea = null;
            }
        }
        else
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i].isActive)
                {
                    if (areas[i].EntersArea(dynamicTarget))
                    {
                        isOnArea = true;

                        staticArea = areas[i];

                        break;
                    }
                }
            }
        }

        if (previousIsOnArea != isOnArea)
        {
            slowTransitionCooldown = slowTransitionDuration;
            previousIsOnArea = isOnArea;
        }
    }

    private void UpdateTargets()
    {
        if (isOnArea && staticArea != null)
        {
            targetPos = (Vector3)(staticArea.position + (dynamicTarget - staticArea.position) * staticArea.dynamicInfluence) + offset;
            targetZoom = staticArea.zoom;
        }
        else
        {
            targetPos = (Vector3)dynamicTarget + offset;
            targetZoom = zoom;
        }
    }

    private void SlowTransitionUpdate()
    {
        if (slowTransitionDuration > 0)
        {
            if (slowTransitionCooldown > 0)
            {
                Timer.ReduceCooldown(ref slowTransitionCooldown);

                float progress = (slowTransitionDuration - slowTransitionCooldown) / slowTransitionDuration;

                targetSlowTransitionMult = Mathf.Lerp(slowTransitionSpeedMult, 1, slowTransitionCurve.Evaluate(progress));
            }
            else
            {
                targetSlowTransitionMult = 1;
            }
        }
        else
        {
            targetSlowTransitionMult = 1;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Movement

    private void CameraMovement()
    {
        prePosition += (targetPos - transform.position) * Mathf.Clamp01(Time.deltaTime * movementSpeed * targetSlowTransitionMult);

        transform.position = prePosition + (Vector3)shakeDisplacement;
    }

    private void CameraZoom()
    {
        cam.orthographicSize += (targetZoom - cam.orthographicSize) * Mathf.Clamp01(Time.deltaTime * zoomSpeed * targetSlowTransitionMult);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Shake

    private void ShakeUpdate()
    {
        if (shakeTimer > 0)
        {
            shakeDisplacement = GenerateShake(shakeCurve.Evaluate(1 - shakeTimer));

            shakeTimer -= Time.deltaTime/shakeDuration;
        }
        else 
        {
            shakeTimer = 0;
            shakeDisplacement = Vector2.zero;
        }
    }

    private Vector2 GenerateShake(float amplitude)
    {
        return new Vector2(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1)) * shakeIntensity * amplitude;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
    }

    public void ActivateArea(int index)
    {
        if (areas != null)
        {
            if (index < areas.Count)
            {
                areas[index].isActive = true;
            }
        }
    }

    public void DeactivateArea(int index)
    {
        if (areas != null)
        {
            if (index < areas.Count)
            {
                areas[index].isActive = false;
            }
        }
    }

    public void OnShake()
    {
        shakeTimer = 1;
    }

    public static void Shake()
    {
        onShake?.Invoke();
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Gizmos

#if UNITY_EDITOR

    private void DrawRectangle(float minX, float maxX, float minY, float maxY, float separation, int maxIters)
    {
        Vector2 from;
        Vector2 to;

        // Bottom --------------------------------------------------------------------------------------------
        from = new Vector2(minX, minY);
        to = new Vector2(maxX, minY);

        GambaFunctions.GizmosDrawPointedLine(from, to, separation, maxIters);

        // Right --------------------------------------------------------------------------------------------
        from = new Vector2(maxX, minY);
        to = new Vector2(maxX, maxY);

        GambaFunctions.GizmosDrawPointedLine(from, to, separation, maxIters);

        // Top --------------------------------------------------------------------------------------------
        from = new Vector2(maxX, maxY);
        to = new Vector2(minX, maxY);

        GambaFunctions.GizmosDrawPointedLine(from, to, separation, maxIters);

        // Left --------------------------------------------------------------------------------------------
        from = new Vector2(minX, maxY);
        to = new Vector2(minX, minY);

        GambaFunctions.GizmosDrawPointedLine(from, to, separation, maxIters);
    }

    private void OnDrawGizmos()
    {
        float separation = 1;
        int maxIters = 500;

        for (int i = 0; i < areas.Count; i++)
        {

            StaticArea a = areas[i];
            a.CheckChanges();

            Color col = a.isActive ? Color.white : Color.red;
            Gizmos.color = col;

            // Inner Area
            DrawRectangle(a.minX, a.maxX, a.minY, a.maxY, separation, maxIters);

            if (a.exitArea > 0)
            {
                // Outer Area
                Gizmos.color = new Color(col.r, col.g, col.b, 0.5f);
                DrawRectangle(a.minX - a.exitArea, a.maxX + a.exitArea, a.minY - a.exitArea, a.maxY + a.exitArea, separation, maxIters);
            }
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            areas[i].SetName($"Area {i}");
            areas[i].CheckChanges();
        }
    }

#endif

    #endregion

}
