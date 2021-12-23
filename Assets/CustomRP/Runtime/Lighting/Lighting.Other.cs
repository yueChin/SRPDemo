using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class Lighting 
    {
        private void SetupPointLight(int index,int visibleLightIndex,ref VisibleLight visibleLight,Light light)
        {
            s_OtherLightColors[index] = visibleLight.finalColor;
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range,0.00001f);
            s_OtherLightPositions[index] = position;
            s_OtherLightSpotAngles[index] = new Vector4(0f, 1f);
            s_OtherLightShadowData[index] = m_Shadows.ReserveOtherShadows(light, visibleLightIndex);
            Vector4 dirAndMask = -visibleLight.localToWorldMatrix.GetColumn(2);
            dirAndMask.w = light.renderingLayerMask.ReinterpertAsFloat();
            s_OtherLightDirectionsAndMask[index] = dirAndMask;

        }

        private void SetupSpotLight(int index ,int visibleLightIndex,ref VisibleLight visibleLight,Light light)
        {
            s_OtherLightColors[index] = visibleLight.finalColor;
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range,0.00001f);
            s_OtherLightPositions[index] = position;
            Vector4 dirAndMask = Vector4.zero;
            dirAndMask.w = light.renderingLayerMask.ReinterpertAsFloat();
            s_OtherLightDirectionsAndMask[index] = dirAndMask;
            //s_OtherLightDirecitons[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
            s_OtherLightSpotAngles[index] = Vector4.zero;
            //Light light = visibleLight.light;
            float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
            float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            s_OtherLightSpotAngles[index] = new Vector4(angleRangeInv, -outerCos * angleRangeInv);
            s_OtherLightShadowData[index] = m_Shadows.ReserveOtherShadows(light, visibleLightIndex);
        }
    }
}