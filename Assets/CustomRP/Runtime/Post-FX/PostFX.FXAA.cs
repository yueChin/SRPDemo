using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace CustomRP
{
    public partial class PostFXStack
    {
        private const string c_FXAAQualityLowKeyword = "FXAA_QUALITY_LOW",c_FXAAQualityMediumKeyword = "FXAA_QUALITY_MEDIUM";
        
        private void ConfigureFXAA()
        {
            if (m_AA.FXAA.Quality == Quality.Low)
            {
                m_Buffer.EnableShaderKeyword(c_FXAAQualityLowKeyword);
                m_Buffer.DisableShaderKeyword(c_FXAAQualityMediumKeyword);
            }
            else if (m_AA.FXAA.Quality == Quality.Medium)
            {
                m_Buffer.EnableShaderKeyword(c_FXAAQualityMediumKeyword);
                m_Buffer.DisableShaderKeyword(c_FXAAQualityLowKeyword);
            }
            else
            {
                m_Buffer.DisableShaderKeyword(c_FXAAQualityLowKeyword);
                m_Buffer.DisableShaderKeyword(c_FXAAQualityMediumKeyword);
            }
            m_Buffer.SetGlobalVector(ShaderIds.FXAAConfigId,new Vector4(m_AA.FXAA.FixedThreshold,m_AA.FXAA.RelativeThreshold,m_AA.FXAA.SubpixelBlending));
        }
    }
}