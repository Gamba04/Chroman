using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUCounter : ProfileCounter
{
    FrameTiming[] frameTimings = new FrameTiming[1];

    protected override void UpdateText()
    {
        FrameTimingManager.CaptureFrameTimings();
        if (Time.frameCount > 1)
        {
            FrameTimingManager.GetLatestTimings(1, frameTimings);
        }

        double gpu = frameTimings[0].gpuFrameTime;

        if (gpu >= 0)
        {
            text.text = $"{gpu.ToString()} ms";
        }
        else
        {
            text.enabled = false;
        }
    }
}
