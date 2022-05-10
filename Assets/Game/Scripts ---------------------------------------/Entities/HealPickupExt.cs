using UnityEngine;

public class HealPickupExt : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float healAmount = 0.2f;
    [SerializeField]
    [Range(0, 1)]
    private float slowDown = 0.2f;
    [SerializeField]
    private float initialMaxSpeed = 1;

    private void Start()
    {
        rb.velocity = GetRandomSpawnSpeed();
    }

    private Vector2 GetRandomSpawnSpeed() => new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized * Random.Range(0, initialMaxSpeed);

    public void OnPickup()
    {
        GameManager.Heal(healAmount);

        rb.velocity *= slowDown;
    }
}