using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Lighting
    {
        private const int c_MaxDirLightCount = 4;

        private static readonly Vector4[] s_DirLightColors = new Vector4[c_MaxDirLightCount];
        private static readonly Vector4[] s_DirectionalLightDirectionsAndMasks = new Vector4[c_MaxDirLightCount];
        
        private void SetupDirectionalLight(int index,int visibleLightIndex,ref VisibleLight visibleLight,Light light)
        {
            // Light light = RenderSettings.sun;
            // m_Buffer.SetGlobalVector(s_DirLightColorId,light.color.linear * light.intensity);
            // m_Buffer.SetGlobalVector(s_DirLightDirctionId,- light.transform.forward);
            s_DirLightColors[index] = visibleLight.finalColor;
            Vector4 dirAndMask = -visibleLight.localToWorldMatrix.GetColumn(2);
            dirAndMask.w = light.renderingLayerMask.ReinterpertAsFloat();
            s_DirectionalLightDirectionsAndMasks[index] = - visibleLight.localToWorldMatrix.GetColumn(2);
            s_DirLightShadowData[index] = m_Shadows.ReserveDirectionalShadows(visibleLight.light, visibleLightIndex);
        }
    }
}