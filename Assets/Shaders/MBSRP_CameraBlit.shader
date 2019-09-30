Shader "Hidden/CameraBlit"
{
    Properties
    {
		//_Chromatic_Aberration_Offset("Chromatic Aberration", Range(0.0, 1.0)) = 0.05
    }

    SubShader
    {
        Pass
        {
			Cull Off

			HLSLPROGRAM
			#pragma vertex Blit_Pas_Vertex
			#pragma fragment Blit_Pas_Fragment
			#pragma multi_compile _ BLOOM_ON
			#pragma multi_compile _ VIGNETTING_ON
			#pragma multi_compile _ FISHEYE_ON_VERTEX FISHEYE_ON_FRAGMENT
			#pragma multi_compile _ LUT_ON
			#pragma multi_compile _ CHROMATIC_ABERRATION
			#include "/ShaderLibrary/_MySRP_GlobalValues.hlsl"
			#include "/ShaderLibrary/_MySRP_BloomPass.hlsl"
			


			sampler2D _FrameBuffer;
			
			float _FishEye;
			
			#if LUT_ON
			sampler2D _LUT_Tex;
			float4 _LUT_TexelSize;
			half _LUT_Power;
			#endif

			#if VIGNETTING_ON
			float3	_VignettingColor;
			float	_Vignetting_Size;
			float	_Vignetting_Contrast;
			#endif

			#if BLOOM_ON
			sampler2D _BloomResult;
			float _BloomIntencity;
			#endif

			#if CHROMATIC_ABERRATION
			float _Chromatic_Aberration_Offset;
			float _Chromatic_Aberration_Radius;
			#endif

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
				o.clipVertex.y *= -1;
				o.clipVertex.z += 0.5;


				o.uv.xy = v.uv.xy;

				//Apply FishEye Vertex
				//----
				#if FISHEYE_ON_VERTEX
				half2 centerUv = v.uv.xy * 2.0f - 1.0f;
				half circle = saturate(dot(centerUv, centerUv));
				half2 scaledUV = (v.uv.xy - 0.5) * (1 - _FishEye) + 0.5;
				half2 fsihEyeUV = lerp(scaledUV, v.uv.xy, circle);
				o.uv.xy = fsihEyeUV;
				#endif
				//----
				return o;
			}

			float4 Blit_Pas_Fragment(VertexOutput i) : SV_TARGET
			{
				half2 uv = i.uv.xy;



				//Apply FishEye Fragment
				//----
				#if FISHEYE_ON_FRAGMENT
				half2 centerUv_F = i.uv.xy * 2.0f - 1.0f;
				half circle_F = saturate(dot(centerUv_F, centerUv_F));
				half2 scaledUV = (i.uv.xy - 0.5) * (1 - _FishEye) + 0.5;
				half2 fsihEyeUV = lerp(scaledUV, i.uv.xy, circle_F);
				uv = fsihEyeUV;
				#endif
				//----


				half4 _col = tex2D(_FrameBuffer, uv);

				//Chromatic Aberration
				//----
				#if CHROMATIC_ABERRATION
				half2 centerUv_Ch = i.uv.xy * 2.0f - 1.0f;
				half circle_Ch = saturate(dot(centerUv_Ch, centerUv_Ch) * _Chromatic_Aberration_Radius);
				_Chromatic_Aberration_Offset *= circle_Ch;
				half _colR = tex2D(_FrameBuffer, half2(uv.x, uv.y - _Chromatic_Aberration_Offset)).r;
				half _colG = tex2D(_FrameBuffer, half2(uv.x - _Chromatic_Aberration_Offset, uv.y)).g;
				half _colB = tex2D(_FrameBuffer, half2(uv.x + _Chromatic_Aberration_Offset, uv.y)).b;
				_col = half4(_colR, _colG, _colB, 1);
				#endif
				//----

				//Unactive Bloom Pass
				#if BLOOM_ON
				half3 _Bloom = tex2D(_BloomResult, uv).rgb;
				_col.rgb = _Bloom * _BloomIntencity;
				#endif
				//----

				//Apply LUT
				#if LUT_ON
				_col = saturate(_col);
				half xOffset = 0.00048828125 + _col.r * 0.0302734375;
				half yOffset = 0.015625 + _col.g * 0.96875;
				half cell = floor(_col.b * 31);
				half2 lutPos = float2(cell / 32 + xOffset, yOffset);
				half4 gradenCol = tex2D(_LUT_Tex, lutPos);
				_col = lerp(_col, gradenCol, _LUT_Power);
				#endif
				//----



				//Vignetting Pass
				#if VIGNETTING_ON
				half2 centerUv = i.uv * 2 - 1;
				half circle = dot(centerUv, centerUv) * _Vignetting_Size - _Vignetting_Contrast;
				half3 vigneting = lerp(1, _VignettingColor, saturate(circle));
				_col.rgb *= vigneting;
				#endif
				//----

				return _col;
			}
			ENDHLSL
        }
    }
}
