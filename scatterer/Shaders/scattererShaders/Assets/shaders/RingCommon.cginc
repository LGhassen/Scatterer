uniform sampler2D ringTexture;
uniform float ringInnerRadius;
uniform float ringOuterRadius;
uniform float3 ringNormal;

inline float getRingShadow(float3 worldPos, float3 sunDir, float3 planetAndRingOrigin)
{
	float3 ringIntersectPt = LinePlaneIntersection(worldPos, sunDir, ringNormal, planetAndRingOrigin); //raycast from worldPos to ring plane and find intersection

	//calculate ring texture position on intersect
	float distance = length (ringIntersectPt - planetAndRingOrigin);
	float ringTexturePosition = (distance - ringInnerRadius) / (ringOuterRadius - ringInnerRadius); //inner and outer radiuses are converted to local space coords on plugin side
	ringTexturePosition = 1 - ringTexturePosition; //flip to match UVs

	float4 ringColor = tex2D(ringTexture, float2 (ringTexturePosition,ringTexturePosition));
	float ringShadow = (1-ringColor.a)*((ringColor.x+ringColor.y+ringColor.z)*0.33334);

	//don't apply any shadows if intersect point is not between inner and outer radius
	ringShadow = (ringTexturePosition > 1 || ringTexturePosition < 0 ) ? 1 : ringShadow;
	return ringShadow;
}