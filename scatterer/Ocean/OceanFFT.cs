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
 * 
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Threading;



namespace scatterer {
	/*
	 * Extend the base class OceanNode to provide the data need 
	 * to create the waves using fourier transform which can then be applied
	 * to the projected grid handled by the OceanNode.
	 * All the fourier transforms are performed on the GPU
	 */
	public class OceanFFT: OceanNode {
		
		WriteFloat m_writeFloat;
		Vector2 m_varianceMax;
		
		//		//CONST DONT CHANGE
		//		const float WAVE_CM = 0.23f;	// Eq 59
		//		const float WAVE_KM = 370.0f;	// Eq 59
		//		const float AMP = 1.0f;
		
		[Persistent] public float WAVE_CM = 0.23f; // Eq 59
		[Persistent] public float WAVE_KM = 370.0f; // Eq 59
		[Persistent] public float AMP = 1.0f;
		
		Material m_initSpectrumMat;
		
		Material m_initDisplacementMat;

		
		[Persistent] public int m_ansio = 2;
		
		//A higher wind speed gives greater swell to the waves
		[Persistent] public float m_windSpeed = 5.0f;
		
		//A lower number means the waves last longer and will build up larger waves
		[Persistent] public float m_omega = 0.84f;
		
		//Size in meters (i.e. in spatial domain) of each grid
//		[Persistent] 
		public Vector4 m_gridSizes = new Vector4(5488, 392, 28, 2);
		
		//strenght of sideways displacement for each grid
//		[Persistent]
		public Vector4 m_choppyness = new Vector4(2.3f, 2.1f, 1.3f, 0.9f);
		
		//This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.
		//		[SerializeField]
		//		int m_fourierGridSize = 256;
		[Persistent] public int m_fourierGridSize = 128;
		
		//int m_varianceSize = 16;
		[Persistent] public int m_varianceSize = 4;
		
		float m_fsize;
		float m_maxSlopeVariance;
		protected int m_idx = 0;
		protected Vector4 m_offset;
		protected Vector4 m_inverseGridSizes;
		
		protected RenderTexture m_spectrum01, m_spectrum23;
		protected RenderTexture m_WTable;
		RenderTexture[] m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2;
		RenderTexture[] m_fourierBuffer3, m_fourierBuffer4;
		RenderTexture m_map0, m_map1, m_map2, m_map3, m_map4;
		
		
		//		RenderTexture m_variance;
		
		
		public Texture3D variance {
			get {
				return m_variance;
			}
		}
		Texture3D m_variance;
		
		protected FourierGPU m_fourier;
		
		public override float GetMaxSlopeVariance() {
			return m_maxSlopeVariance;
		}


		//CPU wave stuff
		float FFTtimer=0;

		volatile int m_bufferIdx = 0;
		public volatile bool done1=true,done2=true,done3=true,done4=true,done5=true;
		public volatile bool done=true;

		int m_passes;
		float[] m_butterflyLookupTable = null;

		
		Vector4[,] m_fourierBuffer0vector;
		Vector4[,] m_fourierBuffer1vector, m_fourierBuffer2vector,m_fourierBuffer3vector,m_fourierBuffer4vector;

		Vector4[] m_spectrum01vector, m_spectrum23vector, m_WTablevector;

//		volatile Vector4[,] m_fourierBuffer0vectorResults, m_fourierBuffer3vectorResults,m_fourierBuffer4vectorResults;
		Vector4[,] m_fourierBuffer0vectorResults, m_fourierBuffer3vectorResults,m_fourierBuffer4vectorResults;

		protected FourierCPU m_CPUfourier;

		
		// Use this for initialization
		public override void Start()
			//		public void Start () 
		{
			base.Start();
			
			
			m_initSpectrumMat = new Material(ShaderTool.GetMatFromShader2("CompiledInitSpectrum.shader"));;
			m_initDisplacementMat = new Material(ShaderTool.GetMatFromShader2("CompiledInitDisplacement.shader"));;
			
			
			if (m_fourierGridSize > 256) {
				Debug.Log("Proland::OceanFFT::Start	- fourier grid size must not be greater than 256, changing to 256");
				m_fourierGridSize = 256;
			}
			
			if (!Mathf.IsPowerOfTwo(m_fourierGridSize)) {
				Debug.Log("Proland::OceanFFT::Start	- fourier grid size must be pow2 number, changing to nearest pow2 number");
				m_fourierGridSize = Mathf.NextPowerOfTwo(m_fourierGridSize);
			}
			
			m_fsize = (float) m_fourierGridSize;
			m_offset = new Vector4(1.0f + 0.5f / m_fsize, 1.0f + 0.5f / m_fsize, 0, 0);
			
			
			float factor = 2.0f * Mathf.PI * m_fsize;
			m_inverseGridSizes = new Vector4(factor / m_gridSizes.x, factor / m_gridSizes.y, factor / m_gridSizes.z, factor / m_gridSizes.w);
			
			
			m_fourier = new FourierGPU(m_fourierGridSize);
			
			m_writeFloat = new WriteFloat(m_fourierGridSize, m_fourierGridSize);


//#if CPUmode
			if (m_manager.GetCore ().craft_WaveInteractions)
			{
				m_CPUfourier = new FourierCPU (m_fourierGridSize);
				
				m_passes = (int)(Mathf.Log (m_fsize) / Mathf.Log (2.0f));
				ComputeButterflyLookupTable ();
				
				m_CPUfourier.m_butterflyLookupTable = m_butterflyLookupTable;
				
				m_fourierBuffer0vector = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				m_fourierBuffer0vectorResults = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				
				
				m_fourierBuffer1vector = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				m_fourierBuffer2vector = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				
				m_fourierBuffer3vector = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				m_fourierBuffer4vector = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				m_fourierBuffer3vectorResults = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
				m_fourierBuffer4vectorResults = new Vector4[2, m_fourierGridSize * m_fourierGridSize];
			}
//#endif			


			//Create the data needed to make the waves each frame
			CreateRenderTextures();
			GenerateWavesSpectrum();
			CreateWTable();
			
			m_initSpectrumMat.SetTexture("_Spectrum01", m_spectrum01);
			m_initSpectrumMat.SetTexture("_Spectrum23", m_spectrum23);
			m_initSpectrumMat.SetTexture("_WTable", m_WTable);
			m_initSpectrumMat.SetVector("_Offset", m_offset);
			m_initSpectrumMat.SetVector("_InverseGridSizes", m_inverseGridSizes);
			
			m_initDisplacementMat.SetVector("_InverseGridSizes", m_inverseGridSizes);

//#if CPUmode
			if (m_manager.GetCore ().craft_WaveInteractions)
			{
				CreateWTableForCPU ();
			}
			
//#endif
			
		}
		
		Vector2 GetSlopeVariances(Vector2 k, float A, float B, float C, float spectrumX, float spectrumY) {
			float w = 1.0f - Mathf.Exp(A * k.x * k.x + B * k.x * k.y + C * k.y * k.y);
			return new Vector2((k.x * k.x) * w, (k.y * k.y) * w) * (spectrumX * spectrumX + spectrumY * spectrumY) * 2.0f;
		}
		
		
		
