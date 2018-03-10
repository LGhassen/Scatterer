// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Scatterer/DepthTexture" {
SubShader {
    Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
    Pass {
    	Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
        Fog { Mode Off }
CGPROGRAM
 
#pragma vertex vert
#pragma fragment frag
#pragma geometry geom
#pragma glsl
#pragma target 4.0

//#define LOGARITHMIC_DEPTH_ON
#define VIEW_SPACE_DISTANCE_ON


#include "UnityCG.cginc"
   
struct v2g {
    float4 pos : SV_POSITION;
    float2 depth : TEXCOORD2;

#if defined (LOGARITHMIC_DEPTH_ON)
    float4 vertexPosClip : TEXCOORD0;
#elif defined (VIEW_SPACE_DISTANCE_ON)
	float3 vertexPosView : TEXCOORD0;
#endif

	float4 vertexPosClip : TEXCOORD1;

};
 
v2g vert (appdata_base v) {
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    o.depth=o.pos.zw;
    o.vertexPosClip = o.pos;

#if defined (LOGARITHMIC_DEPTH_ON)
	o.vertexPosClip = o.pos;
#elif defined (VIEW_SPACE_DISTANCE_ON)
	o.vertexPosView = mul (UNITY_MATRIX_MV, v.vertex);
#endif

    return o;
}

      struct g2f
	  {
      	float4 pos : SV_POSITION;
        //float3 vColor : TEXCOORD0;
        float3 worldNormal  : TEXCOORD0;
        //        float3 viewDir: TEXCOORD1;
        
        //self-explanatory
        //LIGHTING_COORDS(3, 4)
      };

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


struct fout {
	float4 color : COLOR;
	float depth : DEPTH;
};

fout frag(v2f i)
{

	fout OUT;

#if defined (LOGARITHMIC_DEPTH_ON)

	float C=1.0;
	float _offset=2.0;
	return (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));

#elif defined (VIEW_SPACE_DISTANCE_ON)
	OUT.color = abs(i.vertexPosView.z) / 750000.0;
	float C=1.0;
	float _offset=2.0;
	OUT.depth = (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));
	return OUT;

#else

	return (i.depth.x/i.depth.y);

#endif

}
ENDCG
    }
}




// CUT-OUT SUBSHADER
SubShader {

    Tags { "RenderType"="TransparentCutout" "IgnoreProjector" = "True"}
    Pass {
    	Tags { "RenderType"="TransparentCutout" "IgnoreProjector" = "True"}
        Fog { Mode Off }

CGPROGRAM

#pragma vertex vert
#pragma fragment frag

//#define LOGARITHMIC_DEPTH_ON
#define VIEW_SPACE_DISTANCE_ON


#include "UnityCG.cginc"

sampler2D _MainTex;
//float _Cutoff;

struct v2f {
    float4 pos : SV_POSITION;
    float2 depth : TEXCOORD2;

#if defined (LOGARITHMIC_DEPTH_ON)
    float4 vertexPosClip : TEXCOORD0;
#elif defined (VIEW_SPACE_DISTANCE_ON)
	float3 vertexPosView : TEXCOORD0;
#endif

	float4 vertexPosClip : TEXCOORD1;
	float2 uv_MainTex    : TEXCOORD3;

};
 
v2f vert (appdata_base v) {
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    o.depth=o.pos.zw;
    o.vertexPosClip = o.pos;

#if defined (LOGARITHMIC_DEPTH_ON)
	o.vertexPosClip = o.pos;
#elif defined (VIEW_SPACE_DISTANCE_ON)
	o.vertexPosView = mul (UNITY_MATRIX_MV, v.vertex);
#endif

	o.uv_MainTex = v.texcoord;

    return o;
}

struct fout {
	float4 color : COLOR;
	float depth : DEPTH;
};

fout frag(v2f i)
{

	fout OUT;

#if defined (LOGARITHMIC_DEPTH_ON)

	float C=1.0;
	float _offset=2.0;
	return (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));

#elif defined (VIEW_SPACE_DISTANCE_ON)
	OUT.color   = abs(i.vertexPosView.z) / 750000.0;

	if (tex2D(_MainTex, i.uv_MainTex).a <0.7) //works better
	//if (tex2D(_MainTex, i.uv_MainTex).a <=_Cutoff)
		discard;

	float C=1.0;
	float _offset=2.0;
	OUT.depth = (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));
	return OUT;

#else

	return (i.depth.x/i.depth.y);

#endif

}
ENDCG
    }
}






}