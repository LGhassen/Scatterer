// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Scatterer/Terrain - test" {
  SubShader {
    Tags {"Queue" = "Geometry" "IgnoreProjector" = "False" "RenderType" = "Opaque"}

    Pass
    {
      Ztest On
      ZWrite On
      Blend OneMinusSrcAlpha SrcAlpha
      Cull back
	
//	  Tags { "LightMode"="ForwardBase"}

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma glsl
      #pragma target 3.0
//      #pragma multi_compile_fwdbase
	  
	//  #include "UnityShaderVariables.cginc"
      #include "UnityCG.cginc"
//      #include "AutoLight.cginc"
//      #include "Lighting.cginc"
//      #include "AtmosphereScatterer.cginc"


      struct v2f {
      	float4 pos : SV_POSITION;
        //float3 worldPos: TEXCOORD0;
        //float3 worldNormal: TEXCOORD1;
        //float3 vColor: TEXCOORD2;
  		
  		//self-explanatory
		//LIGHTING_COORDS(3,4)
				
      };

      v2f vert(appdata_full v) {

		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

//        o.vColor = v.color.rgb;
//        
//        o.worldPos = mul(_Object2World, v.vertex).xyz;
//
//  		o.worldNormal = normalize(mul( _Object2World, float4(v.normal, 0)).xyz);
//
//        TRANSFER_VERTEX_TO_FRAGMENT(o);
        
        return o;
      }

      half4 frag (v2f IN) : COLOR
      {
//		float3 lightDir= _WorldSpaceLightPos0.xyz;
//		
//		float lightTerm=0.5*dot(IN.worldNormal,lightDir)+0.5;
        
//        return float4(lightTerm,lightTerm,lightTerm,1.0);
		return half4(1.0,0.0,0.0,1.0);
      }
     ENDCG
  	}
  }
  //FallBack "Diffuse"
}