#if defined(SHADER_TARGET_GLSL) || defined(SHADER_API_WIIU)
        #define UNITY_COMPILER_HLSL2GLSL
#endif

#if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(SCATTERER_COMPILER_HLSLCC) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLCORE)
//    // DX11 & hlslcc platforms: built-in PCF, not sure why but works on opengl as well
    #define SCATTERER_DECLARE_SHADOWMAP(tex) Texture2D tex; SamplerComparisonState sampler##tex
    #define SCATTERER_SAMPLE_SHADOW(tex,coord) tex.SampleCmpLevelZero (sampler##tex,(coord).xy,(coord).z)
    #define SCATTERER_SAMPLE_SHADOW_PROJ(tex,coord) tex.SampleCmpLevelZero (sampler##tex,(coord).xy/(coord).w,(coord).z/(coord).w)
//////#elif defined(SCATTERER_COMPILER_HLSL2GLSL) && defined(SHADOWS_NATIVE)
#elif defined(UNITY_COMPILER_HLSL2GLSL)
    // OpenGL-like hlsl2glsl platforms: most of them always have built-in PCF
    #define SCATTERER_DECLARE_SHADOWMAP(tex) sampler2DShadow tex
    #define SCATTERER_SAMPLE_SHADOW(tex,coord) shadow2D (tex,(coord).xyz)
    #define SCATTERER_SAMPLE_SHADOW_PROJ(tex,coord) shadow2Dproj (tex,coord)
#elif defined(SHADER_API_D3D9)
    // D3D9: Native shadow maps FOURCC "driver hack", looks just like a regular
    // texture sample. Have to always do a projected sample
    // so that HLSL compiler doesn't try to be too smart and mess up swizzles
    // (thinking that Z is unused).
    #define SCATTERER_DECLARE_SHADOWMAP(tex) sampler2D tex
    #define SCATTERER_SAMPLE_SHADOW(tex,coord) tex2Dproj (tex,float4((coord).xyz,1)).r
    #define SCATTERER_SAMPLE_SHADOW_PROJ(tex,coord) tex2Dproj (tex,coord).r
#else
    // Fallback / No built-in shadowmap comparison sampling: regular texture sample and do manual depth comparison
    #define SCATTERER_DECLARE_SHADOWMAP(tex) sampler2D_float tex
    #define SCATTERER_SAMPLE_SHADOW(tex,coord) ((SAMPLE_DEPTH_TEXTURE(tex,(coord).xy) < (coord).z) ? 0.0 : 1.0)
    #define SCATTERER_SAMPLE_SHADOW_PROJ(tex,coord) ((SAMPLE_DEPTH_TEXTURE_PROJ(tex,SCATTERER_PROJ_COORD(coord)) < ((coord).z/(coord).w)) ? 0.0 : 1.0)
#endif

float4 _ShadowMapTexture_TexelSize;
float4 unity_ShadowCascadeScales;
SCATTERER_DECLARE_SHADOWMAP(_ShadowMapTexture);

float4 getCascadeWeights(float3 wpos, float z)
{
    float4 zNear = float4( z >= _LightSplitsNear );
    float4 zFar = float4( z < _LightSplitsFar );
    float4 weights = zNear * zFar;
    return weights;
}

inline float4 getCascadeWeights_splitSpheres(float3 wpos)
{
    float3 fromCenter0 = wpos.xyz - unity_ShadowSplitSpheres[0].xyz;
    float3 fromCenter1 = wpos.xyz - unity_ShadowSplitSpheres[1].xyz;
    float3 fromCenter2 = wpos.xyz - unity_ShadowSplitSpheres[2].xyz;
    float3 fromCenter3 = wpos.xyz - unity_ShadowSplitSpheres[3].xyz;
    float4 distances2 = float4(dot(fromCenter0,fromCenter0), dot(fromCenter1,fromCenter1), dot(fromCenter2,fromCenter2), dot(fromCenter3,fromCenter3));
    fixed4 weights = float4(distances2 < unity_ShadowSplitSqRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);
    return weights;
}

