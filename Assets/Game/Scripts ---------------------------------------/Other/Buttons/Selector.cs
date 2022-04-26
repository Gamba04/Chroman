using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Selector : MonoBehaviour
{
    [Serializable]
    private class Option
    {
        public string text = "";

        public UnityEvent action;
    }

    [Header("Components")]
    [SerializeField]
    private Text text;
    [SerializeField]
    private GameObject leftArrow;
    [SerializeField]
    private GameObject rightArrow;
    [Header("Settings")]
    [SerializeField]
    private List<Option> options = new List<Option>();
    [SerializeField]
    private int selection;
    [SerializeField]
    private int id;

    private static int[] savedValues = {-1,-1,-1,-1};


    private void Start()
    {
        if (savedValues[id] != -1)
        {
            selection = savedValues[id];
        }

        UpdateValue(-1);
        UpdateText();
    }

    private void Update()
    {

    }

    public void NextOption()
    {
        int prevCount = selection;
        selection = Mathf.Clamp(++selection, 0, options.Count - 1);

        UpdateValue(prevCount);
        UpdateText();
    }

    public void PreviousOption()
    {
        int prevCount = selection;
        selection = Mathf.Clamp(--selection, 0, options.Count - 1);

        UpdateValue(prevCount);
        UpdateText();
    }

    private void UpdateValue(int prevCount)
    {
        if (selection != prevCount)
        {
            if (selection < options.Count)
            {
                options[selection].action?.Invoke();
                savedValues[id] = selection;
            }
        }
    }

    private void UpdateText()
    {
        if (text != null)
        {
            if (selection < options.Count)
            {
                text.text = options[selection].text;
            }
        }

        if (selection == 0)
        {
            leftArrow?.SetActive(false);
        }
        else
        {
            leftArrow?.SetActive(true);
        }

        if (selection >= options.Count - 1)
        {
            rightArrow?.SetActive(false);
        }
        else
        {
            rightArrow?.SetActive(true);
        }
    }

    private void OnValidate()
    {
        if (text != null)
        {
            if (options.Count > 0)
            {
                text.text = options[selection].text;
            }
        }
    }
}
