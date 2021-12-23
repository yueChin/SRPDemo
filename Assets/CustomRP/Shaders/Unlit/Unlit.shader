Shader "CustomRP/Unlit"
{
    Properties
    {
        _BaseMap("Texture",2D) = "white" {}
        [HDR] _BaseColor("Color",Color) = (1.0,1.0,0.0,1.0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend",Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",Float) = 1
        _Cutoff("Alpha CutOff",Range(0.0,1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping("Alpha Clipping",Float) = 0
        [Toggle(_VERTEX_COLORS)] _VertexColors ("Vertex Colors",float) = 0
        
        [Toggle(_MASK_MAP)] _MaskMapToggle("Mask Map",Float) = 1
        [NoScaleOffset]_MaskMap("Mask (MODS)",2D) = "white" {}
        
        [Toggle(_DETAIL_MAP)] _DetailMapToggle("Normal Map",Float) = 1
        [NoScaleOffset]_DetailNormalMap("Detail Normals",2D) = "bump" {}
        _DetailNormalScale("Detail Normal Scale",Range(0,1)) = 1
        _DetailMap("Details",2D) = "linearGray" {}
        _DetailAlbedo("Detail Albedo",Range(0,1)) = 1
        _DetailSmoothness("Detail Smoothness",Range(0,1)) = 1
    }
    SubShader
    {
        Pass
        {
            Blend[_FinalSrcBlend][_FinalDstBlend], One OneMinusSrcAlpha
            ZWrite[_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _MASK_MAP
            #pragma shader_feature _DETAIL_MAP
            #pragma shader_feature _VERTEX_COLORS
            
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Assets/CustomRP/ShaderLibrary/Common.hlsl"
            #include "UnlitInput.hlsl"
            #include "UnlitPass.hlsl"
           
            ENDHLSL
        }
    }
    
     CustomEditor "CustomShaderGUI"
}
