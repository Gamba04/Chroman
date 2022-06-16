using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthControllerUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private HealthCell cellPrefab;
    [SerializeField]
    private Transform cellsParent;
    [SerializeField]
    private RectTransform border;

    [Header("Settings")]
    [SerializeField]
    private float separation;
    [SerializeField]
    private float borderPadding;

    private List<HealthCell> healthCells = new List<HealthCell>();

    private int health;
    private int maxHealth;
    private float currentRegen;

    private float borderOffsetWidth;

    #region Init

    public void Init(int maxHealth)
    {
        borderOffsetWidth = border.sizeDelta.x;

        CreateCells(maxHealth);
    }

    public void CreateCells(int maxHealth)
    {
        for (int i = 0; i < maxHealth; i++)
        {
            IncreaseMaxHealth();
        }
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Public Mehtods

    public void SetHealth(int health)
    {
        if (health == maxHealth) currentRegen = 0;

        for (int i = 0; i < healthCells.Count; i++)
        {
            HealthCell cell = healthCells[i];

            if (i == health) // Current cell
            {
                cell.SetRegen(currentRegen);
            }
            else // Other cell
            {
                cell.SetRegen(0);
            }

            cell.SetState(i < health);
        }

        this.health = health;
    }

    public void IncreaseMaxHealth()
    {
        HealthCell cell = Instantiate(cellPrefab, cellsParent);
        cell.name = cellPrefab.name;

        cell.transform.localPosition = Vector3.right * maxHealth * separation;

        healthCells.Add(cell);

        maxHealth++;

        health++;
        SetHealth(health);

        UpdateBorder();
    }

    public void ReduceMaxHealth()
    {
        HealthCell cell = healthCells[healthCells.Count - 1];

        healthCells.Remove(cell);
        Destroy(cell);

        maxHealth--;

        UpdateBorder();
    }

    public void SetRegen(float value)
    {
        if (health == maxHealth) return;

        currentRegen = value;

        healthCells[health].SetRegen(value);
    }

    public void SetMaxHealth(float value)
    {
        value = Mathf.Floor(value);

        if (maxHealth != value)
        {
            int difference = (int)value - maxHealth;

            if (value > maxHealth)
            {
                for (int i = 0; i < difference; i++)
                {
                    IncreaseMaxHealth();
                }
            }
            else
            {
                for (int i = 0; i < -difference; i++)
                {
                    ReduceMaxHealth();
                }
            }

        }
    }

    public void SetAlpha(float alpha)
    {
        foreach (HealthCell cell in healthCells) cell.SetAlpha(alpha);
    }

    #endregion

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Other

    private void UpdateBorder()
    {
        float borderWidth = borderOffsetWidth + (separation + borderPadding) * (maxHealth - 1);

        border.sizeDelta = new Vector2(borderWidth, border.sizeDelta.y);
    }

    #endregion

}