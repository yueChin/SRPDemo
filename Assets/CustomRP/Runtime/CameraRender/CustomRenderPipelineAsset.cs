using CustomRP;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 自定义渲染管线资产
/// </summary>
//该标签会在你在Project下右键->Asset/Create菜单中添加一个新的子菜单
[CreateAssetMenu(menuName ="Rendering/CreateCustomRenderPipeline")]
public partial class CustomRenderPipelineAsset : RenderPipelineAsset
{

    [SerializeField]
    private Shader m_CameraRendererShader = default;
    
    [SerializeField]
    private bool m_UseDynamicBatching, m_UseGPUInstancing, m_UseSRPBatcher ,m_UseLightsPerObject= true;

    [SerializeField]
    private ShadowSettings m_ShadowSettings;

    [SerializeField]
    private PostFXSettings m_PostFXSettings = default;

    [SerializeField]
    private CameraBufferSettings m_CameraBufferSettings = new CameraBufferSettings()
    {
        AllowHDR = true,
        RenderScale = 1,
        AA = new AA()
        {
            FXAA = new FXAA()
            {
                FixedThreshold = 0.0833f,
                RelativeThreshold = 0.16f,
                SubpixelBlending = 0.75f,
            },
            TAA = new TAA()
            {
                JitterSpread = 0.75f,
                Sharpness = 0.25f,
                StationaryBlending = 0.95f,
                MotionBlending = 0.85f
            } ,
        },
    };

    [SerializeField]
    private ColorLUTResolution m_ColorLUTResolution = ColorLUTResolution._32;
    
    //重写抽象方法，需要返回一个RenderPipeline实例对象
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(m_UseDynamicBatching,m_UseGPUInstancing,m_UseSRPBatcher,m_UseLightsPerObject,m_CameraBufferSettings
            ,m_ShadowSettings,m_PostFXSettings,(int)m_ColorLUTResolution,m_CameraRendererShader);
    }
}
