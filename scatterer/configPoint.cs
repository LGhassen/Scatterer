using System;
namespace scatterer
{
	public class configPoint
	{
		[Persistent] public float altitude;
		[Persistent] public float skyAlpha;
		[Persistent] public float skyExposure;
		[Persistent] public float postProcessAlpha;
		[Persistent] public float postProcessDepth;
		[Persistent] public float postProcessExposure;
			
			public configPoint(float inAltitude,float inSkyAlpha,float inSkyExposure,float inPostProcessAlpha,float inPostProcessDepth,float inPostProcessExposure)
			{
				altitude=inAltitude;
				skyAlpha=inSkyAlpha;
				skyExposure=inSkyExposure;
				postProcessAlpha=inPostProcessAlpha;
				postProcessDepth=inPostProcessDepth;
				postProcessExposure=inPostProcessExposure;
				
			}
	}
}