		/// <summary>
		/// Iterate over the spectrum and find the variance.
		/// Use in the BRDF equations.
		/// </summary>
		Vector2 ComputeVariance(float slopeVarianceDelta, float[] inSpectrum01, float[] inSpectrum23, float idxX, float idxY, float idxZ) {
			const float SCALE = 10.0f;
			
			float A = Mathf.Pow(idxX / ((float) m_varianceSize - 1.0f), 4.0f) * SCALE;
			float C = Mathf.Pow(idxZ / ((float) m_varianceSize - 1.0f), 4.0f) * SCALE;
			float B = (2.0f * idxY / ((float) m_varianceSize - 1.0f) - 1.0f) * Mathf.Sqrt(A * C);
			A = -0.5f * A;
			B = -B;
			C = -0.5f * C;
			
			Vector2 slopeVariances = new Vector2(slopeVarianceDelta, slopeVarianceDelta);
			
			for (int x = 0; x < m_fourierGridSize; x++) {
				for (int y = 0; y < m_fourierGridSize; y++) {
					int i = x >= m_fsize / 2.0f ? x - m_fourierGridSize : x;
					int j = y >= m_fsize / 2.0f ? y - m_fourierGridSize : y;
					
					Vector2 k = new Vector2(i, j) * 2.0f * Mathf.PI;
					
					slopeVariances += GetSlopeVariances(k / m_gridSizes.x, A, B, C, inSpectrum01[(x + y * m_fourierGridSize) * 4 + 0], inSpectrum01[(x + y * m_fourierGridSize) * 4 + 1]);
					slopeVariances += GetSlopeVariances(k / m_gridSizes.y, A, B, C, inSpectrum01[(x + y * m_fourierGridSize) * 4 + 2], inSpectrum01[(x + y * m_fourierGridSize) * 4 + 3]);
					slopeVariances += GetSlopeVariances(k / m_gridSizes.z, A, B, C, inSpectrum23[(x + y * m_fourierGridSize) * 4 + 0], inSpectrum23[(x + y * m_fourierGridSize) * 4 + 1]);
					slopeVariances += GetSlopeVariances(k / m_gridSizes.w, A, B, C, inSpectrum23[(x + y * m_fourierGridSize) * 4 + 2], inSpectrum23[(x + y * m_fourierGridSize) * 4 + 3]);
				}
			}
			
			return slopeVariances;
		}
		
		
		/*
		 * Initializes the data to the shader that needs to 
		 * have the fourier transform applied to it this frame.
		 */
		protected virtual void InitWaveSpectrum(float t) {
			// init heights (0) and slopes (1,2)
			RenderTexture[] buffers012 = new RenderTexture[] {
				m_fourierBuffer0[1], m_fourierBuffer1[1], m_fourierBuffer2[1]
			};
			m_initSpectrumMat.SetFloat("_T", t);
			//			RTUtility.MultiTargetBlit(buffers012, m_initSpectrumMat);
			RTUtility.MultiTargetBlit(buffers012, m_initSpectrumMat, 0);
			
			// Init displacement (3,4)
			RenderTexture[] buffers34 = new RenderTexture[] {
				m_fourierBuffer3[1], m_fourierBuffer4[1]
			};
			m_initDisplacementMat.SetTexture("_Buffer1", m_fourierBuffer1[1]);
			m_initDisplacementMat.SetTexture("_Buffer2", m_fourierBuffer2[1]);
			//			RTUtility.MultiTargetBlit(buffers34, m_initDisplacementMat);
			RTUtility.MultiTargetBlit(buffers34, m_initDisplacementMat, 0);
		}


		void InitWaveSpectrumCPU(float time)
		{
			Vector2 uv, st, k1, k2, k3, k4, h1, h2, h3, h4, h12, h34, n1, n2, n3, n4;
			Vector4 s12, s34, s12c, s34c;
			int rx, ry;
			
			// init heights (0) and slopes (1,2)
			for (int x = 0; x < m_fourierGridSize; x++)
			{
				for (int y = 0; y < m_fourierGridSize; y++)
				{
					uv.x = x / m_fsize;
					uv.y = y / m_fsize;
					
					st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
					st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;
					
					rx = x;
					ry = y;
					
					s12 = m_spectrum01vector[rx + ry * m_fourierGridSize];
					s34 = m_spectrum23vector[rx + ry * m_fourierGridSize];
					
					rx = (m_fourierGridSize - x) % m_fourierGridSize;
					ry = (m_fourierGridSize - y) % m_fourierGridSize;
					
					s12c = m_spectrum01vector[rx + ry * m_fourierGridSize];
					s34c = m_spectrum23vector[rx + ry * m_fourierGridSize];
					
					k1 = st * m_inverseGridSizes.x;
					k2 = st * m_inverseGridSizes.y;
					k3 = st * m_inverseGridSizes.z;
					k4 = st * m_inverseGridSizes.w;
					
					
					h1 = GetSpectrum(time, m_WTablevector[x + y * m_fourierGridSize].x, s12.x, s12.y, s12c.x, s12c.y);
					h2 = GetSpectrum(time, m_WTablevector[x + y * m_fourierGridSize].y, s12.z, s12.w, s12c.z, s12c.w);
					h3 = GetSpectrum(time, m_WTablevector[x + y * m_fourierGridSize].z, s34.x, s34.y, s34c.x, s34c.y);
					h4 = GetSpectrum(time, m_WTablevector[x + y * m_fourierGridSize].w, s34.z, s34.w, s34c.z, s34c.w);
					
					//heights
					h12 = h1 + COMPLEX(h2);
					h34 = h3 + COMPLEX(h4);
					
					//slopes (normals)
					n1 = COMPLEX(k1.x * h1) - k1.y * h1;
					n2 = COMPLEX(k2.x * h2) - k2.y * h2;
					n3 = COMPLEX(k3.x * h3) - k3.y * h3;
					n4 = COMPLEX(k4.x * h4) - k4.y * h4;
					
					//Heights in last two channels (h34) have been removed as I found they arent really need for the shader
					//h3 and h4 still needs to be calculated for the slope but they are no longer save and transformed by the fourier step
					//m_fourierBuffer0vector[1, x+y*m_fourierGridSize] = new Vector4(h12.x, h12.y, h34.x, h34.y); //I put this back
					
					int i = x + y * m_fourierGridSize;
					
					//                    m_fourierBuffer0vector[1, i] = h12;
					m_fourierBuffer0vector[1, i] = new Vector4(h12.x, h12.y, h34.x, h34.y);
					m_fourierBuffer1vector[1, i] = new Vector4(n1.x, n1.y, n2.x, n2.y);
					m_fourierBuffer2vector[1, i] = new Vector4(n3.x, n3.y, n4.x, n4.y);
					
					
					// Init displacement (3,4)
					
					
					float K1 = (k1).magnitude;
					float K2 = (k2).magnitude;
					float K3 = (k3).magnitude;
					float K4 = (k4).magnitude;
					
					float IK1 = K1 == 0.0f ? 0.0f : 1.0f / K1;
					float IK2 = K2 == 0.0f ? 0.0f : 1.0f / K2;
					float IK3 = K3 == 0.0f ? 0.0f : 1.0f / K3;
					float IK4 = K4 == 0.0f ? 0.0f : 1.0f / K4;
					
					
					Vector4 result = new Vector4(0f,0f,0f,0f);
					
					
					result.x=m_fourierBuffer1vector[1,i].x * IK1;
					result.y=m_fourierBuffer1vector[1,i].y * IK1;
					result.z=m_fourierBuffer1vector[1,i].z * IK2;
					result.w=m_fourierBuffer1vector[1,i].w * IK2;
					
					m_fourierBuffer3vector[1, i] = result;
					
					Vector4 result2 = new Vector4(0f,0f,0f,0f);
					result.x=m_fourierBuffer2vector[1,i].x * IK3;
					result.y=m_fourierBuffer2vector[1,i].y * IK3;
					result.z=m_fourierBuffer2vector[1,i].z * IK4;
					result.w=m_fourierBuffer2vector[1,i].w * IK4;
					
					m_fourierBuffer4vector[1, i] = result2;
				}
			}
		}
		
