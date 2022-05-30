using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
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

    #endregion

}