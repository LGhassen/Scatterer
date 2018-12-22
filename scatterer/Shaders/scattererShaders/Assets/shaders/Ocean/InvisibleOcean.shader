Shader "Scatterer/invisible" 
{
	SubShader 
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	
    	Pass 
    	{
    		ZWrite Off
    		ZTest on

			Blend SrcAlpha OneMinusSrcAlpha


			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl
			
			struct v2f 
			{
    			float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
    			//OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    			OUT.pos = 1;
    			OUT.pos.z = -OUT.pos.w; //cull vertex z/w = -1 behind far plane. source: Siggraph 2012, Creating vast game worlds (just cause 2)
    			return OUT;
			}


			float4 frag(v2f IN) : COLOR
			{
				discard;
				return float4(0.0,0.0,0.0,0.0);			
			}
			
			ENDCG
    	}
	}
}