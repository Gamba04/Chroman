using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AdaptativeMusic : MonoBehaviour
{
    [Serializable]
    public class MusicTrack
    {
        [SerializeField, HideInInspector] private string name;

        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1;
        public AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1, 1));

        public void SetName(string name)
        {
            string state = (clip == null) ? "null" : clip.name;
            this.name = name + " : " + state;
        }
    }

    public enum CombatState
    {
        None,
        Base,
        Boss
    }

    public enum HealthState
    {
        Dead,
        VeryLow,
        Low,
        Medium,
        All
    }

    [Header("Components")]
    [SerializeField]
    private Transform colorParent;
    [SerializeField]
    private Transform combatParent;
    [SerializeField]
    private Transform healthParent;
    [SerializeField]
    private List<MusicTrack> colorTracks = new List<MusicTrack>();
    [SerializeField]
    private List<MusicTrack> combatTracks = new List<MusicTrack>();
    [SerializeField]
    private List<MusicTrack> healthTracks = new List<MusicTrack>();
    [SerializeField]
    private AudioMixerGroup mixer;

    [Header("Settings")]
    [SerializeField]
    private float combatMaxCooldown = 1;
    [Header("Master Volume Transition")]
    [SerializeField]
    private Transition masterVolumeTransition;
    [SerializeField]
    [Range(0f, 1f)]
    private float defaultMasterVolume = 0.5f;
    [GambaHeader("Loop")]
    [SerializeField]
    private AudioClip referenceClip;
    [ReadOnly, SerializeField]
    private float loopDuration;
    [SerializeField]
    [Range(0f, 1f)]
    private float loopPercent;
    [SerializeField]
    [Range(0f, 1f)]
    private float startTransition = 0.5f;

    [GambaHeader("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [GambaHeader("Info")]
    [ReadOnly, SerializeField]
    private float pitch = 1;
    [ReadOnly, SerializeField]
    private float masterVolume = 1;
    [GambaHeader("Music States", 1, 0.3f, 0.35f)]
    [ReadOnly, SerializeField]
    private Player.ColorState colorState;
    [ReadOnly, SerializeField]
    private CombatState combatState;
    [ReadOnly, SerializeField]
    private HealthState healthState;
    [ReadOnly, SerializeField]
    private List<AudioSource> colorAudioSources;
    [ReadOnly, SerializeField]
    private List<AudioSource> combatAudioSources;
    [ReadOnly, SerializeField]
    private List<AudioSource> healthAudioSources;

    private AudioSource referenceAudioSource;

    private float loopTime;
    private bool transitionStarted;
    private float transitionProgress;

    private Player.ColorState nextColorState;
    private CombatState nextCombatState;
    private HealthState nextHealthState;
    
    private Player.ColorState previousColorState;
    private CombatState previousCombatState;
    private HealthState previousHealthState;

    private float combatCooldown;

    public static float DefaultMasterVolume { get => Instance.defaultMasterVolume; set => Instance.defaultMasterVolume = value; }
    public static float MasterVolume
    {
        get => Instance.masterVolume;

        set
        {
            Instance.masterVolume = Mathf.Clamp01(value);
            //Instance.UpdateStates();
        }
    }

    public static float ultraMasterVolume = 1;
    public AudioSource ReferenceAudioSource
    {
        get
        {
            if (referenceAudioSource == null)
            {
                AudioSource checkExisting = GetComponent<AudioSource>();

                if (checkExisting == null)
                {
                    referenceAudioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
                }
                else
                {
                    referenceAudioSource = checkExisting;
                }
            }

            return referenceAudioSource;
        }
    }
    private Transform ColorParent { get => ParentGetter(ref colorParent, nameof(ColorParent)); }
    private Transform CombatParent { get => ParentGetter(ref combatParent, nameof(CombatParent)); }
    private Transform HealthParent { get => ParentGetter(ref healthParent, nameof(HealthParent)); }
    private Transform ParentGetter(ref Transform parent, string name)
    {
        if (parent == null)
        {
            parent = new GameObject(name).transform;
            parent.SetParent(transform);
        }

        return parent;
    }

    #region Singleton

    private static AdaptativeMusic instance = null;
    public static AdaptativeMusic Instance
    {
        get
        {
            if (instance == null)
            {
                AdaptativeMusic sceneResult = FindObjectOfType<AdaptativeMusic>();
                if (sceneResult != null)
                {
                    instance = sceneResult;
                }
                else
                {
                    instance = new GameObject($"{instance.GetType().ToString()}_Instance", typeof(AdaptativeMusic)).GetComponent<AdaptativeMusic>();
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

    void OnStart()
    {
        nextColorState = colorState;
        nextCombatState = combatState;
        nextHealthState = healthState;

        UpdateReferenceAudioSource();
        UpdateStates();

        GameManager.Instance.onColorChange += OnColorChange;
        GameManager.Instance.onCombat += OnCombat;
        GameManager.Instance.onHealthChange += OnHealthChange;
        GameManager.Instance.onPlayerDeath += OnDeath;
        GameManager.Instance.onLoadData += OnLoadData;

        MasterVolume = 0;

        SetPitch(pitch);
    }

    void Update()
    {
        masterVolumeTransition.UpdateTransitionValue();
        MasterVolume = masterVolumeTransition.value;

        CombatCooldownUpdate();

        MainUpdate();
    }

    #region StatesDetection

    private void OnColorChange(Player.ColorState state)
    {
        colorState = state;
    }

    private void OnCombat()
    {
        combatCooldown = combatMaxCooldown;
        combatState = CombatState.Base;
    }

    private void OnHealthChange(float health)
    {
        if (health <= 0)
        {
            healthState = HealthState.Dead;
        }
        else if (health <= 1)
        {
            healthState = HealthState.VeryLow;
        }
        else if (health < Player.MaxHealth)
        {
            if (health <= 2)
            {
                healthState = HealthState.Low;
            }
            else
            {
                healthState = HealthState.Medium;
            }
        }
        else
        {
            healthState = HealthState.All;
        }
    }

    private void CombatCooldownUpdate()
    {
        if (!GameManager.IsBossAwakened())
        {
            if (combatMaxCooldown > 0)
            {
                Timer.ReduceCooldown(ref combatCooldown, () => { combatState = CombatState.None; });
            }
            else
            {
                combatState = CombatState.None;
            }
        }
        else
        {
            combatState = CombatState.Boss;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region LoopUpdates

    private void MainUpdate()
    {
        if (loopDuration > 0)
        {
            if (colorAudioSources[0].time < loopTime)
            {
                transitionProgress = 1;

                loopTime = 0;
                transitionStarted = false;

                previousColorState = nextColorState;
                previousCombatState = nextCombatState;
                previousHealthState = nextHealthState;
            }

            loopTime = colorAudioSources[0].time;

            loopPercent = loopTime / loopDuration;

            if (loopPercent > startTransition)
            {
                if (!transitionStarted)
                {
                    transitionStarted = true;

                    nextColorState = colorState;
                    nextCombatState = combatState;
                    nextHealthState = healthState;
                }

                transitionProgress = (loopPercent - startTransition) / (1 - startTransition);
            }

            UpdateStates();
        }
    }

    private void UpdateStates()
    {
        UpdateTracks(ColorParent, ref colorTracks, ref colorAudioSources, (int)previousColorState + 1, (int)nextColorState + 1);
        UpdateTracks(CombatParent, ref combatTracks, ref combatAudioSources, (int)previousCombatState, (int)nextCombatState);
        UpdateTracks(HealthParent, ref healthTracks, ref healthAudioSources, (int)previousHealthState, (int)nextHealthState);
    }

    private void UpdateTracks(Transform parent, ref List<MusicTrack> tracks, ref List<AudioSource> audioSources, int previousActiveTrack, int nextActiveTrack)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (i < tracks.Count && tracks[i] != null && audioSources[i] != null)
            {
                float previousVolume = (i == previousActiveTrack) ? tracks[i].volume * MasterVolume * ultraMasterVolume * GameManager.MasterVolume : 0;
                float nextVolume = (i == nextActiveTrack) ? tracks[i].volume * MasterVolume * ultraMasterVolume * GameManager.MasterVolume : 0;

                audioSources[i].volume = Mathf.Lerp(previousVolume, nextVolume, tracks[nextActiveTrack].transitionCurve.Evaluate(transitionProgress));
            }
            else
            {
                UpdateTracksParent(parent, ref tracks, ref audioSources);
                break;
            }
        }
    }

    private void UpdateTracksParent(Transform parent, ref List<MusicTrack> tracks, ref List<AudioSource> audioSources)
    {
        if (audioSources.Count != tracks.Count)
        {
            List<AudioSource> existingAudioSources = new List<AudioSource>();

            parent.GetComponents(existingAudioSources);

            if (existingAudioSources.Count > tracks.Count)
            {
                for (int i = existingAudioSources.Count - 1; i >= 0; i--)
                {
                    if (i >= tracks.Count)
                    {
                        Destroy(existingAudioSources[i]);
                        existingAudioSources.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (i >= existingAudioSources.Count)
                    {
                        existingAudioSources.Add((AudioSource)parent.gameObject.AddComponent(typeof(AudioSource)));
                    }
                }
            }

            audioSources = existingAudioSources;
        }

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (i < tracks.Count)
            {
                audioSources[i].clip = tracks[i].clip;
                audioSources[i].playOnAwake = true;
                audioSources[i].loop = true;
                audioSources[i].outputAudioMixerGroup = mixer;
            }
        }
    }

    private void UpdateReferenceAudioSource()
    {
        if (referenceClip == null)
        {
            loopDuration = 0;
        }
        else
        {
            loopDuration = referenceClip.length;

            ReferenceAudioSource.clip = referenceClip;
            ReferenceAudioSource.loop = true;
            ReferenceAudioSource.playOnAwake = true;
            ReferenceAudioSource.volume = 0;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    public static void MasterVolumeTransition(float targetVolume)
    {
        Instance.masterVolumeTransition.StartTransition(targetVolume);
    }

    public static void MasterVolumeTransitionUnscaled(float targetVolume)
    {
        Instance.masterVolumeTransition.StartTransitionUnscaled(targetVolume);
    }

    private void OnDeath()
    {
        MasterVolumeTransition(0.2f);
    }

    private void OnLoadData()
    {
        MasterVolumeTransition(defaultMasterVolume);
    }

    public static void SetPitch(float newPitch)
    {
        for (int i = 0; i < Instance.colorAudioSources.Count; i++)
        {
            Instance.colorAudioSources[i].pitch = newPitch;
        }
        for (int i = 0; i < Instance.combatAudioSources.Count; i++)
        {
            Instance.combatAudioSources[i].pitch = newPitch;
        }
        for (int i = 0; i < Instance.healthAudioSources.Count; i++)
        {
            Instance.healthAudioSources[i].pitch = newPitch;
        }

        Instance.referenceAudioSource.pitch = newPitch;
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    private void UpdateTrackList<T>(Transform parent, ref List<MusicTrack> tracks, ref List<AudioSource> audioSources) where T : Enum
    {
        T[] enumValues = (T[])Enum.GetValues(typeof(T));

        enumValues = GambaFunctions.QuickSort(new List<T>(enumValues), (a, b) => (int)(object)a - (int)(object)b).ToArray(); // Reorder by numeric value

        if (tracks == null || tracks.Count != enumValues.Length)
        {
            GambaFunctions.ResizeList(ref tracks, enumValues.Length);
        }

        UpdateTracksParent(parent, ref tracks, ref audioSources);
          
        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].SetName(Enum.GetName(typeof(T), enumValues[i]));
        }
    }

    [GambaHeader("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------", 0, 0, 0, 0.4f)]
    [SerializeField]
    private bool makeChanges;

    private void OnValidate()
    {
        if (makeChanges)
        {
            makeChanges = false;

            // Changes ------------------------------

            //foreach (MusicTrack track in colorTracks) track.transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0), new Keyframe(1, 1));
            //foreach (MusicTrack track in combatTracks) track.transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0), new Keyframe(1, 1));
            //foreach (MusicTrack track in healthTracks) track.transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0), new Keyframe(1, 1));

            // Changes ------------------------------
        }

        UpdateReferenceAudioSource();

        UpdateTrackList<Player.ColorState>(ColorParent, ref colorTracks, ref colorAudioSources);
        UpdateTrackList<CombatState>(CombatParent, ref combatTracks, ref combatAudioSources);
        UpdateTrackList<HealthState>(HealthParent, ref healthTracks, ref healthAudioSources);
    }

    #endregion

    private void OnDestroy()
    {
        GameManager.Instance.onColorChange -= OnColorChange;
        GameManager.Instance.onCombat -= OnCombat;
        GameManager.Instance.onHealthChange -= OnHealthChange;
        GameManager.Instance.onPlayerDeath -= OnDeath;
        GameManager.Instance.onLoadData -= OnLoadData;
    }

}
