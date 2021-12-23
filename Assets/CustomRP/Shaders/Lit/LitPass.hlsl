#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Surface.hlsl"
#include "Assets/CustomRP/ShaderLibrary/GI.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Lighting.hlsl"
#include "Assets/CustomRP/ShaderLibrary/BRDF.hlsl"
#include "Assets/CustomRP/Shaders/Lit/LitInput.hlsl"

struct Attributes {
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    float3 normalOS :NORMAL;
    float4 tangentOS :TANGENT;
    GI_ATTRIBUTE_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float4 positionCS_SS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
  #if defined(_DETAIL_MAP)
    float2 detailUV : VAR_DETAIL_UV;
  #endif
    float3 normalWS :VAR_NORMAL;
    float3 positionWS :VAR_POSITION;
  #if defined(_NORMAL_MAP)
      float4 tangentWS : VAR_TANGENT;
  #endif
    GI_VARYINGS_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex (Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    TRANSFER_GI_DATA(input,output)
    output.positionWS = TransformObjectToWorld(input.positionOS);
    //float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS_SS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
  #if defined(_NORMAL_MAP)
      output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz),input.tangentOS.w);
  #endif
    
    output.baseUV = TransformBaseUV(input.baseUV);
  #if defined(_DETAIL_MAP)
    output.detailUV = TransformDetailUV(input.baseUV);
  #endif
    return output;
}


float4 LitPassFragment (Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    InputConfig cfg = GetInputConfig(input.positionCS_SS,input.baseUV);
    //return float4(cfg.fragment.depth.xxx / 20.0,1.0);
    ClipLOD(cfg.fragment ,unity_LODFade.x);
  #if defined(_MASK_MAP)
    cfg.useMask = true;
  #endif
  #if defined(_DETAIL_MAP)
    cfg.detailUV = input.detailUV;
    cfg.useDetail = true;
  #endif
  
    float4 base = GetBase(cfg);
  #if defined(_CLIPPING)
    clip(base.a - GetCutoff(input.baseUV);
  #endif

    Surface surface;
    surface.position = input.positionWS;
  #if defined(_NORMAL_MAP)
    surface.normal = NormalTangentToWorld(GetNormalTS(cfg),input.normalWS,input.tangentWS);
    surface.interpolatedNormal = input.normalWS;
  #else
    surface.normal = normalize(input.normalWS);
    surface.interpolatedNormal = surface.normal;
  #endif
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.depth = -TransformWorldToView(input.positionWS).z;
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = GetMetallic(cfg);
    surface.smoothness = GetSmoothness(cfg);
    surface.fresnelStrength = GetFresnel(cfg);
    surface.occlusion = GetOcclusion(cfg);
    surface.dither = InterleavedGradientNoise(cfg.fragment.positionSS,0);
    surface.renderingLayerMask = asuint(unity_RenderingLayer.x);
    
  #if defined(_PREMULTIPLY_ALPHA)
    BRDF brdf = GetBRDF(surface,true);
  #else
    BRDF brdf = GetBRDF(surface);
  #endif
    float2 v = GI_FRAGMENT_DATA(input);
    GI gi = GetGI(v,surface,brdf);
    float3 color = GetLighting(surface,brdf,gi);
    color += GetEmission(cfg);
    return float4(color,GetFinalAlpha(surface.alpha));
}

#endif