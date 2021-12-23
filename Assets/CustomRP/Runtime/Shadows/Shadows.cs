using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private const string c_BufferName = "Shadows";

        private CommandBuffer m_Buffer = new CommandBuffer()
        {
            name = c_BufferName,
        };
        
        private ScriptableRenderContext m_Context;

        private CullingResults m_CullingResults;

        private ShadowSettings m_Settings;

        private Vector4 m_AtlasSizes;

        private static int s_ShadowDistanceId = Shader.PropertyToID("_ShadowDistance");
        private static int s_ShadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
        private static int s_ShadowPanckingId = Shader.PropertyToID("_ShadowPancaking");
        private static int s_ShadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");

        
        public void SetUp(ScriptableRenderContext content,CullingResults results,ShadowSettings settings)
        {
            this.m_Context = content;
            this.m_CullingResults = results;
            this.m_Settings = settings;
            this.m_ShadowedDirectionalLightCount = 0;
            this.m_ShadowOtherLightCount = 0;
        }

        private void SetKeywords(string[] keywords,int enableIndex)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i == enableIndex)
                {
                    m_Buffer.EnableShaderKeyword(keywords[i]);
                }
                else
                {
                    m_Buffer.DisableShaderKeyword(keywords[i]);
                }
            }
        }

        public void Render()
        {
            if (m_ShadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
            else
            {
                m_Buffer.GetTemporaryRT(s_DireShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            }

            if (m_ShadowOtherLightCount > 0)
            {
                RenderOtherShadows();
            }
            else
            {
                m_Buffer.SetGlobalTexture(s_OtherShadowAtlasId, s_DireShadowAtlasId);
            }
            
            m_Buffer.BeginSample(c_BufferName);
            SetKeywords(s_ShodowMaskKeywords,m_UseShadowMask ? QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ? 0 : 1 : -1);
            m_Buffer.SetGlobalInt(s_CascadeCountId,m_ShadowedDirectionalLightCount > 0 ? m_Settings.Directional.CascadeCount:0);
            float f = 1f - m_Settings.Directional.CascadeFade;
            m_Buffer.SetGlobalVector(s_ShadowDistanceFadeId,new Vector4(1f/m_Settings.MaxDistance,1f/m_Settings.DistanceFade,1f / (1f - f*f)));
            
            m_Buffer.SetGlobalVector(s_ShadowAtlasSizeId,m_AtlasSizes);
            m_Buffer.EndSample(c_BufferName);
            ExecuteBuffer();
            
        }

        public void Cleanup()
        {
            m_Buffer.ReleaseTemporaryRT(s_DireShadowAtlasId);
            if (m_ShadowOtherLightCount > 0)
            {
                m_Buffer.ReleaseTemporaryRT(s_OtherShadowAtlasId);
            }
            ExecuteBuffer();
        }

        // private void RenderDirectionalShadows()
        // {
        //     m_Buffer.SetGlobalVectorArray(s_CascadeCountId,s_CascadeData);
        //     m_Buffer.SetGlobalMatrixArray(s_DirShadowMatricesId,s_DirShadowMatrices);
        // }
        
        private Vector2 SetTileViewport(int index,int split, float tileSize)
        {
            Vector2 offset = new Vector2(index % split, index / split);
            m_Buffer.SetViewport(new Rect(offset.x * tileSize,offset.y * tileSize,tileSize,tileSize));
            return offset;
        }

        private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m,Vector2 offset,float scale)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }

            //float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30)*scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31)*scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32)*scale;;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33)*scale;;
            
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30)*scale;;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31)*scale;;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32)*scale;;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33)*scale;;
            
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            return m;
        }
        
        private void ExecuteBuffer()
        {
            m_Context.ExecuteCommandBuffer(m_Buffer);
            m_Buffer.Clear();
        }
    }
}