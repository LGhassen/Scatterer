Shader "Custom/viewCustomDepthShader"
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
 
            uniform sampler2D _DepthTex; //the depth texture
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD1; //Screen position of pos
            };
 
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.projPos = ComputeScreenPos(o.pos);
 
                return o;
            }
            
            
//            // Z buffer to linear 0..1 depth (0 at eye, 1 at far plane)
//			inline float Linear01Depth( float z )
//			{
//				return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
//			}
			
 
            half4 frag(v2f i) : COLOR
            {
                //Grab the depth value from the depth texture
                //Linear01Depth restricts this value to [0, 1]
                
//                float z = tex2Dproj(_DepthTex,UNITY_PROJ_COORD(i.projPos)).r;
				float z = (tex2D(_DepthTex, i.projPos.xy)).r;
                float C=0.005;
                
//                z = (exp(z*log(C* 750000 +1)) - 1) /  C;
                z = (pow(C*750000+1,z)-1)/C;
                
//                float depth = Linear01Depth (z);
//				float depth = z;
				float depth = z/750000;
//				float farpDivNearp = 750000.0 / 300.0;
//				float depth = 1.0 / (z* (1.0 - farpDivNearp) + farpDivNearp);
 				
 				if (depth >=0.99)
 					depth=0.0;
 
 				
                half4 c;
                c.r = depth;
                c.g = depth;
                c.b = depth;
//				c = (tex2D(_DepthTex, i.projPos.xy));

//                c.r = i.projPos.x;
//                c.g = i.projPos.y;
//                c.b = 0;
				

                c.a = 1;
 
                return c;
            }
 
            ENDCG
        }
    }
    FallBack "VertexLit"
}