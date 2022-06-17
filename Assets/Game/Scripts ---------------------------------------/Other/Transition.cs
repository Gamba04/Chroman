using System;
using UnityEngine;

[Serializable]
public class Transition
{
    [SerializeField]
    private AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField]
    private float duration = 1;
    
    public float value;

    [SerializeField]
    private bool isOnTransition;

    private float transitionCooldown;
    private float targetValue;
    private float previousValue;

    private bool unscaled;
    private bool inverseCurve;

    private Action onTransitionEnd;

    public bool IsOnTransition { get => isOnTransition; }

    public void StartTransition(float targetValue, Action onTransitionEnd = null)
    {
        unscaled = false;
        this.onTransitionEnd = onTransitionEnd;

        if (duration > 0 && targetValue != value)
        {
            isOnTransition = true;
            transitionCooldown = duration;
            previousValue = value;
            this.targetValue = targetValue;
        }
        else
        {
            value = targetValue;
            isOnTransition = false;
        }
    }

    public void StartTransitionUnscaled(float targetValue, Action onTransitionEnd = null)
    {
        unscaled = true;
        this.onTransitionEnd = onTransitionEnd;

        if (duration > 0 && targetValue != value)
        {
            isOnTransition = true;
            transitionCooldown = duration;
            previousValue = value;
            this.targetValue = targetValue;
        }
        else
        {
            value = targetValue;
            isOnTransition = false;
        }
    }

    public void StartTransition(float targetValue, bool inverse, Action onTransitionEnd = null)
    {
        unscaled = false;
        inverseCurve = inverse;
        this.onTransitionEnd = onTransitionEnd;

        if (duration > 0 && targetValue != value)
        {
            isOnTransition = true;
            transitionCooldown = duration;
            previousValue = value;
            this.targetValue = targetValue;
        }
        else
        {
            value = targetValue;
            isOnTransition = false;
        }
    }

    public void StartTransitionUnscaled(float targetValue, bool inverse, Action onTransitionEnd = null)
    {
        unscaled = true;
        inverseCurve = inverse;
        this.onTransitionEnd = onTransitionEnd;

        if (duration > 0 && targetValue != value)
        {
            isOnTransition = true;
            transitionCooldown = duration;
            previousValue = value;
            this.targetValue = targetValue;
        }
        else
        {
            value = targetValue;
            isOnTransition = false;
        }
    }

    public void SetDuration(float value)
    {
        duration = value;
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void UpdateTransitionValue()
    {
        if (isOnTransition)
        {
            if (unscaled)
            {
                Timer.ReduceCooldownUnscaled(ref transitionCooldown, () =>
                {
                    isOnTransition = false;
                    onTransitionEnd?.Invoke();
                });
            }
            else
            {
                Timer.ReduceCooldown(ref transitionCooldown, () =>
                {
                    isOnTransition = false;
                    onTransitionEnd?.Invoke();
                });
            }

            float progress = 1 - transitionCooldown / duration;
            value = Mathf.Lerp(previousValue, targetValue, (inverseCurve) ? 1 - curve.Evaluate(1 - progress) : curve.Evaluate(progress));
        }
    }
}
