using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Audio;

public enum TargetInput
{
    MouseKeyboard,
    Controller
}

public enum Platform
{
    Windows,
    WebGL
}

public class GameManager : MonoBehaviour
{

    #region Serializable Classes

    [Serializable]
    private class ColorWall
    {
        [HideInInspector, SerializeField] private string name;
        public Tilemap tilemap;
        public TilemapCollider2D collider;

        public void SetEnable(bool enable)
        {
            if (enable)
            {
                if (tilemap != null)
                {
                    tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, 1);
                }

                if (collider != null)
                {
                    collider.isTrigger = false;
                }
            }
            else
            {
                if (tilemap != null)
                {
                    tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, 0.3f);
                }

                if (collider != null)
                {
                    collider.isTrigger = true;
                }
            }
        }

        public ColorWall() { }
        public ColorWall(string name)
        {
            this.name = name;
        }
    }

    [Serializable]
    private class DataPack
    {
        public List<GameObject> objects = new List<GameObject>();

        public Vector2 playerPosition;
        public float playerHealth;
        public Player.ColorState playerColor;
        public int playerUnlockedColors;
        [Range(0f,1f)]
        public float heartRegen;

        private Transform storageParent;

        public void Save(List<GameObject> objects, Vector2 playerPosition, float playerHealth, int playerUnlockedColors, Player.ColorState playerColor, float heartRegen, Transform storageParent = null)
        {
            if (this.objects == null)
            {
                this.objects = new List<GameObject>();
            }

            // Destroy previous data if existent
            if (this.objects.Count > 0)
            {
                for (int i = this.objects.Count - 1; i >= 0; i--)
                {
                    Destroy(this.objects[i]);
                    this.objects.RemoveAt(i);
                }
            }

            // Begin copy of objects
            objectsCopy = objects;
            this.storageParent = storageParent;
            i = 0;
            c = 0;
            level = 0;
            CopyObjectsUpdate();

            /*for (int i = 0; i < objects.Count; i++)
            {
                this.objects.Add(storageParent ? Instantiate(objects[i], storageParent) : Instantiate(objects[i]));
                this.objects[i].name = objects[i].name;
                this.objects[i].SetActive(false);
            }*/

            // Save player data
            this.playerPosition = playerPosition;
            this.playerHealth = playerHealth;
            this.playerUnlockedColors = playerUnlockedColors;
            this.playerColor = playerColor;
            this.heartRegen = heartRegen;
        }

        public void Load(List<GameObject> objects, Player player, GameManager gameManager, ref List<GameObject> objectsReference)
        {
            // Create new reference to GameObjects
            objectsReference = new List<GameObject>();
            for (int i = 0; i < this.objects.Count; i++)
            {
                objectsReference.Add(this.objects[i]);
            }

            // Replace data
            player.transform.position = playerPosition;
            player.GetRigidbody().position = playerPosition;
            player.Health = playerHealth;
            player.SetUnlockedColors(playerUnlockedColors);
            player.ChangeColor(playerColor);

            gameManager.healthRegen = heartRegen;
            if (!bossKilled)
            {
                gameManager.boss?.ResetBoss();
            }

            // Unload old objects
            for (int i = 0; i < objects.Count; i++)
            {
                Destroy(objects[i]);
            }

            // Activate new objects
            for (int i = 0; i < this.objects.Count; i++)
            {
                this.objects[i].SetActive(true);
            }

            this.objects.Clear();
        }

        private int i = 0;
        private int c = 0;
        private int level = -1;

        public List<GameObject> objectsCopy = new List<GameObject>();

        GameObject parent;

        public void CopyObjectsUpdate()
        {
            if (level >= 0)
            {
                if (i < objectsCopy.Count)
                {
                    if (level == 0)
                    {
                        parent = new GameObject(objectsCopy[i].name);

                        parent.transform.SetParent(storageParent);

                        level = 1;
                    }
                    // Childs
                    if (c < objectsCopy[i].transform.childCount)
                    {
                        GameObject subObject = objectsCopy[i].transform.GetChild(c).gameObject;

                        GameObject newObject = Instantiate(subObject, parent?.transform);
                        newObject.name = subObject.name;
                        newObject.SetActive(false);

                        c++;
                    }
                    else
                    {
                        level = 0;

                        this.objects.Add(parent);
                        this.objects[i].name = objectsCopy[i].name;
                        this.objects[i].SetActive(false);

                        i++;
                    }
                }
                else
                {
                    for (int i = 0; i < this.objects.Count; i++)
                    {
                        for (int o = 0; o < this.objects[i].transform.childCount; o++)
                        {
                            this.objects[i].transform.GetChild(o).gameObject.SetActive(true);
                        }
                    }
                    level = -1;
                }
            }
        }
    }

    [Serializable]
    private class ColorArrowsUI
    {
        public enum ArrowState
        {
            Selected,
            Deselected,
            Locked,
        }

        [SerializeField, HideInInspector] private string name;

        [ReadOnly, SerializeField]
        public ArrowState state;
        [SerializeField]
        public Image arrow;
        [SerializeField]
        public Image lockedArrow;

        public float CurrentAlpha { get; private set; }

        public void SetState(ArrowState state)
        {
            this.state = state;
            switch (state)
            {
                case ArrowState.Selected:
                    CurrentAlpha = 1;

                    if (arrow != null)
                    {
                        arrow.gameObject.SetActive(true);
                        arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, 1f);
                    }

                    if (lockedArrow != null)
                    {
                        lockedArrow.gameObject.SetActive(false);
                    }
                    break;
                case ArrowState.Deselected:
                    CurrentAlpha = 0.5f;

                    if (arrow != null)
                    {
                        arrow.gameObject.SetActive(true);
                        arrow.color = new Color(arrow.color.r, arrow.color.g, arrow.color.b, 0.5f);
                    }

                    if (lockedArrow != null)
                    {
                        lockedArrow.gameObject.SetActive(false);
                    }
                    break;
                case ArrowState.Locked:
                    CurrentAlpha = 1;

                    if (arrow != null)
                    {
                        arrow.gameObject.SetActive(false);
                    }

                    if (lockedArrow != null)
                    {
                        lockedArrow.gameObject.SetActive(true);
                    }
                    break;
            }
        }

        public void SetName(int index)
        {
            name = $"{(Player.ColorState)index} Arrow";
        }
    }

    [Serializable]
    private class HeartUI
    {
        public enum HeartState
        {
            On,
            Off,
            Nothing,
        }

        [SerializeField, HideInInspector] private string name;

        public Image heart;
        public Image emptyHeart;
        public Animator anim;

        [ReadOnly, SerializeField]
        private HeartState state;
        [ReadOnly, SerializeField]
        public Vector2 position;

        public const float EmptyHeartAlpha = 65f/255f;

        public void SetState(HeartState state)
        {
            this.state = state;
            switch (state)
            {
                case HeartState.On:
                    if (heart != null)
                    {
                        heart.gameObject.SetActive(true);
                    }

                    if (emptyHeart != null)
                    {
                        emptyHeart.gameObject.SetActive(false);
                    }
                    break;
                case HeartState.Off:
                    if (heart != null)
                    {
                        heart.gameObject.SetActive(false);
                    }

                    if (emptyHeart != null)
                    {
                        emptyHeart.gameObject.SetActive(true);
                    }
                    break;
                case HeartState.Nothing:
                    if (heart != null)
                    {
                        heart.gameObject.SetActive(false);
                    }

                    if (emptyHeart != null)
                    {
                        emptyHeart.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        public void SetPos(Vector2 initialPos, float number, float separation)
        {
            position = initialPos + Vector2.right * number * separation;
            heart.rectTransform.localPosition = position;
            emptyHeart.rectTransform.localPosition = position;
        }

        public void SetSprites(Sprite sprite)
        {
            if (heart != null)
            {
                heart.sprite = sprite;
            }

            if (emptyHeart != null)
            {
                emptyHeart.sprite = sprite;
                emptyHeart.color = new Color(emptyHeart.color.r, emptyHeart.color.g, emptyHeart.color.b, EmptyHeartAlpha);
            }
        }

        public void SetTrigger(string parameter)
        {
            if (anim != null)
            {
                anim.SetTrigger(parameter);
            }
        }

        public void SetName(int index)
        {
            name = $"Heart {index}";
        }
    }

    #endregion

    [SerializeField]
    private Platform targetPlatform;

    [Header("Components")]
    [SerializeField]
    private Player player;
    [SerializeField]
    private Boss boss;
    [SerializeField]
    private CameraController cameraController;
    [SerializeField]
    private HealthController healthController;
    [Space()]
    [SerializeField]
    private List<ColorWall> colorWalls = new List<ColorWall>();
    [SerializeField]
    private List<ColorArrowsUI> colorArrows = new List<ColorArrowsUI>();
    [SerializeField]
    private bool updateHearts;
    [Space()]
    [SerializeField]
    private Image damageScreen;
    [SerializeField]
    private Animator deathScreenAnim;
    [SerializeField]
    private Animator fadeAnim;
    [SerializeField]
    private Animator unpausedAnim;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private RuntimeAnimatorController heartsAnimator;
    [SerializeField]
    private List<Text> gameOverTexts;
    [SerializeField]
    private GameObject winText;
    [SerializeField]
    private GameObject heavyWorldObjects;
    [SerializeField]
    private Magnetic healPrefab;

    [Header("Pause Menu")]
    [SerializeField]
    private PauseController pauseController;
    [SerializeField]
    private List<Animator> pauseTitleAnims;
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private LightingPostProcess postProcess;
    [SerializeField]
    private GameObject fpsCounter;

    [Header("Boss Health")]
    [SerializeField]
    private Animator bossHealthBarAnim;
    [SerializeField]
    private Animator bossHealthBarSawAnim;
    [SerializeField]
    private Image bossHealthBarMainFill;
    [SerializeField]
    private Image bossHealthBarBGFill;

    [Header("Data Management")]
    [SerializeField]
    private DataPack savedData;
    [SerializeField]
    private List<GameObject> sceneObjects;

    [Header("Parents")]
    [SerializeField]
    private Transform parentBullets;
    [SerializeField]
    private Transform parentObjects;
    [SerializeField]
    private Transform parentGhosting;
    [SerializeField]
    private Transform parentHeartsUI;
    [SerializeField]
    private Transform parentHeals;

    [GambaHeader("Settings ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private float damageScreenDuration = 1;
    [SerializeField]
    [Range(0, 1)]
    private float damageScreenIncrement = 0.3f;
    [SerializeField]
    private float squrFarDistance = 800;

    [Header("HeartsUI")]
    [SerializeField]
    private float heartsSeparation = 1;
    [SerializeField]
    private Sprite heartSprite;
    [SerializeField]
    private Vector2 heartsInitialPos = new Vector2 (20,0);

    [Header("Pause")]
    [SerializeField]
    private Transition hudTransition;

    [Header("Audio Mixer Transitions")]
    [SerializeField]
    private Transition lowPassFilterTransition;
    [SerializeField]
    private float LPFDefaultValue = 5000;
    [SerializeField]
    private float LPFLowValue = 150;
    [Space()]
    [SerializeField]
    private Transition pauseVolumeTransition;
    [SerializeField]
    private float pauseLowValue = 0.5f;

    [Header("Blur Transition")]
    [SerializeField]
    private Transition blurTransition;
    [SerializeField]
    private float blurMaxAmount = 50;

    [GambaHeader("Info ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [ReadOnly,SerializeField]
    private float healthRegen;
    [ReadOnly,SerializeField]
    private float playerHp;
    [ReadOnly,SerializeField]
    private float playerMaxHp;
    [ReadOnly,SerializeField]
    private float damageScreenAlpha;
    [ReadOnly, SerializeField]
    private Player.ColorState activeColor;
    [ReadOnly, SerializeField]
    private bool respawnable;
    [ReadOnly, SerializeField]
    private bool skippable;

    private Action cancelDeathScreenRespawn;

    public static bool bossKilled;

    public static TargetInput targetInput = TargetInput.MouseKeyboard;

    public static Platform TargetPlatform { get => Instance.targetPlatform; }

    private static int muteSoundMult = 1;

    private static float masterVolume = 1;

    private static float renderResolutionScale = 1;
    private static Transform ParentHeals => Instance.parentHeals;

    public static float RenderResolutionScale
    {
        get => renderResolutionScale; 
        set
        {
            renderResolutionScale = Math.Max(value, 0.1f);
            Instance.postProcess.renderResolutionScale = renderResolutionScale;
        }
    }

    public static float MasterVolume { get => masterVolume * muteSoundMult; set => masterVolume = value; }
    public static bool reloading { get; private set; }
    public static bool GamePaused { get; private set; }
    public static Player Player => Instance?.player; 
    public static Transform ParentBullets => Instance?.parentBullets; 
    public static Transform ParentGhosting => Instance?.parentGhosting; 
    public static Canvas Canvas => Instance?.canvas;
    public static CameraController CameraController => Instance?.cameraController;

    public event Action<Player.ColorState> onColorChange;
    public event Action onCombat;
    public event Action onPlayerDeath;
    public event Action onLoadData;
    public event Action<float> onHealthChange;

    public static readonly byte heartsAbsoluteMax = 10; // Max Life Amount

    #region Singleton

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameManager sceneResult = FindObjectOfType<GameManager>();
                if (sceneResult != null)
                {
                    instance = sceneResult;
                }
                else if (Application.isPlaying)
                {
                    instance = new GameObject($"{instance.GetType().ToString()}_Instance", typeof(GameManager)).GetComponent<GameManager>();
                }
            }

            return instance;
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        OnStart();
    }

    #endregion

    private void OnStart()
    {
        Application.targetFrameRate = 144;
        QualitySettings.vSyncCount = 0;

        player.Init();

        player.onCombat += OnCombat;

        player.onDeath += onPlayerDeath;
        player.onUpdateColor += UpdateColorHud;

        ButtonBase.onHover -= OnButtonHoverSound;
        ButtonBase.onPress -= OnButtonPressedSound;

        ButtonBase.onHover += OnButtonHoverSound;
        ButtonBase.onPress += OnButtonPressedSound;

        if (damageScreenDuration <= 0)
        {
            damageScreenDuration = 0.1f;
        }

        lowPassFilterTransition.value = LPFDefaultValue;
        pauseVolumeTransition.value = 1;

        bossKilled = false;

        SetPause(false);

        UpdateWalls();

        playerHp = Player.Health;
        playerMaxHp = Player.MaxHealth;

        healthController.Init((int)playerMaxHp);

        UpdateColorHud();

        SaveData();

        deathScreenAnim.gameObject.SetActive(true);

        deathScreenAnim.SetBool("Start", true);
        player.SetImmobile(false);
        AdaptativeMusic.MasterVolumeTransition(AdaptativeMusic.DefaultMasterVolume);
    }

    private void Update()
    {
        savedData.CopyObjectsUpdate();

        MainInputs();

        DetectHealthChanges();
        DamageScreenUpdate();
        DetectColorChanges();
        BossHealthBarUpdate();

        TransitionsUpdate();
    }

    #region Inputs

    private void MainInputs()
    {
        if (Input.GetButtonDown("Respawn"))
        {
            if (respawnable)
            {
                respawnable = false;
                skippable = false;
                deathScreenAnim.SetBool("Active", false);
                AudioPlayer.PlaySFX(AudioPlayer.SFXTag.Restart);
                if (bossKilled)
                {
                    Timer.CallOnDelay(RestartLevels, 2, "Restart");
                    winText.SetActive(false);
                }
                else
                {
                    Timer.CallOnDelay(Respawn, 2, "Respawn");
                }
            }
            else if (skippable)
            {
                SkipGameOver();
            }
        }

        if (GamePaused)
        {
            if (Input.GetButtonDown("Pause"))
            {
                SetPause(false);
            }
        }
        else
        {
            if (!player.Dead && player.GetPlayerState() != Player.PlayerState.NoInput && !bossKilled) // change to pausable
            {
                if (Input.GetButtonDown("Pause"))
                {
                    SetPause(true);
                }
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region UIControl

    #region Health

    public void IncreaseRegen(float amount)
    {
        if (playerHp == playerMaxHp) return;

        healthRegen += amount;

        if (healthRegen >= 1)
        {
            playerHp++;
            player.Health++;
            healthRegen = 0;
            healthController.UpdateHealth((int)playerHp);
        }

        healthController.UpdateRegen(healthRegen);
    }

    private void DetectHealthChanges()
    {
        if (playerHp != Player.Health)
        {
            if (Player.Health < playerHp)
            {
                // Lost health;
                damageScreenAlpha += damageScreenIncrement;
                //HeartsAnimSetTrigger("Health Down");

                AudioPlayer.PlaySFX(AudioPlayer.SFXTag.DamageTaken);
            }
            else
            {
                //Gained health
                damageScreenAlpha -= damageScreenIncrement;
                //HeartsAnimSetTrigger("Health Up");
                AudioPlayer.PlaySFX(AudioPlayer.SFXTag.HealthRegen);
            }

            playerHp = Player.Health;
            playerMaxHp = Player.MaxHealth;

            healthController.UpdateHealth((int)playerHp);
            if (playerHp == 0) healthController.UpdateRegen(0);

            onHealthChange?.Invoke(playerHp);
        }
        else if (playerMaxHp != Player.MaxHealth)
        {
            playerMaxHp = Player.MaxHealth;

            healthController.UpdateMaxHealth(playerMaxHp);
        }
    }

    private void DamageScreenUpdate()
    {
        if (damageScreenAlpha > 0)
        {
            damageScreenAlpha -= Time.deltaTime / damageScreenDuration;
        }
        else if (damageScreenAlpha < 0)
        {
            damageScreenAlpha = 0;
        }

        damageScreen.color = new Color(damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, damageScreenAlpha);
    }

    #endregion

    #region Color State

    private void UpdateColorHud()
    {
        int colorAmount = Enum.GetValues(typeof(Player.ColorState)).Length - 1;

        for (int i = 0; i < colorAmount; i++)
        {
            if (i <= Player.GetUnlockedColors())
            {
                if ((int)Player.GetColorState() == i)
                {
                    colorArrows[i].SetState(ColorArrowsUI.ArrowState.Selected);
                }
                else
                {
                    colorArrows[i].SetState(ColorArrowsUI.ArrowState.Deselected);
                }
            }
            else
            {
                colorArrows[i].SetState(ColorArrowsUI.ArrowState.Locked);
            }
        }
    }

    #endregion

    #region Boss

    private void BossHealthBarUpdate()
    {
        if (IsBossAwakened())
        {
            bossHealthBarMainFill.fillAmount = boss.Health / boss.MaxHealth;

            bossHealthBarBGFill.fillAmount += (bossHealthBarMainFill.fillAmount - bossHealthBarBGFill.fillAmount) * 0.1f * Time.deltaTime / 0.02f;
        }
    }

    #endregion

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Walls

    private void DetectColorChanges()
    {
        if (activeColor != GetActiveColor())
        {
            activeColor = GetActiveColor();

            UpdateWalls();
            UpdateColorHud();

            onColorChange?.Invoke(activeColor);
        }
    }

    private void UpdateWalls()
    {
        for (int i = 0; i < colorWalls.Count; i++)
        {
            if (i == (int)GetActiveColor())
            {
                // Deactivate
                colorWalls[i].SetEnable(false);
            }
            else
            {
                // Activate
                colorWalls[i].SetEnable(true);
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region DataManagement

    public void SaveData()
    {
        savedData.Save(sceneObjects, player.transform.position, player.Health, player.GetUnlockedColors(), player.GetColorState(), healthRegen, parentObjects);
    }

    public void SaveDataInPosition(Transform overrideTransform)
    {
        player.AddHealth(1);
        savedData.Save(sceneObjects, overrideTransform.position, player.Health, player.GetUnlockedColors(), player.GetColorState(), healthRegen, parentObjects);
    }

    public void LoadData()
    {
        savedData.Load(sceneObjects, player, this, ref sceneObjects);

        playerHp = Player.Health;

        healthController.UpdateMaxHealth(playerMaxHp);
        healthController.UpdateHealth((int)playerHp);
        healthController.UpdateRegen(healthRegen);

        SetPause(false);

        onLoadData?.Invoke();
        onColorChange?.Invoke(Player.GetColorState());

        onHealthChange?.Invoke(playerHp);
    }

    public static void Respawn()
    {
        reloading = false;

        Instance.player.SetImmobile(false);

        Instance.damageScreenAlpha = 0;

        Instance.LoadData();
        Instance.SaveData();

        Instance.player.Respawn();

        SetFade(false);
    }

    private void Restart()
    {
        reloading = false;

        boss?.ResetBoss();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RestartLevels()
    {
        boss?.ResetBoss();

        SceneManager.LoadScene("Level 1");
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public static void SetPause(bool value)
    {
        GamePaused = value;

        if (value)
        {
            // Paused
            Time.timeScale = 0;

            SetBossHealthBar(false);

            Instance.postProcess.intoPause = true;

            if (TargetPlatform != Platform.WebGL) Instance.lowPassFilterTransition.StartTransitionUnscaled(Instance.LPFLowValue, false);
            if (TargetPlatform == Platform.WebGL) Instance.pauseVolumeTransition.StartTransitionUnscaled(Instance.pauseLowValue, false);

            Instance.blurTransition.StartTransitionUnscaled(Instance.blurMaxAmount, () =>
            {
                if (Instance.heavyWorldObjects) Instance.heavyWorldObjects.SetActive(false);
                Instance.postProcess.PauseShaders(true);
                Instance.postProcess.SetBlurAmount(Instance.blurMaxAmount);
            });

            Instance.hudTransition.StartTransitionUnscaled(0);
            Instance.pauseController.SetPauseTransition(true);

            Instance.unpausedAnim.SetBool("Paused", true);

            foreach (Animator anim in Instance.pauseTitleAnims)
            {
                anim.SetInteger("Color", (int)Player.GetColorState());
            }
        }
        else
        {
            // Not Paused
            Player.CancelKinetic();

            if (Instance.heavyWorldObjects) Instance.heavyWorldObjects.SetActive(true);

            Time.timeScale = 1;

            SetBossHealthBar(IsBossAwakened());

            Instance.postProcess.intoPause = false;

            Instance.postProcess.PauseShaders(false);

            if (TargetPlatform != Platform.WebGL) Instance.lowPassFilterTransition.StartTransition(Instance.LPFDefaultValue, true);
            if(TargetPlatform == Platform.WebGL) Instance.pauseVolumeTransition.StartTransitionUnscaled(1, true);
            Instance.blurTransition.StartTransition(0, () =>
            {
                Instance.postProcess.SetBlurAmount(0);
            });
            Instance.hudTransition.StartTransition(1, () => Instance.UpdateColorHud());

            Instance.pauseController.SetPauseTransition(false);

            Instance.unpausedAnim.SetBool("Paused", false);
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        Player.MaxHealth += amount;
        Player.AddHealth(amount);

        healthController.IncreaseMaxHealth();
    }

    public void ReloadScene()
    {
        reloading = true;
        SetFade(true);
        AdaptativeMusic.MasterVolumeTransitionUnscaled(0);
        SetPause(false);
        player.SetImmobile(true);
        SetBossHealthBar(false);

        Timer.CallOnDelayUnscaled(Restart, 2);
    }

    public void LastCheckpoint()
    {
        reloading = true;
        SetFade(true);
        AdaptativeMusic.MasterVolumeTransitionUnscaled(0);
        SetPause(false);
        player.SetImmobile(true);
        SetBossHealthBar(false);

        Timer.CallOnDelayUnscaled(Respawn, 2);
    }

    public void MuteSound(bool value)
    {
        muteSoundMult = value? 0 : 1;
    }

    public void SetFpsCap(int targetFps)
    {
        Application.targetFrameRate = targetFps;
    }

    public void ShowFps(bool value)
    {
        if (fpsCounter != null)
        {
            fpsCounter.SetActive(value);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    public static Player.ColorState GetActiveColor()
    {
        return Player.GetColorState();
    }

    public static bool IsOnActiveDistance(Vector2 point)
    {
        return (point - (Vector2)Player.transform.position).sqrMagnitude < Instance.squrFarDistance;
    }

    public static bool IsOnActiveDistance(Vector2 point, float sum)
    {
        return (point - (Vector2)Player.transform.position).sqrMagnitude - Mathf.Pow(sum, 2) < Instance.squrFarDistance;
    }

    public static bool IsOnActiveDistance(Vector2 point, float sum, float multiplier)
    {
        return (point - (Vector2)Player.transform.position).sqrMagnitude / Mathf.Pow(multiplier, 2) - Mathf.Pow(sum, 2) < Instance.squrFarDistance;
    }

    public static void PlayDeathScreen()
    {
        Instance.deathScreenAnim.SetBool("Active", true);
        SetBossHealthBar(false);

        Instance.skippable = true;

        Timer.CallOnDelay(() =>
        {
            Instance.respawnable = true;
        }, 4, ref Instance.cancelDeathScreenRespawn, "Respawnable Death Screen");
    }

    public static void PlayWinScreen()
    {
        foreach (Text t in Instance.gameOverTexts)
        {
            if (t != null)
            {
                t.text = "YOU WIN";
            }
        }

        Instance.deathScreenAnim.SetBool("Active", true);
        SetBossHealthBar(false);

        Timer.CallOnDelay(() =>
        {
            Instance.winText.SetActive(true);
            Instance.respawnable = true;
        }, 4);
    }

    public static void GoToLevel(int buildIndex)
    {
        SetFade(true);
        AdaptativeMusic.MasterVolumeTransitionUnscaled(0);

        Timer.CallOnDelayUnscaled(() => SceneManager.LoadScene(buildIndex), 2, $"Changing Level to {SceneManager.GetSceneAt(buildIndex).name}");
    }

    public static void GoToLevel(string sceneName)
    {
        SetFade(true);
        AdaptativeMusic.MasterVolumeTransitionUnscaled(0);

        Timer.CallOnDelayUnscaled(() => SceneManager.LoadScene(sceneName), 2, $"Changing Level to {sceneName}");
    }

    public static bool IsBossAwakened()
    {
        bool r = false;

        if (Instance.boss != null)
        {
            r = Instance.boss.Awakened;
        }

        return r;
    }

    public static void SetBossHealthBar(bool value)
    {
        Instance.bossHealthBarAnim.SetBool("Visible", value);
    }

    public static void BossHealthBarChangeState()
    {
        Instance.bossHealthBarSawAnim.SetTrigger("Change State");
    }

    public static void SetFade(bool enable, float playbackSpeed = 1)
    {
        Instance.fadeAnim.speed = playbackSpeed;
        Instance.fadeAnim.SetBool("Visible", enable);
    }

    public static void Heal(float amount)
    {
        if (Player.Health < Player.MaxHealth)
        {
            Instance.IncreaseRegen(amount);
        }
    }

    public static void SpawnHealsAtPos(int amount, Vector3 position, float radius, float speed = 0) => Instance.SpawnHeals(amount, position, radius, speed);

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Others

    private void SkipGameOver()
    {
        skippable = false;

        respawnable = true;
        cancelDeathScreenRespawn?.Invoke();

        deathScreenAnim.SetTrigger("Skip");
    }

    private void TransitionsUpdate()
    {
        if (TargetPlatform == Platform.WebGL) pauseVolumeTransition.UpdateTransitionValue();
        if (TargetPlatform != Platform.WebGL) lowPassFilterTransition.UpdateTransitionValue();
        blurTransition.UpdateTransitionValue();
        hudTransition.UpdateTransitionValue();

        if (lowPassFilterTransition.IsOnTransition)
        {
            if (TargetPlatform != Platform.WebGL) audioMixer?.SetFloat("LowPass Cutoff", lowPassFilterTransition.value);
        }

        if (pauseVolumeTransition.IsOnTransition)
        {
            if (TargetPlatform == Platform.WebGL) audioMixer?.SetFloat("Game Volume", pauseVolumeTransition.value);
        }

        if (postProcess != null)
        {
            if (blurTransition.IsOnTransition)
            {
                postProcess.SetBlurAmount(blurTransition.value);
            }
        }

        if (hudTransition.IsOnTransition)
        {
            for (int i = 0; i < colorArrows.Count; i++)
            {
                ColorArrowsUI c = colorArrows[i];
                if (c.arrow != null)
                {
                    c.arrow.color = new Color(c.arrow.color.r, c.arrow.color.g, c.arrow.color.b, hudTransition.value * c.CurrentAlpha);
                }

                if (c.lockedArrow != null)
                {
                    c.lockedArrow.color = new Color(c.lockedArrow.color.r, c.lockedArrow.color.g, c.lockedArrow.color.b, hudTransition.value * c.CurrentAlpha);
                }
            }
        }
    }

    private void OnCombat()
    {
        if (onCombat.Target != null)
        {
            onCombat?.Invoke();
        }
    }

    private static void OnButtonHoverSound(Button.ButtonTag tag)
    {
        AudioPlayer.SFXTag sfxTag = AudioPlayer.SFXTag.UIButtonHover_Base;

        switch (tag)
        {
            case ButtonBase.ButtonTag.Base:
                sfxTag = AudioPlayer.SFXTag.UIButtonHover_Base;
                break;
            case ButtonBase.ButtonTag.Back:
                sfxTag = AudioPlayer.SFXTag.UIButtonHover_Base;
                break;
        }

        AudioPlayer.PlaySFX(sfxTag);
    }

    private static void OnButtonPressedSound(Button.ButtonTag tag)
    {
        AudioPlayer.SFXTag sfxTag = AudioPlayer.SFXTag.UIButtonPress_Base;

        switch (tag)
        {
            case ButtonBase.ButtonTag.Base:
                sfxTag = AudioPlayer.SFXTag.UIButtonPress_Base;
                break;
            case ButtonBase.ButtonTag.Back:
                sfxTag = AudioPlayer.SFXTag.UIButtonPress_Back;
                break;
        }

        AudioPlayer.PlaySFX(sfxTag);
    }

    private void SpawnHeals(int amount, Vector2 position, float radius, float speed = 0)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnRandomHeal(position, radius, speed);
        }
    }

    private void SpawnRandomHeal(Vector2 position, float radius, float speed = 0)
    {
        Magnetic heal = Instantiate(healPrefab, ParentHeals);

        heal.name = healPrefab.name;

        Vector2 relativePos = GetRandomPos(radius);
        heal.transform.position = position + relativePos;

        Vector2 velocity = relativePos.normalized *  speed;
        heal.Init(velocity);
    }

    private Vector3 GetRandomPos(float radius) => new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized * UnityEngine.Random.Range(0f, 1f) * radius;

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        int colorAmount = Enum.GetValues(typeof(Player.ColorState)).Length - 1;

        #region ColorWalls

        if (colorWalls == null || colorWalls.Count != colorAmount)
        {
            colorWalls = new List<ColorWall>();

            for (int i = 0; i < colorAmount; i++)
            {
                colorWalls.Add(new ColorWall(((Player.ColorState)i).ToString()));
            }
        }

        #endregion

        #region ColorArrowsUI

        if (colorArrows == null || colorArrows.Count != colorAmount)
        {
            colorArrows = new List<ColorArrowsUI>();

            for (int i = 0; i < colorAmount; i++)
            {
                colorArrows.Add(new ColorArrowsUI());
                colorArrows[i].SetName(i);
            }
        }

        #endregion

    }

#endif

    #endregion

}