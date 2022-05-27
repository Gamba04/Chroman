using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private HealthCell cellPrefab;
    [SerializeField]
    private Transform cellsParent;

    [Header("Settings")]
    [SerializeField]
    private float separation;

    #region Init

    public void Init(int maxHealth)
    {
        CreateCells(maxHealth);
    }

    public void CreateCells(int maxHealth)
    {
        for (int i = 0; i < maxHealth; i++)
        {
            
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Mehtods

    public void SetHealth()
    {

    }

    public void InreaseMaxHealth()
    {
        
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    #endregion

}