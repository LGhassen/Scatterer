Shader "Sky/AtmosphereGhoss"
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_SkyDome("SkyDome", 2D) = "white" {}
		_Scale("Scale", Vector) = (1,1,1,1)
	}
	SubShader 
	{
	
		Tags { "Queue"="Transparent-5" "IgnoreProjector"="True" "RenderType"="Transparent" } 
	    Pass 
	    {
	    	ZTest Always
	    	ZWrite off
	    	Fog { Mode Off }
	    	Cull Off
	    	Blend SrcAlpha OneMinusSrcAlpha
//			Blend One One
	    
			CGPROGRAM			

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma glsl
			#pragma target 3.0
			#include "Atmosphere3.cginc"
			#define customDepthBuffer
			
			//the next line allows going over opengl 512 arithmetic ops limit
			#pragma profileoption NumMathInstructionSlots=65535

			
			sampler2D _MainTex, _SkyDome;
			
			uniform float4x4 _ViewProjInv;
			
			
			uniform float _viewdirOffset;
			uniform float _experimentalAtmoScale;
			
			
			uniform float _Scale;
			uniform float _global_alpha;
			uniform float _Exposure;
			uniform float _global_depth;
	
			uniform float _Ocean_Sigma;
			uniform float fakeOcean;
			
			uniform float _fade;
			
			uniform float3 _Ocean_Color;

			uniform float3 _camPos; 		// camera position relative to planet's origin
//			uniform float3 _Globals_Origin;

//			uniform sampler2D _CameraDepthNormalsTexture;

#if defined (customDepthBuffer)
			uniform sampler2D _customDepthTexture;
#else
			uniform sampler2D _CameraDepthTexture;
#endif
			

			uniform float4 _MainTex_TexelSize;

			uniform float _openglThreshold;
			uniform float _globalThreshold;
			uniform float _edgeThreshold;
			uniform float _horizonDepth;

			uniform float4x4 _Globals_CameraToWorld;
						
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float2 uv_depth : TEXCOORD2;

			};
			
			struct Ray {
    			float3 o; //origin
    			float3 d; //direction
			};

			struct Sphere {
    			float3 pos;   //center of sphere position
    			float rad;  //radius
			};
			
			
			v2f vert( appdata_base v )
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				
				o.uv=o.screenPos.xy/o.screenPos.w;
				
				o.uv_depth = o.uv;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif				
				
				COMPUTE_EYEDEPTH(o.screenPos.z);    //o = -mul( UNITY_MATRIX_MV, v.vertex ).z
				
				return o;
			}
			
			//fixes artifacts in d3d9
			float SQRT(float f, float err) {
			#if !defined(SHADER_API_D3D9)
			    return sqrt(f);
			#else
    		return f >= 0.0 ? sqrt(f) : err;
			#endif
			}
			
			
			//stole this from basic GLSL raytracing shader somewhere on the net
			//a quick google search and you'll find it
			float intersectSphere2(float3 p1, float3 p2, float3 p3, float r)
			{
			// The line passes through p1 and p2:
			// p3 is the sphere center
				float3 d = p2 - p1;

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
			
			float3 InScattering2(float3 camera, float3 _point, out float3 extinction, float shaftWidth, float scaleCoeff, float irradianceFactor) 
			{
			// single scattered sunlight between two points
			// camera=observer
			// point=point on the ground
			// sundir=unit vector towards the sun
			// return scattered light and extinction coefficient

    			float3 result = float3(0,0,0);
    			extinction = float3(1,1,1);
        
    			float3 viewdir = _point - camera;
    			float d = length(viewdir)* scaleCoeff;
    			viewdir = viewdir / d;
    
    			/////////////////////experimental block begin
    			Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;
				viewdir.x+=_viewdirOffset;
				viewdir=normalize(viewdir);
				/////////////////////experimental block end
	
	
    			float r = length(camera)* scaleCoeff;
        
    			if (r < 0.9 * Rg) 
    			{
        			camera.y += Rg;
        			_point.y += Rg;
        			r = length(camera)* scaleCoeff;
    			}
    			
    			float rMu = dot(camera, viewdir);
    			float mu = rMu / r;
    			float r0 = r;
    			float mu0 = mu;
    			_point -= viewdir * clamp(shaftWidth, 0.0, d);

    			float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,1e30);
    			float din = max(-rMu - deltaSq, 0.0);
    
    			if (din > 0.0 && din < d) 
    			{
        			camera += din * viewdir;
        			rMu += din;
        			mu = rMu / Rt;
        			r = Rt;
        			d -= din;
    			}

    			if (r <= Rt) 
    			{
        			float nu = dot(viewdir, SUN_DIR);
        			float muS = dot(camera, SUN_DIR) / r;
		
        			float4 inScatter;

        			if (r < Rg + 600.0) 
        			{
            			// avoids imprecision problems in aerial perspective near ground
            			float f = (Rg + 600.0) / r;
            			r = r * f;
            			rMu = rMu * f;
            			_point = _point * f;
        			}

        			float r1 = length(_point);
        			float rMu1 = dot(_point, viewdir);
        			float mu1 = rMu1 / r1;
        			float muS1 = dot(_point, SUN_DIR) / r1;

        			if (mu > 0.0)
        			{
          				extinction = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0);
            		}
        			else
        			{
	            		extinction = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0);
    	        	}

        			const float EPS = 0.004;
        			float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
        
        			if (abs(mu - lim) < EPS) 
        			{
            			float a = ((mu - lim) + EPS) / (2.0 * EPS);

            			mu = lim - EPS;
            			r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
            			mu1 = (r * mu + d) / r1;
            
            			float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
            			float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
            			float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

		           		mu = lim + EPS;
        		   		r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
            			mu1 = (r * mu + d) / r1;
            
            			inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
            			inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
            		
            			float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

            			inScatter = lerp(inScatterA, inScatterB, a);
            
            			irradianceFactor=1.0;
            			//Not sure about where irradianceFactor goes
        			} 
        			else 
        			{
            			float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
            			float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
            			inScatter = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
        			}

        			// avoids imprecision problems in Mie scattering when sun is below horizon
        			inScatter.w *= smoothstep(0.00, 0.02, muS);

        			float3 inScatterM = GetMie(inScatter);
        			float phase = PhaseFunctionR(nu);
        			float phaseM = PhaseFunctionM(nu);
        			result = inScatter.rgb * phase + inScatterM * phaseM;
    			} 

    			return result * SUN_INTENSITY; //sun_intensity may as well be hardcoded to 100, since this shader only handles inscattering
    										   //and has no terrain color information, sun intensity has the same effect as exposure
			}
			
			
			
			float3 hdr(float3 L) 
			{
    			L = L * _Exposure;
    			L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
    			L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
    			L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
    		return L;
    		}
    		
    		
    		
    		// L, V, N in world space
			float ReflectedSunRadiance(float3 L, float3 V, float3 N, float sigmaSq) 
			{
    			float3 H = normalize(L + V);

    			float hn = dot(H, N);
    			float p = exp(-2.0 * ((1.0 - hn * hn) / sigmaSq) / (1.0 + hn)) / (4.0 * M_PI * sigmaSq);

    			float c = 1.0 - dot(V, H);
    			float c2 = c * c;
    			float fresnel = 0.02 + 0.98 * c2 * c2 * c;

    			float zL = dot(L, N);
    			float zV = dot(V, N);
    			zL = max(zL,0.01);
    			zV = max(zV,0.01);

    			// brdf times cos(thetaL)
    			return zL <= 0.0 ? 0.0 : max(fresnel * p * sqrt(abs(zL / zV)), 0.0);
			}


			float MeanFresnel(float cosThetaV, float sigmaV)
			{
    			return pow(1.0 - cosThetaV, 5.0 * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(sigmaV, 1.5));
			}


			float MeanFresnel(float3 V, float3 N, float sigmaSq)
			{
    			return MeanFresnel(dot(V, N), sqrt(sigmaSq));
			}


			float3 OceanRadiance(float3 L, float3 V, float3 N, float sigmaSq, float3 sunL, float3 skyE, float3 seaColor) 
			{
    			float F = MeanFresnel(V, N, sigmaSq);
    			float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
    			float3 Lsky = skyE * F / M_PI;
    			float3 Lsea = (1.0 - F) * seaColor * skyE / M_PI;
    			return Lsun + Lsky + Lsea;
			}

    		
			
			half4 frag(v2f i) : COLOR 
			{
				
#if defined (customDepthBuffer)				
//				float dpth =  Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_customDepthTexture,i.uv_depth)));				

				float C=1;
				
				float z = (tex2D(_customDepthTexture, i.uv_depth)).r;
				
				
				z = (exp  (z*log(C*750000+1)) - 1)  /C;
//                z = (pow(C*750000+1,z)-1)/C;
                
				float dpth = z/750000;

//				float dpth = z;


// 				inline float Linear01Depth( float z )
//				{
//					return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
//				}
//				x is 1.0 - (camera's far plane) / (camera's near plane)
//				y is (camera's far plane) / (camera's near plane)
 				
 				float zbx = 1.0- (750000.0/300.0);
 				float zby = (750000.0/300.0);
 				
 				
 				z= (1/dpth - zby) / zbx;
 				
 				
// 				if (dpth >=0.99)
// 					dpth=0.0;


#else
				float dpth =  Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth)));	
