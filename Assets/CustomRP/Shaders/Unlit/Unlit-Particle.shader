Shader "CustomRP/Particle/Unlit"
{
    Properties
    {
        _BaseMap("Texture",2D) = "white" {}
        [HDR] _BaseColor("Color",Color) = (1.0,1.0,0.0,1.0)
    
        _Cutoff("Alpha CutOff",Range(0.0,1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping",Float) = 0
        [Toggle(_VERTEX_COLORS)] _VertexColors ("Vertex Colors", Float) = 0
        
        [Toggle(_FLIPBOOK_BLENDING)]_FlipbookBlending("Flipbook Blending",Float) = 1
        
        [Toggle(_NEAR_FADE)] _NearFade("Near Fade",float) = 0
        _NearFadeDistance ("Near Fade Distance",Range(0.0,10.0)) = 1
        _NearFadeRange("Near Fade Range",Range(0.01,10.0)) = 1
        
        [Toggle(_SOFT_PARTICLES)] _SoftParticles("Soft Particle",float) = 0
        _SoftParticleDistance("Soft Particle Distance",Range(0.0,10.0)) = 0
        _SoftParticleRange("Soft Particle Range",Range(0.01,10.0)) = 1
        
        [Toggle(_DISTORTION)] _Distortion("Distortion",float) = 0
        [NoScaleOffset] _DistortionMap("Distortion Vectors",2D) = "bumb"{}
        _DistortionStrength("Distortion Strength",Range(0.0,0.2)) = 0.1
        _DistortionBlending("Distortion Blending",Range(0.0,1.0)) = 1
    	
    	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
    }
    SubShader
    {
        Pass
        {
            Blend[_SrcBlend][_DstBlend], One OneMinusSrcAlpha
            ZWrite[_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _VERTEX_COLORS
            #pragma shader_feature _FLIPBOOK_BLENDING
            #pragma shader_feature _NEAR_FADE
            #pragma shader_feature _SOFT_PARTICLES
            #pragma shader_feature _DISTORTION

            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
            #include "UnlitInput.hlsl"
            #include "UnlitPass.hlsl"
           
            ENDHLSL
        }
        Pass
        {
		   Tags {
				"LightMode" = "ShadowCaster"
			}
		    ColorMask 0

            HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
            #include "Assets/CustomRP/Shaders/Lit/ShadowCasterPass.hlsl"
			ENDHLSL
        }
    }
    
     CustomEditor "CustomShaderGUI"
}
