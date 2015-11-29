using UnityEngine;
using System.Collections;

namespace scatterer {
	/*
	 * Extend the OceanFFT node to also generate ocean white caps.
	 */
	public class OceanWhiteCaps: OceanFFT {
		
		Material m_initJacobiansMat;
		
		Material m_whiteCapsPrecomputeMat;
		
		[Persistent] public int m_foamAnsio = 9;
		
		[Persistent] public float m_foamMipMapBias = -2.0f;
		
		[Persistent] public float m_whiteCapStr = 0.1f;
		
		RenderTexture[] m_fourierBuffer5, m_fourierBuffer6, m_fourierBuffer7;
		
		RenderTexture m_map5, m_map6;
		RenderTexture m_foam0, m_foam1;
		
		//		protected override void Start() 
		public override void Start() {
			loadFromConfigNode();
			base.Start();
			
			m_initJacobiansMat = new Material(ShaderTool.GetMatFromShader2("CompiledInitJacobians.shader"));
			m_whiteCapsPrecomputeMat = new Material(ShaderTool.GetMatFromShader2("CompiledWhiteCapsPrecompute.shader"));
			
			
			m_initJacobiansMat.SetTexture("_Spectrum01", m_spectrum01);
			m_initJacobiansMat.SetTexture("_Spectrum23", m_spectrum23);
			m_initJacobiansMat.SetTexture("_WTable", m_WTable);
			m_initJacobiansMat.SetVector("_Offset", m_offset);
			m_initJacobiansMat.SetVector("_InverseGridSizes", m_inverseGridSizes);
		}
		
		protected override void CreateRenderTextures() {
			
			RenderTextureFormat mapFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
			
			//These texture hold the actual data use in the ocean renderer
			CreateMap(ref m_map5, mapFormat, m_ansio);
			CreateMap(ref m_map6, mapFormat, m_ansio);
			
			CreateMap(ref m_foam0, format, m_foamAnsio);
			CreateMap(ref m_foam1, format, m_foamAnsio);
			
			m_foam1.mipMapBias = m_foamMipMapBias;
			
			//These textures are used to perform the fourier transform
			CreateBuffer(ref m_fourierBuffer5, format); // Jacobians XX
			CreateBuffer(ref m_fourierBuffer6, format); // Jacobians YY
			CreateBuffer(ref m_fourierBuffer7, format); // Jacobians XY
			
			//Make sure the base textures are also created
			base.CreateRenderTextures();
		}
		
		public override void OnDestroy() {
			base.OnDestroy();
			
			m_foam0.Release();
			m_foam1.Release();
			
			for (int i = 0; i < 2; i++) {
				m_fourierBuffer5[i].Release();
				m_fourierBuffer6[i].Release();
				m_fourierBuffer7[i].Release();
			}
		}
		
		protected override void InitWaveSpectrum(float t) {
			base.InitWaveSpectrum(t);
			
			// Init jacobians (5,6,7)
			RenderTexture[] buffers567 = new RenderTexture[] {
				m_fourierBuffer5[1], m_fourierBuffer6[1], m_fourierBuffer7[1]
			};
			m_initJacobiansMat.SetFloat("_T", t);
			//			RTUtility.MultiTargetBlit(buffers567, m_initJacobiansMat);
			RTUtility.MultiTargetBlit(buffers567, m_initJacobiansMat, 0);
		}
		
		public override void UpdateNode() {
			if (!MapView.MapIsEnabled) {
				base.UpdateNode();
				
				m_fourier.PeformFFT(m_fourierBuffer5, m_fourierBuffer6, m_fourierBuffer7);
				
				m_whiteCapsPrecomputeMat.SetTexture("_Map5", m_fourierBuffer5[m_idx]);
				m_whiteCapsPrecomputeMat.SetTexture("_Map6", m_fourierBuffer6[m_idx]);
				m_whiteCapsPrecomputeMat.SetTexture("_Map7", m_fourierBuffer7[m_idx]);
				m_whiteCapsPrecomputeMat.SetVector("_Choppyness", m_choppyness);
				
				RenderTexture[] buffers = new RenderTexture[] {
					m_foam0, m_foam1
				};
				
				//			RTUtility.MultiTargetBlit(buffers, m_whiteCapsPrecomputeMat);
				RTUtility.MultiTargetBlit(buffers, m_whiteCapsPrecomputeMat, 0);
				
				m_oceanMaterialFar.SetFloat("_Ocean_WhiteCapStr", m_whiteCapStr);
				m_oceanMaterialFar.SetTexture("_Ocean_Foam0", m_foam0);
				m_oceanMaterialFar.SetTexture("_Ocean_Foam1", m_foam1);
				
				m_oceanMaterialNear.SetFloat("_Ocean_WhiteCapStr", m_whiteCapStr);
				m_oceanMaterialNear.SetTexture("_Ocean_Foam0", m_foam0);
				m_oceanMaterialNear.SetTexture("_Ocean_Foam1", m_foam1);
				

			}
		}
		
		

	}
	
}