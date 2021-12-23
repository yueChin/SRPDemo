using UnityEngine;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private static int s_DireShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        private static int s_DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
        
        private const int c_MaxShadowedDirectionalLightCount = 4;
        
        private struct ShadowedDirectionalLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;
            public float NearPlaneOffset;
        }

        private ShadowedDirectionalLight[] m_ShadowedDirectionalLights = new ShadowedDirectionalLight[c_MaxShadowedDirectionalLightCount];

        private int m_ShadowedDirectionalLightCount;
        
        private static string[] s_DirectionalFilterKeywords =
        {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };
     
        private static Matrix4x4[] s_DirShadowMatrices = new Matrix4x4[c_MaxShadowedDirectionalLightCount * c_MaxCascades];
    }
}