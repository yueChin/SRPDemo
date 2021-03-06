using UnityEngine;

namespace CustomRP.Runtime
{
    public partial class Shadows
    {
        private const int c_MaxCascades = 4;

        private static readonly Vector4[] s_CascadeCullingSpheres = new Vector4[c_MaxCascades];
        private static readonly Vector4[] s_CascadeData = new Vector4[c_MaxCascades];
        
        private static readonly string[] s_CascadeBlendKeywords =
        {
            "_CASCADE_BLEND_SOFT",
            "_CASCADE_BLEND_DITHER",
        };
        
        private void SetCascadeData(int index,Vector4 cullingSphere,float tileSize)
        {
            // s_CascadeData[index].x = 1f / cullingSphere.w;
            // cullingSphere.w *= cullingSphere.w;
            // s_CascadeCullingSpheres[index] = cullingSphere;
            float texelSize = 2f * cullingSphere.w / tileSize;
            float filterSize = texelSize * ((float)m_Settings.Directional.Filter + 1f);
            cullingSphere.w -= filterSize;
            cullingSphere.w *= cullingSphere.w;
            s_CascadeCullingSpheres[index] = cullingSphere;
            s_CascadeData[index] = new Vector4(1f / cullingSphere.w, filterSize * 1.4142136f);
        }
    }
}