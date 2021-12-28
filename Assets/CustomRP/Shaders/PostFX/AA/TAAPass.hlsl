#ifndef CUSTOM_TAA_PASS_INCLUDED
#define CUSTOM_TAA_PASS_INCLUDED

#include "Assets/CustomRP/Shaders/PostFX/PostFXInput.hlsl"
#include "Assets/CustomRP/ShaderLibrary/UnityPostFXXRLib.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Fragment.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

TEXTURE2D(_HistoryTex);
SAMPLER(sampler_HistoryTex);

TEXTURE2D(_LastFrameDepthTexture);
SAMPLER(sampler_LastFrameDepthTexture);

TEXTURE2D(_LastFrameMotionVectors);
SAMPLER(sampler_LastFrameMotionVectors);
float3 _TemporalClipBounding;

float2 _Jitter;
float4 _FinalBlendParameters; // x: static, y: dynamic, z: motion amplification
float _Sharpness;

struct appdata
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	float2 texcoord : TEXCOORD0;
};

v2f vert(appdata v)
{
	v2f o;
	o.vertex = v.vertex;
	o.texcoord = v.texcoord;
	return o;
}

/////////////////////////////////////////////////////////////////////////////////////////////CGBull TemporalAA
#ifndef AA_VARIANCE
    #define AA_VARIANCE 1
#endif

#ifndef AA_Filter
    #define AA_Filter 1
#endif

#define SAMPLE_DEPTH_OFFSET(x,y,z,a) (x.Sample(y,z,a).r )
#define SAMPLE_TEXTURE2D_OFFSET(x,y,z,a) (x.Sample(y,z,a))

float2 _LastJitter;

inline float2 LinearEyeDepth( float2 z )
{
    return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}

