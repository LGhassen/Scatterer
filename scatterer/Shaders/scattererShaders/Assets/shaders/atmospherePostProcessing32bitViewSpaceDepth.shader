// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Scatterer/AtmosphericScatter" {
    SubShader {
          Tags {"Queue" = "Transparent-5" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		
	
    	

        Pass {  //extinction pass

    		ZWrite Off
    		ZTest Off
    		//cull Front
    		Cull Off

            Blend DstColor Zero //multiplicative

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "CommonAtmosphere.cginc"
            
//           	#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
            
            uniform float4x4 _ViewProjInv;
            uniform float _Scale;
            uniform float _global_alpha;
            uniform float _global_depth;
            uniform float _Ocean_Sigma;
            uniform float fakeOcean;
            uniform float _fade;
            uniform float3 _Ocean_Color;
            uniform float3 _camPos; // camera position relative to planet's origin
            uniform float3 _camForward;
            uniform float _Post_Extinction_Tint;
            uniform float postExtinctionMultiplier;
            uniform sampler2D _customDepthTexture;
            uniform float _openglThreshold;
            uniform float _horizonDepth;
            uniform float4x4 _Globals_CameraToWorld;
            uniform float4x4 scattererFrustumCorners;

//            //eclipse uniforms
//#if defined (ECLIPSES_ON)			
//			uniform float4 sunPosAndRadius; //xyz sun pos w radius
//			uniform float4x4 lightOccluders1; //array of light occluders
//											 //for each float4 xyz pos w radius
//			uniform float4x4 lightOccluders2;
//#endif  

            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 view_dir:TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                v.vertex.y = v.vertex.y *_ProjectionParams.x; //flip if flipped projection matrix
                o.pos = float4(v.vertex.xy,1.0,1.0);
				o.uv=v.texcoord.xy;
				o.view_dir = scattererFrustumCorners[(int) v.vertex.z]; 	//interpolated from frustum corners world viewdir
                return o;
            }
            
            half4 frag(v2f i): COLOR
            {
                float fragDepth = tex2D(_customDepthTexture, i.uv).r;         

				float3 rayDir=normalize(i.view_dir);

				//using view-space z
				float aa = dot(rayDir, normalize (_camForward)); //here I basically take the angle between the camera direction and the fragment direction
																			//and multiply the depth value by it to get the true fragment distance
																			//I'm using view-space z value as depth and basically the z depth value is the projection of the fragment on the near plane
																			//As far as I can tell view-space z offers better, linear precision so it covers the whole scene and it's easy to work with
																			//for other effects like SSAO as well
				float fragDistance = fragDepth * 750000 /aa;

				float oceanDistance = intersectSphere2(_camPos, rayDir, float3(0.0, 0.0, 0.0), Rg);  //intersection with ocean surface
				bool rightDir = (oceanDistance > 0); 												//this ensures that we're looking in the right direction // ie ocean surface intersection point is in front of us
				bool  oceanCloserThanTerrain = rightDir && (oceanDistance < fragDistance);						//this condition ensures the ocean is in front of the terrain, if it's in front we use its pos else we don't
				float minDistance = oceanCloserThanTerrain ? oceanDistance : fragDistance;
				                
                bool infinite = (fragDepth == 1.0); //basically viewer ray isn't hitting any terrain
                float minDepth = minDistance * aa;
                bool fragmentInsideOfClippingRange = (minDepth <= _ProjectionParams.z) && (minDepth  >= _ProjectionParams.y); //if fragment depth outside of current camera clipping range, return empty pixel

				bool returnPixel = fragmentInsideOfClippingRange && (rightDir || (!infinite));

                float3 worldPos = minDistance*rayDir + _camPos;
                worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix

                float3 extinction = getExtinction(_camPos, worldPos, 1.0, 1.0, 1.0);
                float average=(extinction.r+extinction.g+extinction.b)/3;

                //lerped manually because of an issue with opengl or whatever
                extinction = float3(_Post_Extinction_Tint*extinction.r + (1-_Post_Extinction_Tint)*average,
                _Post_Extinction_Tint*extinction.g + (1-_Post_Extinction_Tint)*average,
                _Post_Extinction_Tint*extinction.b + (1-_Post_Extinction_Tint)*average);
                //                extinction = lerp(average, extinction, _Post_Extinction_Tint);
                extinction = lerp (float3(1,1,1), extinction, postExtinctionMultiplier);
                
                
//#if defined (ECLIPSES_ON)				
// 				float eclipseShadow = 1;
// 							
//            	for (int i=0; i<4; ++i)
//    			{
//        			if (lightOccluders1[i].w <= 0)	break;
//					eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
//								   lightOccluders1[i].w, sunPosAndRadius.w)	;
//				}
//						
//				for (int j=0; j<4; ++j)
//    			{
//        			if (lightOccluders2[j].w <= 0)	break;
//					eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
//								   lightOccluders2[j].w, sunPosAndRadius.w)	;
//				}
//
//				extinction*=eclipseShadow;
//#endif

                return float4( returnPixel? extinction : float3(1.0,1.0,1.0) , 1.0);
            }
            ENDCG
        }


//scattering pass
Pass {
			//Cull Front
			Cull Off
			ZTest Off
			ZWrite Off
    	
            Blend OneMinusDstColor One //soft additive
//            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "CommonAtmosphere.cginc"

            #pragma multi_compile GODRAYS_OFF GODRAYS_ON
//			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
            
            uniform float4x4 _ViewProjInv;
            uniform float _Scale;
            uniform float _global_alpha;
            uniform float _global_depth;
            uniform float _Ocean_Sigma;
            uniform float fakeOcean;
            uniform float _fade;
            uniform float3 _Ocean_Color;
            uniform float3 _camPos; // camera position relative to planet's origin
            uniform float3 _camForward; //camera's viewing direction, in world space

            uniform sampler2D _customDepthTexture;
#if defined (GODRAYS_ON)
            uniform sampler2D _godrayDepthTexture;
#endif
            //uniform float4 _MainTex_TexelSize;
            uniform float _openglThreshold;
            uniform float _horizonDepth;
            uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 scattererFrustumCorners;

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
                //float4 pos: SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 view_dir:TEXCOORD1;
            };

            v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
            {
                v2f o;
                v.vertex.y = v.vertex.y *_ProjectionParams.x; //flip if flipped projection matrix
                outpos = float4(v.vertex.xy,1.0,1.0);
				o.uv=v.texcoord.xy;
				o.view_dir = scattererFrustumCorners[(int) v.vertex.z]; 	//interpolated from frustum corners world viewdir
                return o;
            }

            half4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
				float fragDepth = tex2D(_customDepthTexture, i.uv).r;                

				float3 rayDir=normalize(i.view_dir);

				//using view-space z
				float aa = dot(rayDir, normalize (_camForward)); //here I basically take the angle between the camera direction and the fragment direction
																			//and multiply the depth value by it to get the true fragment distance
																			//I'm using view-space z value as depth and basically the z depth value is the projection of the fragment on the near plane
																			//As far as I can tell view-space z offers better, linear precision so it covers the whole scene and it's easy to work with
																			//for other effects like SSAO as well
				float fragDistance = fragDepth * 750000 /aa;

				float oceanDistance = intersectSphere2(_camPos, rayDir, float3(0.0, 0.0, 0.0), Rg);  //intersection with ocean surface
				bool rightDir = (oceanDistance > 0); 												//this ensures that we're looking in the right direction // ie ocean surface intersection point is in front of us
				bool  oceanCloserThanTerrain = rightDir && (oceanDistance < fragDistance);						//this condition ensures the ocean is in front of the terrain, if it's in front we use its pos else we don't
				float minDistance = oceanCloserThanTerrain ? oceanDistance : fragDistance;

                bool infinite = (fragDepth == 1.0); //basically viewer ray isn't hitting any terrain
                float minDepth = minDistance * aa;
                bool fragmentInsideOfClippingRange = ((minDepth  >= _ProjectionParams.y)  && (minDepth <= _ProjectionParams.z)); //if fragment depth outside of current camera clipping range, return empty pixel
				bool returnPixel = fragmentInsideOfClippingRange && (rightDir || (!infinite));

                float3 worldPos = minDistance*rayDir + _camPos;
                worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix

                float3 inscatter=0.0;
                float3 extinction=0.0;

#if defined (PLANETSHINE_ON)
			    float3 inscatter2=0;
			    for (int j=0; j<4; ++j)
    			{
    				if (planetShineRGB[j].w == 0) break;

			   		float intensity=1;  
			   		if (planetShineSources[j].w != 1.0f)
					{
						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[j].xyz - worldPos),SUN_DIR)),0); //if source is not a sun compute intensity of light from angle to light source
																													  //totally made up formula by eyeballing it
					}
    				
    				inscatter2+=InScattering2(_camPos, worldPos,extinction, normalize(planetShineSources[j].xyz),1.0, 1.0, 1.0)
    							*planetShineRGB[j].xyz*planetShineRGB[j].w*intensity;
    			}


				inscatter+= inscatter2 * ( (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ); //somehow the shader compiler for OpenGL behaves differently around braces
																														  //and the shader won't work unless you put braces EVERYWHERE
#endif


//Now do the same but for godrays
//WorldPos and godrayWorldPos are kept separate to ensure compatibility with planetshine, light from sun and other sources should be handled separately
//ie if the sun is casting godrays light from the moon shouldn't have the same godrays but render at normal terrain depth
//technically the moon should also render its own godrays but I would need a buffer for each additional light source so NO.
#if defined (GODRAYS_ON)				
				float godrayDepth= tex2D(_godrayDepthTexture, i.uv).r;
				float godrayDistance = godrayDepth * 750000 /aa;

//				//fade godrays when looking at them sideways by lerping to terrain depth
                float3 SidewaysFromSun = normalize(cross(_camPos,SUN_DIR)); 		//can't we simplify this? the idea is to get how far we're looking from plane, containing the sun, the planet center and the camera
                float godrayBlendFactor= 1-abs (dot(SidewaysFromSun,rayDir));	//and fade the godray depth to terrain depth based on that
                godrayDistance = (godrayDistance > 10) ? lerp(minDistance, godrayDistance, godrayBlendFactor) : godrayDistance; //10 or any arbitrary distance that would prevent lerping when on the backside of mountain or w/e
                godrayDistance = (godrayDistance < minDistance) ? godrayDistance : minDistance;

				bool oceanCloserThanGodray = (rightDir) && (oceanDistance < godrayDistance);
				minDistance = oceanCloserThanGodray ? oceanDistance : godrayDistance;
                float3 godrayWorldPos = minDistance * rayDir + _camPos;
				godrayWorldPos=( (length(godrayWorldPos) < (Rg + _openglThreshold)) ? ((Rg + _openglThreshold) * normalize(godrayWorldPos)) : godrayWorldPos); //artifacts fix

				inscatter+= InScattering2(_camPos, godrayWorldPos, extinction, SUN_DIR, 1.0, 1.0, 1.0)*
							( (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ); //somehow the shader compiler for OpenGL behaves differently around braces;

#else
				inscatter+= InScattering2(_camPos, worldPos, extinction, SUN_DIR, 1.0, 1.0, 1.0) * 
							( (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ); //somehow the shader compiler for OpenGL behaves differently around braces
#endif

				//inscatter*= ( (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ); //somehow the shader compiler for OpenGL behaves differently around braces
                
//#if defined (ECLIPSES_ON)				
// 				float eclipseShadow = 1;
// 							
//            	for (int i=0; i<4; ++i)
//    			{
//        			if (lightOccluders1[i].w <= 0)	break;
//					eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders1[i].xyz,
//								   lightOccluders1[i].w, sunPosAndRadius.w)	;
//				}
//						
//				for (int j=0; j<4; ++j)
//    			{
//        			if (lightOccluders2[j].w <= 0)	break;
//					eclipseShadow*=getEclipseShadow(worldPos, sunPosAndRadius.xyz,lightOccluders2[j].xyz,
//								   lightOccluders2[j].w, sunPosAndRadius.w)	;
//				}
//
//				inscatter*=eclipseShadow;
//#endif

                return float4(dither(hdr(inscatter)*_global_alpha, screenPos)*returnPixel, 1);                				
            }
            ENDCG
        }
    }
}