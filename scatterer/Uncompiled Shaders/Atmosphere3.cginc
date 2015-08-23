
//
//  Precomputed Atmospheric Scattering
//  Copyright (c) 2008 INRIA
//  All rights reserved.
// 
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions
//  are met:
//  1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
//  2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
//  3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//  AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
//  ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
//  LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//  SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
//  INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//  CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
//  THE POSSIBILITY OF SUCH DAMAGE.
// 
//  Author: Eric Bruneton
//  Modified and ported to Unity by Justin Hawkins 2013
//

uniform sampler2D _Transmittance;
uniform sampler2D _Inscatter;
uniform sampler2D _Irradiance;

uniform float TRANSMITTANCE_W;
uniform float TRANSMITTANCE_H;

uniform float SKY_W;
uniform float SKY_H;

uniform float M_PI;
uniform float3 EARTH_POS;
uniform float SCALE;
uniform float Rg;
uniform float Rt;
uniform float RL;
uniform float RES_R;
uniform float RES_MU;
uniform float RES_MU_S;
uniform float RES_NU;

uniform float3 SUN_DIR;

//uniform float terrain_reflectance;

uniform float SUN_INTENSITY;
uniform float3 betaR;
uniform float mieG;

//float3 hdr(float3 L) 
//{
//    L = L * 0.4;
//    L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
//    L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
//    L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
//    return L;
//}
 
 float4 Texture4D(sampler2D table, float r, float mu, float muS, float nu)
{
    float H = sqrt(Rt * Rt - Rg * Rg);
    float rho = sqrt(r * r - Rg * Rg);    
    

//#ifdef INSCATTER_NON_LINEAR
    float rmu = r * mu;
    float delta = rmu * rmu - r * r + Rg * Rg;
    float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / RES_MU) : float4(-1.0, H * H, H, 0.5 + 0.5 / RES_MU);
    float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / float(RES_R));
    float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / RES_MU);
    
    // paper formula
    //float uMuS = 0.5 / float(RES_MU_S) + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / float(RES_MU_S));
    // better formula
    
   float uMuS = 0.5 / RES_MU_S + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / RES_MU_S);
//#else
//     uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / RES_R);
//     uMu = 0.5 / RES_MU + (mu + 1.0) / 2.0 * (1.0 - 1.0 / RES_MU);
//     uMuS = 0.5 / RES_MU_S + max(muS + 0.2, 0.0) / 1.2 * (1.0 - 1.0 / RES_MU_S);
//#endif
    float _lerp = (nu + 1.0) / 2.0 * (RES_NU - 1.0);
    float uNu = floor(_lerp);
    _lerp = _lerp - uNu;
    
    //original 3D lookup    
    //return tex3Dlod(table, float4((uNu + uMuS) / RES_NU, uMu, uR, 0)) * (1.0 - _lerp) + tex3Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu, uR, 0)) * _lerp;    
    //new 2D lookup
    
    float u_0 = floor(uR*RES_R-1)/(RES_R);
	float u_1 = floor(uR*RES_R)/(RES_R);
			
	float u_frac = frac(uR*RES_R);
				
//#if !defined(SHADER_API_OPENGL)	
	float4 A = tex2Dlod(table, float4((uNu + uMuS) / RES_NU, uMu / RES_R + u_0,0.0,0.0)) * (1.0 - _lerp) + tex2Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_0,0.0,0.0)) * _lerp;
	float4 B = tex2Dlod(table, float4((uNu + uMuS) / RES_NU, uMu / RES_R + u_1,0.0,0.0)) * (1.0 - _lerp) + tex2Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_1,0.0,0.0)) * _lerp;							

//#else	
//	float4 A = tex2D(table, float2((uNu + uMuS) / RES_NU, uMu / RES_R + u_0)) * (1.0 - _lerp) + tex2D(table, float2((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_0)) * _lerp;
//	float4 B = tex2D(table, float2((uNu + uMuS) / RES_NU, uMu / RES_R + u_1)) * (1.0 - _lerp) + tex2D(table, float2((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_1)) * _lerp;
//	
//#endif
	
	return (A * (1.0-u_frac) + B * u_frac);
