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
    o.depth=o.pos.zw;
    return o;
}
 
float4 frag(v2f i) : COLOR
{
    return (i.depth.x/i.depth.y);
}
ENDCG
    }
}
}