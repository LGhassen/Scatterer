Shader "Proland/Atmo/Sky" 
{
	SubShader 
	{
		 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
	
    	Pass 
    	{
    	 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
    		ZWrite Off
    		ZTest Off
    		
    		cull Front
    
    		Blend One One  //additive blending

			CGPROGRAM
			#include "UnityCG.cginc"
			//#pragma only_renderers d3d9
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Utility.cginc"
			#include "AtmosphereNew.cginc"
			
			#define eclipses
			
			
			//uniform float _Alpha_Cutoff;
			uniform float _viewdirOffset;
			uniform float _experimentalAtmoScale;
			
			uniform float _sunglareScale;
			uniform float _Alpha_Global;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Globals_Origin;
			uniform float _RimExposure;
			

			uniform sampler2D _Sun_Glare;
			uniform float3 _Sun_WorldSunDir;
			uniform float4x4 _Sun_WorldToLocal;
			
//eclipse uniforms
#if defined (eclipses)			
			uniform float4 sunPosAndRadius; //xyz sun pos w radius
			uniform float4x4 lightOccluders; //array of light occluders
											 //for each float4 xyz pos w radius

#endif
			
			
			
			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float2 uv : TEXCOORD0;
    			float3 dir : TEXCOORD1;
    			float3 relativeDir : TEXCOORD2;
			};
			

			v2f vert(appdata_base v)
			{
				v2f OUT;
			    OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
			    float3x3 wtl = _Sun_WorldToLocal;
			    
			    // apply this rotation to view dir to get relative viewdir
			    OUT.relativeDir = mul(wtl, OUT.dir);
    
    			OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
    			OUT.uv = v.texcoord.xy;
    			return OUT;
			}
			
			
			//stole this from basic GLSL raytracing shader somewhere on the net
			//a quick google search and you'll find it
			float intersectSphere3(float3 p1, float3 d, float3 p3, float r)
			{
			// p1 starting point
			// d look direction
			// p3 is the sphere center

				float a = dot(d, d);
				float b = 2.0 * dot(d, p1 - p3);
				float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r*r;

				float test = b*b - 4.0*a*c;

				if (test<0)
				{
					return -1.0;
				}

  					float u = (-b - sqrt(test)) / (2.0 * a);	
  								
//  					float3 hitp = p1 + u * (p2 - p1);			//we'll just do this later instead if needed
//  					return(hitp);
					return u;
			}
			
			
			//for eclipses
			//works from inside sphere
			float intersectSphere4(float3 p1, float3 d, float3 p3, float r)
			{
			// p1 starting point
			// d look direction
			// p3 is the sphere center

				float a = dot(d, d);
				float b = 2.0 * dot(d, p1 - p3);
				float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r*r;

				float test = b*b - 4.0*a*c;

				if (test<0)
				{
					return -1.0;
				}
				
  					float u = (-b - sqrt(test)) / (2.0 * a);
  					
  					//eclipse compatbility for inside the atmosphere
  					if (u<0)
  					{
  						u = (-b + sqrt(test)) / (2.0 * a);
  					}
					return u;
			}
			
			// assumes sundir=vec3(0.0, 0.0, 1.0)
			float3 OuterSunRadiance(float3 viewdir)
			{
			    float3 data = viewdir.z > 0.0 ? tex2D(_Sun_Glare, float2(0.5,0.5) + viewdir.xy * 4.0/_sunglareScale).rgb : float3(0,0,0);
			    
			    
			    return pow(max(float3(0.0,0.0,0.0),data), 2.2) * _Sun_Intensity;		
			}
			
			
			float3 SkyRadiance2(float3 camera, float3 viewdir, float3 sundir, out float3 extinction)//, float shaftWidth)
			{
			extinction = float3(1,1,1);
			float3 result = float3(0,0,0);
	
			float Rt2=Rt;
			Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;
	
	
			viewdir.x+=_viewdirOffset;
			viewdir=normalize(viewdir);

			//camera *= scale;
			//camera += viewdir * max(shaftWidth, 0.0);
			float r = length(camera);
			float rMu = dot(camera, viewdir);
			float mu = rMu / r;
			float r0 = r;
			float mu0 = mu;
	

    		float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,1e30);

    		float din = max(-rMu - deltaSq, 0.0);
    		if (din > 0.0)
    		{
        		camera += din * viewdir;
        		rMu += din;
        		mu = rMu / Rt;
        		r = Rt;
    		}
	
			float nu = dot(viewdir, sundir);
    		float muS = dot(camera, sundir) / r;
    
    		float4 inScatter = Texture4D(_Sky_Inscatter, r, rMu / r, muS, nu);
    
    		extinction = Transmittance(r, mu);
    
    		if (r <= Rt) 
    		{
            
//        if (shaftWidth > 0.0) 
//        {
//            if (mu > 0.0) {
//                inScatter *= min(Transmittance(r0, mu0) / Transmittance(r, mu), 1.0).rgbr;
//            } else {
//                inScatter *= min(Transmittance(r, -mu) / Transmittance(r0, -mu0), 1.0).rgbr;
//            }
//        }

        			float3 inScatterM = GetMie(inScatter);
        			float phase = PhaseFunctionR(nu);
        			float phaseM = PhaseFunctionM(nu);
        			result = inScatter.rgb * phase + inScatterM * phaseM;
    			}
    
         		else
    			{
    				result = float3(0,0,0);
    				extinction = float3(1,1,1);
    			} 

    			return result * _Sun_Intensity;
    		}
    
			
			//Source:   wikibooks.org/wiki/GLSL_Programming/Unity/Soft_Shadows_of_Spheres
			//I believe space engine also uses the same approach because the eclipses look the same ;)
			float getEclipseShadow(float3 worldPos, float3 worldLightPos,float3 occluderSpherePosition,
								   float3 occluderSphereRadius, float3 lightSourceRadius)		
			{
											
				float3 lightDirection = float3(worldLightPos - worldPos);
               	float3 lightDistance = length(lightDirection);
               	lightDirection = lightDirection / lightDistance;
               
				// computation of level of shadowing w  
            	float3 sphereDirection = float3(occluderSpherePosition - worldPos);  //occluder planet
            	float sphereDistance = length(sphereDirection);
            	sphereDirection = sphereDirection / sphereDistance;
            		
            	float dd = lightDistance * (asin(min(1.0, length(cross(lightDirection, sphereDirection)))) 
               				- asin(min(1.0, occluderSphereRadius / sphereDistance)));
            
            	float w = smoothstep(-1.0, 1.0, -dd / lightSourceRadius);
            	w = w * smoothstep(0.0, 0.2, dot(lightDirection, sphereDirection));
            		
				return (1-w);
			}
				
								
			float4 frag(v2f IN) : COLOR
			{
			
			    
			    float3 WSD = _Sun_WorldSunDir;
			    float3 WCP = _Globals_WorldCameraPos;

			    float3 d = normalize(IN.dir);
			    
				float interSectPt= intersectSphere3(WCP,d,_Globals_Origin,Rg);

				bool rightDir = (interSectPt > 0) ;
				if (!rightDir)
				{
					_Exposure=_RimExposure;
				}


			    float3 sunColor = OuterSunRadiance(IN.relativeDir);

			    float3 extinction;
			    float3 inscatter = SkyRadiance2(WCP - _Globals_Origin, d, WSD,extinction);
				
#if defined (eclipses)				
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
        					if (lightOccluders[i].w <= 0)	break;
							eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders[i].xyz,
								   lightOccluders[i].w, sunPosAndRadius.w)	;
						}
				}
#endif
				
				

#if defined (eclipses)
			    float3 finalColor = sunColor * extinction + inscatter * eclipseShadow;
#else
			    float3 finalColor = sunColor * extinction + inscatter;
#endif
				return float4(_Alpha_Global*hdr(finalColor),1.0);			    
	
			}
			
			ENDCG

    	}
	}
}