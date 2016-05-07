Shader "Scatterer/sunFlare" 
{
	SubShader 
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	
    	Pass 
    	{
    		ZWrite Off
    		ZTest Off
    		cull off

//			Blend One One //additive
			Blend One OneMinusSrcColor //"reverse" soft-additive


			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl
			
			uniform float3 _Globals_WorldCameraPos;
			uniform float3 _Globals_Origin;
			
			uniform float sunGlareScale;
			uniform float sunGlareFade;
			
			uniform sampler2D sunSpikes;
			uniform sampler2D sunFlare;
			uniform sampler2D sunGhost1;
			uniform sampler2D sunGhost2;
			uniform sampler2D sunGhost3;
			
			uniform sampler2D _customDepthTexture;  //depth buffer, to check if sun is behin terrain
													//I would love it if this wasn't necessary, unfortunately, mountains far away
													//don't generate colliders so raycasting alone isn't enough
													
													
			uniform float3 flareSettings;  //intensity, aspect ratio, scale
			uniform float3 spikesSettings;
			
			uniform float4x4 ghost1Settings1;// each row is an instance of a ghost
			uniform float4x4 ghost1Settings2;// for each row: intensity, aspect ratio, scale, position on sun-screenCenter line
											 // intensity of 0 means nothing defined
			
			uniform float4x4 ghost2Settings1;
			uniform float4x4 ghost2Settings2;
			
			uniform float4x4 ghost3Settings1;
			uniform float4x4 ghost3Settings2;
											
													
													
			uniform float Rg;
			uniform float Rt;
			uniform sampler2D _Sky_Transmittance;
			
			uniform float useTransmittance;
			
			uniform float3 _Sun_WorldSunDir;
			
			uniform float3 sunViewPortPos;
			uniform float aspectRatio;
			
			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
    			OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
    			OUT.uv = v.texcoord.xy;
    			return OUT;
			}
			
			float2 GetTransmittanceUV(float r, float mu) {
    			float uR, uMu;
				//#ifdef TRANSMITTANCE_NON_LINEAR
    			uR = sqrt((r - Rg) / (Rt - Rg));
    			uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
				//#else
				//    uR = (r - Rg) / (Rt - Rg);
				//    uMu = (mu + 0.15) / (1.0 + 0.15);
				//#endif
    			return float2(uMu, uR);
			}
			
			float3 Transmittance(float r, float mu) 
			{
    			float2 uv = GetTransmittanceUV(r, mu);
//    			return tex2Dlod(_Sky_Transmittance, float4(uv,0,0)).rgb; //shouldn't need tex2Dlod
    			return tex2D(_Sky_Transmittance, uv).rgb; //shouldn't need tex2Dlod
			}
			
			float SQRT(float f, float err)
			{
    			return f >= 0.0 ? sqrt(f) : err;
			}
			
			float3 getExtinction(float3 camera, float3 viewdir)
			{
				float3 extinction = float3(1,1,1);

//				Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;		//not really noticeable

				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;

    			float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,0.000001);
//    			float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);

    			float din = max(-rMu - deltaSq, 0.0);
    			if (din > 0.0)
    			{
        			camera += din * viewdir;
        			rMu += din;
        			mu = rMu / Rt;
        			r = Rt;
    			}

//    			extinction = Transmittance(r, mu);
//
//    			if (r > Rt) 
//    			{
//    				extinction = float3(1,1,1);
//    			} 


    			extinction = (r > Rt) ? float3(1,1,1) : Transmittance(r, mu);

    			return extinction;
    		}
			

			float4 frag(v2f IN) : COLOR
			{

			    float3 WSD = _Sun_WorldSunDir;
			    float3 WCP = _Globals_WorldCameraPos;
			    
				float3 sunColor=0;
				
				//move aspectRatio precomputations to CPU?
				sunColor+=flareSettings.x * (tex2D(sunFlare,(IN.uv.xy-sunViewPortPos.xy)*float2(aspectRatio * flareSettings.y,1)* flareSettings.z * sunGlareScale+0.5).rgb);
			    sunColor+=spikesSettings.x * (tex2D(sunSpikes,(IN.uv.xy-sunViewPortPos.xy)*float2(aspectRatio * spikesSettings.y ,1)* spikesSettings.z * sunGlareScale+0.5).rgb);
			    		    		    
				float2 toScreenCenter=sunViewPortPos.xy-0.5;
			   	float3 ghosts=0;
			
				//ghost 1
				for (int i=0; i<4; ++i)
    			{
        					ghosts+=ghost1Settings1[i].x * (tex2D(sunGhost1,(IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost1Settings1[i].w))*
        					float2(aspectRatio*ghost1Settings1[i].y,1)*ghost1Settings1[i].z+0.5).rgb);
        					
        					ghosts+=ghost1Settings2[i].x * (tex2D(sunGhost1,(IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost1Settings2[i].w))*
        					float2(aspectRatio*ghost1Settings2[i].y,1)*ghost1Settings2[i].z+0.5).rgb);
				}			
	
				//ghost 2
				for (int j=0; j<4; ++j)
    			{        					        					
        					ghosts+=ghost2Settings1[j].x * (tex2D(sunGhost2,(IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost2Settings1[j].w))*
        					float2(aspectRatio*ghost2Settings1[j].y,1)*ghost2Settings1[j].z+0.5).rgb);
        					
        					ghosts+=ghost2Settings2[j].x * (tex2D(sunGhost2,(IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost2Settings2[j].w))*
        					float2(aspectRatio*ghost2Settings2[j].y,1)*ghost2Settings2[j].z+0.5).rgb);
				}
				
				//ghost 3
				for (int k=0; k<4; ++k)
    			{        					        					
        					ghosts+=ghost3Settings1[k].x * (tex2D(sunGhost3,(IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost3Settings1[k].w))*
        					float2(aspectRatio*ghost3Settings1[k].y,1)*ghost3Settings1[k].z+0.5).rgb);
        					
        					ghosts+=ghost3Settings2[k].x * (tex2D(sunGhost3,(IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost3Settings2[k].w))*
        					float2(aspectRatio*ghost3Settings2[k].y,1)*ghost3Settings2[k].z+0.5).rgb);
				}

			   	ghosts=ghosts * smoothstep(0,1,1-length(toScreenCenter));
			   	
			   	sunColor+=ghosts;
			    
				float depth =  tex2D(_customDepthTexture,sunViewPortPos.xy);  //if there's something in the way don't render the flare	
				
				if (depth < 1.0)
					return float4(0.0,0.0,0.0,0.0);
					
				float3 extinction = getExtinction(WCP,WSD);
				if(useTransmittance > 0.0)
					sunColor*=extinction;
					
				sunColor*=sunGlareFade;

				return float4(sunColor,1.0);
				
			}
			
			ENDCG
    	}
	}
}