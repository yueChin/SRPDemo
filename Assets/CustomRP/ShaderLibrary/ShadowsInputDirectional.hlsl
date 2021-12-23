#ifndef CUSTOM_SHADOWS_INPUT_DIRECTIONAL_INCLUDED
#define CUSTOM_SHADOWS_INPUT_DIRECTIONAL_INCLUDED

#if defined(_DIRECTIONAL_PCF3)
    #define DIRECTIONAL_FILTER_SAMPLES 4
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
    #define DIRECTIONAL_FILTER_SAMPLES 9
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
    #define DIRECTIONAL_FILTER_SAMPLES 16
    #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
    float normalBias;
    int shadowMaskChannel;
};

#endif