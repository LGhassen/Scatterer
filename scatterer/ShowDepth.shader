
//Shows the grayscale of the depth from the camera.
 
Shader "Custom/DepthShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
 
        Pass
        {
 
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
//            uniform sampler2D _CameraDepthTexture; //the depth texture
															
			uniform sampler2D _CameraDepthNormalsTexture;
			
			float3 normalValues;
			float depthValue;
 
            struct v2f
            {
//                float4 pos : SV_POSITION;
//                float4 projPos : TEXCOORD1; //Screen position of pos

				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
//				float4 interpolatedRay : TEXCOORD2;
            };
 
            v2f vert(appdata_base v)
            {
//                v2f o;
//                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//                o.projPos = ComputeScreenPos(o.pos);
// 
//                return o;
//
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;
				
//				#if UNITY_UV_STARTS_AT_TOP
//				if (_MainTex_TexelSize.y < 0)
//					o.uv.y = 1-o.uv.y;
//				#endif				
				
//				o.interpolatedRay = _FrustumCorners[(int)index];
//				o.interpolatedRay.w = index;
				
				return o;
            }
 
            half4 frag(v2f i) : COLOR
            {
                //Grab the depth value from the depth texture
                //Linear01Depth restricts this value to [0, 1]
//                float depth = Linear01Depth (tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(i.projPos)).r);
                
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv_depth.xy), depthValue, normalValues);
//                float depth=Linear01Depth(depthValue);
                float depth=depthValue;
 
                half4 c;
                c.r = depth;
                c.g = depth;
                c.b = depth;
                c.a = 1;
 
                return c;
            }
 
            ENDCG
        }
    }
    FallBack "VertexLit"
}