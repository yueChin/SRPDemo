using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    [Serializable]
    public class CameraSettings
    {
        [RenderingLayerMaskField]
        public int RenderingLayerMask = -1;

        public bool MaskLights = false;

        public bool CopyDepth = true;

        public bool CopyColor = true;
        
        public bool CopyMotionVector = true;
        
        public bool OverridePostFX = false;

        public PostFXSettings PostFXSettings = default;
        
        public FinalBlendMode FinalBlendMode = new FinalBlendMode()
        {
            Source = BlendMode.One,
            Dsesination = BlendMode.Zero,
        };

        public RenderScaleMode RenderScaleMode = RenderScaleMode.Inherit;

        [Range(0.1f,2f)]
        public float RenderScale = 1f;

        public float GetRenderScale(float scale)
        {
            return RenderScaleMode == RenderScaleMode.Inherit ? scale
                : RenderScaleMode == RenderScaleMode.Override ? RenderScale : scale * RenderScale;
        }
        
        public CameraAA CameraAA = default;
        
        public bool KeepAlpha = false;
    }

    [Serializable]
    public struct CameraAA
    {
        public bool AllowFXAA;

        public bool AllowTAA;

        public bool AllowSMAA;
    }
    
    [Serializable]
    public struct FinalBlendMode
    {
        public BlendMode Source, Dsesination;
    }

    public enum RenderScaleMode
    {
        Inherit,
        Multiply,
        Override,
    }
}