//	return 0;
}
 
 
//float4 Texture4D(sampler3D table, float r, float mu, float muS, float nu)
//{
//   	float H = sqrt(Rt * Rt - Rg * Rg);
//   	float rho = sqrt(r * r - Rg * Rg);
//
//    float rmu = r * mu;
//    float delta = rmu * rmu - r * r + Rg * Rg;
//    float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / RES_MU) : float4(-1.0, H * H, H, 0.5 + 0.5 / RES_MU);
//    float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / RES_R);
//    float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / float(RES_MU));
//    // paper formula
//    //float uMuS = 0.5 / RES_MU_S + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / RES_MU_S);
//    // better formula
//    float uMuS = 0.5 / RES_MU_S + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / RES_MU_S);
//
//    float lep = (nu + 1.0) / 2.0 * (RES_NU - 1.0);
//    float uNu = floor(lep);
//    lep = lep - uNu;
//
//    return tex3D(table, float3((uNu + uMuS) / RES_NU, uMu, uR)) * (1.0 - lep) + tex3D(table, float3((uNu + uMuS + 1.0) / RES_NU, uMu, uR)) * lep;
//}

float3 GetMie(float4 rayMie) 
{	
	// approximated single Mie scattering (cf. approximate Cm in paragraph "Angular precision")
	// rayMie.rgb=C*, rayMie.w=Cm,r
   	return rayMie.rgb * rayMie.w / max(rayMie.r, 1e-4) * (betaR.r / betaR);
}

float PhaseFunctionR(float mu) 
{
	// Rayleigh phase function
    return (3.0 / (16.0 * M_PI)) * (1.0 + mu * mu);
}

float PhaseFunctionM(float mu) 
{
	// Mie phase function
   	 return 1.5 * 1.0 / (4.0 * M_PI) * (1.0 - mieG*mieG) * pow(1.0 + (mieG*mieG) - 2.0*mieG*mu, -3.0/2.0) * (1.0 + mu * mu) / (2.0 + mieG*mieG);
}

float3 Transmittance(float r, float mu) 
{
	// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
	// (mu=cos(view zenith angle)), intersections with ground ignored
   	float uR, uMu;
    uR = sqrt((r - Rg) / (Rt - Rg));
    uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
//#if !defined(SHADER_API_OPENGL)	
    return tex2Dlod (_Transmittance, float4(uMu, uR,0.0,0.0)).rgb;
//#else
//	return tex2D (_Transmittance, float2(uMu, uR)).rgb;
//#endif	
}

float3 SkyRadiance(float3 camera, float3 viewdir, out float3 extinction)
{
	// scattered sunlight between two points
	// camera=observer
	// viewdir=unit vector towards observed point
	// sundir=unit vector towards the sun
	// return scattered light

	camera += EARTH_POS;

   	float3 result = float3(0,0,0);
   	extinction = float3(1,1,1);
   	
    float r = length(camera);
    float rMu = dot(camera, viewdir);
    float mu = rMu / r;
    float r0 = r;
    float mu0 = mu;

    float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
    float din = max(-rMu - deltaSq, 0.0);
    if (din > 0.0) 
    {
       	camera += din * viewdir;
       	rMu += din;
       	mu = rMu / Rt;
       	r = Rt;
    }
    
    float nu = dot(viewdir, SUN_DIR);
    float muS = dot(camera, SUN_DIR) / r;

    float4 inScatter = Texture4D(_Inscatter, r, rMu / r, muS, nu);
    extinction = Transmittance(r, mu);

    if(r <= Rt) 
    {
        float3 inScatterM = GetMie(inScatter);
        float phase = PhaseFunctionR(nu);
        float phaseM = PhaseFunctionM(nu);
        result = inScatter.rgb * phase + inScatterM * phaseM;
    }

    return result * SUN_INTENSITY;
}

//float ReflectedSunRadiance(float3 L, float3 V, float3 N, float sigmaSq) 
//{
//    float3 H = normalize(L + V);
//
//    float hn = dot(H, N);
//    float p = exp(-2.0 * ((1.0 - hn * hn) / sigmaSq) / (1.0 + hn)) / (4.0 * M_PI * sigmaSq);
//
//    float c = 1.0 - dot(V, H);
//    float c2 = c * c;
//    float fresnel = 0.02 + 0.98 * c2 * c2 * c;
//
//    float zL = dot(L, N);
//    float zV = dot(V, N);
//    zL = max(zL,0.01);
//    zV = max(zV,0.01);
//
//    // brdf times cos(thetaL)
//    return zL <= 0.0 ? 0.0 : max(fresnel * p * sqrt(abs(zL / zV)), 0.0);
//}
//
//float MeanFresnel(float cosThetaV, float sigmaV) {
//    return pow(1.0 - cosThetaV, 5.0 * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(sigmaV, 1.5));
//}
//
//float MeanFresnel(float3 V, float3 N, float sigmaSq) {
//    return MeanFresnel(dot(V, N), sqrt(sigmaSq));
//}
//
//float3 OceanRadiance(float3 L, float3 V, float3 N, float sigmaSq, float3 sunL, float3 skyE, float3 seaColor) 
//{
//    float F = MeanFresnel(V, N, sigmaSq);
//    float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
//    float3 Lsky = skyE * F / M_PI;
//    float3 Lsea = (1.0 - F) * seaColor * skyE / M_PI;
//    return Lsun + Lsky + Lsea;
//}


