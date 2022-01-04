using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace CustomRP
{
    public partial class PostFXStack
    {
        private int m_SampleIndex = 0;
        private const int c_SampleCount = 8;
        private const float c_MotionAmplification_Blending = 100f * 60f;
        private const float c_MotionAmplification_Bounding = 100f * 30f;
        private void PreDrawTAA()
        {
            m_CameraProperty.HistoryTexture.SetupHistoryTexture(m_BufferSize,m_UseHDR);
            m_CameraProperty.PreviousDepthData.SetupPreviousDepthData(m_BufferSize);
            
            m_Buffer.SetGlobalVector(ShaderIds.LastJitterId, m_CameraProperty.HistoryTexture.Jitter);
            m_Camera.ResetProjectionMatrix();
            m_Camera.nonJitteredProjectionMatrix = m_Camera.projectionMatrix;
            m_CameraProperty.HistoryTexture.Jitter = new Vector2(HaltonSeq.Get((m_SampleIndex & 1023) + 1, 2) - 0.5f, HaltonSeq.Get((m_SampleIndex & 1023) + 1, 3) - 0.5f);
            if (++m_SampleIndex >= c_SampleCount)
            {
                m_SampleIndex = 0;
            }
            m_CameraProperty.HistoryTexture.Jitter *= m_AA.TAA.JitterSpread;
            Matrix4x4 cameraProj = m_Camera.orthographic
                ? RuntimeUtilities.GetJitteredOrthographicProjectionMatrix(m_Camera, m_CameraProperty.HistoryTexture.Jitter)
                : RuntimeUtilities.GetJitteredPerspectiveProjectionMatrix(m_Camera, m_CameraProperty.HistoryTexture.Jitter);
            m_CameraProperty.HistoryTexture.Jitter = new Vector2(m_CameraProperty.HistoryTexture.Jitter.x / m_BufferSize.x, m_CameraProperty.HistoryTexture.Jitter.y / m_BufferSize.y);
            m_Camera.projectionMatrix = cameraProj;
            m_Camera.useJitteredProjectionMatrixForTransparentRendering = false;
            m_Buffer.SetGlobalVector(ShaderIds.JitterId,m_CameraProperty.HistoryTexture.Jitter);
        }
        
        private void ConfigureTAA()
        {
            m_CameraProperty.HistoryTexture.SetupProperty(m_BufferSize,m_UseHDR);
            m_CameraProperty.PreviousDepthData.SetupData(m_BufferSize);
            m_CameraProperty.HistoryTexture.SetHistory(m_BufferSize,m_Buffer,ref m_CameraProperty.HistoryTexture.HistoryRT,ShaderIds.ColorAttachmentId);
            
            //TAA Start
            m_Buffer.SetGlobalFloat(ShaderIds.SharpnessId, m_AA.TAA.Sharpness);
            
            m_Buffer.SetGlobalVector(ShaderIds.TemporalClipBoundingId, new Vector4(m_AA.TAA.StationaryAABBScale, m_AA.TAA.MotionAABBScale, c_MotionAmplification_Blending, 0f));
            m_Buffer.SetGlobalVector(ShaderIds.FinalBlendParametersId, new Vector4(m_AA.TAA.StationaryBlending, m_AA.TAA.MotionBlending, c_MotionAmplification_Bounding, 0f));
            m_Buffer.SetGlobalTexture(ShaderIds.HistoryTextureId, m_CameraProperty.HistoryTexture.HistoryRT);
            m_Buffer.SetGlobalTexture(ShaderIds.LastFrameDepthTextureId, m_CameraProperty.PreviousDepthData.SSRPrevDepthRT);
            m_Buffer.SetGlobalTexture(ShaderIds.LastFrameMotionVectorsId, m_CameraProperty.HistoryTexture.HistoryMotionVectorRT);
            m_Buffer.SetGlobalMatrix(ShaderIds.InvLastVPId, m_CameraProperty.LastInverseVP);
        }

        private void AfterDrawTAA()
        {
            RenderTargetIdentifier historyTex = m_CameraProperty.HistoryTexture.HistoryRT;
            RenderTargetIdentifier hisMotionVectorTex = m_CameraProperty.HistoryTexture.HistoryMotionVectorRT;

            m_Buffer.CopyTexture(ShaderIds.FinalResultId, historyTex);
            m_Buffer.CopyTexture(ShaderIds.MotionVectorsTextureId, hisMotionVectorTex);
            m_Buffer.CopyTexture(ShaderIds.DepthTextureId, 0, 0, m_CameraProperty.PreviousDepthData.SSRPrevDepthRT, 0, 0);
        }

        private void PostDrawTAA()
        {
            m_Camera.ResetProjectionMatrix();
        }
    }
}