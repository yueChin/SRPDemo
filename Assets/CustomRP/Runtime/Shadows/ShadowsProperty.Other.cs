using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private const int c_MaxShadowOtherLightCount = 16;

        private int m_ShadowOtherLightCount;
        

        private static int s_OtherShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas");
        
        private static string[] s_OtherFilterKeywords =
        {
            "_OTHER_PCF3",
            "_OTHER_PCF5",
            "_OTHER_PCF7",
        };

        private static int s_OtherShadowMatricesId = Shader.PropertyToID("_OtherShadowMatrices");
        private static Matrix4x4[] s_OtherShadowMatrices = new Matrix4x4[c_MaxShadowOtherLightCount];
        
        private static int s_OtherShadowTilesId = Shader.PropertyToID("_OtherShadowTiles");
        private Vector4[] m_OtherShadowTiles = new Vector4[c_MaxShadowOtherLightCount];

        private struct ShadowedOtherLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;
            public float NormalBias;
            public bool IsPoint;
        }

        private ShadowedOtherLight[] m_ShadowedOtherLights = new ShadowedOtherLight[c_MaxShadowOtherLightCount];
        

    }
}