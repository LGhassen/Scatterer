Shader "Scatterer/ModulateShadowMaskWithOcclusion"
{
	SubShader
	{
		Pass
		{
			Cull Back ZWrite Off ZTest Off
			//Blend DstColor Zero  // Multiplicative
			Blend SrcAlpha OneMinusSrcAlpha //alpha blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			sampler2D _MainTex; //doesn't seem to be set automatically in blit?
			sampler2D OcclusionTexture;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.texcoord;
				return OUT;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col = tex2D(OcclusionTexture, i.uv);
				//return col;
				return float4(0.5,0.5,0.5,0.5);
			}
			ENDCG
		}
	}
}