Shader "Custom/PostFXStack" 
{
    SubShader 
    {
        Cull Off
        
        ZTest Always
        ZWrite Off
        
        HLSLINCLUDE
        #include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
        #include "Assets/CustomRP/Shaders/PostFX/PostFXPasses.hlsl"
        ENDHLSL
        
        Pass
        {
            Name "Bloom Horizontal"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomHorizontalPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Vertical"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomVerticalPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Add"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomAddPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Scatter"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomScatterPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Scatter Final "
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomScatterFinalPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Prefilter"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomPrefilterPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Prefilter Fireflies"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment BloomPrefilterFirefliesPassFragment
            ENDHLSL
        }
          
        Pass
        {
            Name "Copy"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment CopyPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Tone Mapping None"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingNonePassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Tone Mapping ACES"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingACESPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Tone Mapping Neutral"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingNeutralPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Tone Mapping Reinhard"
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ColorGradingReinhardPassFragment
            ENDHLSL
        }
        
        Pass
        {
            Name "Apply Color Grading"
            Blend [_FinalSrcBlend] [_FinalDstBlend]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ApplyColorGradingPassFragment
            ENDHLSL
        }
        Pass
        {
            Name "Apply Color Grading With Luma"
            
            Blend [_FinalSrcBlend] [_FinalDstBlend]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment ApplyColorGradingWithLumaPassFragment
            ENDHLSL
        }
        Pass
        {
            Name "Final Rescale"
            
            Blend [_FinalSrcBlend] [_FinalDstBlend]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment FinalRescalePassFragment
            ENDHLSL
        }
        Pass
        {
            Name "FXAA"
            
            Blend [_FinalSrcBlend] [_FinalDstBlend]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment FXAAPassFragment
            #pragma multi_compile _ FXAA_QUALITY_MEDIUM FAXX_QULITY_LOW
            #include "Assets/CustomRP/Shaders/PostFX/AA/FXAAPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "FXAA With Luma"
            
            Blend [_FinalSrcBlend] [_FinalDstBlend]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment FXAAPassFragment
            #define FXAA_ALPHA_CONTAINS_LUMA
            #pragma multi_compile _ FXAA_QUALITY_MEDIUM FAXX_QULITY_LOW
            #include "Assets/CustomRP/Shaders/PostFX/AA/FXAAPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "TAA"
            
            Blend [_FinalSrcBlend] [_FinalDstBlend]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment TAAPassFragment
            #include "Assets/CustomRP/Shaders/PostFX/AA/TAAPass.hlsl"
            ENDHLSL
        }
    }
}