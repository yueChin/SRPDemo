using UnityEngine;

namespace CustomRP.Runtime
{
    public partial class Lighting
    {
        private Shadows m_Shadows = new Shadows();
        private static int s_DireLIghtShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
        private static Vector4[] s_DirLightShadowData = new Vector4[c_MaxDirLightCount];
    }
}