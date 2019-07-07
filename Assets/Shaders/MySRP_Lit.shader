Shader "My SRP/Lit"
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
			#pragma vertex Lit_Pas_Vertex
			#pragma fragment Lit_Pas_Fragment
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			#include "/ShaderLibrary/_MySRP_Lit.hlsl"

			ENDHLSL
        }
    }
}
