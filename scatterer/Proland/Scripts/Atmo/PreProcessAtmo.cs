/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified and adapted for use with Kerbal Space Program by Ghassen Lahmar 2015
 * 
 * 
 */

using UnityEngine;
using System.Collections;
using System.IO;

namespace scatterer 
{
	/*
	 * Precomputes the tables for the given atmosphere parameters.
	 * To run this just create a new scene and add this script to a game object and then attach the compute shaders.
	 * Once the scene is run the tables will be saved to the file path.
	 * If you change some of the settings, like the table dimensions then you will need to open up the SkyNode.cs script
	 * and make sure the setting for the tables match the settings in that script.
	 */
	public class PreProcessAtmo : MonoBehaviour
	{
		//Dont change these
		const int NUM_THREADS = 8;
		const int READ = 0;
		const int WRITE = 1;

		//You can change these
		//The radius of the planet (Rg), radius of the atmosphere (Rt)
		const float Rg = 6360.0f;
		const float Rt = 6420.0f;
		const float RL = 6421.0f;
		//Dimensions of the tables
		const int TRANSMITTANCE_W = 256;
		const int TRANSMITTANCE_H = 64;
		const int SKY_W = 64;
		const int SKY_H = 16;
		const int RES_R = 32;
		const int RES_MU = 128;
		const int RES_MU_S = 32;
		const int RES_NU = 8;
		//Physical settings, Mie and Rayliegh values
		const float AVERAGE_GROUND_REFLECTANCE = 0.1f;
		readonly Vector4 BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f);
		readonly Vector4 BETA_MSca = new Vector4(4e-3f, 4e-3f, 4e-3f, 0.0f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		const float MIE_G = 0.8f;
		//Half heights for the atmosphere air density (HR) and particle density (HM)
		//This is the height in km that half the particles are found below
		const float HR = 8.0f;
		const float HM = 1.2f;

		RenderTexture m_transmittanceT;
		RenderTexture m_deltaET, m_deltaSRT, m_deltaSMT, m_deltaJT;
		RenderTexture[] m_irradianceT, m_inscatterT;

		//This is where the tables will be saved to
		public string m_filePath = "/Proland/Textures/Atmo";

		public ComputeShader m_copyInscatter1, m_copyInscatterN, m_copyIrradiance;
		public ComputeShader m_inscatter1, m_inscatterN, m_inscatterS;
		public ComputeShader m_irradiance1, m_irradianceN, m_transmittance;
		public ComputeShader m_readData;

		int m_step, m_order;
		bool m_finished = false;

		const bool WRITE_DEBUG_TEX = false;

		void Start()
		{

			m_irradianceT = new RenderTexture[2];
			m_inscatterT = new RenderTexture[2];

			m_transmittanceT = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBFloat);
			m_transmittanceT.enableRandomWrite = true;
			m_transmittanceT.Create();

			m_irradianceT[0] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_irradianceT[0].enableRandomWrite = true;
			m_irradianceT[0].Create();

			m_irradianceT[1] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_irradianceT[1].enableRandomWrite = true;
			m_irradianceT[1].Create();

			m_inscatterT[0] = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
			m_inscatterT[0].isVolume = true;
			m_inscatterT[0].enableRandomWrite = true;
			m_inscatterT[0].volumeDepth = RES_R;
			m_inscatterT[0].Create();

			m_inscatterT[1] = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
			m_inscatterT[1].isVolume = true;
			m_inscatterT[1].enableRandomWrite = true;
			m_inscatterT[1].volumeDepth = RES_R;
			m_inscatterT[1].Create();

			m_deltaET = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_deltaET.enableRandomWrite = true;
			m_deltaET.Create();
			                   
			m_deltaSRT = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
			m_deltaSRT.isVolume = true;
			m_deltaSRT.enableRandomWrite = true;
			m_deltaSRT.volumeDepth = RES_R;
			m_deltaSRT.Create();

			m_deltaSMT = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
			m_deltaSMT.isVolume = true;
			m_deltaSMT.enableRandomWrite = true;
			m_deltaSMT.volumeDepth = RES_R;
			m_deltaSMT.Create();

			m_deltaJT = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBFloat);
			m_deltaJT.isVolume = true;
			m_deltaJT.enableRandomWrite = true;
			m_deltaJT.volumeDepth = RES_R;
			m_deltaJT.Create();

