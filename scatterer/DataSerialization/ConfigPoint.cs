using System;
using UnityEngine;
namespace scatterer
{
	public class ConfigPoint
	{
		[Persistent] public float altitude;

		[Persistent] public float skyExposure;
		[Persistent] public float skyAlpha;
		[Persistent] public float skyExtinctionMultiplier;
		[Persistent] public float skyExtinctionTint;

		[Persistent] public float scatteringExposure;

		[Persistent] public float postProcessAlpha;
		[Persistent] public float postProcessDepth;
		[Persistent] public float _Post_Extinction_Tint;
		[Persistent] public float postExtinctionMultiplier;

		[Persistent] public float openglThreshold;
		[Persistent] public float viewdirOffset;


		public ConfigPoint(float inAltitude,float inSkyAlpha,float inSkyExposure,float inPostProcessAlpha,
		                   float inPostProcessDepth,float inPostProcessExposure, float inSkyExtinctionMultiplier,
		                   float inSkyExtinctionTint, float inOpenglThreshold, float inViewdirOffset, float in_Post_Extinction_Tint,
		                   float inpostExtinctionMultiplier)
		{
			altitude=inAltitude;
			skyAlpha=inSkyAlpha;
			skyExposure=inSkyExposure;
			postProcessAlpha=inPostProcessAlpha;
			postProcessDepth=inPostProcessDepth;
			scatteringExposure=inPostProcessExposure;
			skyExtinctionMultiplier=inSkyExtinctionMultiplier;
			skyExtinctionTint=inSkyExtinctionTint;
			openglThreshold = inOpenglThreshold;
			viewdirOffset = inViewdirOffset;
			postExtinctionMultiplier = inpostExtinctionMultiplier;
			_Post_Extinction_Tint = in_Post_Extinction_Tint;
		}

		public void getValuesFrom(ConfigPoint inConfigPoint)
		{
			skyAlpha=inConfigPoint.skyAlpha;
			skyExposure=inConfigPoint.skyExposure;
			postProcessAlpha=inConfigPoint.postProcessAlpha;
			postProcessDepth=inConfigPoint.postProcessDepth;
			scatteringExposure=inConfigPoint.scatteringExposure;
			skyExtinctionMultiplier=inConfigPoint.skyExtinctionMultiplier;
			skyExtinctionTint=inConfigPoint.skyExtinctionTint;
			openglThreshold = inConfigPoint.openglThreshold;
			viewdirOffset = inConfigPoint.viewdirOffset;
			postExtinctionMultiplier = inConfigPoint.postExtinctionMultiplier;
			_Post_Extinction_Tint = inConfigPoint._Post_Extinction_Tint;
		}

		public void interpolateValuesFrom(ConfigPoint inConfigPoint1, ConfigPoint inConfigPoint2, float x)
		{
			skyAlpha=Mathf.Lerp(inConfigPoint1.skyAlpha, inConfigPoint2.skyAlpha ,x);
			skyExposure=Mathf.Lerp(inConfigPoint1.skyExposure, inConfigPoint2.skyExposure ,x);
			postProcessAlpha=Mathf.Lerp(inConfigPoint1.postProcessAlpha, inConfigPoint2.postProcessAlpha ,x);
			postProcessDepth=Mathf.Lerp(inConfigPoint1.postProcessDepth, inConfigPoint2.postProcessDepth ,x);
			scatteringExposure=Mathf.Lerp(inConfigPoint1.scatteringExposure, inConfigPoint2.scatteringExposure ,x);
			skyExtinctionMultiplier=Mathf.Lerp(inConfigPoint1.skyExtinctionMultiplier, inConfigPoint2.skyExtinctionMultiplier ,x);
			skyExtinctionTint=Mathf.Lerp(inConfigPoint1.skyExtinctionTint, inConfigPoint2.skyExtinctionTint ,x);
			openglThreshold = Mathf.Lerp(inConfigPoint1.openglThreshold, inConfigPoint2.openglThreshold ,x);
			viewdirOffset = Mathf.Lerp(inConfigPoint1.viewdirOffset, inConfigPoint2.viewdirOffset ,x);
			postExtinctionMultiplier = Mathf.Lerp(inConfigPoint1.postExtinctionMultiplier, inConfigPoint2.postExtinctionMultiplier ,x);
			_Post_Extinction_Tint = Mathf.Lerp(inConfigPoint1._Post_Extinction_Tint, inConfigPoint2._Post_Extinction_Tint ,x);
		}

		public ConfigPoint()
		{

		}
	}
}

