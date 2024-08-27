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
 * Modified and adapted for use with Kerbal Space Program by Ghassen Lahmar 2015-2022
 *
 */

using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;

namespace Scatterer 
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class AtmoPreprocessor : MonoBehaviour
	{
		private static AtmoPreprocessor instance;

		public void Awake()
        {
			if (instance == null)
			{
				instance = this;
				instance.InitMaterials();
				Utils.LogDebug("AtmoPreprocessor instance created");
			}
			else
			{
				throw new UnityException("Attempted double instance!");
			}
		}

		public static AtmoPreprocessor Instance
		{
			get 
			{
				return instance;
			}
		}

		public void Start()
		{
			DontDestroyOnLoad(this);
		}

		private static bool atmoGenerationRunning = false;

		public static Vector4 ScatteringLutDimensionsDefault { get => Scatterer.Instance.mainSettings.useLowResolutionAtmosphere ? scatteringLutDimensionsPreview : scatteringLutDimensionsDefault; }

        const int READ = 0; const int WRITE = 1;

		float Rg = 60000.0f;
		float Rt = 71500.603f;

		//Dimensions of the tables
		const int TRANSMITTANCE_W = 512;
		const int TRANSMITTANCE_H = 128;
		const int SKY_W = 64;
		const int SKY_H = 16;

		private static Vector4 scatteringLutDimensionsDefault = new Vector4(32f, 128f, 32f, 16f);   //the one from yusov, double the current one so should be 16 megs in half precision
		public static Vector4 scatteringLutDimensionsPreview = new Vector4(16f, 64f, 16f, 2f);      //fast preview version, 32x smaller

        private static int xTilesDefault = 32, yTilesDefault = 64;
		private static int xTilesPreview = 32, yTilesPreview = 16;

		Vector4 scatteringLutDimensions = ScatteringLutDimensionsDefault;
		int xTiles = xTilesDefault, yTiles = yTilesDefault;

		float AVERAGE_GROUND_REFLECTANCE = 0.1f;
		Vector4 BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f);
		Vector4 BETA_MSca = new Vector4(4e-3f, 4e-3f, 4e-3f, 0.0f);
		
		float MIE_G = 0.8f;
		
		float HR = 8000.0f;
		float HM = 1200.0f;

		Vector3 ozoneAbsorption = new Vector3(0.0000003426f, 0.0000008298f, 0.000000036f);
		float ozoneHeight =  25000f;
		float ozoneFalloff = 15000f;

		RenderTexture ozoneTransmittanceRT;
		RenderTexture deltaET, deltaSRT, deltaSMT, deltaJT;
		public RenderTexture[] irradianceT, inscatterT;

		Material transmittanceMaterial, ozoneTransmittanceMaterial, irradianceNMaterial, irradiance1Material, inscatter1Material,
			inscatterNMaterial, inscatterSMaterial, copyInscatter1Material, copyInscatterNMaterial, copyIrradianceMaterial, atlasMaterial;

		int scatteringOrders = 4; //min is 2 unless you skip that step
		bool multipleScattering = true;
		bool useOzone = false;

		// Process in chunks and return if we spend more time than this
		// Otherwise windows thinks the GPU crashed on slower hardware
		// https://en.wikipedia.org/wiki/Timeout_Detection_and_Recovery
		long generationMsThreshold = 1000;

		private void InitMaterials()
		{
			transmittanceMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Transmittance")]);
			ozoneTransmittanceMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/OzoneTransmittance")]);
			irradianceNMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/IrradianceN")]);
			irradiance1Material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Irradiance1")]);
			inscatter1Material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Inscatter1")]);
			inscatterNMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/InscatterN")]);
			inscatterSMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/InscatterS")]);
			copyInscatter1Material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/CopyInscatter1")]);
			copyInscatterNMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/CopyInscatterN")]);
			copyIrradianceMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/CopyIrradiance")]);
			atlasMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/Preprocessing/Atlas")]);
		}
		
		public static float CalculateRt(float inRg, float inHR, float inHM, Vector3 in_betaR, Vector3 in_BETA_MSca, bool useOzone, float ozoneHeight, float ozoneFalloff)
		{
			float RtHR  =  - 1000f * inHR * Mathf.Log(0.0000001f / in_betaR.magnitude);
			float RtHM  =  - 1000f * inHM * Mathf.Log(0.0000001f / in_BETA_MSca.magnitude);

			float maxHeight = Mathf.Max(RtHR, RtHM);
			
			if (useOzone) maxHeight = Mathf.Max(maxHeight, 1000f * (ozoneHeight + ozoneFalloff));

			return inRg + maxHeight;
		}

		public static void DeleteCache()
		{
			string cachePath = Utils.GameDataPath + "/ScattererAtmosphereCache/PluginData";
			Directory.Delete(cachePath, true);
		}

		[StructLayout(LayoutKind.Sequential)]
		struct AtmoHashData
		{
			public float Rg;
			public float Rt;
			public float AVERAGE_GROUND_REFLECTANCE;
			public Vector4 BETA_R;
			public Vector4 BETA_MSca;
			public Vector4 ScatteringLutDimensions;

			public float MIE_G;
			public float HR;
			public float HM;

			public Vector3 ozoneAbsorption;
			public float ozoneHeight;
			public float ozoneFalloff;
			public bool useOzone;
			public bool useMultipleScattering;

			public AtmoHashData(float rg, float rt, float averageGroundReflectance, Vector4 betaR, Vector4 betaMSca, Vector4 scatteringLutDimensions, float mieG, float hr,float hm,Vector3 ozoneAbsorption,float ozoneHeight,float ozoneFalloff,bool useOzone,bool useMultipleScattering)
			{
				Rg = rg;
				Rt = rt;
				AVERAGE_GROUND_REFLECTANCE = averageGroundReflectance;
				BETA_R = betaR;
				BETA_MSca = betaMSca;
				ScatteringLutDimensions = scatteringLutDimensions;
				MIE_G = mieG;
				HR = hr;
				HM = hm;
				this.ozoneAbsorption = ozoneAbsorption;
				this.ozoneHeight = ozoneHeight;
				this.ozoneFalloff = ozoneFalloff;
				this.useOzone = useOzone;
				this.useMultipleScattering = useMultipleScattering;
			}
		}

		public static string GetAtmoHash(float inRG, float inRT, Vector4 inBETA_R, Vector4 inBETA_MSca, float inMIE_G, float inHR, float inHM, float inAverageGroundRreflectance, bool inMultiple, Vector4 inPRECOMPUTED_SCTR_LUT_DIM, bool inUseOzone, Vector4 inOzoneAbsorption, float inOzoneHeight, float inOzoneFalloff)
		{

			AtmoHashData atmoHashData = new AtmoHashData(
				inRG,
				inRT,
				inAverageGroundRreflectance,
				inBETA_R,
				inBETA_MSca,
				inPRECOMPUTED_SCTR_LUT_DIM,
				inMIE_G,
				inHR,
				inHM,
				inUseOzone ? inOzoneAbsorption : Vector4.zero,
				inUseOzone ? inOzoneHeight  : 0f,
				inUseOzone ? inOzoneFalloff : 0f,
				inUseOzone,
				inMultiple
			);

			Hash128 result = new Hash128();
			HashUtilities.ComputeHash128(ref atmoHashData, ref result);

			return result.ToString();
		}

		public static void Generate(float inRG, float inRT, Vector4 inBETA_R, Vector4 inBETA_MSca, float inMIE_G, float inHR, float inHM, float inGRref, bool inMultiple, Vector4 inScatteringLutDimensions, bool previewMode, string assetPath, bool inUseOzone, Vector4 inOzoneAbsorption, float inOzoneHeight, float inOzoneFalloff, string bodyName)
		{
			if (!atmoGenerationRunning)
			{
				Utils.LogInfo("Generating new atmosphere for "+bodyName);
				instance.StartCoroutine(instance.GenerateAndSaveAtmoCoroutine(inRG, inRT, inBETA_R, inBETA_MSca, inMIE_G, inHR, inHM, inGRref, inMultiple, inScatteringLutDimensions, previewMode, assetPath, inUseOzone, inOzoneAbsorption, inOzoneHeight, inOzoneFalloff));
			}
		}

		IEnumerator GenerateAndSaveAtmoCoroutine(float inRG, float inRT, Vector4 inBETA_R, Vector4 inBETA_MSca, float inMIE_G, float inHR, float inHM, float inGRref, bool inMultiple, Vector4 inScatteringLutDimensions, bool previewMode, string assetPath, bool inUseOzone, Vector4 inOzoneAbsorption, float inOzoneHeight, float inOzoneFalloff)
		{
			atmoGenerationRunning = true;

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
			multipleScattering = inMultiple;
			scatteringLutDimensions = inScatteringLutDimensions;

			useOzone = inUseOzone;
			ozoneAbsorption = inOzoneAbsorption * 0.001f / scaleFactor;
			if (!useOzone) ozoneAbsorption = Vector4.zero;
			ozoneHeight = inOzoneHeight * 1000f * scaleFactor;
			ozoneFalloff = inOzoneFalloff * 1000f* scaleFactor;

			if (previewMode || Scatterer.Instance.mainSettings.useLowResolutionAtmosphere)
            {
				xTiles = xTilesPreview; yTiles = yTilesPreview;
            }
			else
            {
				xTiles = xTilesDefault; yTiles = yTilesDefault;
			}

			irradianceT = new RenderTexture[2];
			inscatterT = new RenderTexture[2];
			
			ozoneTransmittanceRT = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBFloat);
			ozoneTransmittanceRT.wrapMode = TextureWrapMode.Clamp;
			ozoneTransmittanceRT.filterMode = FilterMode.Bilinear;
			ozoneTransmittanceRT.Create();
			
			irradianceT[0] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			irradianceT[0].wrapMode = TextureWrapMode.Clamp;
			irradianceT[0].filterMode = FilterMode.Bilinear;
			irradianceT[0].Create();
			
			irradianceT[1] = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			irradianceT[1].wrapMode = TextureWrapMode.Clamp;
			irradianceT[1].filterMode = FilterMode.Bilinear;
			irradianceT[1].Create();

			deltaET = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			deltaET.Create();

			inscatterT[0] = new RenderTexture((int)(scatteringLutDimensions.x * scatteringLutDimensions.y), (int)(scatteringLutDimensions.z * scatteringLutDimensions.w), 0, RenderTextureFormat.ARGBFloat);
			inscatterT[0].wrapMode = TextureWrapMode.Clamp;
			inscatterT[0].filterMode = FilterMode.Bilinear;
			inscatterT[0].Create();

			inscatterT[1] = new RenderTexture((int)(scatteringLutDimensions.x * scatteringLutDimensions.y), (int)(scatteringLutDimensions.z * scatteringLutDimensions.w), 0, RenderTextureFormat.ARGBFloat);
			inscatterT[1].wrapMode = TextureWrapMode.Clamp;
			inscatterT[1].filterMode = FilterMode.Bilinear;
			inscatterT[1].Create();

			deltaSRT = new RenderTexture((int)(scatteringLutDimensions.x * scatteringLutDimensions.y), (int)(scatteringLutDimensions.z * scatteringLutDimensions.w), 0, RenderTextureFormat.ARGBFloat);
			deltaSRT.wrapMode = TextureWrapMode.Clamp;
			deltaSRT.filterMode = FilterMode.Bilinear;
			deltaSRT.Create();

			deltaSMT = new RenderTexture((int)(scatteringLutDimensions.x * scatteringLutDimensions.y), (int)(scatteringLutDimensions.z * scatteringLutDimensions.w), 0, RenderTextureFormat.ARGBFloat);
			deltaSMT.wrapMode = TextureWrapMode.Clamp;
			deltaSMT.filterMode = FilterMode.Bilinear;
			deltaSMT.Create();

			deltaJT = new RenderTexture((int)(scatteringLutDimensions.x * scatteringLutDimensions.y), (int)(scatteringLutDimensions.z * scatteringLutDimensions.w), 0, RenderTextureFormat.ARGBFloat);
			deltaJT.wrapMode = TextureWrapMode.Clamp;
			deltaJT.filterMode = FilterMode.Bilinear;
			deltaJT.Create();

			SetParameters(ozoneTransmittanceMaterial);
			SetParameters(transmittanceMaterial);
			SetParameters(irradianceNMaterial);
			SetParameters(irradiance1Material);
			SetParameters(inscatter1Material);
			SetParameters(inscatterNMaterial);
			SetParameters(inscatterSMaterial);
			SetParameters(copyInscatter1Material);
			SetParameters(copyInscatterNMaterial);
			SetParameters(copyIrradianceMaterial);

			RTUtility.ClearColor(irradianceT);

			yield return Preprocess(assetPath);

			ReleaseTextures();

			atmoGenerationRunning = false;
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
			mat.SetVector("PRECOMPUTED_SCTR_LUT_DIM", scatteringLutDimensions);
			mat.SetVector ("tiles", new Vector2 (xTiles, yTiles));
			mat.SetFloat("ozoneHeight", ozoneHeight);
			mat.SetFloat("ozoneFalloff", ozoneFalloff);
			mat.SetVector("ozoneAbsorption", new Vector4(ozoneAbsorption.x, ozoneAbsorption.y, ozoneAbsorption.z, 0.0f));
		}

		
		IEnumerator Preprocess(string assetPath)
        {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			ComputeTransmittance();
			ComputeIrradiance();
			yield return ComputeInscatter1(stopwatch);
			CopyIrradiance();
			yield return CopyInscatter1(stopwatch);

			for (int scatteringOrder = 2; scatteringOrder <= (multipleScattering ? scatteringOrders : 2); scatteringOrder++)
			{
				yield return ComputeInscatterS(2, stopwatch); // is this a mistake? should it be scatteringOrder instead of 2?
				ComputeIrradianceN(2);
				yield return ComputeInscatterN(stopwatch);
				CopyIrradianceK1();

				if (multipleScattering)
				{
					yield return CopyInscatterN(stopwatch);
				}
			}

			if (!Directory.Exists(assetPath))
				Directory.CreateDirectory(assetPath);

			var atmosphereAtlas = PackTextures(new RenderTexture[] { inscatterT[READ], irradianceT[READ], ozoneTransmittanceRT });
			SaveAsHalf(atmosphereAtlas, assetPath + "/atlas");

			atmosphereAtlas.Release();

			Utils.LogInfo("Atmo generation successful");
        }

		private RenderTexture PackTextures(RenderTexture[] textures)
		{
			// Just do the simplest packing possible, top-left to bottom-left
			int width = textures.Max(texture => texture.width);
			int height = textures.Sum(texture => texture.height);

			RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, 0);
			rt.wrapMode = TextureWrapMode.Clamp;
			rt.Create();

			atlasMaterial.SetInt("targetWidth", width);
			atlasMaterial.SetInt("targetHeight", height);

			int currentHeight = 0;

			for (int i = 0; i < textures.Length; i++)
			{
				var texture = textures[i];

				atlasMaterial.SetTexture("inputTexture", texture);
				atlasMaterial.SetInt("width", texture.width);
				atlasMaterial.SetInt("height", texture.height);

				atlasMaterial.SetInt("horizontalOffset", 0);
				atlasMaterial.SetInt("verticalOffset", currentHeight);

				Graphics.Blit(null, rt, atlasMaterial);

				currentHeight += texture.height;
			}

			return rt;
		}

		public static List<Vector4> GetPackedTexturesScaleAndOffsets(List<Vector2> dimensions, Vector2 atlasDimensions)
		{
			var result = new List<Vector4>();
			float currentHeight = 0;

			foreach(Vector2 dimension in dimensions)
            {
				Vector2 currentScale = new Vector2(dimension.x / atlasDimensions.x, dimension.y / atlasDimensions.y);
				Vector2 currentOffset = new Vector2(0f, currentHeight /  atlasDimensions.y);

				result.Add(new Vector4(currentScale.x, currentScale.y, currentOffset.x, currentOffset.y));

				currentHeight += dimension.y;
			}

			return result;
		}

		IEnumerator CopyInscatterN(Stopwatch stopwatch)
        {
			// adds deltaS into inscatter texture S(line 11 in algorithm 4.1)
			yield return ProcessInTiles(stopwatch, (int i, int j) =>
            {
                copyInscatterNMaterial.SetVector("currentTile", new Vector2(i, j));

                copyInscatterNMaterial.SetTexture("deltaSRead", deltaSRT);
                copyInscatterNMaterial.SetTexture("inscatterRead", inscatterT[READ]);

                Graphics.Blit(null, inscatterT[WRITE], copyInscatterNMaterial, 0);
            });

            RTUtility.Swap(inscatterT);
        }

        private void CopyIrradianceK1()
        {
			// adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
			copyIrradianceMaterial.SetFloat("k", 1.0f);
            copyIrradianceMaterial.SetTexture("deltaERead", deltaET);
            copyIrradianceMaterial.SetTexture("irradianceRead", irradianceT[READ]);

            Graphics.Blit(null, irradianceT[WRITE], copyIrradianceMaterial);

            RTUtility.Swap(irradianceT);
        }

        IEnumerator ComputeInscatterN(Stopwatch stopwatch)
		{
			// computes deltaS (line 9 in algorithm 4.1)
			yield return ProcessInTiles(stopwatch, (int i, int j) =>
            {
                inscatterNMaterial.SetVector("currentTile", new Vector2(i, j));

                inscatterNMaterial.SetTexture("transmittanceRead", ozoneTransmittanceRT);
                inscatterNMaterial.SetTexture("deltaJRead", deltaJT);
                inscatterNMaterial.SetTexture("deltaJReadSampler", deltaJT);

                Graphics.Blit(null, deltaSRT, inscatterNMaterial, 0);
            });
        }

        IEnumerator ComputeInscatterS(int scatteringOrder, Stopwatch stopwatch)
        {
			yield return ProcessInTiles(stopwatch, (int i, int j) =>
            {
				inscatterSMaterial.SetVector("currentTile", new Vector2(i, j));

                inscatterSMaterial.SetInt("first", (scatteringOrder == 2) ? 1 : 0);
                inscatterSMaterial.SetTexture("transmittanceRead", ozoneTransmittanceRT);
                inscatterSMaterial.SetTexture("deltaERead", deltaET);
                inscatterSMaterial.SetTexture("deltaSRRead", deltaSRT);
                inscatterSMaterial.SetTexture("deltaSMRead", deltaSMT);

                inscatterSMaterial.SetTexture("deltaSRReadSampler", deltaSRT);
                inscatterSMaterial.SetTexture("deltaSMReadSampler", deltaSMT);

                Graphics.Blit(null, deltaJT, inscatterSMaterial, 0);
            });
        }

        private void ComputeIrradianceN(int scatteringOrder)
        {
			// computes deltaE (line 8 in algorithm 4.1)
			irradianceNMaterial.SetInt("first", (scatteringOrder == 2) ? 1 : 0);
            irradianceNMaterial.SetTexture("deltaSRRead", deltaSRT);
            irradianceNMaterial.SetTexture("deltaSMRead", deltaSMT);

            irradianceNMaterial.SetTexture("deltaSRReadSampler", deltaSRT);
            irradianceNMaterial.SetTexture("deltaSMReadSampler", deltaSMT);

            Graphics.Blit(null, deltaET, irradianceNMaterial);
        }

        IEnumerator CopyInscatter1(Stopwatch stopwatch)
		{
			// copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
			yield return ProcessInTiles(stopwatch, (int i, int j) =>
            {
                copyInscatter1Material.SetVector("currentTile", new Vector2(i, j));

                copyInscatter1Material.SetTexture("deltaSRRead", deltaSRT);
                copyInscatter1Material.SetTexture("deltaSMRead", deltaSMT);

                Graphics.Blit(null, inscatterT[WRITE], copyInscatter1Material, 0);
            });

            RTUtility.Swap(inscatterT);
        }

        private void CopyIrradiance()
        {
            copyIrradianceMaterial.SetFloat("k", 0.0f);
            copyIrradianceMaterial.SetTexture("deltaERead", deltaET);
            copyIrradianceMaterial.SetTexture("irradianceRead", irradianceT[READ]);

            Graphics.Blit(null, irradianceT[WRITE], copyIrradianceMaterial, 0);
            RTUtility.Swap(irradianceT);
        }

        IEnumerator ComputeInscatter1(Stopwatch stopwatch)
        {
			// computes single scattering texture deltaS (line 3 in algorithm 4.1)
			// Rayleigh and Mie separated in deltaSR + deltaSM
			inscatter1Material.SetTexture("transmittanceRead", ozoneTransmittanceRT);

			yield return ProcessInTiles(stopwatch, (int i, int j) =>
			{
				inscatter1Material.SetVector("currentTile", new Vector2(i, j));
				Graphics.Blit(null, deltaSRT, inscatter1Material, 0); // rayleigh pass
				Graphics.Blit(null, deltaSMT, inscatter1Material, 1); // mie pass
			});
		}

        private void ComputeIrradiance()
        {
            // computes irradiance texture deltaE (line 2 in algorithm 4.1)
            irradiance1Material.SetTexture("transmittanceRead", ozoneTransmittanceRT);
            Graphics.Blit(null, deltaET, irradiance1Material);
        }

        private void ComputeTransmittance()
        {
            Graphics.Blit(null, ozoneTransmittanceRT, ozoneTransmittanceMaterial);
        }

        // Process in tiles because older GPUs (series 7xx and integrated hd 3xxx) crash when rendering the full res
        IEnumerator ProcessInTiles(Stopwatch stopwatch, Action<int, int> process)
		{
			for (int i = 0; i < xTiles; i++)
			{
				for (int j = 0; j < yTiles; j++)
				{
					process(i, j);
				}

				if (stopwatch.ElapsedMilliseconds > generationMsThreshold)
				{
					yield return new WaitForFixedUpdate(); stopwatch.Restart();
				}
			}
		}

		public void OnDestroy()
        {
			ReleaseTextures();
		}

		private void ReleaseRT(RenderTexture rt)
		{
			if (rt != null)
				rt.Release();
		}

		void ReleaseTextures()
		{
            ReleaseRT(ozoneTransmittanceRT);

            if (irradianceT != null)
            { 
                ReleaseRT(irradianceT[0]);
                ReleaseRT(irradianceT[1]);
            }

            if (inscatterT != null)
            {
                ReleaseRT(inscatterT[0]);
                ReleaseRT(inscatterT[1]);
            }

            ReleaseRT(deltaET);
            ReleaseRT(deltaSRT);
            ReleaseRT(deltaSMT);
            ReleaseRT(deltaJT);
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