﻿//不受光着色器公用属性和方法库
#ifndef CUSTOM_UNLIT_INPUT_INCLUDED
#define CUSTOM_UNLIT_INPUT_INCLUDED

#include "Assets/CustomRP/ShaderLibrary/Common.hlsl"

TEXTURE2D(_BaseMap);
TEXTURE2D(_DistortionMap);
SAMPLER(sampler_BaseMap);
SAMPLER(sampler_DistortionMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)

UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_DEFINE_INSTANCED_PROP(float, _ZWrite)
UNITY_DEFINE_INSTANCED_PROP(float, _NearFadeDistance)
UNITY_DEFINE_INSTANCED_PROP(float, _NearFadeRange)
UNITY_DEFINE_INSTANCED_PROP(float, _SoftParticlesDistance)
UNITY_DEFINE_INSTANCED_PROP(float, _SoftParticlesRange)
UNITY_DEFINE_INSTANCED_PROP(float, _DistortionStrength)
UNITY_DEFINE_INSTANCED_PROP(float, _DistortionBlend)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
#define INPUT_PROP(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)
//输入配置
struct InputConfig {
	Fragment fragment;
	float2 baseUV;
	float4 color;
	float2 detailUV;
	float3 flipbookUVB;
	bool flipbookBlending;	
	bool nearFade;
	bool softParticles;
};
//获取输入配置
InputConfig GetInputConfig(float4 positionSS,float2 baseUV, float2 detailUV = 0.0) {
	InputConfig c;
	c.fragment = GetFragment(positionSS);
	c.color = 1.0;
	c.baseUV = baseUV;
	c.detailUV = detailUV;
	c.flipbookUVB = 0.0;
	c.flipbookBlending = false;
	c.nearFade = false;
	c.softParticles = false;
	return c;
}
//基础纹理UV转换
float2 TransformBaseUV(float2 baseUV) {
	float4 baseST = INPUT_PROP(_BaseMap_ST);
	return baseUV * baseST.xy + baseST.zw;
}
//获取基础纹理的采样数据
float4 GetBase(InputConfig c) {
	float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, c.baseUV);
	if (c.flipbookBlending) {
		baseMap = lerp(baseMap, SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, c.flipbookUVB.xy),c.flipbookUVB.z);
	}
	if (c.nearFade) {
		float nearAttenuation = (c.fragment.depth - INPUT_PROP(_NearFadeDistance)) / INPUT_PROP(_NearFadeRange);
		baseMap.a *= saturate(nearAttenuation);
	}

	if (c.softParticles) {
		float depthDelta = c.fragment.bufferDepth - c.fragment.depth;
		float nearAttenuation = (depthDelta - INPUT_PROP(_SoftParticlesDistance)) /
			INPUT_PROP(_SoftParticlesRange);
		baseMap.a *= saturate(nearAttenuation);
	}
	float4 baseColor = INPUT_PROP(_BaseColor);
	return baseMap * baseColor* c.color;
}
//获取扰动纹理采样数据
float2 GetDistortion(InputConfig c) {
	float4 rawMap = SAMPLE_TEXTURE2D(_DistortionMap, sampler_DistortionMap, c.baseUV);
	if (c.flipbookBlending) {
		rawMap = lerp(rawMap, SAMPLE_TEXTURE2D(_DistortionMap, sampler_DistortionMap, c.flipbookUVB.xy), c.flipbookUVB.z);
	}
	return DecodeNormal(rawMap, INPUT_PROP(_DistortionStrength)).xy;
}

float GetDistortionBlend(InputConfig c) {
	return INPUT_PROP(_DistortionBlend);
}
float GetCutoff(InputConfig c) {
	return INPUT_PROP(_Cutoff);
}

float GetMetallic(InputConfig c) {
	return 0;
}

float GetSmoothness(InputConfig c) {
	return 0;
}
float3 GetEmission (InputConfig c) {
	return GetBase(c).rgb;
}
float GetFresnel (InputConfig c) {
	return 0.0;
}



float GetFinalAlpha(float alpha) {
	return INPUT_PROP(_ZWrite) ? 1.0 : alpha;
}
#endif