float2 GetIrradianceUV(float r, float muS) 
{
    float uR = (r - Rg) / (Rt - Rg);
    float uMuS = (muS + 0.2) / (1.0 + 0.2);
    return float2(uMuS, uR);
}

void GetIrradianceRMuS(float2 coord, out float r, out float muS) 
{
    r = Rg + (coord.y - 0.5) / (SKY_H - 1.0) * (Rt - Rg);
    muS = -0.2 + (coord.x - 0.5) / (SKY_W - 1.0) * (1.0 + 0.2);
}

float3 Irradiance(sampler2D samp, float r, float muS) 
{
    float2 uv = GetIrradianceUV(r, muS);
//#if !defined(SHADER_API_OPENGL)
    return tex2Dlod(samp, float4(uv,0,0)).rgb;
//#else
//	return tex2D(samp, uv).rgb;
//#endif	
}

// input - surfacePos: reconstructed position of current pixel 
// input - texC: texture coordinates 
// input - attenuation: extinction factor along view path 
// input - irradianceFactor: surface hit within atmosphere 1.0f 
// otherwise 0.0f 
// output - return value: total reflected light + direct sunlight 
//float3 GetReflectedLight( in float3 surfacePos, in float2 texC, in float3 attenuation, in float irradianceFactor)

//float3 GetReflectedLight( float3 surfacePos, float3 surfaceColor , float3 attenuation, float irradianceFactor, float3 normalValues, float4x4 camToWorld)
//{
////	float3 surfaceColor = col.rgb;
//		
////	float3 normal = 2.0f * normalValues - 1.0f;
//	float3 normal= mul((float3x3)camToWorld ,normalValues); //try this one if the other gives weird results
//	
////	normal.r= normalValues.r;
////	normal.g= normalValues.g;
////	normal.b= normalValues.b;
//	
//	float lightIntensity = SUN_INTENSITY * terrain_reflectance;
//	
//
//	float lightScale = max(dot(normal, SUN_DIR), 0.0f);
//	// irradiance at surface position due to sky light
//	
//	float surfacePosHeight = length(surfacePos);
//	float musSurfacePos = dot(surfacePos, SUN_DIR) / surfacePosHeight;
//
//	float3 irradianceSurface = Irradiance(_Irradiance, surfacePosHeight, musSurfacePos) * irradianceFactor;
//	// attenuate direct sun light on its path from top of atmosphere to
//	// surface position 
//
//	float3 attenuationSunLight = Transmittance(surfacePosHeight, musSurfacePos);
//	//float3 reflectedLight = surfaceColor * (lightScale * attenuationSunLight + irradianceSurface) * lightIntensity;
//	float3 reflectedLight = float3(0.5,0.5,0.5) * (lightScale * attenuationSunLight + irradianceSurface) * lightIntensity;
//	// attenuate again on path from surface position to camera
//	reflectedLight *= attenuation;
//	return reflectedLight;
//}
//
//float3 TransmittanceWithShadow(float r, float mu) 
//{
//    return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? float3(0,0,0) : Transmittance(r, mu);
//}
//
//float3 SunRadiance(float r, float muS) {
//    return TransmittanceWithShadow(r, muS) * SUN_INTENSITY;
//}
//
//void SunRadianceAndSkyIrradiance(float3 worldP, float3 fn, float3 worldS, out float3 sunL, out float3 skyE, float scale)
//{
//	worldP *= scale;
//    float r = length(worldP);
//    if (r < 0.9 * Rg) {
//        worldP.z += Rg;
//        r = length(worldP);
//    }
//    float3 worldV = worldP / r; // vertical vector
//    float muS = dot(worldV, worldS);
//
//    float sunOcclusion = 1.0;// - sunShadow;
//    sunL = SunRadiance(r, muS) * sunOcclusion;
////    
////    float3 worldNormal= mul((float3x3)camToWorld ,normalValues);
//    
////    fn.z = sqrt(max(0.0, 1.0 - dot(fn.xy, fn.xy)));
//	
//    // ambient occlusion due only to slope, does not take self shadowing into account
////    float skyOcclusion = (1.0 + dot(worldV, worldN)) * 0.5;
//	float skyOcclusion = (1.0 + dot(worldV, fn)) * 0.5;
//    // factor 2.0 : hack to increase sky contribution (numerical simulation of
//    // "precompued atmospheric scattering" gives less luminance than in reality)
////    skyE = 2.0 * SkyIrradiance(r, muS) * skyOcclusion;
//    skyE = 2.0 * Irradiance(_Irradiance,r, muS) * SUN_INTENSITY * skyOcclusion;
//}
	
	
//float3 GetReflectedLight( in float3 surfacePos, in float2 texC, in float3 attenuation, in float irradianceFactor)
//
//{
//// read contents of GBuffer
//	float4 normalData = g_normal.SampleLevel(PointSamplerClamp, texC, 0);
//
//	// decode normal and determine intensity of refected light at
//	// surface postiion
//	
////	float3 normal = 2.0f * normalData.xyz - 1.0f;
//	float3 normal = 2.0f * NormalValues.xyz - 1.0f;
////	float3 normal = NormalValues.xyz; //try this one if the other gives weird results
//	
//	
////	float lightIntensity = SUN_INTENSITY * normalData.w;
//	float lightIntensity = SUN_INTENSITY * terrain_reflectance;
//	
////	float lightScale = max(dot(normal, g_sunVector), 0.0f);
//	float lightScale = max(dot(normal, SUN_DIR), 0.0f);
//	// irradiance at surface position due to sky light
//	
//	float surfacePosHeight = length(surfacePos);
////	float musSurfacePos = dot(surfacePos, g_sunVector) / surfacePosHeight;
//	float musSurfacePos = dot(surfacePos, SUN_DIR) / surfacePosHeight;
//
//	float3 irradianceSurface = irradiance(g_texIrradiance, surfacePosHeight, musSurfacePos) * irradianceFactor;
//	// attenuate direct sun light on its path from top of atmosphere to
//	// surface position 
//
//	float3 attenuationSunLight = transmittance(surfacePosHeight, musSurfacePos);
//	float3 reflectedLight = surfaceColor * (lightScale * attenuationSunLight + irradianceSurface) * lightIntensity;
//	// attenuate again on path from surface position to camera
//	reflectedLight *= attenuation;
//	return reflectedLight;
//}