			SetParameters(m_copyInscatter1);
			SetParameters(m_copyInscatterN);
			SetParameters(m_copyIrradiance);
			SetParameters(m_inscatter1);
			SetParameters(m_inscatterN);
			SetParameters(m_inscatterS);
			SetParameters(m_irradiance1);
			SetParameters(m_irradianceN);
			SetParameters(m_transmittance);

			m_step = 0;
			m_order = 2;

			RTUtility.ClearColor(m_irradianceT);

			while(!m_finished) {
				Preprocess();
			}	
		}

		void SetParameters(ComputeShader mat)
		{
			mat.SetFloat("Rg", Rg);
			mat.SetFloat("Rt", Rt);
			mat.SetFloat("RL", RL);
			mat.SetInt("TRANSMITTANCE_W", TRANSMITTANCE_W);
			mat.SetInt("TRANSMITTANCE_H", TRANSMITTANCE_H);
			mat.SetInt("SKY_W", SKY_W);
			mat.SetInt("SKY_H", SKY_H);
			mat.SetInt("RES_R", RES_R);
			mat.SetInt("RES_MU", RES_MU);
			mat.SetInt("RES_MU_S", RES_MU_S);
			mat.SetInt("RES_NU", RES_NU);
			mat.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
			mat.SetFloat("HR", HR);
			mat.SetFloat("HM", HM);
			mat.SetVector("betaR", BETA_R);
			mat.SetVector("betaMSca", BETA_MSca);
			mat.SetVector("betaMEx", BETA_MSca / 0.9f);
			mat.SetFloat("mieG", Mathf.Clamp(MIE_G, 0.0f, 0.99f));
		}

		void Preprocess()
		{
			if (m_step == 0) 
			{
				// computes transmittance texture T (line 1 in algorithm 4.1)
				m_transmittance.SetTexture(0, "transmittanceWrite", m_transmittanceT);
				m_transmittance.Dispatch(0, TRANSMITTANCE_W/NUM_THREADS, TRANSMITTANCE_H/NUM_THREADS, 1);
			} 
			else if (m_step == 1) 
			{
				// computes irradiance texture deltaE (line 2 in algorithm 4.1)
				m_irradiance1.SetTexture(0, "transmittanceRead", m_transmittanceT);
				m_irradiance1.SetTexture(0, "deltaEWrite", m_deltaET);
				m_irradiance1.Dispatch(0, SKY_W/NUM_THREADS, SKY_H/NUM_THREADS, 1);

				if(WRITE_DEBUG_TEX)
					SaveAs8bit(SKY_W, SKY_H, 4, "/deltaE_debug", m_deltaET);
			} 
			else if (m_step == 2) 
			{
				// computes single scattering texture deltaS (line 3 in algorithm 4.1)
				// Rayleigh and Mie separated in deltaSR + deltaSM
				m_inscatter1.SetTexture(0, "transmittanceRead", m_transmittanceT);
				m_inscatter1.SetTexture(0, "deltaSRWrite", m_deltaSRT);
				m_inscatter1.SetTexture(0, "deltaSMWrite", m_deltaSMT);

				//The inscatter calc's can be quite demanding for some cards so process 
				//the calc's in layers instead of the whole 3D data set.
				for(int i = 0; i < RES_R; i++) {
					m_inscatter1.SetInt("layer", i);
					m_inscatter1.Dispatch(0, (RES_MU_S*RES_NU)/NUM_THREADS, RES_MU/NUM_THREADS, 1);
				}

				if(WRITE_DEBUG_TEX)
					SaveAs8bit(RES_MU_S*RES_NU, RES_MU*RES_R, 4, "/deltaSR_debug", m_deltaSRT);

				if(WRITE_DEBUG_TEX)
					SaveAs8bit(RES_MU_S*RES_NU, RES_MU*RES_R, 4, "/deltaSM_debug", m_deltaSMT);
			} 
			else if (m_step == 3) 
			{
				// copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
				m_copyIrradiance.SetFloat("k", 0.0f);
				m_copyIrradiance.SetTexture(0, "deltaERead", m_deltaET);
				m_copyIrradiance.SetTexture(0, "irradianceRead", m_irradianceT[READ]);
				m_copyIrradiance.SetTexture(0, "irradianceWrite", m_irradianceT[WRITE]);
				m_copyIrradiance.Dispatch(0, SKY_W/NUM_THREADS, SKY_H/NUM_THREADS, 1);

				RTUtility.Swap(m_irradianceT);
			} 
			else if (m_step == 4) 
			{
				// copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
				m_copyInscatter1.SetTexture(0, "deltaSRRead", m_deltaSRT);
				m_copyInscatter1.SetTexture(0, "deltaSMRead", m_deltaSMT);
				m_copyInscatter1.SetTexture(0, "inscatterWrite", m_inscatterT[WRITE]);

				//The inscatter calc's can be quite demanding for some cards so process 
				//the calc's in layers instead of the whole 3D data set.
				for(int i = 0; i < RES_R; i++) {
					m_copyInscatter1.SetInt("layer", i);
					m_copyInscatter1.Dispatch(0, (RES_MU_S*RES_NU)/NUM_THREADS, RES_MU/NUM_THREADS, 1);
				}

				RTUtility.Swap(m_inscatterT);
			} 
			else if (m_step == 5) 
			{
				// computes deltaJ (line 7 in algorithm 4.1)
				m_inscatterS.SetInt("first", (m_order == 2) ? 1 : 0);
				m_inscatterS.SetTexture(0, "transmittanceRead", m_transmittanceT);
				m_inscatterS.SetTexture(0, "deltaERead", m_deltaET);
				m_inscatterS.SetTexture(0, "deltaSRRead", m_deltaSRT);
				m_inscatterS.SetTexture(0, "deltaSMRead", m_deltaSMT);
				m_inscatterS.SetTexture(0, "deltaJWrite", m_deltaJT);

				//The inscatter calc's can be quite demanding for some cards so process 
				//the calc's in layers instead of the whole 3D data set.
				for(int i = 0; i < RES_R; i++) {
					m_inscatterS.SetInt("layer", i);
					m_inscatterS.Dispatch(0, (RES_MU_S*RES_NU)/NUM_THREADS, RES_MU/NUM_THREADS, 1);
				}
			} 
			else if (m_step == 6) 
			{
				// computes deltaE (line 8 in algorithm 4.1)
				m_irradianceN.SetInt("first", (m_order == 2) ? 1 : 0);
				m_irradianceN.SetTexture(0, "deltaSRRead", m_deltaSRT);
				m_irradianceN.SetTexture(0, "deltaSMRead", m_deltaSMT);
				m_irradianceN.SetTexture(0, "deltaEWrite", m_deltaET);
				m_irradianceN.Dispatch(0, SKY_W/NUM_THREADS, SKY_H/NUM_THREADS, 1);
			} 
			else if (m_step == 7) 
			{
				// computes deltaS (line 9 in algorithm 4.1)
				m_inscatterN.SetTexture(0, "transmittanceRead", m_transmittanceT);
				m_inscatterN.SetTexture(0, "deltaJRead", m_deltaJT);
				m_inscatterN.SetTexture(0, "deltaSRWrite", m_deltaSRT);

				//The inscatter calc's can be quite demanding for some cards so process 
				//the calc's in layers instead of the whole 3D data set.
				for(int i = 0; i < RES_R; i++) {
					m_inscatterN.SetInt("layer", i);
					m_inscatterN.Dispatch(0, (RES_MU_S*RES_NU)/NUM_THREADS, RES_MU/NUM_THREADS, 1);
				}
			} 
			else if (m_step == 8) 
			{
				// adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
				m_copyIrradiance.SetFloat("k", 1.0f);
				m_copyIrradiance.SetTexture(0, "deltaERead", m_deltaET);
				m_copyIrradiance.SetTexture(0, "irradianceRead", m_irradianceT[READ]);
				m_copyIrradiance.SetTexture(0, "irradianceWrite", m_irradianceT[WRITE]);
				m_copyIrradiance.Dispatch(0, SKY_W/NUM_THREADS, SKY_H/NUM_THREADS, 1);
				
				RTUtility.Swap(m_irradianceT);
			} 
			else if (m_step == 9) 
			{

				// adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
				m_copyInscatterN.SetTexture(0, "deltaSRead", m_deltaSRT);
				m_copyInscatterN.SetTexture(0, "inscatterRead", m_inscatterT[READ]);
				m_copyInscatterN.SetTexture(0, "inscatterWrite", m_inscatterT[WRITE]);

				//The inscatter calc's can be quite demanding for some cards so process 
				//the calc's in layers instead of the whole 3D data set.
				for(int i = 0; i < RES_R; i++) {
					m_copyInscatterN.SetInt("layer", i);
					m_copyInscatterN.Dispatch(0, (RES_MU_S*RES_NU)/NUM_THREADS, RES_MU/NUM_THREADS, 1);
				}

				RTUtility.Swap(m_inscatterT);

				if (m_order < 4) {
					m_step = 4;
					m_order += 1;
				}
			} 
			else if (m_step == 10) 
			{
				SaveAsRaw(TRANSMITTANCE_W * TRANSMITTANCE_H, 3, "/transmittance", m_transmittanceT);

				SaveAsRaw(SKY_W * SKY_H, 3, "/irradiance", m_irradianceT[READ]);
	
				SaveAsRaw((RES_MU_S*RES_NU) * RES_MU * RES_R, 4, "/inscatter", m_inscatterT[READ]);

				if(WRITE_DEBUG_TEX)
				{
					SaveAs8bit(TRANSMITTANCE_W, TRANSMITTANCE_H, 4, "/transmittance_debug", m_transmittanceT);

					SaveAs8bit(SKY_W, SKY_H, 4, "/irradiance_debug", m_irradianceT[READ], 10.0f);

					SaveAs8bit(RES_MU_S*RES_NU, RES_MU*RES_R, 4, "/inscater_debug", m_inscatterT[READ]);
				}
			} 
			else if (m_step == 11) 
			{
				m_finished = true;
				Debug.Log("Proland::PreProcessAtmo::Preprocess - Preprocess done. Files saved to - " + m_filePath);
			}

			m_step += 1;
		}

		void OnDestroy()
		{
			m_transmittanceT.Release();
			m_irradianceT[0].Release();
			m_irradianceT[1].Release();
			m_inscatterT[0].Release();
			m_inscatterT[1].Release();
			m_deltaET.Release();
			m_deltaSRT.Release();
			m_deltaSMT.Release();
			m_deltaJT.Release();
		}

		void SaveAsRaw(int size, int channels, string fileName, RenderTexture rtex)
		{
			ComputeBuffer buffer = new ComputeBuffer(size, sizeof(float)*channels);
			
			CBUtility.ReadFromRenderTexture(rtex, channels, buffer, m_readData);
			
			float[] data = new float[size * channels];
			
			buffer.GetData(data);

			byte[] byteArray = new byte[size * 4 * channels];
			System.Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);
			System.IO.File.WriteAllBytes(Application.dataPath + m_filePath + fileName + ".raw", byteArray);
			
			buffer.Release();
		}

		void SaveAs8bit(int width, int height, int channels, string fileName, RenderTexture rtex, float scale = 1.0f)
		{
			//Only used to get a visible image for debugging.

			ComputeBuffer buffer = new ComputeBuffer(width*height, sizeof(float)*channels);
			
			CBUtility.ReadFromRenderTexture(rtex, channels, buffer, m_readData);
			
			float[] data = new float[width*height* channels];
			
			buffer.GetData(data);

			Texture2D tex = new Texture2D(width, height);

			for(int x = 0; x < width; x++)
			{
				for(int y = 0; y < height; y++)
				{
					Color col = new Color(0,0,0,1);

					col.r = data[(x + y * width) * channels + 0];

					if(channels > 1)
						col.g = data[(x + y * width) * channels + 1];

					if(channels > 2)
						col.b = data[(x + y * width) * channels + 2];

					tex.SetPixel(x, y, col * scale);
				}
			}

			tex.Apply();

			byte[] bytes = tex.EncodeToPNG();

			System.IO.File.WriteAllBytes(Application.dataPath + m_filePath + fileName + ".png", bytes);

			buffer.Release();

		}

	}

}















