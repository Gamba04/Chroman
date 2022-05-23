using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Border : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private SpriteRenderer sr;
    [SerializeField]
    private new BoxCollider2D collider;
    [Header("Settings")]
    [SerializeField]
    private float textureSpeed = 1;
    [GambaHeader("Alpha Shift")]
    [SerializeField]
    private AnimationCurve distanceCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private float alphaRange = 1f;
    [SerializeField]
    private float minAlpha = 0.3f;
    [SerializeField]
    private float maxAlpha = 0.7f;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private float referenceDistance;
    [ReadOnly, SerializeField]
    private Vector2 textureOffset;
    [ReadOnly, SerializeField]
    private bool isActiveOnDistance;
    [SerializeField]
    // Target detection
    Collider2D[] targets = new Collider2D[5];


    private void Start()
    {
        StartCoroutine(IsActiveOnDistanceLoop());
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (isActiveOnDistance)
            {
                OffsetUpdate();
                TargetUpdate();
                AlphaUpdate();
            }
        }
        else
        {
            if (collider && sr)
            {
                collider.size = sr.size;
            }
        }
    }

    private void OffsetUpdate()
    {
        textureOffset.x += Time.deltaTime * textureSpeed;

        if (textureOffset.x > 1) textureOffset.x = 0;
        if (textureOffset.x < 0) textureOffset.x = 1;

        sr.sharedMaterial.SetVector("_Offset", new Vector4(textureOffset.x, textureOffset.y, 0, 0));
    }

    private void AlphaUpdate()
    {
        float alpha = referenceDistance > 0? alphaRange / referenceDistance : 0;
        alpha = distanceCurve.Evaluate(alpha);
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, Mathf.Clamp(alpha, minAlpha, maxAlpha));
    }

    private void TargetUpdate()
    {
        targets = Physics2D.OverlapCircleAll(transform.position, alphaRange / Mathf.Max(minAlpha, 0.1f), LayerMask.GetMask("Default", "Hittable"));

        if (targets.Length > 0)
        {
            referenceDistance = 0;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].attachedRigidbody?.bodyType == RigidbodyType2D.Dynamic)
                {
                    if (referenceDistance > 0)
                    {
                        referenceDistance = Mathf.Min(referenceDistance, (targets[i].transform.position - transform.position).sqrMagnitude);
                    }
                    else
                    {
                        referenceDistance = (targets[i].transform.position - transform.position).sqrMagnitude;
                    }
                }
            }

            referenceDistance = Mathf.Sqrt(referenceDistance);
        }
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            isActiveOnDistance = GameManager.IsOnActiveDistance(transform.position);

            yield return new WaitForSeconds(1);
        }
    }
}