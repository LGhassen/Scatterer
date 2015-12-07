
Shader "EncodeFloat/EncodeToFloat" 
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	
	struct v2f 
	{
		float4  pos : SV_POSITION;
		float2  uv : TEXCOORD0;
	};

	v2f vert(appdata_base v)
	{
		v2f OUT;
		OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		OUT.uv = v.texcoord.xy;
		return OUT;
	}
	
	//This is a built in function but I like to see the code
	float4 encodeFloatRGBA(float v) 
	{
		float4 enc = float4(1.0, 255.0, 65025.0, 160581375.0) * v;
		enc = frac(enc);
		enc -= enc.yzww * float4(1.0/255.0,1.0/255.0,1.0/255.0,0.0);
		return enc;
	}
				
	float4 fragR(v2f IN) : COLOR
	{
		//This only seems to work in the range 0 - 0.9999+
		float r = tex2D(_MainTex, IN.uv).r;
//		r = clamp(r, 0.0, 0.9999);
		r=r/255;
		r+=0.5;
		return encodeFloatRGBA(r);
	}
	
	float4 fragG(v2f IN) : COLOR
	{
		//This only seems to work in the range 0 - 0.9999+
		float g = tex2D(_MainTex, IN.uv).g;
//		g = clamp(g, 0.0, 0.9999);
		g=g/255;
		g+=0.5;
		return encodeFloatRGBA(g);
	}
	
	float4 fragB(v2f IN) : COLOR
	{
		//This only seems to work in the range 0 - 0.9999+
		float b = tex2D(_MainTex, IN.uv).b;
//		b = clamp(b, 0.0, 0.9999);
		b=b/255;
		b+=0.5;
		return encodeFloatRGBA(b);
	}
	
	float4 fragA(v2f IN) : COLOR
	{
		//This only seems to work in the range 0 - 0.9999+
		float a = tex2D(_MainTex, IN.uv).a;
		a=a/255;
		a+=0.5;
//		a = clamp(a, 0.0, 0.9999);
		return encodeFloatRGBA(a);
	}
	
	ENDCG
	
	SubShader 
	{
	    Pass 
		{
			ZTest Always Cull Off ZWrite Off
	  		Fog { Mode off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragR
			ENDCG
		}
		
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
	  		Fog { Mode off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragG
			ENDCG
		}
		
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
	  		Fog { Mode off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragB
			ENDCG
		}
		
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
	  		Fog { Mode off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragA
			ENDCG
		}
	}
}