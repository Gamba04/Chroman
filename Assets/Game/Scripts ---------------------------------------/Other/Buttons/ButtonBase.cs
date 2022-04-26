using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;

[ExecuteInEditMode]
public class ButtonBase : MonoBehaviour
{
    [Serializable]
    protected class AdvancedSettings
    {
        public DetectionType detectionType;
        public RectTransform rectDetection;
        public Collider2D colliderDetection;
    }

    public enum ButtonTag
    {
        Base,
        Back
    }

    protected enum ButtonState 
    {
        Disselected,
        Highlighted,
        Pressed
    }

    protected enum DetectionType
    {
        Rect,
        Collider
    }

    [SerializeField]
    private ButtonTag tag;

    #region ButtonLowerLayout

    protected virtual ButtonState NewState { get ; set ; }
    protected virtual UnityEvent NewOnClick { get; set; }
    protected virtual AdvancedSettings NewAdvancedSettings { get; set; }

    #endregion

    [HideInInspector]
    public bool pressed;

    public bool Interactable;

    public static Action<ButtonTag> onHover;
    public static Action<ButtonTag> onPress;

    protected virtual void Start()
    {
        ForceChangeState(ButtonState.Disselected);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (NewAdvancedSettings != null)
            {
                if (Interactable)
                {
                    CheckInteraction();
                }
                else
                {
                    ChangeState(ButtonState.Disselected);
                    pressed = false;
                }
            }

            RuntimeUpdate();
        }
        else
        {
            if (NewAdvancedSettings != null)
            {
                if (NewAdvancedSettings.rectDetection == null)
                {
                    NewAdvancedSettings.rectDetection = GetComponent<RectTransform>();
                }

                if (NewAdvancedSettings.colliderDetection == null)
                {
                    NewAdvancedSettings.colliderDetection = GetComponent<Collider2D>();
                }
            }

            EditorUpdate();
        }
    }

    #region StateControl

    private void CheckInteraction()
    {
        bool over = MouseOver();

        if (!pressed) // Not Pressed
        {
            if (over) // Over
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ChangeState(ButtonState.Pressed);
                    pressed = true;
                }
                else // Highlighted
                {
                    ChangeState(ButtonState.Highlighted);
                }
            }
            else // Outside
            {
                ChangeState(ButtonState.Disselected);
            }
        }
        else // Button Pressed
        {
            if (over)
            {
                if (Input.GetMouseButtonUp(0)) // OnClick
                {
                    OnClick();

                    pressed = false;
                }
            }
            else
            {
                pressed = false;
            }
        }
    }

    private void ChangeState(ButtonState state)
    {
        if (state != NewState)
        {
            NewState = state;

            switch (state)
            {
                case ButtonState.Disselected:
                    OnDisselect();
                    break;
                case ButtonState.Highlighted:
                    onHover?.Invoke(tag);
                    OnHighlight();
                    break;
                case ButtonState.Pressed:
                    onPress?.Invoke(tag);
                    OnPressed();
                    break;
            }
        }
    }

    private void ForceChangeState(ButtonState state)
    {
        NewState = state;

        switch (state)
        {
            case ButtonState.Disselected:
                OnDisselect();
                break;
            case ButtonState.Highlighted:
                OnHighlight();
                break;
            case ButtonState.Pressed:
                OnPressed();
                break;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Virtual Methods

    protected virtual void OnClick()
    {
        NewOnClick?.Invoke();
    }

    protected virtual void OnDisselect() { }

    protected virtual void OnHighlight() { }

    protected virtual void OnPressed() { }

    protected virtual void EditorUpdate() { }

    protected virtual void RuntimeUpdate() { }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetDetectionSize(float? width, float? height)
    {
        switch (NewAdvancedSettings.detectionType)
        {
            case DetectionType.Collider:
                if (NewAdvancedSettings.colliderDetection != null)
                {
                    BoxCollider2D box = NewAdvancedSettings.colliderDetection as BoxCollider2D;

                    if (box != null)
                    {
                        float newWidth = box.size.x;
                        float newHeight = box.size.y;

                        if (width.HasValue)
                        {
                            newWidth = width.Value;
                        }

                        if (height.HasValue)
                        {
                            newHeight = height.Value;
                        }

                        box.size = new Vector2(newWidth, newHeight);
                    }
                }
                break;
            case DetectionType.Rect:
                if (NewAdvancedSettings.rectDetection != null)
                {
                    float newWidth = NewAdvancedSettings.rectDetection.sizeDelta.x;
                    float newHeight = NewAdvancedSettings.rectDetection.sizeDelta.y;

                    if (width.HasValue)
                    {
                        newWidth = width.Value;
                    }

                    if (height.HasValue)
                    {
                        newHeight = height.Value;
                    }

                    NewAdvancedSettings.rectDetection.sizeDelta = new Vector2(newWidth, newHeight);
                }
                break;
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private bool MouseOver()
    {
        bool over = false;

        switch (NewAdvancedSettings.detectionType)
        {
            case DetectionType.Rect:
                if (NewAdvancedSettings.rectDetection != null)
                {
                    Vector2 pos = (Input.mousePosition - NewAdvancedSettings.rectDetection.position) / GameManager.Canvas.scaleFactor;
                    Vector2 scaledPos = new Vector2(pos.x / NewAdvancedSettings.rectDetection.localScale.x, pos.y / NewAdvancedSettings.rectDetection.localScale.y);

                    over = NewAdvancedSettings.rectDetection.rect.Contains(scaledPos);
                }
                break;
            case DetectionType.Collider:
                if (NewAdvancedSettings.colliderDetection != null)
                {
                    over = NewAdvancedSettings.colliderDetection.OverlapPoint(Input.mousePosition);
                }
                break;
        }

        return over;
    }

    #endregion

}
