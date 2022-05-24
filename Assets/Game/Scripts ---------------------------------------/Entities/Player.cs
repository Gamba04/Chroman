using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour, IHittable
{
    public enum PlayerState
    {
        Base,
        NoInput,
        Dash,
    }

    public enum ColorState
    {
        None = -1,
        Red,
        Blue,
        Purple,
        Yellow
    }

    [SerializeField]
    private bool allowHacks;

    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private SpriteRenderer sr;
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private GameObject directionArrow;
    [SerializeField]
    private SpriteRenderer dirArrowSprite;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private new SpriteRenderer light;
    [SerializeField]
    private Sprite[] colorSprites;
    [SerializeField]
    private Sprite[] ghostingSprites;
    [SerializeField]
    private Ghosting ghosting;
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private SpriteRenderer kineticCloudSr;
    [SerializeField]
    private Collider2D kineticCloudCollider;

    [Header("Settings")]
    [SerializeField]
    private ColorState startingState;
    [SerializeField]
    private float dashSpeed;
    [SerializeField]
    private float maxDashDuration;
    [SerializeField]
    private float colorTransitionDuration;
    [SerializeField]
    private AnimationCurve deathcurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));

    [GambaHeader("Magnet", 0.5f, 0.5f, 0.5f, 1f)]
    [SerializeField]
    private float magnetRange = 5;
    [SerializeField]
    private LayerMask magnetLayers;

    [GambaHeader("Stats", 1f, 0.3f, 0.1f, 1f)]
    [SerializeField]
    private float health;
    [SerializeField]
    private float meleeDamage;
    [SerializeField]
    private float bulletDamage;
    [SerializeField]
    private float kineticForce = 7;
    [SerializeField]
    private float kineticRigidity;
    [SerializeField]
    private float kineticCloudRange = 5;
    [SerializeField]
    private float knockback;
    [SerializeField]
    private int unlockedColors;

    [GambaHeader("Movement")]
    [SerializeField]
    private float moveAcceleration;
    [SerializeField]
    private float moveDeacceleration;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    [Range(0, 1)]
    private float moveSteadyTreshold;
    [SerializeField]
    [Range(0, 1)]
    private float externalFriction;
    [SerializeField]
    private float defaultMass = 5;
    [SerializeField]
    private float dashMass = 10;

    [GambaHeader("Cooldowns")]
    [SerializeField]
    [Range(0.1f, 2)]
    private float maxCooldown_Invincible;
    [SerializeField]
    [Range(0.1f, 2)]
    private float maxCooldown_Dash;
    [SerializeField]
    [Range(0.1f, 2)]
    private float maxCooldown_Melee;
    [SerializeField]
    [Range(0.1f, 2)]
    private float maxCooldown_Range;
    [SerializeField]
    [Range(0.1f, 2)]
    private float maxCooldown_Kinetic;
    [SerializeField]
    [Range(0.1f, 2)]
    private float maxCooldown_Electric;
    [SerializeField]
    [Range(0.1f, 2)]
    private float deathDuration;

    [GambaHeader("Colors", 0.7f)]
    [SerializeField]
    private Color color_None;
    [SerializeField]
    private Color color_Melee;
    [SerializeField]
    private Color color_Range;
    [SerializeField]
    private Color color_Kinetic;
    [SerializeField]
    private Color color_Electric;

    [Header("Transitions")]
    [SerializeField]
    private Transition kineticCloudTransition;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly, SerializeField]
    private PlayerState state;
    [ReadOnly, SerializeField]
    private ColorState colorState;
    [ReadOnly, SerializeField]
    private bool dead;
    [Range(0, 1)]
    private float colorProgress;
    [ReadOnly, SerializeField]
    private Vector2 velocity;
    [ReadOnly, SerializeField]
    private Vector2 externalMomentum;
    [ReadOnly, SerializeField]
    private bool isInvincible;

    [GambaHeader("Inputs", 0.7f)]
    [ReadOnly, SerializeField]
    private Vector2 moveAxis;
    [ReadOnly, SerializeField]
    private Vector2 direction;

    [GambaHeader("Cooldowns", 0.7f)]
    [ReadOnly, SerializeField]
    private float cooldown_Invincible;
    [ReadOnly, SerializeField]
    private float cooldown_Attack;
    [ReadOnly, SerializeField]
    private float cooldown_Dash;
    [ReadOnly, SerializeField]
    private float duration_Dash;
    [ReadOnly, SerializeField]
    private float deathCounter;
    [ReadOnly, SerializeField]
    private Vector2 lastValidPosition;

    private Color previousColor;
    private Color targetColor;
    private float lightAlpha;

    private int currentAttack;

    private Bullet currentBullet;
    private bool attackCollider;

    private bool kineticActive;
    private bool kineticPressed;
    private IKinetic kineticObject;
    private Vector2 kineticPosition;
    private Vector3 kineticCloudOriginalScale;
    private Vector3 lastCloudScale;
    private Quaternion kineticRotationOffset;

    List<int> ignoreLayers = new List<int>();

    public Action<float> onDamageDealt;
    public Action onCombat;
    public Action onDeath;
    public Action onUpdateColor;

    private static int UnlockedColors = 0;

    public static float MaxHealth = 3;
    public float Health { get => health; set => health = value; }
    public bool Dead { get => dead; set => dead = value; }
    public bool IsInvincible { get => isInvincible; set => isInvincible = value; }

    private void Start()
    {
        health = MaxHealth;

        if (unlockedColors < UnlockedColors)
        {
            unlockedColors = UnlockedColors;
        }
        else
        {
            UnlockedColors = unlockedColors;
        }

        LoadAllSprites();

        kineticCloudOriginalScale = kineticCloudSr.transform.localScale;

        ChangeColor(startingState);

        StartCoroutine(InvincibilityFrames());

        ghosting.SetParent(GameManager.ParentGhosting);

        lightAlpha = light.color.a;

        onDamageDealt += OnDamageToCombat;
    }

    private void Update()
    {
        if (!GameManager.GamePaused)
        {
            if (!dead)
            {
                CooldownUpdate();
                ColorUpdate();
                AttackColliderUpdate();
                MagnetUpdate();

                MainBehaviour();

                ExternalMomentumUpdate();
            }
            else
            {
                DeathUpdate();
            }
        }

        InsideWallDebugUpdate();
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Inputs

    private void InputAttacks()
    {
        // Specific platform inputs
        if (GameManager.targetInput == TargetInput.MouseKeyboard)
        {
            // Mouse and Keyboard
            if (Input.GetMouseButton(0))
            {
                Attack();
            }

            if (Input.GetMouseButton(1))
            {
                SecondaryAttack();
            }
        }
        else
        {
            // Controller
            if (Input.GetButton("Attack"))
            {
                Attack();
            }

            if (Input.GetButton("Secondary Attack"))
            {
                SecondaryAttack();
            }
        }
    }

    private void InputMovement()
    {
        moveAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void InputDirection()
    {
        if (GameManager.targetInput == TargetInput.MouseKeyboard)
        {
            // Mouse and Keyboard
            direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        }
        else
        {
            // Controller

        }
    }

    private void InputAttackUps()
    {
        // Specific platform inputs
        if (GameManager.targetInput == TargetInput.MouseKeyboard)
        {
            if (Input.GetMouseButtonUp(0))
            {
                AttackUp();
            }
        }
        else
        {
            if (Input.GetButtonUp("Attack"))
            {
                AttackUp();
            }
        }
    }

    private void InputColorSwap()
    {
        if (Input.GetButtonDown("Melee"))
        {
            ChangeColor(ColorState.Red);
        }
        if (Input.GetButtonDown("Range"))
        {
            ChangeColor(ColorState.Blue);
        }
        if (Input.GetButtonDown("Kinetic"))
        {
            ChangeColor(ColorState.Purple);
        }
        if (Input.GetButtonDown("Electric"))
        {
            ChangeColor(ColorState.Yellow);
        }

        if (Input.GetButtonDown("Previous Color"))
        {
            ColorSwap(false);
        }
        if (Input.GetButtonDown("Next Color"))
        {
            ColorSwap(true);
        }
    }

    private void InputDash()
    {
        if (Input.GetButton("Dash"))
        {             
            StartDash(moveAxis.normalized);       
        }
    }

    private void InputHacks()
    {
        if (allowHacks)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                AddHealth(1);
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region PlayerStates

    private void ChangeState(PlayerState state)
    {
        this.state = state;

        if (state == PlayerState.NoInput)
        {
            CancelKinetic();
        }
    }

    private void MainBehaviour()
    {
        switch (state)
        {
            case PlayerState.Base:
                MovementUpdate();
                KineticUpdate();

                InputMovement();
                InputDash();
                InputAttacks();
                InputAttackUps();
                InputColorSwap();
                InputDirection();
                InputHacks();

                DirectionArrowUpdate();
                break;
            // ------------------------------------------------
            case PlayerState.Dash:
                KineticUpdate();

                InputMovement();
                InputAttackUps();
                InputColorSwap();
                InputDirection();

                DirectionArrowUpdate();

                Dashing();
                break;
            // ------------------------------------------------
            case PlayerState.NoInput:
                moveAxis = Vector2.zero;
                MovementUpdate();
                break;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region ColorStates

    public void ChangeColor(ColorState colorState)
    {
        if (!IsInsideOfInvisibleColorObject() && (int)colorState < unlockedColors)
        {
            switch (this.colorState)
            {
                case ColorState.None:
                    previousColor = color_None;
                    break;
                case ColorState.Red:
                    previousColor = color_Melee;
                    break;
                case ColorState.Blue:
                    previousColor = color_Range;
                    break;
                case ColorState.Purple:
                    previousColor = color_Kinetic;
                    break;
                case ColorState.Yellow:
                    previousColor = color_Electric;
                    break;
            }

            this.colorState = colorState;

            switch (colorState)
            {
                case ColorState.None:
                    targetColor = color_None;
                    CancelKinetic();
                    anim.SetInteger("State", -1);
                    break;
                case ColorState.Red:
                    targetColor = color_Melee;
                    CancelKinetic();
                    anim.SetInteger("State", 0);
                    break;
                case ColorState.Blue:
                    targetColor = color_Range;
                    CancelKinetic();
                    anim.SetInteger("State", 1);
                    break;
                case ColorState.Purple:
                    targetColor = color_Kinetic;
                    anim.SetInteger("State", 2);
                    break;
                case ColorState.Yellow:
                    CancelKinetic();
                    targetColor = color_Electric;
                    anim.SetInteger("State", 3);
                    break;
            }

            sr.sprite = colorSprites[(int)colorState + 1];

            colorProgress = 0;
        }
    }

    private void ColorSwap(bool nextCol)
    {
        if (unlockedColors > 0)
        {
            if (nextCol)
            {
                if ((int)colorState == unlockedColors - 1)
                {
                    ChangeColor(0);
                }
                else
                {
                    ChangeColor((ColorState)((int)colorState + 1));
                }
            }
            else // Previous
            {
                if (colorState <= 0)
                {
                    ChangeColor((ColorState)unlockedColors - 1);
                }
                else
                {
                    ChangeColor((ColorState)((int)colorState - 1));
                }
            }
        }
    }
    private void ColorUpdate()
    {
        if (colorProgress < 1)
        {
            colorProgress += Time.deltaTime / colorTransitionDuration;

            Color color = previousColor + (targetColor - previousColor) * colorProgress;

            dirArrowSprite.color = targetColor;
            light.color = new Color(color.r, color.g, color.b, lightAlpha);
        }
        else
        {
            colorProgress = 1;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Mechanics

    #region Movement

    private void MovementUpdate()
    {
        // Acceleration
        if (moveAxis.magnitude != 0)
        {
            velocity += moveAxis.normalized * moveAcceleration * Time.deltaTime;
        }

        // Deacceleration
        float analogSpeed = Mathf.Clamp01(moveAxis.magnitude);

        if (velocity.magnitude > moveSteadyTreshold)
        {
            velocity *= 1 - Mathf.Clamp01((1 - analogSpeed) * Time.deltaTime * moveDeacceleration);
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

        rb.velocity = velocity + externalMomentum;
    }

    private void ExternalMomentumUpdate()
    {
        if (externalMomentum.sqrMagnitude > 0 && externalFriction > 0)
        {
            externalMomentum *= 1 - externalFriction * Time.deltaTime * 10;
        }
    }

    #endregion

    #region Kinetic

    private void KineticUpdate()
    {
        if (kineticObject != null)
        {
            if (kineticObject.isDead || kineticObject.kRigidbody == null)
            {
                kineticObject = null;
                SetKineticState(false);
            }
        }

        KineticCloudUpdate();
        DragKineticObject();
    }

    private void KineticCloudUpdate()
    {
        if (kineticActive)
        {
            if (kineticObject == null)
            {
                Collider2D[] targets = new Collider2D[10];

                ContactFilter2D filter = new ContactFilter2D();
                filter.layerMask = LayerMask.GetMask("Default", "Hittable", "Enemy", "EnemyBullet", "PlayerBullet");
                filter.useLayerMask = true;

                Physics2D.OverlapCollider(kineticCloudCollider, filter, targets);

                for (int i = 0; i < targets.Length; i++)
                {
                    IKinetic kObj = targets[i]?.attachedRigidbody?.GetComponent<IKinetic>();

                    if (kObj != null)
                    {
                        if (!kObj.isDead)
                        {
                            GrabKinetic(kObj);
                            break;
                        }
                    }
                }
            }
            else
            {
                if ((kineticObject.kTransform.position - transform.position).magnitude > 15)
                {
                    CancelKinetic();
                }
            }

            // Movement
            CalculateKineticTargetPosition();

            if (kineticObject != null)
            {
                Vector2 realOffset = kineticObject.kTransform.right * kineticObject.kineticCloudOffset.x + kineticObject.kTransform.up * kineticObject.kineticCloudOffset.y;

                kineticCloudSr.transform.position = kineticObject.kTransform.position + (Vector3)realOffset;
                if (kineticObject.rotateCloud)
                {
                    kineticCloudSr.transform.rotation = kineticObject.kTransform.rotation;
                }
            }
            else
            {
                kineticCloudSr.transform.position = kineticPosition;
                kineticCloudCollider.transform.position = kineticPosition;
            }
        }

        // Sprite
        kineticCloudTransition.UpdateTransitionValue();

        kineticCloudSr.color = new Color(kineticCloudSr.color.r, kineticCloudSr.color.g, kineticCloudSr.color.b, kineticCloudTransition.value);
        kineticCloudSr.transform.localScale = Vector3.Lerp(kineticCloudOriginalScale, lastCloudScale, kineticCloudTransition.value);
    }

    private void CalculateKineticTargetPosition()
    {
        Vector2 semiClampedDirection = direction;

        if (direction.magnitude > kineticCloudRange)
        {
            semiClampedDirection = direction.normalized * Mathf.Lerp(kineticCloudRange, direction.magnitude, 0.7f * kineticCloudRange / direction.magnitude);
        }

        kineticPosition = (Vector2)transform.position + semiClampedDirection;
    }

    private void DragKineticObject()
    {
        if (kineticObject != null)
        {
            Rigidbody2D kRb = kineticObject.kRigidbody;
            Vector2 targetPosition = kineticPosition;

            Vector2 momentum = kineticObject.lookAtPlayer ? moveAxis * 1.7f : Vector2.zero;

            kRb.velocity = Vector2.Lerp(kRb.velocity, (targetPosition - kRb.position) * kineticRigidity, 0.1f * Time.deltaTime / 0.02f) + momentum * Time.deltaTime/0.01f;
            if (kineticObject.lookAtPlayer)
            {
                Vector3 targetDirection = kRb.transform.position - transform.position; // Position to Pivot

                float dot = Vector3.Dot(direction.normalized, targetDirection.normalized); // Looking at pivot amount

                targetDirection = Vector3.Lerp(direction, targetDirection, targetDirection.magnitude / 5f); // Blend with local direction

                Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, -targetDirection); // Convert to rotation
                Quaternion targetRotation = Quaternion.Euler(0,0, lookRotation.eulerAngles.z + kineticRotationOffset.eulerAngles.z); // Add offset

                //kRb.rotation = Quaternion.Slerp(kRb.transform.rotation, targetRotation, 0.2f * ((dot + 1)/2f) * Time.deltaTime/0.02f).eulerAngles.z; // 0.2f * looking amount
                kRb.rotation = (kRb.transform.rotation * Quaternion.Slerp(Quaternion.identity, (targetRotation * Quaternion.Inverse(kRb.transform.rotation)).normalized, 0.1f * ((dot + 1) / 2f))).eulerAngles.z;
            }
        }
    }

    private void SetKineticState(bool activated)
    {
        kineticActive = activated;

        if (activated)
        {
            kineticCloudTransition.StartTransition(1);
            kineticCloudCollider.enabled = true;
        }
        else
        {
            cooldown_Attack = maxCooldown_Kinetic;

            kineticCloudTransition.StartTransition(0, () =>
            {
                lastCloudScale = kineticCloudOriginalScale;
                kineticCloudSr.transform.rotation = Quaternion.identity;
            });
            kineticCloudCollider.enabled = false;

            kineticObject = null;
        }
    }

    public void CancelKinetic()
    {
        kineticPressed = false;

        if (kineticObject != null)
        {
            kineticObject.kineticState = KineticState.None;

            kineticObject.Unleash();
        }

        SetKineticState(false);
    }

    private void ThrowKinetic()
    {
        if (kineticObject != null)
        {
            kineticObject.kineticState = KineticState.Thrown;

            kineticObject.Throw(direction, kineticForce);
        }

        SetKineticState(false);
    }

    private void GrabKinetic(IKinetic kineticObject)
    {
        kineticObject.kineticState = KineticState.Grabbed;

        kineticCloudSr.transform.localScale = kineticObject.kineticCloudScale * kineticCloudOriginalScale;
        lastCloudScale = kineticObject.kineticCloudScale * kineticCloudOriginalScale;

        kineticCloudCollider.enabled = false;

        kineticObject.Grab();

        if (kineticObject.lookAtPlayer)
        {
            Transform kTransform = kineticObject.kTransform;
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, transform.position - kTransform.position);
            kineticRotationOffset = Quaternion.Euler(0, 0, kTransform.rotation.eulerAngles.z - lookRotation.eulerAngles.z);
        }

        this.kineticObject = kineticObject;
    }

    #endregion

    #region Attacks

    private void Attack()
    {
        if (cooldown_Attack <= 0)
        {
            switch (colorState)
            {
                case ColorState.Red:
                    cooldown_Attack = maxCooldown_Melee;
                    AttackMelee();

                    //Sound
                    int p = UnityEngine.Random.Range(0, 3);
                    if (p == 0) AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Melee);
                    else if (p==1) AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Melee2);
                    else  AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Melee3);

                    break;

                case ColorState.Blue:
                    cooldown_Attack = maxCooldown_Range;
                    AttackRange();
                    AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Range);
                    break;

                case ColorState.Purple:
                    AttackKinetic();
                    AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Kinetic);

                    kineticPressed = true;
                    break;

                case ColorState.Yellow:
                    cooldown_Attack = maxCooldown_Electric;
                    AttackElectric();
                    AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Electric);
                    break;
            }
        }
    }

    private void AttackUp()
    {
        switch (colorState)
        {
            case ColorState.Red:
                break;

            case ColorState.Blue:
                break;

            case ColorState.Purple:
                CancelKinetic();
                kineticPressed = false;
                break;

            case ColorState.Yellow:
                break;
        }
    }

    private void SecondaryAttack()
    {
        switch (colorState)
        {
            case ColorState.Red:
                break;

            case ColorState.Blue:
                break;

            case ColorState.Purple:
                if (kineticObject != null)
                {
                    ThrowKinetic();
                }
                break;

            case ColorState.Yellow:
                break;
        }
    }

    private void AttackMelee()
    {
        attackCollider = true;

        Timer.CallOnDelay(() =>
        {
            attackCollider = false;
        }, 0.2f, "Player: Deactivate attack collider");

        anim.SetInteger("Rand", currentAttack);
        anim.SetTrigger("Attack");

        if (currentAttack <= 0)
        {
            currentAttack++;
        }
        else
        {
            currentAttack = 0;
        }
    }

    private void AttackRange()
    {
        GameObject bullet;

        if (GameManager.ParentBullets != null)
        {
            bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction.normalized * 0.5f, transform.rotation, GameManager.ParentBullets);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction.normalized * 0.5f, transform.rotation);
        }

        currentBullet = bullet.GetComponent<Bullet>();
        currentBullet.SetUp(direction.normalized, bulletDamage);
        currentBullet.onDamageDealt += onDamageDealt;
    }

    private void AttackKinetic()
    {
        if (!kineticPressed)
        {
            CalculateKineticTargetPosition();
            kineticCloudSr.transform.position = kineticPosition;
            kineticCloudCollider.transform.position = kineticPosition;

            SetKineticState(true);
        }
    }

    private void AttackElectric()
    {

    }

    private void AttackColliderUpdate()
    {
        if (attackCollider)
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll((Vector2)transform.position + direction.normalized * 0.8f, 1f, LayerMask.GetMask("Hittable", "Enemy", "Boss"));

            for (int i = 0; i < targets.Length; i++)
            {
                IHittable hit = targets[i]?.attachedRigidbody?.GetComponent<IHittable>();

                if (hit != null)
                {
                    if (!hit.AvoidingLayer(gameObject.layer))
                    {
                        hit.OnHit(transform.position, meleeDamage, knockback, gameObject.layer);
                        onDamageDealt?.Invoke(meleeDamage * hit.LifeRegenMultiplier);
                        attackCollider = false;
                    }
                }
            }
        }
    }

    #endregion

    #region OtherMechanics

    private void StartDash(Vector2 direction)
    {
        if (moveAxis.magnitude > 0.1f)
        {
            if (duration_Dash <= 0 && cooldown_Dash <= 0)
            {
                rb.mass = dashMass;

                rb.velocity = direction.normalized * dashSpeed;
                duration_Dash = maxDashDuration;
                cooldown_Dash = maxCooldown_Dash;

                ChangeState(PlayerState.Dash);

                ghosting.Enable(ghostingSprites[(int)colorState + 1], targetColor);
                AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Dash);

                ignoreLayers.Add(10); // EnemyBullet
                ignoreLayers.Add(15); // Laser
            }
        }
    }

    private void Dashing()
    {
        ghosting.SetColor(Color.Lerp(ghosting.CurrentColor, targetColor, 0.2f * Time.deltaTime/0.02f));
    }

    private void StopDash()
    {
        rb.mass = defaultMass;

        if (state == PlayerState.Dash)
        {
            ChangeState(PlayerState.Base);
        }
        ghosting.Disable();

        ignoreLayers.Clear();
    }

    private void DirectionArrowUpdate()
    {
        directionArrow.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        sr.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    }

    private void MagnetUpdate()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, magnetRange, magnetLayers);

        for (int i = 0; i < targets.Length; i++)
        {
            Magnetic magnet = targets[i]?.attachedRigidbody?.GetComponent<Magnetic>();

            if (magnet != null)
            {
                Vector2 attractionVector = transform.position - magnet.transform.position;

                magnet.Attract(attractionVector);
            }
        }
    }

    #endregion

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Death

    private void DeathUpdate()
    {
        Timer.ReduceCooldown(ref deathCounter, ()=>
        {
            GameManager.Instance.LastCheckpoint();
        });

        if (deathCounter > 0)
        {
            float timer = deathcurve.Evaluate(1 - (deathCounter / deathDuration));

            light.color = new Color(light.color.r, light.color.g, light.color.b, timer * lightAlpha);
        }
    }

    private void Die()
    {
        explosion.Emit(20);

        // Turn off components
        sr.enabled = false;
        collider.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        directionArrow.SetActive(false);
        ghosting.Disable();
        CancelKinetic();
        kineticCloudSr.color = new Color(kineticCloudSr.color.r, kineticCloudSr.color.g, kineticCloudSr.color.b, 0);

        //Sound
        AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Death);
        onDeath?.Invoke();

        // Add set death sentence
        deathCounter = deathDuration;
        dead = true;
    }

    public void Respawn()
    {
        sr.enabled = true;
        collider.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
        directionArrow.SetActive(true);

        dead = false;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void CooldownUpdate()
    {
        Timer.ReduceCooldown(ref cooldown_Attack);
        Timer.ReduceCooldown(ref cooldown_Dash, () => AudioPlayer.PlaySFX(AudioPlayer.SFXTag.DashAvailable));
        Timer.ReduceCooldown(ref duration_Dash, StopDash);
        Timer.ReduceCooldown(ref cooldown_Invincible, ()=> { isInvincible = false; } );
    }

    private bool IsInsideOfInvisibleColorObject()
    {
        bool r = false;

        Collider2D target = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Map", "Laser"));

        if (target != null)
        {
            if (target.isTrigger)
            {
                r = true;
            }
        }

        return r;
    }

    private bool IsInsideOfWall()
    {
        bool r = false;

        Collider2D[] targets = Physics2D.OverlapPointAll(transform.position, LayerMask.GetMask("Map"));

        for (int i = 0; i < targets.Length; i++)
        {
            TilemapCollider2D target = targets[i].GetComponent<TilemapCollider2D>();

            if (target != null)
            {
                if (!target.isTrigger)
                {
                    r = true;
                    break;
                }
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

    private void OnDamageToCombat(float damage)
    {
        onCombat?.Invoke();
    }

    private void LoadAllSprites()
    {
        if (sr != null)
        {
            for (int i = 0; i < colorSprites.Length; i++)
            {
                if (colorSprites != null)
                {
                    sr.sprite = colorSprites[i];
                }
            }
        }
    }

    //-----------------------------------------------------------------------------------------------

    #region Public Methods

    public void AddHealth(float amount)
    {
        if (amount > 0)
        {
            health += amount;
        }
        else if (!isInvincible)
        {
            health += amount;
        }

        health = Mathf.Clamp(health, 0, MaxHealth);
    }

    public void OnHit(Vector2 hitterPosition, float damage, float knockback, int layer)
    {
        AddHealth(-damage);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            cooldown_Invincible = maxCooldown_Invincible;

            isInvincible = cooldown_Invincible != 0;

            externalMomentum = ((Vector2)transform.position - hitterPosition).normalized * knockback;
        }

        onCombat?.Invoke();
        CameraController.Shake();
    }

    public bool AvoidingLayer(int layer)
    {
        bool check = IsInvincible || GameManager.reloading;

        for (int i = 0; i < ignoreLayers.Count; i++)
        {
            if (layer == ignoreLayers[i])
            {
                check = true;
                break;
            }
        }

        return check;
    }

    public float LifeRegenMultiplier { get => 1; }

    public ColorState GetColorState()
    {
        return colorState;
    }

    public PlayerState GetPlayerState()
    {
        return state;
    }

    public void SetUnlockedColors(int amount)
    {
        if (unlockedColors < amount)
        {
            unlockedColors = amount;
            UnlockedColors = amount;

            onUpdateColor?.Invoke();
        }
    }

    public int GetUnlockedColors()
    {
        return unlockedColors;
    }

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }

    public void SetImmobile(bool value)
    {
        if (value)
        {
            if (state != PlayerState.NoInput)
            {
                ChangeState(PlayerState.NoInput);
            }
        }
        else
        {
            if (state != PlayerState.Base)
            {
                ChangeState(PlayerState.Base);
            }
        }
    }

    #endregion

    //-----------------------------------------------------------------------------------------------

    private void OnDestroy()
    {
        if (currentBullet != null)
        {
            currentBullet.onDamageDealt = null;
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        float step = 0.1f;

        while (true)
        {
            if (isInvincible)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
                yield return new WaitForSeconds(step);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
            }
            else
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
            }
            yield return new WaitForSeconds(step);
        }
    }

    private void OnDrawGizmos()
    {
        if (attackCollider)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + direction.normalized * 0.8f, 1f);
        }
    }

    #endregion

}