// Same as fixedScreenSpaceShadows shader but uses the built-in depth buffer for the first ~8000 and then a custom depth buffer where precision degrades
Shader "Scatterer/longDistanceScreenSpaceShadows" {
Properties {
    _ShadowMapTexture ("", any) = "" {}
    _ODSWorldTexture("", 2D) = "" {}
}

CGINCLUDE
#include "FixedScreenSpaceShadows.cginc"

UNITY_DECLARE_DEPTH_TEXTURE(AdditionalDepthBuffer);
float4x4  ScattererAdditionalInvProjection;

/**
* Get camera space coord from depth and inv projection matrices
*/
inline float3 computeCameraSpacePosFromDualDepthAndInvProjMat(v2f i)
{
    float zdepth  = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
    float zdepth2 = SAMPLE_DEPTH_TEXTURE(AdditionalDepthBuffer, i.uv.xy);

    #if defined(UNITY_REVERSED_Z)
        zdepth  = 1 - zdepth;
        zdepth2 = 1 - zdepth2;
    #endif

    float4 clipPos = float4(i.uv.zw, zdepth, 1.0);
    clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
    float4 camPos = mul(unity_CameraInvProjection, clipPos);
    camPos.xyz /= camPos.w;
    camPos.z *= -1;

    float4 clipPos2 = float4(i.uv.zw, zdepth2, 1.0);
    clipPos2.xyz = 2.0f * clipPos2.xyz - 1.0f;
    float4 camPos2 = mul(ScattererAdditionalInvProjection, clipPos2);
    camPos2.xyz /= camPos2.w;
    camPos2.z *= -1;

    return length(camPos.xyz) < 8000 ? camPos.xyz : camPos2.xyz ;
}
ENDCG

// ----------------------------------------------------------------------------------------
// Subshader for hard shadows:
// Just collect shadows into the buffer. Used on pre-SM3 GPUs and when hard shadows are picked.
// This version does inv projection at the PS level, slower and less precise however more general.

SubShader {
      Tags{ "ShadowmapFilter" = "HardShadow" }
    Pass{
        ZWrite Off ZTest Always Cull Off

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag_hard
        #pragma multi_compile_shadowcollector

        inline float3 computeCameraSpacePosFromDepth(v2f i)
        {
            return computeCameraSpacePosFromDualDepthAndInvProjMat(i);
        }
        ENDCG
    }
}

// ----------------------------------------------------------------------------------------
// Subshader that does soft PCF filtering while collecting shadows.
// Requires SM3 GPU.
// This version does inv projection at the PS level, slower and less precise however more general.
// 

Subshader{
    Tags {"ShadowmapFilter" = "PCF_SOFT"}
    Pass{
        ZWrite Off ZTest Always Cull Off

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag_pcfSoft
        #pragma multi_compile_shadowcollector
        #pragma target 3.0

        inline float3 computeCameraSpacePosFromDepth(v2f i)
        {
            return computeCameraSpacePosFromDualDepthAndInvProjMat(i);
        }

        ENDCG
    }
}

Fallback Off
}