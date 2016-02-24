Shader "Custom/VolumeDepth" {


SubShader {
    Tags { "RenderType"="Opaque" }
      
    Pass {
        Fog { Mode Off }
//        Cull front
//        ztest off
        
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
    float2 depth : TEXCOORD0;
//    float3 backfaceFactor: TEXCOORD1;
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
//  			intersection= (length > 0.0) ? linePoint + normalize(lineVec) * (length) : linePoint;
  			intersection= (length > 600.0) ? linePoint + normalize(lineVec) * (length-600) : linePoint;
  			
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


  
v2f vert (appdata v) {
//    v2f o;
//    
////	float4 _LightDirWorldSpace = float4(_Godray_WorldSunDirX,_Godray_WorldSunDirY,_Godray_WorldSunDirZ,0.0);
//	float4 _LightDirWorldSpace = float4(_Godray_WorldSunDir,0.0);
//	float3 _LightDirObjectSpace = mul(_World2Object,_LightDirWorldSpace);
//
//    float3 toLight=normalize(_LightDirObjectSpace);
//    
//    float backFactor = dot( toLight, v.normal );
//   
//    float extrude = (backFactor < 0.0) ? 1.0 : 0.0;
//   	v.vertex.xyz -= toLight * (extrude * 1000000);
//    
//    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
//    o.depth=o.pos.zw;
//    return o;


    v2f o;
    
//	float4 _LightDirWorldSpace = float4(_Godray_WorldSunDirX,_Godray_WorldSunDirY,_Godray_WorldSunDirZ,0.0);
	float4 _LightDirWorldSpace = float4(_Godray_WorldSunDir,0.0);
	float3 _LightDirObjectSpace = mul(_World2Object,_LightDirWorldSpace);
	
	float3 _LightDirViewSpace = mul(UNITY_MATRIX_MV, float4(_LightDirObjectSpace,0.0)); 
	v.vertex = mul(UNITY_MATRIX_MV, v.vertex);  //both in view space

    float3 toLight=normalize(_LightDirViewSpace);
    
    float backFactor = dot( toLight, mul(UNITY_MATRIX_MV,float4(v.normal,0.0)) );
   	float backfaceFactor = dot(float3(0,0,1),    mul(UNITY_MATRIX_MV,float4(v.normal,0.0)));
   	backfaceFactor = (backfaceFactor < 0.0) ? 1.0 : 0.0;
   
    float extrude = (backFactor < 0.0) ? 1.0 : 0.0;
    
    float towardsSunFactor=dot(toLight,float3(0,0,1));
   	float projectOnNearPlane = (towardsSunFactor < 0.0) ? 1.0 : 0.0;
   	
//   	v.vertex.xyz -= toLight * (extrude * 1000000);
//	v.vertex.xyz -= toLight * (extrude  *  1000000);
	
	v.vertex.xyz=  (projectOnNearPlane * extrude > 0.0) ? LinePlaneIntersection(v.vertex.xyz, -toLight,float3(0,0,1), 0) : (v.vertex.xyz = v.vertex.xyz - toLight * (extrude  *  1000000));
//	v.vertex.xyz=  (projectOnNearPlane * extrude > 0.0) ? LinePlaneIntersection(v.vertex.xyz, -toLight,float3(0,0,1), float3(0,0,600)) : (v.vertex.xyz = v.vertex.xyz - toLight * (extrude  *  1000000));

    
    o.pos = mul (UNITY_MATRIX_P, v.vertex);
    o.depth=o.pos.zw;

    return o;

}
 
float4 frag(v2f i) : COLOR {
    
    //get length of view space vertex -> distance to camera
    //store in buffer and do all your front face back face stuff
    //check depth is inferior to zg first
    return i.depth.x/i.depth.y;
}
ENDCG
    }
}
}