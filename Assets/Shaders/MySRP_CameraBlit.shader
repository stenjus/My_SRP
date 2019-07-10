Shader "Hidden/CameraBlit"
{
    Properties
    {
		_MainTex("Render Texture", 2D) = "white" {}
		[HDR]_VignColor("Vign Color", Color) = (0,0,0,0)
		_Vign("vign", float) = 1
		_Vign2("vign", float) = 1
		_FishEye("FishEye", Range(0, 1)) = 0
    }
    SubShader
    {
        Pass
        {
			Cull Off

			HLSLPROGRAM
			#pragma vertex Blit_Pas_Vertex
			#pragma fragment Blit_Pas_Fragment
			//#include "/ShaderLibrary/_MySRP_GlobalValues.hlsl"
			#include "UnityCG.cginc"


			sampler2D _MainTex;
		float _Vign, _Vign2, _FishEye;
		float3 _VignColor;

			struct VertexInput
			{
				float4 vertex		: POSITION;
				float4 uv			: TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 clipVertex		: SV_POSITION;
				float2 uv				: TEXCOODR0;
			};

			VertexOutput Blit_Pas_Vertex(VertexInput v)
			{
				VertexOutput o;
				
				//Set renderMesh to clipSpace
				o.clipVertex = v.vertex;
				o.clipVertex.xy *= 2.0;
				o.clipVertex.y *= _ProjectionParams.x; //Curently using Unity.cginc _Projectparams, but must be in own srp
				o.clipVertex.z += 0.5;

				half2 centerUv = v.uv * 2 - 1;
				half circle = saturate(dot(centerUv, centerUv));
				half fishEye = lerp(0, _FishEye, circle);
				o.clipVertex.xy /= 1 - _FishEye ;
				o.clipVertex.w += fishEye;

				o.uv = v.uv;
				return o;
			}

			float4 Blit_Pas_Fragment(VertexOutput i) : SV_TARGET
			{
				half2 centerUv = i.uv * 2 - 1;
				half circle = dot(centerUv, centerUv) * _Vign - _Vign2;
				half3 vigneting = lerp(1, _VignColor, saturate(circle));
				float4 _col = tex2D(_MainTex, i.uv);
				_col.rgb *= vigneting;
				return _col;
			}
			ENDHLSL
        }
    }
}
