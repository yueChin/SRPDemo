using CustomRP;
using CustomRP.Runtime;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

/// <summary>
/// 相机渲染管理类：单独控制每个相机的渲染
/// </summary>
public partial class CameraRenderer
{

    ScriptableRenderContext m_Content;

    private Camera m_Camera;

    private const string c_BufferName = "Render Camera";

    private readonly CommandBuffer m_Buffer = new CommandBuffer
    {
        name = c_BufferName
    };
    //存储相机剔除后的结果
    private CullingResults m_CullingResults;
    private static readonly ShaderTagId s_UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static readonly ShaderTagId s_LitShaderTagId = new ShaderTagId("CustomLit");
    
    //private static int s_FrameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

    private static readonly bool s_CopyTextureSupported = SystemInfo.copyTextureSupport > CopyTextureSupport.None;
    private bool m_UseDepthTexture, m_UseMotionVectorTexture, m_UseColorTexture;
    private bool m_UseIntermediateBuffer,m_UseScaleRendering;

    private bool m_UseHDR = false;
    
    private readonly Lighting m_Lighting = new Lighting();
    private readonly PostFXStack m_PostFXStack = new PostFXStack();

    private static readonly CameraSettings s_DefaultCameraSettings = new CameraSettings();

    private readonly Material m_Material;

    private readonly Texture2D m_MissingTexture;

    private static readonly int s_BufferSizeId = Shader.PropertyToID("_CameraBufferSize");
    private Vector2Int m_BufferSize;

    private readonly CameraProperty m_CameraProperty = new CameraProperty();
    public enum CameraPass
    {
        Color = 0,
        Depth,
        MotionVector,
    }
    
    public CameraRenderer(Shader shader)
    {
        m_Material = CoreUtils.CreateEngineMaterial(shader);
        m_MissingTexture = new Texture2D(1, 1)
        {
            hideFlags = HideFlags.HideAndDontSave,
            name = "Missing"
        };
        m_MissingTexture.SetPixel(0,0,Color.white * 0.5f);
        m_MissingTexture.Apply(true,true);
    }
    
    /// <summary>
    /// 相机渲染
    /// </summary>
    public void Render(ScriptableRenderContext content, Camera camera,bool useDynamicBatching,bool useGPUInstancing,bool useLightsPerObject,CameraBufferSettings cameraBufferSettings
        ,ShadowSettings shadowSettings,PostFXSettings postFXSettings,int colorLutResolution)
    {
        this.m_Content = content;
        this.m_Camera = camera;
        CustomRenderPipelinesCamera cmpCamera = camera.GetComponent<CustomRenderPipelinesCamera>();
        CameraSettings cameraSettings = cmpCamera ? cmpCamera.Settings : s_DefaultCameraSettings;
        if (m_Camera.cameraType == CameraType.Reflection)
        {
            m_UseColorTexture = cameraBufferSettings.CopyColorReflection;
            m_UseDepthTexture = cameraBufferSettings.CopyDepthReflection;
            m_UseMotionVectorTexture = cameraBufferSettings.CopyMotionVectorReflection;
        }
        else
        {
            m_UseColorTexture = cameraBufferSettings.CopyColor && cameraSettings.CopyColor;
            m_UseDepthTexture = cameraBufferSettings.CopyDepth && cameraSettings.CopyDepth;
            m_UseMotionVectorTexture = cameraBufferSettings.CopyMotionVector && cameraSettings.CopyMotionVector;
        }
        if (cameraSettings.OverridePostFX)
        {
            postFXSettings = cameraSettings.PostFXSettings;
        }

        float renderScale = cameraSettings.GetRenderScale(cameraBufferSettings.RenderScale);
        m_UseScaleRendering = renderScale < 0.99f || renderScale > 1.0f;
        //设置buffer缓冲区的名字
        PrepareBuffer();
        // 在Game视图绘制的几何体也绘制到Scene视图中
        PrepareForSceneWindow();

        if (!Cull(shadowSettings.MaxDistance))
        {
            return;
        }
        m_UseHDR = cameraBufferSettings.AllowHDR && camera.allowHDR;

        if (m_UseScaleRendering)
        {
            renderScale = Mathf.Clamp(renderScale, 0.1f, 2f);
            m_BufferSize.x = (int)(m_Camera.pixelWidth * renderScale);
            m_BufferSize.y = (int)(m_Camera.pixelHeight * renderScale);
        }
        else
        {
            m_BufferSize.x = m_Camera.pixelWidth;
            m_BufferSize.y = m_Camera.pixelHeight;
        }

        PreDraw();
        
        m_Buffer.BeginSample(SampleName);
        m_Buffer.SetGlobalVector(s_BufferSizeId,new Vector4(1f / m_BufferSize.x,1f / m_BufferSize.y,m_BufferSize.x,m_BufferSize.y));
        ExecuteBuffer();
        m_Lighting.Setup(content,m_CullingResults,shadowSettings,useLightsPerObject,cameraSettings.MaskLights ? cameraSettings.RenderingLayerMask : -1);
        
        //cameraBufferSettings.FXAA.Enable &= cameraSettings.AllowFXAA;
        m_PostFXStack.Setup(content,camera,m_CameraProperty,m_BufferSize,postFXSettings,cameraSettings.KeepAlpha,m_UseHDR,colorLutResolution
            ,cameraSettings.FinalBlendMode,cameraBufferSettings.BicubicRescalingMode,cameraBufferSettings.AA);
        
        m_Buffer.EndSample(SampleName);
        
        Setup();
        
        //绘制几何体
        DrawVisibleGeometry(useDynamicBatching,useGPUInstancing,useLightsPerObject,cameraSettings.RenderingLayerMask);
        //绘制SRP不支持的内置shader类型
        DrawUnsupportedShaders();

        DrawGizmosBeforeFX();
        if (m_PostFXStack.IsActive)
        {
            m_PostFXStack.PreRender();
            m_PostFXStack.Render(ShaderIds.ColorAttachmentId);
            m_PostFXStack.PostRender();
        }
        else if (m_UseIntermediateBuffer)
        {
            Draw(ShaderIds.ColorAttachmentId,BuiltinRenderTextureType.CameraTarget);
            ExecuteBuffer();
        }
        DrawGizmosAfterFX();

        Cleanup();
        //提交命令缓冲区
        Submit();
    }

