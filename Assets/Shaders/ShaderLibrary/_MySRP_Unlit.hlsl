#ifndef _MYSRP_UNLIT_INCLUDED
#define _MYSRP_UNLIT_INCLUDED
#include "/ShaderLibrary/_MySRP_GlobalValues.hlsl"

UNITY_INSTANCING_BUFFER_START(PerInstance)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

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
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput Unlit_Pas_Vertex(VertexInput v) 
{
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);

	//Multypling vertex matrix to Unity ViewProjection and ObjectToWorld projection matrixes
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	float4 wPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0));
	o.clipVertex = mul(unity_MatrixVP, wPos);

	return o;
}

float4 Unlit_Pas_Fragment(VertexOutput i) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);
	return UNITY_ACCESS_INSTANCED_PROP	(PerInstance, _Color);
}

#endif //_MYSRP_UNLIT_INCLUDED