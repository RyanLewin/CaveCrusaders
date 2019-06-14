Shader "Hidden/Custom/DepthFilter"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_RT, sampler_RT);
	TEXTURE2D_SAMPLER2D(_DepthB, sampler_DepthB);

	float _Blend;
	int _Debug;
	int _DFilter;
	sampler2D _CameraDepthTexture;
	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float Currentdepth = tex2D(_CameraDepthTexture, i.texcoord).r;
		float MainCamdepth = SAMPLE_TEXTURE2D(_DepthB, sampler_DepthB, i.texcoord).r;
		if (_DFilter > 0)
		{
			return color;
		}
		if (_Debug > 0)
		{
			return Linear01Depth(MainCamdepth);
		}
		if (MainCamdepth > Currentdepth)
		{
			return color;
		}
		return float4(0,0,0,0);
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