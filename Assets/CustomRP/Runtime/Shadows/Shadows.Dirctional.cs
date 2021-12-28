using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private const int c_MaxShadowedDirectionalLightCount = 4;
        
        private struct ShadowedDirectionalLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;
            public float NearPlaneOffset;
        }

        private readonly ShadowedDirectionalLight[] m_ShadowedDirectionalLights = new ShadowedDirectionalLight[c_MaxShadowedDirectionalLightCount];

        private int m_ShadowedDirectionalLightCount;
        
        private static readonly string[] s_DirectionalFilterKeywords =
        {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };
     
        private static readonly Matrix4x4[] s_DirShadowMatrices = new Matrix4x4[c_MaxShadowedDirectionalLightCount * c_MaxCascades];
        
        public Vector4 ReserveDirectionalShadows(Light light ,int visibleLightIndex )
        {
            if (m_ShadowedDirectionalLightCount < c_MaxShadowedDirectionalLightCount 
                && light.shadows != LightShadows.None 
                && light.shadowStrength > 0f)
            {
                float maskChannel = -1;
                
                LightBakingOutput lightBakingOutput = light.bakingOutput;
                if (lightBakingOutput.lightmapBakeType == LightmapBakeType.Mixed
                    && lightBakingOutput.mixedLightingMode == MixedLightingMode.Shadowmask)
                {
                    m_UseShadowMask = true;
                    maskChannel = lightBakingOutput.occlusionMaskChannel;
                }

                if (!m_CullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
                {
                    return new Vector4(-light.shadowStrength, 0,0,maskChannel);
                }
                
                m_ShadowedDirectionalLights[m_ShadowedDirectionalLightCount] = new ShadowedDirectionalLight()
                {
                    VisibleLightIndex = visibleLightIndex,
                    SlopeScaleBias = light.shadowBias,
                    NearPlaneOffset = light.shadowNearPlane,
                };
                return new Vector4(light.shadowStrength, m_Settings.Directional.CascadeCount * m_ShadowedDirectionalLightCount++,light.shadowNormalBias,maskChannel);
            }
            return new Vector4(0f,0f,0f,-1);
        }
        
        private void RenderDirectionalShadows()
        {
            
            int atlasSize = (int)m_Settings.Directional.AtlasSize;
            m_AtlasSizes.x = atlasSize;
            m_AtlasSizes.y = 1f / atlasSize;
            
            m_Buffer.GetTemporaryRT(ShaderIds.DireShadowAtlasId,atlasSize,atlasSize,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
            m_Buffer.SetRenderTarget(ShaderIds.DireShadowAtlasId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
            m_Buffer.ClearRenderTarget(true,false,Color.clear);
            m_Buffer.SetGlobalFloat(ShaderIds.ShadowPancakingId,1f);
            m_Buffer.BeginSample(c_BufferName);
            
            ExecuteBuffer();
            int tiles = m_ShadowedDirectionalLightCount * m_Settings.Directional.CascadeCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4 ;
            int tileSize = atlasSize / split;
            for (int i = 0; i < m_ShadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i,split,tileSize);
            }
            //m_Buffer.SetGlobalInt(s_CascadeCountId,m_Settings.Directional.CascadeCount);
            m_Buffer.SetGlobalVectorArray(ShaderIds.CascadeCullingSpheresId,s_CascadeCullingSpheres);
            m_Buffer.SetGlobalVectorArray(ShaderIds.CascadeDataId,s_CascadeData);
            m_Buffer.SetGlobalMatrixArray(ShaderIds.DirShadowMatricesId,s_DirShadowMatrices);
            //m_Buffer.SetGlobalFloat(s_ShadowDistanceId,m_Settings.MaxDistance);

            //float f = 1f - m_Settings.Directional.CascadeFade;
            //m_Buffer.SetGlobalVector(s_ShadowDistanceFadeId,new Vector4(1f / m_Settings.MaxDistance,1f / m_Settings.DistanceFade,1f / (1f - f * f)));
            SetKeywords(s_DirectionalFilterKeywords,(int)m_Settings.Directional.Filter - 1);
            SetKeywords(s_CascadeBlendKeywords,(int)m_Settings.Directional.CascadeBlend - 1);
            //m_Buffer.SetGlobalVector(s_ShadowAtlasSizeId,new Vector4(atlasSize,1f/atlasSize));
            
            m_Buffer.EndSample(c_BufferName);
            
            ExecuteBuffer();
        }

        private void RenderDirectionalShadows(int index,int split,int tileSize)
        {
            ShadowedDirectionalLight light = m_ShadowedDirectionalLights[index];
            ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(m_CullingResults, light.VisibleLightIndex);

            int cascadeCount = m_Settings.Directional.CascadeCount;
            int tileOffset = index * cascadeCount;
            Vector3 ratios = m_Settings.Directional.CascadeRatios;
            float cullingFactor = Mathf.Max(0f, 0.8f - m_Settings.Directional.CascadeFade);
            float tileScale = 1f / split;

            for (int i = 0; i < cascadeCount; i++)
            {
                m_CullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.VisibleLightIndex
                    , i, cascadeCount,ratios,tileSize,light.NearPlaneOffset
                    , out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

                if (index == 0)
                {
                    Vector4 cullingSphere = splitData.cullingSphere;
                    SetCascadeData(i,cullingSphere,tileSize);
                }

                splitData.shadowCascadeBlendCullingFactor = cullingFactor;
                shadowSettings.splitData = splitData;
                int tileIndex = tileOffset + i;
                Vector2 offset = SetTileViewport(tileIndex,split,tileSize);
                s_DirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, tileScale);
                m_Buffer.SetViewProjectionMatrices(viewMatrix,projectionMatrix);
                m_Buffer.SetGlobalDepthBias(0f,light.SlopeScaleBias);
                ExecuteBuffer();
                m_Context.DrawShadows(ref shadowSettings);
                m_Buffer.SetGlobalDepthBias(0f,0f);
            }
        }
    }
}