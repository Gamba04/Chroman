using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("General Variables")]
    [SerializeField]
    protected Rigidbody2D rb;
    [SerializeField]
    protected Animator anim;

    [Space()]
    [SerializeField]
    protected Vector2 spawnPosition;
    [SerializeField]
    protected float maxHealth;
    [SerializeField]
    protected int healthDivisions;

    [Space()]
    [SerializeField]
    protected float deathDuration = 4;
    [SerializeField]
    protected AnimationCurve deathCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Space()]
    [SerializeField]
    protected float moveAcceleration = 100;
    [SerializeField]
    protected float moveDeacceleration = 10;
    [SerializeField]
    protected float moveSpeed = 6;
    [SerializeField]
    [Range(0, 1)]
    protected float moveSteadyTreshold = 0.1f;

    [Space()]
    [ReadOnly, SerializeField]
    protected bool awakened;
    [ReadOnly, SerializeField]
    protected float health;
    [ReadOnly, SerializeField]
    protected Player player;
    [ReadOnly, SerializeField]
    protected new CameraController camera;
    [ReadOnly, SerializeField]
    protected bool dead;
    [ReadOnly, SerializeField]
    protected Vector2 velocity;

    protected float lastHealthDivision;
    protected float deathCounter;

    protected int CurrentHealthDivision
    {
        get
        {
            float separation = maxHealth / healthDivisions * 1f;

            for (int i = 0; i <= healthDivisions; i++)
            {
                if (health <= i * separation)
                {
                    return i;
                }
            }

            return healthDivisions;
        }
    }

    public bool Awakened { get => awakened; }
    public float Health { get => health; }
    public float MaxHealth { get => maxHealth; }

    protected virtual void Start()
    {
        player = GameManager.Player;
        camera = GameManager.CameraController;

        ResetBoss();

        lastHealthDivision = CurrentHealthDivision;
    }

    private void Update()
    {
        if (!GameManager.GamePaused)
        {
            if (player != null)
            {
                GeneralUpdate();

                if (awakened)
                {
                    if (!dead)
                    {
                        AliveUpdate();
                    }
                    else
                    {
                        Timer.ReduceCooldown(ref deathCounter, () =>
                        {
                            Destroy(gameObject);
                        });

                        float timer = deathCurve.Evaluate(1 - (deathCounter / deathDuration));

                        DeadUpdate(timer);
                    }
                }
                else
                {
                    SleepingUpdate();
                }
            }
        }
    }

    protected virtual void AliveUpdate()
    {

    }

    protected virtual void DeadUpdate(float timer)
    {

    }

    protected virtual void SleepingUpdate()
    {

    }

    protected virtual void GeneralUpdate()
    {

    }

    #region Mechanics

    protected virtual void Move(Vector2 direction)
    {
        direction = direction.normalized;

        // Acceleration
        if (direction.magnitude != 0)
        {
            velocity += direction * moveAcceleration * Time.deltaTime;
        }

        // Deacceleration
        if (velocity.magnitude > moveSteadyTreshold)
        {
            if (direction.magnitude == 0)
            {
                velocity *= 1 - Time.deltaTime * moveDeacceleration;
            }
        }
        else
        {
            velocity = Vector2.zero;
        }

        // Speed Limit
        if (velocity.magnitude > moveSpeed)
        {
            velocity = velocity.normalized * moveSpeed;
        }

        rb.velocity = velocity;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public virtual void AwakeBoss()
    {
        awakened = true;
        GameManager.SetBossHealthBar(true);
        AdaptativeMusic.SetPitch(1.2f);
    }

    public virtual void ResetBoss()
    {
        dead = false;
        anim.SetBool("Sleeping", true);
        transform.position = spawnPosition;
        rb.position = spawnPosition;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic;
        health = maxHealth;

        UpdateHealthState();

        awakened = false;
    }

    public virtual void StealCamera()
    {
        camera.SetTarget(transform);
    }

    public virtual void StealCamera(Transform customTransform)
    {
        camera.SetTarget(customTransform);
    }

    public void ReturnControl()
    {
        camera.SetTarget(player.transform);
        player.SetImmobile(false);
    }

    public void ActivateRoomZoom(int area)
    {
        camera.ActivateArea(area);
    }

    public void TurnOffVol()
    {
        AdaptativeMusic.MasterVolumeTransition(0);
    }

    public void TurnOnVol()
    {
        AdaptativeMusic.MasterVolumeTransition(AdaptativeMusic.DefaultMasterVolume);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Others

    protected virtual void Die()
    {
        dead = true;
        GameManager.bossKilled = true;
        player.SetImmobile(true);
        rb.bodyType = RigidbodyType2D.Static;
        StealCamera();
        AdaptativeMusic.MasterVolumeTransition(0);
        GameManager.SetBossHealthBar(false);
    }

    public virtual void AddHealth(float ammount)
    {
        health += ammount;

        if (health <= 0)
        {
            Die();
        }
    }

    public void UpdateHealthState()
    {
        int division = CurrentHealthDivision;

        if (division != lastHealthDivision)
        {
            lastHealthDivision = division;

            // Change stage
            if (awakened)
            {
                GameManager.BossHealthBarChangeState();

                OnStageChange(division);
            }
        }
    }

    protected virtual void OnStageChange(int newDivision)
    {
        
    }

    #endregion

}
