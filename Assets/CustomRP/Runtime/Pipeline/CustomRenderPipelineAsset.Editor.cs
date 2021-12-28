using CustomRP;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Lightmapping = UnityEngine.Experimental.GlobalIllumination.Lightmapping;

public partial class CustomRenderPipelineAsset 
{
#if UNITY_EDITOR
    private static readonly string[] s_RenderingLayerNames;

    static CustomRenderPipelineAsset()
    {
        s_RenderingLayerNames = new string[31];
        for (int i = 0; i < s_RenderingLayerNames.Length; i++)
        {
            s_RenderingLayerNames[i] = "Layer" + (i + 1);
        }
    }

    public override string[] renderingLayerMaskNames => s_RenderingLayerNames;

   
#endif
}
