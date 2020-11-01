//check if ray defined by origin and end intersects frustum
//for it to intersect frustum origin and end must be on different ends of one of the axes
bool intersectsFrustum(float3 origin, float3 end)
{
	return !(origin.x > 1.0 && end.x > 1.0 || origin.x < -1.0 && end.x < -1.0)
		|| !(origin.y > 1.0 && end.y > 1.0 || origin.y < -1.0 && end.y < -1.0)
		|| !(origin.z < 0.0 && end.z < 0.0);
}

float rayDistanceToPoint(float3 rayOrigin, float3 rayDirection, float3 targetPoint)
{
	return  length(cross(rayDirection, targetPoint - rayOrigin));
}

inline bool between(float a, float b, float x)
{
	return (x > a) && (x < b);
}

//cascadeWeights -> 0,1,2,3 -> zero is the most detailed -> 3 is the least detailed
//0 is in lower left corner, 1 in lower right corner, 2 in upper left corner, 3 in upper right corner, in the case of regular cascades no split spheres, not sure about splitSpheres
//Still need to check the shadowMap for each cascade though to make sure we have a depth value at that coordinate
inline fixed pickMostDetailedCascade(float4 wpos, out float4 shadowPos, sampler2D shadowMap)
{
	float3 coords0 = mul (unity_WorldToShadow[0], wpos).xyz;
	float3 coords1 = mul (unity_WorldToShadow[1], wpos).xyz;
	float3 coords2 = mul (unity_WorldToShadow[2], wpos).xyz;
	float3 coords3 = mul (unity_WorldToShadow[3], wpos).xyz;

	float zdepth = 0;
	shadowPos = 0;

	//if zdepth is 0, nothing on this cascade, if zdepth is close to 1.0, the reconstructed position will be a wall at the end of the cascade's range
	if (between(0.0, 0.5, coords0.x) &&  between(0.0, 0.5, coords0.y) && ((zdepth = tex2Dlod(shadowMap, float4(coords0.xy,0.0,0.0)).r) > 0.0 ) && (zdepth < 0.98))
	{
		shadowPos = float4(coords0.xy, zdepth, 1.0);
		return 0;
	}
	else if (between(0.5, 1.0, coords1.x) && between(0.0, 0.5, coords1.y) && ((zdepth = tex2Dlod(shadowMap, float4(coords1.xy,0.0,0.0)).r) > 0.0) && (zdepth < 0.98))
	{
		shadowPos = float4(coords1.xy, zdepth, 1.0);
		return 1;
	}
	else if (between(0.0, 0.5, coords2.x) && between(0.5, 1.0, coords2.y) && ((zdepth = tex2Dlod(shadowMap, float4(coords2.xy,0.0,0.0)).r) > 0.0) && (zdepth < 0.98))
	{
		shadowPos = float4(coords2.xy, zdepth, 1.0);
		return 2;
	}
	else if (between(0.5, 1.0, coords3.x) && between(0.5, 1.0, coords3.y) && ((zdepth = tex2Dlod(shadowMap, float4(coords3.xy,0.0,0.0)).r) > 0.0))
	{
		shadowPos = float4(coords3.xy, zdepth, 1.0);
		return 3;
	}
	else
	{
		return -1;
	}
}