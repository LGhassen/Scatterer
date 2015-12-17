Shader "Proland/Atmo/SkyExtinction" 
{
	SubShader 
	{
		//Tags {"Queue" = "Background" "RenderType"="" }
		 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
	
    	Pass 
    	{
    	 Tags {"QUEUE"="Geometry+1" "IgnoreProjector"="True" }
    		ZWrite Off
    		ZTest Off
    		
    		cull Front
    
    		Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			//#pragma only_renderers d3d9
			#pragma glsl
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Utility.cginc"
			#include "AtmosphereNew.cginc"
			
			
			//uniform float _Alpha_Cutoff;
			uniform float _viewdirOffset;
			uniform float _experimentalAtmoScale;
			
			uniform float _sunglareScale;
			uniform float _Alpha_Global;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Globals_Origin;
			uniform float _Globals_ApparentDistance;
			uniform float _Extinction_Tint;
			uniform float extinctionMultiplier;
			uniform float extinctionRimFade;
			uniform float _rimQuickFixMultiplier;
			
			uniform sampler2D _Sun_Glare;
			uniform float3 _Sun_WorldSunDir;
			uniform float4x4 _Sun_WorldToLocal;
			
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
			    ///OUT.dir = float3(1.0,0.0,0.0);

			   //float3x3 wtl = float3x3(_Sun_WorldToLocal);
			    float3x3 wtl = _Sun_WorldToLocal;
			    
			    // apply this rotation to view dir to get relative viewdir
			    OUT.relativeDir = mul(wtl, OUT.dir);
    
    			OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
    			OUT.uv = v.texcoord.xy;
    			return OUT;
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
			
								
			float4 frag(v2f IN) : COLOR
			{

			    float3 extinction = float3(1,1,1);
				
			    float3 WCP = _Globals_WorldCameraPos;
				
				Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;
	
				float3 viewdir=normalize(IN.dir);
				viewdir.x+=_viewdirOffset;
				viewdir=normalize(viewdir);
				
				float3 camera=WCP - _Globals_Origin*_Globals_ApparentDistance;
				//camera *= scale;
				//camera += viewdir * max(shaftWidth, 0.0);
				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;
				float r0 = r;
				float mu0 = mu;

#if !defined(SHADER_API_D3D9)
	float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
#else
    float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,1e30);
#endif

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
    
    			extinction = Transmittance(r, mu);
    			float average=(extinction.r+extinction.g+extinction.b)/3;
    			
    			extinction = float3(_Extinction_Tint*extinction.r + (1-_Extinction_Tint)*average,
    								_Extinction_Tint*extinction.g + (1-_Extinction_Tint)*average,
    								_Extinction_Tint*extinction.b + (1-_Extinction_Tint)*average);
    			
    			float interSectPt= intersectSphere2(WCP - _Globals_Origin*_Globals_ApparentDistance,WCP - _Globals_Origin*_Globals_ApparentDistance+viewdir,_Globals_Origin,Rg*_rimQuickFixMultiplier);
				bool rightDir = (interSectPt > 0) ;
				if (!rightDir)
				{
					extinction= float3(1.0,1.0,1.0)*extinctionRimFade +(1-extinctionRimFade)*extinction;
				}
    									
    			
				
				return float4(extinctionMultiplier * extinction,1.0);			    
			}
			
			ENDCG

    	}
	}
}