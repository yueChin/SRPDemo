using System;
using UnityEngine;

namespace CustomRP
{
    [Serializable]
    public struct ShadowsMidtonesHighlightsSettings
    {
        [ColorUsage(false,true)]
        public Color Shadows, Midtones, Highlights;

        [Range(0f,2f)]
        public float ShadowStart, ShadowsEnd, HighlightsStart, HighlightEnd;
    }
    
   
}