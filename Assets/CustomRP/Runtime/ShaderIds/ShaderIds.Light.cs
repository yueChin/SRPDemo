using UnityEngine;

public static partial class ShaderIds
{
    public static readonly int DirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
    public static readonly int DirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    public static readonly int DirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    public static readonly int DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    public static readonly int DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirectionsAndMask");
    
    public static readonly int OtherLightCountId = Shader.PropertyToID("_OtherLightCount");
    public static readonly int OtherLightColorsId = Shader.PropertyToID("_OtherLightColors");
    public static readonly int OtherLightPositionsId = Shader.PropertyToID("_OtherLightPositions");
    public static readonly int OtherLightDirectionsAndMaskId = Shader.PropertyToID("_OtherLightDirectionsAndMask");
    public static readonly int OtherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles");
    public static readonly int OtherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");
    
    public static readonly int DireLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

}