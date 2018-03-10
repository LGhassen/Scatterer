// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "EVE/Cloud" {
	Properties{
		_Color("Color Tint", Color) = (1,1,1,1)
		_MainTex("Main (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_FalloffPow("Falloff Power", Range(0,3)) = 2
		_FalloffScale("Falloff Scale", Range(0,20)) = 3
		_DetailScale("Detail Scale", Range(0,100)) = 100
		_DetailDist("Detail Distance", Range(0,1)) = 0.00875
		_MinLight("Minimum Light", Range(0,1)) = .5
		_DistFade("Fade Distance", Range(0,100)) = 10
		_DistFadeVert("Fade Scale", Range(0,1)) = .002
		_RimDist("Rim Distance", Range(0,1)) = 1
		_RimDistSub("Rim Distance Sub", Range(0,2)) = 1.01
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = .01
		_OceanRadius("Ocean Radius", Float) = 63000
		_PlanetOrigin("Sphere Center", Vector) = (0,0,0,1)
	}

	Category{

		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode Global}
		AlphaTest Greater 0
		ColorMask RGB
		Cull Off Lighting On ZWrite Off

		SubShader {
			Pass {

				Lighting On
//				Tags { "LightMode" = "ForwardBase"}

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
#ifndef MAP_TYPE_CUBE2_1
				#pragma multi_compile ALPHAMAP_N_1 ALPHAMAP_R_1 ALPHAMAP_G_1 ALPHAMAP_B_1 ALPHAMAP_A_1
#endif
				#include "alphaMap.cginc"
				#include "cubeMap.cginc"
				
//				#define SCATTERER_ON
				
//#ifdef SCATTERER_ON
				#include "AtmosphereScatterer.cginc"
//				#include "AtmosphereNew.cginc"
//#endif

				CUBEMAP_DEF(_MainTex)

				sampler2D _DetailTex;
				fixed4 _Color;
				float _FalloffPow;
				float _FalloffScale;
				float _DetailScale;
				float _DetailDist;
				float _MinLight;
				float _DistFade;
				float _DistFadeVert;
				float _RimDist;
				float _RimDistSub;
				float _OceanRadius;
				float _InvFade;
				float3 _PlanetOrigin;
				sampler2D _CameraDepthTexture;
				
				uniform float _GlobalOceanAlpha;
				uniform float3 _Sun_WorldSunDir;
				

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
				};


				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
					o.pos = UnityObjectToClipPos(v.vertex);

					float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
					float3 origin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
					o.worldVert = vertexPos;
					o.worldNormal = normalize(vertexPos - origin);
					o.objMain = mul(_MainRotation, v.vertex);
					o.objDetail = mul(_DetailRotation, o.objMain);
					o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
#ifdef SOFT_DEPTH_ON
					o.projPos = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.projPos.z);
					TRANSFER_VERTEX_TO_FRAGMENT(o);
#endif

					o.L = _PlanetOrigin - _WorldSpaceCameraPos.xyz;

					return o;
				}



				fixed4 frag(v2f IN) : COLOR
				{
					half4 color;
					half4 main;

					main = GET_CUBE_MAP_1(_MainTex, IN.objMain);
					main = ALPHA_COLOR_1(main);

					half4 detail = GetCubeDetailMap(_DetailTex, IN.objDetail, _DetailScale);

					float viewDist = distance(IN.worldVert,_WorldSpaceCameraPos);
					half detailLevel = saturate(2 * _DetailDist*viewDist);
					color = _Color * main.rgba * lerp(detail.rgba, 1, detailLevel);

					float rim = saturate(dot(IN.viewDir, IN.worldNormal));
					rim = saturate(pow(_FalloffScale*rim,_FalloffPow));
					float dist = distance(IN.worldVert,_WorldSpaceCameraPos);
					float distLerp = saturate(_RimDist*(distance(_PlanetOrigin,_WorldSpaceCameraPos) - _RimDistSub*distance(IN.worldVert,_PlanetOrigin)));
					float distFade = 1 - GetDistanceFade(dist, _DistFade, _DistFadeVert);
					float distAlpha = lerp(distFade, rim, distLerp);

					color.a = lerp(0, color.a, distAlpha);


//#ifdef WORLD_SPACE_ON
//					half3 worldDir = normalize(IN.worldVert - _WorldSpaceCameraPos.xyz);
//					float tc = dot(IN.L, worldDir);
//					float d = sqrt(dot(IN.L,IN.L) - (tc*tc));
//					float3 norm = normalize(-IN.L);
//					float d2 = pow(d,2);
//					float td = sqrt(dot(IN.L,IN.L) - d2);
//					float tlc = sqrt((_OceanRadius*_OceanRadius) - d2);
//
//					half sphereCheck = saturate(step(d, _OceanRadius)*step(0.0, tc) + step(length(IN.L), _OceanRadius));
//					float sphereDist = lerp(tlc - td, tc - tlc, step(0.0, tc));
//					sphereCheck *= step(sphereDist, dist);
//
//					color.a *= 1 - sphereCheck;
//#endif

//					//lighting
//					half transparency = color.a;
//					color = SpecularColorLight(_WorldSpaceLightPos0, IN.viewDir, IN.worldNormal, color, 0, 0, LIGHT_ATTENUATION(IN));
//					color *= Terminator(normalize(_WorldSpaceLightPos0), IN.worldNormal);
//					color.a = transparency;
#ifdef SOFT_DEPTH_ON
					float depth = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
					depth = LinearEyeDepth(depth);
					float partZ = IN.projPos.z;
					float fade = saturate(_InvFade * (depth - partZ));
					color.a *= fade;
#endif

//#ifdef SCATTERER_ON
					float3 extinction = float3(0, 0, 0);

					float3 WCP=_WorldSpaceCameraPos;
					float3 worldPos=IN.worldVert;

                	



                	
//                	//inScattering from cloud to observer
					float3 inscatter = InScattering2(WCP-_PlanetOrigin, worldPos-_PlanetOrigin, extinction, 1.0, 1.0, 1.0);
//					float3 inscatter = SkyRadiance2(_WorldSpaceCameraPos.xyz-_PlanetOrigin, normalize(IN.worldVert-_WorldSpaceCameraPos.xyz), _Sun_WorldSunDir,extinction);
//                	
//                	//extinction from cloud to observer
                	extinction = getExtinction(WCP-_PlanetOrigin, worldPos-_PlanetOrigin, 1.0, 1.0, 1.0);
//                	extinction = 1.0;

                	
                	//extinction of light from sun to cloud
//                	float3 sunExtinction = getSkyExtinction(IN.worldVert-_PlanetOrigin, _Sun_WorldSunDir); //_Sun_WorldSunDir correct?
//                	float3 sunExtinction = getExtinction(_WorldSpaceCameraPos.xyz-_PlanetOrigin, normalize(IN.worldVert-_WorldSpaceCameraPos.xyz)); //_Sun_WorldSunDir correct?
//                	float3 sunExtinction = getSkyExtinction(Rg*normalize(IN.worldVert-_PlanetOrigin), _Sun_WorldSunDir);
//                	float3 sunExtinction = getSkyExtinction(worldPos-_PlanetOrigin, _Sun_WorldSunDir);
                	worldPos=  (dot (WCP-worldPos,WCP-_PlanetOrigin) > 0.0 ) ? Rg*normalize(IN.worldVert-_PlanetOrigin) : worldPos-_PlanetOrigin;
					float3 sunExtinction = getSkyExtinction(worldPos, _Sun_WorldSunDir);
//					float3 sunExtinction = 1.0;
                	
//                	color = float4(hdr(_GlobalOceanAlpha*sunExtinction*color.rgb*extinction+inscatter), color.a);
                	color = float4(hdr(_GlobalOceanAlpha*sunExtinction*extinction+inscatter), color.a);
//                	color = float4(hdr(sunExtinction+_GlobalOceanAlpha), color.a);
//                	color = float4(hdr(inscatter), color.a);
//                	color = float4(hdr(inscatter), color.a);
//#endif
					
					return color;
				}
				ENDCG

			}

		}

	}
}