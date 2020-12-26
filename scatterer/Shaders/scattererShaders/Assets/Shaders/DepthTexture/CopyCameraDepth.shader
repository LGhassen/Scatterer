Shader "Scatterer/CopyCameraDepth"
{
	SubShader
	{
		Pass
		{
			ZTest Always Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _CameraDepthTexture;

			v2f vert( appdata_img v )
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
							
				o.uv.y = (_ProjectionParams.x == 1.0) ? o.uv.y : 1.0 - o.uv.y ;

				return o;
			}

			float frag(v2f IN) : SV_Depth
			{
				return float4(tex2Dlod(_CameraDepthTexture, float4(IN.uv,0.0,0.0)).rgb,1.0);
			}
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;

			v2f vert( appdata_img v )
			{
				v2f o = (v2f)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				o.uv.y = (_ProjectionParams.x == 1.0) ? o.uv.y : 1.0 - o.uv.y ;

				return o;
			}

			float frag(v2f IN) : SV_Depth
			{
				return float4(tex2Dlod(_MainTex, float4(IN.uv,0.0,0.0)).rgb,1.0);
			}
			ENDCG
		} 
	}
	Fallback off
}