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
			#include "../DepthCommon.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl

			#pragma multi_compile SCATTERER_MERGED_DEPTH_ON SCATTERER_MERGED_DEPTH_OFF
			
			uniform float sunGlareScale;
			uniform float sunGlareFade;
			uniform float ghostFade;
			
			uniform sampler2D sunSpikes;
			uniform sampler2D sunFlare;
			uniform sampler2D sunGhost1;
			uniform sampler2D sunGhost2;
			uniform sampler2D sunGhost3;

			uniform float3 flareColor;

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
			
			uniform float3 sunViewPortPos;
			uniform float aspectRatio;

			uniform float renderSunFlare;
			uniform float renderOnCurrentCamera;
			uniform float useDbufferOnCamera;		
			
			struct v2f 
			{
    			float4 pos: SV_POSITION;
    			float2 uv : TEXCOORD0;
    			float3 extinction : TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				v.vertex.y = v.vertex.y *_ProjectionParams.x;

				//if there's something in the way don't render the flare
				float drawFlare = (useDbufferOnCamera < 1.0) ? 1.0 : checkDepthBufferEmpty(sunViewPortPos.xy);

    			OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
    			OUT.pos = (renderSunFlare == 1.0) && (_ProjectionParams.y < 200.0) && (renderOnCurrentCamera == 1.0) && (drawFlare ==1.0) ? OUT.pos : float4(2.0,2.0,2.0,1.0); //if we don't need to render the sunflare, cull vertexes by placing them outside clip space
    																												  //also use near plane to not render on far camera
    			OUT.uv = v.texcoord.xy;

    			OUT.extinction = tex2Dlod(extinctionTexture,float4(0.0,0.0,0.0,0.0)).rgb; //precomputed extinction through multiple atmospheres and rings

//#if UNITY_UV_STARTS_AT_TOP
//				OUT.uv.y = 1.0 - OUT.uv.y;
//#endif
    			return OUT;
			}


			float4 frag(v2f IN) : SV_Target
			{						   
				float3 sunColor=0;
				
				//TODO: move aspectRatio precomputations to CPU?

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
			    
				sunColor*= flareColor * IN.extinction * sunGlareFade;

				return float4(sunColor,1.0);
			}
			
			ENDCG
    	}
	}
}