#ifndef CUSTOM_POST_FX_BLOOM_INCLUDED
#define CUSTOM_POST_FX_BLOOM_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/PostFX/ColorGrade.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/ColorToneMapping.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/Bloom.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/WhiteBalance.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/SplitToning.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/ChannelMixer.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/ShadowsMidtonesHighlights.hlsl"
#include "Assets/CustomRP/Shaders//PostFX/PostFXInput.hlsl"

float4 _ColorGradingLUTParameters;
bool _ColorGradingLUTInLogC;
TEXTURE2D(_ColorGradingLUT);

bool _CopyBicubic;

struct Varyings
{
    float4 positionCS_SS :SV_POSITION;
    float2 screenUV :VAR_SCRENN_UV;
};


float3 ColorGrade(float3 color,bool useACES)
{
    //color = min(color,60.0);
    color = ColorGradePostExposure(color);
    color = ColorGradeWhiteBalance(color);
    color = ColorGradingContrast(color,useACES);
    color = ColorGradingFilter(color);
    color = max(color,0.0);
    color = ColorGradeSplitToning(color,useACES);
    color = ColorGradingChannelMixer(color);
    color = max(color,0.0);
    color = ColorGradingShadowsMidtonesHighlights(color,useACES);
    color = ColorGradingHueShift(color);
    color = ColorGradingSaturation(color,useACES);
    color = max(useACES ? ACEScg_to_ACES(color) : color, 0.0);
    return color;

    color = ColorGradePostExposure(color);
    color = ColorGradeWhiteBalance(color);
    color = ColorGradingContrast(color, useACES);
    color = ColorGradingFilter(color);
    //消除负值
    color = max(color, 0.0);
    color = ColorGradeSplitToning(color, useACES);
    color = ColorGradingChannelMixer(color);
    color = max(color, 0.0);
    color = ColorGradingShadowsMidtonesHighlights(color, useACES);
    color = ColorGradingHueShift(color);
    color = ColorGradingSaturation(color, useACES);
    return max(useACES ? ACEScg_to_ACES(color) : color, 0.0);
    
}

float3 GetColorGradedLUT(float2 uv,bool useACES = false)
{
    float3 color = GetLutStripValue(uv,_ColorGradingLUTParameters);
    return ColorGrade(_ColorGradingLUTInLogC ? LogCToLinear(color) : color,useACES);
}

float4 BloomPrefilterFirefliesPassFragment(Varyings input) :SV_TARGET
{
    float3 color = 0.0;
    float weightSum = 0.0;
    float2 offsets[] =
    {
        float2(0.0,0.0),
        float2(-1.0,-1.0),float2(-1.0,1.0),float2(1.0,-1.0),float2(1.0,1.0),
    };
    for (int i = 0;i < 5;i++)
    {
        float3 c = GetSource(input.screenUV + offsets[i] * GetSourceTexelSize().xy * 2.0).rgb;
        c = ApplyBloomThreshold(c);
        float w = 1.0 / (Luminance(c) + 1.0);
        color += c* w;
        weightSum += w;
    }
    color /= weightSum;
    return float4(color,0.0);
}

float4 BloomPrefilterPassFragment(Varyings input) : SV_TARGET
{
    float3 color = ApplyBloomThreshold(GetSource(input.screenUV).rgb);
    return float4(color,1.0);
}

float4 BloomScatterFinalPassFragment(Varyings input) : SV_TARGET
{
    float3 lowRes;
    if(_BloomBicubicUpsampling)
    {
        lowRes = GetSourceBicubic(input.screenUV).rgb;
    }
    else
    {
        lowRes = GetSource(input.screenUV).rgb;
    }
    float4 highRes = GetSource2(input.screenUV);
    lowRes += highRes - ApplyBloomThreshold(highRes);
    return float4(lerp(highRes,lowRes,_BloomIntensity) , highRes.a);
}

float4 BloomScatterPassFragment(Varyings input) : SV_TARGET
{
    float3 lowRes;
    if(_BloomBicubicUpsampling)
    {
        lowRes = GetSourceBicubic(input.screenUV).rgb;
    }
    else
    {
        lowRes = GetSource(input.screenUV).rgb;
    }
    float3 highRes = GetSource2(input.screenUV).rgb;
    return float4(lerp(highRes,lowRes,_BloomIntensity) ,1.0);
}

