using CustomRP;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 自定义渲染管线实例
/// </summary>
public partial class CustomRenderPipeline : RenderPipeline
{
    private readonly CameraRenderer m_Renderer ;

    private readonly bool m_UseDynamicBatching, m_UseGPUInstancing,m_UseLightPerObject;

    private readonly ShadowSettings m_ShadowSettings;

    private readonly PostFXSettings m_PostFXSettings;

    private readonly CameraBufferSettings m_CameraBufferSettings;
    
    private readonly int m_ColorLUTResolution;
    
    public CustomRenderPipeline(bool useDynamicBatching,bool useGPUInstancing,bool useSRPBatcher,bool useLightsPerObject,CameraBufferSettings cameraBufferSettings
        ,ShadowSettings shadowSettings,PostFXSettings postFXSettings,int colorLutResolution,Shader cameraRendererShader)
    {
        this.m_ShadowSettings = shadowSettings;
        this.m_UseDynamicBatching = useDynamicBatching;
        this.m_UseGPUInstancing = useGPUInstancing;
        this.m_UseLightPerObject = useLightsPerObject;
        this.m_PostFXSettings = postFXSettings;
        this.m_CameraBufferSettings = cameraBufferSettings;
        this.m_ColorLUTResolution = colorLutResolution;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.m_Renderer = new CameraRenderer(cameraRendererShader);
        InitializeForEditor();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //遍历所有相机单独渲染
        foreach (Camera camera in cameras)
        {
            m_Renderer.Render(context, camera,m_UseDynamicBatching,m_UseGPUInstancing,m_UseLightPerObject,m_CameraBufferSettings,m_ShadowSettings,m_PostFXSettings,m_ColorLUTResolution);
        }
        
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DisposeForEditor();
        m_Renderer.Dispose();
    }

    partial void InitializeForEditor();
    partial void DisposeForEditor();

}
