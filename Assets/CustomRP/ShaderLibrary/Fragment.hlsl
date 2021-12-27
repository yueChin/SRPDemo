#ifndef CUSTOM_FRAGMENT_INCLUDED
#define CUSTOM_FRAGMENT_INCLUDED

TEXTURE2D(_CameraColorTexture);

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

TEXTURE2D(_CameraMotionVectorsTexture);
SAMPLER(sampler_CameraMotionVectorsTexture);
float4 _CameraMotionVectorsTexture_TexelSize;

float4x4 _NonJitterVP;
float4x4 _InvNonJitterVP;
float4x4 _LastVp;
float4x4 _InvLastVp;

float4 _CameraBufferSize;

struct Fragment
{
    float2 positionSS;
    float2 screenUV;
    float depth;
    float bufferDepth;
};

Fragment GetFragment(float4 positionSS)
{
    Fragment f;
    f.positionSS = positionSS.xy;
    f.screenUV = f.positionSS * _CameraBufferSize.xy;
    f.depth = IsOrthgraphicCamera() ? OrthgraphicDepthBufferToLinear(positionSS.z) :  positionSS.w;
    f.bufferDepth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture,sampler_linear_clamp,f.screenUV,0);
    f.bufferDepth = IsOrthgraphicCamera() ? OrthgraphicDepthBufferToLinear(f.bufferDepth) : LinearEyeDepth(f.bufferDepth,_ZBufferParams);
    return f;
}

float4 GetBufferColor(Fragment fragment,float2 uvOffset = float2(0.0,0.0))
{
    float2 uv = fragment.screenUV + uvOffset;
    return SAMPLE_TEXTURE2D_LOD(_CameraColorTexture,sampler_CameraColorTexture,uv,0);
}

#endif