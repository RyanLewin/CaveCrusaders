using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class AG_Profiler
{
    float Accum = 0.0f;
    Stopwatch S = new Stopwatch();
    public void Begin()
    {
        S.Restart();
    }
    public void End()
    {
        S.Stop();
        Accum = S.ElapsedMilliseconds;
    }
    public void Reset()
    {
        Accum = 0.0f;
        S.Reset();
    }
    public float GetMS()
    {
        return Accum;
    }
}
