#ifndef CUSTOM_COLOR_INCLUDED
#define CUSTOM_COLOR_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float3 Luminance(float3 color ,bool useACES)
{
    return useACES ? AcesLuminance(color) : Luminance(color);
}



#endif