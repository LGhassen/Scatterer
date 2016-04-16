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


//            #define useHorizonHack
#define useAnalyticTransmittance

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

// Rayleigh
uniform float HR;
uniform float3 betaR;

// Mie
uniform float HM;
uniform float3 betaMSca;
uniform float3 betaMEx;
uniform float mieG;

uniform float _Exposure;
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
uniform float _Sun_Intensity;

uniform float _experimentalAtmoScale;

uniform float _viewdirOffset;

float3 hdr(float3 L) {
    L = L * _Exposure;
    L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
    L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
    L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
	return L;
}

float4 Texture4D(sampler2D table, float r, float mu, float muS, float nu)
{
    float H = sqrt(Rt * Rt - Rg * Rg);
    float rho = sqrt(r * r - Rg * Rg);
    float rmu = r * mu;
    float delta = rmu * rmu - r * r + Rg * Rg;
    float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / RES_MU) : float4(-1.0, H * H, H, 0.5 + 0.5 / RES_MU);
    float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / float(RES_R));
    float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / RES_MU);
    // paper formula
    //    float uMuS = 0.5 / float(RES_MU_S) + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / float(RES_MU_S));
    // better formula
    float uMuS = 0.5 / RES_MU_S + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / RES_MU_S);
    float _lerp = (nu + 1.0) / 2.0 * (RES_NU - 1.0);
    float uNu = floor(_lerp);
    _lerp = _lerp - uNu;
    //original 3D lookup
    //return tex3Dlod(table, float4((uNu + uMuS) / RES_NU, uMu, uR, 0)) * (1.0 - _lerp) + tex3Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu, uR, 0)) * _lerp;
    //new 2D lookup
    float u_0 = floor(uR*RES_R-1)/(RES_R);
    float u_1 = floor(uR*RES_R)/(RES_R);
    float u_frac = frac(uR*RES_R);
    float4 A = tex2Dlod(table, float4((uNu + uMuS) / RES_NU, uMu / RES_R + u_0,0.0,0.0)) * (1.0 - _lerp) + tex2Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_0,0.0,0.0)) * _lerp;
    float4 B = tex2Dlod(table, float4((uNu + uMuS) / RES_NU, uMu / RES_R + u_1,0.0,0.0)) * (1.0 - _lerp) + tex2Dlod(table, float4((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_1,0.0,0.0)) * _lerp;
    return (A * (1.0-u_frac) + B * u_frac);
}

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
    //    return tex2D (_Transmittance, float2(uMu, uR)).rgb;
    //#endif
}

float3 Transmittance(float r, float mu, float Rt0)
{
    // transmittance(=transparency) of atmosphere for infinite ray (r,mu)
    // (mu=cos(view zenith angle)), intersections with ground ignored
    float uR, uMu;
    uR = sqrt((r - Rg) / (Rt0 - Rg));
    uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
    //#if !defined(SHADER_API_OPENGL)
    return tex2Dlod (_Transmittance, float4(uMu, uR,0.0,0.0)).rgb;
    //#else
    //    return tex2D (_Transmittance, float2(uMu, uR)).rgb;
    //#endif
}


//fixes artifacts in d3d9
float SQRT(float f, float err) {
    //#if !defined(SHADER_API_D3D9)
    //                return sqrt(f);
    //#else
    return f >= 0.0 ? sqrt(f) : err;
    //#endif
}


//Source:   wikibooks.org/wiki/GLSL_Programming/Unity/Soft_Shadows_of_Spheres
//I believe space engine also uses the same approach because the eclipses look the same ;)
float getEclipseShadow(float3 worldPos, float3 worldLightPos,float3 occluderSpherePosition,
					   float3 occluderSphereRadius, float3 lightSourceRadius)		
{											
	float3 lightDirection = float3(worldLightPos - worldPos);
	float3 lightDistance = length(lightDirection);
	lightDirection = lightDirection / lightDistance;
               
	// computation of level of shadowing w  
	float3 sphereDirection = float3(occluderSpherePosition - worldPos);  //occluder planet
	float sphereDistance = length(sphereDirection);
	sphereDirection = sphereDirection / sphereDistance;
            		
	float dd = lightDistance * (asin(min(1.0, length(cross(lightDirection, sphereDirection)))) 
			   - asin(min(1.0, occluderSphereRadius / sphereDistance)));
            
	float w = smoothstep(-1.0, 1.0, -dd / lightSourceRadius);
	w = w * smoothstep(0.0, 0.2, dot(lightDirection, sphereDirection));
            		
	return (1-w);
}

