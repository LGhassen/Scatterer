// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Scatterer/sunFlareExtinction" 
{
	SubShader 
	{
    	Pass //pass 0 - atmospheric extinction
    	{
			ZWrite Off
    		ZTest Off
    		cull off

    		Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float Rg;
			uniform float Rt;
			uniform sampler2D _Sky_Transmittance;

			uniform float3 _Sun_WorldSunDir;
			uniform float3 _Globals_WorldCameraPos;

			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = UnityObjectToClipPos(v.vertex);
    			OUT.uv = v.texcoord;
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

    			extinction = (r > Rt) ? float3(1,1,1) : Transmittance(r, mu);

    			return extinction;
    		}

			float4 frag(v2f IN): COLOR
			{
				float3 WSD = _Sun_WorldSunDir;
			    float3 WCP = _Globals_WorldCameraPos;

				float3 extinction = getExtinction(WCP,WSD);

				return float4(extinction,1.0);
			}
			
			ENDCG
    	}





    	Pass //pass 1 - ring extinction
    	{
			ZWrite Off
    		ZTest Off
    		cull off

    		Blend DstColor Zero  //multiplicative blending

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform sampler2D ringTexture;
			uniform float ringInnerRadius;
			uniform float ringOuterRadius;
			uniform float3 ringNormal;

			uniform float3 _Sun_WorldSunDir;
			uniform float3 _Globals_WorldCameraPos;

			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = UnityObjectToClipPos(v.vertex);
    			OUT.uv = v.texcoord;
    			return OUT;
			}

			float3 LinePlaneIntersection(float3 linePoint, float3 lineVec, float3 planeNormal, float3 planePoint)
			{
				float tlength;
				float dotNumerator;
				float dotDenominator;
		
				float3 intersectVector;
				float3 intersection = 0;
 
				//calculate the distance between the linePoint and the line-plane intersection point
				dotNumerator = dot((planePoint - linePoint), planeNormal);
				dotDenominator = dot(lineVec, planeNormal);
 
				//line and plane are not parallel
				//if(dotDenominator != 0.0f)   //don't care, it's faster
				{
					tlength =  dotNumerator / dotDenominator;
  					intersection= (tlength > 0.0) ? linePoint + normalize(lineVec) * (tlength) : linePoint;

					return intersection;	
				}
			}


			float4 frag(v2f IN): COLOR
			{
			    float3 WCP = _Globals_WorldCameraPos;

				//raycast from atmo to ring plane and find intersection
				//float3 ringIntersectPt = LinePlaneIntersection(WCP, _Sun_WorldSunDir, ringNormal, _Scatterer_Origin);
				float3 ringIntersectPt = LinePlaneIntersection(WCP, _Sun_WorldSunDir, ringNormal, float3(0,0,0));

				//calculate ring texture position on intersect
//				float distance = length (ringIntersectPt - _Scatterer_Origin);
				float distance = length (ringIntersectPt);
				float ringTexturePosition = (distance - ringInnerRadius) / (ringOuterRadius - ringInnerRadius); //inner and outer radiuses need are converted to local space coords on plugin side
				ringTexturePosition = 1 - ringTexturePosition; //flip to match UVs

				//read 1-alpha of ring texture
				float4 ringColor = tex2Dlod(ringTexture, float4 (ringTexturePosition,ringTexturePosition,0,0));
				float ringShadow = 1-ringColor.a;

				//don't apply any shadows if intersect point is not between inner and outer radius
				ringColor.xyz*=ringShadow;
				ringColor.xyz = (ringTexturePosition > 1 || ringTexturePosition < 0 ) ? float3(1,1,1) : ringColor.xyz;

				return float4(ringColor.xyz,1.0);
			}
			
			ENDCG
    	}
	}
}