		public override void UpdateNode() {
			
			//			if (!m_spectrum01.IsCreated()) {
			//				waitBeforeReloadCnt++;
			//				if (waitBeforeReloadCnt >= 2) {
			//					
			//					CreateRenderTextures();
			//					GenerateWavesSpectrum();
			//					CreateWTable();
			//
			//					Debug.Log("[Scatterer] Recreated OceanFFT Data");
			//					waitBeforeReloadCnt = 0;
			//				}
			//			}
			//
			//			else 
			{
				float t;
				if (TimeWarp.CurrentRate > 4)
				{
					t = (float) Planetarium.GetUniversalTime();
				}
				else
				{
					t =	Time.time;
				}

//				t =	Time.realtimeSinceStartup;

				InitWaveSpectrum(t);
				
				//Perform fourier transform and record what is the current index
				m_idx = m_fourier.PeformFFT(m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2);
				m_fourier.PeformFFT(m_fourierBuffer3, m_fourierBuffer4);
				
				//Copy the contents of the completed fourier transform to the map textures.
				//You could just use the buffer textures (m_fourierBuffer0,1,2,etc) to read from for the ocean shader 
				//but they need to have mipmaps and unity updates the mipmaps
				//every time the texture is renderer into. This impacts performance during fourier transform stage as mipmaps would be updated every pass
				//and there is no way to disable and then enable mipmaps on render textures in Unity at time of writting.
				
				Graphics.Blit(m_fourierBuffer0[m_idx], m_map0);
				Graphics.Blit(m_fourierBuffer1[m_idx], m_map1);
				Graphics.Blit(m_fourierBuffer2[m_idx], m_map2);
				Graphics.Blit(m_fourierBuffer3[m_idx], m_map3);
				Graphics.Blit(m_fourierBuffer4[m_idx], m_map4);
				
				
				
				m_oceanMaterialNear.SetVector("_Ocean_MapSize", new Vector2(m_fsize, m_fsize));
				m_oceanMaterialNear.SetVector("_Ocean_Choppyness", m_choppyness);
				m_oceanMaterialNear.SetVector("_Ocean_GridSizes", m_gridSizes);
//				m_oceanMaterialNear.SetFloat("_Ocean_HeightOffset", m_oceanLevel);
				m_oceanMaterialNear.SetFloat("_Ocean_HeightOffset", 0f);
				m_oceanMaterialNear.SetTexture("_Ocean_Variance", m_variance);
				m_oceanMaterialNear.SetTexture("_Ocean_Map0", m_map0);
				m_oceanMaterialNear.SetTexture("_Ocean_Map1", m_map1);
				m_oceanMaterialNear.SetTexture("_Ocean_Map2", m_map2);
				m_oceanMaterialNear.SetTexture("_Ocean_Map3", m_map3);
				m_oceanMaterialNear.SetTexture("_Ocean_Map4", m_map4);
				m_oceanMaterialNear.SetVector("_VarianceMax", m_varianceMax);
				
				m_oceanMaterialFar.SetVector("_Ocean_MapSize", new Vector2(m_fsize, m_fsize));
				m_oceanMaterialFar.SetVector("_Ocean_Choppyness", m_choppyness);
				m_oceanMaterialFar.SetVector("_Ocean_GridSizes", m_gridSizes);
//				m_oceanMaterialFar.SetFloat("_Ocean_HeightOffset", m_oceanLevel);
				m_oceanMaterialFar.SetFloat("_Ocean_HeightOffset", 0f);
				m_oceanMaterialFar.SetTexture("_Ocean_Variance", m_variance);
				m_oceanMaterialFar.SetTexture("_Ocean_Map0", m_map0);
				m_oceanMaterialFar.SetTexture("_Ocean_Map1", m_map1);
				m_oceanMaterialFar.SetTexture("_Ocean_Map2", m_map2);
				m_oceanMaterialFar.SetTexture("_Ocean_Map3", m_map3);
				m_oceanMaterialFar.SetTexture("_Ocean_Map4", m_map4);
				m_oceanMaterialFar.SetVector("_VarianceMax", m_varianceMax);

//#if !CPUmode
				//Make sure base class get updated as well
//				base.UpdateNode();
//#else
				if (m_manager.GetCore ().craft_WaveInteractions)
				{
					if(!(done1&&done2&&done3&&done4&&done5))
					{
						base.UpdateNode();
						return;
					}
					
					done1 = false;
					done2 = false;
					done3 = false;
					done4 = false;
					done5 = false;
					
					Debug.Log ("[Scatterer] FFT time " + (Time.realtimeSinceStartup - FFTtimer).ToString ());
					FFTtimer = Time.realtimeSinceStartup;
					
					//				Nullable<float> time = Time.realtimeSinceStartup;
					
					Nullable<float> time = t;
					
					ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded1), time);
					CommitResults (ref m_fourierBuffer0vector, ref m_fourierBuffer0vectorResults);
					CommitResults (ref m_fourierBuffer3vector, ref m_fourierBuffer3vectorResults);
					CommitResults (ref m_fourierBuffer4vector, ref m_fourierBuffer4vectorResults);
				}



				base.UpdateNode();


