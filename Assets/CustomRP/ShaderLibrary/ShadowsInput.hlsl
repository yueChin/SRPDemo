#ifndef CUSTOM_SHADOWS_INPUT_INCLUDED
#define CUSTOM_SHADOWS_INPUT_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Assets/CustomRP/ShaderLibrary/ShadowsInputDirectional.hlsl"
#include "Assets/CustomRP/ShaderLibrary/ShadowsInputOther.hlsl"


#define MAX_CASCADE_COUNT 4

#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
int _CascadeCount;
float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
float4 _CascadeData[MAX_CASCADE_COUNT];
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
//float _ShadowDistance;
float4 _ShadowDistanceFade;
float4 _ShadowAtlasSize;
float4x4 _OtherShadowMatrices[MAX_SHADOWED_OTHER_LIGHT_COUNT];
float4 _OtherShadowTiles[MAX_SHADOWED_OTHER_LIGHT_COUNT];
CBUFFER_END


struct ShadowMask
{
    bool always;
    bool distance;
    float4 shadows;
};

struct ShadowData
{
    int cascadeIndex;
    float strength;
    float cascadeBlend;
    ShadowMask shadowMask;
};

float GetBakedShadow(ShadowMask mask,int channel)
{
    float shadow = 1.0;
    if(mask.always || mask.distance)
    {
        if(channel >= 0)
        {
            shadow = mask.shadows[channel];
        }
    }
    return shadow;
}

float GetBakedShadow(ShadowMask mask,int channel,float strength)
{
    if(mask.always || mask.distance)
    {
        return lerp(1.0,GetBakedShadow(mask,channel),strength);
    }
    return 1.0;
}

float MixBakedAndRealtimeShadows(ShadowData globa,float shadow,int shadowMaskChannel,float strength)
{
    float baked = GetBakedShadow(globa.shadowMask,shadowMaskChannel);
    if(globa.shadowMask.always)
    {
        shadow = lerp(1.0,shadow,globa.strength);
        shadow = min(baked,shadow);
        return lerp(1.0,shadow,strength);
    }
    
    if(globa.shadowMask.distance)
    {
        shadow = lerp(baked,shadow,globa.strength);
        return lerp(1.0,shadow,strength);
    }
    return lerp(1.0,shadow,strength * globa.strength);
}

#endif