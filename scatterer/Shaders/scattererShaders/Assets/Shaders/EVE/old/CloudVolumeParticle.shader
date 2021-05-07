// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "EVE/CloudVolumeParticle" {
	Properties {
		_TopTex("Particle Texture", 2D) = "white" {}
		_LeftTex("Particle Texture", 2D) = "white" {}
		_FrontTex("Particle Texture", 2D) = "white" {}
		_MainTex("Main (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_DetailScale("Detail Scale", Range(0,1000)) = 100
		_DistFade("Distance Fade Near", Range(0,1)) = 1.0
		_DistFadeVert("Distance Fade Vertical", Range(0,1)) = 0.004
		_Color("Color Tint", Color) = (1,1,1,1)
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = .01
		_Rotation("Rotation", Float) = 0
		_MaxScale("Max Scale", Float) = 1
		_MaxTrans("Max Translation", Vector) = (0,0,0)
		_NoiseScale("Noise Scale", Vector) = (1,2,.0005)
	}

	Category {

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode Global}
		AlphaTest Greater 0
		ColorMask RGB
//		Cull Back Lighting On ZWrite Off
		Cull Back Lighting On ZWrite On

		SubShader {
			Pass {

				Lighting On
				Tags { "LightMode"="ForwardBase"}

				CGPROGRAM
				#include "EVEUtils.cginc"
				#include "noiseSimplex.cginc"
				#pragma target 3.0
				#pragma glsl
				#pragma vertex vert
				#pragma fragment frag
				#define MAG_ONE 1.4142135623730950488016887242097
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase
//				#pragma multi_compile SOFT_DEPTH_OFF SOFT_DEPTH_ON
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

				sampler2D _TopTex;
				sampler2D _LeftTex;
				sampler2D _FrontTex;

				sampler2D _DetailTex;
				float _DetailScale;
				fixed4 _Color;
				float _DistFade;
				float _DistFadeVert;
				float _InvFade;
				float _Rotation;
				float _MaxScale;
				float3 _NoiseScale;
				float3 _MaxTrans;

				sampler2D _CameraDepthTexture;

				uniform float _GlobalOceanAlpha;
				uniform float3 _PlanetOrigin;
				uniform float3 _Sun_WorldSunDir;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					fixed4 color : COLOR;
					float3 viewDir : TEXCOORD0;
					float2 texcoordZY : TEXCOORD1;
					float2 texcoordXZ : TEXCOORD2;
					float2 texcoordXY : TEXCOORD3;
					float4 projPos : TEXCOORD4;
					float3 planetPos : TEXCOORD5;
					float3 worldVert : TEXCOORD6;
//					float3 inscatter : TEXCOORD6;
//					float3 extinction : TEXCOORD7;
					//LIGHTING_COORDS(5,6)

				};

				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);

					float4 origin = mul(unity_ObjectToWorld, float4(0,0,0,1));

					float4 planet_pos = mul(_MainRotation, origin);
					float3 normalized = _NoiseScale.z*(planet_pos.xyz);
					float3 hashVect =  .5*(float3(snoise(normalized), snoise(_NoiseScale.x*normalized), snoise(_NoiseScale.y*normalized))+1);

					float4 localOrigin;
					localOrigin.xyz = (2*hashVect-1)*_MaxTrans;
					localOrigin.w = 1;
					float localScale = (hashVect.x*(_MaxScale-1))+1;


					origin = mul(unity_ObjectToWorld, localOrigin);

					planet_pos = mul(_MainRotation, origin);
					float3 detail_pos = mul(_DetailRotation, planet_pos).xyz;
					o.planetPos = planet_pos.xyz;
					o.color = half4(1, 1, 1, 1);
					//o.color = GET_NO_LOD_CUBE_MAP_1(_MainTex, planet_pos.xyz);
					//o.color = ALPHA_COLOR_1(o.color);

					o.color.rgba *= GetCubeDetailMapNoLOD(_DetailTex, detail_pos, _DetailScale);

					o.color.a *= GetDistanceFade(distance(origin,_WorldSpaceCameraPos), _DistFade, _DistFadeVert);

					float4x4 M = rand_rotation(
					(float3(frac(_Rotation),0,0))+hashVect,
					localScale,
					localOrigin.xyz);
					float4x4 mvMatrix = mul(mul(UNITY_MATRIX_V, unity_ObjectToWorld), M);

					float3 viewDir = normalize(mvMatrix[2].xyz);
					o.viewDir = abs(viewDir);


					float4 mvCenter = mul(UNITY_MATRIX_MV, localOrigin);
					o.pos = mul(UNITY_MATRIX_P,
					mvCenter
					+ float4(v.vertex.xyz*localScale,v.vertex.w));

					float2 texcoodOffsetxy = ((2*v.texcoord)- 1);
					float4 texcoordOffset = float4(texcoodOffsetxy.x, texcoodOffsetxy.y, 0, v.vertex.w);

					float4 ZYv = texcoordOffset.zyxw;
					float4 XZv = texcoordOffset.xzyw;
					float4 XYv = texcoordOffset.xyzw;

					ZYv.z*=sign(-viewDir.x);
					XZv.x*=sign(-viewDir.y);
					XYv.x*=sign(viewDir.z);

					ZYv.x += sign(-viewDir.x)*sign(ZYv.z)*(viewDir.z);
					XZv.y += sign(-viewDir.y)*sign(XZv.x)*(viewDir.x);
					XYv.z += sign(-viewDir.z)*sign(XYv.x)*(viewDir.x);

					ZYv.x += sign(-viewDir.x)*sign(ZYv.y)*(viewDir.y);
					XZv.y += sign(-viewDir.y)*sign(XZv.z)*(viewDir.z);
					XYv.z += sign(-viewDir.z)*sign(XYv.y)*(viewDir.y);

					float2 ZY = mul(mvMatrix, ZYv).xy - mvCenter.xy;
					float2 XZ = mul(mvMatrix, XZv).xy - mvCenter.xy;
					float2 XY = mul(mvMatrix, XYv).xy - mvCenter.xy;

					o.texcoordZY = half2(.5 ,.5) + .6*(ZY);
					o.texcoordXZ = half2(.5 ,.5) + .6*(XZ);
					o.texcoordXY = half2(.5 ,.5) + .6*(XY);

					//TRANSFER_VERTEX_TO_FRAGMENT(o);

					float3 worldNormal = normalize(mul( unity_ObjectToWorld, float4( v.normal, 0.0 ) ).xyz);
					viewDir = normalize(origin - _WorldSpaceCameraPos);
					half4 color = SpecularColorLight( _WorldSpaceLightPos0, viewDir, worldNormal, o.color, 0, 0, 1 );
					color *= Terminator( normalize(_WorldSpaceLightPos0), worldNormal);
					o.color.rgb = color.rgb;
					
					float4 vertexPos = mul(unity_ObjectToWorld, v.vertex);
//					float3 worldVert = vertexPos.xyz;
					o.worldVert = vertexPos.xyz;
					
//					float3 extinction = float3(0, 0, 0);
//					
////                	float3 inscatter = InScattering2(_camPos, worldPos, extinction, 1.0, 1.0, 1.0);
//					float3 inscatter = InScattering2(_WorldSpaceCameraPos.xyz-_PlanetOrigin, worldVert-_PlanetOrigin, extinction, 1.0, 1.0, 1.0);
//					
////					float3 inscatter = SkyRadiance2(_WorldSpaceCameraPos.xyz-_PlanetOrigin, normalize(worldVert-_WorldSpaceCameraPos.xyz), _Sun_WorldSunDir,extinction);
//                	extinction = getExtinction(_WorldSpaceCameraPos.xyz-_PlanetOrigin, worldVert-_PlanetOrigin, 1.0, 1.0, 1.0);
//					
//					o.inscatter=inscatter;
//					o.extinction=extinction;
//#ifdef SOFT_DEPTH_ON
//					o.projPos = ComputeScreenPos (o.pos);
//					COMPUTE_EYEDEPTH(o.projPos.z);
//#endif

					return o;
				}

				fixed4 frag (v2f IN) : COLOR
				{

					half xval = IN.viewDir.x;
					half4 xtex = tex2D(_LeftTex, IN.texcoordZY);
					half yval = IN.viewDir.y;
					half4 ytex = tex2D(_TopTex, IN.texcoordXZ);
					half zval = IN.viewDir.z;
					half4 ztex = tex2D(_FrontTex, IN.texcoordXY);

					//half4 tex = (xtex*xval)+(ytex*yval)+(ztex*zval);
					half4 tex = lerp(lerp(xtex, ytex, yval), ztex, zval);

					half4 prev = GET_NO_LOD_CUBE_MAP_1(_MainTex, IN.planetPos);
					prev = ALPHA_COLOR_1(prev);

					prev *= .94*_Color * IN.color * tex;


					half4 color;
					color.rgb = prev.rgb;
					color.a = prev.a;
					
					if (color.a <= 0.1) discard;

//#ifdef SOFT_DEPTH_ON
//					float depth = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
//					depth = LinearEyeDepth (depth);
//					float partZ = IN.projPos.z;
//					float fade = saturate (_InvFade * (depth-partZ));
//					color.a *= fade;
//#endif

//#ifdef SCATTERER_ON
					float3 extinction = float3(0, 0, 0);
					
//                	float3 inscatter = InScattering2(_camPos, worldPos, extinction, 1.0, 1.0, 1.0);
//					float3 inscatter = InScattering2(_WorldSpaceCameraPos.xyz-_PlanetOrigin, IN.worldVert-_PlanetOrigin, extinction, 1.0, 1.0, 1.0);
					
					float3 inscatter = SkyRadiance2(_WorldSpaceCameraPos.xyz-_PlanetOrigin, normalize(IN.worldVert-_WorldSpaceCameraPos.xyz), _Sun_WorldSunDir,extinction);
                	extinction = getExtinction(_WorldSpaceCameraPos.xyz-_PlanetOrigin, IN.worldVert-_PlanetOrigin, 1.0, 1.0, 1.0);
                	
                	
                	color = float4(hdr(_GlobalOceanAlpha*color.rgb*extinction+inscatter), 1.0);
//                	color = float4(hdr(_GlobalOceanAlpha*color.rgb*IN.extinction+IN.inscatter), color.a);
//                	color = float4(hdr(inscatter), color.a);
//#endif

					return color;
				}
				ENDCG
			}

		}

	}
}