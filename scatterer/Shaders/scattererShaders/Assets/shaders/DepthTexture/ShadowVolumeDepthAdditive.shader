// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/VolumeDepthAdditive" {

	SubShader {
		Tags { "RenderType"="Opaque" }

		Pass {

			ZTest Off
			ZWrite Off
			Cull Off
			Blend One One

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl

			#include "UnityCG.cginc"


			float3 _Godray_WorldSunDir;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};


			struct v2f {
				float4 pos : SV_POSITION;
				float3 rayLength : TEXCOORD0;
				float2 depth : TEXCOORD1;
			};

			v2f vert (appdata v)
			{
				v2f o;

				float4 _LightDirWorldSpace = float4(_Godray_WorldSunDir,0.0);
				float3 toLight=normalize(_LightDirWorldSpace.xyz);

				//    float backFacingFromLight = dot( toLight, mul(UNITY_MATRIX_MVP,float4(v.normal,0.0)) );
				float backFacingFromLight = dot( toLight, mul(unity_ObjectToWorld, float4(v.normal,0.0)) );

				float extrude = (backFacingFromLight < 0.0) ? 1.0 : 0.0;

				//    float towardsSunFactor=dot(toLight,float3(0,0,1));
				//   	float projectOnNearPlane = (towardsSunFactor < 0.0) ? 1.0 : 0.0;

				//	v.vertex.xyz=  (projectOnNearPlane * extrude > 0.0) ? 
				//			LinePlaneIntersection(v.vertex.xyz, -toLight,float3(0,0,1), 0) :

				v.vertex = mul(unity_ObjectToWorld, v.vertex);  //both in worldSpace
				v.vertex.xyz -= toLight * extrude  *  1000000;


				o.rayLength= v.vertex.xyz-_WorldSpaceCameraPos;

				o.pos = mul (UNITY_MATRIX_VP, v.vertex);
				o.depth=o.pos.zw;

				return o;

			}

			float4 frag(v2f i, float facing : VFACE) : COLOR
			{
				float len=length(i.rayLength)*facing;	
				return len;
			}
			//float4 frag(v2f i) : COLOR
			//{    
			//	float depth = tex2D(_customDepthTexture, i.uv_depth).r;
			//
			//	float4 H = float4(i.uv_depth.x * 2.0f - 1.0f, (i.uv_depth.y) * 2.0f - 1.0f, depth, 1.0f);
			//    float4 D = mul(_ViewProjInv, H);
			//    float3 groundWorldPos = D / D.w;  //reconstruct world position from depth texture
			//									  //this is to clip geometry that is behind closest terrain to terrain level
			//	float groundRayLength = length(worldPos-_camPos);							  
			//	float rayLength = length(i.rayLength);
			//	
			//	if ( groundRayLength < rayLength )
			//		raylength = groundRayLength;
			//
			//    return i.depth.x/i.depth.y;
			//}

			ENDCG
		}
	}
}