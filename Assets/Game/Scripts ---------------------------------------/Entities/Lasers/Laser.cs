using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Laser : MonoBehaviour
{
    [System.Serializable]
    public class ColoredSprite
    {
        [SerializeField, HideInInspector] private string name;

        public SpriteRenderer sr;
        [SerializeField]
        [Range(0, 255)]
        private byte activeAlpha;

        public float ActiveAlpha { get => activeAlpha / 255f; }
        public void SetColor(float r, float g, float b, float a)
        {
            if (sr != null)
            {
                sr.color = new Color(r,g,b,a);
            }
        }

        public void SetName()
        {
            if (sr != null)
            {
                name = $"{sr.gameObject.name} : {activeAlpha}";
            }
            else
            {
                name = $"null : {activeAlpha}";
            }
        }
    }

    [Header("Components")]
    [SerializeField]
    private Transform laserRoot;
    [SerializeField]
    private SpriteRenderer laserBeam;
    [SerializeField]
    private ParticleSystem ps;
    [SerializeField]
    private List<ColoredSprite> coloredSprites = new List<ColoredSprite>();
    [SerializeField]
    private new Collider2D collider;
    [SerializeField]
    private Animator anim;

    [Header("Settings")]
    [SerializeField]
    private bool updatePositionsInRuntime;
    [SerializeField]
    private LayerMask hitLayers;
    [SerializeField]
    private Color mainColor = Color.white;
    [SerializeField]
    private float damage = 100;
    [SerializeField]
    private Player.ColorState colorTag;
    [SerializeField]
    private bool respondToTag = true;
    [SerializeField]
    private float distanceMultiplier = 1;

    [Header("Info")]
    [ReadOnly, SerializeField]
    protected Vector2 posA;
    [ReadOnly, SerializeField]
    protected Vector2 posB;
    [ReadOnly, SerializeField]
    protected bool activated;
    [ReadOnly, SerializeField]
    private bool isActiveOnDistance;

    private Vector2 lastPosA;
    private Vector2 lastPosB;

    private Color lastColor;

    // Collision Detection
    private List<Collider2D> targets = new List<Collider2D>();
    private ContactFilter2D filter = new ContactFilter2D();

    public bool Activated { get => activated; }

    private void Start()
    {
        CollisionSetup();

        UpdatePositions();
        UpdateLaser();
        UpdateState();
    }

    private void OnEnable()
    {
        StartCoroutine(IsActiveOnDistanceLoop());
        SetActiveOnDistance(isActiveOnDistance);
    }

    private void CollisionSetup()
    {
        filter = new ContactFilter2D();
        filter.layerMask = hitLayers;
        filter.useLayerMask = true;
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (isActiveOnDistance)
            {
                if (!GameManager.GamePaused)
                {
                    if (updatePositionsInRuntime)
                    {
                        UpdatePositions();
                    }

                    if (lastPosA != posA || lastPosB != posB || lastColor != mainColor)
                    {
                        UpdateLaser();
                    }

                    if (activated)
                    {
                        CollisionDetection();
                    }

                    StateDetection();
                }
            }
        }
        else
        {
            UpdatePositions();

            if (lastPosA != posA || lastPosB != posB || lastColor != mainColor)
            {
                UpdateLaser();
            }
        }
    }

    public virtual void UpdatePositions() { }

    protected virtual void UpdateLaser()
    {
        lastPosA = posA;
        lastPosB = posB;
        lastColor = mainColor;

        UpdateBeam();
        UpdateColor();
    }

    #region LaserUpdates

    private void UpdateBeam()
    {
        Vector2 ab = posB - posA;

        laserRoot.position = posA + (ab) * 0.5f;

        laserRoot.rotation = Quaternion.LookRotation(Vector3.forward, (ab));

        laserRoot.localScale = new Vector2(laserRoot.localScale.x, (-ab).magnitude);

        ParticleSystem.ShapeModule module = ps.shape;
        module.scale = new Vector2(module.scale.x, laserRoot.localScale.y);

        Physics2D.SyncTransforms();
    }

    private void UpdateColor()
    {
        for (int i = 0; i < coloredSprites.Count; i++)
        {
            if (coloredSprites[i].sr != null)
            {
                coloredSprites[i].SetColor(mainColor.r, mainColor.g, mainColor.b, coloredSprites[i].sr.color.a);
            }
        }

        ParticleSystem.ColorOverLifetimeModule colorModule = ps.colorOverLifetime;

        ParticleSystem.MinMaxGradient minMaxGradient = colorModule.color;

        GradientColorKey[] gradientColors = new GradientColorKey[2];
        gradientColors[0] = new GradientColorKey(mainColor, 0.2f);
        gradientColors[1] = new GradientColorKey(Color.white, 1f);

        minMaxGradient.gradientMin = new Gradient();

        minMaxGradient.gradientMin.colorKeys = gradientColors;
        colorModule.color = minMaxGradient;
    }

    private void CollisionDetection()
    {
        collider.OverlapCollider(filter, targets);

        for (int i = 0; i < targets.Count; i++)
        {
            IHittable hit = targets[i]?.attachedRigidbody?.GetComponent<IHittable>();

            if (hit != null)
            {
                if (!hit.AvoidingLayer(gameObject.layer))
                {
                    hit.OnHit(transform.position, damage, 0, gameObject.layer);
                    AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Lazered);
                }
            }
        }
    }

    private void StateDetection()
    {
        Player.ColorState activeColor = GameManager.GetActiveColor();

        if (colorTag == activeColor && respondToTag)
        {
            if (activated) // Deactivate
            {
                activated = false;
                UpdateState();
            }
        }
        else
        {
            if (!activated) // Activate
            {
                activated = true;
                UpdateState();
            }
        }
    }

    private void UpdateState()
    {
        if (coloredSprites != null)
        {
            if (activated)
            {
                for (int i = 0; i < coloredSprites.Count; i++)
                {
                    coloredSprites[i].SetColor(mainColor.r, mainColor.g, mainColor.b, coloredSprites[i].ActiveAlpha);
                }
                ps.gameObject.SetActive(true);
                laserBeam.enabled = true;
            }
            else
            {
                for (int i = 0; i < coloredSprites.Count; i++)
                {
                    coloredSprites[i].SetColor(mainColor.r, mainColor.g, mainColor.b, coloredSprites[i].ActiveAlpha * 0.2f);
                }
                ps.gameObject.SetActive(false);
                laserBeam.enabled = false;
            }
        }
    }

    [SerializeField]
    private bool updateLaser;
    private void OnValidate()
    {
        foreach (ColoredSprite c in coloredSprites)
        {
            c.SetName();
        }

        if (updateLaser)
        {
            updateLaser = false;

            UpdatePositions();
            UpdateLaser();
        }
    }

    #endregion

    private void SetActiveOnDistance(bool value)
    {
        ps.enableEmission = value;
        anim.enabled = value;
    }

    private IEnumerator IsActiveOnDistanceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            bool newActive = GameManager.IsOnActiveDistance(transform.position, 0, distanceMultiplier);

            if (newActive != isActiveOnDistance)
            {
                isActiveOnDistance = newActive;

                SetActiveOnDistance(newActive);
            }
        }
    }
}