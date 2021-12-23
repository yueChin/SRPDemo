#ifndef CUSTOM_BLOOM_INPUT_INCLUDED
#define CUSTOM_BLOOM_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Assets/CustomRP/ShaderLibrary/Common.hlsl"

bool _BloomBicubicUpsampling;
float4 _BloomThreshold;
float _BloomIntensity;

float3 ApplyBloomThreshold(float3 color)
{
    float brightness = Max3(color.r,color.g,color.b);
    float soft = brightness + _BloomThreshold.y;
    soft = clamp(soft,0.0, _BloomThreshold.z);
    soft = soft * soft * _BloomThreshold.w;
    float contribution = max(soft,brightness - _BloomThreshold.x);
    contribution /= max(brightness, 0.00001);
    return color * contribution;
}

#endif