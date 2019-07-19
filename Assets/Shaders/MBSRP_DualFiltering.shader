Shader "Hidden/DualFiltering"
{
	Properties
	{
		_Offset("Offset", float) = 0.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }

			Pass
			{
				Name "Down Sample"
				CGPROGRAM
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

				sampler2D _DualFilterTex;
				float4 _DualFilterTex_ST;
				half2 _DualFilterTex_TexelSize;
				half _Offset;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					half2 uv = TRANSFORM_TEX(v.uv, _DualFilterTex);
					_DualFilterTex_TexelSize * 0.5;
					o.uv[0] = uv;
					o.uv[1] = uv - _DualFilterTex_TexelSize * half2(1 + _Offset, 1 + _Offset); //Top right
					o.uv[2] = uv + _DualFilterTex_TexelSize * half2(1 + _Offset, 1 + _Offset); //Bottom left
					o.uv[3] = uv - half2(_DualFilterTex_TexelSize.x, - _DualFilterTex_TexelSize.y) * half2(1 + _Offset, 1 + _Offset); //Top left
					o.uv[4] = uv + half2(_DualFilterTex_TexelSize.x, - _DualFilterTex_TexelSize.y) * half2(1 + _Offset, 1 + _Offset); //Bottom right

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					half4 summary = tex2D(_DualFilterTex, i.uv[0]) * 4.0;
					summary += tex2D(_DualFilterTex, i.uv[1]);
					summary += tex2D(_DualFilterTex, i.uv[2]);
					summary += tex2D(_DualFilterTex, i.uv[3]);
					summary += tex2D(_DualFilterTex, i.uv[4]);
					return summary * 0.125;
				}
				ENDCG
			}
		}
}
