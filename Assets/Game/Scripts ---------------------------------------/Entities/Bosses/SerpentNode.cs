using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SerpentNode : MonoBehaviour, IHittable
{
    public enum NodeState
    {
        Off,
        Base,
        Cannon,
        Laser
    }

    [Header("Components")]
    [SerializeField]
    public Boss2 boss;
    [SerializeField]
    public Transform leadNode;
    [SerializeField]
    public SortingGroup sGroup;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private SpriteRenderer colorFill;
    [SerializeField]
    private new SpriteRenderer light;
    [SerializeField]
    private SpriteRenderer whiteSprite;

    [Header("Settings")]
    [SerializeField]
    public float separation;
    [SerializeField]
    private Transition hitTransition;
    [SerializeField]
    private Transition alphaTransition;
    [SerializeField]
    private float lightAlpha = 0.7f;

    [GambaHeader("Info ----------------------------------------------------------------------------------------------------------------------------------------------------------", 0.5f)]
    [ReadOnly, SerializeField]
    private NodeState state;
    [ReadOnly, SerializeField]
    private Vector3 targetPosition;

    private bool damageOnContact;

    private float damage;
    private float knockback;

    public Vector3 direction { get; private set; }

    public void Setup(Boss2 boss, int sortingOrder, float separation, float damage, float knockback)
    {
        this.boss = boss;
        this.separation = separation;
        this.damage = damage;
        this.knockback = knockback;

        sGroup.sortingOrder = sortingOrder;
        targetPosition = leadNode.position;
    }

    public void UpdateNode()
    {
        MainUpdate();

        PositionUpdate();
        ColorUpdate();
        DamageOnContact();
    }

    #region States

    private void MainUpdate()
    {
        switch (state)
        {
            case NodeState.Off:

                break;
            case NodeState.Base:

                break;
            case NodeState.Cannon:

                break;
            case NodeState.Laser:

                break;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region General Updates

    public void PositionUpdate()
    {
        float leadProgress = (leadNode.position - targetPosition).magnitude;

        if (leadProgress >= separation)
        {
            targetPosition = leadNode.position;
            leadProgress = 0;
        }

        direction = (targetPosition - transform.position);

        transform.position = targetPosition - direction.normalized * (separation - leadProgress);

        Quaternion directLookRotation = Quaternion.LookRotation(Vector3.forward, leadNode.position - transform.position);
        Quaternion indirectLookRotation = Quaternion.LookRotation(Vector3.forward, direction);

        transform.rotation = Quaternion.Lerp(indirectLookRotation, directLookRotation, leadProgress / separation);
    }

    public void ColorUpdate()
    {
        Color col = boss.MainColor;

        alphaTransition.UpdateTransitionValue();
        if (alphaTransition.IsOnTransition)
        {
            col.a = boss.MainColor.a * alphaTransition.value;
        }

        colorFill.color = col;
        light.color = new Color(col.r, col.g, col.b, col.a * lightAlpha);

        hitTransition.UpdateTransitionValue();
        if (hitTransition.IsOnTransition)
        {
            whiteSprite.color = new Color(whiteSprite.color.r, whiteSprite.color.g, whiteSprite.color.b, hitTransition.value);
        }
    }

    private void DamageOnContact()
    {
        if (damageOnContact)
        {
            List<Collider2D> targets = new List<Collider2D>();

            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Player");
            filter.useLayerMask = true;

            collider.OverlapCollider(filter, targets);

            for (int i = 0; i < targets.Count; i++)
            {
                IHittable hit = targets[i]?.attachedRigidbody?.GetComponent<IHittable>();

                if (hit != null)
                {
                    if (!hit.AvoidingLayer(gameObject.layer))
                    {
                        hit.OnHit(transform.position, damage, knockback, gameObject.layer);
                    }
                }
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region IHittable

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        HitColorTransition();

        boss.AddHealth(-damage);
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyHit);

        boss.UpdateHealthState();
    }

    public float LifeRegenMultiplier { get => 1; }

    public bool AvoidingLayer(int layer)
    {
        return !boss.Awakened;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetCollider(bool value)
    {
        collider.enabled = value;
    }

    public void SetDamageOnContact(bool value)
    {
        damageOnContact = value;
    }

    public void UpdateStage()
    {

    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Others

    private void HitColorTransition()
    {
        hitTransition.value = 0;
        hitTransition.StartTransition(1);
    }

    private void DeactivateLight()
    {
        alphaTransition.StartTransition(0);
    }

    #endregion

}