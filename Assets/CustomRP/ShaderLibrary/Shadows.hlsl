#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/ShadowsInput.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Shadows.Directional.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Shadows.Other.hlsl"

float FadedShadowStrength(float distance,float scale,float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surfaceWS)
{
    ShadowData data;
    data.shadowMask.always = false;
    data.shadowMask.distance = false;
    data.shadowMask.shadows = 1.0;
    data.cascadeBlend = 1.0f;
    data.strength = FadedShadowStrength(surfaceWS.depth,_ShadowDistanceFade.x ,_ShadowDistanceFade.y);
    int i;
    for (i = 0;i< _CascadeCount;i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquared(surfaceWS.position,sphere.xyz);
        if(distanceSqr < sphere.w)
        {
            float fade = FadedShadowStrength(distanceSqr,_CascadeData[i].x, _ShadowDistanceFade.z);
            if(i == _CascadeCount - 1)
            {
                data.strength *= fade;
            }
            else
            {
                data.cascadeBlend = fade;
            }
            break;
        }
    }
    if(_CascadeCount > 0 && i == _CascadeCount)
    {
        data.strength = 0.0;
    }
#if defined(_CASCADE_BLEND_DITHER)
    else if(data.cascadeBlend < surfaceWS.dither)
    {
        i += 1;
    }
#endif

#if !defined(_CASCADE_BLEND_SOFT)
    data.cascadeBlend = 1.0;
#endif
    data.cascadeIndex = i;
    return data;
}


#endif