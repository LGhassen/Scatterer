using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{
	public static class AtmosphereUtils
	{
		public static Color getExtinction(Vector3 camera, Vector3 viewdir, float Rt, float Rg, Texture2D m_transmit, float experimentalAtmoScale)
		{
			Rt=Rg+(Rt-Rg)*experimentalAtmoScale;
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

			Color extinction = (r > Rt) ? Color.white : Transmittance(r, mu, Rt, Rg, m_transmit);
			
			return extinction;
		}

		private static Vector2 GetTransmittanceUV(float r, float mu, float Rt, float Rg, Texture2D m_transmit)
		{
			float uR, uMu;
			
			uR = Mathf.Sqrt(Mathf.Max (0,(r - Rg)) / (Rt - Rg));
			uMu = Mathf.Atan((mu + 0.15f) / (1.0f + 0.15f) * Mathf.Tan(1.5f)) / 1.5f;
			
			return new Vector2(uMu, uR);
		}

		private static Color Transmittance(float r, float mu, float Rt, float Rg, Texture2D m_transmit)
		{
			Vector2 uv = GetTransmittanceUV(r, mu, Rt, Rg, m_transmit);
			return m_transmit.GetPixelBilinear(uv.x, uv.y);
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

	}
}

