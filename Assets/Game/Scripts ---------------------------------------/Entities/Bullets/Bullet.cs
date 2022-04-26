using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IKinetic
{
    [Serializable]
    public class BulletSkin
    {
        public SpriteRenderer sr;
        public SpriteRenderer light;
        public ParticleSystem ps;
        public ParticleSystem explosion;
        public TrailRenderer tr;
        public GameObject aura;

        public void SetActive(bool value)
        {
            if (sr) sr.enabled = value;
            if (light) light.enabled = value;
            if (ps) ps.enableEmission = value;
            if (explosion) explosion.enableEmission = value;
            if(tr) tr.enabled = value;
            if (aura) aura.SetActive(value);
        }

        public void CopySkin(BulletSkin replaceValue)
        {
            sr = replaceValue.sr;
            light = replaceValue.light;
            ps = replaceValue.ps;
            explosion = replaceValue.explosion;
            tr = replaceValue.tr;
            aura = replaceValue.aura;
        }
    }

    [Header("Components")]
    [SerializeField]
    protected Rigidbody2D rb;
    [SerializeField]
    protected Collider2D collider;
    [SerializeField]
    protected BulletSkin skin;
    [Space()]
    [GambaHeader("IKinetic", 0.6f, 0.1f, 1)]
    [SerializeField]
    private bool useKineticSkin;
    [SerializeField]
    private BulletSkin kineticSkin;
    [Header("Settings")]
    [SerializeField]
    protected float damage;
    [SerializeField]
    protected float speed = 1;
    [SerializeField]
    protected float maxLifeTime = 10;
    [SerializeField]
    protected LayerMask targetLayers;
    [SerializeField]
    protected LayerMask thrownLayers;
    [SerializeField]
    protected float knockback;
    [SerializeField]
    protected float hitboxRadius = 0.5f;
    [SerializeField]
    private AnimationCurve deathcurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));

    ContactFilter2D filter = new ContactFilter2D();

    protected Vector2 direction;

    private float lifeTime;

    private bool dead;
    private float deathCounter;
    private float deathDuration;

    private bool convertedToKinetic;

    private Vector3 deadVelocity;

    public System.Action<float> onDamageDealt;

    public virtual void SetUp(Vector2 dir, float damage = -1, Vector2 momentum = new Vector2())
    {
        if (damage >= 0)
        {
            this.damage = damage;
        }

        direction = (dir.normalized * speed + momentum).normalized;
        UpdateVelocity();

        deathDuration = skin.ps.duration;
        lifeTime = maxLifeTime;

        filter.layerMask = targetLayers;
        filter.useLayerMask = true;

        if (useKineticSkin)
        {
            kineticSkin.SetActive(false);
        }
    }

    private void Update()
    {
        Timers();

        if (!dead)
        {
            CheckCollision();

            AliveUpdate();
        }
        else
        {
            DeadUpdate();
        }
    }

    private void Timers()
    {
        Timer.ReduceCooldown(ref lifeTime, Die);
        if (dead)
        {
            Timer.ReduceCooldown(ref deathCounter, () =>
            {
                Destroy(gameObject);
            });
        }
    }

    protected virtual void DeadUpdate()
    {
        deadVelocity *= (1 - Time.deltaTime / 0.02f * 0.3f);
        transform.position += deadVelocity * Time.deltaTime;

        float timer = deathcurve.Evaluate(1 - (deathCounter / deathDuration));

        if (skin.light) skin.light.color = new Color(skin.light.color.r, skin.light.color.g, skin.light.color.b, timer);
        skin.sr.color = new Color(skin.sr.color.r, skin.sr.color.g, skin.sr.color.b, timer);
        skin.sr.transform.localScale = Vector3.one * timer;
        if (skin.tr) skin.tr.startWidth = timer / 2;
    }

    protected virtual void AliveUpdate()
    {
        if (kineticState != KineticState.Grabbed)
        {
            UpdateVelocity();
        }
    }

    #region Collisions

    private void CheckCollision()
    {
        Collider2D[] results = new Collider2D[1];

        if (Physics2D.OverlapCircle(kTransform.position, hitboxRadius, filter, results) > 0)
        {
            IHittable hit = results[0]?.attachedRigidbody?.GetComponent<IHittable>();

            if (hit != null)
            {
                if (!hit.AvoidingLayer(gameObject.layer))
                {
                    hit.OnHit(kTransform.position, damage, knockback, gameObject.layer);
                    onDamageDealt?.Invoke(damage * hit.LifeRegenMultiplier);
                    Crash();
                }
            }
        }

        if (!dead)
        {
            if (IsInsideOfWall())
            {
                Crash();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!dead)
        {
            OnOtherCollisionEnter(collision);
        }
    }

    protected virtual void OnOtherCollisionEnter(Collision2D collision)
    {
        Crash();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!dead)
        {
            OnOtherCollisionStay(collision);
        }
    }
    protected virtual void OnOtherCollisionStay(Collision2D collision)
    {
        Crash();
    }

    private bool IsInsideOfWall()
    {
        bool r = false;

        Collider2D target = Physics2D.OverlapPoint(kTransform.position, LayerMask.GetMask("Map"));

        if (target != null)
        {
            if (!target.isTrigger)
            {
                r = true;
            }
        }

        return r;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    protected void Crash()
    {
        skin.explosion.Emit(20);
        Die();
    }

    protected void Die() 
    {
        if (!dead)
        {
            deadVelocity = rb.velocity;

            // Turn off components
            rb.bodyType = RigidbodyType2D.Static;
            collider.enabled = false;
            if (skin.ps) skin.ps.enableEmission = false;
            if (skin.tr) skin.tr.enabled = false;
            if (skin.aura) skin.aura.SetActive(false);

            // Add set death sentence
            deathCounter = deathDuration;
            dead = true;
        }
    }

    private void ConvertToKinetic()
    {
        skin.SetActive(false);
        kineticSkin.SetActive(true);

        skin.CopySkin(kineticSkin);
    }

    protected void UpdateVelocity()
    {
        rb.velocity = direction.normalized * speed;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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
        direction = rb.velocity.normalized * speed;
        UpdateVelocity();
    }

    public virtual void Grab()
    {
        filter.layerMask = thrownLayers;

        lifeTime = maxLifeTime;

        if (useKineticSkin && !convertedToKinetic)
        {
            convertedToKinetic = true;

            ConvertToKinetic();
        }
    }

    public virtual void Throw(Vector2 direction, float force)
    {
        this.direction = direction.normalized * speed;
        UpdateVelocity();
    }

    #endregion

}
