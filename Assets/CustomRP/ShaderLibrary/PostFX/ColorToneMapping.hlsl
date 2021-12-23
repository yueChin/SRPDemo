#ifndef CUSTOM_TONE_MAPPING_INCLUDED
#define CUSTOM_TONE_MAPPING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float4 ToneMappingReinhard(float4 color) : SV_TARGET
{
    color.rgb /= color.rgb + 1.0;
    return color;
}

float4 ToneMappingNeutral(float4 color) : SV_TARGET
{
    color.rgb = NeutralTonemap(color.rgb);
    return color;
}

float4 ToneMappingACES(float4 color) : SV_TARGET
{
    color.rgb = AcesTonemap(unity_to_ACES(color.rgb));
    return color;
}
#endif