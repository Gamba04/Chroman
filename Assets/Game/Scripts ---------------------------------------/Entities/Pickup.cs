using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private GameObject sprite;
    [SerializeField]
    private SpriteRenderer light;
    [Header("Settings")]
    [SerializeField]
    private float detectionRadius = 0.5f;
    [SerializeField]
    private UnityEvent onTrigger;
    [SerializeField]
    private LayerMask collisionLayers;
    [SerializeField]
    private AnimationCurve deathcurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField]
    private AudioPlayer.SFXTag pickupSound;

    private bool dead;
    private float deathCounter;
    private float deathDuration;

    private void Start()
    {
        deathDuration = explosion.duration;
    }

    private void Update()
    {
        CollisionDetection();
        DeathUpdate();
    }

    private void CollisionDetection()
    {
        if (!dead)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = collisionLayers;
            filter.useLayerMask = true;

            Collider2D[] target = new Collider2D[1];

            Physics2D.OverlapCircle(transform.position, detectionRadius, filter, target);

            if (target[0] != null)
            {
                onTrigger?.Invoke();
                Die();

                AudioPlayer.PlaySFX(pickupSound);
            }
        }
    }

    private void DeathUpdate()
    {
        if (dead)
        {
            Timer.ReduceCooldown(ref deathCounter, () =>
            {
                Destroy(gameObject);
            });

            float timer = deathcurve.Evaluate(1 - (deathCounter / deathDuration));

            light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
        }
    }

    protected virtual void Die()
    {
        explosion.Emit(20);

        // Turn off components
        sprite.SetActive(false);

        // Add set death sentence
        deathCounter = deathDuration;
        dead = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.7f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