float4 getShadowCoord( float4 wpos, float4 cascadeWeights )
{
    float3 sc0 = mul (unity_WorldToShadow[0], wpos).xyz;
    float3 sc1 = mul (unity_WorldToShadow[1], wpos).xyz;
    float3 sc2 = mul (unity_WorldToShadow[2], wpos).xyz;
    float3 sc3 = mul (unity_WorldToShadow[3], wpos).xyz;

    float4 shadowMapCoordinate = float4(sc0 * cascadeWeights[0] + sc1 * cascadeWeights[1] + sc2 * cascadeWeights[2] + sc3 * cascadeWeights[3],1.0);
    
    //if reverse z
//	float  noCascadeWeights = 1 - dot(cascadeWeights, float4(1, 1, 1, 1));
//    shadowMapCoordinate.z -= noCascadeWeights;
	
    return shadowMapCoordinate;
}

float _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(float triangleHeight)
{
    return triangleHeight - 0.5;
}

void _UnityInternalGetAreaPerTexel_3TexelsWideTriangleFilter(float offset, out float4 computedArea, out float4 computedAreaUncut)
{
    //Compute the exterior areas
    float offset01SquaredHalved = (offset + 0.5) * (offset + 0.5) * 0.5;
    computedAreaUncut.x = computedArea.x = offset01SquaredHalved - offset;
    computedAreaUncut.w = computedArea.w = offset01SquaredHalved;

    //Compute the middle areas
    //For Y : We find the area in Y of as if the left section of the isoceles triangle would
    //intersect the axis between Y and Z (ie where offset = 0).
    computedAreaUncut.y = _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(1.5 - offset);
    //This area is superior to the one we are looking for if (offset < 0) thus we need to
    //subtract the area of the triangle defined by (0,1.5-offset), (0,1.5+offset), (-offset,1.5).
    float clampedOffsetLeft = min(offset,0);
    float areaOfSmallLeftTriangle = clampedOffsetLeft * clampedOffsetLeft;
    computedArea.y = computedAreaUncut.y - areaOfSmallLeftTriangle;

    //We do the same for the Z but with the right part of the isoceles triangle
    computedAreaUncut.z = _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(1.5 + offset);
    float clampedOffsetRight = max(offset,0);
    float areaOfSmallRightTriangle = clampedOffsetRight * clampedOffsetRight;
    computedArea.z = computedAreaUncut.z - areaOfSmallRightTriangle;
}


void _UnityInternalGetWeightPerTexel_7TexelsWideTriangleFilter(float offset, out float4 texelsWeightsA, out float4 texelsWeightsB)
{
    float4 computedArea_From3texelTriangle;
    float4 computedAreaUncut_From3texelTriangle;
    _UnityInternalGetAreaPerTexel_3TexelsWideTriangleFilter(offset, computedArea_From3texelTriangle, computedAreaUncut_From3texelTriangle);

    //Triangle slop is 45 degree thus we can almost reuse the result of the 3 texel wide computation.
    //the 7 texel wide triangle can be seen as the 3 texel wide one but shifted up by two unit/texel.
    //0.081632 is 1/(the triangle area)
    texelsWeightsA.x = 0.081632 * (computedArea_From3texelTriangle.x);
    texelsWeightsA.y = 0.081632 * (computedAreaUncut_From3texelTriangle.y);
    texelsWeightsA.z = 0.081632 * (computedAreaUncut_From3texelTriangle.y + 1);
    texelsWeightsA.w = 0.081632 * (computedArea_From3texelTriangle.y + 2);
    texelsWeightsB.x = 0.081632 * (computedArea_From3texelTriangle.z + 2);
    texelsWeightsB.y = 0.081632 * (computedAreaUncut_From3texelTriangle.z + 1);
    texelsWeightsB.z = 0.081632 * (computedAreaUncut_From3texelTriangle.z);
    texelsWeightsB.w = 0.081632 * (computedArea_From3texelTriangle.w);
}


float3 UnityCombineShadowcoordComponents(float2 baseUV, float2 deltaUV, float depth, float3 receiverPlaneDepthBias)
{
    float3 uv = float3(baseUV + deltaUV, depth + receiverPlaneDepthBias.z);
    uv.z += dot(deltaUV, receiverPlaneDepthBias.xy);
    return uv;
}

//Same as the getShadowCoord; but optimized for single cascade
inline float4 getShadowCoord_SingleCascade( float4 wpos )
{
    return float4( mul (unity_WorldToShadow[0], wpos).xyz, 0);
}


