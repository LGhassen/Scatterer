Shader "Scatterer/SkyScaled" 
{
	SubShader 
	{
    	Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
		 
		Pass   											//extinction pass, I should really just put the shared components in an include file to clean this up
    	{		
    	 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }    	 	 		

    	 ZWrite Off


//if localSpaceMode //i.e box mode
//    		ZTest Off
//    		cull Front
//endif

    		Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "AtmosphereScatterer.cginc"			
			
			#define useAnalyticSkyTransmittance
			
			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile DEFAULT_SQRT_OFF DEFAULT_SQRT_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
//			#define localSpaceMode

			uniform float _Alpha_Global;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Scatterer_Origin;
			uniform float _Extinction_Tint;
			uniform float extinctionMultiplier;
			uniform float extinctionRimFade;
			uniform float extinctionGroundFade;

			uniform float3 _Sun_WorldSunDir;

			//stuff for kopernicus ring shadows
			uniform sampler2D ringTexture;
			uniform float ringInnerRadius;
			uniform float ringOuterRadius;
			uniform float3 ringNormal;
					

			
//eclipse uniforms
#if defined (ECLIPSES_ON)			
			uniform float4 sunPosAndRadius; //xyz sun pos w radius
			uniform float4x4 lightOccluders1; //array of light occluders
											 //for each float4 xyz pos w radius
			uniform float4x4 lightOccluders2;
#endif
			
			
			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float3 worldPos : TEXCOORD0;

			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
    			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
    			return OUT;
			}
							
			float4 frag(v2f IN) : COLOR
			{
			
			    float3 extinction = float3(1,1,1);

#if defined (localSpaceMode)
				float3 WCP = _WorldSpaceCameraPos; //unity supplied, in local Space
#else
			    float3 WCP = _WorldSpaceCameraPos * 6000; //unity supplied, converted from ScaledSpace to localSpace coords
#endif
			    
			    float3 d = normalize(IN.worldPos-_WorldSpaceCameraPos);  //viewdir computed in scaledSpace or localSpace, depending
				
				Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;
	
//				float3 viewdir=normalize(dir);
//				float3 viewdir=d;
//				viewdir.x+=_viewdirOffset;
				float3 viewdir=normalize(d);
				
				float3 camera=WCP - _Scatterer_Origin;
				
				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;
				float r0 = r;
				float mu0 = mu;

    			float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,0.000001);

    			float din = max(-rMu - deltaSq, 0.0);
    			
    			if (din > 0.0)
    			{
        			camera += din * viewdir;
        			rMu += din;
        			mu = rMu / Rt;
        			r = Rt;
    			}
    			
    			if (r > Rt)
    			{
    				return float4(1.0,1.0,1.0,1.0);
   				} 
    

#if defined (useAnalyticSkyTransmittance)

				if (intersectSphere2(WCP,d,_Scatterer_Origin,Rg) > 0)
				{
					float distInAtmo= intersectSphere2(WCP,d,_Scatterer_Origin,Rg)-intersectSphere2(WCP,d,_Scatterer_Origin,Rt);
					extinction = AnalyticTransmittance(r, mu, (distInAtmo));
				}
				else
					extinction = Transmittance(r, mu); 
				
//				extinction = min(AnalyticTransmittance(r, mu, (distInAtmo)),1.0); //haven't tried this yet

#else				
    			extinction = Transmittance(r, mu);    			
#endif

//    			extinction = lerp(average, extinction, _Extinction_Tint); //causes issues in OpenGL somehow so reproduced lerp manually    								

    			float average=(extinction.r+extinction.g+extinction.b)/3;
    			extinction = extinctionMultiplier *  float3(_Extinction_Tint*extinction.r + (1-_Extinction_Tint)*average,
    								_Extinction_Tint*extinction.g + (1-_Extinction_Tint)*average,
    								_Extinction_Tint*extinction.b + (1-_Extinction_Tint)*average);

				float interSectPt= intersectSphere2(WCP,d,_Scatterer_Origin,Rg);
				
				bool rightDir = (interSectPt > 0) ;
				if (!rightDir)
				{
					extinction= float3(1.0,1.0,1.0)*extinctionRimFade +(1-extinctionRimFade)*extinction;
				}

				//find worldPos of the point in the atmo we're looking at directly
				//necessary for eclipses, ring shadows and planetshine
				float3 worldPos;
#if defined (ECLIPSES_ON) || defined (RINGSHADOW_ON)
			    interSectPt= intersectSphere4(WCP,d,_Scatterer_Origin,Rt);//*_rimQuickFixMultiplier
			    
				if (interSectPt != -1)
				{
					worldPos = WCP + d * interSectPt;  //worldPos, actually relative to planet origin
				}
#endif

/////////////////ECLIPSES///////////////////////////////
#if defined (ECLIPSES_ON)
				if (rightDir&& interSectPt != -1)	//eclipses shouldn't hide celestial objects visible in the sky		
				{
 					float eclipseShadow = 1;
					
					for (int i=0; i<4; ++i)
    				{
        				if (lightOccluders1[i].w <= 0)	break;
						eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
							   lightOccluders1[i].w, sunPosAndRadius.w);
					}
						
					for (int j=0; j<4; ++j)
    				{
        				if (lightOccluders2[j].w <= 0)	break;
						eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
							   lightOccluders2[j].w, sunPosAndRadius.w)	;
					}

			    	extinction*= eclipseShadow;
			    	extinction= float3(1.0,1.0,1.0)*extinctionGroundFade +(1-extinctionGroundFade)*extinction;
			    }
