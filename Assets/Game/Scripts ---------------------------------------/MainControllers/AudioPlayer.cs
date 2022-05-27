using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    [Serializable]
    public class SFXClip
    {
        [SerializeField, HideInInspector] private string name;

        public AudioClip clip;
        [Range(0f,1f)]
        public float volume = 1;
        [Header("2D - 3D")]
        [Range(0f, 1f)]
        public float spatialBlend = 0;
        [SerializeField]
        private float spatialRange = 5;
        public MixerTag targetMixer;

        public void Play(AudioSource source) 
        {
            if (clip != null)
            {
                source.PlayOneShot(clip, volume * MasterVolume * ultraMasterVolume * GameManager.MasterVolume);
            }
        }

        public void Play(AudioSource source, float distance) 
        {
            if (clip != null)
            {
                float totalInfluence;
                if (distance != 0)
                {
                    if (spatialRange > 0)
                    {
                        float distanceMult = Mathf.Log(1 + spatialRange / distance);
                        totalInfluence = Mathf.Clamp01(Mathf.Lerp(1, distanceMult, spatialBlend));
                    }
                    else
                    {
                        totalInfluence = 0;
                    }
                }
                else
                {
                    totalInfluence = 1;
                }
                source.PlayOneShot(clip, volume * MasterVolume * ultraMasterVolume * GameManager.MasterVolume * totalInfluence);
            }
        }

        public void SetName(string name)
        {
            string state = (clip == null)? "null" : clip.name;
            this.name = name + " : " + state;
        }
    }

    [Serializable]
    public class SFXLoopClip
    {
        [SerializeField, HideInInspector] private string name;

        public AudioClip clip;
        [Range(0f,1f)]
        public float volume = 1;
        [Range(0f, 1f)]
        public float spatialBlend = 0;
        public float transitionDuration;
        public MixerTag targetMixer;

        [ReadOnly]
        public bool enabled;
        [ReadOnly]
        public bool isOnTransition;
        [ReadOnly]
        [Range(0f,1f)]
        public float weight;

        public void SetName(string name)
        {
            string state = (clip == null) ? "null" : clip.name;
            this.name = name + " : " + state;
        }
    }

    [Serializable]
    private class CacheSFX
    {
        public SFXTag tag;
        public float timer;

        public CacheSFX(SFXTag tag, float timer)
        {
            this.tag = tag;
            this.timer = timer;
        }
    }

    public enum MixerTag
    {
        Game,
        UI
    }

    public enum SFXTag
    {
        Death,
        Dash,
        Melee,
        Range,
        Kinetic,
        Electric,

        Lazered,
        BoxHit,
        Pickup,
        Door,
        Button,

        MeleeEnemy,
        RangeEnemy,
        SniperEnemy,

        //New sounds
        BoxDead,
        Melee2,
        Melee3,
        HealthRegen,
        HealthUp,
        Upgrade,
        DamageTaken,
        EnemyHit,
        EnemyDead,
        DashAvailable,
        Restart,

        //Boss
        BossIntro,
        BossDashWindUp,
        BossDashEnd,
        BossDeath,

        //Button
        UIButtonHover_Base,
        UIButtonPress_Base,
        UIButtonPress_Back,

        //Boss2
        Boss2Intro,

        ExplosiveBarrel,
        ExplosionWindUp,
        MagneticHeal,
    }

    public enum SFXLoopTag
    {
        BossDash,
    }

    [Header("Components")]
    [SerializeField]
    private AudioSource sfxAudioSourceGame;
    [SerializeField]
    private AudioSource sfxAudioSourceUI;
    [SerializeField]
    private Transform sfxLoopParent;

    [Header("Settings")]
    [SerializeField]
    [Range(0f,1f)]
    private float masterVolume = 1;
    [SerializeField]
    private AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    [Range(0, 0.2f)]
    private float doubleSoundsSeparation = 0.05f;

    [Header("Clips")]
    [SerializeField]
    private List<SFXClip> sfxClips = new List<SFXClip>();
    [SerializeField]
    private List<SFXLoopClip> sfxLoopClips = new List<SFXLoopClip>();

    [Header("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")]
    [ReadOnly, SerializeField]
    private List<AudioSource> sfxLoopAudioSources = new List<AudioSource>();
    [SerializeField]
    private List<CacheSFX> cacheSFX = new List<CacheSFX>();

    public static float ultraMasterVolume = 1;

    private Transform SFXLoopParent
    {
        get
        {
            if (sfxLoopParent == null)
            {
                sfxLoopParent = new GameObject("LoopsParent").transform;
                SFXLoopParent.SetParent(transform);
            }

            return sfxLoopParent;
        }
    }

    public static float MasterVolume
    {
        get => Instance.masterVolume; 
        
        set
        {
            Instance.masterVolume = Mathf.Clamp01(value);
            foreach (SFXLoopClip l in Instance.sfxLoopClips)
            {
                if (l != null)
                {
                    l.isOnTransition = true;
                }
            }
        }
    }

    #region Singleton

    private static AudioPlayer instance = null;
    public static AudioPlayer Instance
    {
        get
        {
            if (instance == null)
            {
                AudioPlayer sceneResult = FindObjectOfType<AudioPlayer>();
                if (sceneResult != null)
                {
                    instance = sceneResult;
                }
                else
                {
                    instance = new GameObject($"{instance.GetType().ToString()}_Instance", typeof(AudioPlayer)).GetComponent<AudioPlayer>();
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
        GambaFunctions.ResizeList(ref sfxClips, Enum.GetNames(typeof(SFXTag)).Length);
        GambaFunctions.ResizeList(ref sfxLoopClips, Enum.GetNames(typeof(SFXLoopTag)).Length);

        UpdateLoopsParent();
    }

    void Update()
    {
        SFXLoopUpdate();
        UpdateCacheSFX();
    }

    #region Update

    private void SFXLoopUpdate()
    {
        for (int i = 0; i < sfxLoopAudioSources.Count; i++)
        {
            if (sfxLoopAudioSources[i] != null && i < sfxLoopClips.Count && sfxLoopClips[i] != null)
            {
                SFXLoopClip sfx = sfxLoopClips[i];

                if (sfx.isOnTransition)
                {
                    int dir = sfx.enabled ? 1 : -1;
                    int targetWeight = sfx.enabled ? 1 : 0;
                    if (sfx.transitionDuration > 0)
                    {
                        sfx.weight += Time.deltaTime / sfx.transitionDuration * dir;
                        sfx.weight = Mathf.Clamp01(sfx.weight);
                    }
                    else
                    {
                        sfx.weight = targetWeight;
                    }

                    sfxLoopAudioSources[i].volume = transitionCurve.Evaluate(sfx.weight) * sfx.volume * MasterVolume * ultraMasterVolume * GameManager.MasterVolume;

                    if (sfx.weight == targetWeight)
                    {
                        sfx.isOnTransition = false;
                    }
                }
            }
            else
            {
                UpdateLoopsParent();
                break;
            }
        }
    }

    private void UpdateLoopsParent()
    {
        List<AudioSource> existingAudioSources = new List<AudioSource>();

        SFXLoopParent.GetComponents(existingAudioSources);

        if (existingAudioSources.Count > sfxLoopClips.Count)
        {
            for (int i = existingAudioSources.Count - 1; i >= 0; i--)
            {
                if (i >= sfxLoopClips.Count)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(existingAudioSources[i]);
                    }
                    else
                    {
#if UNITY_EDITOR

                        GambaFunctions.DestroyInEditor(existingAudioSources[i]);
#endif
                    }
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
            for (int i = 0; i < sfxLoopClips.Count; i++)
            {
                if (i >= existingAudioSources.Count)
                {
                    existingAudioSources.Add((AudioSource)SFXLoopParent.gameObject.AddComponent(typeof(AudioSource)));
                }
            }
        }

        sfxLoopAudioSources = existingAudioSources;

        for (int i = 0; i < sfxLoopAudioSources.Count; i++)
        {
            if (i < sfxLoopClips.Count)
            {
                sfxLoopAudioSources[i].clip = sfxLoopClips[i].clip;
                sfxLoopAudioSources[i].playOnAwake = true;
                sfxLoopAudioSources[i].loop = true;
                sfxLoopAudioSources[i].Play();
                sfxLoopAudioSources[i].volume = sfxLoopClips[i].enabled? sfxLoopClips[i].volume : 0;
                sfxLoopAudioSources[i].outputAudioMixerGroup = GetTargetMixerGroup(sfxLoopClips[i].targetMixer);
            }
        }
    }

    private void UpdateCacheSFX()
    {
        for (int i = 0; i < cacheSFX.Count; i++)
        {
            CacheSFX cache = cacheSFX[i];

            Timer.ReduceCooldown(ref cache.timer, () => cacheSFX.Remove(cache));
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    public static void PlaySFX(SFXTag tag)
    {
        if (Instance.CheckCacheSFX(tag)) return;

        if (Instance.sfxClips != null && (int)tag < Instance.sfxClips.Count)
        {
            SFXClip clip = Instance.sfxClips[(int)tag];

            clip.Play(Instance.GetTargetAudioSource(clip.targetMixer));

            Instance.AddCacheSFX(tag);
        }
    }

    public static void PlaySFX(SFXTag tag, Vector3 position)
    {
        if (Instance.sfxClips != null && (int)tag < Instance.sfxClips.Count)
        {
            SFXClip clip = Instance.sfxClips[(int)tag];

            clip.Play(Instance.GetTargetAudioSource(clip.targetMixer), (GameManager.Player.transform.position - position).magnitude);
        }
    }

    public static void SetSFXLoop(SFXLoopTag tag, bool value)
    {
        if (Instance.sfxLoopClips != null && (int)tag < Instance.sfxLoopClips.Count)
        {
            SFXLoopClip clip = Instance.sfxLoopClips[(int)tag];

            clip.enabled = value;
            clip.isOnTransition = true;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private AudioSource GetTargetAudioSource(MixerTag mixer)
    {
        switch (mixer)
        {
            case MixerTag.Game:
                return sfxAudioSourceGame;
            case MixerTag.UI:
                return sfxAudioSourceUI;
        }

        return null;
    }

    private AudioMixerGroup GetTargetMixerGroup(MixerTag mixer)
    {
        switch (mixer)
        {
            case MixerTag.Game:
                return sfxAudioSourceGame.outputAudioMixerGroup;
            case MixerTag.UI:
                return sfxAudioSourceUI.outputAudioMixerGroup;
        }

        return null;
    }


    private bool CheckCacheSFX(SFXTag tag) => cacheSFX.Exists(sfx => sfx.tag == tag);

    private void AddCacheSFX(SFXTag tag) => cacheSFX.Add(new CacheSFX(tag, doubleSoundsSeparation));

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    private void OnValidate()
    {

        #region ListUpdate

        // SFX ----------------------------------------------------------
        if (sfxClips == null)
        {
            sfxClips = new List<SFXClip>();
        }

        int sfxLenght = Enum.GetNames(typeof(SFXTag)).Length;

        if (sfxClips.Count != sfxLenght)
        {
            GambaFunctions.ResizeList(ref sfxClips, sfxLenght);
        }

        for (int i = 0; i < sfxClips.Count; i++)
        {
            sfxClips[i].SetName(((SFXTag)i).ToString());
        }

        // SFX Loopable -------------------------------------------------
        if (sfxLoopClips == null)
        {
            sfxLoopClips = new List<SFXLoopClip>();
        }

        int sfxLoopableLenght = Enum.GetNames(typeof(SFXLoopTag)).Length;

        if (sfxLoopClips.Count != sfxLoopableLenght)
        {
            GambaFunctions.ResizeList(ref sfxLoopClips, sfxLoopableLenght);
        }

        for (int i = 0; i < sfxLoopClips.Count; i++)
        {
            sfxLoopClips[i].SetName(((SFXLoopTag)i).ToString());
        }

        #endregion

        #region LoopAudioSources

        if (sfxLoopAudioSources == null)
        {
            sfxLoopAudioSources = new List<AudioSource>();
        }

        if (sfxLoopAudioSources.Count != sfxLoopClips.Count)
        {
            UpdateLoopsParent();
        }

        #endregion

    }

    #endregion

}