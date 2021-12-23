#ifndef CUSTOM_WHITE_BALANCE_INCLUDED
#define CUSTOM_WHITE_BALANCE_INCLUDED

float4 _WhiteBalance;

float3 ColorGradeWhiteBalance(float3 color)
{
    color = LinearToLMS(color);
    color *= _WhiteBalance.rgb;
    return LMSToLinear(color);
}

#endif