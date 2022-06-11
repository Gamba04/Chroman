using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss1 : Boss, IHittable
{
    [System.Serializable]
    private class CirclePattern
    {
        public Vector2 center;
        public float radius;

        public Vector2 ClosestPoint(Vector2 position)
        {
            Vector2 toPoint = position - center;

            return center + toPoint.normalized * radius;
        }

        public Vector2 TangentDirection(Vector2 position, bool clockwise)
        {
            Vector2 toPoint = position - center;

            if (clockwise)
            {
                return new Vector2(toPoint.y, -toPoint.x).normalized;
            }
            else
            {
                return new Vector2(-toPoint.y, toPoint.x).normalized;
            }
        }
    }

    [System.Serializable]
    private class Cannon
    {
        [SerializeField, HideInInspector] private string name;

        public Transform cannon;
        public Animator anim;

        public void Shoot(GameObject prefab)
        {
            anim.SetTrigger("Shoot");

            if (prefab != null)
            {
                if (prefab.GetComponent<SawBullet>() != null)
                {
                    Timer.CallOnDelay(() =>
                    {
                        if (cannon != null)
                        {
                            GameObject bullet = Instantiate(prefab, cannon.position + cannon.up * 6f, cannon.rotation, GameManager.ParentBullets);

                            SawBullet saw = bullet.GetComponent<SawBullet>();

                            saw.SetUp(cannon.up);
                        }
                    }, 0.75f);
                }
            }
        }

        public void SetName(int index)
        {
            name = $"Cannon_{index}";
        }
    }

    private enum Boss1State
    {
        Idle,
        Stunned,
        Returning,
        Pattern,
        Centering,
        Dash
    }

    private enum Boss1Pattern
    {
        Shooting,
        Dashing,
        Bouncing
    }

    [Header("Components")]
    [SerializeField]
    private Animator headAnim;
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private Transform saw;
    [SerializeField]
    private Ghosting ghosting;
    [SerializeField]
    private SpriteRenderer fill;
    [SerializeField]
    private GameObject meleeEnemyPrefab;
    [SerializeField]
    private SpriteRenderer damageSprite;
    [SerializeField]
    private new SpriteRenderer light;
    [GambaHeader("Cannons")]
    [SerializeField]
    private Animator cannonsAnim;
    [SerializeField]
    private Transform cannonsRoot;
    [SerializeField]
    private List<Cannon> cannons;
    [SerializeField]
    private GameObject sawPrefab;
    [SerializeField]
    private GameObject cannonsParent;

    [Header("Settings")]
    [SerializeField]
    private int maxSpawnedEnemies = 10;
    [SerializeField]
    private Sprite ghostingSprite;
    [SerializeField]
    private float cannonsRotationSpeed;
    [GambaHeader("Damage")]
    [SerializeField]
    private Color damageColor;
    [SerializeField]
    private AnimationCurve damageCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    [SerializeField]
    [Range(0.1f, 1)]
    private float damageDuration = 0.1f;
    [GambaHeader("Movement")]
    [SerializeField]
    protected float dashSpeed;
    [SerializeField]
    private float dashDuration;
    [SerializeField]
    private float stunDuration;
    [GambaHeader("Stats at MaxHealth", 0.3f, 1, 0.3f)]
    [SerializeField]
    private float easySpeed = 6;
    [SerializeField]
    private float easyDashSpeed = 15;
    [SerializeField]
    private float easyAttackPreWarm = 1.5f;
    [SerializeField]
    private float easyStunDuration = 5;
    [GambaHeader("Stats at 1", 1, 0.3f, 0.3f)]
    [SerializeField]
    private float hardSpeed = 10;
    [SerializeField]
    private float hardDashSpeed = 30;
    [SerializeField]
    private float hardAttackPreWarm = 0.5f;
    [SerializeField]
    private float hardStunDuration = 2;

    [Header("Patterns")]
    [SerializeField]
    private CirclePattern circularPattern;
    [SerializeField]
    private int patternDir = 1;
    [SerializeField]
    private int maxBounces = 10;
    [GambaHeader("Stats")]
    [SerializeField]
    private float damage = 1;
    [SerializeField]
    private float knockback = 40;
    [SerializeField]
    private float rateOfFire = 1;
    [SerializeField]
    private float rateOfSpawn = 1;
    [SerializeField]
    private float attackPreWarm = 1.5f;
    [GambaHeader("Saw")]
    [SerializeField]
    private float sawSpeed_Base = 90;
    [SerializeField]
    private float sawSpeed_Dash = 1080;
    [SerializeField]
    private float sawSpeed_Stunned = 30;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private Boss1State state;
    [ReadOnly, SerializeField]
    private Boss1Pattern pattern;
    [ReadOnly, SerializeField]
    private float sawSpeed;
    [ReadOnly, SerializeField]
    private float dashCooldown;
    [ReadOnly, SerializeField]
    private float patternCooldown;
    [ReadOnly, SerializeField]
    private bool shooting;
    [ReadOnly, SerializeField]
    private bool spawning;
    [ReadOnly, SerializeField]
    private int bouncesCounter;
    [ReadOnly, SerializeField]
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool damageOnContact;
    private Vector2 dashDirection;
    private float targetSawSpeed;
    private float stunCooldown;

    private float damageCooldown;
    private Color originalColor;

    private System.Action onReset;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(ShootingLoop());
        StartCoroutine(EnemySpawningLoop());

        if (ghosting != null)
        {
            ghosting.SetParent(GameManager.ParentGhosting);
        }
    }

    protected override void AliveUpdate()
    {
        base.AliveUpdate();

        StateControlUpdate();
        DamageOnContact();
        UpdateDamage();

        SawUpdate();
        CannonsRotationUpdate();
    }

    #region StateControl

    private void StateControlUpdate()
    {
        switch (state)
        {
            case Boss1State.Idle:
                Idle();
                break;
            // ------------------------------------------------
            case Boss1State.Stunned:
                Stunned();
                break;
            // ------------------------------------------------
            case Boss1State.Returning:
                Vector2 target = transform.position;
                switch (pattern)
                {
                    case Boss1Pattern.Dashing:
                        target = circularPattern.ClosestPoint(transform.position);

                        if ((target - (Vector2)transform.position).magnitude < 0.5f)
                        {
                            // Begin Pattern
                            ChangeStateOnDelay(Boss1State.Pattern, 0.5f);
                            patternCooldown = Random.Range(3f, 5f); // Duration of pattern
                            spawning = true;
                            ReturningExit();
                        }
                        else
                        {
                            Returning(target);
                        }
                        break;
                    case Boss1Pattern.Shooting:
                        target = circularPattern.ClosestPoint(transform.position);

                        if ((target - (Vector2)transform.position).magnitude < 0.5f)
                        {
                            // Begin Pattern
                            ChangeStateOnDelay(Boss1State.Pattern, 0.5f);
                            patternCooldown = Random.Range(10f, 20f); // Duration of pattern
                            shooting = true;
                            cannonsAnim.SetBool("Visible", true);
                            ReturningExit();
                        }
                        else
                        {
                            Returning(target);
                        }
                        break;
                    case Boss1Pattern.Bouncing:
                        Timer.CallOnDelay(() =>
                        {
                            // Begin Pattern
                            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

                            DashAttack(randomDirection);
                            bouncesCounter = 0;
                        }, 0.5f);
                        ChangeState(Boss1State.Idle);
                        ReturningExit();
                        break;
                }
                break;
            // ------------------------------------------------
            case Boss1State.Pattern:
                Pattern();
                switch (pattern)
                {
                    case Boss1Pattern.Dashing:
                        // End Pattern
                        Timer.ReduceCooldown(ref patternCooldown, () =>
                        {
                            DashAttack(player.transform);
                            PatternExit();
                        });
                        break;
                    case Boss1Pattern.Shooting:
                        // End Pattern
                        Timer.ReduceCooldown(ref patternCooldown, () =>
                        {
                            ChangeStateOnDelay(Boss1State.Centering, 0.5f);
                            PatternExit();
                        });
                        break;
                }
                break;
            // ------------------------------------------------
            case Boss1State.Centering:
                target = circularPattern.center;

                if ((target - (Vector2)transform.position).magnitude < 0.2f)
                {
                    ChangeState(Boss1State.Idle);
                    Timer.CallOnDelay(() =>
                    {
                        NewPattern();
                    }, 1f);
                    CenteringExit();
                }
                else
                {
                    Centering(target);
                }
                break;
            // ------------------------------------------------
            case Boss1State.Dash:
                GhostingUpdate();

                Dashing();
                break;
        }
    }

    private void ChangeState(Boss1State state)
    {
        this.state = state;
    }

    private void ChangeStateOnDelay(Boss1State state, float delay)
    {
        ChangeState(Boss1State.Idle);
        Timer.CallOnDelay(() =>
        {
            ChangeState(state);
        }, delay);
    }

    private void NewPattern()
    {
        int amountOfPatterns = System.Enum.GetValues(typeof(Boss1Pattern)).Length;
        Boss1Pattern newPattern = (Boss1Pattern)Random.Range(0, amountOfPatterns);

        int iters = 100;
        for (int i = 0; i < iters; i++)
        {
            if (health < maxHealth || newPattern != Boss1Pattern.Bouncing)
            {
                break;
            }

            newPattern = (Boss1Pattern)Random.Range(0, amountOfPatterns);
        }

        pattern = newPattern;

        int randomDir = Random.Range(0, 2);
        switch (randomDir)
        {
            case 0:
                patternDir = -1;
                break;
            case 1:
                patternDir = 1;
                break;
        }

        ChangeState(Boss1State.Returning);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    #region States

    private void Idle()
    {
        Move(Vector2.zero);
    }

    private void Stunned()
    {
        Timer.ReduceCooldown(ref stunCooldown, StunnedExit);
        Move(Vector2.zero);
    }

    private void Returning(Vector2 target)
    {
        Move(target - (Vector2)transform.position);
    }

    private void Pattern()
    {
        switch (pattern)
        {
            case Boss1Pattern.Dashing:
                Move(circularPattern.ClosestPoint((Vector2)transform.position + circularPattern.TangentDirection(transform.position, patternDir == 1)) - (Vector2)transform.position);
                break;
            case Boss1Pattern.Shooting:
                Move(circularPattern.ClosestPoint((Vector2)transform.position + circularPattern.TangentDirection(transform.position, patternDir == 1)) - (Vector2)transform.position);
                break;
        }
    }

    private void Centering(Vector2 target)
    {
        Move(target - (Vector2)transform.position);
    }

    private void Dashing()
    {
        Timer.ReduceCooldown(ref dashCooldown, ()=>
        {
            ChangeState(Boss1State.Centering);
            DashExit();
        });

        rb.velocity = dashDirection.normalized * dashSpeed;
    }

    private void UpdateDamage()
    {
        Timer.ReduceCooldown(ref damageCooldown);

        float damageProgress = damageDuration - damageCooldown;

        damageSprite.color = Color.Lerp(originalColor, damageColor, damageCurve.Evaluate(damageProgress / damageDuration));
    }

    //-----------------------------------------------------------------------------------------------

    private void StunnedExit()
    {
        anim.SetBool("Stunned", false);
        ChangeState(Boss1State.Centering);
        targetSawSpeed = sawSpeed_Base;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void ReturningExit()
    {
        
    }

    private void PatternExit()
    {
        switch (pattern)
        {
            case Boss1Pattern.Dashing:
                spawning = false;
                break;
            case Boss1Pattern.Shooting:
                cannonsAnim.SetBool("Visible", false);
                shooting = false;
                break;
        }
    }

    private void CenteringExit()
    {

    }

    private void DashExit()
    {
        ghosting.Disable();
        targetSawSpeed = sawSpeed_Base;
        AudioPlayer.SetSFXLoop(AudioPlayer.SFXLoopTag.BossDash, false);
        if (awakened)
        {
            AudioPlayer.PlaySFX(AudioPlayer.SFXTag.BossDashEnd);
        }
    }

    #endregion

    //-----------------------------------------------------------------------------------------------

    private void StartDash(Vector2 direction)
    {
        if (dashDuration > 0)
        {
            ChangeState(Boss1State.Dash);
            dashCooldown = dashDuration;

            ghosting.Enable(ghostingSprite, fill.color);
            rb.velocity = (direction).normalized * dashSpeed;
            dashDirection = direction.normalized;

            AudioPlayer.SetSFXLoop(AudioPlayer.SFXLoopTag.BossDash, true);
        }
        else
        {
            DashExit();
        }
    }

    private void DashAttack(Vector2 direction)
    {
        ChangeState(Boss1State.Idle);
        anim.SetTrigger("Attack");
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.BossDashWindUp);

        Timer.CallOnDelay(() =>
        {
            if (state != Boss1State.Stunned)
            {
                StartDash(direction);
            }
        }, attackPreWarm, ref onReset, "Boss Start Dash");

        targetSawSpeed = sawSpeed_Dash;
    }

    private void DashAttack(Transform target)
    {
        DashAttack(target.position - transform.position);
    }

    private void Stun()
    {
        ChangeState(Boss1State.Stunned);
        anim.SetBool("Stunned", true);
        stunCooldown = stunDuration;
        targetSawSpeed = sawSpeed_Stunned;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
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

    private void ShootSaw()
    {
        int rand = Random.Range(0, 4);

        if (rand < cannons.Count)
        {
            cannons[rand].Shoot(sawPrefab);
        }
    }

    private void SpawnEnemy()
    {
        if (meleeEnemyPrefab != null)
        {
            if (ActiveEnemies() < maxSpawnedEnemies) 
            {
                GameObject newEnemy = Instantiate(meleeEnemyPrefab, transform.position, transform.rotation);
                spawnedEnemies.Add(newEnemy);

                Enemy enemy = newEnemy.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.SetHealth(4);
                    enemy.SetWanderingRange(20);
                }
            }
        }
    }

    private void DestroySpawnedEnemies()
    {
        if (spawnedEnemies.Count > 0)
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] != null)
                {
                    Destroy(spawnedEnemies[i]);
                    spawnedEnemies.RemoveAt(i);
                }
            }

            spawnedEnemies.Clear();
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetSawSpeed(float speed)
    {
        targetSawSpeed = speed;
    }

    public override void AwakeBoss()
    {
        base.AwakeBoss();

        targetSawSpeed = sawSpeed_Base;

        NewPattern();
    }

    public void AwakeAnimator()
    {
        anim.SetBool("Sleeping", false);
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.BossIntro);
    }

    public override void ResetBoss()
    {
        onReset?.Invoke();

        base.ResetBoss();

        sawSpeed = 0;
        targetSawSpeed = 0;
        camera.DeactivateArea(0);
        damageOnContact = true;
        cannonsAnim.SetBool("Visible", false);
        shooting = false;
        spawning = false;

        DashExit();
        StunnedExit();
        ChangeState(Boss1State.Idle);
        DestroySpawnedEnemies();
    }

    public void EnableDamageOnContact()
    {
        damageOnContact = true;
    }

    public void DisableDamageOnContact()
    {
        damageOnContact = false;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Death

    protected override void DeadUpdate(float timer)
    {
        light.color = new Color(light.color.r, light.color.g, light.color.b, timer);
    }

    protected override void Die()
    {
        base.Die();

        switch (state)
        {
            case Boss1State.Dash:
                DashExit();
                break;
            case Boss1State.Centering:
                CenteringExit();
                break;
            case Boss1State.Pattern:
                PatternExit();
                break;
            case Boss1State.Returning:
                ReturningExit();
                break;
        }

        targetSawSpeed = 0;
        sawSpeed = 0;
        awakened = false;
        damageSprite.gameObject.SetActive(false);
        ghosting.Disable();

        anim.speed = 1;
        anim.SetBool("Dead", true);
        camera.DeactivateArea(0);
        DestroySpawnedEnemies();
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.BossDeath);
        collider.gameObject.SetActive(false);

        Timer.CallOnDelay(() =>
        {
            GameManager.GoToLevel("Level 2");
        }, 7);

        Timer.CallOnDelay(() =>
        {
            ReturnControl();
        }, 10);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Others

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        damageCooldown = damageDuration;

        AddHealth(-damage);
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.EnemyHit);

        UpdateHealthState();
    }

    public float LifeRegenMultiplier { get => 1; }

    public bool AvoidingLayer(int layer)
    {
        return !awakened;
    }

    private void CannonsRotationUpdate()
    {
        cannonsRoot.Rotate(0, 0, Time.deltaTime * cannonsRotationSpeed * patternDir);
    }

    private void GhostingUpdate()
    {
        ghosting.Enable(ghostingSprite, fill.color);
    }

    private void SawUpdate()
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
        }
    }

    protected override void OnStageChange(int newDivision)
    {
        base.OnStageChange(newDivision);

        headAnim.SetTrigger("Change State");
        anim.speed = easyAttackPreWarm / attackPreWarm;

        float difference = healthDivisions - newDivision;

        float maxDifference = healthDivisions - 1;

        moveSpeed = Mathf.Lerp(easySpeed, hardSpeed, difference / maxDifference);
        dashSpeed = Mathf.Lerp(easyDashSpeed, hardDashSpeed, difference / maxDifference);
        attackPreWarm = Mathf.Lerp(easyAttackPreWarm, hardAttackPreWarm, difference / maxDifference);
        stunDuration = Mathf.Lerp(easyStunDuration, hardStunDuration, difference / maxDifference); //maybe return this to updatestatechange'
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == Boss1State.Dash)
        {
            if (collision.collider.gameObject.layer == 11) // Map
            {
                switch (pattern)
                {
                    case Boss1Pattern.Bouncing:
                        if (bouncesCounter >= maxBounces)
                        {
                            Stun();
                            DashExit();
                        }
                        else
                        {
                            bouncesCounter++;
                            dashCooldown = dashDuration;
                            dashDirection = GambaFunctions.Bounce(collision.GetContact(0).normal, dashDirection);

                            rb.velocity = dashDirection.normalized * dashSpeed;
                        }
                        break;
                    case Boss1Pattern.Dashing:
                        Stun();
                        DashExit();
                        break;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (state == Boss1State.Dash)
        {
            if (collision.collider.gameObject.layer == 11) // Map
            {
                if (pattern == Boss1Pattern.Bouncing && bouncesCounter < maxBounces)
                {
                    dashDirection = GambaFunctions.Bounce(collision.GetContact(0).normal, dashDirection);

                    rb.velocity = dashDirection.normalized * dashSpeed;
                }
            }
        }
    }

    private int ActiveEnemies()
    {
        int count = 0;

        if (spawnedEnemies != null)
        {
            if (spawnedEnemies.Count > 0)
            {
                for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
                {
                    if (spawnedEnemies[i] != null)
                    {
                        count++;
                    }
                    else
                    {
                        spawnedEnemies.RemoveAt(i);
                    }
                }
            }
        }

        return count;
    }

    #endregion

    private void OnDrawGizmos()
    {
        // Spawn position
        Gizmos.color = new Color(1,1,1,0.7f);
        Gizmos.DrawLine(new Vector2(spawnPosition.x - 1, spawnPosition.y), new Vector2(spawnPosition.x + 1, spawnPosition.y));
        Gizmos.DrawLine(new Vector2(spawnPosition.x, spawnPosition.y - 1), new Vector2(spawnPosition.x, spawnPosition.y + 1));

        // Circular pattern
        Gizmos.DrawWireSphere(circularPattern.center, circularPattern.radius);

        // Dash Direction
        Gizmos.color = new Color(1, 0.2f, 0.3f);
        GambaFunctions.GizmosDrawArrow(transform.position, dashDirection);
    }

    private IEnumerator ShootingLoop()
    {
        while (true)
        {
            if (shooting)
            {
                ShootSaw();
            }
            yield return new WaitForSeconds(1f / rateOfFire);
        }
    }

    private IEnumerator EnemySpawningLoop()
    {
        while (true)
        {
            if (spawning)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(1f / rateOfSpawn);
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        for (int i = 0; i < cannons.Count; i++)
        {
            cannons[i].SetName(i);
        }
    }

#endif

}