				if (m_manager.GetCore ().craft_WaveInteractions)
				{
					PartBuoyancy[] parts = (PartBuoyancy[])PartBuoyancy.FindObjectsOfType (typeof(PartBuoyancy));
					foreach (PartBuoyancy _part in parts)
					{
						//				_part.transform
						Vector3 relativePartPos = _part.transform.position-m_manager.GetCore ().farCamera.transform.position;
						
						//					Debug.Log("new ocean level: "+ (m_oceanLevel+ SampleHeight(relativePartPos)).ToString());
						
						_part.waterLevel=m_oceanLevel+ SampleHeight(new Vector3(Vector3.Dot(relativePartPos,ux.ToVector3()),Vector3.Dot(relativePartPos,uy.ToVector3()),0f));
						//						_part.waterLevel=m_oceanLevel;
					}
				
				}
//#endif

			}
		}
		
		public override void OnDestroy()
			//		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			m_map0.Release();
			m_map1.Release();
			m_map2.Release();
			m_map3.Release();
			m_map4.Release();
			
			m_spectrum01.Release();
			m_spectrum23.Release();
			
			m_WTable.Release();
			//m_variance.Release();
			//			Destroy (m_variance);
			
			for (int i = 0; i < 2; i++) {
				m_fourierBuffer0[i].Release();
				m_fourierBuffer1[i].Release();
				m_fourierBuffer2[i].Release();
				m_fourierBuffer3[i].Release();
				m_fourierBuffer4[i].Release();
			}
		}
		
		protected virtual void CreateRenderTextures() {
			
			RenderTextureFormat mapFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
			
			//These texture hold the actual data use in the ocean renderer
			CreateMap(ref m_map0, mapFormat, m_ansio);
			CreateMap(ref m_map1, mapFormat, m_ansio);
			CreateMap(ref m_map2, mapFormat, m_ansio);
			CreateMap(ref m_map3, mapFormat, m_ansio);
			CreateMap(ref m_map4, mapFormat, m_ansio);
			
			//These textures are used to perform the fourier transform
			CreateBuffer(ref m_fourierBuffer0, format); //heights
			CreateBuffer(ref m_fourierBuffer1, format); // slopes X
			CreateBuffer(ref m_fourierBuffer2, format); // slopes Y
			CreateBuffer(ref m_fourierBuffer3, format); // displacement X
			CreateBuffer(ref m_fourierBuffer4, format); // displacement Y
			
			//These textures hold the specturm the fourier transform is performed on
			m_spectrum01 = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
			m_spectrum01.filterMode = FilterMode.Point;
			m_spectrum01.wrapMode = TextureWrapMode.Repeat;
			m_spectrum01.enableRandomWrite = true;
			m_spectrum01.Create();
			
			m_spectrum23 = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
			m_spectrum23.filterMode = FilterMode.Point;
			m_spectrum23.wrapMode = TextureWrapMode.Repeat;
			m_spectrum23.enableRandomWrite = true;
			m_spectrum23.Create();
			

			m_WTable = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
			m_WTable.filterMode = FilterMode.Point;
			m_WTable.wrapMode = TextureWrapMode.Clamp;
			m_WTable.enableRandomWrite = true;
			m_WTable.Create();
			
			//			m_variance = new RenderTexture(m_varianceSize, m_varianceSize, 0, RenderTextureFormat.RHalf);
			//			m_variance.volumeDepth = m_varianceSize;
			//			m_variance.wrapMode = TextureWrapMode.Clamp;
			//			m_variance.filterMode = FilterMode.Bilinear;
			//			m_variance.isVolume = true;
			//			m_variance.enableRandomWrite = true;
			//			m_variance.useMipMap = true;
			//			m_variance.Create();
			
			
			m_variance = new Texture3D(m_varianceSize, m_varianceSize, m_varianceSize, TextureFormat.ARGB32, true);
			
			m_variance.wrapMode = TextureWrapMode.Clamp;
			m_variance.filterMode = FilterMode.Bilinear;
			
		}
		
		protected void CreateBuffer(ref RenderTexture[] tex, RenderTextureFormat format) {
			tex = new RenderTexture[2];
			
			for (int i = 0; i < 2; i++) {
				tex[i] = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
				tex[i].filterMode = FilterMode.Point;
				tex[i].wrapMode = TextureWrapMode.Clamp;
				tex[i].Create();
			}
		}
		
		protected void CreateMap(ref RenderTexture map, RenderTextureFormat format, int ansio) {
			map = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
			map.filterMode = FilterMode.Trilinear;
			map.wrapMode = TextureWrapMode.Repeat;
			map.anisoLevel = ansio;
			map.useMipMap = true;
			map.Create();
		}
		
		float sqr(float x) {
			return x * x;
		}
		
		float omega(float k) {
			return Mathf.Sqrt(9.81f * k * (1.0f + sqr(k / WAVE_KM)));
		} // Eq 24
		
		float Spectrum(float kx, float ky, bool omnispectrum) {
			//I know this is a big chunk of ugly math but dont worry to much about what it all means
			//It recreates a statistcally representative model of a wave spectrum in the frequency domain.
			
			float U10 = m_windSpeed;
			
			// phase speed
			float k = Mathf.Sqrt(kx * kx + ky * ky);
			float c = omega(k) / k;
			
			// spectral peak
			float kp = 9.81f * sqr(m_omega / U10); // after Eq 3
			float cp = omega(kp) / kp;
			
			// friction velocity
			float z0 = 3.7e-5f * sqr(U10) / 9.81f * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
			float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60
			
			float Lpm = Mathf.Exp(-5.0f / 4.0f * sqr(kp / k)); // after Eq 3
			float gamma = (m_omega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(m_omega); // after Eq 3 // log10 or log?
			float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(m_omega, 3.0f)); // after Eq 3
			float Gamma = Mathf.Exp(-1.0f / (2.0f * sqr(sigma)) * sqr(Mathf.Sqrt(k / kp) - 1.0f));
			float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
			float Fp = Lpm * Jp * Mathf.Exp(-m_omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
			float alphap = 0.006f * Mathf.Sqrt(m_omega); // Eq 34
			float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31
			
			float alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
			float Fm = Mathf.Exp(-0.25f * sqr(k / WAVE_KM - 1.0f)); // Eq 41
			float Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)
			
			Bh *= Lpm; // bug fix???
			
			if (omnispectrum) return AMP * (Bl + Bh) / (k * sqr(k)); // Eq 30
			
			float a0 = Mathf.Log(2.0f) / 4.0f;
			float ap = 4.0f;
			float am = 0.13f * u_star / WAVE_CM; // Eq 59
			float Delta = (float) System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / c, 2.5f)); // Eq 57
			
			float phi = Mathf.Atan2(ky, kx);
			
			if (kx < 0.0f) return 0.0f;
			
			Bl *= 2.0f;
			Bh *= 2.0f;
			
			// remove waves perpendicular to wind dir
			float tweak = Mathf.Sqrt(Mathf.Max(kx / Mathf.Sqrt(kx * kx + ky * ky), 0.0f));
			
			return AMP * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * sqr(sqr(k))) * tweak; // Eq 67
		}
		
		Vector2 GetSpectrumSample(float i, float j, float lengthScale, float kMin) {
			float dk = 2.0f * Mathf.PI / lengthScale;
			float kx = i * dk;
			float ky = j * dk;
			Vector2 result = new Vector2(0.0f, 0.0f);
			
			float rnd = UnityEngine.Random.value;
			
			if (Mathf.Abs(kx) >= kMin || Mathf.Abs(ky) >= kMin) {
				float S = Spectrum(kx, ky, false);
				float h = Mathf.Sqrt(S / 2.0f) * dk;
				
				float phi = rnd * 2.0f * Mathf.PI;
				result.x = h * Mathf.Cos(phi);
				result.y = h * Mathf.Sin(phi);
			}
			
			return result;
		}
		
		float GetSlopeVariance(float kx, float ky, Vector2 spectrumSample) {
			float kSquare = kx * kx + ky * ky;
			float real = spectrumSample.x;
			float img = spectrumSample.y;
			float hSquare = real * real + img * img;
			return kSquare * hSquare * 2.0f;
		}
		
		void GenerateWavesSpectrum() {
			
			// Slope variance due to all waves, by integrating over the full spectrum.
			// Used by the BRDF rendering model
			float theoreticSlopeVariance = 0.0f;
			float k = 5e-3f;
			while (k < 1e3f) {
				float nextK = k * 1.001f;
				theoreticSlopeVariance += k * k * Spectrum(k, 0, true) * (nextK - k);
				k = nextK;
			}
			
			float[] spectrum01 = new float[m_fourierGridSize * m_fourierGridSize * 4];
			float[] spectrum23 = new float[m_fourierGridSize * m_fourierGridSize * 4];

//#if CPUmode
			if (m_manager.GetCore ().craft_WaveInteractions)
			{
				m_spectrum01vector = new Vector4[m_fourierGridSize * m_fourierGridSize];
				m_spectrum23vector = new Vector4[m_fourierGridSize * m_fourierGridSize];
			}
//#endif


			int idx;
			float i;
			float j;
			float totalSlopeVariance = 0.0f;
			Vector2 sample12XY;
			Vector2 sample12ZW;
			Vector2 sample34XY;
			Vector2 sample34ZW;
			
			UnityEngine.Random.seed = 0;
			
			for (int x = 0; x < m_fourierGridSize; x++) {
				for (int y = 0; y < m_fourierGridSize; y++) {
					idx = x + y * m_fourierGridSize;
					i = (x >= m_fourierGridSize / 2) ? (float)(x - m_fourierGridSize) : (float) x;
					j = (y >= m_fourierGridSize / 2) ? (float)(y - m_fourierGridSize) : (float) y;
					
					sample12XY = GetSpectrumSample(i, j, m_gridSizes.x, Mathf.PI / m_gridSizes.x);
					sample12ZW = GetSpectrumSample(i, j, m_gridSizes.y, Mathf.PI * m_fsize / m_gridSizes.x);
					sample34XY = GetSpectrumSample(i, j, m_gridSizes.z, Mathf.PI * m_fsize / m_gridSizes.y);
					sample34ZW = GetSpectrumSample(i, j, m_gridSizes.w, Mathf.PI * m_fsize / m_gridSizes.z);
					

//#if CPUmode
					if (m_manager.GetCore ().craft_WaveInteractions)
					{
						m_spectrum01vector[idx].x = sample12XY.x;
						m_spectrum01vector[idx].y = sample12XY.y;
						m_spectrum01vector[idx].z = sample12ZW.x;
						m_spectrum01vector[idx].w = sample12ZW.y;
						
						m_spectrum23vector[idx].x = sample34XY.x;
						m_spectrum23vector[idx].y = sample34XY.y;
						m_spectrum23vector[idx].z = sample34ZW.x;
						m_spectrum23vector[idx].w = sample34ZW.y;
					}
//#endif



					spectrum01[idx * 4 + 0] = sample12XY.x;
					spectrum01[idx * 4 + 1] = sample12XY.y;
					spectrum01[idx * 4 + 2] = sample12ZW.x;
					spectrum01[idx * 4 + 3] = sample12ZW.y;
					
					spectrum23[idx * 4 + 0] = sample34XY.x;
					spectrum23[idx * 4 + 1] = sample34XY.y;
					spectrum23[idx * 4 + 2] = sample34ZW.x;
					spectrum23[idx * 4 + 3] = sample34ZW.y;
					
					i *= 2.0f * Mathf.PI;
					j *= 2.0f * Mathf.PI;
					
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.x, j / m_gridSizes.x, sample12XY);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.y, j / m_gridSizes.y, sample12ZW);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.z, j / m_gridSizes.z, sample34XY);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.w, j / m_gridSizes.w, sample34ZW);
				}
			}
			
			//Write floating point data into render texture
