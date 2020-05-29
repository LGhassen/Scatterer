// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Scatterer/GRASS"
{	
	SubShader
	{

		//Tags { "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque"}

		Tags { "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque"} //ignore projector for now

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			Ztest On
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha  //alpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma glsl
			#pragma target 4.0
			//#pragma multi_compile_fwdbase
			//#pragma multi_compile_fwdadd_fullshadows”

			//#include "UnityShaderVariables.cginc"
			#include "UnityCG.cginc"
			//#include "AutoLight.cginc"
			//#include "Lighting.cginc"
			//#include "../AtmosphereScatterer.cginc"

			//	  struct v2f
			//	  {
			//      	float4 pos : SV_POSITION;
			//   
			//        float3 planetUV: TEXCOORD0;
			//        float3 planetAbsNormal: TEXCOORD1;
			//        float3 steepAltDist: TEXCOORD2;// x = steep power, y = altitude (atmosphere relative), z = distance to camera, w = scattering dot product
			//        float3 vColor: TEXCOORD3;
			//        float3 viewDir: TEXCOORD4;
			//        
			//        //self-explanatory
			//        LIGHTING_COORDS(5, 6)
			//     
			////        //tangent space crap
			////        //this also contains the worldPos and worldNormal
			////        float4 tSpace0 : TEXCOORD7;
			////  		float4 tSpace1 : TEXCOORD8;
			////  		float4 tSpace2 : TEXCOORD9;		
			//      };


			struct v2g
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 vColor : TEXCOORD0;
				float3 viewDir: TEXCOORD1;
				float3 worldNormal  : TEXCOORD2;
				float3 planetUV: TEXCOORD3;
			};


			v2g vert(appdata_full v)
			{
				v2g o;

				//o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.pos = v.vertex;
				o.normal = v.normal;
				o.vColor = v.color.rgb;

				o.planetUV = normalize(float3(v.texcoord.x, v.texcoord.y, v.texcoord1.x)); 

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldNormal = normalize(mul( unity_ObjectToWorld, float4(v.normal, 0)).xyz);

				o.viewDir = normalize(worldPos - _WorldSpaceCameraPos);

				//TRANSFER_VERTEX_TO_FRAGMENT(o); //not sure about this one when passing to geometry shader

				return o;
			}


			//geometry shader


			struct g2f
			{
				float4 pos : SV_POSITION;
				//float3 vColor : TEXCOORD0;
				float3 worldNormal  : TEXCOORD0;
				//        float3 viewDir: TEXCOORD1;

				//self-explanatory
				//LIGHTING_COORDS(3, 4)
			};


			float rand(float seed) { return frac(sin(seed)*43758.5453123); }

			[maxvertexcount(15)]
			//void Geometry(triangle Attributes input[3], uint pid : SV_PrimitiveID, inout TriangleStream<g2f> outStream)
			void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
			{
				g2f tri;

				//		//original tri
				//		tri.pos = mul(UNITY_MATRIX_MVP,input[0].pos);
				//		tri.worldNormal=mul(unity_ObjectToWorld,input[0].normal);
				//		outStream.Append(tri);
				//
				//		tri.pos = mul(UNITY_MATRIX_MVP,input[1].pos);
				//		tri.worldNormal=mul(UNITY_MATRIX_MVP,input[1].normal);
				//		outStream.Append(tri);
				//
				//		tri.pos = mul(UNITY_MATRIX_MVP,input[2].pos);
				//		tri.worldNormal=mul(UNITY_MATRIX_MVP,input[2].normal);
				//		outStream.Append(tri);
				//
				//		outStream.RestartStrip();

				float3 trianglePos = (input[0].pos.xyz + input[1].pos.xyz + input[2].pos.xyz) / 3;
				float3 triangleNormal = (input[0].normal + input[1].normal + input[2].normal) /3;
				float3 triangleTangent = normalize(trianglePos-input[0].pos.xyz); //randomize?
				float3 outputWorldNormal = mul(unity_ObjectToWorld,normalize(cross(triangleTangent,triangleNormal)));

				float width  = 25.0*rand(input[0].planetUV.x*1000+input[1].planetUV.y*1000);
				float height = 2.0 * width;

				//1st tri
				tri.pos = UnityObjectToClipPos(float4(trianglePos-triangleTangent*width*0.5,1.0));
				tri.worldNormal=outputWorldNormal;
				outStream.Append(tri);

				tri.pos = UnityObjectToClipPos(float4(trianglePos+triangleTangent*width*0.5,1.0));
				tri.worldNormal=outputWorldNormal;
				outStream.Append(tri);

				float3 p0 = trianglePos+triangleNormal*height;

				tri.pos = UnityObjectToClipPos(float4(p0,1.0));
				tri.worldNormal=outputWorldNormal;
				outStream.Append(tri);

				height*=0.9;

				//2nd tri
				float3 p1 = trianglePos+triangleNormal*height + triangleTangent*width;
				tri.pos = UnityObjectToClipPos(float4(p1,1.0));
				tri.worldNormal=outputWorldNormal;
				outStream.Append(tri);

				//rest of tris procedural
				for (int i=0;i<6;i++)
				{
					triangleNormal = 0.8*triangleNormal + 0.2*triangleTangent;
					height*=0.8;
					width*=0.8;

					p0+= (triangleNormal*height+triangleTangent*width*0.6);
					tri.pos = UnityObjectToClipPos(float4(p0,1.0));
					tri.worldNormal=outputWorldNormal;
					outStream.Append(tri);

					p1+= (triangleNormal*height+triangleTangent*width*0.5);
					tri.pos = UnityObjectToClipPos(float4(p1,1.0));
					tri.worldNormal=outputWorldNormal;
					outStream.Append(tri);
				}

				outStream.RestartStrip();
			}


			//float4 frag (g2f i) : COLOR
			float4 frag (v2g i) : COLOR
			{
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz); //directional light		
				float dotLight = abs(dot(normalize(i.worldNormal),lightDir));	

				//float atten = LIGHT_ATTENUATION(IN);

				//float3 finalColor = i.vColor * (dotLight + UNITY_LIGHTMODEL_AMBIENT);
				float3 finalColor = float3(0.25,0.75,0.25);// * (dotLight + UNITY_LIGHTMODEL_AMBIENT);
				//float3 finalColor = i.planetUV;
				//float3 finalColor = float3(0.0,1.0,0.0);

				return float4(finalColor,1.0);
			}
			ENDCG
		}
	}
}
