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
 * Modified and adapted for use with Kerbal Space Program by Ghassen Lahmar 2015-2021
 *
 */

using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

namespace scatterer 
{
	public class AtmoPreprocessor : MonoBehaviour
	{
		private static AtmoPreprocessor instance;

		public static AtmoPreprocessor Instance
		{
			get 
			{
				if (ReferenceEquals(instance,null))
				{
					instance = new AtmoPreprocessor();
					instance.InitMaterials();
					Utils.LogDebug("AtmoPreprocessor instance created");
				}
				return instance;
			}
		}

		const int READ = 0; const int WRITE = 1;

		float Rg = 60000.0f;
		float Rt = 71500.603f;

		//Dimensions of the tables
		const int TRANSMITTANCE_W = 256;
		const int TRANSMITTANCE_H = 64;
		const int SKY_W = 64;
		const int SKY_H = 16;

		public static Vector4 PRECOMPUTED_SCTR_LUT_DIM_DEFAULT = new Vector4(32f,128f,32f,16f);		//the one from yusov, double the current one so should be 16 megs in half precision
		public static Vector4 PRECOMPUTED_SCTR_LUT_DIM_PREVIEW = new Vector4(16f,64f,16f,2f);		//fast preview version, 32x smaller
		private int xTiles = 32, yTiles = 16;

		Vector4 PRECOMPUTED_SCTR_LUT_DIM = PRECOMPUTED_SCTR_LUT_DIM_DEFAULT;
		
		float AVERAGE_GROUND_REFLECTANCE = 0.1f;
		Vector4 BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f);
		Vector4 BETA_MSca = new Vector4(4e-3f, 4e-3f, 4e-3f, 0.0f);
		
		float MIE_G = 0.8f;
		
		float HR = 8.0f;
		float HM = 1.2f;

		RenderTexture m_transmittanceT;
		RenderTexture m_deltaET, m_deltaSRT, m_deltaSMT, m_deltaJT;
		public RenderTexture[] m_irradianceT, m_inscatterT;

		Material m_transmittanceMaterial, m_irradianceNMaterial, m_irradiance1Material, m_inscatter1Material, m_inscatterNMaterial, m_inscatterSMaterial, m_copyInscatter1Material, m_copyInscatterNMaterial, m_copyIrradianceMaterial;

		int m_step, m_order;
		int scatteringOrders = 4; //min is 2 unless you skip that step
		bool multipleScattering = true;

		bool m_finished = false;

//		#define WRITE_DEBUG_TEX
		
		private void InitMaterials()
		{
			m_transmittanceMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Transmittance")]);
			m_irradianceNMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/IrradianceN")]);
			m_irradiance1Material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Irradiance1")]);
			m_inscatter1Material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Inscatter1")]);
			m_inscatterNMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/InscatterN")]);
			m_inscatterSMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/InscatterS")]);
			m_copyInscatter1Material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/CopyInscatter1")]);
			m_copyInscatterNMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/CopyInscatterN")]);
			m_copyIrradianceMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/CopyIrradiance")]);
		}
		
		public static float CalculateRt(float inRg, float inHR, float inHM, Vector3 in_betaR, Vector3 in_BETA_MSca)
		{
			float RtHR  =  - 1000f * inHR * Mathf.Log(0.0000001f / in_betaR.magnitude);
			float RtHM  =  - 1000f * inHM * Mathf.Log(0.0000001f / in_BETA_MSca.magnitude);
			
			return inRg + Mathf.Max(RtHR, RtHM);
		}

		public static void deleteCache()
		{
			string cachePath = Utils.GameDataPath + "/ScattererAtmosphereCache/PluginData";
			Directory.Delete(cachePath, true);
		}

		//Hashing may not be a very good idea with floats though
		public static string GetAtmoHash(float inRG,float inRT, Vector4 inBETA_R, Vector4 inBETA_MSca, float inMIE_G, float inHR, float inHM, float inGRref, bool inMultiple, Vector4 inPRECOMPUTED_SCTR_LUT_DIM)
		{
			Hash128 result = new Hash128();
			
			Matrix4x4 hashMatrix = Matrix4x4.identity;
			hashMatrix.SetRow(0, inBETA_R);
			hashMatrix.SetRow(1, inBETA_MSca);
			hashMatrix.SetRow(2, inPRECOMPUTED_SCTR_LUT_DIM);
			hashMatrix.SetRow(3, new Vector4(inRG, inRT, inHR, inHM));
			
			HashUtilities.QuantisedMatrixHash (ref hashMatrix, ref result);
			
			Hash128 temp = new Hash128();
			Vector3 hashVector = new Vector3 (inMIE_G, inGRref, inMultiple? 1f : 0f);
			HashUtilities.QuantisedVectorHash (ref hashVector, ref temp);
			
			HashUtilities.AppendHash (ref temp, ref result);
			
			return result.ToString ();
		}

