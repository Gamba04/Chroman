using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardBox : Box
{
    [GambaHeader("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private SpriteRenderer light;

    protected override void Start()
    {
        base.Start();

        deathDuration = explosion.duration;
    }

    public override void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        base.OnHit(hitterPosition, damage, knockback, layer);
    }

    protected override void Die()
    {
        base.Die();

        explosion.Emit(20);

        sr.enabled = false;
        collider.enabled = false;
    }

    protected override void DeathAnim(float timer)
    {
        light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
    }
}
