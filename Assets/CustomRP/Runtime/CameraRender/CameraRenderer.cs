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
    private static ShaderTagId s_UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId s_LitShaderTagId = new ShaderTagId("CustomLit");
    
    //private static int s_FrameBufferId = Shader.PropertyToID("_CameraFrameBuffer");
    private static readonly int s_ColorAttachmentId = Shader.PropertyToID("_CameraColorAttachment");
    private static readonly int s_ColorTextureId = Shader.PropertyToID("_CameraTexture");
    private static readonly int s_DepthAttachmentId = Shader.PropertyToID("_CameraDepthAttachment");
    private static readonly int s_DepthTextureId = Shader.PropertyToID("_CameraDepthTexture");
    private static readonly int s_SourceTextureId = Shader.PropertyToID("_SourceTexture");
    private static readonly int s_MotionVectorsAttachmentId = Shader.PropertyToID("_CameraMotionVectorsAttachment");
    private static readonly int s_MotionVectorsTextureId = Shader.PropertyToID("_CameraMotionVectorsTexture");

    private static readonly bool s_CopyTextureSupported = SystemInfo.copyTextureSupport > CopyTextureSupport.None;
    private bool m_UseDepthTexture, m_UseMotionVectorTexture, m_UseColorTexture;
    private bool m_UseIntermediateBuffer,m_UseScaleRendering;

    private bool m_UseHDR = false;
    
    private readonly Lighting m_Lighting = new Lighting();
    private readonly PostFXStack m_PostFXStack = new PostFXStack();

    private static readonly CameraSettings s_DefaultCameraSettings = new CameraSettings();

    private readonly Material m_Material;

    private readonly Texture2D m_MissingTexture;

    private static int s_BufferSizeId = Shader.PropertyToID("_CameraBufferSize");
    private Vector2Int m_BufferSize;
    

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

        PreCull();
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
        
        m_Buffer.BeginSample(SampleName);
        m_Buffer.SetGlobalVector(s_BufferSizeId,new Vector4(1f / m_BufferSize.x,1f / m_BufferSize.y,m_BufferSize.x,m_BufferSize.y));
        ExecuteBuffer();
        m_Lighting.Setup(content,m_CullingResults,shadowSettings,useLightsPerObject,cameraSettings.MaskLights ? cameraSettings.RenderingLayerMask : -1);
        
        //cameraBufferSettings.FXAA.Enable &= cameraSettings.AllowFXAA;
        m_PostFXStack.Setup(content,camera,m_BufferSize,postFXSettings,cameraSettings.KeepAlpha,m_UseHDR,colorLutResolution
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
            m_PostFXStack.Render(s_ColorAttachmentId);
            m_PostFXStack.PostRender();
        }
        else if (m_UseIntermediateBuffer)
        {
            Draw(s_ColorAttachmentId,BuiltinRenderTextureType.CameraTarget);
            ExecuteBuffer();
        }
        DrawGizmosAfterFX();

        Cleanup();
        //提交命令缓冲区
        Submit();
    }
    
    private void Draw(RenderTargetIdentifier form,RenderTargetIdentifier to,bool isDepth = false)
    {
        m_Buffer.SetGlobalTexture(s_SourceTextureId,form);
        m_Buffer.SetRenderTarget(to,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        m_Buffer.DrawProcedural(Matrix4x4.identity, m_Material,isDepth ? 1 : 0,MeshTopology.Triangles,3);
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
            
            m_Buffer.GetTemporaryRT(s_ColorAttachmentId,m_BufferSize.x,m_BufferSize.y,0,FilterMode.Bilinear,m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            m_Buffer.GetTemporaryRT(s_DepthAttachmentId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Point, RenderTextureFormat.Depth);
            m_Buffer.GetTemporaryRT(s_MotionVectorsAttachmentId,m_BufferSize.x,m_BufferSize.y,1,FilterMode.Point, RenderTextureFormat.Depth);
            m_Buffer.SetRenderTarget(s_ColorAttachmentId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store
                ,s_DepthAttachmentId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
            //m_Buffer.SetRenderTarget(s_MotionVectorsAttachmentId,s_DepthAttachmentId);
        }
        //设置相机清除状态
        m_Buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? m_Camera.backgroundColor.linear : Color.clear);
        m_Buffer.BeginSample(SampleName);
        m_Buffer.SetGlobalTexture(s_ColorTextureId, m_MissingTexture);
        m_Buffer.SetGlobalTexture(s_DepthTextureId,m_MissingTexture);
        m_Buffer.SetGlobalTexture(s_MotionVectorsTextureId,m_MissingTexture);
        ExecuteBuffer();
        
    }

    private void Cleanup()
    {
        m_Lighting.Cleanup();
        if (m_UseIntermediateBuffer)
        {
            m_Buffer.ReleaseTemporaryRT(s_ColorAttachmentId);
            m_Buffer.ReleaseTemporaryRT(s_DepthAttachmentId);
            m_Buffer.ReleaseTemporaryRT(s_MotionVectorsAttachmentId);
            if (m_UseDepthTexture)
            {
                m_Buffer.ReleaseTemporaryRT(s_DepthTextureId);
            }

            if (m_UseColorTexture)
            {
                m_Buffer.ReleaseTemporaryRT(s_ColorTextureId);
            }

            if (m_UseMotionVectorTexture)
            {
                m_Buffer.ReleaseTemporaryRT(s_MotionVectorsTextureId);
            }
        }
    }
    
    private void CopyAttachments()
    {
        if (m_UseColorTexture)
        {
            m_Buffer.GetTemporaryRT(s_ColorTextureId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Bilinear
                ,m_UseHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            if (s_CopyTextureSupported)
            {
                m_Buffer.CopyTexture(s_ColorAttachmentId,s_ColorTextureId);
            }
            else
            {
                Draw(s_ColorAttachmentId,s_ColorTextureId);
            }
            ExecuteBuffer();
        }
        
        if (m_UseDepthTexture)
        {
            m_Buffer.GetTemporaryRT(s_DepthTextureId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Point,RenderTextureFormat.Depth);
            if (s_CopyTextureSupported)
            {
                m_Buffer.CopyTexture(s_DepthAttachmentId,s_DepthTextureId);
            }
            else
            {
                Draw(s_DepthAttachmentId,s_DepthTextureId,true);
               
            }
        }

        if (m_UseMotionVectorTexture)
        {
            m_Buffer.GetTemporaryRT(s_MotionVectorsTextureId,m_BufferSize.x,m_BufferSize.y,32,FilterMode.Point,RenderTextureFormat.Depth);
            if (s_CopyTextureSupported)
            {
                m_Buffer.CopyTexture(s_MotionVectorsAttachmentId,s_MotionVectorsTextureId);
            }
            else
            {
                Draw(s_MotionVectorsAttachmentId,s_MotionVectorsTextureId,true);
               
            }
        }
        
        if (!s_CopyTextureSupported)
        {
            m_Buffer.SetRenderTarget(s_ColorAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store
                ,s_DepthAttachmentId,RenderBufferLoadAction.Load,RenderBufferStoreAction.Store);
            m_Buffer.SetRenderTarget(s_MotionVectorsAttachmentId,s_DepthAttachmentId);
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
    private bool Cull(float  maxShadowDistance)
    {
        ScriptableCullingParameters p;

        if (m_Camera.TryGetCullingParameters(out p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, m_Camera.farClipPlane);
            m_CullingResults = m_Content.Cull(ref p);
            return true;
        }
        return false;
    }

    private void PreCull()
    {
        // Matrix4x4 proj = m_Camera.projectionMatrix;
        // m_Camera.nonJitteredProjectionMatrix = proj;
        // m_FrameCount++;
        // int index = m_FrameCount % 8;
        // m_JitterVector2 = new Vector2((m_HaltonSequences[index].x - 0.5f) / m_Camera.pixelWidth
        //     , (m_HaltonSequences[index].y - 0.5f) / m_Camera.pixelHeight);
        // proj.m02 += m_JitterVector2.x * 2;
        // proj.m12 += m_JitterVector2.y * 2;
        // m_Camera.projectionMatrix = proj;
        
        
        float4x4 lastP = GraphicsUtility.GetGPUProjectionMatrix(m_Camera.projectionMatrix, false);
        float4x4 camLocalToWorld = m_Camera.transform.localToWorldMatrix;
        float4x4 nonJitterP = m_Camera.nonJitteredProjectionMatrix;
        float4x4 lastVP = mul(lastP, m_Camera.worldToCameraMatrix);
        float4x4 worldToView = m_Camera.worldToCameraMatrix;
        float4x4 nonJitterPNoTex = GraphicsUtility.GetGPUProjectionMatrix(nonJitterP, false, false);
        float4x4 nonJitterVP = mul(nonJitterPNoTex, worldToView);
        float4x4 nonJitterInverseVP = inverse(nonJitterVP);
        float4x4 nonJitterTextureVP = mul(GraphicsUtility.GetGPUProjectionMatrix(nonJitterP, true, false), worldToView);
        float4x4 lastCameraLocalToWorld = camLocalToWorld;
        float3 sceneOffset = float3.zero;
        lastCameraLocalToWorld.c3.xyz += sceneOffset;
        float4x4 lastV = GetWorldToCamera(ref lastCameraLocalToWorld);
        lastVP = mul(lastP, lastV);
        lastP = nonJitterPNoTex;
        float4x4 lastInverseVP = inverse(lastVP);
        
        float4x4 lastViewProjection = lastVP;
        float4x4 inverseLastViewProjection = lastInverseVP;
    }

    private static float4x4 GetWorldToCamera(ref float4x4 localToWorldMatrix)
    {
        float4x4 worldToCameraMatrix = MathLib.GetWorldToLocal(ref localToWorldMatrix);
        float4 row2 = -float4(worldToCameraMatrix.c0.z, worldToCameraMatrix.c1.z, worldToCameraMatrix.c2.z, worldToCameraMatrix.c3.z);
        worldToCameraMatrix.c0.z = row2.x;
        worldToCameraMatrix.c1.z = row2.y;
        worldToCameraMatrix.c2.z = row2.z;
        worldToCameraMatrix.c3.z = row2.w;
        return worldToCameraMatrix;
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
