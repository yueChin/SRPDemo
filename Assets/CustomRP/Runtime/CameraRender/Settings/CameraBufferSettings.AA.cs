using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class CameraBufferSettings
    {
        public AA AA;
    }

    [Serializable]
    public struct AA
    {
        public FXAA FXAA;

        public TAA TAA;
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

    [Serializable]
    public struct TAA
    {
        public bool Enable;
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