Shader "Scatterer/DepthBufferScattering" {
	SubShader {
		Tags {"Queue" = "Transparent-498" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		//merged scattering+extinction pass
		Pass {
			Tags {"Queue" = "Transparent-498" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

			//Cull Front
			Cull Off
			ZTest Off
			ZWrite Off

			//Blend OneMinusDstColor One //soft additive
			Blend SrcAlpha OneMinusSrcAlpha //traditional alpha-blending

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "../CommonAtmosphere.cginc"

			#pragma multi_compile GODRAYS_OFF GODRAYS_ON
			//			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile CUSTOM_OCEAN_OFF CUSTOM_OCEAN_ON
			#pragma multi_compile DITHERING_OFF DITHERING_ON

			uniform float _global_alpha;
			uniform float _global_depth;
			uniform float3 _planetPos; //planet origin, in world space
			uniform float3 _camForward; //camera's viewing direction, in world space
			uniform float _ScatteringExposure;

			uniform float _PlanetOpacity; //to smooth transition from/to scaledSpace

			uniform float _Post_Extinction_Tint;
			uniform float extinctionThickness;

			uniform sampler2D _customDepthTexture;
#if defined (GODRAYS_ON)
			uniform sampler2D _godrayDepthTexture;
#endif
			uniform float _openglThreshold;

			uniform sampler2D _CameraDepthTexture;
			uniform sampler2D ScattererScreenCopy;
			float4x4 CameraToWorld;

			//            //eclipse uniforms
			//#if defined (ECLIPSES_ON)			
			//			uniform float4 sunPosAndRadius; //xyz sun pos w radius
			//			uniform float4x4 lightOccluders1; //array of light occluders
			//											 //for each float4 xyz pos w radius
			//			uniform float4x4 lightOccluders2;
			//#endif

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
				//outpos = float4(2.0 * v.vertex.x, 2.0 * v.vertex.y *_ProjectionParams.x, 0.0 , 1.0); //sure about this? test in ogl also
				outpos = float4(2.0 * v.vertex.x, 2.0 * v.vertex.y *_ProjectionParams.x, 0.5 , 1.0); //sure about this? test in ogl also
#endif
				o.camPosRelPlanet = _WorldSpaceCameraPos - _planetPos;
				o.screenPos = ComputeScreenPos(outpos);

				return o;
			}

			half4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0));

				if (zdepth == 0.0)
					return float4(0.0,0.0,0.0,0.0);

//				if (uv.x > 0.5)
//				{
//					return float4(zdepth,0.0,0.0,1.0);
					float zdepth2 = zdepth;
			#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
					zdepth2 = 1 - zdepth2;
			#endif

					float4 clipPos1 = float4(uv, zdepth2, 1.0);
					clipPos1.xyz = 2.0f * clipPos1.xyz - 1.0f;
					float4 camPos = mul(unity_CameraInvProjection, clipPos1);

					float4 absWorldPos = mul(CameraToWorld,camPos);
					absWorldPos/=absWorldPos.w;
//					if (uv.x > 0.5)
//					{
//						//return float4(normalize(absWorldPos.xyz - _WorldSpaceCameraPos.xyz),1.0);
//						float dist = length(absWorldPos.xyz - _WorldSpaceCameraPos.xyz);
//						return float4(dist/100000.0,dist/100000.0,dist/100000.0,1.0);
//					}

					float3 worldViewDir = normalize(absWorldPos.xyz - _WorldSpaceCameraPos.xyz);
//				}

				//here's the plan, using current viewDir do a search for the target distance that would give us the same value as the zdepth from texture
				//max distance is farclip plane * angle
				//min distance is nearclip plane
				//iterate, 15 iterations gets you 2^15=32768 which gets you a precision of 750000/32768=22 fucking meters
				//so we need the camera view direction, an arbitrary distance here
				//-> for each iteration, calculate camera position -> calculate clip position -> compare zdepth or 1-zdepth, whatever

//				return float4(i.camViewDir,1.0); //seems to be working

				int maxIterations = 30;
				int iteration = 0;

				float maxSearchDistance = _ProjectionParams.z * 2.0; //replace by correct angle etc //try to init these from the normal depth buffer thingy
//				float maxSearchDistance = _ProjectionParams.z; //replace by correct angle etc
				float minSearchDistance = _ProjectionParams.y;


//				float mid = minSearchDistance; //with this zdepth is 1.0
//				float mid = maxSearchDistance; //with this zdepth is 0.0, so it's correct
				float mid = 0;
				float3 camPosition = float3(0.0,0.0,0.0);
				float3 worldPos0 = float3(0.0,0.0,0.0);
				float4 clipPos = float4(0.0,0.0,0.0,1.0);
				float depth = -10.0;

//				return float4(depth,0.0,0.0,1.0);

				while ((iteration < maxIterations) && (depth != zdepth))
				{
					mid = 0.5 * (maxSearchDistance + minSearchDistance);

					worldPos0 = _WorldSpaceCameraPos + worldViewDir * mid;

					clipPos = mul(UNITY_MATRIX_VP, float4(worldPos0,1.0));
					depth = clipPos.z/clipPos.w;

					if (depth < zdepth)
					{
						maxSearchDistance = mid;
					}
					else// if (depth > zdepth)
					{
						minSearchDistance = mid;
					}

					iteration++;
				}

				//return float4(mid/100000.0,mid/100000.0,mid/100000.0,1.0); //we seem to be overestimating distances

//				return float4(mid / 10000.0,0.0,0.0,1.0); //we know it works until here

//				float3 camPos = mid * i.camViewDir; //position in camera space
//
////				return float4(length(camPos) / 10000.0, 0.0, 0.0, 1.0); //works
//
//				float4 absoluteWorldPos = mul(CameraToWorld,float4(camPos,1.0));
//				absoluteWorldPos/=absoluteWorldPos.w;
//
//				return float4(normalize(absoluteWorldPos.xyz - _WorldSpaceCameraPos.xyz),1.0);

//				float3 worldPos = absoluteWorldPos.xyz - _planetPos; //worldPos relative to planet origin

				float3 worldPos = i.camPosRelPlanet .xyz + worldViewDir * abs(mid); //worldPos relative to planet origin


//				return float4(length(worldPos - i.camPosRelPlanet) / 10000.0, 0.0, 0.0, 1.0);

				float3 groundPos = normalize (worldPos) * Rg*1.0008;
				float Rt2 = Rg + (Rt - Rg) * _experimentalAtmoScale;

				worldPos = (length(worldPos) < Rt2) ? lerp(groundPos,worldPos,_PlanetOpacity) : worldPos; //fades to flatScaledSpace planet shading to ease the transition to scaledSpace
				//this wasn't applied in extinction shader, not sure if it will be an issue

				worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix

				float3 backGrnd = tex2Dlod(ScattererScreenCopy, float4(uv,0.0,0.0));

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


				inscatter+= InScattering2(i.camPosRelPlanet, worldPos,SUN_DIR,extinction);
				inscatter*= (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ; //somehow the shader compiler for OpenGL behaves differently around braces

//				#if defined (ECLIPSES_ON)				
//				 				float eclipseShadow = 1;
//				 							
//				            	for (int i=0; i<4; ++i)
//				    			{
//				        			if (lightOccluders1[i].w <= 0)	break;
//									eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
//												   lightOccluders1[i].w, sunPosAndRadius.w)	;
//								}
//										
//								for (int j=0; j<4; ++j)
//				    			{
//				        			if (lightOccluders2[j].w <= 0)	break;
//									eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
//												   lightOccluders2[j].w, sunPosAndRadius.w)	;
//								}
//				
//								inscatter*=eclipseShadow;
//				#endif

				inscatter = hdr(inscatter,_ScatteringExposure) *_global_alpha;

				//composite background with inscatter, soft-blend it
				backGrnd+= (1.0 - backGrnd) * dither(inscatter,screenPos);

				return float4(backGrnd,1.0);
			}
			ENDCG
		}
	}

}