float4 BloomAddPassFragment(Varyings input) : SV_TARGET
{
    float3 lowRes;
    if(_BloomBicubicUpsampling)
    {
        lowRes = GetSourceBicubic(input.screenUV).rgb;
    }
    else
    {
        lowRes = GetSource(input.screenUV).rgb;
    }
    float4 highRes = GetSource2(input.screenUV);
    return float4(lowRes * _BloomIntensity + highRes,highRes.a);
}

float4 BloomHorizontalPassFragment(Varyings input) : SV_TARGET
{
    float3 color = 0.0;
    float offsets[] = {-4.0,-3.0,-2.0,-1.0,0.0,1.0,2.0,3.0,4.0};
    float weights[] =
        {
            0.01621622,0.05405405,0.12162162,0.19459459,0.22702703,
            0.19459459,0.12162162,0.05405405,0.01621622
        };
    for (int i = 0;i < 9; i ++)
    {
        float offset = offsets[i] * 2.0 * GetSourceTexelSize().x;
        color += GetSource(input.screenUV + float2(offset,0.0)).rgb * weights[i];
    }
    return float4(color,1.0);
}

float4 BloomVerticalPassFragment(Varyings input) : SV_TARGET
{
    float3 color = 0.0;
    float offsets[] = {-3.23076923,-1.38461538,0.0,1.38461538,3.23076923};
    float weights[] =
    {
        0.07027027,0.31621622,0.22702703,0.31621622,0.07027027
    };
    for (int i = 0;i < 5; i ++)
    {
        float offset = offsets[i] * GetSourceTexelSize().y;
        color += GetSource(input.screenUV + float2(0.0,offset)).rgb * weights[i];
    }
    return float4(color,1.0);
}

float4 ColorGradingNonePassFragment(Varyings input) : SV_TARGET
{
    float3 color = GetColorGradedLUT(input.screenUV);
    return float4(color,1.0);
}

float4 ColorGradingReinhardPassFragment(Varyings input) : SV_TARGET
{
    float3 color = GetColorGradedLUT(input.screenUV);
    color /= color +1.0;
    return float4(color,1.0);
}

float4 ColorGradingNeutralPassFragment(Varyings input) : SV_TARGET
{
    float3 color = GetColorGradedLUT(input.screenUV);
    color = NeutralTonemap(color);
    return float4(color,1.0);
}

float4 ColorGradingACESPassFragment(Varyings input) : SV_TARGET
{
    float3 color = GetColorGradedLUT(input.screenUV,true);
    color = AcesTonemap(color);
    return float4(color,1.0);
}

Varyings DefaultPassVertex(uint vertexID : SV_VertexID) 
{
    Varyings output;
    output.positionCS_SS = float4(vertexID <= 1? -1.0 : 3.0,vertexID == 1? 3.0 : -1.0,0.0,1.0);
    output.screenUV = float2(vertexID <= 1 ? 0.0 : 2.0,vertexID == 1 ? 2.0 : 0.0);
    if(_ProjectionParams.x < 0.0)
    {
        output.screenUV.y = 1.0 - output.screenUV.y;
    }
    return output;
}

float4 CopyPassFragment(Varyings input):SV_TARGET
{
    return GetSource(input.screenUV);
}

float3 ApplyColorGradingLUT(float3 color)
{
    return ApplyLut2D(TEXTURE2D_ARGS(_ColorGradingLUT,sampler_linear_clamp),saturate(_ColorGradingLUTInLogC ? LinearToLogC(color):color),_ColorGradingLUTParameters.xyz);
}

float4 ApplyColorGradingPassFragment(Varyings input):SV_TARGET
{
    float4  color = GetSource(input.screenUV);
    color.rgb = ApplyColorGradingLUT(color.rgb);
    return color;
}

float4 ApplyColorGradingWithLumaPassFragment(Varyings input):SV_TARGET
{
    float4 color = GetSource(input.screenUV);
    color.rgb = ApplyColorGradingLUT(color.rgb);
    color.a = sqrt(Luminance(color.rgb));
    return color;
}

float4 FinalRescalePassFragment(Varyings input) :SV_TARGET
{
    if(_CopyBicubic)
    {
        return GetSourceBicubic(input.screenUV);
    }
    else
    {
        return GetSource(input.screenUV);
    }
}

#endif