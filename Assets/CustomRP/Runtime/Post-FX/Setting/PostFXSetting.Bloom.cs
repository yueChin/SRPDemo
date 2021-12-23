using System;
using UnityEngine;

namespace CustomRP
{
    [System.Serializable]
    public struct BloomSettings
    {
        [Range(0f,16f)]
        public int MaxIterations;

        [Min(1f)]
        public int DownScaleLimit;
        
        public bool BicubicUpsampling;

        [Min(0f)]
        public float Threshold;

        [Range(0f,1f)]
        public float ThresholdKnee;

        [Min(0f)]
        public float Intensity;

        public bool FadeFireflies;

        public Mode Mode;

        [Range(0.05f,0.95f)]
        public float Scatter;

        public bool IgnoreRenderScale;
    }
    
    public enum Mode
    {
        Additive,
        Scattering,
    }
}