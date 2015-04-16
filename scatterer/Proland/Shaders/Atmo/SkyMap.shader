 /*
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
 */

/**
 * Precomputed Atmospheric Scattering
 * Copyright (c) 2008 INRIA
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/**
 * Author: Eric Bruneton
 * Modified and ported to Unity by Justin Hawkins 2014
 */
 
Shader "Proland/Atmo/SkyMap" 
{
	SubShader 
	{
    	Pass 
    	{
    	
    	    ZTest Always Cull Off ZWrite Off
      		Fog { Mode off }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Assets/Proland/Shaders/Atmo/Atmosphere.cginc"
			
			uniform float3 _Sun_WorldSunDir;
		
			struct v2f 
			{
    			float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    			
    			float2 uv = v.texcoord;
    			uv = 1.0-uv;
    			
    			OUT.uv = (uv-0.5)*2.2;
    			
    			return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{
			
			   	float2 u = IN.uv;

			   	float l = dot(u, u);
			    float3 result = float3(0,0,0);
			    
		    	if (l <= 1.02 && l > 1.0) 
				{
		            u = u / l;
		            l = 1.0 / l;
		        }
		
		        // inverse stereographic projection,
		        // from skymap coordinates to world space directions
		        float3 r = float3(2.0 * u, 1.0 - l) / (1.0 + l);
		        
		        float3 earthPos = float3(0 , 0, Rg);
		        float3 L = _Sun_WorldSunDir;
		        
		        float3 extinction;
		        float3 inscatter = SkyRadiance(earthPos, r, L, extinction, 1.0);
		        float3 Esky = SkyIrradiance(earthPos.z, L.z);
		       
			    if (l <= 1.02) 
				{
			        result.rgb = inscatter;
			   	}
			   	else
			   	{
			   		float avgFresnel = 0.17;
			   		result.rgb = Esky / M_PI * avgFresnel;
			   	}
			   	
			   	float3 col = result;

				return float4(col,1.0);
			
			}
			
			ENDCG

    	}
	}
}












