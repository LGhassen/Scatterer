/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 */

/*
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 */
 
uniform float _Ocean_Radius;
uniform float3 _Ocean_Horizon1;
uniform float3 _Ocean_Horizon2;
uniform float _Ocean_HeightOffset;
uniform float3 _Ocean_CameraPos;
uniform float4x4 _Ocean_OceanToCamera;
uniform float4x4 _Ocean_CameraToOcean;
 
float2 OceanPos(float4 vert, float4x4 stoc, out float t, out float3 cameraDir, out float3 oceanDir) 
{
	float horizon = _Ocean_Horizon1.x + _Ocean_Horizon1.y * vert.x;
	
	horizon -= sqrt(_Ocean_Horizon2.x + (_Ocean_Horizon2.y + _Ocean_Horizon2.z * vert.x) * vert.x);
	
	float4 v = float4(vert.x, min(vert.y, horizon), 0.0, 1.0);

    cameraDir = normalize(mul(stoc, v).xyz);
    oceanDir = mul(_Ocean_CameraToOcean, float4(cameraDir, 0.0)).xyz;
    
    float cz = _Ocean_CameraPos.z;
    float dz = oceanDir.z;
    float radius = _Ocean_Radius;
    
    if (radius == 0.0) {
        t = (_Ocean_HeightOffset + -cz) / dz;
    } 
    else 
    {
        float b = dz * (cz + radius);
        float c = cz * (cz + 2.0 * radius);
        float tSphere = - b - sqrt(max(b * b - c, 0.0));
        float tApprox = - cz / dz * (1.0 + cz / (2.0 * radius) * (1.0 - dz * dz));
        t = abs((tApprox - tSphere) * dz) < 1.0 ? tApprox : tSphere;
    }

    return _Ocean_CameraPos.xy + t * oceanDir.xy;
}

float2 OceanPos(float4 vert, float4x4 stoc) 
{
    float t;
    float3 cameraDir;
    float3 oceanDir;
    return OceanPos(vert, stoc, t, cameraDir, oceanDir);
}

float4 Tex2DGrad(sampler2D tex, float2 uv, float2 dx, float2 dy, float2 texSize)
{
	//Sampling a texture by derivatives in unsupported in vert shaders in Unity but if you
	//can manually calculate the derivates you can reproduce its effect using tex2Dlod 
	float2 px = texSize.x * dx;
    float2 py = texSize.y * dy;
	float lod = 0.5 * log2(max(dot(px, px), dot(py, py)));
	return tex2Dlod(tex, float4(uv, 0, lod));
}







