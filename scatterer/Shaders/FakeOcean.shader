Shader "Scatterer/FakeOcean"
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

			

			uniform float _Exposure;

	
			uniform float fakeOcean;
			
			uniform float _fade;
			
			uniform float3 _Ocean_Color;

			uniform float3 _cameraPos; 		// camera position relative to planet's origin
			uniform float3 _planetPos;   // planet world pos
			
			uniform float _Ocean_Sigma;
			
//			uniform float4x4 _PlanetToWorld;
//			
//			uniform float4x4 _WorldToPlanet;


//
//			
//
//			uniform float4 _MainTex_TexelSize;
//
//			uniform float _openglThreshold;
//			uniform float _globalThreshold;
//			uniform float _edgeThreshold;
//			uniform float _horizonDepth;
//
//			uniform float4x4 _Globals_CameraToWorld;
						
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 worldPos: TEXCOORD2;

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
				
				
//				float4x4 object2local= mul(_WorldToPlanet,_Object2World);
//				
//				o.worldPos = mul (object2local, v.vertex).xyz;
				
				
				o.worldPos = mul (_Object2World, v.vertex).xyz;
				

				
//				o.worldPos = mul (UNITY_MATRIX_MV, v.vertex).xyz;
				
				
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				
//				o.worldPos=o.pos;

				return o;
			}
			
			
//			float3 InScattering2(float3 camera, float3 _point, out float3 extinction, float shaftWidth, float scaleCoeff, float irradianceFactor) 
//			{
//			// single scattered sunlight between two points
//			// camera=observer
//			// point=point on the ground
//			// sundir=unit vector towards the sun
//			// return scattered light and extinction coefficient
//
//    			float3 result = float3(0,0,0);
//    			extinction = float3(1,1,1);
//        
//    			float3 viewdir = _point - camera;
//    			float d = length(viewdir)* scaleCoeff;
//    			viewdir = viewdir / d;
//    
//    			/////////////////////experimental block begin
//    			Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;
//				viewdir.x+=_viewdirOffset;
//				viewdir=normalize(viewdir);
//				/////////////////////experimental block end
//	
//	
//    			float r = length(camera)* scaleCoeff;
//        
//    			if (r < 0.9 * Rg) 
//    			{
//        			camera.y += Rg;
//        			_point.y += Rg;
//        			r = length(camera)* scaleCoeff;
//    			}
//    			
//    			float rMu = dot(camera, viewdir);
//    			float mu = rMu / r;
//    			float r0 = r;
//    			float mu0 = mu;
//    			_point -= viewdir * clamp(shaftWidth, 0.0, d);
//
//    			float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,1e30);
//    			float din = max(-rMu - deltaSq, 0.0);
//    
//    			if (din > 0.0 && din < d) 
//    			{
//        			camera += din * viewdir;
//        			rMu += din;
//        			mu = rMu / Rt;
//        			r = Rt;
//        			d -= din;
//    			}
//
//    			if (r <= Rt) 
//    			{
//        			float nu = dot(viewdir, SUN_DIR);
//        			float muS = dot(camera, SUN_DIR) / r;
//		
//        			float4 inScatter;
//
//        			if (r < Rg + 600.0) 
//        			{
//            			// avoids imprecision problems in aerial perspective near ground
//            			float f = (Rg + 600.0) / r;
//            			r = r * f;
//            			rMu = rMu * f;
//            			_point = _point * f;
//        			}
//
//        			float r1 = length(_point);
//        			float rMu1 = dot(_point, viewdir);
//        			float mu1 = rMu1 / r1;
//        			float muS1 = dot(_point, SUN_DIR) / r1;
//
//        			if (mu > 0.0)
//        			{
//          				extinction = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0);
//            		}
//        			else
//        			{
//	            		extinction = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0);
//    	        	}
//
//        			const float EPS = 0.004;
//        			float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
//        
//        			if (abs(mu - lim) < EPS) 
//        			{
//            			float a = ((mu - lim) + EPS) / (2.0 * EPS);
//
//            			mu = lim - EPS;
//            			r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
//            			mu1 = (r * mu + d) / r1;
//            
//            			float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            			float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            			float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//
//		           		mu = lim + EPS;
//        		   		r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
//            			mu1 = (r * mu + d) / r1;
//            
//            			inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            			inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            		
//            			float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//
//            			inScatter = lerp(inScatterA, inScatterB, a);
//            
//            			irradianceFactor=1.0;
//            			//Not sure about where irradianceFactor goes
//        			} 
//        			else 
//        			{
//            			float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            			float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            			inScatter = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//        			}
//
//        			// avoids imprecision problems in Mie scattering when sun is below horizon
//        			inScatter.w *= smoothstep(0.00, 0.02, muS);
//
//        			float3 inScatterM = GetMie(inScatter);
//        			float phase = PhaseFunctionR(nu);
//        			float phaseM = PhaseFunctionM(nu);
//        			result = inScatter.rgb * phase + inScatterM * phaseM;
//    			} 
//
//    			return result * SUN_INTENSITY; //sun_intensity may as well be hardcoded to 100, since this shader only handles inscattering
//    										   //and has no terrain color information, sun intensity has the same effect as exposure
//			}
			
			
			
			float3 hdr(float3 L) 
			{
    			L = L * _Exposure;
//    			L = L * 1;
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
				

//			if (fakeOcean==1.0)				
//			{
				

				
				float3 worldPos=i.worldPos-_planetPos;
//				float3 worldPos=i.worldPos;
//				_camPos = _camPos + _planetPos;
				
//				float3 worldPos=i.worldPos;
				
				float3 V = normalize(worldPos);

    			float3 P = V * max(length(worldPos), Rg + 10.0);
    			
    			
    			float3 v = normalize(P - _cameraPos);
    			
//    			return float4(normalize(_cameraPos).r,normalize(_cameraPos).g,normalize(_cameraPos).b,1.0);
    						
				float3 fn = float3(0,0,1); //ocean normal
    			
				float cTheta = dot(fn, SUN_DIR);
				float vSun = dot(V, SUN_DIR);
				
			    float3 sunL;
				float3 skyE;
			    			
			    			
			    SunRadianceAndSkyIrradiance(P, fn, SUN_DIR, sunL, skyE);
							
//				float3 oceanColor = OceanRadiance(SUN_DIR, -v, V, _Ocean_Sigma, 10.0, 10.0, _Ocean_Color*10);
        		float3 oceanColor = OceanRadiance(SUN_DIR, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color*10);
        		
//        		return float4(sunL.r, skyE.r,0.0, 1.0);

//				return float4(length(worldPos)/(Rg*2),length(worldPos)/(Rg*2),length(worldPos)/(Rg*2),1.0);	
				
				return float4(hdr(oceanColor),1);		    	    
				
//	    	    return float4(1.0,0.0,0.0,1.0);		    	    
//			}

        		
        		
//				return float4(0.0,0.0,0.0,0.0);		    	    
			}
			ENDCG
	    }
	}
}