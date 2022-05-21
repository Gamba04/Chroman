using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class Box : MonoBehaviour, IHittable
{
    [Header("Components")]
    [SerializeField]
    protected SpriteRenderer sr;
    [SerializeField]
    protected Rigidbody2D rb;
    [SerializeField]
    protected new Collider2D collider;

    [Header("Settings")]
    [SerializeField]
    protected float life;
    [SerializeField]
    private bool takeDamage;
    [SerializeField]
    private Color damageColor;
    [SerializeField]
    [Range(0.1f, 1)]
    private float damageDuration = 0.1f;
    [SerializeField]
    private AnimationCurve damageCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f,1), new Keyframe(1,0));
    [SerializeField]
    private AnimationCurve deathcurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField]
    private Color originalColor;

    [Header("Heals")]
    [SerializeField]
    private int healSpawns;
    [SerializeField]
    private float healSpawnRange = 1;
    [SerializeField]
    private float healSpawnSpeed = 3;

    private float damageCooldown;

    protected bool dead;
    private float deathCounter;
    protected float deathDuration;

    private Vector2 lastValidPosition;

    private bool isActiveOnDistance;

    protected virtual void Start()
    {
        if (Application.isPlaying)
        {
            damageCooldown = 0;
            UpdateDamage();

            if (damageDuration <= 0)
            {
                damageDuration = 0.1f;
            }

            if (deathDuration <= 0)
            {
                deathDuration = 0.1f;
            }

            StartCoroutine(IsActiveOnDistanceLoop());
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (isActiveOnDistance)
            {
                if (!dead)
                {
                    if (damageCooldown > 0)
                    {
                        UpdateDamage();
                    }

                    InsideWallDebugUpdate();

                    AliveUpdate();
                }
                else
                {
                    DeathUpdate();
                }
            }
        }
        else
        {
            EditorUpdate();
        }
    }

    private void UpdateDamage()
    {
        if (takeDamage)
        {
            Timer.ReduceCooldown(ref damageCooldown);

            float damageProgress = damageDuration - damageCooldown;

            sr.color = Color.Lerp(originalColor, damageColor, damageCurve.Evaluate(damageProgress / damageDuration));
        }
    }

    #region Death

    private void DeathUpdate()
    {
        Timer.ReduceCooldown(ref deathCounter, () =>
        {
            Destroy(gameObject);
        });

        float timer = deathcurve.Evaluate(1 - (deathCounter / deathDuration));

        DeathAnim(timer);
    }

    /// <param name="timer"> Follows deathCurve. </param>
    protected virtual void DeathAnim(float timer) { }

    /// <summary> Start death. </summary>
    protected virtual void Die()
    {
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.BoxDead);

        GameManager.SpawnHealsAtPos(healSpawns, transform.position, healSpawnRange, healSpawnSpeed);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region IHittable

    public virtual void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        if (takeDamage)
        {
            if (!dead)
            {
                life -= damage;
                if (life <= 0)
                {
                    life = 0;

                    Die();
                    //onDeath?.Invoke();

                    // Add set death sentence
                    deathCounter = deathDuration;
                    dead = true;
                }
                else
                {
                    damageCooldown = damageDuration;
                    AudioPlayer.PlaySFX(AudioPlayer.SFXTag.BoxHit, transform.position);
                }
            }
        }
    }

    public bool AvoidingLayer(int layer)
    {
        return false;
    }

    public float LifeRegenMultiplier { get => takeDamage? 10f/6f : 0; }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    protected virtual void AliveUpdate()
    {

    }

    private bool IsInsideOfWall()
    {
        bool r = false;

        Collider2D target = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Map"));

        if (target != null)
        {
            if (!target.isTrigger)
            {
                r = true;
            }
        }

        return r;
    }

    private void InsideWallDebugUpdate()
    {
        if (IsInsideOfWall())
        {
            transform.position = lastValidPosition;
        }
        else
        {
            lastValidPosition = transform.position;
        }
    }

    private void SetActiveOnDistance(bool active)
    {
        if (active)
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
        else
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }
        }
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        SetActiveOnDistance(GameManager.IsOnActiveDistance(transform.position));

        while (true)
        {
            bool newActive = GameManager.IsOnActiveDistance(transform.position);

            if (newActive != isActiveOnDistance)
            {
                isActiveOnDistance = newActive;

                SetActiveOnDistance(newActive);
            }

            yield return new WaitForSeconds(1);
        }
    }

    private void EditorUpdate()
    {
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    #endregion

}
