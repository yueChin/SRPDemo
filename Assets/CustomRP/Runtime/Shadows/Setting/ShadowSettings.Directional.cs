using UnityEngine;

namespace CustomRP
{
    [System.Serializable]
    public struct Directional
    {
        public ShadowSettings.TextureSize AtlasSize;
        
        public ShadowSettings.FilterMode Filter;
        
        public ShadowSettings.CascadeBlendMode CascadeBlend;

        [Range(1,4)]
        public int CascadeCount;

        [Range(0,1)]
        public float CascadeRatio1, CascadeRatio2, CascadeRatio3;
        
        [Range(0.001f,1f)]
        public float CascadeFade;

        public Vector3 CascadeRatios => new Vector3(CascadeRatio1, CascadeRatio2, CascadeRatio3);
        
    }
}