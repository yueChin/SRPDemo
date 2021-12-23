using System;
using UnityEngine;

namespace CustomRP
{
    [Serializable]
    public struct SplitToningSettings
    {
        [ColorUsage(false)]
        public Color Shadows, HightLights;

        [Range(-100f,100f)]
        public float Balance;
    }
    
   
}