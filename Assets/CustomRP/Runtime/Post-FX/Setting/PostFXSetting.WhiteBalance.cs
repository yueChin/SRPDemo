using System;
using UnityEngine;

namespace CustomRP
{
    [Serializable]
    public struct WhiteBalanceSettings
    {
        [Range(-100,100f)]
        public float Temperature;

        [Range(-100f,100f)]
        public float Tint;
    }
    
   
}