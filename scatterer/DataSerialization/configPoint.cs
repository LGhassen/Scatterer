using System;
using UnityEngine;
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

		public void getValuesFrom(configPoint inConfigPoint)
		{
			//altitude=inConfigPoint.altitude;
			skyAlpha=inConfigPoint.skyAlpha;
			skyExposure=inConfigPoint.skyExposure;
			postProcessAlpha=inConfigPoint.postProcessAlpha;
			postProcessDepth=inConfigPoint.postProcessDepth;
			postProcessExposure=inConfigPoint.postProcessExposure;
			skyExtinctionMultiplier=inConfigPoint.skyExtinctionMultiplier;
			skyExtinctionTint=inConfigPoint.skyExtinctionTint;
			openglThreshold = inConfigPoint.openglThreshold;
			edgeThreshold = inConfigPoint.edgeThreshold;
			viewdirOffset = inConfigPoint.viewdirOffset;
			skyRimExposure = inConfigPoint.skyRimExposure;
			skyextinctionRimFade = inConfigPoint.skyextinctionRimFade;
			skyextinctionGroundFade = inConfigPoint.skyextinctionGroundFade;
			postExtinctionMultiplier = inConfigPoint.postExtinctionMultiplier;
			_Post_Extinction_Tint = inConfigPoint._Post_Extinction_Tint;
			_GlobalOceanAlpha = inConfigPoint._GlobalOceanAlpha;
			_extinctionScatterIntensity = inConfigPoint._extinctionScatterIntensity;
		}

		public void interpolateValuesFrom(configPoint inConfigPoint1, configPoint inConfigPoint2, float x)
		{
			//altitude
			skyAlpha=Mathf.Lerp(inConfigPoint1.skyAlpha, inConfigPoint2.skyAlpha ,x);
			skyExposure=Mathf.Lerp(inConfigPoint1.skyExposure, inConfigPoint2.skyExposure ,x);
			postProcessAlpha=Mathf.Lerp(inConfigPoint1.postProcessAlpha, inConfigPoint2.postProcessAlpha ,x);
			postProcessDepth=Mathf.Lerp(inConfigPoint1.postProcessDepth, inConfigPoint2.postProcessDepth ,x);
			postProcessExposure=Mathf.Lerp(inConfigPoint1.postProcessExposure, inConfigPoint2.postProcessExposure ,x);
			skyExtinctionMultiplier=Mathf.Lerp(inConfigPoint1.skyExtinctionMultiplier, inConfigPoint2.skyExtinctionMultiplier ,x);
			skyExtinctionTint=Mathf.Lerp(inConfigPoint1.skyExtinctionTint, inConfigPoint2.skyExtinctionTint ,x);
			openglThreshold = Mathf.Lerp(inConfigPoint1.openglThreshold, inConfigPoint2.openglThreshold ,x);
			edgeThreshold = Mathf.Lerp(inConfigPoint1.edgeThreshold, inConfigPoint2.edgeThreshold ,x);
			viewdirOffset = Mathf.Lerp(inConfigPoint1.viewdirOffset, inConfigPoint2.viewdirOffset ,x);
			skyRimExposure = Mathf.Lerp(inConfigPoint1.skyRimExposure, inConfigPoint2.skyRimExposure ,x);
			skyextinctionRimFade = Mathf.Lerp(inConfigPoint1.skyextinctionRimFade, inConfigPoint2.skyextinctionRimFade ,x);
			skyextinctionGroundFade = Mathf.Lerp(inConfigPoint1.skyextinctionGroundFade, inConfigPoint2.skyextinctionGroundFade ,x);
			postExtinctionMultiplier = Mathf.Lerp(inConfigPoint1.postExtinctionMultiplier, inConfigPoint2.postExtinctionMultiplier ,x);
			_Post_Extinction_Tint = Mathf.Lerp(inConfigPoint1._Post_Extinction_Tint, inConfigPoint2._Post_Extinction_Tint ,x);
			_GlobalOceanAlpha = Mathf.Lerp(inConfigPoint1._GlobalOceanAlpha, inConfigPoint2._GlobalOceanAlpha ,x);
			_extinctionScatterIntensity = Mathf.Lerp(inConfigPoint1._extinctionScatterIntensity, inConfigPoint2._extinctionScatterIntensity ,x);
		}

		public configPoint()
		{

		}
	}
}

