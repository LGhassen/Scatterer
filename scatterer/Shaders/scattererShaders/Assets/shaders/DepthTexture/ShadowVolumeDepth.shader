// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Scatterer/GodrayDepthTexture" {

SubShader {
    Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
      
    Pass {
        Fog { Mode Off }
//        Cull front
//        ztest off
			Cull Off
        
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
    float3 vertexPosView : TEXCOORD0;
    float4 vertexPosClip : TEXCOORD1;
};

float3 LinePlaneIntersection(float3 linePoint, float3 lineVec, float3 planeNormal, float3 planePoint)
//linePoint=original vertex, lineVec=extrusion direction, plane normal 0,0,1, plane point 0,0,0
{
 
 			
		float length;
		float dotNumerator;
		float dotDenominator;
		
		float3 intersectVector;
		float3 intersection = 0;
 
		//calculate the distance between the linePoint and the line-plane intersection point
		dotNumerator = dot((planePoint - linePoint), planeNormal);
		dotDenominator = dot(lineVec, planeNormal);
 
		//line and plane are not parallel
//		if(dotDenominator != 0.0f)
		{
			length =  dotNumerator / dotDenominator;
  			intersection= (length > 0.0) ? linePoint + normalize(lineVec) * (length) : linePoint;
  			//intersection= (length > 600.0) ? linePoint + normalize(lineVec) * (length-600) : linePoint;
  			intersection= (length > 1.0) ? linePoint + normalize(lineVec) * (length-1.0) : linePoint;
  			
			//create a vector from the linePoint to the intersection point
//			intersectVector = normalize(lineVec) * length;
//			intersectVector = normalize(lineVec) * 3000;
//			intersectVector = normalize(lineVec) * (length-600);
//			intersectVector = normalize(lineVec) * (length);
 
			//get the coordinates of the line-plane intersection point
//			intersection = linePoint + intersectVector;	
// 			intersection= (length > 600.0) ? intersection : linePoint;

			return intersection;	
		}
 
		//output not valid
//		else{
//			return false;
//		}
}


  
v2f vert (appdata v)
{
    v2f o;
    
	float4 _LightDirWorldSpace = float4(_Godray_WorldSunDir,0.0);
	float3 _LightDirObjectSpace = mul(unity_WorldToObject,_LightDirWorldSpace);
	
	float3 _LightDirViewSpace = mul(UNITY_MATRIX_MV, float4(_LightDirObjectSpace,0.0)); 
	v.vertex = mul(UNITY_MATRIX_MV, v.vertex);  //both in view space

    float3 toLight=normalize(_LightDirViewSpace); //why isn't lightDirWorldSpace already normalized?
    
    float backFactor = dot( toLight, mul(UNITY_MATRIX_MV,float4(v.normal,0.0)) );
   	float backfaceFactor = dot(float3(0,0,1),    mul(UNITY_MATRIX_MV,float4(v.normal,0.0)));
   	backfaceFactor = (backfaceFactor < 0.0) ? 1.0 : 0.0;
   
    float extrude = (backFactor < 0.0) ? 1.0 : 0.0;
    
    float towardsSunFactor=dot(toLight,float3(0,0,1));
   	float projectOnNearPlane = (towardsSunFactor < 0.0) ? 1.0 : 0.0;
	
	v.vertex.xyz=  (projectOnNearPlane * extrude > 0.0) ? 
		LinePlaneIntersection(v.vertex.xyz, -toLight,float3(0,0,1), 0) 
		: (v.vertex.xyz = v.vertex.xyz - toLight * (extrude  *  1000000));

    
    o.pos = mul (UNITY_MATRIX_P, v.vertex);
    o.vertexPosView = v.vertex.xyz;
    o.vertexPosClip = o.pos;
    return o;

}

struct fout {
	float4 color : COLOR;
	float depth : DEPTH;
};
 
//float4 frag(v2f i) : COLOR {
fout frag(v2f i) {
    
    fout OUT;
    OUT.color=abs(i.vertexPosView.z) / 750000.0;

	float C=1.0;
	float _offset=2.0;
	OUT.depth = (log(C * i.vertexPosClip.z + _offset) / log(C * _ProjectionParams.z + _offset));
	return OUT;
}
ENDCG
    }
}
}