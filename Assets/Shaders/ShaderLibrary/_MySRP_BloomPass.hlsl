#ifndef _MYSRP_BLOOMPASS_INCLUDED
#define _MYSRP_BLOOMPASS_INCLUDED
#include "/ShaderLibrary/_MySRP_GlobalValues.hlsl"

half4 BloomPass(half4 _FrameBuffer) 
{
	half4 _BloomResult;
	for (int i = 0; i < BLOOM_PASSES_COUNT; i++)
	{
		_BloomResult = _FrameBuffer;
	}
	return _BloomResult;
}

#endif //_MYSRP_BLOOMPASS_INCLUDED