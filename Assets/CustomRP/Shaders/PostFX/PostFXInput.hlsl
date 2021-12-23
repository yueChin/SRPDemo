#ifndef CUSTOM_POST_FX_INPUT_INCLUDED
#define CUSTOM_POST_FX_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
#include "Assets/CustomRP/ShaderLibrary/PostFX/Bloom.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_PostFXSource);
TEXTURE2D(_PostFXSource2);

float4 _PostFXSource_TexelSize;

float4 GetSource(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostFXSource,sampler_linear_clamp,screenUV,0);
}

float4 GetSource2(float2 screenUV)
{
    return SAMPLE_TEXTURE2D_LOD(_PostFXSource2,sampler_linear_clamp,screenUV,0);
}

float4 GetSourceTexelSize()
{
    return _PostFXSource_TexelSize;
}

float4 GetSourceBicubic(float2 screenUV)
{
    return SampleTexture2DBicubic(TEXTURE2D_ARGS(_PostFXSource,sampler_linear_clamp),screenUV,_PostFXSource_TexelSize.zwxy,1.0,0.0);
}

#endif