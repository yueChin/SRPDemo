#ifndef CUSTOM_TAA_PASS_INCLUDED
#define CUSTOM_TAA_PASS_INCLUDED

#include "Assets/CustomRP/Shaders/PostFX/PostFXInput.hlsl"
#include "Assets/CustomRP/ShaderLibrary/UnityPostFXXRLib.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Fragment.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

struct Varyings
{
    float4 positionCS_SS :SV_POSITION;
    float2 screenUV :VAR_SCRENN_UV;
};

TEXTURE2D(_HistoryTex);
int _IgnoreHistory;
float2 _Jitter;

static const int2 kOffsets3x3[9] =
{
    int2(-1, -1),
    int2( 0, -1),
    int2( 1, -1),
    int2(-1,  0),
    int2( 0,  0),
    int2( 1,  0),
    int2(-1,  1),
    int2( 0,  1),
    int2( 1,  1),
};

float2 GetClosestFragment(float2 uv)
{
    float2 k = _CameraDepthTexture_TexelSize.xy;
    const float4 neighborhood = float4(
        SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_point_clamp, UnityStereoClamp(uv - k),0),
        SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_point_clamp, UnityStereoClamp(uv + float2(k.x, -k.y)),0),
        SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_point_clamp, UnityStereoClamp(uv + float2(-k.x, k.y)),0),
        SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_point_clamp, UnityStereoClamp(uv + k),0)
    );
    
#if UNITY_REVERSED_Z
    #define COMPARE_DEPTH(a, b) step(b, a)
#else
    #define COMPARE_DEPTH(a, b) step(a, b)
    
#endif
    float3 result = float3(0.0, 0.0, SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, sampler_point_clamp, uv),0);
    result = lerp(result, float3(-1.0, -1.0, neighborhood.x), COMPARE_DEPTH(neighborhood.x, result.z));
    result = lerp(result, float3( 1.0, -1.0, neighborhood.y), COMPARE_DEPTH(neighborhood.y, result.z));
    result = lerp(result, float3(-1.0,  1.0, neighborhood.z), COMPARE_DEPTH(neighborhood.z, result.z));
    result = lerp(result, float3( 1.0,  1.0, neighborhood.w), COMPARE_DEPTH(neighborhood.w, result.z));
    return (uv + result.xy * k);
}

// float3 RGBToYCoCg( float3 RGB )
// {
//     float Y  = dot( RGB, float3(  1, 2,  1 ) );
//     float Co = dot( RGB, float3(  2, 0, -2 ) );
//     float Cg = dot( RGB, float3( -1, 2, -1 ) );
//     
//     float3 YCoCg = float3( Y, Co, Cg );
//     return YCoCg;
// }
//
// float3 YCoCgToRGB( float3 YCoCg )
// {
//     float Y  = YCoCg.x * 0.25;
//     float Co = YCoCg.y * 0.25;
//     float Cg = YCoCg.z * 0.25;
//
//     float R = Y + Co - Cg;
//     float G = Y + Cg;
//     float B = Y - Co - Cg;
//
//     float3 RGB = float3( R, G, B );
//     return RGB;
// }


float3 ClipHistory(float3 History, float3 BoxMin, float3 BoxMax)
{
    float3 Filtered = (BoxMin + BoxMax) * 0.5f;
    float3 RayOrigin = History;
    float3 RayDir = Filtered - History;
    RayDir = abs( RayDir ) < (1.0/65536.0) ? (1.0/65536.0) : RayDir;
    float3 InvRayDir = rcp( RayDir );

    float3 MinIntersect = (BoxMin - RayOrigin) * InvRayDir;
    float3 MaxIntersect = (BoxMax - RayOrigin) * InvRayDir;
    float3 EnterIntersect = min( MinIntersect, MaxIntersect );
    float ClipBlend = max( EnterIntersect.x, max(EnterIntersect.y, EnterIntersect.z ));
    ClipBlend = saturate(ClipBlend);
    return lerp(History, Filtered, ClipBlend);
}
            
float4 TAAPassFragment(Varyings input) : SV_Target
{
    float2 uv = input.screenUV - _Jitter;
    float4 Color = GetSource(uv);
    //当没有上帧的历史数据，就直接使用当前帧的数据
    if(_IgnoreHistory)
    {
        return Color;
    }
     
    //因为镜头的移动会导致物体被遮挡关系变化，这步的目的是选择出周围距离镜头最近的点
    float2 closest = GetClosestFragment(input.screenUV);
    
    //得到在屏幕空间中，和上帧相比UV偏移的距离
    float2 Motion = SAMPLE_TEXTURE2D(_CameraMotionVectorsTexture, sampler_linear_clamp, closest).xy;

    float2 HistoryUV = input.screenUV - Motion;
    float4 HistoryColor = _HistoryTex.Sample(sampler_linear_clamp, HistoryUV);

    // 在 YCoCg色彩空间中进行Clip判断
    float3 AABBMin, AABBMax;
    AABBMax = AABBMin = RGBToYCoCg(Color);
    for(int k = 0; k < 9; k++)
    {
        float3 color = RGBToYCoCg(GetSource(input.screenUV, kOffsets3x3[k]).rgb);
        AABBMin = min(AABBMin, color);
        AABBMax = max(AABBMax, color);
    }
    float3 HistoryYCoCg = RGBToYCoCg(HistoryColor);
    //根据AABB包围盒进行Clip计算:
    HistoryColor.rgb = YCoCgToRGB(ClipHistory(HistoryYCoCg, AABBMin, AABBMax));
    // Clamp计算
    // HistoryColor.rgb = YCoCgToRGB(clamp(HistoryYCoCg, AABBMin, AABBMax));
    
    //跟随速度变化混合系数
    float BlendFactor = saturate(0.05 + length(Motion) * 1000);

    if(HistoryUV.x < 0 || HistoryUV.y < 0 || HistoryUV.x > 1.0f || HistoryUV.y > 1.0f)
    {
        BlendFactor = 1.0f;
    }
    return lerp(HistoryColor, Color, BlendFactor);
}

#endif