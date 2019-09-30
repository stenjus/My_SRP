Shader "Hidden/ShaderError"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ErrorTexScale("Error Texture Scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "Forward"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			half _ErrorTexScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				half2 screenSpaceUV = (i.uv.xy / i.uv.w) * _ErrorTexScale;
                fixed4 col = tex2D(_MainTex, screenSpaceUV);
                return col;
            }
            ENDCG
        }
    }
}
