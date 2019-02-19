Shader "Scatterer/AtmosphericLocalScatter" {
    SubShader {
        Tags {"Queue" = "Transparent-5" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		
//extinction pass
Pass {
			//Cull Front
			Cull Back
			ZTest LEqual
			ZWrite Off

            Blend DstColor Zero //multiplicative

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "../CommonAtmosphere.cginc"

            //#pragma multi_compile DISABLE_UNDERWATER_OFF DISABLE_UNDERWATER_ON

			//#define LOGARITHMIC_DEPTH_ON
            
            uniform float _global_alpha;
            uniform float _global_depth;
            //uniform float3 _camPos; // camera position relative to planet's origin
            uniform float3 _planetPos; //planet origin, in world space
            uniform float3 _camForward; //camera's viewing direction, in world space
            
            uniform float _PlanetOpacity; //to smooth transition from/to scaledSpace

            uniform float _Post_Extinction_Tint;
            uniform float extinctionThickness;

            //uniform sampler2D _customDepthTexture;

            //uniform float4 _MainTex_TexelSize;
            uniform float _openglThreshold;
//            uniform float _horizonDepth;
            uniform float4x4 _Globals_CameraToWorld;
//			uniform float4x4 scattererFrustumCorners;

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
                float4 pos: SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float3 _camPos : TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                               
				float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
				worldPos.xyz/=worldPos.w; //needed?

				worldPos.xyz = (_PlanetOpacity < 1.0) && (length(worldPos.xyz-_planetPos) < Rg) ? _planetPos+Rg* normalize(worldPos.xyz-_planetPos)  : worldPos.xyz  ;

				o.worldPos = float4(worldPos.xyz,1.0);
				o.pos = mul (UNITY_MATRIX_VP,o.worldPos);
				o._camPos = _WorldSpaceCameraPos - _planetPos;

//#if defined (DISABLE_UNDERWATER_ON) //disables rendering the scattering when underwater
//				o.pos = (length(o._camPos) >= Rg ) ? o.pos : float4(2.0,2.0,2.0,1.0);
//#endif
				//o.pos = UnityObjectToClipPos(v.vertex);
				//_PlanetOpacity

                return o;
            }


//			half4 frag(v2f IN): COLOR
			half4 frag(v2f i) : SV_Target
			{

				//float3 _camPos = _WorldSpaceCameraPos - _planetPos;

                float3 worldPos = i.worldPos.xyz/i.worldPos.w - _planetPos; //worldPos relative to planet origin

				half returnPixel = ((  (length(i._camPos)-Rg) < 1000 )  && (length(worldPos) < (Rg-50))) ? 0.0: 1.0;  //enable in case of ocean and close enough to water surface, works well for kerbin

                worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix


               //
//                float3 groundPos = normalize (worldPos) * Rg*1.0008;
				
                float3 extinction = getExtinction(i._camPos, worldPos, 1.0, 1.0, 1.0);
                float average=(extinction.r+extinction.g+extinction.b)/3;

                //lerped manually because of an issue with opengl or whatever
                extinction = _Post_Extinction_Tint * extinction + (1-_Post_Extinction_Tint) * float3(average,average,average);

                extinction= max(float3(0.0,0.0,0.0), (float3(1.0,1.0,1.0)*(1-extinctionThickness) + extinctionThickness*extinction) );
                extinction = (returnPixel == 1.0) ? extinction : float3(1.0,1.0,1.0);
				//return float4(extinction,1.0);
				return float4(1.0,1.0,1.0,1.0);
            }
            ENDCG
        }


//scattering pass
Pass {
			//Cull Front
			Cull Back
			ZTest LEqual
			ZWrite Off

            Blend OneMinusDstColor One //soft additive

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "../CommonAtmosphere.cginc"

            #pragma multi_compile GODRAYS_OFF GODRAYS_ON
//			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
			//#pragma multi_compile DISABLE_UNDERWATER_OFF DISABLE_UNDERWATER_ON

			//#define LOGARITHMIC_DEPTH_ON
            
            uniform float _global_alpha;
            uniform float _global_depth;
            //uniform float3 _camPos; // camera position relative to planet's origin
            uniform float3 _planetPos; //planet origin, in world space
            uniform float3 _camForward; //camera's viewing direction, in world space
            uniform float _ScatteringExposure;
            
            uniform float _PlanetOpacity; //to smooth transition from/to scaledSpace

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
                float4 worldPos : TEXCOORD0;
                float3 _camPos  : TEXCOORD1;
            };

            v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
            {
                v2f o;

				float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
				worldPos.xyz/=worldPos.w; //needed?

				worldPos.xyz = (_PlanetOpacity < 1.0) && (length(worldPos.xyz-_planetPos) < Rg) ? _planetPos+Rg* normalize(worldPos.xyz-_planetPos)  : worldPos.xyz  ;

				o.worldPos = float4(worldPos.xyz,1.0);
				o.worldPos.xyz*=worldPos.w;
				outpos = mul (UNITY_MATRIX_VP,o.worldPos);

				o._camPos = _WorldSpaceCameraPos - _planetPos;

//#if defined (DISABLE_UNDERWATER_ON) //disables rendering the scattering when underwater
//				outpos = (length(o._camPos) >= Rg ) ? outpos : float4(2.0,2.0,2.0,1.0);
//#endif

				//outpos = UnityObjectToClipPos(v.vertex);
				//_PlanetOpacity

                return o;
            }


//			half4 frag(v2f IN): COLOR
			half4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{

				

                float3 worldPos = i.worldPos.xyz/i.worldPos.w - _planetPos; //worldPos relative to planet origin

                half returnPixel = ((  (length(i._camPos)-Rg) < 1000 )  && (length(worldPos) < (Rg-50))) ? 0.0: 1.0;  //enable in case of ocean and close enough to water surface, works well for kerbin

                worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ; //artifacts fix
//
//                float3 groundPos = normalize (worldPos) * Rg*1.0008;

				float minDistance = length(worldPos-i._camPos);
                float3 inscatter=0.0;
                float3 extinction=1.0;

#if defined (PLANETSHINE_ON)
			    for (int j=0; j<4;++j)
    			{
    				if (planetShineRGB[j].w == 0) break;

			   		float intensity=1;  
			   		if (planetShineSources[j].w != 1.0f)
					{
						intensity = 0.57f*max((0.75-dot(normalize(planetShineSources[j].xyz - worldPos),SUN_DIR)),0); //if source is not a sun compute intensity of light from angle to light source
																													  //totally made up formula by eyeballing it
					}
    				
    				inscatter+=InScattering2(i._camPos, worldPos, normalize(planetShineSources[j].xyz),extinction)  //lot of potential extinction recomputations here
    							*planetShineRGB[j].xyz*planetShineRGB[j].w*intensity;
    			}
#endif


//Now do the same but for godrays
//WorldPos and godrayWorldPos are kept separate to ensure compatibility with planetshine, light from sun and other sources should be handled separately
//ie if the sun is casting godrays light from the moon shouldn't have the same godrays but render at normal terrain depth
//technically the moon should also render its own godrays but I would need a buffer for each additional light source so NO.
//#if defined (GODRAYS_ON)
//				float godrayDistance = tex2D(_godrayDepthTexture, i.uv).r;// godray depth
//				godrayDistance *= 750000 /aa; 							  //the real distance
//
////				//fade godrays when looking at them sideways by lerping to terrain depth
//                float3 SidewaysFromSun = normalize(cross(_camPos,SUN_DIR)); 		//can't we simplify this? the idea is to get how far we're looking from plane, containing the sun, the planet center and the camera
//                float godrayBlendFactor= 1-abs (dot(SidewaysFromSun,rayDir));	//and fade the godray depth to terrain depth based on that
//                godrayDistance = (godrayDistance > 10) ? lerp(minDistance, godrayDistance, godrayBlendFactor) : godrayDistance; //10 or any arbitrary distance that would prevent lerping when on the backside of mountain or w/e
//                //godrayDistance = lerp(minDistance, godrayDistance, godrayBlendFactor); //can godray distance be negative? I don't think so
//                godrayDistance = (godrayDistance < minDistance) ? godrayDistance : minDistance;
//
//				//bool oceanCloserThanGodray = (rightDir) && (oceanDistance < godrayDistance);
//				minDistance = (rightDir) && (oceanDistance < godrayDistance) ? oceanDistance : godrayDistance;
//                float3 godrayWorldPos = minDistance * rayDir + _camPos;
//				godrayWorldPos=( (length(godrayWorldPos) < (Rg + _openglThreshold)) ? ((Rg + _openglThreshold) * normalize(godrayWorldPos)) : godrayWorldPos); //artifacts fix
//
//				godrayWorldPos =  lerp(groundPos,godrayWorldPos,_PlanetOpacity);
//
//				inscatter+= InScattering2(_camPos,godrayWorldPos, SUN_DIR)*
//							( (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ); //somehow the shader compiler for OpenGL behaves differently around braces;
//
//#else
				//worldPos =  lerp(groundPos,worldPos,_PlanetOpacity);
				inscatter+= InScattering2(i._camPos, worldPos,SUN_DIR,extinction);
				inscatter*= (minDistance <= _global_depth) ? (1 - exp(-1 * (4 * minDistance / _global_depth))) : 1.0 ; //somehow the shader compiler for OpenGL behaves differently around braces            				
//#endif

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
				//_PlanetOpacity = (_PlanetOpacity == 0.0 ) ? 0.0 : 1.0;

                //OUT.color=float4(hdr(inscatter,_ScatteringExposure) *_global_alpha, 1);
                //OUT.color=float4(1-extinction, 1);

                inscatter = hdr(inscatter,_ScatteringExposure) *_global_alpha;

				return float4(dither(inscatter,screenPos)*returnPixel,1.0);
				//return float4(1.0,0.0,0.0,1.0);
            }
            ENDCG
        }
    }

}