using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : ProfileCounter
{
    protected override void UpdateText()
    {
        int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);

        text.text = fps.ToString();
    }
}
