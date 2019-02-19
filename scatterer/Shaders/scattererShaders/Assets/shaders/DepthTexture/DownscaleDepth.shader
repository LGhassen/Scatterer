Shader "Scatterer/DownscaleDepth" {
	
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f 
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _CameraDepthTexture;
	float4 _CameraDepthTexture_TexelSize; // (1.0/width, 1.0/height, width, height)
	
	v2f vert( appdata_img v ) 
	{
		v2f o = (v2f)0; 
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		
		return o;
	}

	float4 frag(v2f input) : SV_Target 
	{		
		float2 texelSize = 0.5 * _CameraDepthTexture_TexelSize.xy;
		float2 taps[4] = { 	float2(input.uv + float2(-1,-1)*texelSize),
							float2(input.uv + float2(-1,1)*texelSize),
							float2(input.uv + float2(1,-1)*texelSize),
							float2(input.uv + float2(1,1)*texelSize) };
		
		float depth1 = tex2D(_CameraDepthTexture, taps[0]);
		float depth2 = tex2D(_CameraDepthTexture, taps[1]);
		float depth3 = tex2D(_CameraDepthTexture, taps[2]);
		float depth4 = tex2D(_CameraDepthTexture, taps[3]);

		float depth0 = tex2D(_CameraDepthTexture,input.uv);
		
		float result = (depth0 == 1.0) ? 1.0 : min(depth1, min(depth2, min(depth3, depth4)));
		
		return float4(result,result,result,result);
	}
	
	ENDCG
	SubShader 
	{
		 Pass 
		 {
			  ZTest Always Cull Off ZWrite Off

			  CGPROGRAM
			  #pragma vertex vert
			  #pragma fragment frag
			  ENDCG
		  }
	}
	Fallback off
}
