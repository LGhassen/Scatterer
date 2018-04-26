Shader "Scatterer-EVE/Cloud" {
	Properties{
		_Color("Color Tint", Color) = (1,1,1,1)
		_MainTex("Main (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_UVNoiseTex("UV Noise (RG)", 2D) = "black" {}
		_FalloffPow("Falloff Power", Range(0,3)) = 2
		_FalloffScale("Falloff Scale", Range(0,20)) = 3
		_DetailScale("Detail Scale", Range(0,100)) = 100
		_DetailDist("Detail Distance", Range(0,1)) = 0.00875
		_UVNoiseScale("UV Noise Scale", Range(0,0.1)) = 0.01
		_UVNoiseStrength("UV Noise Strength", Range(0,0.1)) = 0.002
		_UVNoiseAnimation("UV Noise Animation", Vector) = (0.002,0.001,0)
		_UniversalTime("Universal Time", Vector) = (0,0,0,0)
		_MinLight("Minimum Light", Range(0,1)) = 0
		_DistFade("Fade Distance", Range(0,100)) = 10
		_DistFadeVert("Fade Scale", Range(0,1)) = .002
		_RimDist("Rim Distance", Range(0,1)) = 1
		_RimDistSub("Rim Distance Sub", Range(0,2)) = 1.01
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = .01
		_OceanRadius("Ocean Radius", Float) = 63000
		_PlanetOrigin("Sphere Center", Vector) = (0,0,0,1)
		_DepthPull("Depth Augment", Float) = .99
		_SunPos("_SunPos", Vector) = (0,0,0)
		_SunRadius("_SunRadius", Float) = 1
	}

	Category{

		Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Fog { Mode Global}
		//AlphaTest Greater 0
		//ColorMask RGB
		Cull Off
		Lighting On
		ZWrite Off

		SubShader {
			Pass {

				Lighting On
				Tags { "LightMode" = "ForwardBase"}

				CGPROGRAM


				#include "EVEUtils.cginc"
				#pragma target 3.0
				#pragma glsl
				#pragma vertex vert
				#pragma fragment frag
				#define MAG_ONE 1.4142135623730950488016887242097
//				#pragma multi_compile_fwdbase
				#pragma multi_compile SOFT_DEPTH_OFF SOFT_DEPTH_ON
				#pragma multi_compile WORLD_SPACE_OFF WORLD_SPACE_ON
				#pragma multi_compile MAP_TYPE_1 MAP_TYPE_CUBE_1 MAP_TYPE_CUBE2_1 MAP_TYPE_CUBE6_1

				//scatterer eclipses and ring shadows
				#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
//				#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
				#pragma multi_compile PRESERVECLOUDCOLORS_OFF PRESERVECLOUDCOLORS_ON
				#pragma multi_compile SCATTERER_OFF SCATTERER_ON
				
#ifndef MAP_TYPE_CUBE2_1
#pragma multi_compile ALPHAMAP_N_1 ALPHAMAP_1
#endif
				#include "alphaMap.cginc"
				#include "cubeMap.cginc"
				
#ifdef SCATTERER_ON
				#include "AtmosphereScatterer.cginc"
#endif

				CUBEMAP_DEF_1(_MainTex)

				sampler2D _DetailTex;
				sampler2D _UVNoiseTex;
				fixed4 _Color;
				float _FalloffPow;
				float _FalloffScale;
				float _DetailScale;
				float _DetailDist;

				float _UVNoiseScale;
				float _UVNoiseStrength;
				float2 _UVNoiseAnimation;

				float _MinLight;
				float _DistFade;
				float _DistFadeVert;
				float _RimDist;
				float _RimDistSub;
				float _OceanRadius;
				float _InvFade;
				float3 _PlanetOrigin;
				sampler2D _CameraDepthTexture;
				float _DepthPull;

#if defined (SCATTERER_ON)
				uniform float cloudColorMultiplier;
				uniform float cloudScatteringMultiplier;
				uniform float cloudSkyIrradianceMultiplier;
				uniform float3 _Sun_WorldSunDir;
				uniform float3 _Scatterer_Origin;
#endif

				//scatterer eclipse uniforms
#if defined (SCATTERER_ON) && defined (ECLIPSES_ON)
				uniform float4 sunPosAndRadius; //xyz sun pos w radius
				uniform float4x4 lightOccluders1; //array of light occluders
											 //for each float4 xyz pos w radius
				uniform float4x4 lightOccluders2;
#endif

//			//stuff for kopernicus ring shadows
//#if defined (SCATTERER_ON) && defined (RINGSHADOW_ON)	
//			uniform sampler2D ringTexture;
//			uniform float ringInnerRadius;
//			uniform float ringOuterRadius;
//			uniform float3 ringNormal;
//#endif

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float3 normal : NORMAL;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldVert : TEXCOORD0;
					float3 L : TEXCOORD1;
					float4 objDetail : TEXCOORD2;
					float4 objMain : TEXCOORD3;
					float3 worldNormal : TEXCOORD4;
					float3 viewDir : TEXCOORD5;
					LIGHTING_COORDS(6,7)
					float4 projPos : TEXCOORD8;
#if defined (SCATTERER_ON)
					float3 worldOrigin: TEXCOORD9;
#endif					
				};


				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					o.pos = UnityObjectToClipPos(v.vertex);

					float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
					float3 origin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
					o.worldVert = vertexPos;
					o.worldNormal = normalize(vertexPos - origin);
					o.objMain = mul(_MainRotation, v.vertex);
					o.objDetail = mul(_DetailRotation, o.objMain);
					o.viewDir = normalize(WorldSpaceViewDir(v.vertex));

					o.projPos = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.projPos.z);
					TRANSFER_VERTEX_TO_FRAGMENT(o);

					o.L = _PlanetOrigin - _WorldSpaceCameraPos.xyz;
#if defined (SCATTERER_ON)
					o.worldOrigin = origin;
#endif

					return o;
				}

				struct fout {
					float4 color : COLOR;
					float depth : DEPTH;
				};

				fout frag(v2f IN)
				{
					fout OUT;
					float4 color;
					float4 main;

					main = GET_CUBE_MAP_P(_MainTex, IN.objMain.xyz, _UVNoiseTex, _UVNoiseScale, _UVNoiseStrength, _UVNoiseAnimation);
					main = ALPHA_COLOR_1(main);

					float4 detail = GetCubeDetailMap(_DetailTex, IN.objDetail, _DetailScale);

					float viewDist = distance(IN.worldVert,_WorldSpaceCameraPos);
					half detailLevel = saturate(2 * _DetailDist*viewDist);
					color = _Color * main.rgba * lerp(detail.rgba, 1, detailLevel);

					float rim = saturate(abs(dot(IN.viewDir, IN.worldNormal)));
					rim = saturate(pow(_FalloffScale*rim,_FalloffPow));
					float dist = distance(IN.worldVert,_WorldSpaceCameraPos);
					float distLerp = saturate(_RimDist*(distance(_PlanetOrigin,_WorldSpaceCameraPos) - _RimDistSub*distance(IN.worldVert,_PlanetOrigin)));
					float distFade = 1 - GetDistanceFade(dist, _DistFade, _DistFadeVert);
					float distAlpha = lerp(distFade, rim, distLerp);

					color.a = lerp(0, color.a, distAlpha);

					//suspect

#ifdef WORLD_SPACE_ON
					float3 worldDir = normalize(IN.worldVert - _WorldSpaceCameraPos.xyz);
					float tc = dot(IN.L, worldDir);
					float d = sqrt(dot(IN.L,IN.L) - (tc*tc));
					float3 norm = normalize(-IN.L);
					float d2 = pow(d,2);
					float td = sqrt(dot(IN.L,IN.L) - d2);
					float tlc = sqrt((_OceanRadius*_OceanRadius) - d2);

					half sphereCheck = saturate(step(d, _OceanRadius)*step(0.0, tc) + step(length(IN.L), _OceanRadius));
					float sphereDist = lerp(tlc - td, tc - tlc, step(0.0, tc));
					sphereCheck *= step(sphereDist, dist);

					color.a *= 1 - sphereCheck;
#endif





//SCATTERER_OFF
#if !defined (SCATTERER_ON)
					//lighting
					half transparency = color.a;
					float4 scolor = SpecularColorLight(_WorldSpaceLightPos0, IN.viewDir, IN.worldNormal, color, 0, 0, LIGHT_ATTENUATION(IN));
					scolor *= Terminator(normalize(_WorldSpaceLightPos0), IN.worldNormal);
					scolor.a = transparency;

	#ifdef SOFT_DEPTH_ON
					float depth = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
					depth = LinearEyeDepth(depth);
					float partZ = IN.projPos.z;
					float fade = saturate(_InvFade * (depth - partZ));
					scolor.a *= fade;
	#endif

					//scolor.rgb *= MultiBodyShadow(IN.worldVert, _SunRadius, _SunPos, _ShadowBodies); //causes artifacts idk why
					OUT.color = lerp(scolor, color, _MinLight);
//SCATTERER_ON
#else 
					float4 texColor = color;
					float3 extinction = float3(0, 0, 0);

	#ifdef WORLD_SPACE_ON
					float3 WCP = _WorldSpaceCameraPos; //unity supplied, in local Space
					float3 worldPos = IN.worldVert;
					float3 worldOrigin = IN.worldOrigin;
	#else
			    	float3 WCP = _WorldSpaceCameraPos * 6000; //unity supplied, converted from ScaledSpace to localSpace coords
					float3 worldPos = IN.worldVert * 6000;
					float3 worldOrigin = IN.worldOrigin * 6000;
	#endif

					float3 relWorldPos=worldPos-worldOrigin;
					float alt = length(relWorldPos);
					float threshold = Rg * 1.00333333;

					relWorldPos = (alt < threshold) ? normalize(relWorldPos) * (threshold) : relWorldPos;   //artifacts fix (black scattering and overbright skyirradiance) when cloud altitude < Rg *( 1 + 2000/600000)

					float3 relCameraPos=WCP-worldOrigin;

                	//inScattering from cloud to observer
					float3 inscatter = InScattering2(relCameraPos, relWorldPos, extinction, _Sun_WorldSunDir, 1.0, 1.0, 1.0);
					      	
                	//extinction from cloud to observer
					extinction = getExtinction(relCameraPos, relWorldPos, 1.0, 1.0, 1.0);

					//extinction of light from sun to cloud
					extinction*=getSkyExtinction(relWorldPos,_Sun_WorldSunDir);					

					//skyLight
					float3 skyE = SimpleSkyirradiance(relWorldPos, IN.viewDir, _Sun_WorldSunDir);
	#if defined (PRESERVECLOUDCOLORS_OFF)
					color = float4(hdrNoExposure(color.rgb*cloudColorMultiplier*extinction+ inscatter*cloudScatteringMultiplier+skyE*cloudSkyIrradianceMultiplier), color.a); //not bad
					//color = float4(hdrNoExposure(color.rgb*cloudColorMultiplier*extinction*skyE*cloudSkyIrradianceMultiplier+ inscatter*cloudScatteringMultiplier), color.a); //not bad
	#else
					float3 cloudColor = color.rgb*cloudColorMultiplier*extinction*hdrNoExposure(skyE * cloudSkyIrradianceMultiplier);
					//float3 cloudColor = color.rgb*cloudColorMultiplier*extinction;
					//float3 otherColors = hdrNoExposure(inscatter * cloudScatteringMultiplier + skyE * cloudSkyIrradianceMultiplier);
					float3 otherColors = hdrNoExposure(inscatter * cloudScatteringMultiplier);
					
					color = float4(cloudColor + (float3(1.0,1.0,1.0)-cloudColor)*otherColors, color.a); //basically soft blend
	#endif					

/////////////////ECLIPSES///////////////////////////////		
	#if defined (ECLIPSES_ON)				
 					float eclipseShadow = 1; 						

					for (int i=0; i<4; ++i)
    				{
        				if (lightOccluders1[i].w <= 0)	break;
						eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
//						eclipseShadow*=getEclipseShadow(normalize(relWorldPos)*Rt+_Scatterer_Origin, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
							lightOccluders1[i].w, sunPosAndRadius.w)	;
					}
						
					for (int j=0; j<4; ++j)
    				{
        				if (lightOccluders2[j].w <= 0)	break;
						eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
//						eclipseShadow*=getEclipseShadow(normalize(relWorldPos)*Rt+_Scatterer_Origin, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
							lightOccluders2[j].w, sunPosAndRadius.w)	;
					}

					color.rgb*=eclipseShadow;
	#endif

///////////////////RING SHADOWS///////////////////////////////			
//#if defined (RINGSHADOW_ON)
//				//raycast from atmo to ring plane and find intersection			
//				float3 ringIntersectPt = LinePlaneIntersection(worldPos, _Sun_WorldSunDir, ringNormal, _Scatterer_Origin);
//
//				//draw ring shadow at Rt height for now to coincide with atmo shading
//				//float3 ringIntersectPt = LinePlaneIntersection(normalize(relWorldPos)*Rt, _Sun_WorldSunDir, ringNormal, float3(0,0,0));
//
//				//calculate ring texture position on intersect
//				float distance = length (ringIntersectPt - _Scatterer_Origin);
//				float ringTexturePosition = (distance - ringInnerRadius) / (ringOuterRadius - ringInnerRadius); //inner and outer radiuses need are converted to local space coords on plugin side
//				ringTexturePosition = 1 - ringTexturePosition; //flip to match UVs			
//
////				//read 1-alpha of ring texture
////				float ringShadow = 1- (tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition))).a;
//
//				float4 ringColor = tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition));
//				float ringShadow = (1-ringColor.a)*((ringColor.x+ringColor.y+ringColor.z)*0.33334);
//
//				//don't apply any shadows if intersect point is not between inner and outer radius
//				ringShadow = (ringTexturePosition > 1 || ringTexturePosition < 0 ) ? 1 : ringShadow;
//
//				color.rgb*=ringShadow;
//#endif

//					OUT.color = lerp(scolor, color, _MinLight);					
//					color.rgb*= MultiBodyShadow(IN.worldVert, _SunRadius, _SunPos, _ShadowBodies); //causes artifacts with SVE for some reason


					OUT.color = lerp(color, texColor, _MinLight);
//endif SCATTERER_ON
#endif
					
					float depthWithOffset = IN.projPos.z;
#ifndef WORLD_SPACE_ON
					depthWithOffset *= _DepthPull;
					OUT.color.a *= step(0, dot(IN.viewDir, IN.worldNormal));
#endif
					OUT.depth = (1.0 - depthWithOffset * _ZBufferParams.w) / (depthWithOffset * _ZBufferParams.z);
					return OUT;
				}
				ENDCG

			}

		}

	}
}