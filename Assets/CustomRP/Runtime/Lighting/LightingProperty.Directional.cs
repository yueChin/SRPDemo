using UnityEngine;

namespace CustomRP.Runtime
{
    public partial class Lighting
    {
        private static int s_DirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
        private static int s_DirLightDirctionId = Shader.PropertyToID("_DirectionalLightDirection");

        private const int c_MaxDirLightCount = 4;

        private static int s_DirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
        private static int s_DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        private static int s_DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirectionsAndMask");

        private static Vector4[] s_DirLightColors = new Vector4[c_MaxDirLightCount];
        private static Vector4[] s_DirectionalLightDirectionsAndMasks = new Vector4[c_MaxDirLightCount];
    }
}