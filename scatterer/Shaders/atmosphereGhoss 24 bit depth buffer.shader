
Shader "Sky/AtmosphereGhoss" {
    SubShader {
        Tags {
            "Queue" = "Transparent-5"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        Pass {  //extinction pass
            ZWrite off
            Fog {
                Mode Off
            }
            Cull Front
            Blend DstColor Zero //multiplicative
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma glsl
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "AtmosphereScatterer.cginc"
            uniform float4x4 _ViewProjInv;
            uniform float _viewdirOffset;
            uniform float _Scale;
            uniform float _global_alpha;
            uniform float _global_depth;
            uniform float _Ocean_Sigma;
            uniform float fakeOcean;
            uniform float _fade;
            uniform float3 _Ocean_Color;
            uniform float3 _camPos; // camera position relative to planet's origin
            uniform float _Post_Extinction_Tint;
            uniform float postExtinctionMultiplier;
            uniform sampler2D _customDepthTexture;
            uniform float4 _MainTex_TexelSize;
            uniform float _openglThreshold;
            //   uniform float _edgeThreshold;
            uniform float _horizonDepth;
            uniform float4x4 _Globals_CameraToWorld;
            struct v2f {
                float4 pos: SV_POSITION;
                float4 screenPos: TEXCOORD0;
                float2 uv: TEXCOORD1;
                float2 uv_depth: TEXCOORD2;
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
                return o;
            }
            half4 frag(v2f i): COLOR
            {
                float depth = tex2D(_customDepthTexture, i.uv_depth).r;
                bool infinite = (depth == 1.0);
                float4 H = float4(i.uv_depth.x * 2.0f - 1.0f, (i.uv_depth.y) * 2.0f - 1.0f, depth, 1.0f);
                float4 D = mul(_ViewProjInv, H);
                float3 worldPos = D / D.w;  //reconstruct world position from depth
                float interSectPt = intersectSphere2(_camPos, worldPos, float3(0.0, 0.0, 0.0), Rg);
                //this ensures that we're looking in the right direction
                //That is, the ocean surface intersection point is in front of us
                //If we look up the intersection point is behind us and we don't want to use that
                bool rightDir = (interSectPt > 0);
                //                bool infinite = (length(worldPos) >= (Rg + (Rt - Rg) * _experimentalAtmoScale)); //basically viewer ray isn't hitting any terrain
                if (!(rightDir) && (infinite))
                {
                    return float4(1.0, 1.0, 1.0, 1.0);
                }
                float3 worldPos2 = _camPos + interSectPt * (worldPos - (_camPos));
                //this condition ensures the ocean is in front of the terrain
                //if the terrain is in front of the ocean we don't want to cover it up
                //with the wrong postprocessing depth
                bool oceanCloserThanTerrain = (length(worldPos2 - _camPos) < length(worldPos - _camPos));
                float3 oceanColor = float3(1.0, 1.0, 1.0);
                if ((rightDir) && oceanCloserThanTerrain)
                {
                    worldPos = worldPos2;
                }
                //artifacts fix
                worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ;
                float3 extinction = float3(0, 0, 0);
                float3 inscatter = InScattering2(_camPos, worldPos, extinction, 1.0, 1.0, 1.0);
                float average=(extinction.r+extinction.g+extinction.b)/3;
                extinction = float3(_Post_Extinction_Tint*extinction.r + (1-_Post_Extinction_Tint)*average,
                _Post_Extinction_Tint*extinction.g + (1-_Post_Extinction_Tint)*average,
                _Post_Extinction_Tint*extinction.b + (1-_Post_Extinction_Tint)*average);
                //                extinction = lerp(average, extinction, _Post_Extinction_Tint);
                extinction = lerp (float3(1,1,1), extinction, postExtinctionMultiplier);
                return float4(extinction, 1.0);
            }
            ENDCG
        }


Pass {
            //scattering pass
            ZWrite off
            Fog {
                Mode Off
            }
            Cull Front
            Blend OneMinusDstColor One //soft additive
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma glsl
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "AtmosphereScatterer.cginc"
            #pragma multi_compile GODRAYS_OFF GODRAYS_ON
            uniform float4x4 _ViewProjInv;
            uniform float _viewdirOffset;
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
            struct v2f {
                float4 pos: SV_POSITION;
                float4 screenPos: TEXCOORD0;
                float2 uv: TEXCOORD1;
                float2 uv_depth: TEXCOORD2;
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
                return o;
            }
            half4 frag(v2f i): COLOR
            {
                float depth = tex2D(_customDepthTexture, i.uv_depth).r;
                bool infinite = (depth == 1.0);
                float4 H = float4(i.uv_depth.x * 2.0f - 1.0f, i.uv_depth.y * 2.0f - 1.0f, depth, 1.0f);
                float4 D = mul(_ViewProjInv, H);
                float3 worldPos = D / D.w;  //reconstruct world position from depth
                //viewdir the stupid way, just for testing
                //////////////////////////////////////////////////////////////////////////////////////
                float3 viewdir = normalize(worldPos-_camPos);
                #if defined (GODRAYS_ON)
                float3 SidewaysFromSun = normalize(cross(_camPos,SUN_DIR));
                float godrayBlendFactor= 1-abs (dot(SidewaysFromSun,viewdir));
                float godrayDepth= tex2D(_godrayDepthTexture, i.uv_depth).r;
                if ((godrayDepth > 0) && (godrayDepth < depth)&&(depth<1))
                {
                    depth=lerp(depth, godrayDepth, godrayBlendFactor);
                    //                   depth=godrayDepth;
                }
                #endif
                H = float4(i.uv_depth.x * 2.0f - 1.0f, i.uv_depth.y * 2.0f - 1.0f, depth, 1.0f);
                D = mul(_ViewProjInv, H);
                worldPos = D / D.w;
                /////////////////////////////////////////////////////////////////////////////////////////
                float interSectPt = intersectSphere2(_camPos, worldPos, float3(0.0, 0.0, 0.0), Rg);
                //this ensures that we're looking in the right direction
                //That is, the ocean surface intersection point is in front of us
                //If we look up the intersection point is behind us and we don't want to use that
                bool rightDir = (interSectPt > 0);
                //                bool infinite = (length(worldPos) >= (Rg + (Rt - Rg) * _experimentalAtmoScale)); //basically viewer ray isn't hitting any terrain
                if (!(rightDir) && (infinite))
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }
                float3 worldPos2 = _camPos + interSectPt * (worldPos - (_camPos));
                //this condition ensures the ocean is in front of the terrain
                //if the terrain is in front of the ocean we don't want to cover it up
                //with the wrong postprocessing depth
                bool oceanCloserThanTerrain = (length(worldPos2 - _camPos) < length(worldPos - _camPos));
                float3 oceanColor = float3(1.0, 1.0, 1.0);
                worldPos = ((rightDir) && oceanCloserThanTerrain) ? worldPos2 : worldPos;
                //artifacts fix
                worldPos= (length(worldPos) < (Rg + _openglThreshold)) ? (Rg + _openglThreshold) * normalize(worldPos) : worldPos ;
                float3 extinction = float3(0, 0, 0);
                float3 inscatter = InScattering2(_camPos, worldPos, extinction, 1.0, 1.0, 1.0);
                float visib = 1;
                float dpth = length(worldPos - _camPos);
                visib = (dpth <= _global_depth) ? (1 - exp(-1 * (4 * dpth / _global_depth))) : visib;
                return float4(hdr(inscatter)*_global_alpha * visib, 1);
            }
            ENDCG
        }
    }
}