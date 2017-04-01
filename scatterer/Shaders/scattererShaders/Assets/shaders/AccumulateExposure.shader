Shader "LongExposure/Accumulate"
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "black" {}
	}
	SubShader 
	{
	    Pass 
	    {
	    	Blend One One
	    
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
 
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma glsl
			
			uniform sampler2D _MainTex;
//			uniform sampler2D cameraRender;
//			uniform sampler2D accumulated;
 
			float4 frag(v2f_img i) : COLOR 
			{
				float3 col = float3(0.0001,0.0001,0.0001)*tex2D(_MainTex,i.uv);
//				float3 col = tex2D(_MainTex,i.uv);
//				float3 col = tex2D(cameraRender,i.uv);
				return float4(col.rgb, 1.0);
			}
			ENDCG
	    }
	}
}
