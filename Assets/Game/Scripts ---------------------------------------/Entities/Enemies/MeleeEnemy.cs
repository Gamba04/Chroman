using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy, IKinetic
{
    [GambaHeader("Melee Enemy ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    protected float knockback;
    [SerializeField]
    private Transform saw;
    [SerializeField]
    private float sawAttackSpeed = 1080;
    [SerializeField]
    private float sawBaseSpeed = 90;
    [SerializeField]
    private float damageOnContactRadius = 0.5f;
    [SerializeField]
    private Color kineticColor;
    [ReadOnly, SerializeField]
    private float sawSpeed;

    private float targetSawSpeed;

    private LayerMask hitLayers;

    private CollisionInfo lastCollision;

    protected override void Start()
    {
        base.Start();

        targetSawSpeed = sawBaseSpeed;

        hitLayers = LayerMask.GetMask("Player");
    }

    protected override void OnAliveUpdate()
    {
        if (saw != null)
        {
            if (sawSpeed != targetSawSpeed)
            {
                sawSpeed += (targetSawSpeed - sawSpeed) * 0.1f * Time.deltaTime / 0.02f;
            }

            if (sawSpeed != 0)
            {
                saw.Rotate(0, 0, Time.deltaTime * sawSpeed);
            }

            DamageOnContact();
        }
    }

    #region Attack

    private void DamageOnContact()
    {
        if (kineticState != KineticState.Grabbed)
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll(kTransform.position, damageOnContactRadius, hitLayers);

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != collider)
                {
                    IHittable hit = targets[i]?.attachedRigidbody?.GetComponent<IHittable>();

                    if (hit != null)
                    {
                        if (!hit.AvoidingLayer(gameObject.layer))
                        {
                            hit.OnHit(kTransform.position, damage, knockback, gameObject.layer);
                            hitLayers = LayerMask.GetMask();
                            break;
                        }
                    }
                }
            }
        }
    }

    protected override void Hit()
    {
        StartDash(player.transform.position - kTransform.position);
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.MeleeEnemy, kTransform.position);
        targetSawSpeed = sawAttackSpeed;
    }

    protected override void DashExit()
    {
        base.DashExit();

        targetSawSpeed = sawBaseSpeed;
        hitLayers = LayerMask.GetMask("Player");
        light.color = lightDefaultColor;

        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Collisions

    protected override void OnNewCollisionEnter2D(Collision2D collision)
    {
        base.OnNewCollisionEnter2D(collision);

        if (state == EnemyState.Dash)
        {
            // Debug
            lastCollision.inputDir = dashDirection;
            lastCollision.point = collision.GetContact(0).point;
            lastCollision.normal = collision.GetContact(0).normal;

            Bounce(collision);

            // Debug
            lastCollision.outputDir = dashDirection;
        }
    }

    protected override void OnNewCollisionStay2D(Collision2D collision)
    {
        base.OnNewCollisionStay2D(collision);

        if (state == EnemyState.Dash)
        {
            Bounce(collision);
        }
    }

    private void Bounce(Collision2D collision)
    {
        dashDirection = GambaFunctions.Bounce(collision.GetContact(0).normal, dashDirection);

        rb.velocity = dashDirection.normalized * dashSpeed;
    }

    protected override void InsideWallDebugUpdate()
    {
        if (kineticState != KineticState.Grabbed)
        {
            base.InsideWallDebugUpdate();
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    protected override void Die()
    {
        base.Die();

        if (saw != null)
        {
            saw.gameObject.SetActive(false);
        }

        ghosting.Disable();
    }

    private void OnDrawGizmos()
    {
        if (state == EnemyState.Dash)
        {
            GambaFunctions.GizmosDrawArrow(kTransform.position, dashDirection.normalized);
        }

        lastCollision.DebugCollision();
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

    public void Unleash()
    {
        light.color = lightDefaultColor;
        ghosting.Disable();

        lockStateChange = false;
        ChangeStateOnDelay(EnemyState.Wandering, 0.3f);

        hitLayers = LayerMask.GetMask("Player");

        attackCooldown = attackMaxCooldown;
        subAttackCooldown = 0;
    }

    public void Grab()
    {
        AnyStateExit();
        ChangeState(EnemyState.None);
        lockStateChange = true;

        light.color = kineticColor;
        ghosting.Enable(ghostingSprite, kineticColor);
        targetSawSpeed = sawBaseSpeed;

        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Throw(Vector2 direction, float force)
    {
        hitLayers = LayerMask.GetMask("Enemy", "Hittable", "Boss");

        lockStateChange = false;

        StartDash(direction);
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.MeleeEnemy, kTransform.position);
        targetSawSpeed = sawAttackSpeed;
    }

    #endregion

}