half UnitySampleShadowmap_PCF7x7(float4 coord, float3 receiverPlaneDepthBias, float4 _ShadowMapTexture_TexelSize)
{
    half shadow = 1;

//    #ifndef SHADOWS_NATIVE
//        // when we don't have hardware PCF sampling, fallback to a simple 3x3 sampling with averaged results.
//        return UnitySampleShadowmap_PCF3x3NoHardwareSupport(coord, receiverPlaneDepthBias);
//    #endif

    // tent base is 7x7 base thus covering from 49 to 64 texels, thus we need 16 bilinear PCF fetches
    float2 tentCenterInTexelSpace = coord.xy * _ShadowMapTexture_TexelSize.zw;
    float2 centerOfFetchesInTexelSpace = floor(tentCenterInTexelSpace + 0.5);
    float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace;

    // find the weight of each texel based on the area of a 45 degree slop tent above each of them.
    float4 texelsWeightsU_A, texelsWeightsU_B;
    float4 texelsWeightsV_A, texelsWeightsV_B;
    _UnityInternalGetWeightPerTexel_7TexelsWideTriangleFilter(offsetFromTentCenterToCenterOfFetches.x, texelsWeightsU_A, texelsWeightsU_B);
    _UnityInternalGetWeightPerTexel_7TexelsWideTriangleFilter(offsetFromTentCenterToCenterOfFetches.y, texelsWeightsV_A, texelsWeightsV_B);

    // each fetch will cover a group of 2x2 texels, the weight of each group is the sum of the weights of the texels
    float4 fetchesWeightsU = float4(texelsWeightsU_A.xz, texelsWeightsU_B.xz) + float4(texelsWeightsU_A.yw, texelsWeightsU_B.yw);
    float4 fetchesWeightsV = float4(texelsWeightsV_A.xz, texelsWeightsV_B.xz) + float4(texelsWeightsV_A.yw, texelsWeightsV_B.yw);

    // move the PCF bilinear fetches to respect texels weights
    float4 fetchesOffsetsU = float4(texelsWeightsU_A.yw, texelsWeightsU_B.yw) / fetchesWeightsU.xyzw + float4(-3.5,-1.5,0.5,2.5);
    float4 fetchesOffsetsV = float4(texelsWeightsV_A.yw, texelsWeightsV_B.yw) / fetchesWeightsV.xyzw + float4(-3.5,-1.5,0.5,2.5);
    fetchesOffsetsU *= _ShadowMapTexture_TexelSize.xxxx;
    fetchesOffsetsV *= _ShadowMapTexture_TexelSize.yyyy;

    // fetch !
    float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * _ShadowMapTexture_TexelSize.xy;

    shadow  = fetchesWeightsU.x * fetchesWeightsV.x * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.x * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.x * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.x * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.y * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.y * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.y * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.y * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.z * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.z * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.z * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.z * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.z), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.w * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.w * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.z * fetchesWeightsV.w * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.z, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.w * fetchesWeightsV.w * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.w, fetchesOffsetsV.w), coord.z, receiverPlaneDepthBias));

    return shadow;
}

/**
 * Assuming a isoceles triangle of 1.5 texels height and 3 texels wide lying on 4 texels.
 * This function return the weight of each texels area relative to the full triangle area.
 */
void _UnityInternalGetWeightPerTexel_3TexelsWideTriangleFilter(float offset, out float4 computedWeight)
{
    float4 dummy;
    _UnityInternalGetAreaPerTexel_3TexelsWideTriangleFilter(offset, computedWeight, dummy);
    computedWeight *= 0.44444;//0.44 == 1/(the triangle area)
}


