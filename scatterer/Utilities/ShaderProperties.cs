using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace scatterer
{
	
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class ShaderProperties : MonoBehaviour
	{
		//sorry about this huge wall
		//I wrote a script to take care of the whole thing

		public static int _experimentalAtmoScale_PROPERTY { get { return _experimentalAtmoScale; } }
		private static int _experimentalAtmoScale;
		public static int _viewdirOffset_PROPERTY { get { return _viewdirOffset; } }
		private static int _viewdirOffset;
		public static int atmosphereGlobalScale_PROPERTY { get { return atmosphereGlobalScale; } }
		private static int atmosphereGlobalScale;
		public static int scale_PROPERTY { get { return scale; } }
		private static int scale;
		public static int _Scale_PROPERTY { get { return _Scale; } }
		private static int _Scale;
		public static int Rg_PROPERTY { get { return Rg; } }
		private static int Rg;
		public static int Rt_PROPERTY { get { return Rt; } }
		private static int Rt;
		public static int RL_PROPERTY { get { return RL; } }
		private static int RL;
		public static int mieG_PROPERTY { get { return mieG; } }
		private static int mieG;
		public static int _Sun_Intensity_PROPERTY { get { return _Sun_Intensity; } }
		private static int _Sun_Intensity;
		public static int _Sun_WorldSunDir_PROPERTY { get { return _Sun_WorldSunDir; } }
		private static int _Sun_WorldSunDir;
		public static int _Godray_WorldSunDir_PROPERTY { get { return _Godray_WorldSunDir; } }
		private static int _Godray_WorldSunDir;
		public static int SUN_DIR_PROPERTY { get { return SUN_DIR; } }
		private static int SUN_DIR;
		public static int SUN_INTENSITY_PROPERTY { get { return SUN_INTENSITY; } }
		private static int SUN_INTENSITY;
		public static int _Globals_WorldToCamera_PROPERTY { get { return _Globals_WorldToCamera; } }
		private static int _Globals_WorldToCamera;
		public static int _Globals_CameraToWorld_PROPERTY { get { return _Globals_CameraToWorld; } }
		private static int _Globals_CameraToWorld;
		public static int _Globals_CameraToScreen_PROPERTY { get { return _Globals_CameraToScreen; } }
		private static int _Globals_CameraToScreen;
		public static int _Globals_ScreenToCamera_PROPERTY { get { return _Globals_ScreenToCamera; } }
		private static int _Globals_ScreenToCamera;
		public static int _Globals_WorldCameraPos_PROPERTY { get { return _Globals_WorldCameraPos; } }
		private static int _Globals_WorldCameraPos;
		public static int _Scatterer_Origin_PROPERTY { get { return _Scatterer_Origin; } }
		private static int _Scatterer_Origin;
		public static int _Exposure_PROPERTY { get { return _Exposure; } }
		private static int _Exposure;
		public static int _RimExposure_PROPERTY { get { return _RimExposure; } }
		private static int _RimExposure;
		public static int lightOccluders1_PROPERTY { get { return lightOccluders1; } }
		private static int lightOccluders1;
		public static int lightOccluders2_PROPERTY { get { return lightOccluders2; } }
		private static int lightOccluders2;
		public static int sunPosAndRadius_PROPERTY { get { return sunPosAndRadius; } }
		private static int sunPosAndRadius;
		public static int extinctionGroundFade_PROPERTY { get { return extinctionGroundFade; } }
		private static int extinctionGroundFade;
		public static int _Alpha_Global_PROPERTY { get { return _Alpha_Global; } }
		private static int _Alpha_Global;
		public static int _Extinction_Tint_PROPERTY { get { return _Extinction_Tint; } }
		private static int _Extinction_Tint;
		public static int extinctionMultiplier_PROPERTY { get { return extinctionMultiplier; } }
		private static int extinctionMultiplier;
		public static int extinctionRimFade_PROPERTY { get { return extinctionRimFade; } }
		private static int extinctionRimFade;
		public static int _extinctionScatterIntensity_PROPERTY { get { return _extinctionScatterIntensity; } }
		private static int _extinctionScatterIntensity;
		public static int _global_alpha_PROPERTY { get { return _global_alpha; } }
		private static int _global_alpha;
		public static int _global_depth_PROPERTY { get { return _global_depth; } }
		private static int _global_depth;
		public static int _Post_Extinction_Tint_PROPERTY { get { return _Post_Extinction_Tint; } }
		private static int _Post_Extinction_Tint;
		public static int postExtinctionMultiplier_PROPERTY { get { return postExtinctionMultiplier; } }
		private static int postExtinctionMultiplier;
		public static int _openglThreshold_PROPERTY { get { return _openglThreshold; } }
		private static int _openglThreshold;
		public static int _ViewProjInv_PROPERTY { get { return _ViewProjInv; } }
		private static int _ViewProjInv;
		public static int _camPos_PROPERTY { get { return _camPos; } }
		private static int _camPos;
		public static int _Ocean_SunDir_PROPERTY { get { return _Ocean_SunDir; } }
		private static int _Ocean_SunDir;
		public static int _Ocean_CameraToOcean_PROPERTY { get { return _Ocean_CameraToOcean; } }
		private static int _Ocean_CameraToOcean;
		public static int _Ocean_OceanToCamera_PROPERTY { get { return _Ocean_OceanToCamera; } }
		private static int _Ocean_OceanToCamera;
		public static int _Globals_WorldToOcean_PROPERTY { get { return _Globals_WorldToOcean; } }
		private static int _Globals_WorldToOcean;
		public static int _Globals_OceanToWorld_PROPERTY { get { return _Globals_OceanToWorld; } }
		private static int _Globals_OceanToWorld;
		public static int _Ocean_CameraPos_PROPERTY { get { return _Ocean_CameraPos; } }
		private static int _Ocean_CameraPos;
		public static int _Ocean_Color_PROPERTY { get { return _Ocean_Color; } }
		private static int _Ocean_Color;
		public static int _Ocean_ScreenGridSize_PROPERTY { get { return _Ocean_ScreenGridSize; } }
		private static int _Ocean_ScreenGridSize;
		public static int _Ocean_Radius_PROPERTY { get { return _Ocean_Radius; } }
		private static int _Ocean_Radius;
		public static int _OceanAlpha_PROPERTY { get { return _OceanAlpha; } }
		private static int _OceanAlpha;
		public static int alphaRadius_PROPERTY { get { return alphaRadius; } }
		private static int alphaRadius;
		public static int _GlobalOceanAlpha_PROPERTY { get { return _GlobalOceanAlpha; } }
		private static int _GlobalOceanAlpha;
		public static int sphereDir_PROPERTY { get { return sphereDir; } }
		private static int sphereDir;
		public static int cosTheta_PROPERTY { get { return cosTheta; } }
		private static int cosTheta;
		public static int sinTheta_PROPERTY { get { return sinTheta; } }
		private static int sinTheta;
		public static int _Ocean_MapSize_PROPERTY { get { return _Ocean_MapSize; } }
		private static int _Ocean_MapSize;
		public static int _Ocean_Choppyness_PROPERTY { get { return _Ocean_Choppyness; } }
		private static int _Ocean_Choppyness;
		public static int _Ocean_GridSizes_PROPERTY { get { return _Ocean_GridSizes; } }
		private static int _Ocean_GridSizes;
		public static int _Ocean_HeightOffset_PROPERTY { get { return _Ocean_HeightOffset; } }
		private static int _Ocean_HeightOffset;
		public static int _Ocean_Variance_PROPERTY { get { return _Ocean_Variance; } }
		private static int _Ocean_Variance;
		public static int _Ocean_Map0_PROPERTY { get { return _Ocean_Map0; } }
		private static int _Ocean_Map0;
		public static int _Ocean_Map1_PROPERTY { get { return _Ocean_Map1; } }
		private static int _Ocean_Map1;
		public static int _Ocean_Map2_PROPERTY { get { return _Ocean_Map2; } }
		private static int _Ocean_Map2;
		public static int _Ocean_Map3_PROPERTY { get { return _Ocean_Map3; } }
		private static int _Ocean_Map3;
		public static int _Ocean_Map4_PROPERTY { get { return _Ocean_Map4; } }
		private static int _Ocean_Map4;
		public static int _VarianceMax_PROPERTY { get { return _VarianceMax; } }
		private static int _VarianceMax;
		public static int _Map5_PROPERTY { get { return _Map5; } }
		private static int _Map5;
		public static int _Map6_PROPERTY { get { return _Map6; } }
		private static int _Map6;
		public static int _Map7_PROPERTY { get { return _Map7; } }
		private static int _Map7;
		public static int _Choppyness_PROPERTY { get { return _Choppyness	; } }
		private static int _Choppyness;
		public static int _Ocean_WhiteCapStr_PROPERTY { get { return _Ocean_WhiteCapStr; } }
		private static int _Ocean_WhiteCapStr;
		public static int farWhiteCapStr_PROPERTY { get { return farWhiteCapStr; } }
		private static int farWhiteCapStr;
		public static int _Ocean_Foam0_PROPERTY { get { return _Ocean_Foam0; } }
		private static int _Ocean_Foam0;
		public static int _Ocean_Foam1_PROPERTY { get { return _Ocean_Foam1; } }
		private static int _Ocean_Foam1;
		public static int _Transmittance_PROPERTY { get { return _Transmittance; } }
		private static int _Transmittance;
		public static int _Inscatter_PROPERTY { get { return _Inscatter; } }
		private static int _Inscatter;
		public static int _Irradiance_PROPERTY { get { return _Irradiance; } }
		private static int _Irradiance;
		public static int _customDepthTexture_PROPERTY { get { return _customDepthTexture; } }
		private static int _customDepthTexture;
		public static int _godrayDepthTexture_PROPERTY { get { return _godrayDepthTexture; } }
		private static int _godrayDepthTexture;
		public static int M_PI_PROPERTY { get { return M_PI; } }
		private static int M_PI;
		public static int Rl_PROPERTY { get { return Rl; } }
		private static int Rl;
		public static int RES_R_PROPERTY { get { return RES_R; } }
		private static int RES_R;
		public static int RES_MU_PROPERTY { get { return RES_MU; } }
		private static int RES_MU;
		public static int RES_MU_S_PROPERTY { get { return RES_MU_S; } }
		private static int RES_MU_S;
		public static int RES_NU_PROPERTY { get { return RES_NU; } }
		private static int RES_NU;
		public static int SKY_W_PROPERTY { get { return SKY_W; } }
		private static int SKY_W;
		public static int SKY_H_PROPERTY { get { return SKY_H; } }
		private static int SKY_H;
		public static int betaMSca_PROPERTY { get { return betaMSca; } }
		private static int betaMSca;
		public static int betaMEx_PROPERTY { get { return betaMEx; } }
		private static int betaMEx;
		public static int HR_PROPERTY { get { return HR; } }
		private static int HR;
		public static int HM_PROPERTY { get { return HM; } }
		private static int HM;
		public static int betaR_PROPERTY { get { return betaR; } }
		private static int betaR;

		public static int TRANSMITTANCE_W_PROPERTY { get { return TRANSMITTANCE_W; } }
		private static int TRANSMITTANCE_W;

		public static int TRANSMITTANCE_H_PROPERTY { get { return TRANSMITTANCE_H; } }
		private static int TRANSMITTANCE_H;

		public static int AVERAGE_GROUND_REFLECTANCE_PROPERTY { get { return AVERAGE_GROUND_REFLECTANCE; } }
		private static int AVERAGE_GROUND_REFLECTANCE;

		public static int _Spectrum01_PROPERTY { get { return _Spectrum01; } }
		private static int _Spectrum01;
		public static int _Spectrum23_PROPERTY { get { return _Spectrum23; } }
		private static int _Spectrum23;
		public static int _WTable_PROPERTY { get { return _WTable; } }
		private static int _WTable;
		public static int _Offset_PROPERTY { get { return _Offset; } }
		private static int _Offset;
		public static int _InverseGridSizes_PROPERTY { get { return _InverseGridSizes; } }
		private static int _InverseGridSizes;

		public static int _T_PROPERTY { get { return _T; } }
		private static int _T;
		public static int _Buffer1_PROPERTY { get { return _Buffer1; } }
		private static int _Buffer1;
		public static int _Buffer2_PROPERTY { get { return _Buffer2; } }
		private static int _Buffer2;

			
				
		
		private void Awake()
		{
			_experimentalAtmoScale = Shader.PropertyToID("_experimentalAtmoScale");
			_viewdirOffset = Shader.PropertyToID("_viewdirOffset");
			atmosphereGlobalScale = Shader.PropertyToID("atmosphereGlobalScale");
			scale = Shader.PropertyToID("scale");
			_Scale = Shader.PropertyToID("_Scale");
			Rg = Shader.PropertyToID("Rg");
			Rt = Shader.PropertyToID("Rt");
			RL = Shader.PropertyToID("RL");
			mieG = Shader.PropertyToID("mieG");
			_Sun_Intensity = Shader.PropertyToID("_Sun_Intensity");
			_Sun_WorldSunDir = Shader.PropertyToID("_Sun_WorldSunDir");
			_Godray_WorldSunDir = Shader.PropertyToID("_Godray_WorldSunDir");
			SUN_DIR = Shader.PropertyToID("SUN_DIR");
			SUN_INTENSITY = Shader.PropertyToID("SUN_INTENSITY");
			_Globals_WorldToCamera = Shader.PropertyToID("_Globals_WorldToCamera");
			_Globals_CameraToWorld = Shader.PropertyToID("_Globals_CameraToWorld");
			_Globals_CameraToScreen = Shader.PropertyToID("_Globals_CameraToScreen");
			_Globals_ScreenToCamera = Shader.PropertyToID("_Globals_ScreenToCamera");
			_Globals_WorldCameraPos = Shader.PropertyToID("_Globals_WorldCameraPos");
			_Scatterer_Origin = Shader.PropertyToID("_Scatterer_Origin");
			_Exposure = Shader.PropertyToID("_Exposure");
			_RimExposure = Shader.PropertyToID("_RimExposure");
			lightOccluders1 = Shader.PropertyToID("lightOccluders1");
			lightOccluders2 = Shader.PropertyToID("lightOccluders2");
			sunPosAndRadius = Shader.PropertyToID("sunPosAndRadius");
			extinctionGroundFade = Shader.PropertyToID("extinctionGroundFade");
			_Alpha_Global = Shader.PropertyToID("_Alpha_Global");
			_Extinction_Tint = Shader.PropertyToID("_Extinction_Tint");
			extinctionMultiplier = Shader.PropertyToID("extinctionMultiplier");
			extinctionRimFade = Shader.PropertyToID("extinctionRimFade");
			_extinctionScatterIntensity = Shader.PropertyToID("_extinctionScatterIntensity");
			_global_alpha = Shader.PropertyToID("_global_alpha");
			_global_depth = Shader.PropertyToID("_global_depth");
			_Post_Extinction_Tint = Shader.PropertyToID("_Post_Extinction_Tint");
			postExtinctionMultiplier = Shader.PropertyToID("postExtinctionMultiplier");
			_openglThreshold = Shader.PropertyToID("_openglThreshold");
			_ViewProjInv = Shader.PropertyToID("_ViewProjInv");
			_camPos = Shader.PropertyToID("_camPos");
			_Ocean_SunDir = Shader.PropertyToID("_Ocean_SunDir");
			_Ocean_CameraToOcean = Shader.PropertyToID("_Ocean_CameraToOcean");
			_Ocean_OceanToCamera = Shader.PropertyToID("_Ocean_OceanToCamera");
			_Globals_WorldToOcean = Shader.PropertyToID("_Globals_WorldToOcean");
			_Globals_OceanToWorld = Shader.PropertyToID("_Globals_OceanToWorld");
			_Ocean_CameraPos = Shader.PropertyToID("_Ocean_CameraPos");
			_Ocean_Color = Shader.PropertyToID("_Ocean_Color");
			_Ocean_ScreenGridSize = Shader.PropertyToID("_Ocean_ScreenGridSize");
			_Ocean_Radius = Shader.PropertyToID("_Ocean_Radius");
			_OceanAlpha = Shader.PropertyToID("_OceanAlpha");
			alphaRadius = Shader.PropertyToID("alphaRadius");
			_GlobalOceanAlpha = Shader.PropertyToID("_GlobalOceanAlpha");
			sphereDir = Shader.PropertyToID("sphereDir");
			cosTheta = Shader.PropertyToID("cosTheta");
			sinTheta = Shader.PropertyToID("sinTheta");
			_Ocean_MapSize = Shader.PropertyToID("_Ocean_MapSize");
			_Ocean_Choppyness = Shader.PropertyToID("_Ocean_Choppyness");
			_Ocean_GridSizes = Shader.PropertyToID("_Ocean_GridSizes");
			_Ocean_HeightOffset = Shader.PropertyToID("_Ocean_HeightOffset");
			_Ocean_Variance = Shader.PropertyToID("_Ocean_Variance");
			_Ocean_Map0 = Shader.PropertyToID("_Ocean_Map0");
			_Ocean_Map1 = Shader.PropertyToID("_Ocean_Map1");
			_Ocean_Map2 = Shader.PropertyToID("_Ocean_Map2");
			_Ocean_Map3 = Shader.PropertyToID("_Ocean_Map3");
			_Ocean_Map4 = Shader.PropertyToID("_Ocean_Map4");
			_VarianceMax = Shader.PropertyToID("_VarianceMax");
			_Map5 = Shader.PropertyToID("_Map5");
			_Map6 = Shader.PropertyToID("_Map6");
			_Map7 = Shader.PropertyToID("_Map7");
			_Choppyness	= Shader.PropertyToID("_Choppyness");
			_Ocean_WhiteCapStr = Shader.PropertyToID("_Ocean_WhiteCapStr");
			farWhiteCapStr = Shader.PropertyToID("farWhiteCapStr");
			_Ocean_Foam0 = Shader.PropertyToID("_Ocean_Foam0");
			_Ocean_Foam1 = Shader.PropertyToID("_Ocean_Foam1");

			_Transmittance = Shader.PropertyToID("_Transmittance");
			_Inscatter = Shader.PropertyToID("_Inscatter");
			_Irradiance = Shader.PropertyToID("_Irradiance");
			_customDepthTexture = Shader.PropertyToID("_customDepthTexture");
			_godrayDepthTexture = Shader.PropertyToID("_godrayDepthTexture");

			M_PI = Shader.PropertyToID("M_PI");
			Rl = Shader.PropertyToID("Rl");
			RES_R = Shader.PropertyToID("RES_R");
			RES_MU = Shader.PropertyToID("RES_MU");
			RES_MU_S = Shader.PropertyToID("RES_MU_S");
			RES_NU = Shader.PropertyToID("RES_NU");
			SKY_W = Shader.PropertyToID("SKY_W");
			SKY_H = Shader.PropertyToID("SKY_H");
			betaMSca = Shader.PropertyToID("betaMSca");
			betaMEx = Shader.PropertyToID("betaMEx");
			HR = Shader.PropertyToID("HR");
			HM = Shader.PropertyToID("HM");
			betaR = Shader.PropertyToID("betaR");

			TRANSMITTANCE_W = Shader.PropertyToID("TRANSMITTANCE_W");
			TRANSMITTANCE_H = Shader.PropertyToID("TRANSMITTANCE_H");
			AVERAGE_GROUND_REFLECTANCE = Shader.PropertyToID("AVERAGE_GROUND_REFLECTANCE");

			_Spectrum01 = Shader.PropertyToID("_Spectrum01");
			_Spectrum23 = Shader.PropertyToID("_Spectrum23");
			_WTable = Shader.PropertyToID("_WTable");
			_Offset = Shader.PropertyToID("_Offset");
			_InverseGridSizes = Shader.PropertyToID("_InverseGridSizes");
			_InverseGridSizes = Shader.PropertyToID("_InverseGridSizes");

			_T = Shader.PropertyToID("_T");
			_Buffer1 = Shader.PropertyToID("_Buffer1");
			_Buffer2 = Shader.PropertyToID("_Buffer2");

		}
	}
}