float3 InScattering(float3 camera, float3 _point, out float3 extinction, float shaftWidth, float scaleCoeff, float irradianceFactor) 
{
//#if !defined(SHADER_API_OPENGL)	
////directx code
	// single scattered sunlight between two points
	// camera=observer
	// point=point on the ground
	// sundir=unit vector towards the sun
	// return scattered light and extinction coefficient

    float3 result = float3(0,0,0);
    extinction = float3(1,1,1);
        
    float3 viewdir = _point - camera;
    float d = length(viewdir)* scaleCoeff;
    viewdir = viewdir / d;
    d=d;
    float r = length(camera)* scaleCoeff;
        
    if (r < 0.9 * Rg) 
    {
        camera.y += Rg;
        _point.y += Rg;
        r = length(camera)* scaleCoeff;
    }
    float rMu = dot(camera, viewdir);
    float mu = rMu / r;
    float r0 = r;
    float mu0 = mu;
    _point -= viewdir * clamp(shaftWidth, 0.0, d);

    float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
    float din = max(-rMu - deltaSq, 0.0);
    
    if (din > 0.0 && din < d) 
    {
        camera += din * viewdir;
        rMu += din;
        mu = rMu / Rt;
        r = Rt;
        d -= din;
    }

    if (r <= Rt) 
    {
        float nu = dot(viewdir, SUN_DIR);
        float muS = dot(camera, SUN_DIR) / r;

        float4 inScatter;

        if (r < Rg + 600.0) 
        {
            // avoids imprecision problems in aerial perspective near ground
            float f = (Rg + 600.0) / r;
            r = r * f;
            rMu = rMu * f;
            _point = _point * f;
        }

        float r1 = length(_point);
        float rMu1 = dot(_point, viewdir);
        float mu1 = rMu1 / r1;
        float muS1 = dot(_point, SUN_DIR) / r1;

        if (mu > 0.0) {
          extinction = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0);
            }
        else {
            extinction = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0);}

        const float EPS = 0.004;
        float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
        
        if (abs(mu - lim) < EPS) 
        {
            float a = ((mu - lim) + EPS) / (2.0 * EPS);

            mu = lim - EPS;
            r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
            mu1 = (r * mu + d) / r1;
            
            float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
            float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
            float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//            float4 inScatterA=(0.0,0.0,0.0,0.0);
//            float4 inScatterA=Texture4D(_Inscatter, r, mu, muS, nu);
//			float4 inScatterA=Texture4D(_Inscatter, 0, 0, 0, 0);

            mu = lim + EPS;
            r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
            mu1 = (r * mu + d) / r1;
            
            inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
            inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
            float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//            float4 inScatterB=(0.0,0.0,0.0,0.0);

            inScatter = lerp(inScatterA, inScatterB, a);
            
            irradianceFactor=1.0;
            //Not sure about where irradianceFactor goes
        } 
        else 
        {
            float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
            float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
            inScatter = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
        }

        // avoids imprecision problems in Mie scattering when sun is below horizon
        inScatter.w *= smoothstep(0.00, 0.02, muS);

        float3 inScatterM = GetMie(inScatter);
        float phase = PhaseFunctionR(nu);
        float phaseM = PhaseFunctionM(nu);
        result = inScatter.rgb * phase + inScatterM * phaseM;
    } 

    return result * SUN_INTENSITY;






