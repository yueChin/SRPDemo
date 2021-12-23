﻿#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED

struct Surface
{
    float3 position;
    float3 normal;
    float3 color;
    float alpha;
    float metallic;
    float smoothness;
    float3 viewDirection;
    float depth;
    float dither;
    float fresnelStrength;
    float3 interpolatedNormal;
    uint renderingLayerMask;
    float occlusion;
};



#endif