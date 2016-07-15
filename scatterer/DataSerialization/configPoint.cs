using System;
namespace scatterer
{
	public class configPoint
	{
				[Persistent] public float altitude;
				[Persistent] public float skyAlpha;
				[Persistent] public float skyExposure;
				[Persistent] public float skyRimExposure;
				[Persistent] public float postProcessAlpha;
				[Persistent] public float postProcessDepth;
				[Persistent] public float postProcessExposure;
				[Persistent] public float skyExtinctionMultiplier;
				[Persistent] public float skyExtinctionTint;
				[Persistent] public float skyextinctionRimFade;
				[Persistent] public float skyextinctionGroundFade;
				[Persistent] public float openglThreshold;
				[Persistent] public float edgeThreshold;
				[Persistent] public float viewdirOffset;
				[Persistent] public float _Post_Extinction_Tint;
				[Persistent] public float postExtinctionMultiplier;
				[Persistent] public float _GlobalOceanAlpha;
				[Persistent] public float _extinctionScatterIntensity;
		

			


		public configPoint(float inAltitude,float inSkyAlpha,float inSkyExposure,float inSkyRimExposure,float inPostProcessAlpha,
		                   float inPostProcessDepth,float inPostProcessExposure, float inSkyExtinctionMultiplier,
		                   float inSkyExtinctionTint, float inSkyextinctionRimFade,float inSkyextinctionGroundFade, float inOpenglThreshold, float inEdgeThreshold, float inViewdirOffset, float in_Post_Extinction_Tint,
		                   float inpostExtinctionMultiplier, float in_GlobalOceanAlpha, float in_extinctionScatterIntensity)
		{
			altitude=inAltitude;
			skyAlpha=inSkyAlpha;
			skyExposure=inSkyExposure;
			postProcessAlpha=inPostProcessAlpha;
			postProcessDepth=inPostProcessDepth;
			postProcessExposure=inPostProcessExposure;
			skyExtinctionMultiplier=inSkyExtinctionMultiplier;
			skyExtinctionTint=inSkyExtinctionTint;
			openglThreshold = inOpenglThreshold;
			edgeThreshold = inEdgeThreshold;
			viewdirOffset = inViewdirOffset;
			skyRimExposure = inSkyRimExposure;
			skyextinctionRimFade = inSkyextinctionRimFade;
			skyextinctionGroundFade = inSkyextinctionGroundFade;
			postExtinctionMultiplier = inpostExtinctionMultiplier;
			_Post_Extinction_Tint = in_Post_Extinction_Tint;
			_GlobalOceanAlpha = in_GlobalOceanAlpha;
			_extinctionScatterIntensity = in_extinctionScatterIntensity;
		}

		public configPoint()
		{

		}
	}
}

