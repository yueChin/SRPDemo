#ifndef CUSTOM_SHADOWS_INPUT_OTHER_INCLUDED
#define CUSTOM_SHADOWS_INPUT_OTHER_INCLUDED

#if defined(_OTHER_PCF3)
#define OTHER_FILTER_SAMPLES 4
#define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_OTHER_PCF5)
#define OTHER_FILTER_SAMPLES 9
#define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_OTHER_PCF7)
#define OTHER_FILTER_SAMPLES 16
#define OTHER_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

TEXTURE2D_SHADOW(_OtherShadowAtlas);

static const float3 pointShadowPlanes[6] =
{
    float3(-1.0,0.0,0.0),
    float3(1.0,0.0,0.0),
    float3(0.0,-1.0,0.0),
    float3(0.0,1.0,0.0),
    float3(0.0,0.0,-1.0),
    float3(0.0,0.0,1.0),
};

#define MAX_SHADOWED_OTHER_LIGHT_COUNT 16

struct OtherShadowData
{
    float strength;
    int tileIndex;
    int shadowMaskChannel;
    float3 lightPositionWS;
    float3 spotDirectionWS;
    bool isPoint;
    float3 lightDirectionWS;
};

#endif