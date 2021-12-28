using UnityEngine;

public static partial class ShaderIds
{
    public static readonly int ShadowDistanceId = Shader.PropertyToID("_ShadowDistance");
    public static readonly int ShadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
    public static readonly int ShadowPancakingId = Shader.PropertyToID("_ShadowPancaking");
    public static readonly int ShadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");
    
    public static readonly int DireShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    public static readonly int DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    
    public static readonly int CascadeCountId = Shader.PropertyToID("_CascadeCount");
    public static readonly int CascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
    public static readonly int CascadeDataId = Shader.PropertyToID("_CascadeData");
    
    public static readonly int OtherShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas");
    public static readonly int OtherShadowMatricesId = Shader.PropertyToID("_OtherShadowMatrices");
    public static readonly int OtherShadowTilesId = Shader.PropertyToID("_OtherShadowTiles");

}