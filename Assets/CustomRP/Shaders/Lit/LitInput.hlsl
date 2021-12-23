#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
#include "Assets/CustomRP/ShaderLibrary/TextureInput.hlsl"


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURE2D(_DetailMap);
SAMPLER(sampler_DetailMap);

TEXTURE2D(_EmissionMap);

TEXTURE2D(_MaskMap);

TEXTURE2D(_NormalMap);
TEXTURE2D(_DetailNormalMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)

UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)

UNITY_DEFINE_INSTANCED_PROP(float4, _EmissionColor)

UNITY_DEFINE_INSTANCED_PROP(float4,_DetailMap_ST)
UNITY_DEFINE_INSTANCED_PROP(float,_DetailAlbedo)
UNITY_DEFINE_INSTANCED_PROP(float,_DetailSmoothness)

UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
UNITY_DEFINE_INSTANCED_PROP(float,_Fresnel)
UNITY_DEFINE_INSTANCED_PROP(float,_ZWrite)
UNITY_DEFINE_INSTANCED_PROP(float,_Occlusion)

UNITY_DEFINE_INSTANCED_PROP(float,_NormalScale)
UNITY_DEFINE_INSTANCED_PROP(float,_DetailNormalScale)

UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct InputConfig
{
    Fragment fragment;
    float2 baseUV;
    float2 detailUV;
    bool useMask;
    bool useDetail;
};

InputConfig GetInputConfig(float4 positionSS,float2 baseUV, float2 detailUV = 0.0)
{
    InputConfig inputConfig;
    inputConfig.fragment = GetFragment(positionSS);
    inputConfig.baseUV = baseUV;
    inputConfig.detailUV = detailUV;
    inputConfig.useMask = false;
    inputConfig.useDetail = false;
    return inputConfig;
}

float2 TransformBaseUV(float2 baseUV)
{
    float4 baseST = INPUT_PROP(_BaseMap_ST);
    return baseUV * baseST.xy + baseST.zw;
}

float2 TransformDetailUV(float2 detailUV)
{
    float4 detailST = INPUT_PROP(_DetailMap_ST);
    return detailUV * detailST.xy + detailST.zw;
}

float3 GetNormalTS(InputConfig cfg)
{
    float4 map = SAMPLE_TEXTURE2D(_NormalMap,sampler_BaseMap,cfg.baseUV);
    float scale = INPUT_PROP(_NormalScale);
    float3 normal = DecodeNormal(map,scale);

    if(cfg.useDetail)
    {
        map = SAMPLE_TEXTURE2D(_DetailNormalMap,sampler_BaseMap,cfg.detailUV);
        scale = INPUT_PROP(_DetailNormalScale);
        float3 detail = DecodeNormal(map,scale);
        normal = BlendNormalRNM(normal,detail);
    }
    return normal;
}


float4 GetDetail(InputConfig cfg)
{
    if(cfg.useDetail)
    {
        float4 map = SAMPLE_TEXTURE2D(_DetailMap,sampler_DetailMap,cfg.detailUV);
        return map * 2.0 - 1.0;
    }
    return 0.0;
}

float4 GetMask(InputConfig cfg)
{
    if(cfg.useMask)
    {
        return SAMPLE_TEXTURE2D(_MaskMap,sampler_BaseMap,cfg.baseUV);
    }
    return 1.0;
}

float4 GetBase(InputConfig cfg)
{
    float4 map = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,cfg.baseUV);
    float4 color = INPUT_PROP(_BaseColor);
    if(cfg.useDetail)
    {
        float4 detail = GetDetail(cfg).r * INPUT_PROP(_DetailAlbedo);
        float mask = GetMask(cfg).b;
        map.rgb = lerp(sqrt(map.rgb),detail < 0.0 ? 0.0 : 1.0 ,abs(detail) * mask);
        map.rgb *= map.rgb;
    }
    return map * color;
}

float GetCutoff(InputConfig cfg)
{
    return INPUT_PROP(_Cutoff);
}

float GetOcclusion(InputConfig cfg)
{
    float strength =  INPUT_PROP(_Occlusion);
    float occlusion = GetMask(cfg).g;
    occlusion = lerp(occlusion,1.0,strength);
    return occlusion;
}

float GetMetallic(InputConfig cfg)
{
    float metallic = INPUT_PROP(_Metallic);
    metallic *= GetMask(cfg).r;
    return metallic;
}

float GetSmoothness(InputConfig cfg)
{
    float smoothness = INPUT_PROP(_Smoothness);
    if(cfg.useDetail)
    {
        float detail = GetDetail(cfg).b * INPUT_PROP(_DetailSmoothness);
        float mask = GetMask(cfg).b;
        smoothness = lerp(smoothness,detail < 0.0 ? 0.0: 1.0 ,abs(detail) * mask);
    }
    return smoothness;
}

float GetFresnel(InputConfig cfg)
{
    return INPUT_PROP(_Fresnel);
}

float3 GetEmission(InputConfig cfg)
{
    float4 map = SAMPLE_TEXTURE2D(_EmissionMap,sampler_BaseMap,cfg.baseUV);
    float4 color = INPUT_PROP(_EmissionColor);
    return map * color;
}

float GetFinalAlpha(float alpha)
{
    return INPUT_PROP(_ZWrite) ? 1.0 :alpha;
}

#endif