using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DynamicBox : Box, IKinetic
{
    [GambaHeader("Dynamic Box------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private new SpriteRenderer light;
    [SerializeField]
    private float defaultMass = 10;
    [SerializeField]
    private float grabbedMass = 5;

    protected override void Start()
    {
        base.Start();

        deathDuration = explosion.main.duration;

        rb.mass = defaultMass;
    }

    public override void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        base.OnHit(hitterPosition, damage, knockback, layer);

        if (!dead)
        {
            rb.velocity = ((Vector2)kTransform.position - hitterPosition).normalized * knockback;
        }
    }

    protected override void Die()
    {
        base.Die();

        explosion.Emit(20);

        sr.enabled = false;
        collider.enabled = false;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }

    protected override void DeathAnim(float timer)
    {
        light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
    }

    #region IKinetic

    public KineticState kineticState { get; set; }

    public bool isDead => dead;

    public Rigidbody2D kRigidbody => rb;

    public Transform kTransform => transform;

    public Vector2 kineticCloudScale => Vector2.one;

    public Vector2 kineticCloudOffset => Vector2.zero;

    public float kineticMomentumMagnitude => 0;

    public bool lookAtPlayer => false;

    public bool rotateCloud => false;

    public virtual void Unleash()
    {
        rb.mass = defaultMass;
    }

    public virtual void Grab()
    {
        rb.mass = grabbedMass;
    }

    public virtual void Throw(Vector2 direction, float force)
    {
        rb.mass = defaultMass;
        rb.velocity += direction.normalized * force;
    }

    #endregion

}
