#ifndef CUSTOM_META_PASS_INCLUDED
#define CUSTOM_META_PASS_INCLUDED

#include "LitInput.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Surface.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Shadows.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Light.hlsl"
#include "Assets/CustomRP/ShaderLibrary/BRDF.hlsl"

bool4 unity_MetaFragmentControl;
float unity_OneOverOutputBoost;
float unity_MaxOutputValue;


struct Attributes {
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    float2 lightMapUV :TEXCOORD1;
};

struct Varyings {
    float4 positionCS_SS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    float2 detailUV : VAR_DETAIL_UV;
};

Varyings MetaPassVertex (Attributes input)
{
    Varyings output;
    input.positionOS.xy = input.lightMapUV * unity_LightmapST.xy + unity_LightmapST.zw;
    input.positionOS.z = input.positionOS.z > 0 ? FLT_MIN : 0.0;
    output.positionCS_SS = TransformWorldToHClip(input.positionOS);
    output.baseUV = TransformBaseUV(input.baseUV);
    return output;
}


float4 MetaPassFragment (Varyings input) : SV_TARGET
{
    InputConfig cfg = GetInputConfig(input.positionCS_SS,input.baseUV,input.detailUV);
    float4 base = GetBase(cfg);
    Surface surface;
    ZERO_INITIALIZE(Surface,surface);
    surface.color = base.rgb;
    surface.metallic = GetMetallic(cfg);
    surface.smoothness = GetSmoothness(cfg);
    BRDF brdf = GetBRDF(surface);
    float4 meta = 0.0;
    if(unity_MetaFragmentControl.x)
    {
        meta = float4(brdf.diffuse,1.0);
        meta.rgb += brdf.specular * brdf.roughness * 0.5;
        meta.rgb = min(PositivePow(meta.rgb,unity_OneOverOutputBoost),unity_MaxOutputValue);
    }
    else if(unity_MetaFragmentControl.y)
    {
        meta = float4(GetEmission(cfg),1.0);
    }
    return meta;
}

#endif