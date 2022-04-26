using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUCounter : ProfileCounter
{
    //FrameTiming[] frameTimings = new FrameTiming[1];

    TimeSpan lastCapture = new TimeSpan(0);
    TimeSpan currentCapture = new TimeSpan(0);

    PerformanceCounter counter;

    private void Start()
    {
        counter = new PerformanceCounter("Processor", "% Processor Time", "0,0");

        counter.NextValue();
    }

    protected override void UpdateText()
    {
        double cpu = GetCPUTime();

        if (cpu >= 0)
        {
            string time = cpu.ToString();

            string newTime = "";

            int decimalIndex = time.Length;
            for (int i = 0; i < time.Length; i++)
            {
                if (time[i] == '.')
                {
                    decimalIndex = i;
                }

                if (i - decimalIndex > 1)
                {
                    break;
                }

                newTime += time[i];
            }

            text.text = $"{newTime} ms";
        }
        else
        {
            text.enabled = false;
        }
    }

    private float GetCPUTime()
    {
        float time = 0;

        lastCapture = currentCapture;

        // Mathod #1 -----------------------------------------------------------------------

        //FrameTimingManager.CaptureFrameTimings();
        //if (Time.frameCount > 1)
        //{
        //    FrameTimingManager.GetLatestTimings(1, frameTimings);
        //}

        // Mathod #2 -----------------------------------------------------------------------

        Process[] processes = Process.GetProcesses();

        currentCapture = new TimeSpan(0);

        for (int i = 0; i < processes.Length; i++)
        {
            currentCapture += processes[i].TotalProcessorTime;
        }

        TimeSpan frameStats = currentCapture - lastCapture;

        time = ((float)frameStats.TotalMilliseconds * updateDelay) / Environment.ProcessorCount;

        // Mathod #3 -----------------------------------------------------------------------

        //time = counter.NextValue();

        return time;
    }
}
