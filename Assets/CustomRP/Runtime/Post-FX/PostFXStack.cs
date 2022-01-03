using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP
{
    public partial class PostFXStack
    {
        private const string c_BufferName = "PostFX";

        private readonly CommandBuffer m_Buffer = new CommandBuffer()
        {
            name = c_BufferName,
        };

        private ScriptableRenderContext m_Content;

        private Camera m_Camera;
        private CameraRenderer.CameraProperty m_CameraProperty;
        
        private PostFXSettings m_PostFXSettings;

        public bool IsActive => m_PostFXSettings != null;

        private bool m_UseHDR, m_BicubicRescale,m_KeepAlpha;
        private int m_ColorLUTResolution;
        private Vector2Int m_BufferSize;
        private FinalBlendMode m_FinalBlendMode;
        private BicubicRescalingMode m_BicubicRescaling;

        private AA m_AA;
        public PostFXStack()
        {
            m_BloomPyramidId = Shader.PropertyToID("_BloomPyramid0");
            for (int i = 1; i < c_MaxBloomPyramidLevel * 2; i++)
            {
                Shader.PropertyToID("_BloomPyramid" + i);
            }
        }
        
        public void Setup(ScriptableRenderContext content,Camera camera,CameraRenderer.CameraProperty cameraProperties,Vector2Int bufferSize, PostFXSettings settings,bool keepAlpha,bool useHDR
            ,int colorLutResolution,FinalBlendMode finalBlendMode,BicubicRescalingMode bicubicRescalingMode,AA aa)
        {
            m_Content = content;
            m_Camera = camera;
            m_CameraProperty = cameraProperties;
            m_BufferSize = bufferSize;
            m_PostFXSettings = camera.cameraType <= CameraType.SceneView ? settings : null;
            m_KeepAlpha = keepAlpha;
            m_UseHDR = useHDR;
            m_ColorLUTResolution = colorLutResolution;
            m_FinalBlendMode = finalBlendMode;
            m_BicubicRescaling = bicubicRescalingMode;
            m_AA = aa;
            ApplySceneViewState();
        }

        public void Render(int sourceId)
        {
            if(DoBloom(sourceId))
            {
                DoFinal(ShaderIds.BloomResultId);
                m_Buffer.ReleaseTemporaryRT(ShaderIds.BloomResultId);
            }
            else
            {
                DoFinal(sourceId);
            }
            m_Content.ExecuteCommandBuffer(m_Buffer);
            m_Buffer.Clear();
        }
        
        private void Draw(RenderTargetIdentifier from,RenderTargetIdentifier to,Pass pass)
        {
            m_Buffer.SetGlobalTexture(ShaderIds.FXSourceId,from);
            m_Buffer.SetRenderTarget(to,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
            m_Buffer.DrawProcedural(Matrix4x4.identity, m_PostFXSettings.Material,(int)pass,MeshTopology.Triangles,3);
        }
        
        private void DrawFinal(RenderTargetIdentifier from,Pass pass)
        {
            m_Buffer.SetGlobalFloat(ShaderIds.FinalSrcBlendId,(float)m_FinalBlendMode.Source);
            m_Buffer.SetGlobalFloat(ShaderIds.FinalDstBlendId,(float)m_FinalBlendMode.Dsesination);
            m_Buffer.SetGlobalTexture(ShaderIds.FXSourceId,from);
            m_Buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,m_FinalBlendMode.Dsesination == BlendMode.Zero
                ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load,RenderBufferStoreAction.Store);
            m_Buffer.SetViewport(m_Camera.pixelRect);
            m_Buffer.DrawProcedural(Matrix4x4.identity, m_PostFXSettings.Material,(int)pass,MeshTopology.Triangles,3);
        }

        public void PreRender()
        {
            if (m_AA.TAA.Enable)
            {
                PreDrawTAA();
            }
        }

        public void PostRender()
        {
            if (m_AA.TAA.Enable)
            {
                PostDrawTAA();
            }
        }
        
        partial void ApplySceneViewState();
    }
}