using UnityEngine;
using UnityEngine.UI;

public class HealthCell : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Image cellImage;
    [SerializeField]
    private Image regenImage;

    [Header("Settings")]
    [SerializeField]
    private Sprite fullSprite;
    [SerializeField]
    private Sprite emptySprite;

    #region Public Methods

    public void SetState(bool value)
    {
        cellImage.sprite = value ? fullSprite : emptySprite;
    }

    public void SetRegen(float value)
    {
        regenImage.fillAmount = value;
    }

    public void SetAlpha(float alpha)
    {
        cellImage.color = GambaFunctions.GetColorWithAlpha(cellImage.color, alpha);
        regenImage.color = GambaFunctions.GetColorWithAlpha(regenImage.color, alpha);
    }

    #endregion

}