#ifndef CUSTOM_SHADOWS_OTHER_INCLUDED
#define CUSTOM_SHADOWS_OTHER_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Surface.hlsl"
#include "Assets/CustomRP/ShaderLibrary/ShadowsInput.hlsl"

float SampleOtherShadowAtlas(float3 positionSTS ,float3 bounds)
{
    positionSTS.xy = clamp(positionSTS.xy,bounds.xy ,bounds.xy + bounds.z);
    return SAMPLE_TEXTURE2D_SHADOW(_OtherShadowAtlas,SHADOW_SAMPLER,positionSTS);
}

float FilterOtherShadow(float3 positionSTS,float3 bounds)
{
    #if defined(OTHER_FILTER_SAMPLES)
    real weights[OTHER_FILTER_SAMPLER];
    real2 positions[OTHER_FILTER_SAMPLER];
    float4 size = _ShadowAtlasSize.wwzz;
    OTHER_FILTER_SETUP(size,positionSTS.xy,weights,position);
    float shadow = 0;
    for (int i = 0; i < OTHER_FILTER_SAMPLES; i++)
    {
        shadow += weights[i] * SampleOtherShadowAtlas(float3(positions[i].xy,positionSTS.z),bounds);
    }
    return shadow;
    #else
    return SampleOtherShadowAtlas(positionSTS,bounds);
    #endif
}

float GetOtherShadow(OtherShadowData other,ShadowData global,Surface surfaceWS)
{
    float tileIndex = other.tileIndex;
    float3 lightPlane = other.spotDirectionWS;
    if(other.isPoint)
    {
        float faceOffset = CubeMapFaceID(-other.lightDirectionWS);
        tileIndex += faceOffset;
        lightPlane = pointShadowPlanes[faceOffset];
    }
    float4 tileData = _OtherShadowTiles[tileIndex];
    float3 surfaceToLight = other.lightPositionWS - surfaceWS.position;
    float distanceToLightPlane = dot(surfaceToLight,lightPlane);
    float3 normalBias = surfaceWS.interpolatedNormal * (distanceToLightPlane * tileData.w);
    float4 positionSTS = mul(_OtherShadowMatrices[tileIndex],float4(surfaceWS.position + normalBias,1.0));
    return FilterOtherShadow(positionSTS.xyz / positionSTS.w,tileData.xyz);
}


float GetOtherShadowAttenuation(OtherShadowData other,ShadowData global,Surface surfaceWS)
{
    #if !defined(_RECEIVE_SHADOWS)
    return 1.0;
    #endif

    float shadow;
    if(other.strength * global.strength <= 0.0)
    {
        shadow = GetBakedShadow(global.shadowMask,other.shadowMaskChannel,abs(other.strength));
    }
    else
    {
        shadow = GetOtherShadow(other,global,surfaceWS);
        shadow = MixBakedAndRealtimeShadows(global,shadow,other.shadowMaskChannel,other.strength);
    }
    return shadow;
}


#endif