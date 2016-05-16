Shader "Proland/Atmo/Sky" 
{
	SubShader 
	{
		 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
		 
		Pass   											//extinction pass, I should really just put the shared components in an include file to clean this up
    	{		
    	 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
    	 
 	 			
    		ZWrite Off

//if localSpaceMode //i.e box mode
    		ZTest Off
    		cull Front
//endif

    		Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			//#pragma only_renderers d3d9
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
//			#include "Utility.cginc"
//			#include "AtmosphereNew.cginc"
			#include "AtmosphereScatterer.cginc"
			
//			#define eclipses
			
			#define useAnalyticSkyTransmittance
			
			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#define localSpaceMode

			uniform float _Alpha_Global;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Globals_Origin;
			uniform float _Extinction_Tint;
			uniform float extinctionMultiplier;
			uniform float extinctionRimFade;
			uniform float extinctionGroundFade;

			
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
				OUT.worldPos = mul(_Object2World, v.vertex);
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
				
				float3 camera=WCP - _Globals_Origin;
				
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

				if (intersectSphere2(WCP,d,_Globals_Origin,Rg) > 0)
				{
					float distInAtmo= intersectSphere2(WCP,d,_Globals_Origin,Rg)-intersectSphere2(WCP,d,_Globals_Origin,Rt);
					extinction = AnalyticTransmittance(r, mu, (distInAtmo));
				}
				else
					extinction = Transmittance(r, mu); 
				
//				extinction = min(AnalyticTransmittance(r, mu, (distInAtmo)),1.0); //haven't tried this yet

#else				
    			extinction = Transmittance(r, mu);    			
#endif
    			
    			float average=(extinction.r+extinction.g+extinction.b)/3;
    			
    			
    			extinction = extinctionMultiplier *  float3(_Extinction_Tint*extinction.r + (1-_Extinction_Tint)*average,
    								_Extinction_Tint*extinction.g + (1-_Extinction_Tint)*average,
    								_Extinction_Tint*extinction.b + (1-_Extinction_Tint)*average);
    								
    								
    								
//    			extinction = lerp(average, extinction, _Extinction_Tint); //causes issues in OpenGL somehow so reproduced lerp manually
    			
    			
				float interSectPt= intersectSphere2(WCP,d,_Globals_Origin,Rg);
				
				bool rightDir = (interSectPt > 0) ;
				if (!rightDir)
				{
					extinction= float3(1.0,1.0,1.0)*extinctionRimFade +(1-extinctionRimFade)*extinction;
				}


#if defined (ECLIPSES_ON)
				else  	//eclipses shouldn't hide celestial objects visible in the sky		
				{
 					float eclipseShadow = 1;
 						
 					//trick to make the eclipse shadow less obvious inside the atmosphere									
					float eclipseCeiling=Rt;
					float height= length(WCP-_Globals_Origin);
				
//					if (height>Rt)
//					{
						interSectPt= intersectSphere4(WCP,d,_Globals_Origin,eclipseCeiling);
//					}
//					else
//					{
//						interSectPt= intersectSphere4(WCP,IN.dir,WCP,15000);
//					}
					
					
					if (interSectPt != -1)
					{
						float3 worldPos = WCP + d * interSectPt;  //worldPos, actually relative to planet origin
					
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
					}

			    	extinction*= eclipseShadow;
			    	extinction= float3(1.0,1.0,1.0)*extinctionGroundFade +(1-extinctionGroundFade)*extinction;
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
    		ZTest Off
    		cull Front
//endif

 
    		Blend One One  //additive blending
//            Blend OneMinusDstColor One //soft additive

			CGPROGRAM
			#include "UnityCG.cginc"
			//#pragma only_renderers d3d9
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "AtmosphereScatterer.cginc"
			
//			#define eclipses
			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			
			#define localSpaceMode
			
			uniform float _Alpha_Global;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Globals_Origin;
			uniform float _RimExposure;
			

//			uniform sampler2D _Sun_Glare;
			uniform float3 _Sun_WorldSunDir;
//							uniform float4x4 _Sun_WorldToLocal;
			
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
				OUT.worldPos = mul(_Object2World, v.vertex);
				
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
			    

//			    float3 d = normalize(IN.dir);
			    
			    float3 d = normalize(IN.worldPos-_WorldSpaceCameraPos);  //viewdir computed in scaledSpace
			    
				float interSectPt= intersectSphere2(WCP,d,_Globals_Origin,Rg);

				bool rightDir = (interSectPt > 0) ;  //rightdir && exists combined
				if (!rightDir)
				{
					_Exposure=_RimExposure;
				}

			    float3 extinction;
			    float3 inscatter = SkyRadiance2(WCP - _Globals_Origin, d, WSD,extinction);
			    
			    
#if defined (PLANETSHINE_ON)
			    float3 inscatter2=0;
			    for (int i=0; i<4; ++i)
    			{
    				if (planetShineSources[i].w == 0) break;
    				inscatter2+=SkyRadiance2(WCP - _Globals_Origin, d, planetShineSources[i].xyz,extinction)
    							*planetShineRGB[i].xyz*planetShineRGB[i].w;
    			}
			    
#endif	    
				
#if defined (ECLIPSES_ON)				
 				float eclipseShadow = 1;
 						
 				//trick to make the eclipse shadow less obvious inside the atmosphere									
				float eclipseCeiling=Rt;
				float height= length(WCP-_Globals_Origin);
				
//				if (height>Rt)
//				{
//					interSectPt= intersectSphere4(WCP - _Globals_Origin*_Globals_ApparentDistance,IN.dir,_Globals_Origin,eclipseCeiling*_rimQuickFixMultiplier);//*_rimQuickFixMultiplier
					interSectPt= intersectSphere4(WCP,d,_Globals_Origin,eclipseCeiling);//*_rimQuickFixMultiplier
//				}
//				else
//				{
//					interSectPt= intersectSphere4(WCP,IN.dir,WCP,15000);
//				}
					
					
				
				if (interSectPt != -1)
				{
					float3 worldPos = WCP + d * interSectPt;  //worldPos, actually relative to planet origin
					
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
				
				

#if defined (ECLIPSES_ON)
//			    float3 finalColor = sunColor * extinction + inscatter * eclipseShadow;
			    float3 finalColor = inscatter * eclipseShadow;
#else
//			    float3 finalColor = sunColor * extinction;
			    float3 finalColor = inscatter;
#endif

#if defined (PLANETSHINE_ON)
//				finalColor=finalColor+inscatter2;
				finalColor+=inscatter2;
#endif
				return float4(_Alpha_Global*hdr(finalColor),1.0);			    	
	
			}
			
			ENDCG

    	}

	}
	
}