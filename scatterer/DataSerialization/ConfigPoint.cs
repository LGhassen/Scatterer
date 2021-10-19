using System;
using UnityEngine;
namespace scatterer
{
	public class ConfigPoint
	{
		[Persistent] public float altitude;

		[Persistent] public float skyExposure;
		[Persistent] public float skyAlpha;
		[Persistent] public float skyExtinctionTint;

		[Persistent] public float scatteringExposure;
		[Persistent] public float extinctionThickness;

		[Persistent] public float postProcessAlpha;
		[Persistent] public float postProcessDepth;
		[Persistent] public float extinctionTint;

		public ConfigPoint(float inAltitude,float inSkyAlpha,float inSkyExposure,float inPostProcessAlpha,
		                   float inPostProcessDepth,float inPostProcessExposure,
		                   float inSkyExtinctionTint, float in_Post_Extinction_Tint,
		                   float inExtinctionThickness)
		{
			altitude=inAltitude;
			skyAlpha=inSkyAlpha;
			skyExposure=inSkyExposure;
			postProcessAlpha=inPostProcessAlpha;
			postProcessDepth=inPostProcessDepth;
			scatteringExposure=inPostProcessExposure;
			skyExtinctionTint=inSkyExtinctionTint;
			extinctionTint = in_Post_Extinction_Tint;
			extinctionThickness = inExtinctionThickness;
		}

		public void getValuesFrom(ConfigPoint inConfigPoint)
		{
			skyAlpha=inConfigPoint.skyAlpha;
			skyExposure=inConfigPoint.skyExposure;
			postProcessAlpha=inConfigPoint.postProcessAlpha;
			postProcessDepth=inConfigPoint.postProcessDepth;
			scatteringExposure=inConfigPoint.scatteringExposure;
			skyExtinctionTint=inConfigPoint.skyExtinctionTint;
			extinctionTint = inConfigPoint.extinctionTint;
			extinctionThickness = inConfigPoint.extinctionThickness;
		}

		public void interpolateValuesFrom(ConfigPoint inConfigPoint1, ConfigPoint inConfigPoint2, float x)
		{
			skyAlpha=Mathf.Lerp(inConfigPoint1.skyAlpha, inConfigPoint2.skyAlpha ,x);
			skyExposure=Mathf.Lerp(inConfigPoint1.skyExposure, inConfigPoint2.skyExposure ,x);
			postProcessAlpha=Mathf.Lerp(inConfigPoint1.postProcessAlpha, inConfigPoint2.postProcessAlpha ,x);
			postProcessDepth=Mathf.Lerp(inConfigPoint1.postProcessDepth, inConfigPoint2.postProcessDepth ,x);
			scatteringExposure=Mathf.Lerp(inConfigPoint1.scatteringExposure, inConfigPoint2.scatteringExposure ,x);
			skyExtinctionTint=Mathf.Lerp(inConfigPoint1.skyExtinctionTint, inConfigPoint2.skyExtinctionTint ,x);
			extinctionTint = Mathf.Lerp(inConfigPoint1.extinctionTint, inConfigPoint2.extinctionTint ,x);
			extinctionThickness = Mathf.Lerp(inConfigPoint1.extinctionThickness, inConfigPoint2.extinctionThickness ,x);
		}

		public ConfigPoint()
		{

		}
	}
}

