using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ExplosiveBox : Box, IKinetic
{
    [GambaHeader("Explosive Box------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private float explosionDelay = 0.5f;
    [SerializeField]
    private Explosion explosion;
    [SerializeField]
    private new SpriteRenderer light;
    [SerializeField]
    private float defaultMass = 10;
    [SerializeField]
    private float grabbedMass = 5;
    [SerializeField]
    private float environmentDamage;
    [SerializeField]
    private float playerDamage;
    [SerializeField]
    private LayerMask hitLayers;
    [SerializeField]
    private Collider2D explosionCollider;

    protected override void Start()
    {
        base.Start();

        deathDuration = 2; // duration

        rb.mass = defaultMass;
    }

    public override void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        base.OnHit(hitterPosition, damage, knockback, layer);

        rb.velocity = ((Vector2)kTransform.position - hitterPosition).normalized * knockback;
    }

    protected override void Die()
    {
        base.Die();

        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.ExplosionWindUp);

        Timer.CallOnDelay(() =>
        {
            explosion.Play();

            sr.enabled = false;
            collider.enabled = false;
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

            AudioPlayer.PlaySFX(AudioPlayer.SFXTag.ExplosiveBarrel);

            CollisionDetection();
        }, explosionDelay, "Box explosion");
    }

    protected override void DeathAnim(float timer)
    {
        light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
    }

    private void CollisionDetection()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = hitLayers;
        filter.useLayerMask = true;

        List<Collider2D> targets = new List<Collider2D>();

        explosionCollider.OverlapCollider(filter, targets);

        for (int i = 0; i < targets.Count; i++)
        {
            IHittable hit = targets[i]?.attachedRigidbody?.GetComponent<IHittable>();

            if (hit != null)
            {
                if (!hit.AvoidingLayer(gameObject.layer))
                {
                    hit.OnHit(transform.position, targets[i].attachedRigidbody.gameObject.layer == 8 ? playerDamage : environmentDamage, 20, gameObject.layer);
                }
            }
        }
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