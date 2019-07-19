Shader "My SRP/Unlit"
{
    Properties
    {
		[HDR]_Color ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex Unlit_Pas_Vertex
			#pragma fragment Unlit_Pas_Fragment
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			#include "/ShaderLibrary/_MySRP_Unlit.hlsl"

			ENDHLSL
        }
    }
}
