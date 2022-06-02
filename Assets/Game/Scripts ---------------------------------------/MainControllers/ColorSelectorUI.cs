using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectorUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private List<ColorArrow> colorArrows;
    [SerializeField]
    private Image bg;

    private readonly int colorsAmount = Enum.GetValues(typeof(Player.ColorState)).Length - 1;

    #region Public Methods

    public void UpdateColorState(Player.ColorState state, int unlockedColors)
    {
        for (int i = 0; i < colorsAmount; i++)
        {
            if (i < unlockedColors)
            {
                if ((int)state == i)
                {
                    colorArrows[i].SetState(ColorArrow.State.Selected);
                }
                else
                {
                    colorArrows[i].SetState(ColorArrow.State.Deselected);
                }
            }
            else
            {
                colorArrows[i].SetState(ColorArrow.State.Locked);
            }
        }
    }

    public void SetAlpha(float alpha)
    {
        foreach (ColorArrow arrow in colorArrows) arrow.SetAlpha(alpha);

        bg.color = GambaFunctions.GetColorWithAlpha(bg.color, alpha);
    }

    #endregion

}