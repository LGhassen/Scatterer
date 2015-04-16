Shader "Sky/AtmosphereImageEffect"
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
			
			sampler2D _MainTex, _SkyDome;
			uniform float _Scale;
			uniform float _global_alpha;
			uniform float _Exposure;
			uniform float _global_depth;
			uniform float3 _inCamPos;
			uniform float3 _Globals_Origin;
			uniform float3 _CameraForwardDirection;
			uniform sampler2D _CameraDepthTexture;
			uniform float4x4 _FrustumCorners;
			uniform float4 _MainTex_TexelSize;
			
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
				float4 col = tex2D(_MainTex, i.uv);
				float4 skyCol = tex2D(_SkyDome, i.uv_depth);
				
				float dpth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv_depth)));
				float3 worldPos = (_inCamPos-_Globals_Origin + dpth * i.interpolatedRay);
				
				
//				float3 relWorldPos = dpth * i.interpolatedRay; //relative to camera
//				float3 relDir = normalize(relWorldPos); //direction from camera to point
////				float dist=abs(relWorldPos);
//				float dist=length(relWorldPos);
				
				//If the depth buffer has not been written into this must be the sky, copy the sky color into this frag
				//This acts as a mask so we can tell what areas is sky and what is not. The sky needs its own method for scattering so has 
				//been calcuated before hand and written into a render texture. 
				//This is not a very good method for masking but as Unity does not at this time have stencil buffers its the 
				//only easy method I can think off. 
				//EDIT - looks like Unity may not support stencil buffers in post process effects so it may not even be a option.
				//if(dpth == 1.0) return skyCol;
				if(dpth == 1.0) return float4(0.0,0.0,0.0,0.0);
				
//				float3x3 wtl= _Globals_CameraToWorld;
//				float3 absDir = normalize(mul(wtl,relWorldPos));
//				float3 absWorldPos=_inCamPos+ absDir * dist;

				//Quaternion q = Quaternion.SetFromToRotation (relDir,_CameraForwardDirection);
//				float3 absDir = relWorldPos * q;
				//float3 absDir = relDir * q;
				//float3 absWorldPos=_inCamPos+ absDir * dist;
			    
				float3 extinction = float3(0,0,0);
//				float3 inscatter = InScattering(_inCamPos*_Scale, worldPos*_Scale, extinction, 1.0);
				//float3 inscatter = InScattering(_inCamPos+_Globals_Origin, absDir, absWorldPos, extinction, 1.0, dist);
				float3 inscatter = _global_depth*  InScattering((_inCamPos-_Globals_Origin) , worldPos , extinction, 1.0, _Scale);
//				col.rgb = col.rgb * extinction + inscatter;
				col.rgb = col.rgb * extinction + inscatter;
				float visib=1;
				if (dpth<=0.015){
				visib=dpth/0.015;
				}
				
				
				return float4(hdr(col.rgb), _global_alpha*visib);				
			    
			}
			

			

			ENDCG
	    }
	}
}
