using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Turret : MonoBehaviour, IHittable
{
    [Header("Components")]
    [SerializeField]
    private SpriteRenderer sr;
    [SerializeField]
    protected Animator anim;
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private Collider2D collider;
    [SerializeField]
    private SpriteRenderer light;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject cannon;
    [SerializeField]
    private SpriteRenderer[] whiteSprites;

    [Header("Settings")]
    [GambaHeader("Combat", 1, 0.1f, 0.1f)]
    [SerializeField]
    private bool respondToRange;
    [SerializeField]
    private bool lockDirection;
    [SerializeField]
    private bool takeDamage;
    [SerializeField]
    private Vector2 direction;
    [SerializeField]
    private float range = 10;
    [SerializeField]
    private float rateOfFire = 1;
    [SerializeField]
    private float damage = 1;
    [Space()]
    [GambaHeader("Other", 0.7f)]
    [SerializeField]
    private AnimationCurve deathcurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField]
    private UnityEvent onDeadEvent;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private float distanceToPlayer;
    [ReadOnly, SerializeField]
    private bool isActiveOnDistance;
    [ReadOnly, SerializeField]
    private bool active = true;
    [ReadOnly, SerializeField]
    private Player player;
    [ReadOnly, SerializeField]
    private float health = 6;
    [ReadOnly, SerializeField]
    private bool dead;

    private float deathCounter;
    private float deathDuration;



    private void Start()
    {
        StartCoroutine(IsActiveOnDistanceLoop());
        SetActiveOnDistance(isActiveOnDistance);

        StartCoroutine(ShootingLoop());

        player = GameManager.Player;

        deathDuration = explosion.duration;
    }

    private void Update()
    {
        if (!GameManager.GamePaused)
        {
            if (player != null)
            {
                if (!dead)
                {
                    if (isActiveOnDistance && active)
                    {
                        LookAtPlayerUpdate();
                        RotationUpdate();
                    }
                }
                else
                {
                    DeathUpdate();
                }
            }
            else
            {
                Debug.LogError("Player not found");
            }
        }
    }

    #region Mechanics

    private void Attack()
    {
        if (!dead && !player.Dead)
        {
            anim.SetBool("Cancel", false);
            anim.SetTrigger("Attack");

            Timer.CallOnDelay(Shoot, 0.5f, "Turret: Shoot");
        }
    }

    private void Shoot()
    {
        GameObject bullet;

        if (GameManager.ParentBullets != null)
        {
            bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction.normalized * 0.2f, transform.rotation, GameManager.ParentBullets);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction.normalized * 0.2f, transform.rotation);
        }

        bullet.GetComponent<Bullet>()?.SetUp(direction, damage);

        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.RangeEnemy, transform.position);
    }

    private void LookAtPlayerUpdate()
    {
        if (RangeCheck() && !lockDirection)
        {
            direction = player.transform.position - transform.position;
        }
    }

    private void RotationUpdate()
    {
        if (sr != null)
        {
            sr.transform.rotation = Quaternion.Slerp(sr.transform.rotation, Quaternion.LookRotation(Vector3.forward, direction), 0.2f * Time.deltaTime / 0.02f);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Death

    private void DeathUpdate()
    {
        Timer.ReduceCooldown(ref deathCounter, () =>
        {
            Destroy(gameObject);
        });

        float timer = deathcurve.Evaluate(1 - (deathCounter / deathDuration));

        light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
    }

    private void Die()
    {
        explosion.Emit(20);

        // Turn off components
        sr.enabled = false;
        collider.enabled = false;
        cannon.SetActive(false);
        foreach (SpriteRenderer w in whiteSprites)
        {
            w.enabled = false;
        }

        // Add set death sentence
        deathCounter = deathDuration;
        dead = true;

        onDeadEvent?.Invoke();
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    public void SetActive(bool value)
    {
        active = value;
    }

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        if (takeDamage)
        {
            anim.SetBool("Cancel", false);
            anim.SetTrigger("Hit");

            AddHealth(-damage);
            AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyHit);
        }
    }

    public bool AvoidingLayer(int layer)
    {
        return false; // !takeDamage
    }

    public float LifeRegenMultiplier => 1;

    private void AddHealth(float ammount)
    {
        health += ammount;

        if (health <= 0)
        {
            AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyDead);
            Die();
        }
    }

    private bool RangeCheck()
    {
        distanceToPlayer = (player.transform.position - transform.position).magnitude;

        bool r = true;

        if (respondToRange && player != null) 
        {
            if ((!IsInLineOfSight(player.transform.position) && !lockDirection) || distanceToPlayer > range)
            {
                r = false;
            }
        }

        return r;
    }

    private bool IsInLineOfSight(Vector2 obj)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, obj - (Vector2)transform.position, (obj - (Vector2)transform.position).magnitude);

        for (int i = 0; i < hits.Length; i++)
        {
            int hitLayer = hits[i].collider.gameObject.layer;
            if (hitLayer == 11 || hitLayer == 0 || hitLayer == 9 || hitLayer == 15) // Map, Default, Hittable, Laser
            {
                bool activeLaser = false;
                if (hitLayer == 15)
                {
                    Laser laser = hits[i].collider.GetComponentInParent<Laser>();

                    if (laser != null)
                    {
                        if (laser.Activated)
                        {
                            activeLaser = true;
                        }
                    }
                }
                if (!hits[i].collider.isTrigger || activeLaser) // Laser exception
                {
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerator ShootingLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f / rateOfFire);

            if (!dead)
            {
                if (RangeCheck() && active)
                {
                    Attack();
                }
            }
        }
    }

    private void SetActiveOnDistance(bool value)
    {
        anim.enabled = value;
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            bool newActive = GameManager.IsOnActiveDistance(transform.position);

            if (newActive != isActiveOnDistance)
            {
                isActiveOnDistance = newActive;

                SetActiveOnDistance(newActive);
            }
        }
    }

    #endregion

    private Vector2 lastLookDirection;

    private void OnValidate()
    {
        if (rateOfFire <= 0)
        {
            rateOfFire = 0.1f;
        }

        if (direction != lastLookDirection)
        {
            lastLookDirection = direction;

            if (sr != null)
            {
                sr.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            }
        }
    }

}