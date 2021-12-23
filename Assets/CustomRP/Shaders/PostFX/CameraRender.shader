Shader "Hidden/Custom/CameraRender" 
{
    SubShader 
    {
        Cull Off
        
        ZTest Always
        ZWrite Off
        
        HLSLINCLUDE
        #include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
        #include "Assets/CustomRP/Shaders/PostFX/CameraRenderPasses.hlsl"
        ENDHLSL
        
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
            Name "Copy Depath"
            ColorMask 0
            ZWrite On
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DefaultPassVertex
            #pragma fragment CopyDepthPassFragment
            ENDHLSL
        }
    }
}