//stole this from basic GLSL raytracing shader somewhere on the net
//a quick google search and you'll find it
//float intersectSphere2(float3 p1, float3 p2, float3 p3, float r) {
float intersectSphere2(float3 p1, float3 d, float3 p3, float r) {
    // The line passes through p1 and p2:
    // p3 is the sphere center
	//float3 d = p2 - p1;
    float a = dot(d, d);
    float b = 2.0 * dot(d, p1 - p3);
    float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
    float test = b * b - 4.0 * a * c;
//    if (test < 0)
//    {
//        return -1.0;
//    }
//    float u = (-b - sqrt(test)) / (2.0 * a);
    float u = (test < 0) ? -1.0 : (-b - sqrt(test)) / (2.0 * a);
    //                      float3 hitp = p1 + u * (p2 - p1);            //we'll just do this later instead if needed
    //                      return(hitp);
    return u;
}


//for eclipses
//works from inside sphere
float intersectSphere4(float3 p1, float3 d, float3 p3, float r)
{
	// p1 starting point
	// d look direction
	// p3 is the sphere center

	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r*r;

	float test = b*b - 4.0*a*c;

	if (test<0)
	{
		return -1.0;
	}
	
  	float u = (-b - sqrt(test)) / (2.0 * a);
  		
	//eclipse compatbility for inside the atmosphere
//  		if (u<0)
//  		{
//  			u = (-b + sqrt(test)) / (2.0 * a);
//  		}

  	u = (u < 0) ? (-b + sqrt(test)) / (2.0 * a) : u;
  			
	return u;
}


//Can't get this simpler version to work, will re-try it later
//float intSphere( float4 sp, float3 ro, float3 rd, float tm) //sp.xyz sphere Pos? sp.w sphere rad?, r0 ray origin rd ray direction, tm?
float intSphere( float3 ro, float3 rd, float4 sp) //sp.xyz sphere Pos? sp.w sphere rad?, r0 ray origin rd ray direction, tm?
{
    float3 d = ro - sp.xyz;
    float b = dot(rd,d);
    float c = dot(d,d) - sp.w*sp.w;
    float t = b*b-c;
    t = ( t > 0.0 ) ? -b-sqrt(t) : t;
    
//    if( t > 0.0 )
//    {
//        t = -b-sqrt(t);
////        r = (t > 0.0) && (t < tm);
//    }

    return t;
}

//Can't get this simpler version to work, will re-try it later
float sphere(float3 ray, float3 dir, float3 center, float radius)
{
 float3 rc = ray-center;
 float c = dot(rc, rc) - (radius*radius);
 float b = dot(dir, rc);
 float d = b*b - c;
 float t = -b - sqrt(abs(d));
 float st = step(0.0, min(t,d)); //if min(t,d) >= 0 return 1 else return 0
 return lerp(-1.0, t, st);
}


// optical depth for ray (r,mu) of length d, using analytic formula
// (mu=cos(view zenith angle)), intersections with ground ignored
// H=height scale of exponential density function
float OpticalDepth(float H, float r, float mu, float d)
{
    float a = sqrt((0.5/H)*r);
    float2 a01 = a*float2(mu, mu + d / r);
    float2 a01s = sign(a01);
    float2 a01sq = a01*a01;
    float x = a01s.y > a01s.x ? exp(a01sq.x) : 0.0;
    float2 y = a01s / (2.3193*abs(a01) + sqrt(1.52*a01sq + 4.0)) * float2(1.0, exp(-d/H*(d/(2.0*r)+mu)));
    return sqrt((6.2831*H)*r) * exp((Rg-r)/H) * (x + dot(y, float2(1.0, -1.0)));
}


