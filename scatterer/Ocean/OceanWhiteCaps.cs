using UnityEngine;
using System.Collections;

namespace scatterer {
	/*
	 * Extend the OceanFFT node to also generate ocean white caps.
	 */
	public class OceanWhiteCaps: OceanFFTgpu {
		
		Material m_initJacobiansMat;
		
		Material m_whiteCapsPrecomputeMat;

		[Persistent]
		protected string name;
		
		[Persistent] public int m_foamAnsio = 9;
		
		[Persistent] public float m_foamMipMapBias = -2.0f;
		
		[Persistent] public float m_whiteCapStr = 0.1f;

		[Persistent] public float shoreFoam = 1.0f;

		[Persistent] public float m_farWhiteCapStr = 0.1f;

		[Persistent] public float choppynessMultiplier = 1f;
		
		RenderTexture[] m_fourierBuffer5, m_fourierBuffer6, m_fourierBuffer7;
		
		RenderTexture m_map5, m_map6;
		RenderTexture m_foam0, m_foam1;
		
		//		protected override void Start() 
		public override void Start() {
			base.Start();

			m_initJacobiansMat = new Material(ShaderReplacer.Instance.LoadedShaders[ ("Proland/Ocean/InitJacobians")]);
			m_whiteCapsPrecomputeMat = new Material(ShaderReplacer.Instance.LoadedShaders[("Proland/Ocean/WhiteCapsPrecompute0")]);
			
			
			m_initJacobiansMat.SetTexture(ShaderProperties._Spectrum01_PROPERTY, m_spectrum01);
			m_initJacobiansMat.SetTexture(ShaderProperties._Spectrum23_PROPERTY, m_spectrum23);
			m_initJacobiansMat.SetTexture(ShaderProperties._WTable_PROPERTY, m_WTable);
			m_initJacobiansMat.SetVector (ShaderProperties._Offset_PROPERTY, m_offset);
			m_initJacobiansMat.SetVector (ShaderProperties._InverseGridSizes_PROPERTY, m_inverseGridSizes);
		}
		
		protected override void CreateRenderTextures() {
			
			RenderTextureFormat mapFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
			
			//These texture hold the actual data use in the ocean renderer
			CreateMap(ref m_map5, mapFormat, m_ansio, true);
			CreateMap(ref m_map6, mapFormat, m_ansio, true);
			
			CreateMap(ref m_foam0, format, m_foamAnsio, true);
			CreateMap(ref m_foam1, format, m_foamAnsio, true);

//			CreateMap(ref m_foam1, format, m_foamAnsio, m_manager.GetCore().foamMipMapping);
			
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
			m_initJacobiansMat.SetFloat (ShaderProperties._T_PROPERTY, t);
			//			RTUtility.MultiTargetBlit(buffers567, m_initJacobiansMat);
			RTUtility.MultiTargetBlit(buffers567, m_initJacobiansMat, 0);
		}
		
		public override void UpdateNode() {
			if (!MapView.MapIsEnabled) {
				base.UpdateNode();

				if (!MapView.MapIsEnabled)
				{
					m_fourier.PeformFFT(m_fourierBuffer5, m_fourierBuffer6, m_fourierBuffer7);
					
					//original block, two textures in one pass
					//				m_whiteCapsPrecomputeMat.SetTexture(ShaderProperties._Map5_PROPERTY, m_fourierBuffer5[m_idx]);
					//				m_whiteCapsPrecomputeMat.SetTexture(ShaderProperties._Map6_PROPERTY, m_fourierBuffer6[m_idx]);
					//				m_whiteCapsPrecomputeMat.SetTexture(ShaderProperties._Map7_PROPERTY, m_fourierBuffer7[m_idx]);
					//				m_whiteCapsPrecomputeMat.SetVector (ShaderProperties._Choppyness_PROPERTY, m_choppyness * choppynessMultiplier);
					//				RenderTexture[] buffers = new RenderTexture[] {m_foam0, m_foam1};
					//				RTUtility.MultiTargetBlit(buffers, m_whiteCapsPrecomputeMat, 0);
					
					//fixed block, two passes, fixes mipmapping issue resulting in black ocean
					m_whiteCapsPrecomputeMat.SetTexture(ShaderProperties._Map5_PROPERTY, m_fourierBuffer5[m_idx]);
					m_whiteCapsPrecomputeMat.SetTexture(ShaderProperties._Map6_PROPERTY, m_fourierBuffer6[m_idx]);
					m_whiteCapsPrecomputeMat.SetTexture(ShaderProperties._Map7_PROPERTY, m_fourierBuffer7[m_idx]);
					m_whiteCapsPrecomputeMat.SetVector (ShaderProperties._Choppyness_PROPERTY, m_choppyness);
					Graphics.Blit (null, m_foam0, m_whiteCapsPrecomputeMat, 0);
					Graphics.Blit (null, m_foam1, m_whiteCapsPrecomputeMat, 1);
					
					
					m_oceanMaterial.SetFloat (ShaderProperties._Ocean_WhiteCapStr_PROPERTY, m_whiteCapStr);
					m_oceanMaterial.SetFloat (ShaderProperties.farWhiteCapStr_PROPERTY, m_farWhiteCapStr);
					m_oceanMaterial.SetTexture(ShaderProperties._Ocean_Foam0_PROPERTY, m_foam0);
					m_oceanMaterial.SetTexture(ShaderProperties._Ocean_Foam1_PROPERTY, m_foam1);


					m_oceanMaterial.SetFloat ("shoreFoam", shoreFoam);

//					m_oceanMaterialNear.SetFloat (ShaderProperties._Ocean_WhiteCapStr_PROPERTY, m_whiteCapStr);
//					m_oceanMaterialNear.SetFloat (ShaderProperties.farWhiteCapStr_PROPERTY, m_farWhiteCapStr);
//					m_oceanMaterialNear.SetTexture(ShaderProperties._Ocean_Foam0_PROPERTY, m_foam0);
//					m_oceanMaterialNear.SetTexture(ShaderProperties._Ocean_Foam1_PROPERTY, m_foam1);
				}
				

			}
		}
		
		

	}
	
}