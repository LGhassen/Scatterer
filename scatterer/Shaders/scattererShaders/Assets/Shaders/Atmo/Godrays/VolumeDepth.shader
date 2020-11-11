Shader "Scatterer/VolumeDepth"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100


		Pass
		{

			Cull Off

			ZWrite Off
			ZTest Always
			Blend One One

//			Cull Off
//			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain

			#include "UnityCG.cginc"
			#include "UnityShadowLibrary.cginc"
			#include "ShadowVolumeUtils.cginc"
			#include "../../IntersectCommon.cginc"

			#pragma multi_compile DUAL_DEPTH_OFF DUAL_DEPTH_ON
			#pragma multi_compile OCEAN_INTERSECT_OFF OCEAN_INTERSECT_ON  //enable ocean_intersect only for custom ocean shader
			#pragma multi_compile CLOUDSMAP_OFF CLOUDSMAP_ON

			sampler2D _ShadowMapTextureCopyScatterer;


			sampler2D _CameraDepthTexture;
			float4x4 CameraToWorld;

			float3 lightDirection;

			float3 _planetPos;
			float Rg;
			float Rt;
			float _experimentalAtmoScale;

			sampler2D cloudShadowMap;

#if defined(DUAL_DEPTH_ON)
			UNITY_DECLARE_DEPTH_TEXTURE(AdditionalDepthBuffer);
			float4x4  ScattererAdditionalInvProjection;
#endif

			StructuredBuffer<float4x4> inverseShadowMatricesBuffer;

			float4x4 lightToWorld;


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2t //vertex 2 tesselation
			{
				float4 pos : INTERNALTESSPOS;
				float4 worldOriginPos : TEXCOORD0;
				float4 worldEndPos : TEXCOORD1;
			};

			struct d2f //domain 2 frag
			{
				float4 pos : SV_POSITION;
				float4 viewPos: TEXCOORD0;
				float4 projPos : TEXCOORD1;
				float4 finalWorldPos : TEXCOORD2;
				float isCloud : TEXCOORD3;
			};

			v2t vert (appdata v)
			{
				v2t o;

				//o.pos = float4(2.0*(v.uv-0.5),0.0,1.0); //try modifying this, clip space goes from -1 to 1, our UVs go from?to?
				o.pos = float4(3.0*(v.uv-0.5),0.0,1.0); //HOLY FUCK THIS FIXES THE DISAPPEARING CHUNKS WHAT THE FUCK

				o.worldOriginPos = mul(lightToWorld,o.pos);
				o.worldEndPos = mul(lightToWorld,float4(o.pos.x,o.pos.y,1.0,1.0));

				return o;
			}

			struct OutputPatchConstant
			{
				float edge[3]: SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			OutputPatchConstant constantsFixed(InputPatch<v2t,3> patch)
			{
				OutputPatchConstant o;

				bool isVisible = false;

				for (int i=0;i<3;i++)
				{
					float4 clipSpaceOrigin = mul(UNITY_MATRIX_VP, patch[i].worldOriginPos);
					float4 clipSpaceEnd    = mul(UNITY_MATRIX_VP, patch[i].worldEndPos);
					isVisible = isVisible || intersectsFrustum(clipSpaceOrigin.xyz/clipSpaceOrigin.w, clipSpaceEnd.xyz/clipSpaceEnd.w); //consider this to be working though I'm not sure, maybe pass a color?
				}

				if (!isVisible)
				{
					o.edge[0] = 0.0;
					o.edge[1] = 0.0;
					o.edge[2] = 0.0;
					o.inside = 0.0;
				}
				else
				{
					float maxTesselationFactor = 64.0;

					float distEdge0 = rayDistanceToPoint(0.5*(patch[1].worldOriginPos+patch[2].worldOriginPos), lightDirection, _WorldSpaceCameraPos);
					float distEdge1 = rayDistanceToPoint(0.5*(patch[2].worldOriginPos+patch[0].worldOriginPos), lightDirection, _WorldSpaceCameraPos);
					float distEdge2 = rayDistanceToPoint(0.5*(patch[0].worldOriginPos+patch[1].worldOriginPos), lightDirection, _WorldSpaceCameraPos);

					float factor0 = clamp(maxTesselationFactor * 225.0 / distEdge0,1.0,maxTesselationFactor);
					float factor1 = clamp(maxTesselationFactor * 225.0 / distEdge1,1.0,maxTesselationFactor);
					float factor2 = clamp(maxTesselationFactor * 225.0 / distEdge2,1.0,maxTesselationFactor);

//					float factor0 = clamp(maxTesselationFactor * 225.0 / distEdge0,4.0,maxTesselationFactor); // min of 4.0 is slooooow
//					float factor1 = clamp(maxTesselationFactor * 225.0 / distEdge1,4.0,maxTesselationFactor);
//					float factor2 = clamp(maxTesselationFactor * 225.0 / distEdge2,4.0,maxTesselationFactor);

					//float insideFactor = max(factor0, max(factor1,factor2)); //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max

					o.edge[0] = factor0;
					o.edge[1] = factor1;
					o.edge[2] = factor2;
					o.inside  = (factor0 + factor1 + factor2) / 3.0;  //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max
				}

				return o;
			}

			[domain("tri")]
			[partitioning("integer")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("constantsFixed")]
			[outputcontrolpoints(3)]
			v2t hull(InputPatch<v2t,3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}
				
			[domain("tri")]
			d2f domain(OutputPatchConstant tessFactors,const OutputPatch<v2t,3> vs, float3 d:SV_DomainLocation)
			{
				d2f o;
				float3 worldPos = vs[0].worldOriginPos.xyz/vs[0].worldOriginPos.w * d.x + vs[1].worldOriginPos.xyz/vs[1].worldOriginPos.w * d.y + vs[2].worldOriginPos.xyz/vs[2].worldOriginPos.w * d.z;

				float4 finalWorldPos = 0;
				float4 shadowPos = 0;

				float isCloud = 0;
				//only support 4 cascades here
#if defined(CLOUDSMAP_ON)
				fixed cascadeIndex = pickMostDetailedCascadeCloud (float4(worldPos,1.0), shadowPos, _ShadowMapTextureCopyScatterer, cloudShadowMap, isCloud);
#else
				fixed cascadeIndex = pickMostDetailedCascade (float4(worldPos,1.0), shadowPos, _ShadowMapTextureCopyScatterer);		
#endif

				float4x4 shadowToWorld = inverseShadowMatricesBuffer[max(cascadeIndex,0)];

				finalWorldPos = mul( shadowToWorld, shadowPos);

				if (cascadeIndex < 0.0)
				{
					//finalWorldPos = float4(_WorldSpaceCameraPos.xyz + lightDirection * 700000,1.0); //this works well for stock but breaks with parallax, what gives?
					finalWorldPos = float4(_WorldSpaceCameraPos.xyz + lightDirection * 500000,1.0);
				}

				o.pos=UnityWorldToClipPos(finalWorldPos);
				o.viewPos = float4(UnityWorldToViewPos(finalWorldPos),1.0);
				o.projPos = ComputeScreenPos(o.pos);
				o.finalWorldPos = finalWorldPos;
				o.isCloud = isCloud;

				return o;
			}


			float4 frag (d2f i, fixed facing : VFACE) : SV_Target
			{
				float2 depthUV = i.projPos.xy/i.projPos.w;
				float zdepth = tex2Dlod(_CameraDepthTexture, float4(depthUV,0,0));

#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
				zdepth = 1 - zdepth;
#endif

				float4 depthClipPos = float4(depthUV, zdepth, 1.0);
				depthClipPos.xyz = 2.0f * depthClipPos.xyz - 1.0f;
				float4 depthViewPos = mul(unity_CameraInvProjection, depthClipPos);

				float depthLength = length(depthViewPos.xyz/depthViewPos.w);
#if defined(DUAL_DEPTH_ON)
				float zdepth2 = SAMPLE_DEPTH_TEXTURE(AdditionalDepthBuffer, depthUV);

	#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
				zdepth2 = 1 - zdepth2;
	#endif
				depthClipPos = float4(depthUV, zdepth2, 1.0);
				depthClipPos.xyz = 2.0f * depthClipPos.xyz - 1.0f;
				float4 depthViewPos2 = mul(ScattererAdditionalInvProjection, depthClipPos);

				depthLength = (depthLength < 8000) || (zdepth2 == 0.0) ? depthLength : length(depthViewPos2.xyz/depthViewPos2.w);
#endif

				float3 viewDir = normalize(i.finalWorldPos.xyz/i.finalWorldPos.w - _WorldSpaceCameraPos);
				float viewLength = length(i.viewPos.xyz/i.viewPos.w);

				if (zdepth != 1.0) //cap by terrain distance
				{
					viewLength = min(depthLength, viewLength);
				}
				else //cap by boundary to atmo
				{
					//I think here I should take into account optical depth, or use log(depth), seems too dark
					float skyIntersectDistance = intersectSphereInside(_WorldSpaceCameraPos, viewDir, _planetPos, Rg + _experimentalAtmoScale * (Rt-Rg));
					viewLength = min(skyIntersectDistance, viewLength);

//					if (i.isCloud > 0.0)
//					{
//						viewLength*= 0.3; //this looks nice against the sky but where terrain and shadow meet it looks like shite
//					}
				}

#if defined(OCEAN_INTERSECT_ON)  //cap by boundary to ocean
				float oceanIntersectDistance = intersectSphereOutside(_WorldSpaceCameraPos, viewDir, _planetPos, Rg);
				viewLength = (oceanIntersectDistance > 0.0) ? min(oceanIntersectDistance, viewLength) : viewLength;
#endif

				return facing > 0 ? viewLength : -viewLength;
			}
			ENDCG
		}
	}
}