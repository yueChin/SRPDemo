#ifndef CUSTOM_CHANNEL_MIXER_INCLUDED
#define CUSTOM_CHANNEL_MIXER_INCLUDED

float4 _ChannelMixerRed;
float4 _ChannelMixerGreen;
float4 _ChannelMixerBlue;

float3 ColorGradingChannelMixer(float3 color)
{
    return mul(float3x3(_ChannelMixerRed.rgb,_ChannelMixerGreen.rgb,_ChannelMixerBlue.rgb),color);
}

#endif