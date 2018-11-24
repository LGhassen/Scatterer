Shader "Scatterer/ScaledPlanetScattering" {
    SubShader 
    {
        Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
		
        Pass {  //extinction pass

    		ZWrite Off
    		ZTest LEqual
    		Cull Back

    		Offset -1, -1

            Blend DstColor Zero //multiplicative

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonAtmosphere.cginc"
			#include "../EclipseCommon.cginc"
			#include "../RingCommon.cginc"		
			
			#define useAnalyticSkyTransmittance
			
			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON

			uniform float _Alpha_Global;
			uniform float _Extinction_Tint;
			uniform float extinctionMultiplier;
			uniform float3 _Sun_WorldSunDir;

			uniform float renderScattering;
			uniform float flatScaledSpaceModel;

            struct v2f
            {
                float4 pos: SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 planetOrigin: TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(v.vertex);
                OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
                OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz;
                OUT.pos = (renderScattering == 1.0) ? OUT.pos : 0.0;
                return OUT;
            }
            
            half4 frag(v2f IN): COLOR
            {
                float3 extinction=0.0;

                float3 WCP = _WorldSpaceCameraPos ;

                float3 planetSurfacePosition = IN.worldPos-IN.planetOrigin;
                float3 planetSurfaceScatteringPosition = (flatScaledSpaceModel == 1.0) ? normalize(planetSurfacePosition) * Rg * 1.0001 : planetSurfacePosition * 6005; //transform to scaledspace here,
                																																						//6005 instead of 6000 due to precision issues, same with 1.0008
                extinction = getExtinction((WCP-IN.planetOrigin)*6000, planetSurfaceScatteringPosition, 1.0, 1.0, 1.0);

#if defined (ECLIPSES_ON)	
				extinction*= getEclipseShadows(IN.worldPos*6000);
#endif

#if defined (RINGSHADOW_ON)
				extinction *= getRingShadow(IN.worldPos*6000, _Sun_WorldSunDir, IN.planetOrigin*6000);
#endif

                return float4(extinction,1.0);
            }
            ENDCG
        }

		Pass {  //scattering pass

    		ZWrite Off
    		ZTest LEqual
    		Cull Back

    		Offset -1, -1

            Blend OneMinusDstColor One //soft additive

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonAtmosphere.cginc"
			#include "../EclipseCommon.cginc"
			#include "../RingCommon.cginc"	
			
			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON

			//#pragma fragmentoption ARB_precision_hint_nicest
			
			uniform float _Alpha_Global;

			uniform float3 _Sun_WorldSunDir;
			uniform float _ScatteringExposure;

			uniform float renderScattering;

#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif

			//sampler2D _MainTex;
			uniform float flatScaledSpaceModel;

            struct v2f
            {
                float4 pos: SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 planetOrigin: TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(v.vertex);

                OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
                OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz;

                OUT.pos = (renderScattering == 1.0) ? OUT.pos : 0.0;
                return OUT;
            }
            
            half4 frag(v2f IN): COLOR
            {
				float3 inscatter=0.0;
                float3 extinction=0.0;

                float3 WCP = _WorldSpaceCameraPos;

                float3 planetSurfacePosition = IN.worldPos-IN.planetOrigin;
                float3 planetSurfaceScatteringPosition = (flatScaledSpaceModel == 1.0) ? normalize(planetSurfacePosition) * Rg * 1.0001 : planetSurfacePosition * 6005; //transform to scaledspace here,
                																																						//6005 instead of 6000 due to precision issues, same with 1.0008
                inscatter= InScattering2((WCP-IN.planetOrigin)*6000, planetSurfaceScatteringPosition, extinction, _Sun_WorldSunDir, 1.0, 1.0, 1.0);

#if defined (ECLIPSES_ON)
				inscatter *= getEclipseShadows(IN.worldPos*6000);
#endif

#if defined (RINGSHADOW_ON)
				inscatter *= getRingShadow(IN.worldPos*6000, _Sun_WorldSunDir, IN.planetOrigin*6000);
#endif

///////////////////PLANETSHINE///////////////////////////////						    
//#if defined (PLANETSHINE_ON)
//			    float3 inscatter2=0;
//			   	float intensity=1;
//			    for (int i=0; i<4; ++i)
//    			{
//    				if (planetShineRGB[i].w == 0) break;
//    					
//    				//if source is not a sun compute intensity of light from angle to light source
//			   		intensity=1;  
//			   		if (planetShineSources[i].w != 1.0f)
//					{
////						intensity = 0.5f*(1-dot(normalize(planetShineSources[i].xyz - worldPos),WSD));
//						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[i].xyz - planetSurfacePosition),WSD)),0);
//					}
//				    				
//    				inscatter2+=SkyRadiance3(WCP - IN.planetOrigin, d, normalize(planetShineSources[i].xyz))
//    							*planetShineRGB[i].xyz*planetShineRGB[i].w*intensity;
//    			}
//			    
//				finalColor+=inscatter2;
//#endif
/////////////////////////////////////////////////////////////	

                return float4(hdr(inscatter,_ScatteringExposure),1.0);
            }
            ENDCG
        }

    }
}