		public void Generate(float inRG,float inRT, Vector4 inBETA_R, Vector4 inBETA_MSca, float inMIE_G, float inHR, float inHM, float inGRref, bool inMultiple, Vector4 inPRECOMPUTED_SCTR_LUT_DIM, string assetPath)
		{
			//Rescale to a fixed radius which we know never causes issues
			float referenceRadius = 1000000f;
			float scaleFactor = referenceRadius / inRG;

			Rg = referenceRadius;
			Rt = inRT * scaleFactor;

			HR = inHR * 1000f * scaleFactor;
			HM = inHM * 1000f * scaleFactor;

			BETA_R = inBETA_R * 0.001f / scaleFactor;
			BETA_MSca = inBETA_MSca * 0.001f / scaleFactor;

			MIE_G = inMIE_G;
			if (inMultiple)
				MIE_G = Mathf.Min (MIE_G, 0.86f); //values of mie_G > 0.86 seem to break multiple scattering for some weird reason

			AVERAGE_GROUND_REFLECTANCE = inGRref;
			m_finished = false;
			multipleScattering = inMultiple;
			PRECOMPUTED_SCTR_LUT_DIM = inPRECOMPUTED_SCTR_LUT_DIM;

			m_irradianceT = new RenderTexture[2];
			m_inscatterT = new RenderTexture[2];
			
			m_transmittanceT = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBFloat);
			m_transmittanceT.wrapMode = TextureWrapMode.Clamp;
			m_transmittanceT.filterMode = FilterMode.Bilinear;
			m_transmittanceT.Create();
			
			m_irradianceT[0] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_irradianceT[0].wrapMode = TextureWrapMode.Clamp;
			m_irradianceT[0].filterMode = FilterMode.Bilinear;
			m_irradianceT[0].Create();
			
			m_irradianceT[1] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_irradianceT[1].wrapMode = TextureWrapMode.Clamp;
			m_irradianceT[1].filterMode = FilterMode.Bilinear;
			m_irradianceT[1].Create();

			m_deltaET = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_deltaET.Create();

			m_inscatterT[0] = new RenderTexture((int)(PRECOMPUTED_SCTR_LUT_DIM.x * PRECOMPUTED_SCTR_LUT_DIM.y), (int)(PRECOMPUTED_SCTR_LUT_DIM.z * PRECOMPUTED_SCTR_LUT_DIM.w), 0, RenderTextureFormat.ARGBFloat);
			m_inscatterT[0].wrapMode = TextureWrapMode.Clamp;
			m_inscatterT[0].filterMode = FilterMode.Bilinear;
			m_inscatterT[0].Create();

			m_inscatterT[1] = new RenderTexture((int)(PRECOMPUTED_SCTR_LUT_DIM.x * PRECOMPUTED_SCTR_LUT_DIM.y), (int)(PRECOMPUTED_SCTR_LUT_DIM.z * PRECOMPUTED_SCTR_LUT_DIM.w), 0, RenderTextureFormat.ARGBFloat);
			m_inscatterT[1].wrapMode = TextureWrapMode.Clamp;
			m_inscatterT[1].filterMode = FilterMode.Bilinear;
			m_inscatterT[1].Create();

			m_deltaSRT = new RenderTexture((int)(PRECOMPUTED_SCTR_LUT_DIM.x * PRECOMPUTED_SCTR_LUT_DIM.y), (int)(PRECOMPUTED_SCTR_LUT_DIM.z * PRECOMPUTED_SCTR_LUT_DIM.w), 0, RenderTextureFormat.ARGBFloat);
			m_deltaSRT.wrapMode = TextureWrapMode.Clamp;
			m_deltaSRT.filterMode = FilterMode.Bilinear;
			m_deltaSRT.Create();

			m_deltaSMT = new RenderTexture((int)(PRECOMPUTED_SCTR_LUT_DIM.x * PRECOMPUTED_SCTR_LUT_DIM.y), (int)(PRECOMPUTED_SCTR_LUT_DIM.z * PRECOMPUTED_SCTR_LUT_DIM.w), 0, RenderTextureFormat.ARGBFloat);
			m_deltaSMT.wrapMode = TextureWrapMode.Clamp;
			m_deltaSMT.filterMode = FilterMode.Bilinear;
			m_deltaSMT.Create();

			m_deltaJT = new RenderTexture((int)(PRECOMPUTED_SCTR_LUT_DIM.x * PRECOMPUTED_SCTR_LUT_DIM.y), (int)(PRECOMPUTED_SCTR_LUT_DIM.z * PRECOMPUTED_SCTR_LUT_DIM.w), 0, RenderTextureFormat.ARGBFloat);
			m_deltaJT.wrapMode = TextureWrapMode.Clamp;
			m_deltaJT.filterMode = FilterMode.Bilinear;
			m_deltaJT.Create();