//PCF tent shadowmap filtering based on a 3x3 kernel (optimized with 4 taps)
half UnitySampleShadowmap_PCF3x3Tent(float4 coord, float3 receiverPlaneDepthBias, float4 _ShadowMapTexture_TexelSize)
{
    half shadow = 1;

//    #ifndef SHADOWS_NATIVE
//        // when we don't have hardware PCF sampling, fallback to a simple 3x3 sampling with averaged results.
//        return UnitySampleShadowmap_PCF3x3NoHardwareSupport(coord, receiverPlaneDepthBias);
//    #endif

    // tent base is 3x3 base thus covering from 9 to 12 texels, thus we need 4 bilinear PCF fetches
    float2 tentCenterInTexelSpace = coord.xy * _ShadowMapTexture_TexelSize.zw;
    float2 centerOfFetchesInTexelSpace = floor(tentCenterInTexelSpace + 0.5);
    float2 offsetFromTentCenterToCenterOfFetches = tentCenterInTexelSpace - centerOfFetchesInTexelSpace;

    // find the weight of each texel based
    float4 texelsWeightsU, texelsWeightsV;
    _UnityInternalGetWeightPerTexel_3TexelsWideTriangleFilter(offsetFromTentCenterToCenterOfFetches.x, texelsWeightsU);
    _UnityInternalGetWeightPerTexel_3TexelsWideTriangleFilter(offsetFromTentCenterToCenterOfFetches.y, texelsWeightsV);

    // each fetch will cover a group of 2x2 texels, the weight of each group is the sum of the weights of the texels
    float2 fetchesWeightsU = texelsWeightsU.xz + texelsWeightsU.yw;
    float2 fetchesWeightsV = texelsWeightsV.xz + texelsWeightsV.yw;

    // move the PCF bilinear fetches to respect texels weights
    float2 fetchesOffsetsU = texelsWeightsU.yw / fetchesWeightsU.xy + float2(-1.5,0.5);
    float2 fetchesOffsetsV = texelsWeightsV.yw / fetchesWeightsV.xy + float2(-1.5,0.5);
    fetchesOffsetsU *= _ShadowMapTexture_TexelSize.xx;
    fetchesOffsetsV *= _ShadowMapTexture_TexelSize.yy;

    // fetch !
    float2 bilinearFetchOrigin = centerOfFetchesInTexelSpace * _ShadowMapTexture_TexelSize.xy;
    shadow =  fetchesWeightsU.x * fetchesWeightsV.x * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.x * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.x), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.x * fetchesWeightsV.y * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.x, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));
    shadow += fetchesWeightsU.y * fetchesWeightsV.y * SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, UnityCombineShadowcoordComponents(bilinearFetchOrigin, float2(fetchesOffsetsU.y, fetchesOffsetsV.y), coord.z, receiverPlaneDepthBias));

    return shadow;
}

half getOceanShadow(float4 worldPos, float viewPosZ)
{

	half shadowTerm = 1.0;
	float4 cascadeWeights = getCascadeWeights(worldPos.xyz, viewPosZ);			
	//float4 cascadeWeights = getCascadeWeights_splitSpheres(worldPos);
    float4 shadowCoord 	  = getShadowCoord(float4(worldPos.xyz,1.0), cascadeWeights);

    bool noShadow = (length(cascadeWeights) == 0.0); 						//optimize this
#if SHADER_API_D3D11
    shadowCoord.z = clamp(shadowCoord.z,0.01,0.99);
#endif


#if defined (OCEAN_SHADOWS_HARD)
	shadowTerm= SCATTERER_SAMPLE_SHADOW(_ShadowMapTexture, shadowCoord);

#elif defined (OCEAN_SHADOWS_SOFT)
    shadowTerm = UnitySampleShadowmap_PCF7x7(shadowCoord, 0.0,_ShadowMapTexture_TexelSize);

    half4 z4 = (float4(viewPosZ,viewPosZ,viewPosZ,viewPosZ) - _LightSplitsNear) / (_LightSplitsFar - _LightSplitsNear);
    half alpha = dot(z4 * cascadeWeights, half4(1,1,1,1));

    float UNITY_CASCADE_BLEND_DISTANCE = 0.1;

	cascadeWeights = fixed4(0, cascadeWeights.xyz);  //next cascade

	if ((alpha > 1 - UNITY_CASCADE_BLEND_DISTANCE) && (length(cascadeWeights) > 0.0) && !noShadow) //check if we have a next cascade, ie we are not the last cascade of the camera before blending
	{
    	// get alpha to 0..1 range over the blend distance
    	alpha = (alpha - (1 - UNITY_CASCADE_BLEND_DISTANCE)) / UNITY_CASCADE_BLEND_DISTANCE;

    	shadowCoord = getShadowCoord(worldPos,cascadeWeights);

    	half shadowNextCascade = UnitySampleShadowmap_PCF3x3Tent(shadowCoord, 0.0,_ShadowMapTexture_TexelSize);

        shadowTerm = lerp(shadowTerm, shadowNextCascade, alpha);
	}

#endif
	return (noShadow ? 1.0 : shadowTerm);
}