#endif


/////////////////RING SHADOWS///////////////////////////////			
#if defined (RINGSHADOW_ON)
				if (rightDir&& interSectPt != -1)	//eclipses shouldn't hide celestial objects visible in the sky		
				{
					//raycast from atmo to ring plane and find intersection
					float3 ringIntersectPt = LinePlaneIntersection(worldPos, _Sun_WorldSunDir, ringNormal, _Scatterer_Origin);

					//calculate ring texture position on intersect
					float distance = length (ringIntersectPt - _Scatterer_Origin);
					float ringTexturePosition = (distance - ringInnerRadius) / (ringOuterRadius - ringInnerRadius); //inner and outer radiuses need are converted to local space coords on plugin side
					ringTexturePosition = 1 - ringTexturePosition; //flip to match UVs

//					//read 1-alpha of ring texture
//					float ringShadow = 1- (tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition))).a;

					float4 ringColor = tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition));
					float ringShadow = (1-ringColor.a)*((ringColor.x+ringColor.y+ringColor.z)*0.33334);

					//don't apply any shadows if intersect point is not between inner and outer radius
					ringShadow = (ringTexturePosition > 1 || ringTexturePosition < 0 ) ? 1 : ringShadow;

					extinction*=ringShadow;
				}
#endif
				
				return float4(extinction,1.0);
			}
			
			ENDCG

    	}
    	
    	
    	
    	Pass 		//sky pass
    	{

    	 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }

    	 ZWrite Off

//if localSpaceMode
//    		ZTest Off
//    		cull Front
//endif

    		Blend One One  //additive blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "AtmosphereScatterer.cginc"
			
			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile DEFAULT_SQRT_OFF DEFAULT_SQRT_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			#pragma multi_compile RINGSHADOW_OFF RINGSHADOW_ON
//			#define PLANETSHINE_OFF
			
//			#define localSpaceMode
			
			uniform float _Alpha_Global;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Scatterer_Origin;
			uniform float _RimExposure;
					
			uniform float3 _Sun_WorldSunDir;

			//stuff for kopernicus ring shadows
#if defined (RINGSHADOW_ON)	
			uniform sampler2D ringTexture;
			uniform float ringInnerRadius;
			uniform float ringOuterRadius;
			uniform float3 ringNormal;
#endif
			
			
			
//eclipse uniforms
#if defined (ECLIPSES_ON)			
			uniform float4 sunPosAndRadius; //xyz sun pos w radius
			uniform float4x4 lightOccluders1; //array of light occluders
											 //for each float4 xyz pos w radius
			uniform float4x4 lightOccluders2;

#endif

#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif
		
			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float3 worldPos : TEXCOORD0;
    			
			};
			

			v2f vert(appdata_base v)
			{
				v2f OUT;
			    OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				
    			return OUT;
			}
			
	
