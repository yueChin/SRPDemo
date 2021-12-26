using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [Serializable]
    public partial class CameraBufferSettings
    {
        public bool AllowHDR;

        public bool CopyDepth;

        public bool CopyDepthReflection;

        public bool CopyColor;

        public bool CopyColorReflection;

        [Range(0.1f,2f)]
        public float RenderScale;

        public BicubicRescalingMode BicubicRescalingMode;

        public bool CopyMotionVector;
        
        public bool CopyMotionVectorReflection;

    }
}