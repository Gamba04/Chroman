using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeysFeedback : MonoBehaviour
{
    [Serializable]
    private class Key
    {
        [SerializeField, HideInInspector] private string name;

        [SerializeField]
        public SpriteRenderer sr;
        [SerializeField]
        private bool mouse;
        [SerializeField]
        private int mouseButton;
        [SerializeField]
        private List<KeyCode> keys;

        public void UpdateColor(Color defaultColor, Color pressedColor)
        {
            if (sr != null)
            {
                if (mouse)
                {
                    if (Input.GetMouseButton(mouseButton))
                    {
                        sr.color = pressedColor;

                    }
                    else
                    {
                        sr.color = defaultColor;
                    }
                }
                else
                {
                    if (IsAnyKeyPressed())
                    {
                        sr.color = pressedColor;
                    }
                    else
                    {
                        sr.color = defaultColor;
                    }
                }
            }
        }

        private bool IsAnyKeyPressed()
        {
            bool isPressed = false;

            for (int i = 0; i < keys.Count; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    isPressed = true;
                    break;
                }
            }

            return isPressed;
        }

        public void SetName()
        {
            string srName = (sr != null) ? sr.gameObject.name : null;
            if (mouse)
            {
                name = $"Mouse {mouseButton} : {srName}";
            }
            else
            {
                name = $"{GetKeyNames()} : {srName}";
            }
        }

        private string GetKeyNames()
        {
            string text = "";

            for (int i = 0; i < keys.Count; i++)
            {
                text += keys[i].ToString();

                if (i < keys.Count - 1) text += ", ";
            }

            return text;
        }
    }

    [Header("Components")]
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private List<Key> keys = new List<Key>();
    [Header("Settings")]
    [SerializeField]
    private float visibleDistance = 20;
    [SerializeField]
    private Color defaultColor = Color.white;
    [SerializeField]
    private Color pressedColor = Color.white;

    private void Update()
    {
        if (!GameManager.GamePaused)
        {
            if (targetTransform != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i].sr != null && (keys[i].sr.transform.position - targetTransform.position).magnitude < visibleDistance)
                    {
                        keys[i].UpdateColor(defaultColor, pressedColor);
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            keys[i].SetName();
        }
    }

}
