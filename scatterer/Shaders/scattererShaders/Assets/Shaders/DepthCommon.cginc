#include "UnityCG.cginc"

uniform sampler2D _CameraDepthTexture;
uniform sampler2D _customDepthTexture;

float3 getViewSpacePosFromDepth(float2 uv)
{
	float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));

#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
	zdepth = 1 - zdepth;
#endif

	float4 clipPos = float4(uv, zdepth, 1.0);
	clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
	float4 camPos = mul(unity_CameraInvProjection, clipPos);
    camPos.xyz /= camPos.w;
    camPos.z *= -1;
    return camPos.xyz;
}

float getScattererFragDistance(float2 uv)
{
#if defined (SCATTERER_MERGED_DEPTH_ON)
	return tex2Dlod(_customDepthTexture, float4(uv,0,0)).r* 750000;
#else
	return length(getViewSpacePosFromDepth(uv).xyz);
#endif
}

//checks if we have anything in the depth buffer or is empty at coordinate
//used by sunflare to check if we should block the sun
fixed checkDepthBufferEmpty(float2 uv)
{
#if defined (SCATTERER_MERGED_DEPTH_ON)
	float depth =  tex2Dlod(_customDepthTexture,float4(uv,0.0,0.0));  //if there's something in the way don't render the flare	
	return (depth < 1.0) ? 0.0 : 1.0 ;
#else
	float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));

	#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
		zdepth = 1 - zdepth;
	#endif

	return (zdepth < 1.0) ? 0.0 : 1.0 ;
#endif
}