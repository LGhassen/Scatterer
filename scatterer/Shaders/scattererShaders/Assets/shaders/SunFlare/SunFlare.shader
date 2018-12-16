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
			#include "../CommonAtmosphere.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl

			
			uniform float sunGlareScale;
			uniform float sunGlareFade;
			uniform float ghostFade;
			
			uniform sampler2D sunSpikes;
			uniform sampler2D sunFlare;
			uniform sampler2D sunGhost1;
			uniform sampler2D sunGhost2;
			uniform sampler2D sunGhost3;
			
			uniform sampler2D _customDepthTexture;  //depth buffer, to check if sun is behin terrain
													//I would love it if this wasn't necessary, unfortunately, mountains far away
													//don't generate colliders so raycasting alone isn't enough

			uniform sampler2D extinctionTexture;
													
			uniform float3 flareSettings;  //intensity, aspect ratio, scale
			uniform float3 spikesSettings;
			
			uniform float4x4 ghost1Settings1;// each row is an instance of a ghost
			uniform float4x4 ghost1Settings2;// for each row: intensity, aspect ratio, scale, position on sun-screenCenter line
											 // intensity of 0 means nothing defined
			
			uniform float4x4 ghost2Settings1;
			uniform float4x4 ghost2Settings2;
			
			uniform float4x4 ghost3Settings1;
			uniform float4x4 ghost3Settings2;


			//uniform float useTransmittance;
			

			
			uniform float3 sunViewPortPos;
			uniform float aspectRatio;
			
			struct v2f 
			{
    			float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v, out float4 outpos: SV_POSITION)
			{
				v2f OUT;
				v.vertex.y = v.vertex.y *_ProjectionParams.x;
    			outpos = float4(v.vertex.xy, 1.0, 1.0);
    			OUT.uv = v.texcoord.xy;

//#if UNITY_UV_STARTS_AT_TOP
//				OUT.uv.y = 1.0 - OUT.uv.y;
//#endif
    			return OUT;
			}


			float4 frag(v2f IN, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
			{
//
//			    float3 WSD = _Sun_WorldSunDir;
//			    float3 WCP = _Globals_WorldCameraPos;
			    
				float3 sunColor=0;
				
				//move aspectRatio precomputations to CPU?
				sunColor+=flareSettings.x * (tex2Dlod(sunFlare,float4((IN.uv.xy-sunViewPortPos.xy)*float2(aspectRatio * flareSettings.y,1)* flareSettings.z * sunGlareScale+0.5,0,0) ).rgb);
			    sunColor+=spikesSettings.x * (tex2Dlod(sunSpikes,float4((IN.uv.xy-sunViewPortPos.xy)*float2(aspectRatio * spikesSettings.y ,1)* spikesSettings.z * sunGlareScale+0.5,0,0) ).rgb);
			    		    		    
				float2 toScreenCenter=sunViewPortPos.xy-0.5;
			   	float3 ghosts=0;
			
				//ghost 1
				for (int i=0; i<4; ++i)
    			{
        					ghosts+=ghost1Settings1[i].x * (tex2Dlod(sunGhost1,float4((IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost1Settings1[i].w))*
        					float2(aspectRatio*ghost1Settings1[i].y,1)*ghost1Settings1[i].z+0.5,0,0)).rgb);
        					
        					ghosts+=ghost1Settings2[i].x * (tex2Dlod(sunGhost1,float4((IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost1Settings2[i].w))*
        					float2(aspectRatio*ghost1Settings2[i].y,1)*ghost1Settings2[i].z+0.5,0,0)).rgb);
				}			
	
				//ghost 2
				for (int j=0; j<4; ++j)
    			{        					        					
        					ghosts+=ghost2Settings1[j].x * (tex2Dlod(sunGhost2,float4((IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost2Settings1[j].w))*
        					float2(aspectRatio*ghost2Settings1[j].y,1)*ghost2Settings1[j].z+0.5,0,0)).rgb);
        					
        					ghosts+=ghost2Settings2[j].x * (tex2Dlod(sunGhost2,float4((IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost2Settings2[j].w))*
        					float2(aspectRatio*ghost2Settings2[j].y,1)*ghost2Settings2[j].z+0.5,0,0)).rgb);
				}
				
				//ghost 3
				for (int k=0; k<4; ++k)
    			{        					        					
        					ghosts+=ghost3Settings1[k].x * (tex2Dlod(sunGhost3,float4((IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost3Settings1[k].w))*
        					float2(aspectRatio*ghost3Settings1[k].y,1)*ghost3Settings1[k].z+0.5,0,0)).rgb);
        					
        					ghosts+=ghost3Settings2[k].x * (tex2Dlod(sunGhost3,float4((IN.uv.xy-sunViewPortPos.xy+(toScreenCenter*ghost3Settings2[k].w))*
        					float2(aspectRatio*ghost3Settings2[k].y,1)*ghost3Settings2[k].z+0.5,0,0)).rgb);
				}

			   	ghosts=ghostFade * ghosts * smoothstep(0,1,1-length(toScreenCenter));
			   	
			   	sunColor+=ghosts;
			    
				float depth =  tex2D(_customDepthTexture,sunViewPortPos.xy);  //if there's something in the way don't render the flare	
				
				if (depth < 1.0)
					return float4(0.0,0.0,0.0,0.0);
					
//				float3 extinction = getExtinction(WCP,WSD);
//				if(useTransmittance > 0.0)
//					sunColor*=extinction;

				float3 extinction = tex2D(extinctionTexture,float2(0,0)); //precomputed extinction through multiple atmospheres and rings
				sunColor*=extinction;

				sunColor*=sunGlareFade;

				return float4(dither(sunColor, screenPos),1.0);
			}
			
			ENDCG
    	}
	}
}