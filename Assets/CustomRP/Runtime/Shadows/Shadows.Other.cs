using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private const int c_MaxShadowOtherLightCount = 16;

        private int m_ShadowOtherLightCount;
        
        private static readonly string[] s_OtherFilterKeywords =
        {
            "_OTHER_PCF3",
            "_OTHER_PCF5",
            "_OTHER_PCF7",
        };
        private static readonly Matrix4x4[] s_OtherShadowMatrices = new Matrix4x4[c_MaxShadowOtherLightCount];
        private readonly Vector4[] m_OtherShadowTiles = new Vector4[c_MaxShadowOtherLightCount];

        private struct ShadowedOtherLight
        {
            public int VisibleLightIndex;
            public float SlopeScaleBias;
            public float NormalBias;
            public bool IsPoint;
        }

        private readonly ShadowedOtherLight[] m_ShadowedOtherLights = new ShadowedOtherLight[c_MaxShadowOtherLightCount];
        
        public Vector4 ReserveOtherShadows(Light light ,int visibleLightIndex )
        {
            if (light.shadows == LightShadows.None || light.shadowStrength <= 0f)
            {
                return new Vector4(0f, 0f, 0f, -1f);
            }

            float maskChannel = -1f;
            LightBakingOutput lightBakingOutput = light.bakingOutput;
            if (lightBakingOutput.lightmapBakeType == LightmapBakeType.Mixed
                && lightBakingOutput.mixedLightingMode == MixedLightingMode.Shadowmask)
            {
                m_UseShadowMask = true;
                maskChannel = lightBakingOutput.occlusionMaskChannel;
            }

            bool isPoint = light.type == LightType.Point;
            int newLightCount = m_ShadowOtherLightCount + (isPoint ? 6 : 1);
            
            if (newLightCount >= c_MaxShadowOtherLightCount ||
                !m_CullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
            }

            m_ShadowedOtherLights[m_ShadowOtherLightCount] = new ShadowedOtherLight()
            {
                VisibleLightIndex = visibleLightIndex,
                SlopeScaleBias = light.shadowBias,
                NormalBias = light.shadowNormalBias,
                IsPoint = isPoint,
            };

            Vector4 data = new Vector4(light.shadowStrength, m_ShadowOtherLightCount, isPoint ? 1f : 0f, maskChannel);
            m_ShadowOtherLightCount = newLightCount;
            return data;
            
        }
        
        private void RenderOtherShadows()
        {
            
            int atlasSize = (int)m_Settings.Other.AtlasSize;
            m_AtlasSizes.z = atlasSize;
            m_AtlasSizes.w = 1f / atlasSize;
            
            m_Buffer.GetTemporaryRT(ShaderIds.OtherShadowAtlasId,atlasSize,atlasSize,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
            m_Buffer.SetRenderTarget(ShaderIds.OtherShadowAtlasId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
            m_Buffer.ClearRenderTarget(true,false,Color.clear);
            m_Buffer.SetGlobalFloat(ShaderIds.ShadowPancakingId,0f);
            m_Buffer.BeginSample(c_BufferName);
            
            ExecuteBuffer();
            int tiles = m_ShadowOtherLightCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4 ;
            int tileSize = atlasSize / split;
            for (int i = 0; i < m_ShadowOtherLightCount;)
            {
                if (m_ShadowedOtherLights[i].IsPoint)
                {
                    RenderPointShadows(i,split,tileSize);
                    i += 6;
                }
                else
                {
                    RenderSpotShadows(i,split,tileSize);
                    i += 1;
                }
            }
        
            m_Buffer.SetGlobalMatrixArray(ShaderIds.OtherShadowMatricesId,s_OtherShadowMatrices);
            m_Buffer.SetGlobalVectorArray(ShaderIds.OtherShadowTilesId,m_OtherShadowTiles);
            SetKeywords(s_OtherFilterKeywords,(int)m_Settings.Other.Filter - 1);
            
            m_Buffer.EndSample(c_BufferName);
            
            ExecuteBuffer();
        }
        
        private void RenderSpotShadows(int index,int split,int tileSize)
        {
            ShadowedOtherLight light = m_ShadowedOtherLights[index];
            ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(m_CullingResults,light.VisibleLightIndex);
            m_CullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(light.VisibleLightIndex,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,out ShadowSplitData splitData);
            shadowDrawingSettings.splitData = splitData;

            float texelSize = 2f / (tileSize * projectionMatrix.m00);
            float filterSize = texelSize * ((float)m_Settings.Other.Filter + 1);
            float bias = light.NormalBias * filterSize * 1.4142136f;
            float tileScale = 1f / split;
            Vector2 offset = SetTileViewport(index, split, tileSize);
            SetOtherTileData(index,offset, tileScale,bias);

            s_OtherShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, tileScale);
            
            m_Buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            m_Buffer.SetGlobalDepthBias(0f, light.SlopeScaleBias);
            ExecuteBuffer();
            m_Context.DrawShadows(ref shadowDrawingSettings);
            m_Buffer.SetGlobalDepthBias(0f, 0f);
        }

        private void RenderPointShadows(int index,int split,int tileSize)
        {
            ShadowedOtherLight light = m_ShadowedOtherLights[index];
            ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(m_CullingResults,light.VisibleLightIndex);
         
            float texelSize = 2f / (tileSize);
            float filterSize = texelSize * ((float)m_Settings.Other.Filter + 1);
            float bias = light.NormalBias * filterSize * 1.4142136f;
            float tileScale = 1f / split;

            float fovBias = Mathf.Atan(1f + bias + filterSize) * Mathf.Rad2Deg * 2f - 90f;
            for (int i = 0; i < 6; i++)
            {
                m_CullingResults.ComputePointShadowMatricesAndCullingPrimitives(light.VisibleLightIndex, (CubemapFace)i,fovBias
                    ,out Matrix4x4 viewMatrix,out Matrix4x4 projectionMatrix,out ShadowSplitData splitData);
                viewMatrix.m11 = -viewMatrix.m11;
                viewMatrix.m12 = -viewMatrix.m12;
                viewMatrix.m13 = -viewMatrix.m13;
                shadowDrawingSettings.splitData = splitData;
                int tileIndex = index + i;
                Vector2 offset = SetTileViewport(tileIndex, split, tileSize);
                SetOtherTileData(tileIndex,offset, tileScale,bias);
                s_OtherShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, tileScale);
            
                m_Buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                m_Buffer.SetGlobalDepthBias(0f, light.SlopeScaleBias);
                ExecuteBuffer();
                m_Context.DrawShadows(ref shadowDrawingSettings);
                m_Buffer.SetGlobalDepthBias(0f, 0f);
            }
        }
        
        private void SetOtherTileData(int index,Vector2 offset,float scale,float bias)
        {
            float border = m_AtlasSizes.w * 0.5f;
            Vector4 data ;
            data.x = offset.x * scale + border;
            data.y = offset.y * scale + border;
            data.z = scale - border - border;
            data.w = bias;
            m_OtherShadowTiles[index] = data;
        }
    }
}