#ifndef CUBE_MAP_CG_INC
#define CUBE_MAP_CG_INC

#ifdef MAP_TYPE_CUBE_1
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(cube ## name, vect)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name, vect)
#elif defined (MAP_TYPE_CUBE6_1)
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(cube ## name ## xn, cube ## name ## xp, \
											cube ## name ## yn, cube ## name ## yp, \
											cube ## name ## zn, cube ## name ## zp, vect)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name ## xn, cube ## name ## xp, \
														cube ## name ## yn, cube ## name ## yp, \
														cube ## name ## zn, cube ## name ## zp, vect)
#elif defined (MAP_TYPE_CUBE2_1)
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(cube ## name ## POS, cube ## name ## NEG, vect)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(cube ## name ## POS, cube ## name ## NEG, vect)
#else
#define GET_CUBE_MAP_1(name, vect) GetCubeMap(name, vect)
#define GET_NO_LOD_CUBE_MAP_1(name, vect) GetCubeMapNoLOD(name, vect)
#endif

#define CUBEMAP_DEF(name) \
	uniform samplerCUBE cube ## name; \
	sampler2D cube ## name ## POS; \
	sampler2D cube ## name ## NEG; \
	sampler2D name; \
	sampler2D cube ## name ## xn, cube ## name ## xp; \
	sampler2D cube ## name ## yn, cube ## name ## yp; \
	sampler2D cube ## name ## zn, cube ## name ## zp;


inline float4 CubeDerivatives(float2 uv, float scale)
{
	
	//Make the UV continuous. 
	float2 uvS = abs(uv - (.5*scale));

	float2 uvCont;
	uvCont.x = max(uvS.x, uvS.y);
	uvCont.y = min(uvS.x, uvS.y);

	return float4(ddx(uvCont), ddy(uvCont));
}


inline float4 Derivatives(float2 uv)
{
	float2 uvCont = uv;
	//Make the UV continuous. 
	uvCont.x = abs(uvCont.x - .5);
	return float4(ddx(uvCont), ddy(uvCont));
}

inline float2 GetCubeUV(float3 cubeVect, float2 uvOffset)
{
	float2 uv;
	uv.x = .5 + (INV_2PI*atan2(cubeVect.x, cubeVect.z));
	uv.y = INV_PI*acos(cubeVect.y);
	uv += uvOffset;
	return uv;
}

inline float2 GetCubeCubeUV(float3 cubeVectNorm)
{
	float3 cubeVectNormAbs = abs(cubeVectNorm);
	half zxlerp = step(cubeVectNormAbs.x, cubeVectNormAbs.z);
	half nylerp = step(cubeVectNormAbs.y, max(cubeVectNormAbs.x, cubeVectNormAbs.z));

	half s = lerp(cubeVectNorm.x, cubeVectNorm.z, zxlerp);
	s = sign(lerp(cubeVectNorm.y, s, nylerp));

	float3 detailCoords = lerp(float3(1, -s, 1)*cubeVectNorm.xzy, float3(1, s, 1)*cubeVectNorm.zxy, zxlerp);
	detailCoords = lerp(float3(1, 1, -s)*cubeVectNorm.yxz, detailCoords, nylerp);

	float2 uv;
	uv.x = ((.5*detailCoords.y) / abs(detailCoords.x)) + .5;
	uv.y = ((.5*detailCoords.z) / abs(detailCoords.x)) + .5;
	return uv;
}


inline half4 GetCubeMapNoLOD(sampler2D texSampler, float3 cubeVect)
{
	float4 uv;
	float3 cubeVectNorm = normalize(cubeVect);
	uv.xy = GetCubeUV(cubeVectNorm, float2(0, 0));
	uv.zw = float2(0, 0);
	half4 tex = tex2Dlod(texSampler, uv);
	return tex;
}

inline half4 GetCubeMap(sampler2D texSampler, float3 cubeVect)
{
	float3 cubeVectNorm = normalize(cubeVect);
	float2 uv = GetCubeUV(cubeVectNorm, float2(0, 0));

	float4 uvdd = Derivatives(uv);
	half4 tex = tex2D(texSampler, uv, uvdd.xy, uvdd.zw);
	return tex;
}

inline half4 GetCubeMapNoLOD(samplerCUBE texSampler, float3 cubeVect)
{
	half4 uv;
	uv.xyz = normalize(cubeVect);
	uv.w = 0;
	half4 tex = texCUBElod(texSampler, uv);
	return tex;
}

inline half4 GetCubeMap(samplerCUBE texSampler, float3 cubeVect)
{
	half4 tex = texCUBE(texSampler, normalize(cubeVect));
	return tex;
}


inline half4 GetCubeMapNoLOD(sampler2D texXn, sampler2D texXp, sampler2D texYn, sampler2D texYp, sampler2D texZn, sampler2D texZp, float3 cubeVect)
{
	float4 uv;
	uv.zw = float2(0, 0);

	float3 cubeVectNorm = normalize(cubeVect);
	float3 cubeVectNormAbs = abs(cubeVectNorm);


	half zxlerp = step(cubeVectNormAbs.x, cubeVectNormAbs.z);
	half nylerp = step(cubeVectNormAbs.y, max(cubeVectNormAbs.x, cubeVectNormAbs.z));

	half s = lerp(cubeVectNorm.x, cubeVectNorm.z, zxlerp);
	s = sign(lerp(-cubeVectNorm.y, s, nylerp));

	half3 detailCoords = lerp(half3(1, -s, -1)*cubeVectNorm.xzy, half3(1, s, -1)*cubeVectNorm.zxy, zxlerp);
	detailCoords = lerp(half3(1, 1, s)*cubeVectNorm.yxz, detailCoords, nylerp);

	uv.xy = ((.5*detailCoords.yz) / abs(detailCoords.x)) + .5;

	
	half4 sampxn = tex2Dlod(texXn, uv);
	half4 sampxp = tex2Dlod(texXp, uv);
	half4 sampyn = tex2Dlod(texYn, uv);
	half4 sampyp = tex2Dlod(texYp, uv);
	half4 sampzn = tex2Dlod(texZn, uv);
	half4 sampzp = tex2Dlod(texZp, uv);

	half4 sampx = lerp(sampxn, sampxp, step(0, s));
	half4 sampy = lerp(sampyn, sampyp, step(0, s));
	half4 sampz = lerp(sampzn, sampzp, step(0, s));
	
	half4 samp = lerp(sampx, sampz, zxlerp);
		  samp = lerp(sampy, samp, nylerp);
	

	return samp;
	
}

inline half4 GetCubeMap(sampler2D texXn, sampler2D texXp, sampler2D texYn, sampler2D texYp, sampler2D texZn, sampler2D texZp, float3 cubeVect)
{
	float3 cubeVectNorm = normalize(cubeVect);
	
	float3 cubeVectNormAbs = abs(cubeVectNorm);


	half zxlerp = step(cubeVectNormAbs.x, cubeVectNormAbs.z);
	half nylerp = step(cubeVectNormAbs.y, max(cubeVectNormAbs.x, cubeVectNormAbs.z));

	half s = lerp(cubeVectNorm.x, cubeVectNorm.z, zxlerp);
	s = sign(lerp(cubeVectNorm.y, s, nylerp));

	half3 detailCoords = lerp(half3(1, -s, -1)*cubeVectNorm.xzy, half3(1, s, -1)*cubeVectNorm.zxy, zxlerp);
	detailCoords = lerp(half3(1, 1, s)*cubeVectNorm.yxz, detailCoords, nylerp);

	float2 uv = ((.5*detailCoords.yz) / abs(detailCoords.x)) + .5;
	//this fixes UV discontinuity on Y-X seam by swapping uv coords in derivative calcs when in the X quadrants.
	float4 uvdd = CubeDerivatives(uv, 1);


	half4 sampxn = tex2D(texXn, uv, uvdd.xy, uvdd.zw);
	half4 sampxp = tex2D(texXp, uv, uvdd.xy, uvdd.zw);
	half4 sampyn = tex2D(texYn, uv, uvdd.xy, uvdd.zw);
	half4 sampyp = tex2D(texYp, uv, uvdd.xy, uvdd.zw);
	half4 sampzn = tex2D(texZn, uv, uvdd.xy, uvdd.zw);
	half4 sampzp = tex2D(texZp, uv, uvdd.xy, uvdd.zw);

	half4 sampx = lerp(sampxn, sampxp, step(0, s));
	half4 sampy = lerp(sampyn, sampyp, step(0, s));
	half4 sampz = lerp(sampzn, sampzp, step(0, s));

	half4 samp = lerp(sampx, sampz, zxlerp);
	samp = lerp(sampy, samp, nylerp);
	return samp;
}

inline half4 GetCubeMapNoLOD(sampler2D texSamplerPos, sampler2D texSamplerNeg, float3 cubeVect)
{

	float3 cubeVectNorm = normalize(cubeVect);
	float3 cubeVectNormAbs = abs(cubeVectNorm);

	float4 uv;
	uv.zw = float2(0, 0);

	half zxlerp = step(cubeVectNormAbs.x, cubeVectNormAbs.z);
	half nylerp = step(cubeVectNormAbs.y, max(cubeVectNormAbs.x, cubeVectNormAbs.z));

	half s = lerp(cubeVectNorm.x, cubeVectNorm.z, zxlerp);
	s = sign(lerp(cubeVectNorm.y, s, nylerp));

	half3 detailCoords = lerp(half3(1, -s, -1)*cubeVectNorm.xzy, half3(1, s, -1)*cubeVectNorm.zxy, zxlerp);
	detailCoords = lerp(half3(1, 1, s)*cubeVectNorm.yxz, detailCoords, nylerp);

	uv.xy = ((.5*detailCoords.yz) / abs(detailCoords.x)) + .5;

	half4 texPos = tex2Dlod(texSamplerPos, uv);
	half4 texNeg = tex2Dlod(texSamplerNeg, uv);

	half4 tex = lerp(texNeg, texPos, step(0, s));

	half alpha = lerp(tex.r, tex.b, zxlerp);
	alpha = lerp(tex.g, alpha, nylerp);
	return half4(tex.a, tex.a, tex.a, alpha);

}

inline half4 GetCubeMap(sampler2D texSamplerPos, sampler2D texSamplerNeg, float3 cubeVect)
{

	float3 cubeVectNorm = normalize(cubeVect);
	float3 cubeVectNormAbs = abs(cubeVectNorm);


	half zxlerp = step(cubeVectNormAbs.x, cubeVectNormAbs.z);
	half nylerp = step(cubeVectNormAbs.y, lerp(cubeVectNormAbs.x, cubeVectNormAbs.z, zxlerp));

	half s = lerp(cubeVectNorm.x, cubeVectNorm.z, zxlerp);
	s = sign(lerp(-cubeVectNorm.y, s, nylerp));

	half3 detailCoords = lerp(half3(1, -s, -1)*cubeVectNorm.xzy, half3(1, s, -1)*cubeVectNorm.zxy, zxlerp);
	detailCoords = lerp(half3(1, 1, s)*cubeVectNorm.yxz, detailCoords, nylerp);

	float2 uv = ((.5*detailCoords.yz) / abs(detailCoords.x)) + .5;

	float4 uvdd = CubeDerivatives(uv, 1);

	half4 texPos = tex2D(texSamplerPos, uv, uvdd.xy, uvdd.zw);
	half4 texNeg = tex2D(texSamplerNeg, uv, uvdd.xy, uvdd.zw);

	half4 tex = lerp(texNeg, texPos, step(0, s));

	half alpha = lerp(tex.r, tex.b, zxlerp);
	alpha = lerp(tex.g, alpha, nylerp);
	return half4(tex.a, tex.a, tex.a, alpha);

}

inline half4 GetCubeDetailMapNoLOD(sampler2D texSampler, float3 cubeVect, float detailScale)
{
	float3 cubeVectNorm = normalize(cubeVect);
	float4 uv;
	uv.xy = GetCubeCubeUV(cubeVectNorm)*detailScale;
	uv.zw = float2(0, 0);
	half4 tex = tex2Dlod(texSampler, uv);
	return tex;
}

inline half4 GetCubeDetailMap(sampler2D texSampler, float3 cubeVect, float detailScale)
{
	float3 cubeVectNorm = normalize(cubeVect);
	float2 uv = GetCubeCubeUV(cubeVectNorm)*detailScale;
	float4 uvdd = CubeDerivatives(uv, detailScale);
	half4 tex = tex2D(texSampler, uv, uvdd.xy, uvdd.zw);
	return 	tex;
}


#endif