// transmittance(=transparency) of atmosphere for ray (r,mu) of length d
// (mu=cos(view zenith angle)), intersections with ground ignored
// uses analytic formula instead of transmittance texture
float3 AnalyticTransmittance(float r, float mu, float d)
{
    return exp(- betaR * OpticalDepth(HR * _experimentalAtmoScale, r, mu, d) - betaMEx * OpticalDepth(HM * _experimentalAtmoScale, r, mu, d));
}

//the extinction part extracted from the inscattering function
//this is for objects in atmo, computed using analyticTransmittance (better precision and less artifacts) or the precomputed transmittance table
float3 getExtinction(float3 camera, float3 _point, float shaftWidth, float scaleCoeff, float irradianceFactor)
{
    float3 extinction = float3(1, 1, 1);
    float3 viewdir = _point - camera;
    float d = length(viewdir) * scaleCoeff;
    viewdir = viewdir / d;
    /////////////////////experimental block begin
    float Rt0=Rt;
    Rt = Rg + (Rt - Rg) * _experimentalAtmoScale;
    //                viewdir.x += _viewdirOffset;
    viewdir = normalize(viewdir);
    /////////////////////experimental block end
    float r = length(camera) * scaleCoeff;
    
    if (r < 0.9 * Rg) {
        camera.y += Rg;
        r = length(camera) * scaleCoeff;
    }
    
    float rMu = dot(camera, viewdir);
    float mu = rMu / r;

    float deltaSq = SQRT(rMu * rMu - r * r + Rt * Rt, 0.000001);
//    float deltaSq = sqrt(rMu * rMu - r * r + Rt * Rt);
    
    float din = max(-rMu - deltaSq, 0.0);
    
    if (din > 0.0 && din < d)
    {
        rMu += din;
        mu = rMu / Rt;
        r = Rt;
        d -= din;
    }
	if (r <= Rt)
    { 
    	if (r < Rg + 1600.0)
    	{
    		// avoids imprecision problems in aerial perspective near ground
    		//Not sure if necessary with extinction
        	float f = (Rg + 1600.0) / r;
        	r = r * f;
    	}
        
    	//set to analyticTransmittance only atm
    	#if defined (useAnalyticTransmittance)
    	extinction = min(AnalyticTransmittance(r, mu, d), 1.0);
    	#endif
    }	
	else
    {	//if out of atmosphere
        extinction = float3(1,1,1);
    }

    return extinction;
}

//Extinction for a ray going all the way to the end of the atmosphere
//i.e an infinite ray
//for clouds so no analyticTransmittance required
			float3 getSkyExtinction(float3 camera, float3 viewdir) //instead of camera this is the cloud position
			{
				float3 extinction = float3(1,1,1);

				Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;		//not really noticeable

				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;

    			float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,0.000001);
//    			float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);

    			float din = max(-rMu - deltaSq, 0.0);
    			if (din > 0.0)
    			{
        			camera += din * viewdir;
        			rMu += din;
        			mu = rMu / Rt;
        			r = Rt;
    			}

//    			extinction = Transmittance(r, mu);
//
//    			if (r > Rt) 
//    			{
//    				extinction = float3(1,1,1);
//    			} 


    			extinction = (r > Rt) ? float3(1,1,1) : Transmittance(r, mu);

    			return extinction;
    		}
    		
    		float3 sunsetExtinction(float3 camera)
    		{
    			return(getSkyExtinction(camera,SUN_DIR));
    		}
    		

