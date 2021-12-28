using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Lighting
    {
        private const string m_BufferName = "Lighting";

        private readonly CommandBuffer m_Buffer = new CommandBuffer()
        {
            name = m_BufferName,
        };

        private CullingResults m_CullingResults;

        private static string s_LightsPerObjectKeyword = "_LIGHTS_PER_OBJECT";
        
        private readonly Shadows m_Shadows = new Shadows();
        private static readonly Vector4[] s_DirLightShadowData = new Vector4[c_MaxDirLightCount];
        
        public void Setup(ScriptableRenderContext content,CullingResults cullingResults,ShadowSettings shadowSettings,bool useLightsPerObject,int renderingLayerMask)
        {
            m_CullingResults = cullingResults;
            
            m_Buffer.BeginSample(m_BufferName);
            m_Shadows.SetUp(content,cullingResults,shadowSettings);
            SetupLights(useLightsPerObject,renderingLayerMask);
            m_Shadows.Render();
            //SetDirectionalLight();
            m_Buffer.EndSample(m_BufferName);
            
            content.ExecuteCommandBuffer(m_Buffer);
            m_Buffer.Clear();
        }

        public void Cleanup()
        {
            m_Shadows.Cleanup();
        }

        private void SetupLights(bool useLightsPerObject,int renderingLayerMask)
        {
            NativeArray<int> indexMap = useLightsPerObject ? m_CullingResults.GetLightIndexMap(Allocator.Temp) : default;

            NativeArray<VisibleLight> visibleLights = m_CullingResults.visibleLights;

            int dirLightCount = 0,otherLightCount = 0;

            int i;
            for (i = 0; i < visibleLights.Length; i++)
            {
                int j = -1;
                VisibleLight visibleLight = visibleLights[i];
                Light light = visibleLight.light;
                if ((light.renderingLayerMask & renderingLayerMask) != 0)
                {
                    if (visibleLight.lightType == LightType.Directional)
                    {
                        SetupDirectionalLight(dirLightCount++, i, ref visibleLight, light);
                        if (dirLightCount >= c_MaxDirLightCount)
                        {
                            break;
                        }
                    }
                    else if (visibleLight.lightType == LightType.Point)
                    {
                        j = otherLightCount;
                        SetupPointLight(otherLightCount++, i, ref visibleLight, light);
                        if (otherLightCount >= c_MaxOtherLightCount)
                        {
                            break;
                        }
                    }
                    else if (visibleLight.lightType == LightType.Spot)
                    {
                        j = otherLightCount;
                        SetupSpotLight(otherLightCount++, i, ref visibleLight, light);
                        if (otherLightCount >= c_MaxOtherLightCount)
                        {
                            break;
                        }
                    }
                }

                if (useLightsPerObject)
                {
                    indexMap[i] = j;
                }
            }

            if (useLightsPerObject)
            {
                for (; i < indexMap.Length; i++)
                {
                    indexMap[i] = -1;
                }
                
                m_CullingResults.SetLightIndexMap(indexMap);
                indexMap.Dispose();
                Shader.EnableKeyword(s_LightsPerObjectKeyword);
            }
            else
            {
                Shader.DisableKeyword(s_LightsPerObjectKeyword);
            }
            
            m_Buffer.SetGlobalInt(ShaderIds.DirLightCountId,dirLightCount);
            if (dirLightCount > 0)
            {
                m_Buffer.SetGlobalVectorArray(ShaderIds.DirLightColorsId,s_DirLightColors);
                m_Buffer.SetGlobalVectorArray(ShaderIds.DirLightDirectionsId,s_DirectionalLightDirectionsAndMasks);
                m_Buffer.SetGlobalVectorArray(ShaderIds.DireLightShadowDataId,s_DirLightShadowData);
            }

            m_Buffer.SetGlobalInt(ShaderIds.OtherLightCountId,otherLightCount);
            if (otherLightCount > 0)
            {
                m_Buffer.SetGlobalVectorArray(ShaderIds.OtherLightColorsId,s_OtherLightColors);
                m_Buffer.SetGlobalVectorArray(ShaderIds.OtherLightPositionsId,s_OtherLightPositions);
                m_Buffer.SetGlobalVectorArray(ShaderIds.OtherLightDirectionsAndMaskId,s_OtherLightDirectionsAndMask);
                m_Buffer.SetGlobalVectorArray(ShaderIds.OtherLightSpotAnglesId,s_OtherLightSpotAngles);
                m_Buffer.SetGlobalVectorArray(ShaderIds.OtherLightShadowDataId,s_OtherLightShadowData);
            }
        }

    }
}