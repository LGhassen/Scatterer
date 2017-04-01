Shader "Scatterer/AtmosphericScatter" {
    SubShader {
          Tags { "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
//scattering pass
Pass {
			Cull Off
			ZTest Off
			ZWrite Off
    		Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma glsl
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "AtmosphereScatterer.cginc"



            #pragma multi_compile GODRAYS_OFF GODRAYS_ON
//			#pragma multi_compile ECLIPSES_OFF ECLIPSES_ON
			#pragma multi_compile PLANETSHINE_OFF PLANETSHINE_ON
            
            uniform float4x4 _ViewProjInv;
//            uniform float _viewdirOffset;
            uniform float _Scale;
            uniform float _global_alpha;
//            uniform float _Exposure;
            uniform float _global_depth;
            uniform float _Ocean_Sigma;
            uniform float fakeOcean;
            uniform float _fade;
            uniform float3 _Ocean_Color;
            uniform float3 _camPos; // camera position relative to planet's origin
            uniform sampler2D _customDepthTexture;
#if defined (GODRAYS_ON)
            uniform sampler2D _godrayDepthTexture;
#endif
            uniform float4 _MainTex_TexelSize;
            uniform float _openglThreshold;
            uniform float _horizonDepth;
            uniform float4x4 _Globals_CameraToWorld;
            
//            //eclipse uniforms
//#if defined (ECLIPSES_ON)			
//			uniform float4 sunPosAndRadius; //xyz sun pos w radius
//			uniform float4x4 lightOccluders1; //array of light occluders
//											 //for each float4 xyz pos w radius
//			uniform float4x4 lightOccluders2;
//#endif
        
#if defined (PLANETSHINE_ON)
			uniform float4x4 planetShineSources;
			uniform float4x4 planetShineRGB;
#endif

            struct v2f {
                float4 pos: SV_POSITION;
                float4 screenPos: TEXCOORD0;
                float2 uv: TEXCOORD1;
                float2 uv_depth: TEXCOORD2;
                float3 view_dir:TEXCOORD3;
            };
            v2f vert(appdata_base v) {
                v2f o;

                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = o.screenPos.xy / o.screenPos.w;
                o.uv_depth = o.uv;
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                o.uv.y = 1 - o.uv.y;
                #endif
                COMPUTE_EYEDEPTH(o.screenPos.z); //o = -mul( UNITY_MATRIX_MV, v.vertex ).z
				o.view_dir = mul(unity_ObjectToWorld, v.vertex) - _WorldSpaceCameraPos;
                return o;
            }
			
			fixed4 frag (v2f i) : SV_Target
			{

				float depth = tex2D(_customDepthTexture, i.uv_depth).r;
                bool infinite = (depth == 1.0); //basically viewer ray isn't hitting any terrain
                
				float3 rayDir=i.view_dir;
				

                float4 H = float4(i.uv_depth.x * 2.0f - 1.0f, i.uv_depth.y * 2.0f - 1.0f, depth, 1.0f);
                float4 D = mul(_ViewProjInv, H); //reconstruct world position from depth
                						  //in this case lerped depth and godray depth
                float3 worldPos = D / D.w;

				// sample the texture
//				fixed4 col = fixed4(1.0,0.0,0.0,1.0);
				fixed4 col = fixed4(1-depth,0.0,0.0,1.0);

				return col;
			}
			ENDCG
		}
	}
}