//			ComputeBuffer buffer = new ComputeBuffer(m_fourierGridSize * m_fourierGridSize, sizeof(float) * 4);
			
			//			buffer.SetData(spectrum01);
			//			CBUtility.WriteIntoRenderTexture(m_spectrum01, 4, buffer, m_manager.GetWriteData());
			
			//			buffer.SetData(spectrum23);
			//			CBUtility.WriteIntoRenderTexture(m_spectrum23, 4, buffer, m_manager.GetWriteData());
			
			m_writeFloat.WriteIntoRenderTexture(m_spectrum01, 4, spectrum01);
			m_writeFloat.WriteIntoRenderTexture(m_spectrum23, 4, spectrum23);
			
			
			//			buffer.Release();
			
			//			m_varianceShader.SetFloat("_SlopeVarianceDelta", 0.5f * (theoreticSlopeVariance - totalSlopeVariance));
			//			m_varianceShader.SetFloat("_VarianceSize", (float)m_varianceSize);
			//			m_varianceShader.SetFloat("_Size", m_fsize);
			//			m_varianceShader.SetVector("_GridSizes", m_gridSizes);
			//			m_varianceShader.SetTexture(0, "_Spectrum01", m_spectrum01);
			//			m_varianceShader.SetTexture(0, "_Spectrum23", m_spectrum23);
			//			m_varianceShader.SetTexture(0, "des", m_variance);
			//			
			//			m_varianceShader.Dispatch(0,m_varianceSize/4,m_varianceSize/4,m_varianceSize/4);
			
			//			Find the maximum value for slope variance
			
			//			buffer = new ComputeBuffer(m_varianceSize*m_varianceSize*m_varianceSize, sizeof(float));
			//			CBUtility.ReadFromRenderTexture(m_variance, 1, buffer, m_manager.GetReadData());
			
			
			
			
			//			Compute variance for the BRDF 
			//			copied from the dx9 project
			//			Crashes everything
			
			float slopeVarianceDelta = 0.5f * (theoreticSlopeVariance - totalSlopeVariance);
			//			
			m_varianceMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
			
			Vector2[, , ] variance32bit = new Vector2[m_varianceSize, m_varianceSize, m_varianceSize];
			Color[] variance8bit = new Color[m_varianceSize * m_varianceSize * m_varianceSize];
			//			
			for (int x = 0; x < m_varianceSize; x++) {
				for (int y = 0; y < m_varianceSize; y++) {
					for (int z = 0; z < m_varianceSize; z++) {
						variance32bit[x, y, z] = ComputeVariance(slopeVarianceDelta, spectrum01, spectrum23, x, y, z);
						//problematic line
						
						if (variance32bit[x, y, z].x > m_varianceMax.x) m_varianceMax.x = variance32bit[x, y, z].x;
						if (variance32bit[x, y, z].y > m_varianceMax.y) m_varianceMax.y = variance32bit[x, y, z].y;
					}
				}
			}
			
			for (int x = 0; x < m_varianceSize; x++) {
				for (int y = 0; y < m_varianceSize; y++) {
					for (int z = 0; z < m_varianceSize; z++) {
						idx = x + y * m_varianceSize + z * m_varianceSize * m_varianceSize;
						
						variance8bit[idx] = new Color(variance32bit[x, y, z].x / m_varianceMax.x, variance32bit[x, y, z].y / m_varianceMax.y, 0.0f, 1.0f);
					}
				}
			}
			
			m_variance.SetPixels(variance8bit);
			m_variance.Apply();
			
			
			float[] varianceData = new float[m_varianceSize * m_varianceSize * m_varianceSize];
			//			buffer.GetData(varianceData);
			
			//			EncodeFloat.ReadFromRenderTexture (m_variance, 1, varianceData);
			
			m_maxSlopeVariance = 0.0f;
			for (int v = 0; v < m_varianceSize * m_varianceSize * m_varianceSize; v++) {
				m_maxSlopeVariance = Mathf.Max(m_maxSlopeVariance, varianceData[v]);
			}
			
			//			buffer.Release();
			
		}
		
		void CreateWTable() {
			//Some values need for the InitWaveSpectrum function can be precomputed
			Vector2 uv, st;
			float k1, k2, k3, k4, w1, w2, w3, w4;
			
			float[] table = new float[m_fourierGridSize * m_fourierGridSize * 4];
			
			for (int x = 0; x < m_fourierGridSize; x++) {
				for (int y = 0; y < m_fourierGridSize; y++) {
					uv = new Vector2(x, y) / m_fsize;
					
					st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
					st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;
					
					k1 = (st * m_inverseGridSizes.x).magnitude;
					k2 = (st * m_inverseGridSizes.y).magnitude;
					k3 = (st * m_inverseGridSizes.z).magnitude;
					k4 = (st * m_inverseGridSizes.w).magnitude;
					
					w1 = Mathf.Sqrt(9.81f * k1 * (1.0f + k1 * k1 / (WAVE_KM * WAVE_KM)));
					w2 = Mathf.Sqrt(9.81f * k2 * (1.0f + k2 * k2 / (WAVE_KM * WAVE_KM)));
					w3 = Mathf.Sqrt(9.81f * k3 * (1.0f + k3 * k3 / (WAVE_KM * WAVE_KM)));
					w4 = Mathf.Sqrt(9.81f * k4 * (1.0f + k4 * k4 / (WAVE_KM * WAVE_KM)));
					
					table[(x + y * m_fourierGridSize) * 4 + 0] = w1;
					table[(x + y * m_fourierGridSize) * 4 + 1] = w2;
					table[(x + y * m_fourierGridSize) * 4 + 2] = w3;
					table[(x + y * m_fourierGridSize) * 4 + 3] = w4;
					
				}
			}
			
			//Write floating point data into render texture
			//			ComputeBuffer buffer = new ComputeBuffer(m_fourierGridSize*m_fourierGridSize, sizeof(float)*4);
			
			//			buffer.SetData(table);
			//			CBUtility.WriteIntoRenderTexture(m_WTable, 4, buffer, m_manager.GetWriteData());
			
			m_writeFloat.WriteIntoRenderTexture(m_WTable, 4, table);
			
			//			buffer.Release();
			
		}

		/// <summary>
		/// Some of the values needed in the InitWaveSpectrum function can be precomputed.
		/// If the grid sizes change this function must called again.
		/// </summary>
		void CreateWTableForCPU()
		{
			
			Vector2 uv, st;
			float k1, k2, k3, k4, w1, w2, w3, w4;
			
			m_WTablevector = new Vector4[m_fourierGridSize * m_fourierGridSize];
			
			for (int x = 0; x < m_fourierGridSize; x++)
			{
				for (int y = 0; y < m_fourierGridSize; y++)
				{
					uv = new Vector2(x, y) / m_fsize;
					
					st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
					st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;
					
					k1 = (st * m_inverseGridSizes.x).magnitude;
					k2 = (st * m_inverseGridSizes.y).magnitude;
					k3 = (st * m_inverseGridSizes.z).magnitude;
					k4 = (st * m_inverseGridSizes.w).magnitude;
					
					w1 = Mathf.Sqrt(9.81f * k1 * (1.0f + k1 * k1 / (WAVE_KM * WAVE_KM)));
					w2 = Mathf.Sqrt(9.81f * k2 * (1.0f + k2 * k2 / (WAVE_KM * WAVE_KM)));
					w3 = Mathf.Sqrt(9.81f * k3 * (1.0f + k3 * k3 / (WAVE_KM * WAVE_KM)));
					w4 = Mathf.Sqrt(9.81f * k4 * (1.0f + k4 * k4 / (WAVE_KM * WAVE_KM)));
					
					m_WTablevector[x + y * m_fourierGridSize] = new Vector4(w1, w2, w3, w4);
				}
			}
			
			//			m_writeFloat.WriteIntoRenderTexture( m_WTabletex, 4, m_WTable);
			//			packVector4arrayinTexture (m_WTable, m_WTabletex);
			
		}
		
		Vector2 GetSpectrum(float t, float w, float s0x, float s0y, float s0cx, float s0cy)
		{
			float c = Mathf.Cos(w * t);
			float s = Mathf.Sin(w * t);
			return new Vector2((s0x + s0cx) * c - (s0y + s0cy) * s, (s0x - s0cx) * s + (s0y - s0cy) * c);
		}
		
		Vector2 COMPLEX(Vector2 z)
		{
			return new Vector2(-z.y, z.x); // returns i times z (complex number)
		}
		
		public virtual void onThreadsDone()
		{
			return;
		}
		
