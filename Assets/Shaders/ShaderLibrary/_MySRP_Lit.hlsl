#ifndef _MYSRP_LIT_INCLUDED
#define _MYSRP_LIT_INCLUDED
#include "/ShaderLibrary/_MySRP_GlobalValues.hlsl"

UNITY_INSTANCING_BUFFER_START(PerInstance)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

struct VertexInput 
{
	float4 vertex		: POSITION;
	float4 uv			: TEXCOORD1;
	float4 uv2			: TEXCOORD2;
	float4 color		: COLOR;
	float3 normal		: NORMAL;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput 
{
	float4 clipVertex		: SV_POSITION;
	float3 normal			: TEXCOORD0;
	float3 worldPos			: TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput Lit_Pas_Vertex(VertexInput v) 
{
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

	//Multypling vertex matrix to Unity ViewProjection and ObjectToWorld projection matrixes
	float4 wPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1.0));
	o.worldPos = wPos.xyz;
	
	o.normal = mul((float3x3)UNITY_MATRIX_M, v.normal);
	o.clipVertex = mul(unity_MatrixVP, wPos);

	return o;
}

float3 DiffuseLightFunc(int index, float3 normal, float3 worldPos)
{
	float3 lightColor = _VisibleLightColor[index].rgb;
	float4 lightDirectionOrPosition = _VisibleLightDirectionOrPosition[index];
	float3 lightVector = lightDirectionOrPosition.xyz - worldPos * lightDirectionOrPosition.w;
	float3 lightDirection = normalize(lightVector);
	float4 lightAttenuation = _VisibleLightAttenuation[index];

	float3 diffuseLight = saturate(dot(normal, lightDirection));

	//Point Lights Range Calculations
	float rangeFade = dot(lightVector, lightVector) * lightAttenuation.x;
	rangeFade = saturate(1.0 - rangeFade * rangeFade);
	rangeFade *= rangeFade;

	//Point Lights Distance falloff
	float distanceSqr = max(dot(lightVector, lightVector), 0.0001f);

	//Spot Lights Calculations 
	float3 spotDirections = _visibleLightSpotDirections[index].xyz;
	float spotFade = dot(spotDirections, lightDirection);
	spotFade = saturate(spotFade * lightAttenuation.z + lightAttenuation.w);
	spotFade *= spotFade;


	diffuseLight *= spotFade * rangeFade / distanceSqr;
	return diffuseLight * lightColor;
}

float4 Lit_Pas_Fragment(VertexOutput i) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);
	i.normal = normalize(i.normal);
	half3 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color).rgb;
	half3 DiffuseLight = 0;
	for (int Z = 0; Z < MAX_VISIBLE_LIGHTS; Z++)
	{
		DiffuseLight += DiffuseLightFunc(Z, i.normal, i.worldPos);
	}
	return float4(DiffuseLight * albedo, 1);
}



#endif //_MYSRP_LIT_INCLUDED