using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdvancedButton : ButtonBase
{
    [Serializable]
    private class TargetGraphic
    {
        [SerializeField, HideInInspector] private string name;

        [Header("Components")]
        public Graphic targetGraphic;
        [Header("Settings")]
        public GraphicOptions disselected;
        public GraphicOptions highlighted;
        public GraphicOptions pressed;

        [SerializeField]
        private float duration;
        [SerializeField]
        private AnimationCurve transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        private Color lastColor;
        private Color targetColor;

        private Vector2 lastPosition;
        private Vector2 targetPosition;

        private Vector2 lastScale;
        private Vector2 targetScale;

        private float progress;
        private bool unscaled;

        public void Initialize()
        {
            targetGraphic = null;
            transitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

            disselected.Initialize();
            highlighted.Initialize();
            pressed.Initialize();
        }

        #region States

        public void Reset()
        {
            if (targetGraphic != null)
            {
                targetGraphic.color = disselected.color;
                targetGraphic.transform.localPosition = disselected.localPosition;
                targetGraphic.transform.localScale = disselected.localScale;
            }
        }

        public void StateUpdate()
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

                float curveValue = transitionCurve.Evaluate(progress);

                targetGraphic.color = Color.Lerp(lastColor, targetColor, curveValue);
                targetGraphic.transform.localPosition = Vector2.Lerp(lastPosition, targetPosition, curveValue);
                targetGraphic.transform.localScale = Vector2.Lerp(lastScale, targetScale, curveValue);
            }
        }

        public void ChangeStates(ButtonState state)
        {
            if (targetGraphic != null)
            {
                GraphicOptions options = disselected;
                switch (state)
                {
                    case ButtonState.Disselected:
                        options = disselected;
                        break;
                    case ButtonState.Highlighted:
                        options = highlighted;
                        break;
                    case ButtonState.Pressed:
                        options = pressed;
                        break;
                }

                unscaled = false;

                lastColor = targetGraphic.color;
                targetColor = options.color;

                lastPosition = targetGraphic.transform.localPosition;
                targetPosition = options.localPosition;

                lastScale = targetGraphic.transform.localScale;
                targetScale = options.localScale;

                if (duration > 0)
                {
                    progress = 0;
                }
                else
                {
                    targetGraphic.color = targetColor;
                    targetGraphic.transform.localPosition = targetPosition;
                    targetGraphic.transform.localScale = targetScale;

                    progress = 1;
                }
            }
        }

        public void ChangeColorUnscaled(ButtonState state)
        {
            if (targetGraphic != null)
            {
                GraphicOptions options = disselected;
                switch (state)
                {
                    case ButtonState.Disselected:
                        options = disselected;
                        break;
                    case ButtonState.Highlighted:
                        options = highlighted;
                        break;
                    case ButtonState.Pressed:
                        options = pressed;
                        break;
                }

                unscaled = true;

                lastColor = targetGraphic.color;
                targetColor = options.color;

                lastPosition = targetGraphic.transform.localPosition;
                targetPosition = options.localPosition;

                lastScale = targetGraphic.transform.localScale;
                targetScale = options.localScale;

                if (duration > 0)
                {
                    progress = 0;
                }
                else
                {
                    targetGraphic.color = targetColor;
                    targetGraphic.transform.localPosition = targetPosition;
                    targetGraphic.transform.localScale = targetScale;

                    progress = 1;
                }
            }
        }

        #endregion

        #region Editor

        [SerializeField, HideInInspector]
        private Graphic preGraphic;

        private ButtonState preState;
        private GraphicOptions preDisselected;
        private GraphicOptions preHighlighted;
        private GraphicOptions prePressed;

        public void EditorColorUpdate(ButtonState state)
        {
            if (targetGraphic != null)
            {
                if (targetGraphic != preGraphic)
                {
                    disselected.CopyGraphic(targetGraphic);
                    highlighted.CopyGraphic(targetGraphic);
                    pressed.CopyGraphic(targetGraphic);
                }

                if (disselected != preDisselected)
                {
                    targetGraphic.color = disselected.color;
                    targetGraphic.transform.localPosition = disselected.localPosition;
                    targetGraphic.transform.localScale = disselected.localScale;
                }
                else if (highlighted != preHighlighted)
                {
                    targetGraphic.color = highlighted.color;
                    targetGraphic.transform.localPosition = highlighted.localPosition;
                    targetGraphic.transform.localScale = highlighted.localScale;
                }
                else if (pressed != prePressed)
                {
                    targetGraphic.color = pressed.color;
                    targetGraphic.transform.localPosition = pressed.localPosition;
                    targetGraphic.transform.localScale = pressed.localScale;
                }
                else if (state != preState)
                {
                    switch (state)
                    {
                        case ButtonState.Disselected:
                            targetGraphic.color = disselected.color;
                            targetGraphic.transform.localPosition = disselected.localPosition;
                            targetGraphic.transform.localScale = disselected.localScale;
                            break;
                        case ButtonState.Highlighted:
                            targetGraphic.color = highlighted.color;
                            targetGraphic.transform.localPosition = highlighted.localPosition;
                            targetGraphic.transform.localScale = highlighted.localScale;
                            break;
                        case ButtonState.Pressed:
                            targetGraphic.color = pressed.color;
                            targetGraphic.transform.localPosition = pressed.localPosition;
                            targetGraphic.transform.localScale = pressed.localScale;
                            break;
                    }
                }

                preDisselected = disselected;
                preHighlighted = highlighted;
                prePressed = pressed;
                preState = state;

                preGraphic = targetGraphic;
            }
        }

        public void SetName()
        {
            name = (targetGraphic != null) ? $"{targetGraphic.gameObject.name} ({targetGraphic.GetType().Name})" : "None";
        }

        #endregion

    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Serializable]
    private struct GraphicOptions
    {
        public Color color;
        public Vector2 localPosition;
        public Vector2 localScale;
        //public Sprite customSprite;

        public void Initialize()
        {
            color = new Color(1, 1, 1, 1);
            localScale = Vector2.one;
        }

        public void CopyGraphic(Graphic graphic)
        {
            color = graphic.color;
            localPosition = graphic.transform.localPosition;
            localScale = graphic.transform.localScale;
        }

        public static bool operator ==(GraphicOptions a, GraphicOptions b)
        {
            return (a.color == b.color && a.localPosition == b.localPosition && a.localScale == b.localScale);
        }

        public static bool operator !=(GraphicOptions a, GraphicOptions b)
        {
            return !(a == b);
        }

        public override bool Equals(object a)
        {
            return this == (GraphicOptions)a;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    [SerializeField]
    private List<TargetGraphic> targetGraphics = new List<TargetGraphic>();

    [Space()]

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
        foreach (TargetGraphic t in targetGraphics)
        {
            if (t.targetGraphic != null)
            {
                t.Reset();
            }
        }

        base.Start();
    }

    #region Overrides

    protected override void EditorUpdate()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.EditorColorUpdate(state);
            t.SetName();
        }
    }

    protected override void RuntimeUpdate()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            if (t.targetGraphic != null && t.targetGraphic.gameObject.activeInHierarchy && t.targetGraphic.enabled)
            {
                t.StateUpdate();
            }
        }
    }

    protected override void OnDisselect()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.ChangeColorUnscaled(ButtonState.Disselected);
        }
    }

    protected override void OnHighlight()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.ChangeColorUnscaled(ButtonState.Highlighted);
        }
    }

    protected override void OnPressed()
    {
        foreach (TargetGraphic t in targetGraphics)
        {
            t.ChangeColorUnscaled(ButtonState.Pressed);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Editor

    [SerializeField, HideInInspector]
    private int lastTargetGraphicsLenght;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (lastTargetGraphicsLenght < targetGraphics.Count)
            {
                for (int i = lastTargetGraphicsLenght; i < targetGraphics.Count; i++)
                {
                    targetGraphics[i].Initialize();
                }
            }

            lastTargetGraphicsLenght = targetGraphics.Count;
        }
    }

#endif

    #endregion

}
