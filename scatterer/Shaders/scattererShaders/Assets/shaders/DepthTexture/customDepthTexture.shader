// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Scatterer/DepthTexture" {
	SubShader {
		Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
		Pass {
			Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
			Fog { Mode Off }
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			//#define LOGARITHMIC_DEPTH_ON
			#define VIEW_SPACE_DISTANCE_ON


			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float2 depth : TEXCOORD2;

				#if defined (LOGARITHMIC_DEPTH_ON)
				float4 vertexPosClip : TEXCOORD0;
				#elif defined (VIEW_SPACE_DISTANCE_ON)
				float3 vertexPosView : TEXCOORD0;
				#endif

				float4 vertexPosClip : TEXCOORD1;

			};

			v2f vert (appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.depth=o.pos.zw;
				o.vertexPosClip = o.pos;

				#if defined (LOGARITHMIC_DEPTH_ON)
				o.vertexPosClip = o.pos;
				#elif defined (VIEW_SPACE_DISTANCE_ON)
				o.vertexPosView = mul (UNITY_MATRIX_MV, v.vertex);
				#endif

				return o;
			}

			struct fout {
				float4 color : COLOR;
				float depth : DEPTH;
			};

			fout frag(v2f i)
			{

				fout OUT;

				#if defined (LOGARITHMIC_DEPTH_ON)

				float C=1.0;
				float _offset=2.0;
				return (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));

				#elif defined (VIEW_SPACE_DISTANCE_ON)
				OUT.color = abs(i.vertexPosView.z) / 750000.0;
				float C=1.0;
				float _offset=2.0;
				OUT.depth = (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));
				return OUT;

				#else

				return (i.depth.x/i.depth.y);

				#endif

			}
			ENDCG
		}
	}




	// CUT-OUT SUBSHADER
	SubShader {

		Tags { "RenderType"="TransparentCutout" "IgnoreProjector" = "True"}
		Pass {
			Tags { "RenderType"="TransparentCutout" "IgnoreProjector" = "True"}
			Fog { Mode Off }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			//#define LOGARITHMIC_DEPTH_ON
			#define VIEW_SPACE_DISTANCE_ON


			#include "UnityCG.cginc"

			sampler2D _MainTex;
			//float _Cutoff;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 depth : TEXCOORD2;

				#if defined (LOGARITHMIC_DEPTH_ON)
				float4 vertexPosClip : TEXCOORD0;
				#elif defined (VIEW_SPACE_DISTANCE_ON)
				float3 vertexPosView : TEXCOORD0;
				#endif

				float4 vertexPosClip : TEXCOORD1;
				float2 uv_MainTex    : TEXCOORD3;

			};

			v2f vert (appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.depth=o.pos.zw;
				o.vertexPosClip = o.pos;

				#if defined (LOGARITHMIC_DEPTH_ON)
				o.vertexPosClip = o.pos;
				#elif defined (VIEW_SPACE_DISTANCE_ON)
				o.vertexPosView = mul (UNITY_MATRIX_MV, v.vertex);
				#endif

				o.uv_MainTex = v.texcoord;

				return o;
			}

			struct fout {
				float4 color : COLOR;
				float depth : DEPTH;
			};

			fout frag(v2f i)
			{

				fout OUT;

				#if defined (LOGARITHMIC_DEPTH_ON)

				float C=1.0;
				float _offset=2.0;
				return (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));

				#elif defined (VIEW_SPACE_DISTANCE_ON)
				OUT.color   = abs(i.vertexPosView.z) / 750000.0;

				if (tex2D(_MainTex, i.uv_MainTex).a <0.7) //works better
				//if (tex2D(_MainTex, i.uv_MainTex).a <=_Cutoff)
				discard;

				float C=1.0;
				float _offset=2.0;
				OUT.depth = (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));
				return OUT;

				#else

				return (i.depth.x/i.depth.y);

				#endif

			}
			ENDCG
		}
	}






}