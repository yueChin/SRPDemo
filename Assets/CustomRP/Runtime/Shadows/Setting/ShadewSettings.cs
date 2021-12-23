using UnityEngine;

namespace CustomRP
{
    [System.Serializable]
    public partial class ShadowSettings
    {

        [Min(0.001f)]
        public float MaxDistance = 100f;

        [Range(0.001f,1f)]
        public float DistanceFade = 0f;

        public Directional Directional = new Directional()
        {
            AtlasSize = TextureSize._1024,
            Filter = FilterMode.PCF2x2,
            CascadeBlend = CascadeBlendMode.Hard,
            CascadeCount = 4,
            CascadeRatio1 = 0.1f,
            CascadeRatio2 = 0.25f,
            CascadeRatio3 = 0.5f,
            CascadeFade = 0.1f,
        };

        public Other Other = new Other()
        {
            AtlasSize = TextureSize._1024,
            Filter = FilterMode.PCF2x2,
        };
    }
}


