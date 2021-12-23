#ifndef CUSTOM_SPLIT_TONING_INCLUDED
#define CUSTOM_SPLIT_TONING_INCLUDED


float4 _SplitToningShadows;
float4 _SplitToningHighlights;

float3 ColorGradeSplitToning(float3 color)
{
    color = PositivePow(color,1.0 / 2.2);
    float t = saturate(Luminance(saturate(color)) + _SplitToningShadows.w);
    float3 shadows = lerp(0.5,_SplitToningShadows.rgb,1.0 -t);
    float highlights = lerp(0.5,_SplitToningHighlights.rgb,t);
    color = SoftLight(color,shadows);
    color = SoftLight(color,highlights);
    return PositivePow(color,2.2);
}

#endif