//		public void WriteResultsCPU()
//		{
//			
//			//			if(!done) return;
//			//			if(!(done1 && done2 && done3 && done4 && done5)) return;
//			
//			m_map0tex2D.SetPixels(m_result0);
//			m_map1tex2D.SetPixels(m_result1);
//			//			m_map1tex2DR.SetPixels (result1R);
//			//			m_map1tex2DG.SetPixels (result1G);
//			//			m_map1tex2DB.SetPixels (result1B);
//			//			m_map1tex2DA.SetPixels (result1A);
//			//
//			m_map2tex2D.SetPixels(m_result2);
//			//			m_map2tex2DR.SetPixels (result2R);
//			//			m_map2tex2DG.SetPixels (result2G);
//			//			m_map2tex2DB.SetPixels (result2B);
//			//			m_map2tex2DA.SetPixels (result2A);
//			
//			
//			m_map3tex2D.SetPixels(m_result3);
//			m_map4tex2D.SetPixels(m_result4);
//			
//			m_map0tex2D.Apply();
//			m_map1tex2D.Apply();
//			m_map2tex2D.Apply();
//			
//			//			m_map1tex2DR.Apply();
//			//			m_map1tex2DG.Apply();
//			//			m_map1tex2DB.Apply();
//			//			m_map1tex2DA.Apply();
//			//
//			//			m_map2tex2DR.Apply();
//			//			m_map2tex2DG.Apply();
//			//			m_map2tex2DB.Apply();
//			//			m_map2tex2DA.Apply();
//			
//			
//			m_map3tex2D.Apply();
//			m_map4tex2D.Apply();
//			
//			//			m_writeFloat.WriteIntoRenderTexture(m_map0,4,m_fourierBuffer0vector, m_bufferIdx, m_fourierGridSize);
//			//			m_writeFloat.WriteIntoRenderTexture(m_map1,4,m_fourierBuffer1vector, m_idx, m_fourierGridSize);
//			//			m_writeFloat.WriteIntoRenderTexture(m_map2,4,m_fourierBuffer2vector, m_idx, m_fourierGridSize);
//		}
		
		
		void CommitResults(ref Vector4[,] data, ref Vector4[,] output)
		{
			
			for (int x = 0; x < m_fourierGridSize; x++)
			{
				for (int y = 0; y < m_fourierGridSize; y++)
				{
					int i = x + y * m_fourierGridSize;
					
					output[m_bufferIdx,i] = data[m_bufferIdx, i];
					
				}
			}
		}
		
		
