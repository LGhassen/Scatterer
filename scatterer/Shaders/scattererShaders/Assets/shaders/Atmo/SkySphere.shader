Shader "Scatterer/SkySphere" 
{
	SubShader 
	{
		Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }

		Pass   	//extinction pass, I should really just put the shared components in an include file to clean this up
		{		
			Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }    	 	 		

			ZWrite Off
			cull Front

			Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonAtmosphere.cginc"

			uniform float _Alpha_Global;
			uniform float _Extinction_Tint;
			uniform float extinctionMultiplier;

			uniform float _experimentalExtinctionScale;
			uniform float3 _Sun_WorldSunDir;

			uniform float extinctionThickness;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 planetOrigin: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				//v.vertex.xyz*= (_experimentalExtinctionScale * (Rt-Rg)+ Rg) / Rt;
				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz * 6000; //all calculations are done in localSpace
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{

				float3 extinction = float3(1,1,1);

				float3 WCP = _WorldSpaceCameraPos * 6000; //unity supplied, converted from ScaledSpace to localSpace coords			    
				float3 d = normalize(IN.worldPos-_WorldSpaceCameraPos);  //viewdir

				//Rt=Rg+(Rt-Rg)*_experimentalExtinctionScale;

				float3 viewdir=normalize(d);
				float3 camera=WCP - IN.planetOrigin;

				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;
				float r0 = r;
				float mu0 = mu;

				float dSq = rMu * rMu - r * r + Rt*Rt;
				float deltaSq = sqrt(dSq);

				float din = max(-rMu - deltaSq, 0.0);

				if (din > 0.0)
				{
					camera += din * viewdir;
					rMu += din;
					mu = rMu / Rt;
					r = Rt;
				}

				if (r > Rt || dSq < 0.0)
				{
					return float4(1.0,1.0,1.0,1.0);
				} 

				extinction = Transmittance(r, mu);    			

				float average=(extinction.r+extinction.g+extinction.b)/3;
				extinction = _Extinction_Tint * extinction + (1-_Extinction_Tint) * float3(average,average,average);

				extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );

				return float4(extinction,1.0);
			}

			ENDCG

		}



		Pass 	//inscattering pass
		{

			Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }

			ZWrite Off
			cull Front

			//Blend One One  //additive blending
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
			#include "Godrays/GodraysCommon.cginc"

			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
			#pragma multi_compile DITHERING_OFF DITHERING_ON
			#pragma multi_compile GODRAYS_OFF GODRAYS_ON

			uniform float _Alpha_Global;

			uniform float3 _Sun_WorldSunDir;
			uniform float _SkyExposure;

#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif

#if defined (GODRAYS_ON)
			uniform sampler2D _godrayDepthTexture;
			uniform float _godrayStrength;
#endif

			struct v2f 
			{
				float3 worldPos : TEXCOORD0;
				float3 planetOrigin: TEXCOORD1;
#if defined (GODRAYS_ON)
				float4 projPos  : TEXCOORD2;
#endif
			};


			v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
			{
				v2f OUT;
				v.vertex.xyz*= (_experimentalAtmoScale * (Rt-Rg)+ Rg) / Rt;
				outpos = UnityObjectToClipPos(v.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.planetOrigin = mul (unity_ObjectToWorld, float4(0,0,0,1)).xyz * 6000;  //all calculations are done in localSpace

#if defined (GODRAYS_ON)
				OUT.projPos = ComputeScreenPos(outpos);
#endif

				return OUT;
			}

			float4 frag(v2f IN, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
				float3 WSD = _Sun_WorldSunDir;
				float3 WCP = _WorldSpaceCameraPos * 6000; //unity supplied, converted from ScaledSpace to localSpace coords

				float3 d = normalize(IN.worldPos-_WorldSpaceCameraPos);  //viewdir computed from scaledSpace

				float3 scatteringCameraPos = WCP - IN.planetOrigin;
#if defined (GODRAYS_ON)
				float2 depthUV = IN.projPos.xy/IN.projPos.w;
				scatteringCameraPos = scatteringCameraPos + d * sampleGodrayDepth(_godrayDepthTexture, depthUV, _godrayStrength);
#endif
				float3 inscatter = SkyRadiance3(scatteringCameraPos, d, WSD);

				float3 finalColor = inscatter;
				float eclipseShadow = 1;

				//find worldPos of the point in the atmo we're looking at directly
				//necessary for eclipses, ring shadows and planetshine
				float3 worldPos;
#if defined (PLANETSHINE_ON) || defined (ECLIPSES_ON) || defined (RINGSHADOW_ON)
				float interSectPt= intersectSphereInside(WCP,d,IN.planetOrigin,Rt);//*_rimQuickFixMultiplier

				if (interSectPt != -1)
				{
					worldPos = WCP + d * interSectPt;
				}
#endif

#if defined (ECLIPSES_ON)
				if (interSectPt != -1)
				{				
					finalColor*= getEclipseShadows(worldPos);
				}
#endif

#if defined (RINGSHADOW_ON)
				if (interSectPt != -1)
				{
					finalColor *= getLinearRingColor(worldPos, _Sun_WorldSunDir, IN.planetOrigin).a;
				}
#endif

				/////////////////PLANETSHINE///////////////////////////////						    
#if defined (PLANETSHINE_ON)
				float3 inscatter2=0;
				float intensity=1;
				for (int i=0; i<4; ++i)
				{
					if (planetShineRGB[i].w == 0) break;

					//if source is not a sun compute intensity of light from angle to light source
					intensity=1;  
					if (planetShineSources[i].w != 1.0f)
					{
//						intensity = 0.5f*(1-dot(normalize(planetShineSources[i].xyz - worldPos),WSD));
						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[i].xyz - worldPos),WSD)),0);
					}

					inscatter2+=SkyRadiance3(WCP - IN.planetOrigin, d, normalize(planetShineSources[i].xyz)) *planetShineRGB[i].xyz*planetShineRGB[i].w*intensity;
				}

				finalColor+=inscatter2;
				#endif
				///////////////////////////////////////////////////////////	

				return float4(_Alpha_Global*dither(hdr(finalColor,_SkyExposure), screenPos),1.0);	
			}
			ENDCG
		}


	}
}