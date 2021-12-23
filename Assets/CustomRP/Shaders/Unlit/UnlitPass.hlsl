#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "Assets/CustomRP/Shaders/Unlit/UnlitInput.hlsl"

struct Attributes
{
    float3 positionOS : POSITION;
    float4 color : COLOR;
#if defined(_FLIPBOOK_BLENDING)
    float4 baseUV : TEXCOORD0;
    float flipbookBlend :TEXCOORD1;
#else
    float2 baseUV : TEXCOORD0;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS_SS : SV_POSITION;
#if defined (_VERTEX_COLORS)
    float4 color : VAR_COLOR;
#endif
    float2 baseUV : VAR_BASE_UV;
#if defined(_FLIPBOOK_BLENDING)
    float3 flipbookUVB : VAR_FLIPBOOK;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex (Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS_SS = TransformWorldToHClip(positionWS);
#if defined(_VERTEX_COLORS)
    output.color = input.color;
#endif
    output.baseUV.xy = TransformBaseUV(input.baseUV.xy);
#if defined(_FLIPBOOK_BLENDING)
    output.flipbookUVB.xy = TransformBaseUV(input.baseUV.zw);
    output.flipbookUVB.z = input.flipbookBlend;
#endif
    return output;
}

float4 UnlitPassFragment (Varyings input) : SV_TARGET {
    UNITY_SETUP_INSTANCE_ID(input);
    InputConfig cfg = GetInputConfig(input.positionCS_SS,input.baseUV);
    //return float4(cfg.fragment.depth.xxx / 20.0,1.0);
#if defined(_VERTEX_COLORS)
    cfg.color = input.color;
#endif
    
#if defined(_FLIBOOK_BLENDING)
    cfg.flipbookUVB = input.flipbookUVB;
    cfg.flipbookBlending = true;
#endif
    
#if defined(_NEAR_FADE)
    cfg.nearFade = true;
#endif
    
#if defined(_SOFT_PARTICLES)
    cfg.softParticles = true;
#endif
    
    float4 base = GetBase(cfg);
#if defined(_CLIPPING)
    clip(base.a - INPUT_PROP(_Cutoff));
#endif
    
#if defined(_DISTORTION)
    float2 distortion =GetDistortion(cfg) * base.a;
    base.rgb = lerp(GetBufferColor(cfg.fragment,distortion).rgb,base.rgb,saturate(base.a - GetDistortionBlend(cfg)));
#endif
    float3 color = base.rgb;
    //color += GetEmission(cfg);

    return float4(color,GetFinalAlpha(base.a));
}

#endif