			SetParameters(m_transmittanceMaterial);
			SetParameters(m_irradianceNMaterial);
			SetParameters(m_irradiance1Material);
			SetParameters(m_inscatter1Material);
			SetParameters(m_inscatterNMaterial);
			SetParameters(m_inscatterSMaterial);
			SetParameters(m_copyInscatter1Material);
			SetParameters(m_copyInscatterNMaterial);
			SetParameters(m_copyIrradianceMaterial);

			m_step = 0;
			m_order = 2;

			RTUtility.ClearColor(m_irradianceT);
			
			while(!m_finished)
			{
				Preprocess(assetPath);
			}

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

		void SetParameters(Material mat)
		{
			mat.SetFloat("Rg", Rg);
			mat.SetFloat("Rt", Rt);
			mat.SetInt("TRANSMITTANCE_W", TRANSMITTANCE_W);
			mat.SetInt("TRANSMITTANCE_H", TRANSMITTANCE_H);
			mat.SetInt("SKY_W", SKY_W);
			mat.SetInt("SKY_H", SKY_H);
			mat.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
			mat.SetFloat("HR", HR);
			mat.SetFloat("HM", HM);
			mat.SetVector("betaR", BETA_R);
			mat.SetVector("betaMSca", BETA_MSca);
			mat.SetVector("betaMEx", BETA_MSca / 0.9f);
			mat.SetFloat("mieG", Mathf.Clamp(MIE_G, 0.0f, 0.99f));
			mat.SetFloat ("Sun_intensity", 10f);
			mat.SetVector("PRECOMPUTED_SCTR_LUT_DIM", PRECOMPUTED_SCTR_LUT_DIM);
			mat.SetVector ("tiles", new Vector2 (xTiles, yTiles));
		}

