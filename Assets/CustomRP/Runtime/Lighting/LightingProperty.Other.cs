using UnityEngine;

namespace CustomRP.Runtime
{
    public partial class Lighting
    {
        private const int c_MaxOtherLightCount = 64;

        private static int s_OtherLightCountId = Shader.PropertyToID("_OtherLightCount");
        private static int s_OtherLightColorsId = Shader.PropertyToID("_OtherLightColors");
        private static int s_OtherLightPositionsId = Shader.PropertyToID("_OtherLightPositions");
        //private static int s_OtherLightDirecitonsId = Shader.PropertyToID("_OtherLightDirectionsAndMask");
        private static int s_OtherLightDirectionsAndMaskId = Shader.PropertyToID("_OtherLightDirectionsAndMask");
        private static int s_OtherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles");
        private static int s_OtherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");
        
        private static Vector4[] s_OtherLightColors = new Vector4[c_MaxOtherLightCount];
        private static Vector4[] s_OtherLightPositions = new Vector4[c_MaxOtherLightCount];
        //private static Vector4[] s_OtherLightDirecitons = new Vector4[c_MaxOtherLightCount];
        private static Vector4[] s_OtherLightDirectionsAndMask = new Vector4[c_MaxOtherLightCount];
        private static Vector4[] s_OtherLightSpotAngles = new Vector4[c_MaxOtherLightCount];
        private static Vector4[] s_OtherLightShadowData = new Vector4[c_MaxOtherLightCount];
    }
}