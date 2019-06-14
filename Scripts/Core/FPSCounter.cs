using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    const float fpsMeasurePeriod = 0.25f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    const string display = "{0} FPS {1}ms\n{2}ms AVG";
    [SerializeField]
    Text m_Text;
    [SerializeField]
    float TargetFps = 60;
    [SerializeField]
    GameObject Panel;
    [SerializeField]
    bool active = true;
    float AVGTime = 0;
    int avgaccum = 0;
    static FPSCounter instance;
    [SerializeField]
    int FramerateLimit = 300;
    [SerializeField]
    bool LimitFpsHere = false;
    FrameTiming[] Times = new FrameTiming[1];
    [SerializeField]
    bool UseFrameTiming = false;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        active = true;
#else
        active = false;
#endif
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        Panel.SetActive(active);
        DontDestroyOnLoad(this);

    }


    private void Update()
    {
        if (UseFrameTiming)
        {
            FrameTimingManager.CaptureFrameTimings();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (active)
            {
                active = false;
            }
            else
            {
                active = true;
            }
            Panel.SetActive(active);
        }
        // measure average frames per second
        m_FpsAccumulator++;
        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            avgaccum++;
            AVGTime += (Time.deltaTime - AVGTime) / avgaccum;
            m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            if (LimitFpsHere)
            {
                Application.targetFrameRate = FramerateLimit;
            }
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
            uint ResultCount = 0;
            if (UseFrameTiming)
            {
                ResultCount = FrameTimingManager.GetLatestTimings(1, Times);
            }
            string Output = "FPS " + m_CurrentFps  +" "+ (AVGTime * 1000).ToString("00.00")+"ms";
            if (Times.Length > 0 && ResultCount > 0 )
            {
                Output += " CPU: " + Times[0].cpuFrameTime + "ms GPU: " + Times[0].gpuFrameTime + "ms";
            }
            m_Text.text = Output;
            if (avgaccum > 10)
            {
                avgaccum = 1;
            }
        }


        if (m_CurrentFps >= FramerateLimit)
        {
            m_Text.color = Color.blue;
        }
        else if (m_CurrentFps >= TargetFps)
        {
            m_Text.color = Color.green;
        }
        else if (m_CurrentFps < 60)
        {
            m_Text.color = Color.red;
        }
        else if (m_CurrentFps <= 30)
        {
            m_Text.color = Color.red;
        }
    }


}
