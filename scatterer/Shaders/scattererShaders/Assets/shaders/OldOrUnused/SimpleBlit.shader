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
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
 
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma glsl
			
			uniform sampler2D _MainTex;
 
			float4 frag(v2f_img i) : COLOR 
			{
				float4 col = tex2D(_MainTex,i.uv);
//				float3 col = tex2D(cameraRender,i.uv);
				return float4(1.0,0.0,0.0,1.0);
			}
			ENDCG
	    }
	}
}
