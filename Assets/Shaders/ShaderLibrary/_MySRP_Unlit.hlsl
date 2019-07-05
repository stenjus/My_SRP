#ifndef _MYSRP_UNLIT_INCLUDED
#define _MYSRP_UNLIT_INCLUDED
#include "/ShaderLibrary/_MySRP_GlobalValues.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _Color;
CBUFFER_END

struct VertexInput 
{
	float4 vertex		: POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 uv			: TEXCOORD1;
	float4 uv2			: TEXCOORD2;
	float4 color		: COLOR;
};

struct VertexOutput 
{
	float4 clipVertex		: SV_POSITION;
};

VertexOutput Unlit_Pas_Vertex(VertexInput v) 
{
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	o.clipVertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0))); //Multypling vertex matrix to Unity ViewProjection and ObjectToWorld projection matrixes

	return o;
}

float4 Unlit_Pas_Fragment(VertexOutput i) : SV_TARGET
{
	return _Color; 
}

#endif //_MYSRP_UNLIT_INCLUDED