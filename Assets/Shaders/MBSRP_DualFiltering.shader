Shader "Hidden/DualFiltering"
{
	Properties
	{
		//_MainTex("Texture", 2D) = "white" {}
		_Bright("Offset", float) = 0.0
		_Bright2("Offset", float) = 0.0
	}
		SubShader
		{
			ZTest Always Cull Off ZWrite Off

			Pass
			{
				Name "Down Sample"
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv[5] : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				half2 _MainTex_TexelSize;
				half _BlurOffsetDown;

				v2f vert(appdata v)
				{
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);

					half2 uv = v.uv;
					_MainTex_TexelSize * 0.5;
					o.uv[0] = uv;
					o.uv[1] = uv - _MainTex_TexelSize * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Top right
					o.uv[2] = uv + _MainTex_TexelSize * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Bottom left
					o.uv[3] = uv - half2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Top left
					o.uv[4] = uv + half2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Bottom right

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 summary = tex2D(_MainTex, i.uv[0]) * 4.0;
					half4 summary2 = tex2D(_MainTex, i.uv[0]) * 4.0;
					summary += tex2D(_MainTex, i.uv[1]);
					summary += tex2D(_MainTex, i.uv[2]);
					summary += tex2D(_MainTex, i.uv[3]);
					summary += tex2D(_MainTex, i.uv[4]);
					return summary * 0.125;
				}
				ENDHLSL
			}

			Pass
			{
				Name "Up Sample"
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv[8] : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				half2 _MainTex_TexelSize;
				half _BlurOffsetUp;
				sampler2D _BloomUp;

				v2f vert(appdata v)
				{
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);

					half2 uv = v.uv;
					_MainTex_TexelSize * 0.5;
					_BlurOffsetUp = half2(_BlurOffsetUp + 1.0, _BlurOffsetUp + 1.0);

					o.uv[0] = uv + half2(-_MainTex_TexelSize.x * 2.0, 0.0) * _BlurOffsetUp;
					o.uv[1] = uv + half2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y) * _BlurOffsetUp;
					o.uv[2] = uv + half2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurOffsetUp;
					o.uv[3] = uv + _MainTex_TexelSize * _BlurOffsetUp;
					o.uv[4] = uv + half2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurOffsetUp;
					o.uv[5] = uv + half2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _BlurOffsetUp;
					o.uv[6] = uv + half2(0.0, -_MainTex_TexelSize.y * 2.0) * _BlurOffsetUp;
					o.uv[7] = uv - _MainTex_TexelSize * _BlurOffsetUp;

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 summary = 0;
					summary += tex2D(_MainTex, i.uv[0]);
					summary += tex2D(_MainTex, i.uv[1]) * 2.0;
					summary += tex2D(_MainTex, i.uv[2]);
					summary += tex2D(_MainTex, i.uv[3]) * 2.0;
					summary += tex2D(_MainTex, i.uv[4]);
					summary += tex2D(_MainTex, i.uv[5]) * 2.0;
					summary += tex2D(_MainTex, i.uv[6]);
					summary += tex2D(_MainTex, i.uv[7]) * 2.0;
					return summary * 0.0833;
				}
				ENDHLSL
			}

			Pass
			{
				Name "BrightPass"
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float _Bright;
				float _Bright2;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 col = tex2D(_MainTex, i.uv);
					half4 brightResult = saturate(col - _Bright);
					return brightResult;
				}
				ENDHLSL
			}
		}
}
