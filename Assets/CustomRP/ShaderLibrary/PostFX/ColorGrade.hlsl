#ifndef CUSTOM_COLOR_GRADE_INCLUDED
#define CUSTOM_COLOR_GRADE_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Color.hlsl"

float4 _ColorAdjustments;
float4 _ColorFilter;

float3 ColorGradePostExposure(float3 color)
{
    return color * _ColorAdjustments.x;
}

float3 ColorGradingContrast(float3 color,bool useACES)
{
    color = useACES ? ACES_to_ACEScc(unity_to_ACES(color)) :  LinearToLogC(color);
    color = (color - ACEScc_MIDGRAY) * _ColorAdjustments.y + ACEScc_MIDGRAY;
    return useACES ? ACES_to_ACEScg(ACEScc_to_ACES(color)) : LogCToLinear(color);
}

float3 ColorGradingFilter(float3 color)
{
    return color * _ColorFilter.rgb;
}

float3 ColorGradingHueShift(float3 color)
{
    color = RgbToHsv(color);
    float hue = color.x + _ColorAdjustments.z;
    color.x = RotateHue(hue,0.0,1.0);
    return HsvToRgb(color);
}

float3 ColorGradingSaturation(float3 color,bool useACES)
{
    float luminace = Luminance(color,useACES);
    return (color - luminace) * _ColorAdjustments.w + luminace;
}

#endif