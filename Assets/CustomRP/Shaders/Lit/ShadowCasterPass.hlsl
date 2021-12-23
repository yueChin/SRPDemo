#ifndef CUSTOM_SHADOW_SHADOWCASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_SHADOWCASTER_PASS_INCLUDED

#include "LitInput.hlsl"

struct Attributes {
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float4 positionCS_SS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    float2 detailUV : VAR_DETAIL_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

bool _ShadowPancaking;

Varyings ShadowCasterPassVertex (Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS_SS = TransformWorldToHClip(positionWS);

    if(_ShadowPancaking)
    {
        #if UNITY_REVERSED_Z
        output.positionCS_SS.z = min(output.positionCS_SS.z,output.positionCS_SS.w * UNITY_NEAR_CLIP_VALUE);
        #else
        output.positionCS.z = max(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #endif
    }
    output.baseUV = TransformBaseUV(input.baseUV);
    return output;
}


void ShadowCasterPassFragment (Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    InputConfig cfg = GetInputConfig(input.positionCS_SS,input.baseUV,input.detailUV);
    ClipLOD(cfg.fragment ,unity_LODFade.x);
    float4 base = GetBase(cfg);
    #if defined(_SHADOWS_CLIP)
        clip(base.a - INPUT_PROP(_Cutoff));
    #elif defined(_SHADOWS_DITHER)
        flaot dither = InterleavedGradientNoise(cfg.fragment.positionSS.xy,0)
        clip(base.a - dither);
    #endif
}

#endif