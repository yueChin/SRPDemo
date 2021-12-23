using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [Serializable]
    public class CameraBufferSettings
    {
        public bool AllowHDR;

        public bool CopyDepth;

        public bool CopyDepthReflection;

        public bool CopyColor;

        public bool CopyColorReflection;

        [Range(0.1f,2f)]
        public float RenderScale;

        public BicubicRescalingMode BicubicRescalingMode;

        public FXAA FXAA;
    }

    [Serializable]
    public struct FXAA
    {
        public bool Enable;

        [Range(0.0312f,0.0833f)]
        public float FixedThreshold;

        [Range(0.063f,0.333f)]
        public float RelativeThreshold;
        
        [Range(0,1f)]
        public float SubpixelBlending;

        public Quality Quality;
    }
    
    public enum BicubicRescalingMode
    {
        Off,
        UpOnly,
        UpAndDown
    }

    public enum Quality
    {
        Low,
        Medium,
        High,
    }
}