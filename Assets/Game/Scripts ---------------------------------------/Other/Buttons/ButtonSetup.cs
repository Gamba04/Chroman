using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ButtonSetup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private List<ButtonBase> buttons;
    [SerializeField]
    private List<Graphic> graphics = new List<Graphic>();
    [Header("Settings")]
    [SerializeField]
    private float textSizeMult = 50;
    [SerializeField]
    private bool autoSetSize;

    public List<Graphic> Graphics { get => graphics; }

    private void Update()
    {
        if (Application.isPlaying)
        {

        }
        else
        {
            for (int i = 0; i < graphics.Count; i++)
            {
                Text text = graphics[i] as Text;

                if (text != null)
                {
                    gameObject.name = text.text;
                    break;
                }
            }
        }
    }

    public void SetHeight(float value) 
    {
        if (autoSetSize)
        {
            if (buttons != null)
            {
                bool hasText = false;
                for (int i = 0; i < graphics.Count; i++)
                {
                    Text text = graphics[i] as Text;

                    if (text != null)
                    {
                        hasText = true;
                        foreach (ButtonBase button in buttons)
                        {
                            button.SetDetectionSize(text.text.Length * textSizeMult, value);
                        }
                        break;
                    }
                }

                if (!hasText)
                {
                    foreach (ButtonBase button in buttons)
                    {
                        button.SetDetectionSize(null, value);
                    }
                }
            }
        }
    }

    public void SetInteractable(bool value)
    {
        if (buttons != null)
        {
            foreach (ButtonBase button in buttons)
            {
                button.Interactable = value;
            }
        }
    }
}
