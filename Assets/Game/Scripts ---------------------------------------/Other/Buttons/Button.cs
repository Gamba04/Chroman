using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Button : ButtonBase
{
    [Header("Components")]
    [SerializeField]
    private Graphic targetGraphic;
    [Header("Settings")]
    [SerializeField]
    private Color color_Disselected = new Color(1, 1, 1, 1);
    [SerializeField]
    private Color color_Highlighted = new Color(1, 1, 1, 1);
    [SerializeField]
    private Color color_Pressed = new Color(1, 1, 1, 1);
    [SerializeField]
    private float duration;
    [SerializeField]
    private AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Color lastColor;
    private Color targetColor;
    private float progress;
    private bool unscaled;

    #region ButtonLowerLayout

    [SerializeField]
    private ButtonState state;
    [Space()]
    [SerializeField]
    private UnityEvent onClick;
    [Space()]
    [SerializeField]
    private AdvancedSettings advancedSettings;
    
    protected override ButtonState NewState { get => state; set => state = value; }
    protected override UnityEvent NewOnClick { get => onClick; set => onClick = value; }
    protected override AdvancedSettings NewAdvancedSettings { get => advancedSettings; set => advancedSettings = value; }

    #endregion

    protected override void Start()
    {
        if (targetGraphic != null)
        {
            targetGraphic.color = color_Disselected;
        }

        base.Start();
    }

    #region Overrides

    protected override void EditorUpdate()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }
    }

    protected override void RuntimeUpdate()
    {
        if (targetGraphic != null)
        {
            ColorUpdate();
        }
    }

    protected override void OnDisselect()
    {
        ChangeColorUnscaled(color_Disselected);
    }

    protected override void OnHighlight()
    {
        ChangeColorUnscaled(color_Highlighted);
    }

    protected override void OnPressed()
    {
        ChangeColorUnscaled(color_Pressed);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void ColorUpdate()
    {
        if (duration > 0)
        {
            if (progress < 1)
            {
                progress += unscaled ? Time.unscaledDeltaTime / duration : Time.deltaTime / duration;

                if (progress > 1)
                {
                    progress = 1;
                }
            }

            targetGraphic.color = Color.Lerp(lastColor, targetColor, transitionCurve.Evaluate(progress));
        }
    }

    private void ChangeColor(Color newColor)
    {
        unscaled = false;

        if (targetGraphic != null)
        {
            lastColor = targetGraphic.color;
            targetColor = newColor;

            if (duration > 0)
            {
                progress = 0;
            }
            else
            {
                targetGraphic.color = newColor;
                progress = 1;
            }
        }
    }

    private void ChangeColorUnscaled(Color newColor)
    {
        unscaled = true;

        if (targetGraphic != null)
        {
            lastColor = targetGraphic.color;
            targetColor = newColor;

            if (duration > 0)
            {
                progress = 0;
            }
            else
            {
                targetGraphic.color = newColor;
                progress = 1;
            }
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    // Show last modified color on scene or direct selection
#if UNITY_EDITOR

    private Color preDiss = new Color(1, 1, 1, 1);
    private Color preHigh = new Color(1, 1, 1, 1);
    private Color prePress = new Color(1, 1, 1, 1);
    private ButtonState preState;

    private void OnValidate()
    {
        if (targetGraphic != null)
        {
            if (color_Disselected != preDiss)
            {
                targetGraphic.color = color_Disselected;
            }
            else if (color_Highlighted != preHigh)
            {
                targetGraphic.color = color_Highlighted;
            }
            else if (color_Pressed != prePress)
            {
                targetGraphic.color = color_Pressed;
            }
            else if (state != preState)
            {
                switch (state)
                {
                    case ButtonState.Disselected:
                        targetGraphic.color = color_Disselected;
                        break;
                    case ButtonState.Highlighted:
                        targetGraphic.color = color_Highlighted;
                        break;
                    case ButtonState.Pressed:
                        targetGraphic.color = color_Pressed;
                        break;
                }
            }

            preDiss = color_Disselected;
            preHigh = color_Highlighted;
            prePress = color_Pressed;
            preState = state;
        }
    }

#endif

    #endregion

}
