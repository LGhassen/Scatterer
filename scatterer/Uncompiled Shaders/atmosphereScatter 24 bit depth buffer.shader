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
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members screenPos)
//#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma glsl
			#pragma target 3.0
			#include "Atmosphere3.cginc"
			
			//the next line allows going over opengl 512 arithmetic ops limit
			#pragma profileoption NumMathInstructionSlots=65535
			
//			float3 normalValues;
//			float depthValue;

			
			sampler2D _MainTex, _SkyDome;
			
			uniform float4x4 _ViewProjInv;
			
//			uniform float _OceanRadius;
			uniform float _Scale;
			uniform float _global_alpha;
			uniform float _Exposure;
			uniform float _global_depth;
//			uniform float _global_depth2;
			uniform float3 _inCamPos;
			uniform float3 _Globals_Origin;
//			uniform float3 _CameraForwardDirection;
//			uniform sampler2D _CameraDepthNormalsTexture;
//			uniform sampler2D _CameraDepthTexture;
			uniform sampler2D _customDepthTexture;
//			uniform float4x4 _FrustumCorners;
			uniform float4 _MainTex_TexelSize;
			
//			uniform float _irradianceFactor;
//			uniform float _Ocean_Sigma;
//			uniform float _Ocean_Threshold;
			
//			uniform float _inscatteringCoeff;
//			uniform float _extinctionCoeff;
			uniform float _openglThreshold;
			uniform float _globalThreshold;
			uniform float _edgeThreshold;
			uniform float _horizonDepth;
			
//			uniform float3 SUN_DIR;
			
			uniform float4x4 _Globals_CameraToWorld;
			//uniform float4x4 _Globals_ScreenToCamera;
			//uniform float3 _Globals_WorldCameraPos;
			//uniform float3 _Globals_Origin;
						
			struct v2f 
			{
				//float4 pos : POSITION;
				//float4 screenPos;
				float4 pos : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float2 uv_depth : TEXCOORD2;

			};
			
			v2f vert( appdata_base v )
			{
				v2f o;
				//half index = v.vertex.z;
//				v.vertex.z = 0.1;
				
				
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
//				

//				
//				float4 ray_x0,ray_x1;
				
//				ray_x0= _FrustumCorners[TL]*(1.0-uv.x)+_FrustumCorners[TR]*uv.x;
//				ray_x1= _FrustumCorners[BL]*(1.0-uv.x)+_FrustumCorners[BR]*uv.x;

//				ray_x0= _FrustumCorners[0]*(1.0-uv.x)+_FrustumCorners[1]*uv.x;
//				ray_x1= _FrustumCorners[3]*(1.0-uv.x)+_FrustumCorners[2]*uv.x;
////				
//				o.interpolatedRay = ray_x0 * (1.0-uv.y) + ray_x1 * uv.y;
				
				o.uv=o.screenPos.xy/o.screenPos.w;
				
				//o.uv = v.texcoord.xy;
				o.uv_depth = o.uv;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif				
				
//				o.interpolatedRay = _FrustumCorners[(int)index];
//				o.interpolatedRay.w = index;
				
				COMPUTE_EYEDEPTH(o.screenPos.z);
		   		TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				return o;
			}
			
			
//			float4 GetWorldPositionFromDepth( float2 uv_depth )
//			{    
//        		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv_depth);
////        		float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,uv_depth)));
//        		
//        		float4 H = float4(uv_depth.x*2.0-1.0, (uv_depth.y)*2.0-1.0, depth, 1.0);
//        		
//        		float4 D = mul(_ViewProjInv,H);
//        		//float4 D = mul(_ViewProjInv,depth);
//        		return D/D.w;
//			}
			
			
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
//				DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv_depth.xy), depthValue, normalValues);
			
//				float4 col = tex2D(_MainTex, i.uv);
				
//				float dpth =  Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth)));	
				float dpth =  Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_customDepthTexture,i.uv_depth)));	

				if(dpth >= _edgeThreshold) return float4(0.0,0.0,0.0,0.0);
				
				
				float visib=1;

				if (dpth<=_global_depth)
				{
					visib=1-exp(-1* (4*dpth/_global_depth));
				}
				
				
				
				
				//float dpth=_global_depth2 * (depthValue);			
				//float3 worldPos = (_inCamPos-_Globals_Origin + dpth *_Scale * i.interpolatedRay);
				//float3 worldPos = float3(GetWorldPositionFromDepth(i.uv_depth));
				
