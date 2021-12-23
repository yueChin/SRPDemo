using UnityEngine;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private const int c_MaxCascades = 4;
        
        private static int s_CascadeCountId = Shader.PropertyToID("_CascadeCount");
        private static int s_CascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
        private static int s_CascadeDataId = Shader.PropertyToID("_CascadeData");

        private static Vector4[] s_CascadeCullingSpheres = new Vector4[c_MaxCascades];
        private static Vector4[] s_CascadeData = new Vector4[c_MaxCascades];
        
        private static string[] s_CascadeBlendKeywords =
        {
            "_CASCADE_BLEND_SOFT",
            "_CASCADE_BLEND_DITHER",
        };
    }
}