inline float2 Linear01Depth( float2 z )
{
return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
}
float Luma4(float3 Color)
{
    return (Color.g * 2) + (Color.r + Color.b);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// float3 RGBToYCoCg(float3 RGB)
// {
//     const float3x3 mat = float3x3(0.25,0.5,0.25,0.5,0,-0.5,-0.25,0.5,-0.25);
//     float3 col =mul(mat, RGB);
//     return col;
// }
//
// float3 YCoCgToRGB(float3 YCoCg)
// {
//     const float3x3 mat = float3x3(1,1,-1,1,0,1,1,-1,-1);
//     return mul(mat, YCoCg);
// }

float4 RGBToYCoCg(float4 RGB)
{
    return float4(RGBToYCoCg(RGB.xyz), RGB.w);
}

float4 YCoCgToRGB(float4 YCoCg)
{
    return float4(YCoCgToRGB(YCoCg.xyz), YCoCg.w); 
}

float Luma(float3 Color)
{
    return (Color.g * 0.5) + (Color.r + Color.b) * 0.25;
}

#define TONE_BOUND 0.5
float3 Tonemap(float3 x) 
{ 
    float luma = Luma(x);
    [flatten]
    if(luma <= TONE_BOUND)
        return x;
    else
        return x * (TONE_BOUND * TONE_BOUND - luma) / (luma * (2 * TONE_BOUND - 1 - luma));
    //return x * weight;
}

float3 TonemapInvert(float3 x) { 
    float luma = Luma(x);
    [flatten]
    if(luma <= TONE_BOUND)
        return x;
    else
        return x * (TONE_BOUND * TONE_BOUND - (2 * TONE_BOUND - 1) * luma) / (luma * (1 - luma));
}

float Pow2(float x)
{
    return x * x;
}

float HdrWeight4(float3 Color, const float Exposure) 
{
    return rcp(Luma4(Color) * Exposure + 4);
}

float3 ClipToAABB(float3 color, float3 minimum, float3 maximum)
{
    // Note: only clips towards aabb center (but fast!)
    float3 center = 0.5 * (maximum + minimum);
    float3 extents = 0.5 * (maximum - minimum);

    // This is actually `distance`, however the keyword is reserved
    float3 offset = color.rgb - center;

    float3 ts = abs(extents / (offset + 0.0001));
    float t = saturate(Min3(ts.x, ts.y, ts.z));
    color.rgb = center + offset * t;
    return color;
}

static const int2 _OffsetArray[8] = {
    int2(-1, -1),
    int2(0, -1),
    int2(1, -1),
    int2(-1, 0),
    int2(1, 1),
    int2(1, 0),
    int2(-1, 1),
    int2(0, -1)
};

#define HALF_MAX_MINUS1 65472.0 // (2 - 2^-9) * 2^15

#if defined(UNITY_REVERSED_Z)
    #define COMPARE_DEPTH(a, b) step(b, a)
#else
    #define COMPARE_DEPTH(a, b) step(a, b)
#endif

float2 ReprojectedMotionVectorUV(float2 uv, out float outDepth)
{
    float neighborhood;
    const float2 k = _CameraDepthTexture_TexelSize.xy;
    uint i;
    outDepth  = _CameraDepthTexture.Sample(sampler_CameraDepthTexture, uv).x;
    float3 result = float3(0, 0,  outDepth);
    UNITY_UNROLL
    for(i = 0; i < 8; ++i){
        neighborhood = SAMPLE_DEPTH_OFFSET(_CameraDepthTexture, sampler_CameraDepthTexture, uv, _OffsetArray[i]);
        result = lerp(result, float3(_OffsetArray[i], neighborhood), COMPARE_DEPTH(neighborhood, result.z));
    }

    return uv + result.xy * k;
}

struct Neighborhood
{
    float totalWeight,m1,m2;
    float4 filtered,curtColor; 
};

float4 GetSourceUV(float2 uv,float uOffset = 0.0,float vOffset = 0.0)
{
    uv += float2(uOffset ,vOffset) * GetSourceTexelSize().xy;
    return GetSource(uv);

}

Neighborhood GetWeightAndFiltered(float2 uv,float4 mc,float scale)
{
    Neighborhood neighborhood;
    float4 tl = GetSourceUV(uv,-1.0,-1.0);
    float4 tc = GetSourceUV(uv,0.0,-1.0);
    float4 tr = GetSourceUV(uv,1.0,-1.0);
    float4 ml = GetSourceUV(uv,-1.0,0.0);
    float4 mr = GetSourceUV(uv,1.0,0.0);
    float4 bl = GetSourceUV(uv,-1.0,1.0);
    float4 bc = GetSourceUV(uv,0.0,1.0);
    float4 br = GetSourceUV(uv,1.0,1.0);
    float samplesWeight[9];
    samplesWeight[0] = HdrWeight4(tl.rgb, scale);
    samplesWeight[1] = HdrWeight4(tc.rgb, scale);
    samplesWeight[2] = HdrWeight4(tr.rgb, scale);
    samplesWeight[3] = HdrWeight4(ml.rgb, scale);
    samplesWeight[4] = HdrWeight4(mc.rgb, scale);
    samplesWeight[5] = HdrWeight4(mr.rgb, scale);
    samplesWeight[6] = HdrWeight4(bl.rgb, scale);
    samplesWeight[7] = HdrWeight4(bc.rgb, scale);
    samplesWeight[8] = HdrWeight4(br.rgb, scale);
    tl = RGBToYCoCg(tl);
    tc = RGBToYCoCg(tc);
    tr = RGBToYCoCg(tr);
    ml = RGBToYCoCg(ml);
    mc = RGBToYCoCg(mc);
    mr = RGBToYCoCg(mr);
    bl = RGBToYCoCg(bl);
    bc = RGBToYCoCg(bc);
    br = RGBToYCoCg(br);
    
    neighborhood.totalWeight = samplesWeight[0] + samplesWeight[1]+ samplesWeight[2]+ samplesWeight[3] + samplesWeight[4] + samplesWeight[5]+ samplesWeight[6] + samplesWeight[7] + samplesWeight[8];                   
    neighborhood.filtered = (tl * samplesWeight[0] + tc * samplesWeight[1] + tr * samplesWeight[2] + ml * samplesWeight[3] + mc * samplesWeight[4] + mr * samplesWeight[5] + bl * samplesWeight[6] + bc * samplesWeight[7] + br * samplesWeight[8]) / neighborhood.totalWeight;

    neighborhood.m1 = tl + tc + tr + ml + mc + mr + bl + bc + br;
    neighborhood.m2 = tl * tl + tc * tc + tr * tr + ml * ml + mc * mc + mr * mr + bl * bl + bc * bc + br * br;

    neighborhood.curtColor = YCoCgToRGB(mc);
    
    float4 corners = ( YCoCgToRGB(tl + br + tr + bl) - neighborhood.curtColor ) * 2;
    neighborhood.curtColor += (neighborhood.curtColor - (corners * 0.166667) ) * 2.718282 * _Sharpness;
    neighborhood.curtColor = clamp(neighborhood.curtColor, 0, HALF_MAX_MINUS1);
    return neighborhood;
}

float4 Solver_CGBullTAA(v2f i) : SV_TARGET0
{
    const float ExposureScale = 10;
    float2 uv = (i.texcoord - _Jitter);
    float depth;
    float2 closest = ReprojectedMotionVectorUV(i.texcoord, /*out*/depth);
    float2 velocity = SAMPLE_TEXTURE2D(_CameraMotionVectorsTexture, sampler_CameraMotionVectorsTexture, closest).xy;

//////////////////TemporalClamp
    float2 PrevCoord = (i.texcoord - velocity);
    float4 MiddleCenter = GetSourceUV(uv,0.0,0.0);
    if (PrevCoord.x > 1 || PrevCoord.y > 1 || PrevCoord.x < 0 || PrevCoord.y < 0) {
        return float4(MiddleCenter.xyz, 1);
    }

    Neighborhood neighborhood = GetWeightAndFiltered(uv,MiddleCenter,ExposureScale);
    // Resolver Average
    float VelocityLength = length(velocity);
    float VelocityWeight = saturate(VelocityLength * _TemporalClipBounding.z);
    float AABBScale = lerp(_TemporalClipBounding.x, _TemporalClipBounding.y, VelocityWeight);

    float4 mean = neighborhood.m1 / 9;
    float4 stddev = sqrt(neighborhood.m2 / 9 - mean * mean);  

    float4 minColor = mean - AABBScale * stddev;
    float4 maxColor = mean + AABBScale * stddev;
    minColor = min(minColor, neighborhood.filtered);
    maxColor = max(maxColor, neighborhood.filtered);

//////////////////TemporalResolver

    // HistorySample
    float2 prevDepthUV = PrevCoord + _Jitter - _LastJitter;
    float lastFrameDepth = _LastFrameDepthTexture.Sample(sampler_LastFrameDepthTexture, prevDepthUV);
    float2 lastFrameMV = _LastFrameMotionVectors.Sample(sampler_LastFrameMotionVectors, prevDepthUV);
    float lastFrameMVLen = dot(lastFrameMV, lastFrameMV);
    UNITY_UNROLL
    for(uint ite = 0; ite < 8; ++ite)
    {
        float2 currentMV = _LastFrameMotionVectors.Sample(sampler_LastFrameMotionVectors, prevDepthUV, _OffsetArray[ite]);
        float currentMVLen = dot(currentMV, currentMV);
        lastFrameMVLen = max(currentMVLen, lastFrameMVLen);
    }
    float LastVelocityWeight = saturate(sqrt(lastFrameMVLen) * _TemporalClipBounding.z);
     float4 worldPos = mul(_InvNonJitterVP, float4(i.texcoord, depth, 1));
    float4 lastWorldPos = mul(_InvLastVp, float4(prevDepthUV, lastFrameDepth, 1));
    worldPos /= worldPos.w; lastWorldPos /= lastWorldPos.w;
    worldPos -= lastWorldPos;
    float depthAdaptiveForce = 1 - saturate((dot(worldPos.xyz, worldPos.xyz) - 0.02) * 10);
    float4 PrevColor = SAMPLE_TEXTURE2D(_HistoryTex, sampler_HistoryTex, PrevCoord);
    float colDiff = depthAdaptiveForce - PrevColor.w;//Whether current Color is brighter than last
    float tWeight = lerp(0.7, 0.9, saturate(tanh(colDiff * 2) * 0.5 + 0.5));
    depthAdaptiveForce = lerp(depthAdaptiveForce, PrevColor.w, tWeight);
    depthAdaptiveForce = lerp(depthAdaptiveForce, 1, VelocityWeight);
    depthAdaptiveForce = lerp(depthAdaptiveForce, 1, LastVelocityWeight);
   
    float2 depth01 = Linear01Depth(float2(lastFrameDepth, depth));
    float finalDepthAdaptive = lerp(depthAdaptiveForce, 1, (depth01.x > 0.9999) || (depth01.y > 0.9999));
    PrevColor.xyz =  lerp(PrevColor.xyz, YCoCgToRGB( ClipToAABB( RGBToYCoCg(PrevColor.xyz), minColor.xyz, maxColor.xyz )), finalDepthAdaptive);
    // HistoryBlend
  //  return float4(lerp(depthAdaptiveForce, 1, (depth01.x > 0.9999) || (depth01.y > 0.9999)).xxx, depthAdaptiveForce);
    float HistoryWeight = lerp(_FinalBlendParameters.x, _FinalBlendParameters.y, VelocityWeight);
    neighborhood.curtColor.xyz = Tonemap(neighborhood.curtColor.xyz);
    PrevColor.xyz = Tonemap(PrevColor.xyz);
    float4 TemporalColor = lerp(neighborhood.curtColor, PrevColor, HistoryWeight);
    TemporalColor.xyz = TonemapInvert(TemporalColor.xyz);
    TemporalColor.w = depthAdaptiveForce;
    return max(0, TemporalColor);
}

#endif