    private void PreDraw()
    {
        m_PostFXStack.PreDrawTAA();
    }
    
    private void Draw(RenderTargetIdentifier form,RenderTargetIdentifier to,CameraPass pass = CameraPass.Color)
    {
        m_Buffer.SetGlobalTexture(ShaderIds.SourceTextureId,form);
        m_Buffer.SetRenderTarget(to,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        m_Buffer.DrawProcedural(Matrix4x4.identity, m_Material,(int)pass,MeshTopology.Triangles,3);
    }
    
    /// <summary>
    /// 绘制几何体
    /// </summary>
    private void DrawVisibleGeometry(bool useDynamicBatching,bool useGPUInstancing,bool useLightsPerObject,int renderingLayerMask)
    {
        PerObjectData lightPerObjectDataFlags =
            useLightsPerObject ? PerObjectData.LightData | PerObjectData.LightIndices : PerObjectData.None;
        
        //设置绘制顺序和指定渲染相机
        SortingSettings sortingSettings = new SortingSettings(m_Camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        //设置渲染的shader pass和渲染排序
        DrawingSettings drawingSettings = new DrawingSettings(s_UnlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
            perObjectData = PerObjectData.Lightmaps  | PerObjectData.ShadowMask 
                        | PerObjectData.LightProbe | PerObjectData.OcclusionProbe 
                        | PerObjectData.LightProbeProxyVolume | PerObjectData.OcclusionProbeProxyVolume 
                        | PerObjectData.ReflectionProbes | lightPerObjectDataFlags
        };
        drawingSettings.SetShaderPassName(1,s_LitShaderTagId);
        ////只绘制RenderQueue为opaque不透明的物体
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque,renderingLayerMask:(uint)renderingLayerMask);
        //1.绘制不透明物体
        m_Content.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
        
        //2.绘制天空盒
        m_Content.DrawSkybox(m_Camera);
        if (m_UseDepthTexture || m_UseColorTexture || m_UseMotionVectorTexture)
        {
            CopyAttachments();
        }
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        //只绘制RenderQueue为transparent透明的物体
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        //3.绘制透明物体
        m_Content.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
        
    }
    
    /// <summary>
    /// 提交命令缓冲区
    /// </summary>
    private void Submit()
    {
        m_Buffer.EndSample(SampleName);
        ExecuteBuffer();
        m_Content.Submit();
    }
    
    /// <summary>
    /// 设置相机的属性和矩阵
    /// </summary>
    private void Setup()
    {
        m_Content.SetupCameraProperties(m_Camera);
        //得到相机的clear flags
        CameraClearFlags flags = m_Camera.clearFlags;

        this.m_UseIntermediateBuffer = m_UseScaleRendering || m_UseColorTexture || m_UseDepthTexture || m_UseMotionVectorTexture || m_PostFXStack.IsActive;
        if (m_UseIntermediateBuffer)
        {
            if (flags > CameraClearFlags.Color)
            {
                flags = CameraClearFlags.Color;
            }
            

            m_Buffer.GetTemporaryRT(ShaderIds.ColorAttachmentId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            m_Buffer.GetTemporaryRT(ShaderIds.DepthAttachmentId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Point, RenderTextureFormat.Depth);
            m_Buffer.SetRenderTarget(ShaderIds.ColorAttachmentId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store
                ,ShaderIds.DepthAttachmentId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
            m_Buffer.GetTemporaryRT(ShaderIds.MotionVectorsTextureId,m_BufferSize.x,m_BufferSize.y,1,FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
        }
        //设置相机清除状态
        m_Buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? m_Camera.backgroundColor.linear : Color.clear);
        m_Buffer.BeginSample(SampleName);
        m_Buffer.SetGlobalTexture(ShaderIds.ColorTextureId, m_MissingTexture);
        m_Buffer.SetGlobalTexture(ShaderIds.DepthTextureId,m_MissingTexture);
        m_Buffer.SetGlobalTexture(ShaderIds.MotionVectorsTextureId,m_MissingTexture);
        ExecuteBuffer();
    }

    private void Cleanup()
    {
        m_Lighting.Cleanup();
        if (m_UseIntermediateBuffer)
        {
            m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorAttachmentId);
            m_Buffer.ReleaseTemporaryRT(ShaderIds.DepthAttachmentId);
            if (m_UseDepthTexture)
            {
                m_Buffer.ReleaseTemporaryRT(ShaderIds.DepthTextureId);
            }

            if (m_UseColorTexture)
            {
                m_Buffer.ReleaseTemporaryRT(ShaderIds.ColorTextureId);
            }

            if (m_UseMotionVectorTexture)
            {
                m_Buffer.ReleaseTemporaryRT(ShaderIds.MotionVectorsTextureId);
            }
        }
    }
    
    private void CopyAttachments()
    {
        if (m_UseColorTexture)
        {
            m_Buffer.GetTemporaryRT(ShaderIds.ColorTextureId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Bilinear
                ,m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            if (s_CopyTextureSupported)
            {
                m_Buffer.CopyTexture(ShaderIds.ColorAttachmentId,ShaderIds.ColorTextureId);
            }
            else
            {
                Draw(ShaderIds.ColorAttachmentId,ShaderIds.ColorTextureId);
            }
            ExecuteBuffer();
        }
        
        if (m_UseDepthTexture)
        {
            m_Buffer.GetTemporaryRT(ShaderIds.DepthTextureId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Point,RenderTextureFormat.Depth);
            if (s_CopyTextureSupported)
            {
                m_Buffer.CopyTexture(ShaderIds.DepthAttachmentId,ShaderIds.DepthTextureId);
            }
            else
            {
                Draw(ShaderIds.DepthAttachmentId,ShaderIds.DepthTextureId,CameraPass.Depth);
            }
        }

        if (!s_CopyTextureSupported)
        {
            m_Buffer.SetRenderTarget(ShaderIds.ColorAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store
                ,ShaderIds.DepthAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store);
        }
        ExecuteBuffer();
        
        
        if (m_UseMotionVectorTexture)
        {
            m_Buffer.SetRenderTarget(ShaderIds.MotionVectorsTextureId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
            m_Buffer.DrawProcedural(Matrix4x4.identity, m_Material,(int)CameraPass.MotionVector,MeshTopology.Triangles,3);
        }
        ExecuteBuffer();
    }

    /// <summary>
    /// 执行缓冲区命令
    /// </summary>
    private void ExecuteBuffer()
    {
        m_Content.ExecuteCommandBuffer(m_Buffer);
        m_Buffer.Clear();
    }
    
    /// <summary>
    /// 剔除
    /// </summary>
    /// <returns></returns>
    private bool Cull(float maxShadowDistance)
    {
        ScriptableCullingParameters p;

        if (m_Camera.TryGetCullingParameters(out p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, m_Camera.farClipPlane);
            m_CullingResults = m_Content.Cull(ref p);
            m_CameraProperty.PreRender(m_Camera,float3.zero);
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        CoreUtils.Destroy(m_Material);
        CoreUtils.Destroy(m_MissingTexture);
    }
    
    partial void DrawUnsupportedShaders();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
    partial void DrawGizmosBeforeFX();
    partial void DrawGizmosAfterFX();

   
}
