#ifndef _MYSRP_GLOBALVALUES_INCLUDED
#define _MYSRP_GLOBALVALUES_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4 unity_LightIndicesOffsetAndCount;
	float4 unity_4LightIndices0, unity_4LightIndices1;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 16
CBUFFER_START(_LightBuffer)
	int _MaxVisibleLights = 4;
	float4 _VisibleLightColor[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirectionOrPosition[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightAttenuation[MAX_VISIBLE_LIGHTS];
	float4 _visibleLightSpotDirections[MAX_VISIBLE_LIGHTS];

CBUFFER_END
#define UNITY_MATRIX_M unity_ObjectToWorld
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"


#endif //_MYSRP_GLOBALVALUES_INCLUDED