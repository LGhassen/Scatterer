Shader "Scatterer/SimpleDepthTexture" {
SubShader {
    Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
    Pass {
    	Tags { "RenderType"="Opaque" "IgnoreProjector" = "True"}
CGPROGRAM
 
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
   
struct v2f {
    float4 pos : SV_POSITION;
};
 
v2f vert (appdata_base v)
{
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
	return 1.0;
}
ENDCG
}
}
////shouldn't need CUT-OUT SUBSHADER, we only target "main" Opaque surfaces
}