// Composits the 1/4 res light rays into the scene using nearest-depth upscaling

Shader "Scatterer/CompositeCausticsGodrays"
{
	Properties{
	}

	SubShader{
		Tags { "Queue"="Transparent+2" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass {
			Blend OneMinusDstColor One //softAdditive
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include"UnityCG.cginc"

			uniform sampler2D LightRaysTexture;

			uniform sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;

			uniform sampler2D ScattererDownscaledDepth;
			float4 ScattererDownscaledDepth_TexelSize;


			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
//				#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
//				o.pos = float4(v.vertex.x, v.vertex.y *_ProjectionParams.x, -1.0 , 1.0);
//				#else
//				o.pos = float4(v.vertex.x, v.vertex.y *_ProjectionParams.x, 1.0 , 1.0);
//				#endif
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}

			void UpdateNearestSample(	inout float MinDist,
				inout float2 NearestUV,
				float Z,
				float2 UV,
				float ZFull
			)
			{
				float Dist = abs(Z - ZFull);
				if (Dist < MinDist)
				{
					MinDist = Dist;
					NearestUV = UV;
				}
			}


			float4 frag(v2f i) : COLOR
			{

//				//read full resolution depth
//				float ZFull = Linear01Depth( SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv) );
//
//				//find low res depth texture texel size
//				float2 lowResTexelSize = 2.0 * _CameraDepthTexture_TexelSize.xy;
//				float depthTreshold =  0.01; //play with this?
//
//				float2 lowResUV = i.uv; 
//
//				float MinDist = 1.e8f;
//
//				float2 UV00 = lowResUV - 0.5 * lowResTexelSize;
//				float2 NearestUV = UV00;
//				float Z00 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledDepth, UV00) );   
//				UpdateNearestSample(MinDist, NearestUV, Z00, UV00, ZFull);
//
//				float2 UV10 = float2(UV00.x+lowResTexelSize.x, UV00.y);
//				float Z10 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledDepth, UV10) );  
//				UpdateNearestSample(MinDist, NearestUV, Z10, UV10, ZFull);
//
//				float2 UV01 = float2(UV00.x, UV00.y+lowResTexelSize.y);
//				float Z01 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledDepth, UV01) );  
//				UpdateNearestSample(MinDist, NearestUV, Z01, UV01, ZFull);
//
//				float2 UV11 = UV00 + lowResTexelSize;
//				float Z11 = Linear01Depth( SAMPLE_DEPTH_TEXTURE( ScattererDownscaledDepth, UV11) );  
//				UpdateNearestSample(MinDist, NearestUV, Z11, UV11, ZFull);
//
//				float4 color = float4(0,0,0,0);
//
//				//couldn't get this to work, I suspect my depthThreshold is too small, so for now no bilinear sampling over the same surface, only nearest sample, looks fine though
//
//				//				[branch]
//				//				if (abs(Z00 - ZFull) < depthTreshold &&
//				//					abs(Z10 - ZFull) < depthTreshold &&
//				//					abs(Z01 - ZFull) < depthTreshold &&
//				//					abs(Z11 - ZFull) < depthTreshold )
//				//				{
//				//					color = tex2Dlod( cloudTexture, float4(lowResUV,0,0)) ;
//				//				}
//				//				else
//				//				{
//				color = tex2Dlod(LightRaysTexture, float4(NearestUV,0,0)) ;
//				//				}


				float4 color = tex2Dlod(LightRaysTexture, float4(i.uv,0,0)) ;
				return float4(color.rgb,1.0);
			}

			ENDCG
		}
	}
}