float3 SkyRadiance2(float3 camera, float3 viewdir, float3 sundir, out float3 extinction)//, float shaftWidth)
{
	extinction = float3(1,1,1);
	float3 result = float3(0,0,0);
	
	float Rt2=Rt;
	Rt=Rg+(Rt-Rg)*_experimentalAtmoScale;
	
	
	viewdir.x+=_viewdirOffset;
	viewdir=normalize(viewdir);

	//camera *= scale;
	//camera += viewdir * max(shaftWidth, 0.0);
	float r = length(camera);
	float rMu = dot(camera, viewdir);
	float mu = rMu / r;
	float r0 = r;
	float mu0 = mu;
	
	float deltaSq = SQRT(rMu * rMu - r * r + Rt*Rt,0.000001);

	float din = max(-rMu - deltaSq, 0.0);
	if (din > 0.0)
	{
    	camera += din * viewdir;
    	rMu += din;
    	mu = rMu / Rt;
    	r = Rt;
	}
	
	float nu = dot(viewdir, sundir);
	float muS = dot(camera, sundir) / r;
    
//	float4 inScatter = Texture4D(_Sky_Inscatter, r, rMu / r, muS, nu);
	float4 inScatter = Texture4D(_Inscatter, r, rMu / r, muS, nu);
    
	extinction = Transmittance(r, mu);
    
	if (r <= Rt) 
	{
            
//        if (shaftWidth > 0.0) 
//        {
//            if (mu > 0.0) {
//                inScatter *= min(Transmittance(r0, mu0) / Transmittance(r, mu), 1.0).rgbr;
//            } else {
//                inScatter *= min(Transmittance(r, -mu) / Transmittance(r0, -mu0), 1.0).rgbr;
//            }
//        }

    	float3 inScatterM = GetMie(inScatter);
    	float phase = PhaseFunctionR(nu);
    	float phaseM = PhaseFunctionM(nu);
    	result = inScatter.rgb * phase + inScatterM * phaseM;
	}    
     else
	{
		result = float3(0,0,0);
		extinction = float3(1,1,1);
	} 
	
	return result * _Sun_Intensity;
}

float2 GetIrradianceUV(float r, float muS) 
{
    float uR = (r - Rg) / (Rt - Rg);
    float uMuS = (muS + 0.2) / (1.0 + 0.2);
    return float2(uMuS, uR);
}

float3 Irradiance(sampler2D samp, float r, float muS) 
{
    float2 uv = GetIrradianceUV(r, muS);  
	return tex2Dlod(samp,float4(uv,0.0,0.0)).rgb;    
}

// incident sky light at given position, integrated over the hemisphere (irradiance)
// r=length(x)
// muS=dot(x,s) / r
float3 SkyIrradiance(float r, float muS)
{
    return Irradiance(_Irradiance, r, muS) * _Sun_Intensity;
}

// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
// (mu=cos(view zenith angle)), or zero if ray intersects ground

float3 TransmittanceWithShadow(float r, float mu) 
{
    return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? float3(0,0,0) : Transmittance(r, mu);
}

// incident sun light at given position (radiance)
// r=length(x)
// muS=dot(x,s) / r
float3 SunRadiance(float r, float muS)
{
    return TransmittanceWithShadow(r, muS) * _Sun_Intensity;
}

void SunRadianceAndSkyIrradiance(float3 worldP, float3 worldN, float3 worldS, out float3 sunL, out float3 skyE)
{
//	worldP *= scale;
    float r = length(worldP);
    if (r < 0.9 * Rg) {
        worldP.z += Rg;
        r = length(worldP);
    }
    float3 worldV = worldP / r; // vertical vector
    float muS = dot(worldV, worldS);

    float sunOcclusion = 1.0;// - sunShadow;
    sunL = SunRadiance(r, muS) * sunOcclusion;

    // ambient occlusion due only to slope, does not take self shadowing into account
    float skyOcclusion = (1.0 + dot(worldV, worldN)) * 0.5;
    // factor 2.0 : hack to increase sky contribution (numerical simulation of
    // "precompued atmospheric scattering" gives less luminance than in reality)
    skyE = 2.0 * SkyIrradiance(r, muS) * skyOcclusion;
}




