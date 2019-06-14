Shader "Hidden/Custom/OutlineBliter"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_RT, sampler_RT);
	float _Blend;
	int _Debug;
	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float4 RTColour = SAMPLE_TEXTURE2D(_RT, sampler_RT, i.texcoord);
		//color = RTColour;
		if (_Debug > 0.1f)
		{
			return RTColour;
		}
		if (RTColour.a > 0.5f)
		{
			return RTColour;
		}
		return color;
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment Frag

			ENDHLSL
		}
	}
}