float4 frag(v2f IN) : COLOR
			{
			    float3 WSD = _Sun_WorldSunDir;

#if defined (localSpaceMode)
				float3 WCP = _WorldSpaceCameraPos; //unity supplied, in local Space
#else
			    float3 WCP = _WorldSpaceCameraPos * 6000; //unity supplied, converted from ScaledSpace to localSpace coords
#endif
			    
			    float3 d = normalize(IN.worldPos-_WorldSpaceCameraPos);  //viewdir computed in scaledSpace
			    
				float interSectPt= intersectSphere2(WCP,d,_Scatterer_Origin,Rg);

				bool rightDir = (interSectPt > 0) ;  //rightdir && exists combined
				if (!rightDir)
				{
					_Exposure=_RimExposure;
				}

			    float3 extinction;
			    float3 inscatter = SkyRadiance2(WCP - _Scatterer_Origin, d, WSD,extinction);

				//find worldPos of the point in the atmo we're looking at directly
				//necessary for eclipses, ring shadows and planetshine
				float3 worldPos;
#if defined (PLANETSHINE_ON) || defined (ECLIPSES_ON) || defined (RINGSHADOW_ON)
			    interSectPt= intersectSphere4(WCP,d,_Scatterer_Origin,Rt);//*_rimQuickFixMultiplier
			    
				if (interSectPt != -1)
				{
					worldPos = WCP + d * interSectPt;
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
						//intensity *= Mathf.SmoothStep(0,1, Mathf.Clamp01(-Vector3.Dot(sourcePosRelPlanet,sunPosRelPlanet)));
//						intensity *= Mathf.SmoothStep(0,1, 0.5f*(1+(-Vector3.Dot(sourcePosRelPlanet,sunPosRelPlanet))));
						//intensity *= 0.5f*(1+(-Vector3.Dot(sourcePosRelPlanet,sunPosRelPlanet)));
//						intensity = 0.5f*(1-dot(normalize(planetShineSources[i].xyz - worldPos),WSD));
						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[i].xyz - worldPos),WSD)),0);
					}
				    				
    				inscatter2+=SkyRadiance2(WCP - _Scatterer_Origin, d, normalize(planetShineSources[i].xyz),extinction)
    							*planetShineRGB[i].xyz*planetShineRGB[i].w*intensity;
    			}
			    
#endif	    

/////////////////ECLIPSES///////////////////////////////		
#if defined (ECLIPSES_ON)				
 				float eclipseShadow = 1; 						
				
				if (interSectPt != -1)
				{					
            		    for (int i=0; i<4; ++i)
    					{
        					if (lightOccluders1[i].w <= 0)	break;
							eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
								   lightOccluders1[i].w, sunPosAndRadius.w)	;
						}
						
						for (int j=0; j<4; ++j)
    					{
        					if (lightOccluders2[j].w <= 0)	break;
							eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
								   lightOccluders2[j].w, sunPosAndRadius.w)	;
						}
				}
#endif


/////////////////RING SHADOWS///////////////////////////////			
#if defined (RINGSHADOW_ON)
				//raycast from atmo to ring plane and find intersection
				float3 ringIntersectPt = LinePlaneIntersection(worldPos, _Sun_WorldSunDir, ringNormal, _Scatterer_Origin);

				//calculate ring texture position on intersect
				float distance = length (ringIntersectPt - _Scatterer_Origin);
				float ringTexturePosition = (distance - ringInnerRadius) / (ringOuterRadius - ringInnerRadius); //inner and outer radiuses need are converted to local space coords on plugin side
				ringTexturePosition = 1 - ringTexturePosition; //flip to match UVs

//				//read 1-alpha of ring texture
//				float ringShadow = 1- (tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition))).a;

				float4 ringColor = tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition));
				float ringShadow = (1-ringColor.a)*((ringColor.x+ringColor.y+ringColor.z)*0.33334);

				//don't apply any shadows if intersect point is not between inner and outer radius
				ringShadow = (ringTexturePosition > 1 || ringTexturePosition < 0 ) ? 1 : ringShadow;
#endif

#if defined (ECLIPSES_ON)
			    float3 finalColor = inscatter * eclipseShadow;
#else
			    float3 finalColor = inscatter;
#endif

#if defined (RINGSHADOW_ON)
				finalColor *= ringShadow;
#endif


#if defined (PLANETSHINE_ON)
				finalColor+=inscatter2;
#endif
				return float4(_Alpha_Global*hdr(finalColor),1.0);		    	
	
			}
			
			ENDCG

    	}

	}
	
}