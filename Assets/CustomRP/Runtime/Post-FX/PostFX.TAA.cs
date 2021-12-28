using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace CustomRP
{
    public partial class PostFXStack
    {
        private int m_SampleIndex = 0;
        private const int c_SampleCount = 8;
        private void ConfigureTAA()
        {
            // texComponent.UpdateProperty(cam);
            // SetHistory(cam.cam, m_Buffer, ref texComponent.historyTex, cam.targets.renderTargetIdentifier);
            // RenderTexture historyTex = texComponent.historyTex;
            // //TAA Start
            // const float kMotionAmplification_Blending = 100f * 60f;
            // const float kMotionAmplification_Bounding = 100f * 30f;
            // m_Buffer.SetGlobalFloat(ShaderIDs._Sharpness, sharpness);
            //
            // m_Buffer.SetGlobalVector(ShaderIDs._TemporalClipBounding, new Vector4(stationaryAABBScale, motionAABBScale, kMotionAmplification_Bounding, 0f));
            // m_Buffer.SetGlobalVector(ShaderIDs._FinalBlendParameters, new Vector4(stationaryBlending, motionBlending, kMotionAmplification_Blending, 0f));
            // m_Buffer.SetGlobalTexture(ShaderIDs._HistoryTex, historyTex);
            // m_Buffer.SetGlobalTexture(ShaderIDs._LastFrameDepthTexture, prevDepthData.SSR_PrevDepth_RT);
            // m_Buffer.SetGlobalTexture(ShaderIDs._LastFrameMotionVectors, texComponent.historyMV);
            // m_Buffer.SetGlobalMatrix(ShaderIDs._InvLastVp, proper.inverseLastViewProjection);
            // RenderTargetIdentifier source, dest;
            // PipelineFunctions.RunPostProcess(ref cam.targets, out source, out dest);
            // m_Buffer.BlitSRT(source, dest, ShaderIDs._DepthBufferTexture, taaMat, 0);
            // m_Buffer.CopyTexture(dest, historyTex);
            // m_Buffer.CopyTexture(ShaderIDs._CameraMotionVectorsTexture, texComponent.historyMV);
            // prevDepthData.UpdateCameraSize(new Vector2Int(cam.cam.pixelWidth, cam.cam.pixelHeight));
            // m_Buffer.CopyTexture(ShaderIDs._CameraDepthTexture, 0, 0, prevDepthData.SSR_PrevDepth_RT, 0, 0);
        }

        
        public void ConfigureJitteredProjectionMatrix(Camera camera, ref Vector2 jitter)
        {
            camera.nonJitteredProjectionMatrix = camera.projectionMatrix;
            camera.projectionMatrix = GetJitteredProjectionMatrix(camera, ref jitter);
            camera.useJitteredProjectionMatrixForTransparentRendering = false;
        }
        
        public Matrix4x4 GetJitteredProjectionMatrix(Camera camera, ref Vector2 jitter)
        {
            jitter = GenerateRandomOffset();
            jitter *= m_AA.TAA.JitterSpread;
            Matrix4x4 cameraProj = camera.orthographic
                ? RuntimeUtilities.GetJitteredOrthographicProjectionMatrix(camera, jitter)
                : RuntimeUtilities.GetJitteredPerspectiveProjectionMatrix(camera, jitter);
            jitter = new Vector2(jitter.x / camera.pixelWidth, jitter.y / camera.pixelHeight);
            return cameraProj;
        }

        Vector2 GenerateRandomOffset()
        {
            Vector2 offset = new Vector2(HaltonSeq.Get((m_SampleIndex & 1023) + 1, 2) - 0.5f
                , HaltonSeq.Get((m_SampleIndex & 1023) + 1, 3) - 0.5f);

            if (++m_SampleIndex >= c_SampleCount)
            {
                m_SampleIndex = 0;
            }

            return offset;
        }
    }
}