//InScattering with modified atmo heights
float3 InScattering2(float3 camera, float3 _point, out float3 extinction, float shaftWidth, float scaleCoeff, float irradianceFactor) {
    // single scattered sunlight between two points
    // camera=observer
    // point=point on the ground
    // sundir=unit vector towards the sun
    // return scattered light and extinction coefficient
    float3 result = float3(0, 0, 0);
    extinction = float3(1, 1, 1);
    float3 viewdir = _point - camera;
    float d = length(viewdir) * scaleCoeff;
    viewdir = viewdir / d;
    /////////////////////experimental block begin
    float Rt0=Rt;
    Rt = Rg + (Rt - Rg) * _experimentalAtmoScale;
    //                viewdir.x += _viewdirOffset;
    viewdir = normalize(viewdir);
    /////////////////////experimental block end
    float r = length(camera) * scaleCoeff;
    if (r < 0.9 * Rg) {
        camera.y += Rg;
        _point.y += Rg;
        r = length(camera) * scaleCoeff;
    }
    float rMu = dot(camera, viewdir);
    float mu = rMu / r;
    float r0 = r;
    float mu0 = mu;
    float muExtinction=mu;
    _point -= viewdir * clamp(shaftWidth, 0.0, d);
    float deltaSq = SQRT(rMu * rMu - r * r + Rt * Rt, 0.000001);
//    float deltaSq = sqrt(rMu * rMu - r * r + Rt * Rt);
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
        if (r < Rg + 1600.0) {
            // avoids imprecision problems in aerial perspective near ground
            float f = (Rg + 1600.0) / r;
            r = r * f;
            rMu = rMu * f;
            _point = _point * f;
        }
        float r1 = length(_point);
        float rMu1 = dot(_point, viewdir);
        float mu1 = rMu1 / r1;
        float muS1 = dot(_point, SUN_DIR) / r1;
        #if defined (useAnalyticTransmittance)
        extinction = min(AnalyticTransmittance(r, mu, d), 1.0);
        //#else
        ////                                        float Rt1=lerp(Rt0, Rt, clamp((r-Rg-2000)/2000,0.0,1.0));    //fix line in the horizon, the dumb way but IDK it works
        //                    float Rt1=lerp(Rt0, Rt, clamp((r-Rg-2000)/1000,0.0,1.0));
        //                    if (mu > 0.0)
        //                    {
            //                        extinction = min(Transmittance(r, mu, Rt1) / Transmittance(r1, mu1, Rt1), 1.0);
        //                    }
        //                        else
        //                    {
            //                        extinction = min(Transmittance(r1, -mu1, Rt1) / Transmittance(r, -mu, Rt1), 1.0);
        //                    }
        //
        #endif
//        #ifdef useHorizonHack
//        const float EPS = 0.004;
//        //                    float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
//        float lim = -SQRT(1.0 - (Rg / r) * (Rg / r),0.000001);
//        if (abs(mu - lim) < EPS){                //ground fix, somehow doesn't really make a difference and causes all kind of crap in dx11/ogl
//            float a = ((mu - lim) + EPS) / (2.0 * EPS);
//            mu = lim - EPS;
//            //                        r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
//            r1 = SQRT(r * r + d * d + 2.0 * r * d * mu,0.000001);
//            mu1 = (r * mu + d) / r1;
//            float4 inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            float4 inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//            mu = lim + EPS;
//            //                        r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
//            r1 = SQRT(r * r + d * d + 2.0 * r * d * mu,0.000001);
//            mu1 = (r * mu + d) / r1;
//            inScatter0 = Texture4D(_Inscatter, r, mu, muS, nu);
//            inScatter1 = Texture4D(_Inscatter, r1, mu1, muS1, nu);
//            float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
//            inScatter = lerp(inScatterA, inScatterB, a);
//        }
//        else
//        #endif
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
    else
    {	//if out of atmosphere
        result = float3(0,0,0);
//        extinction = float3(1,1,1);
    }
    return result * SUN_INTENSITY;
}