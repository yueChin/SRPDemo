#ifndef CUSTOM_CAMERA_RENDER_PASSES_INCLUDED
#define CUSTOM_CAMERA_RENDER_PASSES_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Fragment.hlsl"

TEXTURE2D(_SourceTexture);

struct Varyings
{
    float4 positionCS :SV_POSITION;
    float2 screenUV :VAR_SCRENN_UV;
};

Varyings DefaultPassVertex(uint vertexID : SV_VertexID) 
{
    Varyings output;
    output.positionCS = float4(vertexID <= 1? -1.0 : 3.0,vertexID == 1? 3.0 : -1.0,0.0,1.0);
    output.screenUV = float2(vertexID <= 1 ? 0.0 : 2.0,vertexID == 1 ? 2.0 : 0.0);
    if(_ProjectionParams.x < 0.0)
    {
        output.screenUV.y = 1.0 - output.screenUV.y;
    }
    return output;
}

float4 CopyPassFragment(Varyings input) :SV_TARGET
{
    return SAMPLE_TEXTURE2D_LOD(_SourceTexture,sampler_linear_clamp,input.screenUV,0);
}

float4 CopyDepthPassFragment(Varyings input) :SV_TARGET
{
    return SAMPLE_DEPTH_TEXTURE_LOD(_SourceTexture,sampler_linear_clamp,input.screenUV,0);
}

float2 CopyMotionVectorPassFragment(Varyings input) :SV_TARGET
{
    float depth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture,sampler_CameraDepthTexture,input.screenUV,0);
    float4 worldPos = mul(_InvNonJitterVP, float4(input.screenUV * 2 - 1, depth, 1));
    float4 lastClip = mul(_LastVp, worldPos);
    float2 uv = lastClip.xy / lastClip.w;
    uv = uv * 0.5 + 0.5;
    return  input.screenUV - uv;
}
#endif