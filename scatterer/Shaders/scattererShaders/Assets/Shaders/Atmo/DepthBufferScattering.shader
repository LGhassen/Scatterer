Shader "Scatterer/DepthBufferScattering" {
	SubShader {
		Tags {"Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass {
			Tags {"Queue" = "Transparent-499" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			Cull Off
			ZTest Off

			//Blend OneMinusDstColor One //soft additive
			Blend SrcAlpha OneMinusSrcAlpha //traditional alpha-blending

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "../CommonAtmosphere.cginc"
			#include "../DepthCommon.cginc"
			#include "Godrays/GodraysCommon.cginc"

//			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile CUSTOM_OCEAN_OFF CUSTOM_OCEAN_ON
			#pragma multi_compile DITHERING_OFF DITHERING_ON
			#pragma multi_compile GODRAYS_OFF GODRAYS_ON

			uniform float _global_alpha;
			uniform float _global_depth;
			uniform float3 _planetPos; //planet origin, in world space
			uniform float3 _camForward; //camera's viewing direction, in world space
			uniform float _ScatteringExposure;

			uniform float _PlanetOpacity; //to smooth transition from/to scaledSpace

			uniform float _Post_Extinction_Tint;
			uniform float extinctionThickness;

#if defined (GODRAYS_ON)
			uniform sampler2D _godrayDepthTexture;
			uniform float _godrayStrength;
#endif
			uniform float _openglThreshold;

#if defined (CUSTOM_OCEAN_ON)
			uniform sampler2D ScattererScreenCopy;
			uniform sampler2D ScattererDepthCopy;
#else
			uniform sampler2D ScattererScreenCopyBeforeOcean;
#endif
			float4x4 CameraToWorld;

#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif

			struct v2f
			{
				float3 camPosRelPlanet : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
			{
				v2f o;

#if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
				outpos = float4(2.0 * v.vertex.x, 2.0 * v.vertex.y *_ProjectionParams.x, -1.0 , 1.0);
#else
				outpos = float4(2.0 * v.vertex.x, 2.0 * v.vertex.y, 0.0 , 1.0);
#endif
				o.camPosRelPlanet = _WorldSpaceCameraPos - _planetPos;
				o.screenPos = ComputeScreenPos(outpos);

				return o;
			}

			//this needs to only be done if rendering with an ocean
			//doesn't hurt either way so whatever
			struct fout
			{
				float4 color : COLOR;
				float depth : DEPTH;
			};

			fout frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS)
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;

#if defined (CUSTOM_OCEAN_ON)
				float zdepth = tex2Dlod(ScattererDepthCopy, float4(uv,0,0));
#else
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));
#endif

#if SHADER_API_D3D11 || SHADER_API_D3D || SHADER_API_D3D12
				if (_ProjectionParams.x > 0) {uv.y = 1.0 - uv.y;}
				if (zdepth == 0.0) {discard;}
#else
				if (zdepth == 1.0) {discard;}
#endif

				float3 absWorldPos = getPreciseWorldPosFromDepth(i.screenPos.xy / i.screenPos.w, zdepth, CameraToWorld);
				float3 worldPos = absWorldPos - _planetPos;  //worldPos relative to planet origin

				float3 groundPos = normalize (worldPos) * Rg * 1.0008;
				float Rt2 = Rg + (Rt - Rg) * _experimentalAtmoScale;

				worldPos = (length(worldPos) < Rt2) ? lerp(groundPos,worldPos,_PlanetOpacity) : worldPos; //fades to flatScaledSpace planet shading to ease the transition to scaledSpace
				//this wasn't applied in extinction shader, not sure if it will be an issue

				worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix

#if defined (CUSTOM_OCEAN_ON)
				float3 backGrnd = tex2Dlod(ScattererScreenCopy, float4(uv.x, uv.y,0.0,0.0));
#else
				float3 backGrnd = tex2Dlod(ScattererScreenCopyBeforeOcean, float4(uv.x, uv.y,0.0,0.0));
#endif

				float3 extinction = getExtinction(i.camPosRelPlanet, worldPos, 1.0, 1.0, 1.0); //same function as in inscattering2 or different?
				float average=(extinction.r+extinction.g+extinction.b)/3;

				//lerped manually because of an issue with opengl or whatever
				extinction = _Post_Extinction_Tint * extinction + (1-_Post_Extinction_Tint) * float3(average,average,average);

				extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );

				//composite backGround by extinction
				backGrnd*=extinction;

				float minDistance = length(worldPos-i.camPosRelPlanet);
				float3 inscatter=0.0;
				extinction=1.0;

//				//TODO: put planetshine stuff in callable function
//				#if defined (PLANETSHINE_ON)
//				for (int j=0; j<4;++j)
//				{
//				if (planetShineRGB[j].w == 0) break;
//
//				float intensity=1;  
//				if (planetShineSources[j].w != 1.0f)
//				{
//				intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[j].xyz - worldPos),SUN_DIR)),0); //if source is not a sun compute intensity of light from angle to light source
//				//totally made up formula by eyeballing it
//				}
//
//				inscatter+=InScattering2(i.camPosRelPlanet, worldPos, normalize(planetShineSources[j].xyz),extinction)  //lot of potential extinction recomputations here
//				*planetShineRGB[j].xyz*planetShineRGB[j].w*intensity;
//				}
//				#endif

#if defined (GODRAYS_ON)
				float godrayDepth = 0.0;

				godrayDepth = sampleGodrayDepth(_godrayDepthTexture, float2(uv.x,1.0-uv.y), 1.0);

				//trying to find the optical depth from the terrain level
				float muTerrain = dot(normalize(worldPos), normalize(_WorldSpaceCameraPos - absWorldPos));

				godrayDepth = _godrayStrength * DistanceFromOpticalDepth(_experimentalAtmoScale * (Rt-Rg) * 0.5, length(worldPos), muTerrain, godrayDepth, minDistance);

				worldPos -= godrayDepth * normalize(worldPos-i.camPosRelPlanet);
#endif

				inscatter+= InScattering2(i.camPosRelPlanet, worldPos,SUN_DIR,extinction);
				inscatter*= (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ; //somehow the shader compiler for OpenGL behaves differently around braces

				inscatter = hdr(inscatter,_ScatteringExposure) *_global_alpha;

				//composite background with inscatter, soft-blend it
				backGrnd+= (1.0 - backGrnd) * dither(inscatter,screenPos);

				fout output;
				output.color = float4(backGrnd,1.0);
				output.depth = zdepth;	//this needs to only be done if rendering with an ocean
				return output;
			}
			ENDCG
		}
	}

}