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

    			float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
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
				
				return float4(extinctionMultiplier * extinction,1.0);			    
			}
			
			ENDCG

    	}
	}
}