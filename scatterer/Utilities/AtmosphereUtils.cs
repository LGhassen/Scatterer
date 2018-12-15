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
		public static Color getExtinction(Vector3 camera, Vector3 viewdir, float Rt, float Rg, Texture2D m_transmit)
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

	}
}

