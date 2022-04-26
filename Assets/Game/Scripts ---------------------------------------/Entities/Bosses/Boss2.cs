using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2 : Boss, IHittable
{
    private enum Boss2State
    {
        Idle,
        Searching
    }

    private enum Boss2Pattern
    {
        
    }

    [Header("Components")]
    [SerializeField]
    private Transform serpentParent;
    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private GameObject tailPrefab;
    [SerializeField]
    private Animator cinematicAnim;
    [GambaHeader("Head")]
    [SerializeField]
    private SpriteRenderer fillColor;
    [SerializeField]
    private SpriteRenderer light;
    [SerializeField]
    private SpriteRenderer whiteSprite;
    [SerializeField]
    private Transform cinematicTarget;
    [SerializeField]
    private Collider2D collider;
    [SerializeField]
    private BounceSimulator bounceSimulator;

    [GambaHeader("Settings")]
    [SerializeField]
    private bool demoBoss;
    [SerializeField]
    private int serpentLenght;
    [SerializeField]
    private float nodesSeparation = 1f;
    [SerializeField]
    private Transition hitTransition;
    [SerializeField]
    private float lightAlpha = 0.7f;
    [GambaHeader("Stats")]
    [SerializeField]
    private float damage = 1;
    [SerializeField]
    private float knockback = 1;

    [GambaHeader("Info ----------------------------------------------------------------------------------------------------------------------------------------------------------", 0.5f)]
    [ReadOnly, SerializeField]
    private Boss2State state;
    [ReadOnly, SerializeField]
    private bool cinematic;

    private List<SerpentNode> serpentNodes = new List<SerpentNode>();
    private bool damageOnContact;

    private Vector2 direction;

    public Color MainColor => fillColor.color;

    protected override void Start()
    {
        CreateSerpent();

        base.Start();
    }

    protected override void AliveUpdate()
    {
        base.AliveUpdate();

        MainUpdate();
        HitColorUpdate();
        DamageOnContact();
    }

    protected override void SleepingUpdate()
    {
        base.SleepingUpdate();

        if (cinematic)
        {
            transform.position = cinematicTarget.position;
            transform.rotation = cinematicTarget.rotation;
        }
    }

    protected override void GeneralUpdate()
    {
        LightColorUpdate();
        UpdateSerpent();
    }

    #region States

    private void MainUpdate()
    {
        switch (state)
        {
            case Boss2State.Idle:
                Move(Vector2.zero);
                break;
            case Boss2State.Searching:
                Searching();
                break;
        }
    }

    private void ChangeState(Boss2State state)
    {
        this.state = state;
    }

    private void ChangeStateOnDelay(Boss2State state, float delay)
    {
        ChangeState(Boss2State.Idle);
        Timer.CallOnDelay(() =>
        {
            ChangeState(state);
        }, delay);
    }

    private void Searching()
    {
        direction = Vector3.Lerp(direction, (bounceSimulator.transform.position - transform.position).normalized, 0.05f * Time.deltaTime/0.02f);
        Move(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, direction), 0.3f);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region GeneralUpdates

    private void LightColorUpdate()
    {
        light.color = new Color(fillColor.color.r, fillColor.color.g, fillColor.color.b, fillColor.color.a * lightAlpha);
    }

    private void HitColorUpdate()
    {
        hitTransition.UpdateTransitionValue();
        if (hitTransition.IsOnTransition)
        {
            whiteSprite.color = new Color(whiteSprite.color.r, whiteSprite.color.g, whiteSprite.color.b, hitTransition.value);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

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

    #region Serpent

    private void CreateSerpent()
    {
        serpentNodes.Clear();

        for (int i = 0; i < serpentLenght; i++)
        {
            GameObject newObj = Instantiate((i < serpentLenght - 1)? nodePrefab : tailPrefab, serpentParent);
            newObj.name = i < serpentLenght - 1? $"SerpentNode {i}" : "Tail";
            SerpentNode node = newObj.GetComponent<SerpentNode>();

            if (node != null)
            {
                if (i > 0)
                {
                    node.leadNode = serpentNodes[i - 1].transform;
                }
                else
                {
                    node.leadNode = transform;
                }
                
                node.Setup(this, -i, nodesSeparation, damage, knockback);
                serpentNodes.Add(node);
            }
        }
    }

    private void UpdateSerpent()
    {
        for (int i = 0; i < serpentNodes.Count; i++)
        {
            serpentNodes[i].UpdateNode();
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void StartCinematic()
    {
        cinematic = true;
        cinematicAnim.SetTrigger("Start");
        Timer.CallOnDelay(()=> AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Boss2Intro), 1);
        AdaptativeMusic.MasterVolumeTransition(0);
    }

    public void EndCinematic()
    {
        if (!demoBoss)
        {
            cinematic = false;

            player.SetImmobile(false);
            AdaptativeMusic.MasterVolumeTransition(AdaptativeMusic.DefaultMasterVolume);

            AwakeBoss();
        }
        else
        {
            GameManager.PlayWinScreen();
            GameManager.bossKilled = true;
        }
    }

    public override void AwakeBoss()
    {
        base.AwakeBoss();

        SetColliders(true);

        ChangeState(Boss2State.Searching);
        direction = transform.up;
        bounceSimulator.transform.position = transform.position + (Vector3)direction.normalized * 20f;
        bounceSimulator.StartSimulation(direction);
    }

    public override void ResetBoss()
    {
        base.ResetBoss();

        camera.DeactivateArea(2);

        transform.position = spawnPosition;

        cinematicAnim.SetTrigger("Reset");

        SetColliders(false);
        damageOnContact = true;

        for (int i = 0; i < serpentNodes.Count; i++)
        {
            serpentNodes[i].transform.position = spawnPosition;
            serpentNodes[i].SetDamageOnContact(true);
        }

        bounceSimulator.ResetProbe(transform.position);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Death

    protected override void DeadUpdate(float timer)
    {
        base.DeadUpdate(timer);
    }

    protected override void Die()
    {
        base.Die();

        Timer.CallOnDelay(() =>
        {
            GameManager.PlayWinScreen();
        }, 2);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region IHittable

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        HitColorTransition();

        AddHealth(-damage);
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyHit);

        UpdateHealthState();
    }

    public float LifeRegenMultiplier { get => 1; }

    public bool AvoidingLayer(int layer)
    {
        return !awakened;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Others

    public void HitColorTransition()
    {
        hitTransition.value = 0;
        hitTransition.StartTransition(1);
    }

    private void SetColliders(bool value)
    {
        collider.enabled = value;

        for (int i = 0; i < serpentNodes.Count; i++)
        {
            serpentNodes[i].SetCollider(value);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       /* if (state == Boss2State.Searching)
        {
            if (collision.collider.gameObject.layer == 11) // Map
            {
                direction = GambaFunctions.Bounce(collision.GetContact(0).normal, direction);

                Move(direction);
            }
        }*/
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        /*if (state == Boss2State.Searching)
        {
            if (collision.collider.gameObject.layer == 11) // Map
            {
                direction = GambaFunctions.Bounce(collision.GetContact(0).normal, direction);

                Move(direction);
            }
        }*/
    }

    #endregion

    private void OnDrawGizmos()
    {
        for (int i = 0; i < serpentNodes.Count; i++)
        {
            if (serpentNodes[i].leadNode != null)
            {
                Gizmos.DrawRay(serpentNodes[i].transform.position, serpentNodes[i].direction);
            }
        }
    }
}