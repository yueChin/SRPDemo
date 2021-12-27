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
        
        [Tooltip("值越小，输出越清晰但锯齿越多，而值越大，输出越稳定但越模糊")]
        [Range(0.1f, 1f)]
        public float JitterSpread;

        [Tooltip("控制颜色缓冲区的锐度。过高的值可能会引入暗边界瑕疵。")]
        [Range(0f, 3f)]
        public float Sharpness;

        [Tooltip("静止时的混合系数。混合到最终颜色中的历史样本的百分比。")]
        [Range(0f, 0.99f)]
        public float StationaryBlending;

        [Tooltip("具有显著运动的碎片的混合系数。控制混合到最终颜色中的历史样本的百分比。")]
        [Range(0f, 0.99f)]
        public float MotionBlending;
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