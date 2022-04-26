using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Slider : MonoBehaviour
{
    private enum TargetOption
    {
        // Audio
        MasterVolume,
        MusicVolume,
        SfxVolume,

        // VIdeo
        RenderResolution
    }

    [Header("Components")]
    [SerializeField]
    private ButtonBase button;
    [SerializeField]
    private Transform head;
    [SerializeField]
    private Image bg;
    [SerializeField]
    private Image bar;
    [Header("Settings")]
    [SerializeField]
    private float size = 5;
    [SerializeField]
    private TargetOption targetOption;
    [Header("Info")]
    [SerializeField]
    [Range(0f,1f)]
    private float value = 0.5f;

    private float savedMasterVol = -1;
    private float savedMusicVol = -1;
    private float savedSfxVol = -1;

    private bool pressed;

    private float BGDistance { get => GameManager.Canvas? BGSize * GameManager.Canvas.scaleFactor : 0; }
    private float BGSize { get => GameManager.Canvas? size * GameManager.Canvas.referencePixelsPerUnit: 0; }

    private float TargetValue 
    {
        get
        {
            switch (targetOption)
            {
                case TargetOption.MasterVolume:
                    return GameManager.MasterVolume;
                case TargetOption.MusicVolume:
                    return AdaptativeMusic.ultraMasterVolume;
                case TargetOption.SfxVolume:
                    return AudioPlayer.ultraMasterVolume;
                case TargetOption.RenderResolution:
                    return GameManager.RenderResolutionScale;
            }
            return 0.5f;
        }

        set
        {
            switch (targetOption)
            {
                case TargetOption.MasterVolume:
                    GameManager.MasterVolume = value;
                    savedMasterVol = value;
                    break;
                case TargetOption.MusicVolume:
                    AdaptativeMusic.ultraMasterVolume = value;
                    savedMusicVol = value;
                    break;
                case TargetOption.SfxVolume:
                    AudioPlayer.ultraMasterVolume = value;
                    savedSfxVol = value;
                    break;
                case TargetOption.RenderResolution:
                    GameManager.RenderResolutionScale = 0.1f + value * 0.9f;
                    break;
            }
        }
    }

    void Start()
    {
        switch (targetOption)
        {
            case TargetOption.MasterVolume:
                if (savedMasterVol != -1) TargetValue = savedMasterVol;
                break;
            case TargetOption.MusicVolume:
                if (savedMusicVol != -1) TargetValue = savedMusicVol;
                break;
            case TargetOption.SfxVolume:
                if (savedSfxVol != -1) TargetValue = savedSfxVol;
                break;
        }

        value = TargetValue;

        if (head)
        {
            head.transform.position = ValueToPosition(value);
            UpdateBarSize(value);
        }
    }

    void Update()
    {
        InteractionUpdate();
        UpdateButtonPressed();
    }

    private void InteractionUpdate()
    {
        if (button && head && bg)
        {
            if (pressed)
            {
                head.transform.position = new Vector2(Input.mousePosition.x, head.transform.position.y);

                value = PositionToValue();
                UpdateBarSize(value);

                head.transform.position = ValueToPosition(value);

                TargetValue = value;
            }
        }
    }

    private void UpdateButtonPressed()
    {
        if (button)
        {
            if (button.pressed)
            {
                pressed = true;
            }

            if (pressed)
            {
                button.pressed = true;

                if (Input.GetMouseButtonUp(0) || !GameManager.GamePaused)
                {
                    pressed = false;
                    button.pressed = false;
                }
            }
        }
    }

    #region Other

    private Vector2 ValueToPosition(float value)
    {
        if (bg)
        {
            return bg.transform.position + Vector3.right * BGDistance * value;
        }

        return Vector2.zero;
    }

    private float PositionToValue()
    {
        if (bg && head)
        {
            return Mathf.Clamp01((head.transform.position.x - bg.transform.position.x) / BGDistance);
        }

        return -1;
    }

    private void UpdateBarSize(float value)
    {
        if (bar)
        {
            bar.rectTransform.sizeDelta = new Vector2(BGSize * value, bar.rectTransform.sizeDelta.y);
        }
    }

    private void SetSize()
    {
        if (bg)
        {
            bg.rectTransform.sizeDelta = new Vector2(BGSize, bg.rectTransform.sizeDelta.y);

            head.transform.position = ValueToPosition(value);
            UpdateBarSize(value);
        }
    }

    #endregion

    private void OnValidate()
    {
        SetSize();
    }
}
