using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace Scatterer
{
	public static class AtmosphereUtils
	{		
		private static float Limit(float r, float mu, float Rg, float Rt) 
		{
			float dout = -r * mu + Mathf.Sqrt(r * r * (mu * mu - 1f) + Rt * Rt);
			float delta2 = r * r * (mu * mu - 1f) + (Rg) * (Rg);
			
			if (delta2 >= 0f) 
			{ 
				float din = -r * mu - Mathf.Sqrt(delta2); 
				if (din >= 0f)
				{ 
					dout = Mathf.Min(dout, din); 
				}
			} 
			
			return dout;
		}

		// optical depth for ray (r,mu) of length d, using analytic formula
		// (mu=cos(view zenith angle)), intersections with ground ignored
		// H=height scale of exponential density function
		private static float OpticalDepth(float H, float r, float mu, float d, float Rg, float Rt)
		{
			float a = Mathf.Sqrt((0.5f/H)*r);
			Vector2 a01 = a*new Vector2(mu, mu + d / r);
			Vector2 a01s = new Vector2(Mathf.Sign(a01.x),Mathf.Sign(a01.y));
			Vector2 a01sq = a01*a01;
			float x = a01s.y > a01s.x ? Mathf.Exp(a01sq.x) : 0f;
			Vector2 y = a01s / (2.3193f* new Vector2(Mathf.Abs(a01.x), Mathf.Abs(a01.y)) + new Vector2(Mathf.Sqrt(1.52f*a01sq.x + 4f), Mathf.Sqrt(1.52f*a01sq.y + 4f))) * new Vector2(1f, Mathf.Exp(-d/H*(d/(2f*r)+mu)));
			return Mathf.Sqrt((6.2831f*H)*r) * Mathf.Exp((Rg-r)/H) * (x + Vector2.Dot(y, new Vector2(1f, -1f)));
		}

		private static float OpticalDepthToBoundaries(float H, float r, float mu, float Rg, float Rt)
		{ 
			float result = 0f;
			float d = Limit(r, mu, Rg, Rt); 
			
			result = OpticalDepth(H, r, mu, d, Rg, Rt);
			
			return mu < -Mathf.Sqrt(1f - (Rg / r) * (Rg / r)) ? 1e9f : result; 
		} 
		
		private static Color AnalyticTransmittance(float r, float mu, float Rt, float Rg, float HR, float HM, Vector3 betaR, Vector3 betaMEx)
		{
			Vector3 depth = betaR * OpticalDepthToBoundaries(HR, r, mu, Rg, Rt) + betaMEx * OpticalDepthToBoundaries(HM, r, mu, Rg, Rt);
			depth.x = Mathf.Clamp (Mathf.Exp (-depth.x), 1e-36f, 1f);
			depth.y = Mathf.Clamp (Mathf.Exp(-depth.y), 1e-36f, 1f);
			depth.z = Mathf.Clamp (Mathf.Exp(-depth.z), 1e-36f, 1f);
			return new Color (depth.x,depth.y,depth.z,1f);
		}

		private static Vector2 GetTransmittanceUV(float r, float mu, float Rt, float Rg)
		{
			float uR, uMu;

			uR = Mathf.Sqrt(Mathf.Max(0, (r - Rg)) / (Rt - Rg));
			uMu = Mathf.Atan((mu + 0.15f) / (1.0f + 0.15f) * Mathf.Tan(1.5f)) / 1.5f;

			return new Vector2(uMu, uR);
		}

		public static Color getOzoneExtinction(float r, float mu, float Rt, float Rg, Texture2D atmosphereAtlas, Vector2 ozoneTextureDimensions, Vector4 textureScaleAndOffsetInAtlas, Vector2 AtmosphereAtlasDimensions)
        {
            Vector2 uv = GetTransmittanceUV(r, mu, Rt, Rg);
            uv = remapUVToAtlas(uv, ozoneTextureDimensions, textureScaleAndOffsetInAtlas, AtmosphereAtlasDimensions);

            // Unity's get pixel Bilinear doesn't work exactly like shader-based bilinear sampling
            // See thread https://forum.unity.com/threads/confusion-about-texture-getpixelbinear.1236826/
            // Therefore manually remap from shader-based UV to unity-style UV to be able to sample bilinear correctly here
            // The equivalent transformation is just to remove a half-texel offset
            // Note that I checked all the math 10 times and compared GetPixelBilinear() to what the shader tex2Dlod outputs and confirmed
            // There is always a half texel offset
            uv -= new Vector2(0.5f, 0.5f) / AtmosphereAtlasDimensions;

            return atmosphereAtlas.GetPixelBilinear(uv.x, uv.y);
        }

		private static Vector2 remapUVToAtlas(Vector2 uv, Vector2 oldTexDimensions, Vector4 textureScaleAndOffsetInAtlas, Vector2 AtmosphereAtlasDimensions)
		{
			// Remove half pixel offset
			uv -= new Vector2(0.5f, 0.5f) / oldTexDimensions;

			// Clamp, note the half pixel offset is taken into account on both sides
			uv.x = Mathf.Clamp(uv.x, 0f, 1f - 1.0f / oldTexDimensions.x);
			uv.y = Mathf.Clamp(uv.y, 0f, 1f - 1.0f / oldTexDimensions.y);

			// Scale, offset and add new half pixel offset
			uv = uv * new Vector2(textureScaleAndOffsetInAtlas.x, textureScaleAndOffsetInAtlas.y) +
				new Vector2(textureScaleAndOffsetInAtlas.z, textureScaleAndOffsetInAtlas.w) +
				new Vector2(0.5f, 0.5f) / AtmosphereAtlasDimensions;

            return uv;
		}

		public static Color getExtinction(Vector3 camera, Vector3 viewdir, float Rt, float Rg, float HR, float HM, Vector3 betaR, Vector3 betaMEx, bool useOzone, Texture2D atmosphereAtlas, Vector2 ozoneTextureDimensions, Vector4 textureScaleAndOffsetInAtlas, Vector2 AtmosphereAtlasDimensions)
		{
			float r = camera.magnitude;
			float rMu = Vector3.Dot(camera, viewdir);
			float mu = rMu / r;
			
			float deltaSq = Mathf.Sqrt(rMu * rMu - r * r + Rt*Rt);
			
			float din = Mathf.Max(-rMu - deltaSq, 0f);
			if (din > 0f)
			{
				camera += din * viewdir;
				rMu += din;
				mu = rMu / Rt;
				r = Rt;
			}
			
			Color extinction = (r > Rt) ? Color.white : AnalyticTransmittance(r, mu, Rt, Rg, HR, HM, betaR, betaMEx);

			if (useOzone && r < Rt)
				extinction *= getOzoneExtinction(r, mu, Rt, Rg, atmosphereAtlas, ozoneTextureDimensions, textureScaleAndOffsetInAtlas, AtmosphereAtlasDimensions);

			return extinction;
		}

		public static Color Hdr(Color L, float exposure) {
			L = L * exposure;
			L.r = L.r < 1.413 ? Mathf.Pow(L.r * 0.38317f, 1.0f / 2.2f) : 1.0f - Mathf.Exp(-L.r);
			L.g = L.g < 1.413 ? Mathf.Pow(L.g * 0.38317f, 1.0f / 2.2f) : 1.0f - Mathf.Exp(-L.g);
			L.b = L.b < 1.413 ? Mathf.Pow(L.b * 0.38317f, 1.0f / 2.2f) : 1.0f - Mathf.Exp(-L.b);
			return L;
		}

		public static Color SimpleSkyirradiance(Vector3 worldP, Vector3 worldS, float Rt, float Rg, Color sunColor, Texture2D m_irradiance)
		{
			float r = worldP.magnitude;
			if (r < 0.9 * Rg)
			{
				worldP.z += Rg;
				r = worldP.magnitude;
			}
			
			Vector3 worldV = worldP.normalized; // vertical vector
			float muS = Vector3.Dot(worldV, worldS.normalized);
			
			// factor 2.0 : hack to increase sky contribution (numerical simulation of
			// "precompued atmospheric scattering" gives less luminance than in reality)
			Color skyE = 2.0f * SkyIrradiance(r, muS, Rt, Rg, sunColor, m_irradiance);

			return skyE;
		}

		// incident sky light at given position, integrated over the hemisphere (irradiance)
		// r=length(x)
		// muS=dot(x,s) / r
		private static Color SkyIrradiance(float r, float muS, float Rt, float Rg, Color sunColor, Texture2D m_irradiance)
		{	
			return Irradiance(r, muS, Rt, Rg, m_irradiance) * sunColor;
		}

		private static Color Irradiance(float r, float muS, float Rt, float Rg, Texture2D m_irradiance)
		{
			Vector2 uv = GetIrradianceUV(r, muS, Rt, Rg);
			return m_irradiance.GetPixelBilinear(uv.x, uv.y);
		}

		private static Vector2 GetIrradianceUV(float r, float muS, float Rt, float Rg) 
		{
			float uR = (r - Rg) / (Rt - Rg);
			float uMuS = (muS + 0.2f) / (1.0f + 0.2f);
			return new Vector2(uMuS, uR);
		}

		public static float getEclipseShadow(Vector3 worldPos, Vector3 worldLightPos, Vector3 occluderSpherePosition, float occluderSphereRadius, float lightSourceRadius)
		{
			var lightDirection = worldLightPos - worldPos;
			float lightDistance = lightDirection.magnitude;
			lightDirection = lightDirection / lightDistance;

			// computation of level of shadowing w  
			var sphereDirection = occluderSpherePosition - worldPos;  //occluder planet
			float sphereDistance = sphereDirection.magnitude;
			sphereDirection = sphereDirection / sphereDistance;

			float dd = lightDistance * (Mathf.Asin(Mathf.Min(1.0f, (Vector3.Cross(lightDirection, sphereDirection)).magnitude))
				- Mathf.Asin(Mathf.Min(1.0f, occluderSphereRadius / sphereDistance)));

			float w = smoothstep(-1.0f, 1.0f, -dd / lightSourceRadius);
			w = w * smoothstep(0.0f, 0.2f, Vector3.Dot(lightDirection, sphereDirection));

			return (1 - w);
		}

		// Reimplement because the Mathf Smoothstep doesn't match what is done in shaders
		public static float smoothstep(float a, float b, float x)
		{
			float t = Mathf.Clamp01((x - a) / (b - a));
			return t * t * (3.0f - (2.0f * t));
		}

	}
}

