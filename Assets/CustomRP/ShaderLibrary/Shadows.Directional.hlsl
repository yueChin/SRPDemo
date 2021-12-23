#ifndef CUSTOM_SHADOWS_DIRECTIONAL_INCLUDED
#define CUSTOM_SHADOWS_DIRECTIONAL_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Surface.hlsl"
#include "Assets/CustomRP/ShaderLibrary/ShadowsInput.hlsl"

float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,positionSTS);
}

float FilterDirectionalShadow(float3 positionSTS)
{
#if defined(DIRECTIONAL_FILTER_SETUP)
    float weights[DIRECTIONAL_FILTER_SAMPLES];
    float2 positions[DIRECTIONAL_FILTER_SAMPLES];
    float4 size = _ShadowAtlasSize.yyxx;
    DIRECTIONAL_FILTER_SETUP(size,positionSTS.xy,weights,positions);
    float shadow = 0;
    for (int i = 0; i< DIRECTIONAL_FILTER_SAMPLES;i ++)
    {
        shadow += weights[i] * SampleDirectionalShadowAtlas(float3(positions[i].xy,positionSTS.z));
    }
    return shadow;
#else
    return SampleDirectionalShadowAtlas(positionSTS);
#endif
}

float GetCascadedShadow(DirectionalShadowData directional, ShadowData global, Surface surfaceWS)
{
    float3 normalBias = surfaceWS.interpolatedNormal * (directional.normalBias *  _CascadeData[global.cascadeIndex].y);
    float3 positionSTS = mul(_DirectionalShadowMatrices[directional.tileIndex],float4(surfaceWS.position + normalBias,1.0)).xyz;
    float shadow = FilterDirectionalShadow(positionSTS);

    if(global.cascadeBlend < 1.0)
    {
        normalBias = surfaceWS.interpolatedNormal * (directional.normalBias *  _CascadeData[global.cascadeIndex + 1].y);
        positionSTS = mul(_DirectionalShadowMatrices[directional.tileIndex + 1],float4(surfaceWS.position + normalBias,1.0)).xyz;
        shadow = lerp(FilterDirectionalShadow(positionSTS),shadow,global.cascadeBlend);
    }

    return shadow;
}

float GetDirectionalShadowAttenuation(DirectionalShadowData directional,ShadowData global ,Surface surfaceWS)
{
#if !defined(_RECEIVE_SHADOWS)
    return 1.0f;
#endif
    
    float shadow;
    if(directional.strength * global.strength <= 0.0)
    {
        shadow = GetBakedShadow(global.shadowMask,directional.shadowMaskChannel,abs(directional.strength));
    }
    else
    {
        shadow = GetCascadedShadow(directional,global,surfaceWS);
        shadow = MixBakedAndRealtimeShadows(global,shadow,directional.shadowMaskChannel,directional.strength);
    }
    return shadow;
}

#endif