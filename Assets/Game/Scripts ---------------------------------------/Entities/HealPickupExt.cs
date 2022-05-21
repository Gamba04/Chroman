using UnityEngine;

public class HealPickupExt : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Magnetic magnetic;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float healAmount = 0.2f;

    public void OnPickup()
    {
        GameManager.Heal(healAmount);

        magnetic.OnPickup();
    }
}