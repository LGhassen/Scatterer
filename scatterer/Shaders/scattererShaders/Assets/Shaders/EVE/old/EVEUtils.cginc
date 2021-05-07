#ifndef EVE_UTILS_CG_INCLUDED
#define EVE_UTILS_CG_INCLUDED

	
	#include "UnityCG.cginc"
	#include "AutoLight.cginc"
	#include "Lighting.cginc"
	#define PI 3.1415926535897932384626
	#define INV_PI (1.0/PI)
	#define TWOPI (2.0*PI) 
	#define INV_2PI (1.0/TWOPI)
	#define SQRT_2 (1.41421356237)
	#pragma fragmentoption ARB_precision_hint_fastest

#ifdef DIRLIGHT_ONLY
#define DIRECTIONAL 1
#define SHADOWS_OFF 1
#define LIGHTMAP_OFF 1
#define DIRLIGHTMAP_OFF 1
#endif

	uniform float4x4 _MainRotation;
	uniform float4x4 _DetailRotation;


	/*=========================================================================*
	* R A N D _ R O T A T I O N Author: Jim Arvo, 1991 *
	* *
	* This routine maps three values (x[0], x[1], x[2]) in the range [0,1] *
	* into a 3x3 rotation matrix, M. Uniformly distributed random variables *
	* x0, x1, and x2 create uniformly distributed random rotation matrices. *
	* To create small uniformly distributed "perturbations", supply *
	* samples in the following ranges *
	* *
	* x[0] in [ 0, d ] *
	* x[1] in [ 0, 1 ] *
	* x[2] in [ 0, d ] *
	* *
	* where 0 < d < 1 controls the size of the perturbation. Any of the *
	* random variables may be stratified (or "jittered") for a slightly more *
	* even distribution. *
	* *
	*=========================================================================*/
	float4x4 rand_rotation(float3 x, float scale, float3 trans)
	{
		float theta = x[0] * TWOPI; /* Rotation about the pole (Z). */
		float phi = x[1] * TWOPI; /* For direction of pole deflection. */
		float z = x[2] * 2.0; /* For magnitude of pole deflection. */

							  /* Compute a vector V used for distributing points over the sphere */
							  /* via the reflection I - V Transpose(V). This formulation of V */
							  /* will guarantee that if x[1] and x[2] are uniformly distributed, */
							  /* the reflected points will be uniform on the sphere. Note that V */
							  /* has length sqrt(2) to eliminate the 2 in the Householder matrix. */

		float r = sqrt(z);
		float Vx = sin(phi) * r;
		float Vy = cos(phi) * r;
		float Vz = sqrt(2.0 - z);

		/* Compute the row vector S = Transpose(V) * R, where R is a simple */
		/* rotation by theta about the z-axis. No need to compute Sz since */
		/* it's just Vz. */

		float st = sin(theta);
		float ct = cos(theta);
		float Sx = Vx * ct - Vy * st;
		float Sy = Vx * st + Vy * ct;

		/* Construct the rotation matrix ( V Transpose(V) - I ) R, which */
		/* is equivalent to V S - R. */


		float4x4 M = float4x4(
			scale*(Vx * Sx - ct), Vy * Sx + st, Vz * Sx, trans.x,
			Vx * Sy - st, scale*(Vy * Sy - ct), Vz * Sy, trans.y,
			Vx * Vz, Vy * Vz, scale*(1.0 - z), trans.z,
			0, 0, 0, 1);

		return M;
	}

	inline float3 hash( float3 val )
	{
		return frac(sin(val)*1232.53);
	}

	inline float GetDistanceFade( float dist, float fade, float fadeVert )
	{
		float fadeDist = fade*dist;
		float distVert = 1-(fadeVert*dist);
		return saturate(fadeDist) * saturate(distVert);
	}
			
	inline half4 GetLighting(half3 worldNorm, half3 lightDir, fixed atten, fixed ambient)
	{
		half3 ambientLighting = ambient * UNITY_LIGHTMODEL_AMBIENT;
		half NdotL = dot (worldNorm, lightDir);
		half lightIntensity = saturate(_LightColor0.a * NdotL * 2 * atten);
		half4 light;
		light.rgb = max(ambientLighting + (_LightColor0.rgb * lightIntensity), 0);
		light.a = max(ambientLighting + lightIntensity, 0);
		
		return light;
	}
	
	// Calculates Blinn-Phong (specular) lighting model
	inline half4 SpecularColorLight( half3 lightDir, half3 viewDir, half3 normal, half4 color, half4 specColor, float specK, half atten )
	{
	    
	    lightDir = normalize(lightDir);
	    viewDir = normalize(viewDir);
	    normal = normalize(normal);
	    half3 h = normalize( lightDir + viewDir );
	    
	    half diffuse = dot( normal, lightDir );
	    
	    float nh = saturate( dot( h, normal ) );
	    float spec = pow( nh, specK ) * specColor.a;
	    
	    half4 c;
	    c.rgb = (color.rgb * _LightColor0.rgb * diffuse + _LightColor0.rgb * specColor.rgb * spec) * (atten * 4);
	    c.a = diffuse*(atten * 4);//_LightColor0.a * specColor.a * spec * atten; // specular passes by default put highlights to overbright
	    return c;
	}
	
	inline half Terminator(half3 lightDir, half3 normal)
	{
		half NdotL = dot( normal, lightDir );
		half termlerp = saturate(10*-NdotL);
		half terminator = lerp(1,saturate(floor(1.01+NdotL)), termlerp);
		return terminator;
	}
#endif