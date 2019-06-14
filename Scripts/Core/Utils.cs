using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    class MovingAverage
    {

        public MovingAverage(short length)
        {
            FilterLength = length;
            init();
        }
        public MovingAverage()
        {
            FilterLength = 10;
            init();
        }

        public void Add(float x)
        {
            Index = (Index + 1) % FilterLength;
            Sum -= Array[Index];
            Array[Index] = x;
            Sum += x;
            if (!FilterComplete && Index == (FilterLength - 1))
            {
                FilterComplete = true;
            }
            if (FilterComplete)
            {
                Average = Sum / FilterLength;
            }
            else
            {
                Average = Sum / (Index + 1);
            }
            Sum = Mathf.Max(Sum, 0.0f);
            Average = Mathf.Max(Average, 0.0f);
        }

        short GetFilterLength()
        {
            return FilterLength;
        }

        public float GetCurrentAverage()
        {
            return Average;
        }

        public void Clear()
        {
            for (short i = 0; i < FilterLength; i++)
            {
                Array[i] = 0;
            }
            Sum = 0;
            Average = 0.0f;
            Index = -1;
        }

        void init()
        {
            FilterComplete = false;
            Index = -1;
            Sum = 0;
            Average = 0;
            Array = new float[FilterLength];
            Clear();
        }
        // Length of the filter
        short FilterLength;
        float[] Array;
        float Sum;
        float Average;
        int Index = 0;
        bool FilterComplete;
    };
    public class Utils
    {
        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
        {
            var v1 = camera.ScreenToViewportPoint(screenPosition1);
            var v2 = camera.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = camera.nearClipPlane;
            max.z = camera.farClipPlane;
            //min.z = 0.0f;
            //max.z = 1.0f;
            Bounds bounds = new Bounds();
            bounds.SetMinMax(min, max);

            return bounds;
        }
        public static bool FastApproximately(float a, float b, float threshold)
        {
            return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
        }
    }
}
