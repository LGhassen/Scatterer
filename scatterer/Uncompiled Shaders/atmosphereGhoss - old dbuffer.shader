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
			
			//the next line allows going over opengl 512 arithmetic ops limit
			#pragma profileoption NumMathInstructionSlots=65535

			
			sampler2D _MainTex, _SkyDome;
			
			uniform float4x4 _ViewProjInv;

			uniform float _Scale;
			uniform float _global_alpha;
			uniform float _Exposure;
			uniform float _global_depth;

			uniform float3 _camPos; 		// camera position relative to planet's origin
//			uniform float3 _Globals_Origin;

//			uniform sampler2D _CameraDepthNormalsTexture;
			uniform sampler2D _CameraDepthTexture;
//			uniform sampler2D _customDepthTexture;

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
				
				COMPUTE_EYEDEPTH(o.screenPos.z);
		   		TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				return o;
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
			
			
			float3 hdr(float3 L) 
			{
    			L = L * _Exposure;
    			L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
    			L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
    			L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
    		return L;
    		}
			
			half4 frag(v2f i) : COLOR 
			{
				
				float dpth =  Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth)));	
//				float dpth =  Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_customDepthTexture,i.uv_depth)));	
				//this dpth value is only used for edge threshold checks and depth distance
				//I realize this isn't very efficient but I'll change it later

//				float visib=1;
//
//				if (dpth<=_global_depth)
//				{
//					visib=1-exp(-1* (4*dpth/_global_depth));
//				}
				
				
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
//				float depth = SAMPLE_DEPTH_TEXTURE(_customDepthTexture, i.uv_depth);

        		#if !defined(SHADER_API_OPENGL)
        		float4 H = float4(i.uv_depth.x*2.0f-1.0f, (i.uv_depth.y)*2.0f-1.0f, depth, 1.0f);
        		#else	
        		float4 H = float4(i.uv_depth.x*2.0f-1.0f, i.uv_depth.y*2.0f-1.0f, depth*2.0f-1.0f, 1.0f);
        		#endif
        		
        		float4 D = mul(_ViewProjInv,H);

        		float3 worldPos = D/D.w;
	
        		float interSectPt= intersectSphere2(_camPos,worldPos,float3(0.0,0.0,0.0),Rg);
        		
        		float3 worldPos2= _camPos + interSectPt * (worldPos - (_camPos));
        		bool intersectExists = (interSectPt !=  -1.0); // this ensures an intersection point exists
        		
        		bool rightDir = dot (worldPos2 - (_camPos), worldPos - (_camPos)) > 0 ;  //this ensures that we're looking in the right direction
        																				  //That is, the ocean surface intersection point is in front of us
        																				  //If we look up the intersection point is behind us and we don't want to use that
        		

        		
        		#if !defined(SHADER_API_D3D9)
        		if ((dpth >= _edgeThreshold)  &&  !(intersectExists && rightDir))
        		{
        			return float4(0.0,0.0,0.0,0.0);
        		}
        		#else
        		if ((dpth >= 0.99)  &&  !(intersectExists && rightDir))
        		{
        			return float4(0.0,0.0,0.0,0.0);
        		}
        		#endif
        		
        		bool oceanCloserThanTerrain = ( length (worldPos2 -_camPos) < length (worldPos - _camPos)); //this condition ensures the ocean is in front of the terrain
        																								   //if the terrain is in front of the ocean we don't want to cover it up
        																								   //with the wrong postprocessing depth
        		
        		if ((intersectExists) && (rightDir) && oceanCloserThanTerrain)
        		{
        				worldPos=worldPos2;
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

				float3 inscatter =  InScattering(_camPos, worldPos , extinction, 1.0, 1.0, 1.0);
								
				float visib=1;

				dpth = length (worldPos - _camPos);
				if (dpth<=_global_depth)
				{
					visib=1-exp(-1* (4*dpth/_global_depth));
				}			
				
				return float4(hdr(inscatter),_global_alpha * visib);			    
			}
			ENDCG
	    }
	}
}