		void Preprocess(string assetPath)
		{
			if (m_step == 0) 
			{
				Graphics.Blit(null, m_transmittanceT, m_transmittanceMaterial);
			} 
			else if (m_step == 1) 	// computes irradiance texture deltaE (line 2 in algorithm 4.1)
			{
				m_irradiance1Material.SetTexture("transmittanceRead", m_transmittanceT);
				Graphics.Blit(null, m_deltaET, m_irradiance1Material);

				#if(WRITE_DEBUG_TEX)
					SaveAs8bit(SKY_W, SKY_H, 4, "/deltaE_debug", m_deltaET);
				#endif
			} 
			else if (m_step == 2) 
			{
				// computes single scattering texture deltaS (line 3 in algorithm 4.1)
				// Rayleigh and Mie separated in deltaSR + deltaSM
				m_inscatter1Material.SetTexture("transmittanceRead", m_transmittanceT);

				//render in tiles because older GPUs (series 7xx and integrated hd 3xxx) crash when rendering the full res
				for (int i=0;i<xTiles;i++)
				{
					for (int j=0;j<yTiles;j++)
					{
						m_inscatter1Material.SetVector("currentTile", new Vector2(i,j));

						// rayleigh pass
						Graphics.Blit(null, m_deltaSRT, m_inscatter1Material, 0);
						
						// mie pass
						Graphics.Blit(null, m_deltaSMT, m_inscatter1Material, 1);
					}
				}
			}
			else if (m_step == 3) // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
			{
				m_copyIrradianceMaterial.SetFloat("k", 0.0f);
				m_copyIrradianceMaterial.SetTexture("deltaERead", m_deltaET);
				m_copyIrradianceMaterial.SetTexture("irradianceRead", m_irradianceT[READ]);

				Graphics.Blit(null, m_irradianceT[WRITE], m_copyIrradianceMaterial, 0);

				RTUtility.Swap(m_irradianceT);
			} 
			else if (m_step == 4) // copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
			{
				//render in tiles because older GPUs (series 7xx and integrated hd 3xxx) crash when rendering the full res
				for (int i=0;i<xTiles;i++)
				{
					for (int j=0;j<yTiles;j++)
					{
						m_copyInscatter1Material.SetVector("currentTile", new Vector2(i,j));
						
						m_copyInscatter1Material.SetTexture("deltaSRRead", m_deltaSRT);
						m_copyInscatter1Material.SetTexture("deltaSMRead", m_deltaSMT);
						
						Graphics.Blit(null, m_inscatterT[WRITE], m_copyInscatter1Material, 0);
					}
				}

				RTUtility.Swap(m_inscatterT);
			} 
			else if (m_step == 5) 
			{
				//render in tiles because older GPUs (series 7xx and integrated hd 3xxx) crash when rendering the full res
				for (int i=0;i<xTiles;i++)
				{
					for (int j=0;j<yTiles;j++)
					{
						m_inscatterSMaterial.SetVector("currentTile", new Vector2(i,j));

						m_inscatterSMaterial.SetInt("first", (m_order == 2) ? 1 : 0);
						m_inscatterSMaterial.SetTexture("transmittanceRead", m_transmittanceT);
						m_inscatterSMaterial.SetTexture("deltaERead", m_deltaET);
						m_inscatterSMaterial.SetTexture("deltaSRRead", m_deltaSRT);
						m_inscatterSMaterial.SetTexture("deltaSMRead", m_deltaSMT);
						
						m_inscatterSMaterial.SetTexture("deltaSRReadSampler", m_deltaSRT);
						m_inscatterSMaterial.SetTexture("deltaSMReadSampler", m_deltaSMT);
						
						Graphics.Blit(null, m_deltaJT, m_inscatterSMaterial, 0);
					}
				}
			}
			else if (m_step == 6) // computes deltaE (line 8 in algorithm 4.1)
			{
				m_irradianceNMaterial.SetInt("first", (m_order == 2) ? 1 : 0);
				m_irradianceNMaterial.SetTexture("deltaSRRead", m_deltaSRT);
				m_irradianceNMaterial.SetTexture("deltaSMRead", m_deltaSMT);

				m_irradianceNMaterial.SetTexture("deltaSRReadSampler", m_deltaSRT);
				m_irradianceNMaterial.SetTexture("deltaSMReadSampler", m_deltaSMT);

				Graphics.Blit(null, m_deltaET, m_irradianceNMaterial);
			} 
			else if (m_step == 7) // computes deltaS (line 9 in algorithm 4.1)
			{
				//render in tiles because older GPUs (series 7xx and integrated hd 3xxx) crash when rendering the full res
				for (int i=0;i<xTiles;i++)
				{
					for (int j=0;j<yTiles;j++)
					{
						m_inscatterNMaterial.SetVector("currentTile", new Vector2(i,j));

						m_inscatterNMaterial.SetTexture("transmittanceRead", m_transmittanceT);
						m_inscatterNMaterial.SetTexture("deltaJRead", m_deltaJT);
						m_inscatterNMaterial.SetTexture("deltaJReadSampler", m_deltaJT);
						
						
						Graphics.Blit(null, m_deltaSRT, m_inscatterNMaterial, 0);
					}
				}
			}
			else if (m_step == 8) // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
			{
				m_copyIrradianceMaterial.SetFloat("k", 1.0f);
				m_copyIrradianceMaterial.SetTexture("deltaERead", m_deltaET);
				m_copyIrradianceMaterial.SetTexture("irradianceRead", m_irradianceT[READ]);

				Graphics.Blit(null, m_irradianceT[WRITE], m_copyIrradianceMaterial);

				RTUtility.Swap(m_irradianceT);
			}
			else if (m_step == 9 && multipleScattering)
			{
				//adds deltaS into inscatter texture S (line 11 in algorithm 4.1)

				//render in tiles because older GPUs (series 7xx and integrated hd 3xxx) crash when rendering the full res
				for (int i=0;i<xTiles;i++)
				{
					for (int j=0;j<yTiles;j++)
					{
						m_copyInscatterNMaterial.SetVector("currentTile", new Vector2(i,j));
						
						m_copyInscatterNMaterial.SetTexture("deltaSRead", m_deltaSRT);
						m_copyInscatterNMaterial.SetTexture("inscatterRead", m_inscatterT[READ]);
						
						Graphics.Blit(null,  m_inscatterT[WRITE], m_copyInscatterNMaterial, 0);
					}
				}
				
				RTUtility.Swap(m_inscatterT);
				
				if (m_order < scatteringOrders) {
					m_step = 4;
					m_order += 1;
				}
			} 
			else if (m_step == 10) 
			{
				if (!Directory.Exists(assetPath))
					Directory.CreateDirectory(assetPath);

				// Add an option to clean up old cache files on start?
				SaveAsHalf(m_irradianceT[READ], assetPath+"/irradiance");
				SaveAsHalf(m_inscatterT[READ], assetPath+"/inscatter");
			} 
			else if (m_step == 11) 
			{
				m_finished = true;
				Utils.LogInfo("Atmo generation successful");
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

		void SaveAsHalf(RenderTexture rtex, string fileName)
		{
			Texture2D temp = new Texture2D(rtex.width, rtex.height, TextureFormat.RGBAHalf, false, false);
			
			RenderTexture.active = rtex;
			temp.ReadPixels (new Rect (0, 0, rtex.width, rtex.height), 0, 0);
			temp.Apply ();
			
			byte[] byteArray = temp.GetRawTextureData();
			System.IO.File.WriteAllBytes(fileName + ".half", byteArray);
		}
	}

}















