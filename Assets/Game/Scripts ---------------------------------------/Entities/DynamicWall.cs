using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicWall : MonoBehaviour, IKinetic
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private Collider2D debugCollider;

    [Header("Settings")]
    [SerializeField]
    private float grabbedMass = 5;
    [SerializeField]
    private float defaultMass = 500;
    [SerializeField]
    private float throwTime = 0.2f;

    [GambaHeader("Cloud")]
    [SerializeField]
    private Vector2 size = Vector2.one;
    [SerializeField]
    private Vector2 offset = Vector2.zero;

    [ReadOnly, SerializeField]
    private bool isInsideOfWall;

    private bool isActiveOnDistance;

    private Vector2 lastValidPosition;
    private Quaternion lastValidRotation;

    private event Action onKineticGrab;

    // Wall Detection
    ContactFilter2D filter = new ContactFilter2D();

    private void Start()
    {
        rb.mass = defaultMass;

        filter.layerMask = LayerMask.GetMask("Map");
        filter.useLayerMask = true;

        StartCoroutine(IsActiveOnDistanceLoop());
    }

    private void LateUpdate()
    {
        if (isActiveOnDistance)
        {
            InsideOfWallUpdate();
        }
    }

    #region IKinetic

    public KineticState kineticState { get; set; }

    public bool isDead => false;

    public Vector2 kineticCloudScale => size;

    public Vector2 kineticCloudOffset => offset;

    public bool lookAtPlayer => true;

    public bool rotateCloud => true;

    public Rigidbody2D kRigidbody => rb;

    public Transform kTransform => transform;

    public float kineticMomentumMagnitude => 1.5f;

    public void Grab()
    {
        rb.mass = grabbedMass;
        rb.bodyType = RigidbodyType2D.Dynamic;

        onKineticGrab?.Invoke();
    }

    public void Throw(Vector2 direction, float force)
    {
        rb.velocity += direction.normalized * force;

        rb.mass = defaultMass;

        Timer.CallOnDelay(() => rb.bodyType = RigidbodyType2D.Static, throwTime, ref onKineticGrab, "Throw dynamic wall");
    }

    public void Unleash()
    {
        rb.mass = defaultMass;
        rb.bodyType = RigidbodyType2D.Static;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void InsideOfWallUpdate()
    {
        isInsideOfWall = IsInsideOfWall();

        if (isInsideOfWall)
        {
            transform.position = lastValidPosition;
            transform.rotation = lastValidRotation;
        }
        else
        {
            lastValidPosition = transform.position;
            lastValidRotation = transform.rotation;
        }
    }

    private bool IsInsideOfWall()
    {
        bool r = false;

        List<Collider2D> results = new List<Collider2D>();

        debugCollider.OverlapCollider(filter, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i] != null)
            {
                if (!results[i].isTrigger)
                {
                    r = true;
                    break;
                }
            }
        }

        return r;
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            isActiveOnDistance = GameManager.IsOnActiveDistance(transform.position);

            yield return new WaitForSeconds(1);
        }
    }

    #endregion

}