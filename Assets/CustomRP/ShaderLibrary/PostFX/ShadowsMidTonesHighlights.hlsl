#ifndef CUSTOM_SHADOWS_MIDTONES_HIGHLIGHT_INCLUDED
#define CUSTOM_SHADOWS_MIDTONES_HIGHLIGHT_INCLUDED

#include "Assets/CustomRP//ShaderLibrary/Color.hlsl"


float4 _SMHShadows;
float4 _SMHMidtones;
float4 _SMHHighlights;
float4 _SMHRange;

float3 ColorGradingShadowsMidtonesHighlights(float3 color,bool useACES)
{
    float luminance = Luminance(color,useACES);
    float shadowsWeight = 1.0 - smoothstep(_SMHRange.x,_SMHRange.y,luminance);
    float highlightsWeight = 1.0 - smoothstep(_SMHRange.z,_SMHRange.w,luminance);
    float midtonesWeight = 1.0 - shadowsWeight - highlightsWeight;
    return color * _SMHShadows.rgb * shadowsWeight + color * _SMHMidtones * midtonesWeight + color * _SMHHighlights * highlightsWeight;
}

#endif