float3 LinePlaneIntersection(float3 linePoint, float3 lineVec, float3 planeNormal, float3 planePoint, out float parallel)
{
	float tlength;
	float dotNumerator;
	float dotDenominator;

	float3 intersectVector;
	float3 intersection = 0.0;

	//calculate the distance between the linePoint and the line-plane intersection point
	dotNumerator = dot((planePoint - linePoint), planeNormal);
	dotDenominator = dot(lineVec, planeNormal);

	//line and plane are not parallel
	if(dotDenominator != 0.0f)
	{
		tlength =  dotNumerator / dotDenominator;
		intersection= (tlength > 0.0) ? linePoint + normalize(lineVec) * (tlength) : linePoint;
		parallel = 0.0;
	}
	else
	{
		parallel = 1.0;
	}

	return intersection;
}