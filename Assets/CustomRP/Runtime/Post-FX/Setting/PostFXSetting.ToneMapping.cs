using System;
using UnityEngine;

namespace CustomRP
{
    [Serializable]
    public struct ToneMappingSettings
    {
        public ToneMappingMode Mode;
    }
    
    public enum ToneMappingMode
    {
        None = 0,
        ACES,
        Neutral,
        Reinhard,
    }
}