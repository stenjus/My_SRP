Shader "Hidden/DualFiltering"
{
	Properties
	{
		_Bright("Offset", float) = 0.0
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

				sampler2D _DownScalePassTex;
				half2 _DownScalePassTex_TexelSize;
				half _BlurOffsetDown;

				v2f vert(appdata v)
				{
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);

					half2 uv = v.uv;
					_DownScalePassTex_TexelSize * 0.5;
					o.uv[0] = uv;
					o.uv[1] = uv - _DownScalePassTex_TexelSize * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Top right
					o.uv[2] = uv + _DownScalePassTex_TexelSize * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Bottom left
					o.uv[3] = uv - half2(_DownScalePassTex_TexelSize.x, -_DownScalePassTex_TexelSize.y) * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Top left
					o.uv[4] = uv + half2(_DownScalePassTex_TexelSize.x, -_DownScalePassTex_TexelSize.y) * half2(1 + _BlurOffsetDown, 1 + _BlurOffsetDown); //Bottom right

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 summary = tex2D(_DownScalePassTex, i.uv[0]) * 4.0;
					half4 summary2 = tex2D(_DownScalePassTex, i.uv[0]) * 4.0;
					summary += tex2D(_DownScalePassTex, i.uv[1]);
					summary += tex2D(_DownScalePassTex, i.uv[2]);
					summary += tex2D(_DownScalePassTex, i.uv[3]);
					summary += tex2D(_DownScalePassTex, i.uv[4]);
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
					float2 uvOriginal : TEXCOORD8;
					float4 vertex : SV_POSITION;
				};

				sampler2D _UpscalePassTex;
				sampler2D _DownPass;
				half2 _UpscalePassTex_TexelSize;
				half _BlurOffsetUp;

				v2f vert(appdata v)
				{
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);

					half2 uv = v.uv;
					_UpscalePassTex_TexelSize * 0.5;
					_BlurOffsetUp = half2(_BlurOffsetUp + 1.0, _BlurOffsetUp + 1.0);

					o.uv[0] = uv + half2(-_UpscalePassTex_TexelSize.x * 2.0, 0.0) * _BlurOffsetUp;
					o.uv[1] = uv + half2(-_UpscalePassTex_TexelSize.x, _UpscalePassTex_TexelSize.y) * _BlurOffsetUp * 1.21f;
					o.uv[2] = uv + half2(0.0, _UpscalePassTex_TexelSize.y * 2.0) * _BlurOffsetUp;
					o.uv[3] = uv + _UpscalePassTex_TexelSize * _BlurOffsetUp  * 1.21f;
					o.uv[4] = uv + half2(_UpscalePassTex_TexelSize.x * 2.0, 0.0) * _BlurOffsetUp;
					o.uv[5] = uv + half2(_UpscalePassTex_TexelSize.x, -_UpscalePassTex_TexelSize.y) * _BlurOffsetUp  * 1.21f;
					o.uv[6] = uv + half2(0.0, -_UpscalePassTex_TexelSize.y * 2.0) * _BlurOffsetUp;
					o.uv[7] = uv - _UpscalePassTex_TexelSize * _BlurOffsetUp  * 1.21f;
					o.uvOriginal = v.uv;

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 summary = 0;
					summary += tex2D(_UpscalePassTex, i.uv[0]);
					summary += tex2D(_UpscalePassTex, i.uv[1]) * 2.0;
					summary += tex2D(_UpscalePassTex, i.uv[2]);
					summary += tex2D(_UpscalePassTex, i.uv[3]) * 2.0;
					summary += tex2D(_UpscalePassTex, i.uv[4]);
					summary += tex2D(_UpscalePassTex, i.uv[5]) * 2.0;
					summary += tex2D(_UpscalePassTex, i.uv[6]);
					summary += tex2D(_UpscalePassTex, i.uv[7]) * 2.0;
					
					half4 downP = tex2D(_DownPass, i.uvOriginal);
					//return  downP * 0.3f;
					return (summary * 0.0833) * 0.5f + downP * 0.5f;
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

				sampler2D _BrightPassTex;
				float _Bright;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 col = tex2D(_BrightPassTex, i.uv);
					half4 brightResult = saturate(col - _Bright);
					return brightResult;
				}
				ENDHLSL
			}
		}
}
