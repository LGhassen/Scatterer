Shader "Custom/DepthTexture" {
SubShader {
    Tags { "RenderType"="Opaque" }
    Pass {
        Fog { Mode Off }
CGPROGRAM
 
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
 
struct v2f {
    float4 pos : SV_POSITION;
    float2 depth : TEXCOORD0;
};
 
v2f vert (appdata_base v) {
    v2f o;
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    
    
//        z = log(C*w + 1) / log(C*Far + 1) * w      //DirectX with depth range 0..1
//or 
//    z = (2*log(C*w + 1) / log(C*Far + 1) - 1) * w   //OpenGL, depth range -1..1
    
    float C=1;
    
    
//    o.pos.z = log(C* o.pos.w + 1) / log(C*_ProjectionParams.z + 1);
	
	o.pos.z = log(C * o.pos.w + 1.0) / log(C * 750000.0f + 1.0);
    o.pos.z*=o.pos.w;
    
    o.depth=o.pos.zw;
    return o;
}
 
half4 frag(v2f i) : COLOR {
    return i.depth.x/i.depth.y;
}
ENDCG
    }
}
}