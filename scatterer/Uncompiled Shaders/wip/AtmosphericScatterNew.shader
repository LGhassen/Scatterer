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
	
		Tags { "Queue" = "Transparent" } 
	    Pass 
	    {
	    	ZTest Always
	    	ZWrite off
	    	Fog { Mode Off }
	    	Cull front 
	    	Blend SrcAlpha OneMinusSrcAlpha
	    
			CGPROGRAM			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 3.0
			#include "Atmosphere3.cginc"
			
			//the next line allows going over opengl 512 arithmetic ops limit
			#pragma profileoption NumMathInstructionSlots=65535
			
			float3 normalValues;
			float depthValue;
			
			sampler2D _MainTex, _SkyDome;
			uniform float _OceanRadius;
			uniform float _Scale;
			uniform float _global_alpha;
			uniform float _Exposure;
			uniform float _global_depth;
			uniform float _global_depth2;
			uniform float3 _inCamPos;
			uniform float3 _Globals_Origin;
			uniform float3 _CameraForwardDirection;
			uniform sampler2D _CameraDepthNormalsTexture;
//			uniform sampler2D _CameraDepthTexture;
			uniform float4x4 _FrustumCorners;
			uniform float4 _MainTex_TexelSize;
			
			uniform float _inscatteringCoeff;
			uniform float _extinctionCoeff;
			
			//uniform float3 SUN_DIR;
			
			uniform float4x4 _Globals_CameraToWorld;
			//uniform float4x4 _Globals_ScreenToCamera;
			//uniform float3 _Globals_WorldCameraPos;
			//uniform float3 _Globals_Origin;
						
			struct v2f 
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
			};
			
			v2f vert( appdata_img v )
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif				
				
				o.interpolatedRay = _FrustumCorners[(int)index];
				o.interpolatedRay.w = index;
				
				return o;
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
			DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv_depth.xy), depthValue, normalValues);
			
				float4 col = tex2D(_MainTex, i.uv);
				float4 col2=col;//
				float4 skyCol = tex2D(_SkyDome, i.uv_depth);
				
//				float dpth = _global_depth2 * Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth)));	

					float dpth=_global_depth2 * (depthValue);			
				float3 worldPos = (_inCamPos-_Globals_Origin + dpth *_Scale * i.interpolatedRay);
								
//				
				if(dpth >= 0.9) return float4(0.0,0.0,0.0,0.0);
				

			    
				float3 extinction = float3(0,0,0);
//				float3 inscatter = InScattering(_inCamPos*_Scale, worldPos*_Scale, extinction, 1.0);
				//float3 inscatter = InScattering(_inCamPos+_Globals_Origin, absDir, absWorldPos, extinction, 1.0, dist);
				float3 inscatter =  InScattering((_inCamPos-_Globals_Origin) *_Scale , worldPos *_Scale , extinction, 1.0, 1.0);
				
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
				
				
				col2.rgb =   col.rgb * extinction + inscatter;
																
				
				float visib=1;
				if (dpth<=_global_depth){
//				visib=dpth/_global_depth;
				visib=1-exp(-1* (4*dpth/_global_depth));
				}
				
				
				if (length(inscatter)<_extinctionCoeff)
				{
					return float4( hdr(col2.rgb) * (length(inscatter)/_extinctionCoeff)  +  col.rgb * (1-(length(inscatter)/_extinctionCoeff) ), _global_alpha*visib);
				}
				
				
				//if (length(inscatter)>_inscatteringCoeff)
				//{
					return float4(hdr(col2.rgb), _global_alpha*visib);
				//}

				//return float4(hdr(col2.rgb), visib);							
				//return float4(hdr(col.rgb), _global_alpha*visib);				
			    
			}
			

			

			ENDCG
	    }
	}
}
