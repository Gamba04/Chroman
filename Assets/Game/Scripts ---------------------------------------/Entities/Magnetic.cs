using UnityEngine;

public class Magnetic : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 2)]
    private float attractionForce = 0.25f;

    [Header("Info")]
    [ReadOnly, SerializeField]
    private bool active = true;

    public void Attract(Vector2 attractionVector)
    {
        if (!active) return;

        rb.velocity += attractionVector.normalized * attractionForce / attractionVector.magnitude;
    }

    public void SetEnabled(bool active)
    {
        this.active = active;
    }
}