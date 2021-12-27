using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
/// <summary>
/// 相机渲染管理类
/// </summary>
public partial class CameraRenderer
{
#if UNITY_EDITOR
    //SRP不支持的着色器标签类型
    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };
    //绘制成使用错误材质的粉红颜色
    private static Material s_ErrorMaterial;

    private string SampleName { get; set; }

    /// <summary>
    /// 绘制SRP不支持的内置shader类型
    /// </summary>
    partial void DrawUnsupportedShaders()
    {
        //不支持的shaderTag类型我们使用错误材质专用shader来渲染(粉色颜色)
        if (s_ErrorMaterial == null)
        {
            s_ErrorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
         
        //数组第一个元素用来构造DrawingSettings的时候设置
        DrawingSettings drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(m_Camera))
        {overrideMaterial = s_ErrorMaterial };
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            //遍历数组逐个设置着色器的PassName，从i=1开始
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        //使用默认设置即可，反正画出来的都是错误的
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        //绘制不支持的shaderTag类型的物体
        m_Content.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
    }
    
    partial void DrawGizmosBeforeFX()
    {
        if (Handles.ShouldRenderGizmos())
        {
            if (m_UseIntermediateBuffer)
            {
                Draw(s_DepthAttachmentId,BuiltinRenderTextureType.CameraTarget,CameraPass.Depth);
                ExecuteBuffer();
            }
            m_Content.DrawGizmos(m_Camera, GizmoSubset.PreImageEffects);
        }
    }
    
    partial void DrawGizmosAfterFX()
    {
        if (Handles.ShouldRenderGizmos())
        {
            m_Content.DrawGizmos(m_Camera, GizmoSubset.PostImageEffects);
        }
    }
    
    /// <summary>
    /// 在Game视图绘制的几何体也绘制到Scene视图中
    /// </summary>
    partial void PrepareForSceneWindow()
    {
        if (m_Camera.cameraType == CameraType.SceneView)
        {
            //如果切换到了Scene视图，调用此方法完成绘制
            ScriptableRenderContext.EmitWorldGeometryForSceneView(m_Camera);
            m_UseScaleRendering = false;
        }
    }

    /// <summary>
    /// 设置buffer缓冲区的名字
    /// </summary>
    partial void PrepareBuffer()
    {
        //设置一下只有在编辑器模式下才分配内存
        Profiler.BeginSample("Editor Only");
        m_Buffer.name = SampleName = m_Camera.name;
        Profiler.EndSample();
    }
    
#else
	const string SampleName = bufferName;

#endif
}
