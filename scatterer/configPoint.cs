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
		[Persistent] public float openglThreshold;
		[Persistent] public float edgeThreshold;
		[Persistent] public float viewdirOffset;

			


		public configPoint(float inAltitude,float inSkyAlpha,float inSkyExposure,float inSkyRimExposure,float inPostProcessAlpha,
		                   float inPostProcessDepth,float inPostProcessExposure, float inSkyExtinctionMultiplier,
		                   float inSkyExtinctionTint, float inSkyextinctionRimFade, float inOpenglThreshold, float inEdgeThreshold, float inViewdirOffset)
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
		}

		public configPoint()
		{

		}
	}
}