//		void PackResults(ref Vector4[,] data, ref Color[] results, float packingFactor)
//		{
//			Vector4 map;
//			float packFactorHalf = packingFactor / 2f;
//			
//			for (int x = 0; x < m_fourierGridSize; x++)
//			{
//				for (int y = 0; y < m_fourierGridSize; y++)
//				{
//					int i = x + y * m_fourierGridSize;
//					
//					map = data[m_bufferIdx, i];
//					
//					results[i].r = (map.x + packFactorHalf) / packingFactor;
//					results[i].g = (map.y + packFactorHalf) / packingFactor;
//					results[i].b = (map.z + packFactorHalf) / packingFactor;
//					results[i].a = (map.w + packFactorHalf) / packingFactor;
//					
//				}
//			}
//		}
		
		
		
		
		
		
//		void PackResultsAsTheyAre(ref Vector4[,] data, ref Color[] results)
//		{
//			Vector4 map;
//			for (int x = 0; x < m_fourierGridSize; x++)
//			{
//				for (int y = 0; y < m_fourierGridSize; y++)
//				{
//					int i = x + y * m_fourierGridSize;
//					
//					map = data[m_bufferIdx, i];
//					
//					results[i].r = map.x;
//					results[i].g = map.y;
//					results[i].b = map.z;
//					results[i].a = map.w;
//					
//				}
//			}
//		}
		
		
//		//I use this to encode a single 32bit per channel texture into 4 separate 8bit per channel texture
//		//This is because unity 4 doesn't support floating point textures outside of rendertextures
//		//Later on these are decoded in the shader as if coming from a single texture
//		void PackResultsRGBA(ref Vector4[,] data, ref Color[] resultsR, ref Color[] resultsG,
//		                     ref Color[] resultsB, ref Color[] resultsA, float packingFactor)
//			
//		{
//			Vector4 map,encode;
//			for (int x = 0; x < m_fourierGridSize; x++)
//			{
//				for (int y = 0; y < m_fourierGridSize; y++)
//				{
//					int i = x + y * m_fourierGridSize;
//					
//					
//					map = data[m_bufferIdx, i]/packingFactor;
//					map+=new Vector4(0.5f,0.5f,0.5f,0.5f);
//					
//					
//					encode=encodeFloatRGBA(map.x);
//					resultsR[i].r = encode.x;
//					resultsR[i].g = encode.y;
//					resultsR[i].b = encode.z;
//					resultsR[i].a = encode.w;
//					
//					encode=encodeFloatRGBA(map.y);
//					resultsG[i].r = encode.x;
//					resultsG[i].g = encode.y;
//					resultsG[i].b = encode.z;
//					resultsG[i].a = encode.w;
//					
//					encode=encodeFloatRGBA(map.z);
//					resultsB[i].r = encode.x;
//					resultsB[i].g = encode.y;
//					resultsB[i].b = encode.z;
//					resultsB[i].a = encode.w;
//					
//					encode=encodeFloatRGBA(map.w);
//					resultsA[i].r = encode.x;
//					resultsA[i].g = encode.y;
//					resultsA[i].b = encode.z;
//					resultsA[i].a = encode.w;
//					
//				}
//			}
//		}
		
		
//		Vector4 encodeFloatRGBA(float v) 
//		{
//			Vector4 enc = new Vector4(1.0f, 255.0f, 65025.0f, 160581375.0f) * v;
//			
//			//			enc = frac(enc);
//			enc.x = enc.x - (float) Math.Floor(enc.x);   //get the fractional
//			enc.y = enc.y - (float) Math.Floor(enc.y);
//			enc.z = enc.z - (float) Math.Floor(enc.z);
//			enc.w = enc.w - (float) Math.Floor(enc.w);
//			
//			//			enc -= enc.yzww * float4(1.0/255.0,1.0/255.0,1.0/255.0,0.0);
//			
//			Vector4 temp = new Vector4 (enc.y, enc.z, enc.w, enc.w);
//			temp.Scale (new Vector4 (1.0f / 255.0f, 1.0f / 255.0f, 1.0f / 255.0f, 0.0f));
//			enc -= temp;
//			
//			return enc;
//		}
		

		
		void RunThreaded1(object o)
		{
			
			Nullable<float> time  = o as Nullable<float>;
			
			InitWaveSpectrumCPU(time.Value);
			
			//			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreadedInit), time);
			//			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded(m_fourierBuffer1vector, m_result1, 2, 2 )), time);
			//			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded(m_fourierBuffer2vector, m_result2, 2, 3 )), time);
			//			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded(m_fourierBuffer3vector, m_result3, 2, 4 )), time);
			//			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded(m_fourierBuffer4vector, m_result4, 2, 5 )), time);
			
			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded2), time);
			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded3), time);
			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded4), time);
			ThreadPool.QueueUserWorkItem(new WaitCallback(RunThreaded5), time);
			
			
			//			m_bufferIdx = m_fourier.PeformFFT(0, m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2);
			m_bufferIdx = m_CPUfourier.PeformFFT(0, m_fourierBuffer0vector);
			//			CommitResults (ref m_fourierBuffer0vector, ref m_fourierBuffer0vectorResults);
			
			
			//			PackResults(ref m_fourierBuffer0vector, ref m_result0,2f);
			
			
			done1 = true;
			
		}
		
		//		void RunThreaded(object o,Vector4[,] data, Color[]  results, float packingFactor, ref bool inDone)
		//		{
		//			
		////			Nullable<float> time  = o as Nullable<float>;
		//
		//			m_fourier.PeformFFT(0, data);
		//			
		//			PackResults(data, results, packingFactor);
		//
		//			inDone = true;
		//		}
		
		void RunThreaded2(object o)
		{
			
			Nullable<float> time  = o as Nullable<float>;
			
			m_CPUfourier.PeformFFT(0, m_fourierBuffer1vector);
			
			//			PackResults (ref m_fourierBuffer1vector, ref m_result1, 2f);
			//			PackResultsAsTheyAre (ref m_fourierBuffer1vector, ref m_result1);
			//			PackResultsRGBA (ref m_fourierBuffer2vector, ref result1R, ref result1G,
			//			                 ref result1B, ref result1A,4f);
			
			
			done2 = true;
			
		}
		
		void RunThreaded3(object o)
		{
			
			Nullable<float> time  = o as Nullable<float>;
			
			m_CPUfourier.PeformFFT(0, m_fourierBuffer2vector);
			
			//			PackResults (ref m_fourierBuffer2vector, ref m_result2, 2f);
			//			PackResultsAsTheyAre (ref m_fourierBuffer2vector, ref m_result2);
			//			PackResultsRGBA (ref m_fourierBuffer2vector, ref result2R, ref result2G,
			//			                 ref result2B, ref result2A,4f);
			
			
			done3 = true;
			
		}
		
		void RunThreaded4(object o)
		{
			
			Nullable<float> time  = o as Nullable<float>;
			
			m_CPUfourier.PeformFFT(0, m_fourierBuffer3vector);
			//			CommitResults (ref m_fourierBuffer3vector, ref m_fourierBuffer3vectorResults);
			
			//			PackResults (ref m_fourierBuffer3vector, ref m_result3, 2f);
			
			done4 = true;
			
		}
		
		
		void RunThreaded5(object o)
		{
			
			Nullable<float> time  = o as Nullable<float>;
			
			m_CPUfourier.PeformFFT(0, m_fourierBuffer4vector);
			//			CommitResults (ref m_fourierBuffer4vector, ref m_fourierBuffer4vectorResults);
			
			//			PackResults (ref m_fourierBuffer4vector, ref m_result4, 2f);
			
			done5 = true;
			
		}
		
		
		void ComputeButterflyLookupTable()
		{
			m_butterflyLookupTable = new float[m_fourierGridSize * m_passes * 4];
			
			for (int i = 0; i < m_passes; i++)
			{
				int nBlocks = (int)Mathf.Pow(2, m_passes - 1 - i);
				int nHInputs = (int)Mathf.Pow(2, i);
				
				for (int j = 0; j < nBlocks; j++)
				{
					for (int k = 0; k < nHInputs; k++)
					{
						int i1, i2, j1, j2;
						if (i == 0)
						{
							i1 = j * nHInputs * 2 + k;
							i2 = j * nHInputs * 2 + nHInputs + k;
							j1 = BitReverse(i1);
							j2 = BitReverse(i2);
						}
						else
						{
							i1 = j * nHInputs * 2 + k;
							i2 = j * nHInputs * 2 + nHInputs + k;
							j1 = i1;
							j2 = i2;
						}
						
						float wr = Mathf.Cos(2.0f * Mathf.PI * (float)(k * nBlocks) / m_fsize);
						float wi = Mathf.Sin(2.0f * Mathf.PI * (float)(k * nBlocks) / m_fsize);
						
						int offset1 = 4 * (i1 + i * m_fourierGridSize);
						m_butterflyLookupTable[offset1 + 0] = j1;
						m_butterflyLookupTable[offset1 + 1] = j2;
						m_butterflyLookupTable[offset1 + 2] = wr;
						m_butterflyLookupTable[offset1 + 3] = wi;
						
						int offset2 = 4 * (i2 + i * m_fourierGridSize);
						m_butterflyLookupTable[offset2 + 0] = j1;
						m_butterflyLookupTable[offset2 + 1] = j2;
						m_butterflyLookupTable[offset2 + 2] = -wr;
						m_butterflyLookupTable[offset2 + 3] = -wi;
						
					}
				}
			}
		}
		
		int BitReverse(int i)
		{
			int j = i;
			int Sum = 0;
			int W = 1;
			int M = m_fourierGridSize / 2;
			while (M != 0)
			{
				j = ((i & M) > M - 1) ? 1 : 0;
				Sum += j * W;
				W *= 2;
				M /= 2;
			}
			return Sum;
		}
		
		void OnGUI(){
			//			GUI.DrawTexture(new Rect(0,0,512, 512), m_map0tex2DScaleMode.ScaleToFit, false);
			//			GUI.DrawTexture(new Rect(0,0,512, 512), m_map0, ScaleMode.ScaleToFit, false);
			
			//			GUI.DrawTexture(new Rect(512,0,512, 512), m_map0);
		}

		/// <summary>
		/// Get the two indices that need to be sampled for bilinear filtering.
		/// </summary>
		public void Index(double x, int sx, out int ix0, out int ix1)
		{
			
			ix0 = (int)x;
			ix1 = (int)x + (int)Math.Sign(x);
			
			//			if(m_wrap)
			//			{
			if(ix0 >= sx || ix0 <= -sx) ix0 = ix0 % sx;
			if(ix0 < 0) ix0 = sx - -ix0;
			
			if(ix1 >= sx || ix1 <= -sx) ix1 = ix1 % sx;
			if(ix1 < 0) ix1 = sx - -ix1;
			//			}
			//			else
			//			{
			//				if(ix0 < 0) ix0 = 0;
			//				else if(ix0 >= sx) ix0 = sx-1;
			//				
			//				if(ix1 < 0) ix1 = 0;
			//				else if(ix1 >= sx) ix1 = sx-1;
			//			}
			//			
		}
		
		
		public void GetUsingBilinearFiltering(float x, float y, float[] v, Vector4[,] m_data, int m_c)
		{
			
			//un-normalize cords
			x *= (float)m_fourierGridSize;
			y *= (float)m_fourierGridSize;
			
			x -= 0.5f;
			y -= 0.5f;
			
			int x0, x1;
			float fx = Math.Abs(x - (int)x);
			Index(x, m_fourierGridSize, out x0, out x1);
			
			int y0, y1;
			float fy = Math.Abs(y - (int)y);
			Index(y, m_fourierGridSize, out y0, out y1);
			
			//			for(int c = 0; c < m_c; c++)  //change this loop to work with Vector4 format
			//			{
			//				float v0 = m_data[(x0 + y0 * m_size) * m_c + c] * (1.0f-fx) + m_data[(x1 + y0 * m_size) * m_c + c] * fx;
			//				float v1 = m_data[(x0 + y1 * m_size) * m_c + c] * (1.0f-fx) + m_data[(x1 + y1 * m_size) * m_c + c] * fx;
			//				
			//				v[c] = v0 * (1.0f-fy) + v1 * fy;
			//			}
			
			
			
			float v0 = m_data[m_bufferIdx,(x0 + y0 * m_fourierGridSize)].x * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y0 * m_fourierGridSize)].x * fx;
			float v1 = m_data[m_bufferIdx,(x0 + y1 * m_fourierGridSize)].x * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y1 * m_fourierGridSize)].x * fx;
			v[0] = v0 * (1.0f-fy) + v1 * fy;
			
			v0 = m_data[m_bufferIdx,(x0 + y0 * m_fourierGridSize)].y * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y0 * m_fourierGridSize)].y * fx;
			v1 = m_data[m_bufferIdx,(x0 + y1 * m_fourierGridSize)].y * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y1 * m_fourierGridSize)].y * fx;
			v[1] = v0 * (1.0f-fy) + v1 * fy;
			
			v0 = m_data[m_bufferIdx,(x0 + y0 * m_fourierGridSize)].z * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y0 * m_fourierGridSize)].z * fx;
			v1 = m_data[m_bufferIdx,(x0 + y1 * m_fourierGridSize)].z * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y1 * m_fourierGridSize)].z * fx;
			v[2] = v0 * (1.0f-fy) + v1 * fy;
			
			v0 = m_data[m_bufferIdx,(x0 + y0 * m_fourierGridSize)].w * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y0 * m_fourierGridSize)].w * fx;
			v1 = m_data[m_bufferIdx,(x0 + y1 * m_fourierGridSize)].w * (1.0f-fx) + m_data[m_bufferIdx,(x1 + y1 * m_fourierGridSize)].w * fx;
			v[3] = v0 * (1.0f-fy) + v1 * fy;
			
			
		}
		
		/// <summary>
		/// This will return the ocean height at any world pos.
		/// </summary>
		public float SampleHeight(Vector3 worldPos)
		{
			float ht = 0.0f;
			
			int HEIGHTS_CHANNELS = 4;
			float[] result = new float[HEIGHTS_CHANNELS];
			
			Vector2 pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.x;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer0vectorResults, 4);
			ht += result[0];
			
			pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.y;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer0vectorResults, 4);
			ht += result[1];
			
			pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.z;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer0vectorResults, 4);
			ht += result[2];
			
			pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.w;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer0vectorResults, 4);
			ht += result[3];
			
			return ht;
		}
		
		
		
		/// <summary>
		/// This will return the ocean height at any world pos.
		/// </summary>
		public Vector2 SampleDisplacement(Vector3 worldPos)
		{
			Vector2 disp = Vector2.zero;
			
			int HEIGHTS_CHANNELS = 4;
			float[] result = new float[HEIGHTS_CHANNELS];
			
			Vector2 pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.x;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer3vectorResults, 4);
			disp.x += result[0]* m_choppyness.x ;
			disp.y += result[1]* m_choppyness.x;
			
			pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.y;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer3vectorResults, 4);
			disp.x += result[2]* m_choppyness.y;
			disp.y += result[3]* m_choppyness.y;
			
			pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.z;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer4vectorResults, 4);
			disp.x += result[0]* m_choppyness.z;
			disp.y += result[1]* m_choppyness.z;
			
			pos = new Vector2(worldPos.x, worldPos.y) / m_gridSizes.w;
			
			GetUsingBilinearFiltering(pos.x, pos.y, result, m_fourierBuffer4vectorResults, 4);
			disp.x += result[2]* m_choppyness.w;
			disp.y += result[3]* m_choppyness.w;
			
			
			return disp;
		}
		
		public float findHeight(Vector3 worldPos, float precision)
		{
			
			int it = 0;
			Vector3 newPos = worldPos;
			
			Vector3 oldPos = worldPos;
			
			
			Vector2 disp = SampleDisplacement (worldPos);
			Vector3 newPosR = newPos + new Vector3 (disp.x, disp.y, 0f);
			
			while (((newPosR - worldPos).magnitude > precision) && (it<30)) {
				
				newPos = newPos - (newPosR - worldPos);
				
				disp = SampleDisplacement (newPos);
				newPosR = newPos + new Vector3 (disp.x, disp.y, 0f);
				it++;
			}
			
			if (it >= 30)
			{
				Debug.Log("[Scatterer] findHeight exceeded 30 iterations and quit");
				
			}
			
			//			Debug.Log ("findheight iterations " + it.ToString ());
			
			
			return (SampleHeight (newPos));
		}
		
	}
	
}