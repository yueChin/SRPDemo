using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace CustomRP
{
    public partial class PostFXStack
    {
        private int m_SampleIndex = 0;
        private const int c_SampleCount = 8;
        private Vector2 m_Jitter;

        private void PreDrawTAA()
        {
            m_Buffer.SetGlobalVector(ShaderIds.LastJitterId, m_Jitter);
            m_Camera.ResetProjectionMatrix();
            m_Camera.nonJitteredProjectionMatrix = m_Camera.projectionMatrix;
            m_Jitter = new Vector2(HaltonSeq.Get((m_SampleIndex & 1023) + 1, 2) - 0.5f, HaltonSeq.Get((m_SampleIndex & 1023) + 1, 3) - 0.5f);
            if (++m_SampleIndex >= c_SampleCount)
            {
                m_SampleIndex = 0;
            }
            m_Jitter *= m_AA.TAA.JitterSpread;
            Matrix4x4 cameraProj = m_Camera.orthographic
                ? RuntimeUtilities.GetJitteredOrthographicProjectionMatrix(m_Camera, m_Jitter)
                : RuntimeUtilities.GetJitteredPerspectiveProjectionMatrix(m_Camera, m_Jitter);
            m_Jitter = new Vector2(m_Jitter.x / m_BufferSize.x, m_Jitter.y / m_BufferSize.y);
            m_Camera.projectionMatrix = cameraProj;
            m_Camera.useJitteredProjectionMatrixForTransparentRendering = false;
            m_Buffer.SetGlobalVector(ShaderIds.JitterId,m_Jitter);
        }
        
        private void ConfigureTAA()
        {
            if (m_CameraProperty.HistoryTextures == null)
            {
                m_CameraProperty.HistoryTextures = new RenderTexture(m_BufferSize.x, m_BufferSize.y,0,RenderTextureFormat.ARGBHalf, 0)
                {
                    filterMode = FilterMode.Bilinear,
                    bindTextureMS = false,
                    antiAliasing = 1
                };
                //m_Buffer.CopyTexture(ShaderIds.HistoryTextureId, m_CameraProperty.HistoryTextures);
            }
            else if (m_CameraProperty.HistoryTextures.width != m_BufferSize.x || m_CameraProperty.HistoryTextures.height != m_BufferSize.y)
            {
                m_CameraProperty.HistoryTextures.Release();
                CoreUtils.Destroy(m_CameraProperty.HistoryTextures);
                m_CameraProperty.HistoryTextures = new RenderTexture(m_BufferSize.x, m_BufferSize.y, 0, RenderTextureFormat.ARGBHalf, 0)
                {
                    filterMode = FilterMode.Bilinear,
                    bindTextureMS = false,
                    antiAliasing = 1
                };
                //m_Buffer.CopyTexture(ShaderIds.HistoryTextureId, m_CameraProperty.HistoryTextures);
            }
            
            //TAA Start
            const float kMotionAmplification_Blending = 100f * 60f;
            const float kMotionAmplification_Bounding = 100f * 30f;
            m_Buffer.SetGlobalFloat(ShaderIds.SharpnessId, m_AA.TAA.Sharpness);
            
            m_Buffer.SetGlobalVector(ShaderIds.TemporalClipBoundingId, new Vector4(m_AA.TAA.StationaryAABBScale, m_AA.TAA.MotionAABBScale, kMotionAmplification_Bounding, 0f));
            m_Buffer.SetGlobalVector(ShaderIds.FinalBlendParametersId, new Vector4(m_AA.TAA.StationaryBlending, m_AA.TAA.MotionBlending, kMotionAmplification_Blending, 0f));
            m_Buffer.SetGlobalTexture(ShaderIds.HistoryTextureId, m_CameraProperty.HistoryTextures);
            //m_Buffer.SetGlobalTexture(ShaderIds.LastFrameDepthTextureId, prevDepthData.SSR_PrevDepth_RT);
            m_Buffer.SetGlobalTexture(ShaderIds.LastFrameMotionVectorsId, m_CameraProperty.HistoryMotionVectorTextures);
            m_Buffer.SetGlobalMatrix(ShaderIds.InvLastVPId, m_CameraProperty.LastInverseVP);
        }

        private void AfterDrawTAA()
        {
            //m_Buffer.CopyTexture(BuiltinRenderTextureType.CameraTarget, m_CameraProperty.HistoryTextures);
            //m_Buffer.CopyTexture(m_CameraProperty.MotionVectorTextures, m_CameraProperty.HistoryMotionVectorTextures);
            //m_Buffer.CopyTexture(ShaderIds.DepthTextureId, 0, 0, prevDepthData.SSR_PrevDepth_RT, 0, 0);
        }

        private void PostDrawTAA()
        {
            m_Camera.ResetProjectionMatrix();
        }
    }
}