//				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth);
				float depth = SAMPLE_DEPTH_TEXTURE(_customDepthTexture, i.uv_depth);

        		#if !defined(SHADER_API_OPENGL)
        		float4 H = float4(i.uv_depth.x*2.0f-1.0f, (i.uv_depth.y)*2.0f-1.0f, depth, 1.0f);
        		#else	
        		float4 H = float4(i.uv_depth.x*2.0f-1.0f, i.uv_depth.y*2.0f-1.0f, depth*2.0f-1.0f, 1.0f);
        		#endif
        		
        		float4 D = mul(_ViewProjInv,H);

        		float3 worldPos = D/D.w;
        		
        		#if !defined(SHADER_API_D3D9)

        		if (length(worldPos) < (Rg+_openglThreshold))
        		{
        			worldPos=(Rg+_openglThreshold)*normalize(worldPos);
        		}
				#endif
			    
				float3 extinction = float3(0,0,0);
				
				float irradianceFactor=0.0;
				float3 inscatter =  InScattering((_inCamPos-_Globals_Origin) *_Scale , worldPos *_Scale , extinction, 1.0, 1.0, 1.0);
				
//				col.rgb = tan(1.37 * col.rgb) / tan(1.37);//RGB to reflectance, whatever that means
				
//				float3 sunL;
//			    float3 skyE;
			            
//    			float3 fn= mul((float3x3)_Globals_CameraToWorld ,normalValues); //WorldNormal
//			    SunRadianceAndSkyIrradiance(worldPos, fn, SUN_DIR, sunL, skyE, _Scale);
			    
		    	// diffuse ground color
		    	
//		    	float cTheta = dot(fn, SUN_DIR);
//			    float3 groundColor = extinction * 1.5 * col.rgb * (sunL * max(cTheta, 0.0) + skyE) / 3.14159265;
			    
//			    float3 reflectedLight = GetReflectedLight( worldPos *_Scale, col.rgb, extinction, _irradianceFactor, normalValues,_Globals_CameraToWorld);
			    //float3 groundColor = GetReflectedLight( worldPos *_Scale, col.rgb, extinction, _irradianceFactor, normalValues,_Globals_CameraToWorld);
			    
//			    if(length(worldPos) <= (Rg+ _Ocean_Threshold ))
//			    {
//			    	
//			        float3 v = normalize(worldPos + (_inCamPos-_Globals_Origin));
////        			groundColor = OceanRadiance(SUN_DIR, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color);
//        			groundColor = OceanRadiance(-SUN_DIR, -v, normalize(worldPos), _Ocean_Sigma, sunL, skyE, float3(0.0039f, 0.0156f, 0.047f));
//				}
				
				
//				col.rgb = col.rgb * extinction + inscatter;
//				col.rgb =  col.rgb *(1-_extinctionCoeff) + col.rgb * extinction * _extinctionCoeff +  inscatter * _inscatteringCoeff;
								

//				float ht=length(_inCamPos-_Globals_Origin)-_OceanRadius;
//				
//				if( ht<=1)
//				{
//				
////				col.rgb = OceanRadiance(SUN_DIR, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color);
//    			float3 v = normalize(worldPos - (_inCamPos-_Globals_Origin));
//				col.rgb = OceanRadiance(SUN_DIR, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color);
//				
////				float3 V = normalize(IN.p);
////    			float3 P = V * max(length(IN.p), _Deform_Radius + 10.0);
////    			float3 v = normalize(P - WCP);
//
//				//I think V is the view direction
//				//v
//				
//				}
				
				
				//col2.rgb =   col.rgb * extinction + inscatter;
																
				

				
				
//				if (length(inscatter)<_extinctionCoeff)
//				{
//					return float4( hdr(col2.rgb) * (length(inscatter)/_extinctionCoeff)  +  col.rgb * (1-(length(inscatter)/_extinctionCoeff) ), _global_alpha*visib);
//				}
//				
//				
//				//if (length(inscatter)>_inscatteringCoeff)
//				//{
//					return float4(hdr(col2.rgb), _global_alpha*visib);					
//				//}

//				return float4(hdr(reflectedLight +  inscatter),_global_alpha*visib);
//				return float4(hdr(groundColor * extinction +  inscatter),_global_alpha*visib);
				
				return float4(hdr(inscatter),_global_alpha * visib);
//				return float4(depthOutput,depthOutput,depthOutput,_global_alpha*visib);
//				return float4(float3(i.interpolatedRay),1.0);

				//return float4(hdr(col2.rgb), visib);							
				//return float4(hdr(col.rgb), _global_alpha*visib);				
			    
			}
			

			

			ENDCG
	    }
	}
}