#endif
				
				//this dpth value is only used for edge threshold checks and depth distance
				//I realize this isn't very efficient but I'll change it later

//				float visib=1;
//
//				if (dpth<=_global_depth)
//				{
//					visib=1-exp(-1* (4*dpth/_global_depth));
//				}
				
#if defined (customDepthBuffer)
//				float depth = SAMPLE_DEPTH_TEXTURE(_customDepthTexture, i.uv_depth);  //(tex2D(sampler, uv).r)
				float depth = z;
				
#else
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);  //(tex2D(sampler, uv).r)
#endif
				

        		#if !defined(SHADER_API_OPENGL)
//        		float4 H = float4(i.uv_depth.x*2.0f-1.0f, (i.uv_depth.y)*2.0f-1.0f, depth, 1.0f);
				float4 H = float4(i.uv_depth.x*2.0f-1.0f, (i.uv_depth.y)*2.0f-1.0f, depth, 1.0f);
        		#else	
        		float4 H = float4(i.uv_depth.x*2.0f-1.0f, i.uv_depth.y*2.0f-1.0f, depth*2.0f-1.0f, 1.0f);
        		#endif
        		
        		float4 D = mul(_ViewProjInv,H);

        		float3 worldPos = D/D.w;
	
        		float interSectPt= intersectSphere2(_camPos,worldPos,float3(0.0,0.0,0.0),Rg);
        		