//#else	
////opengl code
//    float3 result = float3(0,0,0);
//    extinction = float3(1,1,1);
//        
//    float3 viewdir = _point - camera;
//    float d = length(viewdir)* scaleCoeff;
//    viewdir = viewdir / d;
//    d=d;
//    float r = length(camera)* scaleCoeff;
//        
//    if (r < 0.9 * Rg) 
//    {
//        camera.y += Rg;
//        _point.y += Rg;
//        r = length(camera)* scaleCoeff;
//    }
//    float rMu = dot(camera, viewdir);
//    float mu = rMu / r;
//    float r0 = r;
//    float mu0 = mu;
//    _point -= viewdir * clamp(shaftWidth, 0.0, d);
//
//    float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
//    float din = max(-rMu - deltaSq, 0.0);
//    
//    if (din > 0.0 && din < d) 
//    {
//        camera += din * viewdir;
//        rMu += din;
//        mu = rMu / Rt;
//        r = Rt;
//        d -= din;
//    }
//
//    if (!(r <= Rt)){return result * SUN_INTENSITY;}
//    
//    
//        float nu = dot(viewdir, SUN_DIR);
//        float muS = dot(camera, SUN_DIR) / r;
//
//        float4 inScatter;
//
//        if (r < Rg + 600.0) 
//        {
//            // avoids imprecision problems in aerial perspective near ground
//            float f = (Rg + 600.0) / r;
//            r = r * f;
//            rMu = rMu * f;
//            _point = _point * f;
//        }
//
//        float r1 = length(_point);
//        float rMu1 = dot(_point, viewdir);
//        float mu1 = rMu1 / r1;
//        float muS1 = dot(_point, SUN_DIR) / r1;
//
//        if (mu > 0.0) {
//          extinction = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0);
//            }
//        else {
//            extinction = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0);}
//
//        const float EPS = 0.004;
//        float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
//        
//        float choice;
//        
//        if (abs(mu - lim) < EPS) {choice=1.0;}
//        else{choice=0.0;}
//        
//        
////choice=1        
//        {
//            float a = ((mu - lim) + EPS) / (2.0 * EPS);
//
//            mu = lim - EPS;
//            r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
//            mu1 = (r * mu + d) / r1;
//            
//            float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
////            float4 inScatterA=(0.0,0.0,0.0,0.0);
////            float4 inScatterA=Texture4D(_Inscatter, r, mu, muS, nu);
////			float4 inScatterA=Texture4D(_Inscatter, 0, 0, 0, 0);
//
//            mu = lim + EPS;
//            r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
//            mu1 = (r * mu + d) / r1;
//            
//            inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
////            float4 inScatterB=(0.0,0.0,0.0,0.0);
//
////            inscatterChoice1 = lerp(inScatterA, inScatterB, a);
//          	inScatter = choice * lerp(inScatterA, inScatterB, a);
//        }                 
////        else 
//
////choice=0        
//		{
//            float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
////            inscatterChoice2 =  (max(inScatter0 - inScatter1 * extinction.rgbr, 0.0));
//          inScatter = inScatter + (1.0-choice) * (max(inScatter0 - inScatter1 * extinction.rgbr, 0.0));
//        }
//        
//
//        // avoids imprecision problems in Mie scattering when sun is below horizon
//        //inScatter.w *= smoothstep(0.00, 0.02, muS);
//
//        float3 inScatterM = GetMie(inScatter);
//        float phase = PhaseFunctionR(nu);
//        float phaseM = PhaseFunctionM(nu);
//        result = inScatter.rgb * phase + inScatterM * phaseM;
//     
//
//    return result * SUN_INTENSITY;
//
//#endif
}