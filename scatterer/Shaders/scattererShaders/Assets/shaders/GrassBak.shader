// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Scatterer/GRASS"
{	
  SubShader
  {

    Tags { "Queue" = "Transparent" "IgnoreProjector" = "False" "RenderType" = "Opaque"}

    Pass
    {
	  Tags { "LightMode" = "ForwardBase" }
		
	  Ztest On
      ZWrite On
      Blend SrcAlpha OneMinusSrcAlpha  //alpha

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
      };

				
	  v2g vert(appdata_full v)
	  {
		v2g o;

		//o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.pos = v.vertex;
		o.normal = v.normal;
		o.vColor = v.color.rgb;
		        
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
        float3 vColor : TEXCOORD0;
        float3 worldNormal  : TEXCOORD1;
        //        float3 viewDir: TEXCOORD1;
        
        //self-explanatory
        //LIGHTING_COORDS(3, 4)
      };


   	[maxvertexcount(15)]
	//void Geometry(triangle Attributes input[3], uint pid : SV_PrimitiveID, inout TriangleStream<g2f> outStream)
    void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream)
	{
        // Vertex inputs

//        //position in clip space
//        float3 vPosition0 = input[0].pos.xyz;
//        float3 vPosition1 = input[1].pos.xyz;
//        float3 vPosition2 = input[2].pos.xyz;
//
//        //normal in object space
//        float3 vWorldNormal0 = input[0].worldNormal;
//        float3 vWorldNormal1 = input[1].worldNormal;
//        float3 vWorldNormal2 = input[2].worldNormal;
//
//        //vertex colors
//       	float3 vColor0 = input[0].vColor;
//        float3 vColor1 = input[1].vColor;
//        float3 vColor2 = input[2].vColor;
//
//        float3 trianglePos = (vPosition0 + vPosition1 + vPosition2) / 3;
//        //float3 triangleWorldNormal = (vWorldNormal0 + vWorldNormal1 +vWorldNormal2) /3;
//        //float3 triangleNormal = (vWorldNormal0 + vWorldNormal1 +vWorldNormal2) /3;
//        float3 triangleColorMixed = (vColor0 + vColor1 +vColor2) /3;
//
//		float3 triangleTangent = (trianglePos-vPosition0);



//		tri.pos = float4(trianglePos + triangleTangent*3,1.0);
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//
//		tri.pos = float4(trianglePos - triangleTangent*3,1.0);
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//
//		float4x4 vp =mul(UNITY_MATRIX_MVP, unity_WorldToObject);
//
//		//tri.pos = float4(trianglePos + triangleWorldNormal*10,1.0);  //top vertex of the triangle
//		tri.pos = float4(trianglePos + mul(vp, triangleWorldNormal)*10,1.0);  //top vertex of the triangle
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//
//		//second tri for backface visibility, taking advantage of tri strip		
//		tri.pos = float4(trianglePos + triangleTangent*3,1.0);
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//		outStream.RestartStrip();

		

		//tri.pos = input[0].pos;
//		tri.pos = mul(UNITY_MATRIX_MVP,float4(input[0].pos.xyz+triangleNormal*2000,1.0));
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//
//		tri.pos = mul(UNITY_MATRIX_MVP,float4(input[1].pos.xyz+triangleNormal*2000,1.0));
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//
//		tri.pos = mul(UNITY_MATRIX_MVP,float4(input[2].pos.xyz+triangleNormal*2000,1.0));
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);



		g2f tri;

		float3 trianglePos = (input[0].pos.xyz + input[1].pos.xyz + input[2].pos.xyz) / 3;
		float3 triangleNormal = (input[0].normal + input[1].normal + input[2].normal) /3;
		float3 triangleTangent = normalize(trianglePos-input[0].pos.xyz);
		float3 triangleColorMixed = (input[0].vColor + input[1].vColor +input[2].vColor) /3;
		float3 outputWorldNormal = mul(unity_ObjectToWorld,normalize(cross(triangleNormal,triangleTangent)));

		//original tri
		tri.pos = UnityObjectToClipPos(input[0].pos);
		tri.vColor = triangleColorMixed;
		tri.worldNormal=mul(unity_ObjectToWorld,input[0].normal);
		outStream.Append(tri);

		tri.pos = UnityObjectToClipPos(input[1].pos);
		tri.vColor = triangleColorMixed;
		tri.worldNormal=UnityObjectToClipPos(input[1].normal);
		outStream.Append(tri);

		tri.pos = UnityObjectToClipPos(input[2].pos);
		tri.vColor = triangleColorMixed;
		tri.worldNormal=UnityObjectToClipPos(input[2].normal);
		outStream.Append(tri);

		outStream.RestartStrip();

		//first tri
		tri.pos = UnityObjectToClipPos(float4(trianglePos+triangleTangent*15,1.0));
		tri.vColor = triangleColorMixed;
		tri.worldNormal=outputWorldNormal;
		outStream.Append(tri);

		tri.pos = UnityObjectToClipPos(float4(trianglePos-triangleTangent*15,1.0));
		tri.vColor = triangleColorMixed;
		tri.worldNormal=outputWorldNormal;
		outStream.Append(tri);

		tri.pos = UnityObjectToClipPos(float4(trianglePos+triangleNormal*45,1.0));
		tri.vColor = triangleColorMixed;
		tri.worldNormal=outputWorldNormal;
		outStream.Append(tri);

		outStream.RestartStrip();

//		//2nd tri
//		tri.pos = mul(UNITY_MATRIX_MVP,float4(trianglePos+triangleNormal*45,1.0));
//		tri.vColor = triangleColorMixed;
//		tri.worldNormal=-outputWorldNormal;
//		outStream.Append(tri);
//
//		tri.pos = mul(UNITY_MATRIX_MVP,float4(trianglePos-triangleTangent*15,1.0));
//		tri.vColor = triangleColorMixed;
//		tri.worldNormal=-outputWorldNormal;
//		outStream.Append(tri);
//
//		tri.pos = mul(UNITY_MATRIX_MVP,float4(trianglePos+triangleTangent*15,1.0));
//		tri.vColor = triangleColorMixed;
//		tri.worldNormal=-outputWorldNormal;
//		outStream.Append(tri);
//
//		outStream.RestartStrip();

		//first tri
		tri.pos = UnityObjectToClipPos(float4(trianglePos+triangleTangent*15,1.0));
		tri.vColor = triangleColorMixed;
		tri.worldNormal=-outputWorldNormal;
		outStream.Append(tri);

		tri.pos = UnityObjectToClipPos(float4(trianglePos+triangleNormal*45,1.0));
		tri.vColor = triangleColorMixed;
		tri.worldNormal=-outputWorldNormal;
		outStream.Append(tri);

		tri.pos = UnityObjectToClipPos(float4(trianglePos-triangleTangent*15,1.0));
		tri.vColor = triangleColorMixed;
		tri.worldNormal=-outputWorldNormal;
		outStream.Append(tri);

		outStream.RestartStrip();


//		//tri.pos = input[1].pos;
//		tri.pos = mul(UNITY_MATRIX_MVP,input[1].pos);
//
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);
//
//		//tri.pos = input[2].pos;
//		tri.pos = mul(UNITY_MATRIX_MVP,input[2].pos);
//		tri.vColor = triangleColorMixed;
//		outStream.Append(tri);

		//outStream.RestartStrip();
	}

			
	  float4 frag (g2f i) : COLOR
	  //float4 frag (v2g i) : COLOR
      {
			float3 lightDir = normalize(_WorldSpaceLightPos0.xyz); //directional light		
			float dotLight = dot(normalize(i.worldNormal),lightDir);	

			//float atten = LIGHT_ATTENUATION(IN);
		
			//float3 finalColor = i.vColor * (dotLight + UNITY_LIGHTMODEL_AMBIENT);
			float3 finalColor = float3(0.25,0.75,0.25) * (dotLight + UNITY_LIGHTMODEL_AMBIENT);
			//float3 finalColor = float3(0.0,1.0,0.0);
   
			return float4(finalColor,1.0);
      }
     ENDCG
  	 }
	}
}
