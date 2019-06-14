using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
#if UNITY_EDITOR
public class FluidPerformanceMenu : MonoBehaviour
{


    bool EnableStats = false;

    bool IsActive = false;
    public const float NStoMS = 1000000.0f;
    Rect WindowRect = new Rect(500, 100, 500, 100);
    string FluidStats = "";
    string RenderingStats = "";
    Sample[] Samples;
    class Sample
    {
        public Sampler sampler;
        public Utils.MovingAverage TotalAVg;
        public Utils.MovingAverage InstanceAVg;
        public int Samples = 0;
        public Sample(string name)
        {
            sampler = Sampler.Get(name);
            TotalAVg = new Utils.MovingAverage();
            InstanceAVg = new Utils.MovingAverage();
        }
        public void Update()
        {
            if (sampler.GetRecorder().sampleBlockCount == 0)
            {
                return;
            }
            float TotalTime = (sampler.GetRecorder().elapsedNanoseconds / NStoMS);
            TotalAVg.Add(TotalTime);
            if (sampler.GetRecorder().sampleBlockCount > 1)
            {

                InstanceAVg.Add(TotalTime / sampler.GetRecorder().sampleBlockCount);
            }
            else
            {
                InstanceAVg.Add(TotalTime);
            }
            Samples = sampler.GetRecorder().sampleBlockCount;
            sampler.GetRecorder().enabled = true;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Profiler.enabled = true;
        Profiler.SetAreaEnabled(ProfilerArea.CPU, true);
        const int SamplerCount = 6;
        Samples = new Sample[SamplerCount];
        Samples[0] = new Sample("Fluid Update");
        Samples[1] = new Sample("Fluid Render (" + Fluid.FluidType.Water.ToString() + ")");
        Samples[2] = new Sample("Fluid Render (" + Fluid.FluidType.Lava.ToString() + ")");
        Samples[3] = new Sample("FluidRenderJob");
        Samples[4] = new Sample("FluidCullJob");
        Samples[5] = new Sample("RenderPipelineManager.DoRenderLoop_Internal()");
    }

    string GetValue(Sample s, bool isJob = false)
    {
        if (s == null)
        {
            return "";
        }
        if (!s.sampler.isValid)
        {
            return " Invalid Timer\n ";
        }
        string TimerData = s.sampler.name + " ";
        TimerData += s.InstanceAVg.GetCurrentAverage().ToString("0.00");
        TimerData += " ms " + s.Samples + " Instances";
        if (!isJob)
        {
            TimerData += " Totaling " + s.TotalAVg.GetCurrentAverage().ToString("0.00") + "ms ";
        }
        TimerData += "\n";
        return TimerData;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            IsActive = !IsActive;
        }
        if (!IsActive)
        {
            return;
        }
        foreach (Sample s in Samples)
        {
            s.Update();
        }
        EnableStats = IsActive;
        FluidStats = "Fluid Stats\n";
        FluidStats += GetValue(Samples[0]);
        FluidStats += GetValue(Samples[1]);
        FluidStats += GetValue(Samples[2]);
        FluidStats += "      Job: " + GetValue(Samples[3], true);
        FluidStats += "      Job: " + GetValue(Samples[4], true);
        RenderingStats = "Rendering Stats\n";
        RenderingStats += GetValue(Samples[5]);
    }

    private void OnGUI()
    {
        if (!IsActive)
        {
            return;
        }
        GUILayout.Window(444, WindowRect, DoMyWindow, "Performance Menu");
    }
    void DoMyWindow(int windowID)
    {
        GUILayout.Label(FluidStats);
        GUILayout.Label(RenderingStats);
        GUI.DragWindow();
    }
}
#endif