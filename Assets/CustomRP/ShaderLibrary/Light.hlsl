#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Shadows.hlsl"

struct Light
{
    float3 color;
    float3 direction;
    float attenuation;
    uint renderingLayerMask;
};

#define MAX_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_OTHER_LIGHT_COUNT 64

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    //float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirectionsAndMask[MAX_DIRECTIONAL_LIGHT_COUNT];
    // float3 _DirectionalLightColor;
    // float3 _DirectionalLightDirection;
    int _OtherLightCount;
    float4 _OtherLightColors[MAX_OTHER_LIGHT_COUNT];
    float4 _OtherLightPositions[MAX_OTHER_LIGHT_COUNT];
    //float4 _OtherLightDirections[MAX_OTHER_LIGHT_COUNT];
    float4 _OtherLightSpotAngles[MAX_OTHER_LIGHT_COUNT];
    float4 _OtherLightShadowData[MAX_OTHER_LIGHT_COUNT];
    float4 _OtherLightDirectionsAndMask[MAX_OTHER_LIGHT_COUNT];
CBUFFER_END

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

int GetOtherLightCount()
{
    return _OtherLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex,ShadowData shadowData)
{
    DirectionalShadowData data;
    //data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
    data.strength = _DirectionalLightShadowData[lightIndex].x;
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
    data.normalBias = _DirectionalLightShadowData[lightIndex].z;
    data.shadowMaskChannel = _DirectionalLightShadowData[lightIndex].w;
    return data;
}

Light GetDirectionalLight(int index,Surface surfaceWS,ShadowData shadowData)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirectionsAndMask[index].xyz;
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(index,shadowData);
    light.attenuation = GetDirectionalShadowAttenuation(dirShadowData,shadowData,surfaceWS);
    light.renderingLayerMask = asuint(_DirectionalLightDirectionsAndMask[index].w);
    return light;
}

OtherShadowData GetOtherShadowData(int lightIndex)
{
    OtherShadowData data;
    data.strength = _OtherLightShadowData[lightIndex].x;
    data.tileIndex = _OtherLightShadowData[lightIndex].y;
    data.shadowMaskChannel = _OtherLightShadowData[lightIndex].w;
    data.lightPositionWS = 0.0;
    data.spotDirectionWS = 0.0;
    data.isPoint = _OtherLightShadowData[lightIndex].z == 1.0;
    data.lightDirectionWS = 0.0;
    return data;
}

Light GetOtherLight(int index,Surface surfaceWS,ShadowData shadowData)
{
    Light light;
    light.color = _OtherLightColors[index].rgb;
    float3 position = _OtherLightPositions[index].xyz;
    float3 ray = position - surfaceWS.position;
    
    light.direction = normalize(ray);
    float distanceSqr = max(dot(ray,ray),0.00001);
    float rangeAttenuation = Square(saturate(1.0 - Square(distanceSqr * _OtherLightPositions[index].w)));
    float4 spotAngles = _OtherLightSpotAngles[index];
    float3 spotDirection = _DirectionalLightDirectionsAndMask[index].xyz;
    float4 spotAttenuation = Square(saturate(dot(_DirectionalLightDirectionsAndMask[index].xyz,light.direction) * spotAngles.x + spotAngles.y));
    OtherShadowData otherShadowData = GetOtherShadowData(index);
    otherShadowData.lightPositionWS = position;
    otherShadowData.lightDirectionWS = light.direction;
    otherShadowData.spotDirectionWS = spotDirection;
    light.attenuation = GetOtherShadowAttenuation(otherShadowData,shadowData,surfaceWS) * spotAttenuation * rangeAttenuation  / distanceSqr;
    light.renderingLayerMask = asuint(_OtherLightDirectionsAndMask[index].w);
    return light;
}

#endif