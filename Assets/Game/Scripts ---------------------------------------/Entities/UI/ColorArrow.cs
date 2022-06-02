using UnityEngine;
using UnityEngine.UI;

public class ColorArrow : MonoBehaviour
{
    public enum State
    {
        Locked,
        Deselected,
        Selected
    }

    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Image locked;

    [Header("Settings")]
    [SerializeField]
    [Range(0, 1)]
    private float deselectedAlpha = 0.3f;
    [SerializeField]
    private Transition alphaTransition;

    private float globalAlpha;

    #region Update

    private void Update()
    {
        TransitionsUpdate();
    }

    private void TransitionsUpdate()
    {
        alphaTransition.UpdateTransitionValue();

        if (alphaTransition.IsOnTransition)
        {
            image.color = GambaFunctions.GetColorWithAlpha(image.color, globalAlpha * alphaTransition.value);
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void SetState(State state)
    {
        anim.SetInteger("State", (int)state);

        switch (state)
        {
            case State.Deselected:
                alphaTransition.StartTransition(deselectedAlpha);
                break;

            case State.Selected:
                alphaTransition.StartTransition(1);
                break;
        }
    }

    public void SetAlpha(float alpha)
    {
        globalAlpha = alpha;

        locked.enabled = alpha > 0.1f;

        image.color = GambaFunctions.GetColorWithAlpha(image.color, globalAlpha * alphaTransition.value);
        locked.color = GambaFunctions.GetColorWithAlpha(locked.color, alpha);
    }

    #endregion

}