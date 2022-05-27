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
    private Image image;

    [Header("Settings")]
    [SerializeField]
    private Sprite fullSprite;
    [SerializeField]
    private Sprite emptySprite;

    #region Public Methods

    public void SetState(bool value)
    {
        image.sprite = value ? fullSprite : emptySprite;
    }

    #endregion

}