//        		float3 worldPos2= _camPos + interSectPt * (worldPos - (_camPos));
//        		bool intersectExists = (interSectPt !=  -1.0); // this ensures an intersection point exists
//        		
//        		bool rightDir = dot (worldPos2 - (_camPos), worldPos - (_camPos)) > 0 ;  //this ensures that we're looking in the right direction
//        																				  //That is, the ocean surface intersection point is in front of us
//        																				  //If we look up the intersection point is behind us and we don't want to use that

				bool rightDir = (interSectPt > 0) ;			//this just does both the previous conditions
        		

        		
#if defined (customDepthBuffer)      		
//        		if ((dpth >= _edgeThreshold)  &&  !(intersectExists && rightDir))
        		if ((dpth >= _edgeThreshold)  &&  !(rightDir))
//				if ((dpth >= 0.95)  &&  !(rightDir))
				
        		{
        			return float4(0.0,0.0,0.0,0.0);
        		}
#else
	#if !defined(SHADER_API_D3D9)
//        		if ((dpth >= _edgeThreshold)  &&  !(intersectExists && rightDir))
        		if ((dpth >= _edgeThreshold)  &&  !(rightDir))
        		{
        			return float4(0.0,0.0,0.0,0.0);
        		}
    #else
//        		if ((dpth >= 0.99)  &&  !(intersectExists && rightDir))
        		if ((dpth >= 0.99)  &&  !(rightDir))
        		{
        			return float4(0.0,0.0,0.0,0.0);
        		}
    #endif
#endif
				
				
				float3 worldPos2= _camPos + interSectPt * (worldPos - (_camPos));
        		bool oceanCloserThanTerrain = ( length (worldPos2 -_camPos) < length (worldPos - _camPos)); //this condition ensures the ocean is in front of the terrain
        																								   //if the terrain is in front of the ocean we don't want to cover it up
        		float3 oceanColor = float3(1.0,1.0,1.0);																			   //with the wrong postprocessing depth
        		
//        		if ((intersectExists) && (rightDir) && oceanCloserThanTerrain)
//        		if ((rightDir && oceanCloserThanTerrain) || (length(worldPos)<(Rg+1000)))
//				if ((rightDir && oceanCloserThanTerrain))
        		{
        				worldPos=worldPos2;
						
//						if (fakeOcean==1.0)					//scrapped due to depth buffer imprecisions even with a log dbuffer
//						{
							float3 V = normalize(worldPos);
    						float3 P = V * max(length(worldPos), Rg + 10.0);
    						float3 v = normalize(P - _camPos);
    						
//    						return float4(v.r,v.g,v.b,1.0);
							return float4(normalize(_camPos).r,normalize(_camPos).g,normalize(_camPos).b,1.0);
    						
							float3 fn = float3(0,0,1); //ocean normal
    			
							float cTheta = dot(fn, SUN_DIR);
							float vSun = dot(V, SUN_DIR);
				
			    			float3 sunL;
						    float3 skyE;
			    			
			    			
			    			SunRadianceAndSkyIrradiance(P, fn, SUN_DIR, sunL, skyE);
							

        					oceanColor = OceanRadiance(SUN_DIR, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color*10);
        					
							
//						}
        		}
        		
        		
        		//artifacts fix
        		#if !defined(SHADER_API_D3D9)
        		if (length(worldPos) < (Rg+_openglThreshold))
        		{
        			worldPos=(Rg+_openglThreshold)*normalize(worldPos);
        		}
				#endif
			    
				float3 extinction = float3(0,0,0);
				
				float irradianceFactor=0.0;

				float3 inscatter =  InScattering2(_camPos, worldPos , extinction, 1.0, 1.0, 1.0);
								
				float visib=1;

				dpth = length (worldPos - _camPos);
				if (dpth<=_global_depth)
				{
					visib=1-exp(-1* (4*dpth/_global_depth));
				}			
				

////				if( ((rightDir && oceanCloserThanTerrain) || (length(worldPos)<(Rg+1000))) && (fakeOcean==1.0))
//				if( ((rightDir && oceanCloserThanTerrain)) && (fakeOcean==1.0))
//				{
					return float4(lerp(hdr(oceanColor*extinction), hdr(inscatter),_global_alpha * visib),_fade);	
//				}
				
//				return float4(hdr(inscatter),_global_alpha * visib*_fade);		    	    
//				return float4(hdr(inscatter),_global_alpha * visib);	
//				return float4(hdr(inscatter), 1);	
//				return float4(dpth,dpth,dpth,1);
			}
			ENDCG
	    }
	}
}