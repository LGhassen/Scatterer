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

//			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma hull hull
			#pragma domain domain

			#include "UnityCG.cginc"
			#include "UnityShadowLibrary.cginc"
			#include "ShadowVolumeUtils.cginc"

			#pragma multi_compile DUAL_DEPTH_OFF DUAL_DEPTH_ON

			sampler2D _ShadowMapTextureCopyScatterer;
			sampler2D _CameraDepthTexture;
			float4x4 CameraToWorld;

			float3 lightDirection;
			float3 cameraForwardDir;

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
				float3 outputColor: TEXCOORD0;
				float4 viewPos: TEXCOORD1;
				float4 projPos : TEXCOORD2;
			};

			v2t vert (appdata v)
			{
				v2t o;

				//o.pos = float4(2.0*(v.uv-0.5),0.0,1.0); //try modifying this, clip space goes from -1 to 1, our UVs go from?to?
				o.pos = float4(3.0*(v.uv-0.5),0.0,1.0); //HOLY FUCK THIS FIXES THE DISAPPEARING CHUNKS WHAT THE FUCK

				o.worldOriginPos = mul(lightToWorld,o.pos);
				//o.worldEndPos = mul(lightToWorld,float4(o.pos.x,o.pos.y,1.0,1.0));
				o.worldEndPos = mul(lightToWorld,float4(o.pos.x,o.pos.y,10.0,1.0));

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

//				bool isVisible = false;
//
//				for (int i=0;i<3;i++)
//				{
//					float4 clipSpaceOrigin = mul(UNITY_MATRIX_VP, patch[i].worldOriginPos);
//					float4 clipSpaceEnd    = mul(UNITY_MATRIX_VP, patch[i].worldEndPos);
//					isVisible = isVisible || intersectsFrustum(clipSpaceOrigin.xyz/clipSpaceOrigin.w, clipSpaceEnd.xyz/clipSpaceEnd.w); //consider this to be working though I'm not sure, maybe pass a color?
//				}
//
//				if (!isVisible)
//				{
//					o.edge[0] = 0.0;
//					o.edge[1] = 0.0;
//					o.edge[2] = 0.0;
//					o.inside = 0.0;
//				}
//				else
				{
					float maxTesselationFactor = 64.0;

					float distEdge0 = rayDistanceToPoint(0.5*(patch[1].worldOriginPos+patch[2].worldOriginPos), lightDirection, _WorldSpaceCameraPos);
					float distEdge1 = rayDistanceToPoint(0.5*(patch[2].worldOriginPos+patch[0].worldOriginPos), lightDirection, _WorldSpaceCameraPos);
					float distEdge2 = rayDistanceToPoint(0.5*(patch[0].worldOriginPos+patch[1].worldOriginPos), lightDirection, _WorldSpaceCameraPos);

					float factor0 = clamp(maxTesselationFactor * 225.0 / distEdge0,1.0,maxTesselationFactor);
					float factor1 = clamp(maxTesselationFactor * 225.0 / distEdge1,1.0,maxTesselationFactor);
					float factor2 = clamp(maxTesselationFactor * 225.0 / distEdge2,1.0,maxTesselationFactor);

					//float insideFactor = max(factor0, max(factor1,factor2)); //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max

					o.edge[0] = factor0;
					o.edge[1] = factor1;
					o.edge[2] = factor2;
					o.inside  = (factor0 + factor1 + factor2) / 3.0;  //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max

//					o.edge[0] = 16;
//					o.edge[1] = 16;
//					o.edge[2] = 16;
//					o.inside  = 16;  //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max

//					o.edge[0] = 1;
//					o.edge[1] = 1;
//					o.edge[2] = 1;
//					o.inside  = 1;  //doesn't seem to be always correct, so try to think about it, think about what causes cracks in this case, it's normal, every triangle's max isn't necessarily the adjacent triangle's max

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

			float3 LinePlaneIntersection(float3 linePoint, float3 lineVec, float3 planeNormal, float3 planePoint, out float parallel)
			{
				float tlength;
				float dotNumerator;
				float dotDenominator;

				float3 intersectVector;
				float3 intersection = 0.0;

				//calculate the distance between the linePoint and the line-plane intersection point
				dotNumerator = dot((planePoint - linePoint), planeNormal);
				dotDenominator = dot(lineVec, planeNormal);

				//line and plane are not parallel
				if(dotDenominator != 0.0f)
				{
					tlength =  dotNumerator / dotDenominator;
					intersection= (tlength > 0.0) ? linePoint + normalize(lineVec) * (tlength) : linePoint;
					parallel = 0.0;
				}
				else
				{
					parallel = 1.0;
				}

				return intersection;
			}


			[domain("tri")]
			d2f domain(OutputPatchConstant tessFactors,const OutputPatch<v2t,3> vs, float3 d:SV_DomainLocation)
			{
				d2f o;
				float3 worldPos = vs[0].worldOriginPos.xyz/vs[0].worldOriginPos.w * d.x + vs[1].worldOriginPos.xyz/vs[1].worldOriginPos.w * d.y + vs[2].worldOriginPos.xyz/vs[2].worldOriginPos.w * d.z;

				float4 finalWorldPos = 0;
				float4 shadowPos = 0;
				fixed cascadeIndex = pickMostDetailedCascade (float4(worldPos,1.0), shadowPos, _ShadowMapTextureCopyScatterer);		//only support 4 cascades here

				float4x4 shadowToWorld = inverseShadowMatricesBuffer[max(cascadeIndex,0)];

				finalWorldPos = mul( shadowToWorld, shadowPos);
				if (cascadeIndex < 0.0)
				{
					float3 worldEndPos = vs[0].worldEndPos.xyz/vs[0].worldEndPos.w * d.x + vs[1].worldEndPos.xyz/vs[1].worldEndPos.w * d.y + vs[2].worldEndPos.xyz/vs[2].worldEndPos.w * d.z;
					finalWorldPos = float4(worldEndPos,1.0);
				}

				//the next step is to project this finalWorldPos on the near Plane

				//works well, fixes the case where we are inside the shadow and looking away from the light
				//one problem remains, when looking in the other direction(towards the light), there is nothing left behind us to project to near clip plane
				//means we are inside the shadow, looking towards terrain, and there are no intervals crossed, there is zero written to this buffer, and things behind terrain are discarded
				//my solution is execute a compute shader which would check if our camera is in shadow beforeHand -> if camera is in shadow and no transitions -> darken the whole interval, this has to be done in the scattering shader though
				//use the same code with the mostDetailed cascade thing?

				//If I get that working I may not even need to do this nearPlane projection in fact, the solution would be so much more elegant
				if ( dot(cameraForwardDir, normalize(finalWorldPos.xyz/finalWorldPos.w - _WorldSpaceCameraPos.xyz)) < 0.0 )
				{
					float parallel = 0.0;
					float3 nearPlanePoint = LinePlaneIntersection(finalWorldPos.xyz/finalWorldPos.w, lightDirection, cameraForwardDir, _WorldSpaceCameraPos.xyz + 1.0 * cameraForwardDir, parallel);
					finalWorldPos = (parallel == 0.0 ) ? float4(nearPlanePoint,1.0) : finalWorldPos;
				}

				//color debugging stuff
				o.outputColor = float3(cascadeIndex == 0, cascadeIndex == 1, cascadeIndex ==2);
				o.outputColor =  (cascadeIndex == -1) ? 0 :  ((cascadeIndex == 3) ? 1.0 : o.outputColor );

				o.pos=UnityWorldToClipPos(finalWorldPos);
				//o.pos=UnityWorldToClipPos(worldPos);
				o.viewPos = float4(UnityWorldToViewPos(finalWorldPos),1.0);
				o.projPos = ComputeScreenPos(o.pos);
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
//#if defined(DUAL_DEPTH_ON)
//				zdepth = SAMPLE_DEPTH_TEXTURE(AdditionalDepthBuffer, depthUV);
//
//#ifdef SHADER_API_D3D11  //#if defined(UNITY_REVERSED_Z)
//				zdepth = 1 - zdepth;
//#endif
//				depthClipPos = float4(depthUV, zdepth, 1.0);
//				depthClipPos.xyz = 2.0f * depthClipPos.xyz - 1.0f;
//				float4 depthViewPos2 = mul(ScattererAdditionalInvProjection, depthClipPos);
//
//				depthLength = (depthLength < 8000) || (depthLength > 50000) ? depthLength : length(depthViewPos2.xyz/depthViewPos2.w); //the 50000 is hardcoded shadow distance, hopefully we don't even need this crap
//#endif

				float viewLength = length(i.viewPos.xyz/i.viewPos.w);

				if (viewLength>depthLength) //discard what is behind terrain
					viewLength = 0.0;

//				if (viewLength>depthLength) //discard what is behind terrain
//					discard;

				return facing > 0 ? viewLength : -viewLength;

//				return facing > 0 ? float4(0.0,1.0,0.0,1.0) :  float4(1.0,0.0,0.0,1.0);
//				return float4(i.outputColor,1.0);

			}
			ENDCG
		}
	}
}