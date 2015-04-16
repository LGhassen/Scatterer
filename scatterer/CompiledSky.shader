// Compiled shader for all platforms, uncompressed size: 56.0KB

Shader "Proland/Atmo/Sky" {
SubShader { 
 Tags { "QUEUE"="Transparent" }


 // Stats for Vertex shader:
 //       d3d11 : 10 math
 //        d3d9 : 14 math
 //      opengl : 14 math
 // Stats for Fragment shader:
 //       d3d11 : 178 math, 6 texture, 3 branch
 //        d3d9 : 269 math, 6 texture, 2 branch
 //      opengl : 221 math, 6 texture
 Pass {
  Tags { "QUEUE"="Transparent" }
  Cull Front
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "opengl " {
// Stats: 14 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 1 [_Globals_CameraToWorld]
Matrix 5 [_Globals_ScreenToCamera]
Matrix 9 [_Sun_WorldToLocal]
"3.0-!!ARBvp1.0
PARAM c[13] = { { 1, 0 },
		program.local[1..12] };
TEMP R0;
TEMP R1;
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MOV R1.w, c[0].y;
DP4 R1.z, vertex.position, c[7];
DP4 R0.z, R1, c[3];
DP4 R0.x, R1, c[1];
DP4 R0.y, R1, c[2];
MOV result.texcoord[1].xyz, R0;
DP3 result.texcoord[2].z, R0, c[11];
DP3 result.texcoord[2].y, R0, c[10];
DP3 result.texcoord[2].x, R0, c[9];
MOV result.position.xy, vertex.position;
MOV result.position.zw, c[0].x;
MOV result.texcoord[0].xy, vertex.texcoord[0];
END
# 14 instructions, 2 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 14 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [_Globals_CameraToWorld]
Matrix 4 [_Globals_ScreenToCamera]
Matrix 8 [_Sun_WorldToLocal]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
def c12, 0.00000000, 1.00000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v1
dp4 r1.x, v0, c4
dp4 r1.y, v0, c5
mov r1.w, c12.x
dp4 r1.z, v0, c6
dp4 r0.z, r1, c2
dp4 r0.x, r1, c0
dp4 r0.y, r1, c1
mov o2.xyz, r0
dp3 o3.z, r0, c10
dp3 o3.y, r0, c9
dp3 o3.x, r0, c8
mov o0.xy, v0
mov o0.zw, c12.y
mov o1.xy, v1
"
}
SubProgram "d3d11 " {
// Stats: 10 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "$Globals" 384
Matrix 144 [_Globals_CameraToWorld]
Matrix 208 [_Globals_ScreenToCamera]
Matrix 320 [_Sun_WorldToLocal]
BindCB  "$Globals" 0
"vs_4_0
eefiecedlcindejcjibafomacbmhonlpccdhadaaabaaaaaaheadaaaaadaaaaaa
cmaaaaaakaaaaaaaciabaaaaejfdeheogmaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaafjaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahaaaaaagaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apadaaaafaepfdejfeejepeoaaeoepfcenebemaafeeffiedepepfceeaaklklkl
epfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
heaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaahaiaaaaheaaaaaaacaaaaaa
aaaaaaaaadaaaaaaadaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffied
epepfceeaaklklklfdeieefceeacaaaaeaaaabaajbaaaaaafjaaaaaeegiocaaa
aaaaaaaabhaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaacaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagfaaaaad
hccabaaaacaaaaaagfaaaaadhccabaaaadaaaaaagiaaaaacacaaaaaadgaaaaaf
dccabaaaaaaaaaaaegbabaaaaaaaaaaadgaaaaaimccabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaiadpaaaaiadpdgaaaaafdccabaaaabaaaaaaegbabaaa
acaaaaaadiaaaaaihcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiccaaaaaaaaaaa
aoaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaaaaaaaaaanaaaaaaagbabaaa
aaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaaaaaaaaa
apaaaaaakgbkbaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaa
egiccaaaaaaaaaaabaaaaaaapgbpbaaaaaaaaaaaegacbaaaaaaaaaaadiaaaaai
hcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaaaaaaaaaaakaaaaaadcaaaaak
lcaabaaaaaaaaaaaegiicaaaaaaaaaaaajaaaaaaagaabaaaaaaaaaaaegaibaaa
abaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaaaaaaaaaalaaaaaakgakbaaa
aaaaaaaaegadbaaaaaaaaaaadgaaaaafhccabaaaacaaaaaaegacbaaaaaaaaaaa
diaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaaaaaaaaaabfaaaaaa
dcaaaaaklcaabaaaaaaaaaaaegiicaaaaaaaaaaabeaaaaaaagaabaaaaaaaaaaa
egaibaaaabaaaaaadcaaaaakhccabaaaadaaaaaaegiccaaaaaaaaaaabgaaaaaa
kgakbaaaaaaaaaaaegadbaaaaaaaaaaadoaaaaab"
}
SubProgram "gles " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 _Globals_CameraToWorld;
uniform highp mat4 _Globals_ScreenToCamera;
uniform highp mat4 _Sun_WorldToLocal;
varying highp vec2 xlv_TEXCOORD0;
varying highp vec3 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD2;
void main ()
{
  highp vec4 tmpvar_1;
  tmpvar_1.w = 0.0;
  tmpvar_1.xyz = (_Globals_ScreenToCamera * _glesVertex).xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (_Globals_CameraToWorld * tmpvar_1).xyz;
  mat3 tmpvar_3;
  tmpvar_3[0] = _Sun_WorldToLocal[0].xyz;
  tmpvar_3[1] = _Sun_WorldToLocal[1].xyz;
  tmpvar_3[2] = _Sun_WorldToLocal[2].xyz;
  highp vec4 tmpvar_4;
  tmpvar_4.zw = vec2(1.0, 1.0);
  tmpvar_4.xy = _glesVertex.xy;
  gl_Position = tmpvar_4;
  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
  xlv_TEXCOORD1 = tmpvar_2;
  xlv_TEXCOORD2 = (tmpvar_3 * tmpvar_2);
}



#endif
#ifdef FRAGMENT

uniform highp float _Exposure;
uniform highp float Rg;
uniform highp float Rt;
uniform highp vec3 betaR;
uniform highp float mieG;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
uniform highp float RES_NU;
uniform highp float _Sun_Intensity;
uniform sampler2D _Sky_Transmittance;
uniform sampler2D _Sky_Inscatter;
uniform highp float _Alpha_Cutoff;
uniform highp float _Alpha_Global;
uniform highp vec3 _Globals_WorldCameraPos;
uniform highp vec3 _Globals_Origin;
uniform sampler2D _Sun_Glare;
uniform highp vec3 _Sun_WorldSunDir;
varying highp vec3 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD2;
void main ()
{
  highp vec4 tmpvar_1;
  highp vec3 sunColor_2;
  highp vec3 tmpvar_3;
  tmpvar_3 = normalize(xlv_TEXCOORD1);
  highp vec3 data_4;
  lowp vec3 tmpvar_5;
  if ((xlv_TEXCOORD2.z > 0.0)) {
    highp vec2 P_6;
    P_6 = (vec2(0.5, 0.5) + (xlv_TEXCOORD2.xy * 4.0));
    tmpvar_5 = texture2D (_Sun_Glare, P_6).xyz;
  } else {
    tmpvar_5 = vec3(0.0, 0.0, 0.0);
  };
  data_4 = tmpvar_5;
  sunColor_2 = (pow (max (vec3(0.0, 0.0, 0.0), data_4), vec3(2.2, 2.2, 2.2)) * _Sun_Intensity);
  highp vec3 camera_7;
  camera_7 = (_Globals_WorldCameraPos + _Globals_Origin);
  highp vec3 extinction_8;
  highp float mu_9;
  highp float rMu_10;
  highp float r_11;
  highp vec3 result_12;
  result_12 = vec3(0.0, 0.0, 0.0);
  highp float tmpvar_13;
  tmpvar_13 = sqrt(dot (camera_7, camera_7));
  r_11 = tmpvar_13;
  highp float tmpvar_14;
  tmpvar_14 = dot (camera_7, tmpvar_3);
  rMu_10 = tmpvar_14;
  mu_9 = (tmpvar_14 / tmpvar_13);
  highp float f_15;
  f_15 = (((tmpvar_14 * tmpvar_14) - (tmpvar_13 * tmpvar_13)) + (Rt * Rt));
  highp float tmpvar_16;
  if ((f_15 >= 0.0)) {
    tmpvar_16 = sqrt(f_15);
  } else {
    tmpvar_16 = 1e+30;
  };
  highp float tmpvar_17;
  tmpvar_17 = max ((-(tmpvar_14) - tmpvar_16), 0.0);
  if ((tmpvar_17 > 0.0)) {
    camera_7 = (camera_7 + (tmpvar_17 * tmpvar_3));
    highp float tmpvar_18;
    tmpvar_18 = (tmpvar_14 + tmpvar_17);
    rMu_10 = tmpvar_18;
    mu_9 = (tmpvar_18 / Rt);
    r_11 = Rt;
  };
  highp float tmpvar_19;
  tmpvar_19 = dot (tmpvar_3, _Sun_WorldSunDir);
  highp float tmpvar_20;
  tmpvar_20 = (dot (camera_7, _Sun_WorldSunDir) / r_11);
  highp vec4 tmpvar_21;
  highp float uMu_22;
  highp float uR_23;
  highp float tmpvar_24;
  tmpvar_24 = sqrt(((Rt * Rt) - (Rg * Rg)));
  highp float tmpvar_25;
  tmpvar_25 = sqrt(((r_11 * r_11) - (Rg * Rg)));
  highp float tmpvar_26;
  tmpvar_26 = (r_11 * (rMu_10 / r_11));
  highp float tmpvar_27;
  tmpvar_27 = (((tmpvar_26 * tmpvar_26) - (r_11 * r_11)) + (Rg * Rg));
  highp vec4 tmpvar_28;
  if (((tmpvar_26 < 0.0) && (tmpvar_27 > 0.0))) {
    highp vec4 tmpvar_29;
    tmpvar_29.xyz = vec3(1.0, 0.0, 0.0);
    tmpvar_29.w = (0.5 - (0.5 / RES_MU));
    tmpvar_28 = tmpvar_29;
  } else {
    highp vec4 tmpvar_30;
    tmpvar_30.x = -1.0;
    tmpvar_30.y = (tmpvar_24 * tmpvar_24);
    tmpvar_30.z = tmpvar_24;
    tmpvar_30.w = (0.5 + (0.5 / RES_MU));
    tmpvar_28 = tmpvar_30;
  };
  uR_23 = ((0.5 / RES_R) + ((tmpvar_25 / tmpvar_24) * (1.0 - (1.0/(RES_R)))));
  uMu_22 = (tmpvar_28.w + ((((tmpvar_26 * tmpvar_28.x) + sqrt((tmpvar_27 + tmpvar_28.y))) / (tmpvar_25 + tmpvar_28.z)) * (0.5 - (1.0/(RES_MU)))));
  highp float y_over_x_31;
  y_over_x_31 = (max (tmpvar_20, -0.1975) * 5.34962);
  highp float x_32;
  x_32 = (y_over_x_31 * inversesqrt(((y_over_x_31 * y_over_x_31) + 1.0)));
  highp float tmpvar_33;
  tmpvar_33 = ((0.5 / RES_MU_S) + (((((sign(x_32) * (1.5708 - (sqrt((1.0 - abs(x_32))) * (1.5708 + (abs(x_32) * (-0.214602 + (abs(x_32) * (0.0865667 + (abs(x_32) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
  highp float tmpvar_34;
  tmpvar_34 = (((tmpvar_19 + 1.0) / 2.0) * (RES_NU - 1.0));
  highp float tmpvar_35;
  tmpvar_35 = floor(tmpvar_34);
  highp float tmpvar_36;
  tmpvar_36 = (tmpvar_34 - tmpvar_35);
  highp float tmpvar_37;
  tmpvar_37 = (floor(((uR_23 * RES_R) - 1.0)) / RES_R);
  highp float tmpvar_38;
  tmpvar_38 = (floor((uR_23 * RES_R)) / RES_R);
  highp float tmpvar_39;
  tmpvar_39 = fract((uR_23 * RES_R));
  highp vec2 tmpvar_40;
  tmpvar_40.x = ((tmpvar_35 + tmpvar_33) / RES_NU);
  tmpvar_40.y = ((uMu_22 / RES_R) + tmpvar_37);
  lowp vec4 tmpvar_41;
  tmpvar_41 = texture2D (_Sky_Inscatter, tmpvar_40);
  highp vec2 tmpvar_42;
  tmpvar_42.x = (((tmpvar_35 + tmpvar_33) + 1.0) / RES_NU);
  tmpvar_42.y = ((uMu_22 / RES_R) + tmpvar_37);
  lowp vec4 tmpvar_43;
  tmpvar_43 = texture2D (_Sky_Inscatter, tmpvar_42);
  highp vec2 tmpvar_44;
  tmpvar_44.x = ((tmpvar_35 + tmpvar_33) / RES_NU);
  tmpvar_44.y = ((uMu_22 / RES_R) + tmpvar_38);
  lowp vec4 tmpvar_45;
  tmpvar_45 = texture2D (_Sky_Inscatter, tmpvar_44);
  highp vec2 tmpvar_46;
  tmpvar_46.x = (((tmpvar_35 + tmpvar_33) + 1.0) / RES_NU);
  tmpvar_46.y = ((uMu_22 / RES_R) + tmpvar_38);
  lowp vec4 tmpvar_47;
  tmpvar_47 = texture2D (_Sky_Inscatter, tmpvar_46);
  tmpvar_21 = ((((tmpvar_41 * (1.0 - tmpvar_36)) + (tmpvar_43 * tmpvar_36)) * (1.0 - tmpvar_39)) + (((tmpvar_45 * (1.0 - tmpvar_36)) + (tmpvar_47 * tmpvar_36)) * tmpvar_39));
  highp vec3 tmpvar_48;
  highp float y_over_x_49;
  y_over_x_49 = (((mu_9 + 0.15) / 1.15) * 14.1014);
  highp float x_50;
  x_50 = (y_over_x_49 * inversesqrt(((y_over_x_49 * y_over_x_49) + 1.0)));
  highp vec2 tmpvar_51;
  tmpvar_51.x = ((sign(x_50) * (1.5708 - (sqrt((1.0 - abs(x_50))) * (1.5708 + (abs(x_50) * (-0.214602 + (abs(x_50) * (0.0865667 + (abs(x_50) * -0.0310296))))))))) / 1.5);
  tmpvar_51.y = sqrt(((r_11 - Rg) / (Rt - Rg)));
  lowp vec4 tmpvar_52;
  tmpvar_52 = texture2D (_Sky_Transmittance, tmpvar_51);
  tmpvar_48 = tmpvar_52.xyz;
  extinction_8 = tmpvar_48;
  if ((r_11 <= Rt)) {
    result_12 = ((tmpvar_21.xyz * (0.0596831 * (1.0 + (tmpvar_19 * tmpvar_19)))) + ((((tmpvar_21.xyz * tmpvar_21.w) / max (tmpvar_21.x, 0.0001)) * (betaR.x / betaR)) * ((((0.119366 * (1.0 - (mieG * mieG))) * pow (((1.0 + (mieG * mieG)) - ((2.0 * mieG) * tmpvar_19)), -1.5)) * (1.0 + (tmpvar_19 * tmpvar_19))) / (2.0 + (mieG * mieG)))));
  } else {
    result_12 = vec3(0.0, 0.0, 0.0);
    extinction_8 = vec3(1.0, 1.0, 1.0);
  };
  highp vec3 tmpvar_53;
  tmpvar_53 = ((sunColor_2 * extinction_8) + (result_12 * _Sun_Intensity));
  highp vec3 tmpvar_54;
  tmpvar_54 = abs(tmpvar_53);
  if ((tmpvar_54.x <= _Alpha_Cutoff)) {
    highp vec3 L_55;
    highp vec3 tmpvar_56;
    tmpvar_56 = (tmpvar_53 * _Exposure);
    L_55 = tmpvar_56;
    highp float tmpvar_57;
    if ((tmpvar_56.x < 1.413)) {
      tmpvar_57 = pow ((tmpvar_56.x * 0.38317), 0.454545);
    } else {
      tmpvar_57 = (1.0 - exp(-(tmpvar_56.x)));
    };
    L_55.x = tmpvar_57;
    highp float tmpvar_58;
    if ((tmpvar_56.y < 1.413)) {
      tmpvar_58 = pow ((tmpvar_56.y * 0.38317), 0.454545);
    } else {
      tmpvar_58 = (1.0 - exp(-(tmpvar_56.y)));
    };
    L_55.y = tmpvar_58;
    highp float tmpvar_59;
    if ((tmpvar_56.z < 1.413)) {
      tmpvar_59 = pow ((tmpvar_56.z * 0.38317), 0.454545);
    } else {
      tmpvar_59 = (1.0 - exp(-(tmpvar_56.z)));
    };
    L_55.z = tmpvar_59;
    highp vec4 tmpvar_60;
    tmpvar_60.xyz = L_55;
    tmpvar_60.w = (tmpvar_54.x * _Alpha_Global);
    tmpvar_1 = tmpvar_60;
  } else {
    highp vec3 L_61;
    highp vec3 tmpvar_62;
    tmpvar_62 = (tmpvar_53 * _Exposure);
    L_61 = tmpvar_62;
    highp float tmpvar_63;
    if ((tmpvar_62.x < 1.413)) {
      tmpvar_63 = pow ((tmpvar_62.x * 0.38317), 0.454545);
    } else {
      tmpvar_63 = (1.0 - exp(-(tmpvar_62.x)));
    };
    L_61.x = tmpvar_63;
    highp float tmpvar_64;
    if ((tmpvar_62.y < 1.413)) {
      tmpvar_64 = pow ((tmpvar_62.y * 0.38317), 0.454545);
    } else {
      tmpvar_64 = (1.0 - exp(-(tmpvar_62.y)));
    };
    L_61.y = tmpvar_64;
    highp float tmpvar_65;
    if ((tmpvar_62.z < 1.413)) {
      tmpvar_65 = pow ((tmpvar_62.z * 0.38317), 0.454545);
    } else {
      tmpvar_65 = (1.0 - exp(-(tmpvar_62.z)));
    };
    L_61.z = tmpvar_65;
    highp vec4 tmpvar_66;
    tmpvar_66.xyz = L_61;
    tmpvar_66.w = _Alpha_Global;
    tmpvar_1 = tmpvar_66;
  };
  gl_FragData[0] = tmpvar_1;
}



#endif"
}
SubProgram "glesdesktop " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 _Globals_CameraToWorld;
uniform highp mat4 _Globals_ScreenToCamera;
uniform highp mat4 _Sun_WorldToLocal;
varying highp vec2 xlv_TEXCOORD0;
varying highp vec3 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD2;
void main ()
{
  highp vec4 tmpvar_1;
  tmpvar_1.w = 0.0;
  tmpvar_1.xyz = (_Globals_ScreenToCamera * _glesVertex).xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (_Globals_CameraToWorld * tmpvar_1).xyz;
  mat3 tmpvar_3;
  tmpvar_3[0] = _Sun_WorldToLocal[0].xyz;
  tmpvar_3[1] = _Sun_WorldToLocal[1].xyz;
  tmpvar_3[2] = _Sun_WorldToLocal[2].xyz;
  highp vec4 tmpvar_4;
  tmpvar_4.zw = vec2(1.0, 1.0);
  tmpvar_4.xy = _glesVertex.xy;
  gl_Position = tmpvar_4;
  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
  xlv_TEXCOORD1 = tmpvar_2;
  xlv_TEXCOORD2 = (tmpvar_3 * tmpvar_2);
}



#endif
#ifdef FRAGMENT

uniform highp float _Exposure;
uniform highp float Rg;
uniform highp float Rt;
uniform highp vec3 betaR;
uniform highp float mieG;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
uniform highp float RES_NU;
uniform highp float _Sun_Intensity;
uniform sampler2D _Sky_Transmittance;
uniform sampler2D _Sky_Inscatter;
uniform highp float _Alpha_Cutoff;
uniform highp float _Alpha_Global;
uniform highp vec3 _Globals_WorldCameraPos;
uniform highp vec3 _Globals_Origin;
uniform sampler2D _Sun_Glare;
uniform highp vec3 _Sun_WorldSunDir;
varying highp vec3 xlv_TEXCOORD1;
varying highp vec3 xlv_TEXCOORD2;
void main ()
{
  highp vec4 tmpvar_1;
  highp vec3 sunColor_2;
  highp vec3 tmpvar_3;
  tmpvar_3 = normalize(xlv_TEXCOORD1);
  highp vec3 data_4;
  lowp vec3 tmpvar_5;
  if ((xlv_TEXCOORD2.z > 0.0)) {
    highp vec2 P_6;
    P_6 = (vec2(0.5, 0.5) + (xlv_TEXCOORD2.xy * 4.0));
    tmpvar_5 = texture2D (_Sun_Glare, P_6).xyz;
  } else {
    tmpvar_5 = vec3(0.0, 0.0, 0.0);
  };
  data_4 = tmpvar_5;
  sunColor_2 = (pow (max (vec3(0.0, 0.0, 0.0), data_4), vec3(2.2, 2.2, 2.2)) * _Sun_Intensity);
  highp vec3 camera_7;
  camera_7 = (_Globals_WorldCameraPos + _Globals_Origin);
  highp vec3 extinction_8;
  highp float mu_9;
  highp float rMu_10;
  highp float r_11;
  highp vec3 result_12;
  result_12 = vec3(0.0, 0.0, 0.0);
  highp float tmpvar_13;
  tmpvar_13 = sqrt(dot (camera_7, camera_7));
  r_11 = tmpvar_13;
  highp float tmpvar_14;
  tmpvar_14 = dot (camera_7, tmpvar_3);
  rMu_10 = tmpvar_14;
  mu_9 = (tmpvar_14 / tmpvar_13);
  highp float f_15;
  f_15 = (((tmpvar_14 * tmpvar_14) - (tmpvar_13 * tmpvar_13)) + (Rt * Rt));
  highp float tmpvar_16;
  if ((f_15 >= 0.0)) {
    tmpvar_16 = sqrt(f_15);
  } else {
    tmpvar_16 = 1e+30;
  };
  highp float tmpvar_17;
  tmpvar_17 = max ((-(tmpvar_14) - tmpvar_16), 0.0);
  if ((tmpvar_17 > 0.0)) {
    camera_7 = (camera_7 + (tmpvar_17 * tmpvar_3));
    highp float tmpvar_18;
    tmpvar_18 = (tmpvar_14 + tmpvar_17);
    rMu_10 = tmpvar_18;
    mu_9 = (tmpvar_18 / Rt);
    r_11 = Rt;
  };
  highp float tmpvar_19;
  tmpvar_19 = dot (tmpvar_3, _Sun_WorldSunDir);
  highp float tmpvar_20;
  tmpvar_20 = (dot (camera_7, _Sun_WorldSunDir) / r_11);
  highp vec4 tmpvar_21;
  highp float uMu_22;
  highp float uR_23;
  highp float tmpvar_24;
  tmpvar_24 = sqrt(((Rt * Rt) - (Rg * Rg)));
  highp float tmpvar_25;
  tmpvar_25 = sqrt(((r_11 * r_11) - (Rg * Rg)));
  highp float tmpvar_26;
  tmpvar_26 = (r_11 * (rMu_10 / r_11));
  highp float tmpvar_27;
  tmpvar_27 = (((tmpvar_26 * tmpvar_26) - (r_11 * r_11)) + (Rg * Rg));
  highp vec4 tmpvar_28;
  if (((tmpvar_26 < 0.0) && (tmpvar_27 > 0.0))) {
    highp vec4 tmpvar_29;
    tmpvar_29.xyz = vec3(1.0, 0.0, 0.0);
    tmpvar_29.w = (0.5 - (0.5 / RES_MU));
    tmpvar_28 = tmpvar_29;
  } else {
    highp vec4 tmpvar_30;
    tmpvar_30.x = -1.0;
    tmpvar_30.y = (tmpvar_24 * tmpvar_24);
    tmpvar_30.z = tmpvar_24;
    tmpvar_30.w = (0.5 + (0.5 / RES_MU));
    tmpvar_28 = tmpvar_30;
  };
  uR_23 = ((0.5 / RES_R) + ((tmpvar_25 / tmpvar_24) * (1.0 - (1.0/(RES_R)))));
  uMu_22 = (tmpvar_28.w + ((((tmpvar_26 * tmpvar_28.x) + sqrt((tmpvar_27 + tmpvar_28.y))) / (tmpvar_25 + tmpvar_28.z)) * (0.5 - (1.0/(RES_MU)))));
  highp float y_over_x_31;
  y_over_x_31 = (max (tmpvar_20, -0.1975) * 5.34962);
  highp float x_32;
  x_32 = (y_over_x_31 * inversesqrt(((y_over_x_31 * y_over_x_31) + 1.0)));
  highp float tmpvar_33;
  tmpvar_33 = ((0.5 / RES_MU_S) + (((((sign(x_32) * (1.5708 - (sqrt((1.0 - abs(x_32))) * (1.5708 + (abs(x_32) * (-0.214602 + (abs(x_32) * (0.0865667 + (abs(x_32) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
  highp float tmpvar_34;
  tmpvar_34 = (((tmpvar_19 + 1.0) / 2.0) * (RES_NU - 1.0));
  highp float tmpvar_35;
  tmpvar_35 = floor(tmpvar_34);
  highp float tmpvar_36;
  tmpvar_36 = (tmpvar_34 - tmpvar_35);
  highp float tmpvar_37;
  tmpvar_37 = (floor(((uR_23 * RES_R) - 1.0)) / RES_R);
  highp float tmpvar_38;
  tmpvar_38 = (floor((uR_23 * RES_R)) / RES_R);
  highp float tmpvar_39;
  tmpvar_39 = fract((uR_23 * RES_R));
  highp vec2 tmpvar_40;
  tmpvar_40.x = ((tmpvar_35 + tmpvar_33) / RES_NU);
  tmpvar_40.y = ((uMu_22 / RES_R) + tmpvar_37);
  lowp vec4 tmpvar_41;
  tmpvar_41 = texture2D (_Sky_Inscatter, tmpvar_40);
  highp vec2 tmpvar_42;
  tmpvar_42.x = (((tmpvar_35 + tmpvar_33) + 1.0) / RES_NU);
  tmpvar_42.y = ((uMu_22 / RES_R) + tmpvar_37);
  lowp vec4 tmpvar_43;
  tmpvar_43 = texture2D (_Sky_Inscatter, tmpvar_42);
  highp vec2 tmpvar_44;
  tmpvar_44.x = ((tmpvar_35 + tmpvar_33) / RES_NU);
  tmpvar_44.y = ((uMu_22 / RES_R) + tmpvar_38);
  lowp vec4 tmpvar_45;
  tmpvar_45 = texture2D (_Sky_Inscatter, tmpvar_44);
  highp vec2 tmpvar_46;
  tmpvar_46.x = (((tmpvar_35 + tmpvar_33) + 1.0) / RES_NU);
  tmpvar_46.y = ((uMu_22 / RES_R) + tmpvar_38);
  lowp vec4 tmpvar_47;
  tmpvar_47 = texture2D (_Sky_Inscatter, tmpvar_46);
  tmpvar_21 = ((((tmpvar_41 * (1.0 - tmpvar_36)) + (tmpvar_43 * tmpvar_36)) * (1.0 - tmpvar_39)) + (((tmpvar_45 * (1.0 - tmpvar_36)) + (tmpvar_47 * tmpvar_36)) * tmpvar_39));
  highp vec3 tmpvar_48;
  highp float y_over_x_49;
  y_over_x_49 = (((mu_9 + 0.15) / 1.15) * 14.1014);
  highp float x_50;
  x_50 = (y_over_x_49 * inversesqrt(((y_over_x_49 * y_over_x_49) + 1.0)));
  highp vec2 tmpvar_51;
  tmpvar_51.x = ((sign(x_50) * (1.5708 - (sqrt((1.0 - abs(x_50))) * (1.5708 + (abs(x_50) * (-0.214602 + (abs(x_50) * (0.0865667 + (abs(x_50) * -0.0310296))))))))) / 1.5);
  tmpvar_51.y = sqrt(((r_11 - Rg) / (Rt - Rg)));
  lowp vec4 tmpvar_52;
  tmpvar_52 = texture2D (_Sky_Transmittance, tmpvar_51);
  tmpvar_48 = tmpvar_52.xyz;
  extinction_8 = tmpvar_48;
  if ((r_11 <= Rt)) {
    result_12 = ((tmpvar_21.xyz * (0.0596831 * (1.0 + (tmpvar_19 * tmpvar_19)))) + ((((tmpvar_21.xyz * tmpvar_21.w) / max (tmpvar_21.x, 0.0001)) * (betaR.x / betaR)) * ((((0.119366 * (1.0 - (mieG * mieG))) * pow (((1.0 + (mieG * mieG)) - ((2.0 * mieG) * tmpvar_19)), -1.5)) * (1.0 + (tmpvar_19 * tmpvar_19))) / (2.0 + (mieG * mieG)))));
  } else {
    result_12 = vec3(0.0, 0.0, 0.0);
    extinction_8 = vec3(1.0, 1.0, 1.0);
  };
  highp vec3 tmpvar_53;
  tmpvar_53 = ((sunColor_2 * extinction_8) + (result_12 * _Sun_Intensity));
  highp vec3 tmpvar_54;
  tmpvar_54 = abs(tmpvar_53);
  if ((tmpvar_54.x <= _Alpha_Cutoff)) {
    highp vec3 L_55;
    highp vec3 tmpvar_56;
    tmpvar_56 = (tmpvar_53 * _Exposure);
    L_55 = tmpvar_56;
    highp float tmpvar_57;
    if ((tmpvar_56.x < 1.413)) {
      tmpvar_57 = pow ((tmpvar_56.x * 0.38317), 0.454545);
    } else {
      tmpvar_57 = (1.0 - exp(-(tmpvar_56.x)));
    };
    L_55.x = tmpvar_57;
    highp float tmpvar_58;
    if ((tmpvar_56.y < 1.413)) {
      tmpvar_58 = pow ((tmpvar_56.y * 0.38317), 0.454545);
    } else {
      tmpvar_58 = (1.0 - exp(-(tmpvar_56.y)));
    };
    L_55.y = tmpvar_58;
    highp float tmpvar_59;
    if ((tmpvar_56.z < 1.413)) {
      tmpvar_59 = pow ((tmpvar_56.z * 0.38317), 0.454545);
    } else {
      tmpvar_59 = (1.0 - exp(-(tmpvar_56.z)));
    };
    L_55.z = tmpvar_59;
    highp vec4 tmpvar_60;
    tmpvar_60.xyz = L_55;
    tmpvar_60.w = (tmpvar_54.x * _Alpha_Global);
    tmpvar_1 = tmpvar_60;
  } else {
    highp vec3 L_61;
    highp vec3 tmpvar_62;
    tmpvar_62 = (tmpvar_53 * _Exposure);
    L_61 = tmpvar_62;
    highp float tmpvar_63;
    if ((tmpvar_62.x < 1.413)) {
      tmpvar_63 = pow ((tmpvar_62.x * 0.38317), 0.454545);
    } else {
      tmpvar_63 = (1.0 - exp(-(tmpvar_62.x)));
    };
    L_61.x = tmpvar_63;
    highp float tmpvar_64;
    if ((tmpvar_62.y < 1.413)) {
      tmpvar_64 = pow ((tmpvar_62.y * 0.38317), 0.454545);
    } else {
      tmpvar_64 = (1.0 - exp(-(tmpvar_62.y)));
    };
    L_61.y = tmpvar_64;
    highp float tmpvar_65;
    if ((tmpvar_62.z < 1.413)) {
      tmpvar_65 = pow ((tmpvar_62.z * 0.38317), 0.454545);
    } else {
      tmpvar_65 = (1.0 - exp(-(tmpvar_62.z)));
    };
    L_61.z = tmpvar_65;
    highp vec4 tmpvar_66;
    tmpvar_66.xyz = L_61;
    tmpvar_66.w = _Alpha_Global;
    tmpvar_1 = tmpvar_66;
  };
  gl_FragData[0] = tmpvar_1;
}



#endif"
}
SubProgram "gles3 " {
"!!GLES3#version 300 es


#ifdef VERTEX

in vec4 _glesVertex;
in vec4 _glesMultiTexCoord0;
uniform highp mat4 _Globals_CameraToWorld;
uniform highp mat4 _Globals_ScreenToCamera;
uniform highp mat4 _Sun_WorldToLocal;
out highp vec2 xlv_TEXCOORD0;
out highp vec3 xlv_TEXCOORD1;
out highp vec3 xlv_TEXCOORD2;
void main ()
{
  highp vec4 tmpvar_1;
  tmpvar_1.w = 0.0;
  tmpvar_1.xyz = (_Globals_ScreenToCamera * _glesVertex).xyz;
  highp vec3 tmpvar_2;
  tmpvar_2 = (_Globals_CameraToWorld * tmpvar_1).xyz;
  mat3 tmpvar_3;
  tmpvar_3[0] = _Sun_WorldToLocal[0].xyz;
  tmpvar_3[1] = _Sun_WorldToLocal[1].xyz;
  tmpvar_3[2] = _Sun_WorldToLocal[2].xyz;
  highp vec4 tmpvar_4;
  tmpvar_4.zw = vec2(1.0, 1.0);
  tmpvar_4.xy = _glesVertex.xy;
  gl_Position = tmpvar_4;
  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
  xlv_TEXCOORD1 = tmpvar_2;
  xlv_TEXCOORD2 = (tmpvar_3 * tmpvar_2);
}



#endif
#ifdef FRAGMENT

out mediump vec4 _glesFragData[4];
uniform highp float _Exposure;
uniform highp float Rg;
uniform highp float Rt;
uniform highp vec3 betaR;
uniform highp float mieG;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
uniform highp float RES_NU;
uniform highp float _Sun_Intensity;
uniform sampler2D _Sky_Transmittance;
uniform sampler2D _Sky_Inscatter;
uniform highp float _Alpha_Cutoff;
uniform highp float _Alpha_Global;
uniform highp vec3 _Globals_WorldCameraPos;
uniform highp vec3 _Globals_Origin;
uniform sampler2D _Sun_Glare;
uniform highp vec3 _Sun_WorldSunDir;
in highp vec3 xlv_TEXCOORD1;
in highp vec3 xlv_TEXCOORD2;
void main ()
{
  highp vec4 tmpvar_1;
  highp vec3 sunColor_2;
  highp vec3 tmpvar_3;
  tmpvar_3 = normalize(xlv_TEXCOORD1);
  highp vec3 data_4;
  lowp vec3 tmpvar_5;
  if ((xlv_TEXCOORD2.z > 0.0)) {
    highp vec2 P_6;
    P_6 = (vec2(0.5, 0.5) + (xlv_TEXCOORD2.xy * 4.0));
    tmpvar_5 = texture (_Sun_Glare, P_6).xyz;
  } else {
    tmpvar_5 = vec3(0.0, 0.0, 0.0);
  };
  data_4 = tmpvar_5;
  sunColor_2 = (pow (max (vec3(0.0, 0.0, 0.0), data_4), vec3(2.2, 2.2, 2.2)) * _Sun_Intensity);
  highp vec3 camera_7;
  camera_7 = (_Globals_WorldCameraPos + _Globals_Origin);
  highp vec3 extinction_8;
  highp float mu_9;
  highp float rMu_10;
  highp float r_11;
  highp vec3 result_12;
  result_12 = vec3(0.0, 0.0, 0.0);
  highp float tmpvar_13;
  tmpvar_13 = sqrt(dot (camera_7, camera_7));
  r_11 = tmpvar_13;
  highp float tmpvar_14;
  tmpvar_14 = dot (camera_7, tmpvar_3);
  rMu_10 = tmpvar_14;
  mu_9 = (tmpvar_14 / tmpvar_13);
  highp float f_15;
  f_15 = (((tmpvar_14 * tmpvar_14) - (tmpvar_13 * tmpvar_13)) + (Rt * Rt));
  highp float tmpvar_16;
  if ((f_15 >= 0.0)) {
    tmpvar_16 = sqrt(f_15);
  } else {
    tmpvar_16 = 1e+30;
  };
  highp float tmpvar_17;
  tmpvar_17 = max ((-(tmpvar_14) - tmpvar_16), 0.0);
  if ((tmpvar_17 > 0.0)) {
    camera_7 = (camera_7 + (tmpvar_17 * tmpvar_3));
    highp float tmpvar_18;
    tmpvar_18 = (tmpvar_14 + tmpvar_17);
    rMu_10 = tmpvar_18;
    mu_9 = (tmpvar_18 / Rt);
    r_11 = Rt;
  };
  highp float tmpvar_19;
  tmpvar_19 = dot (tmpvar_3, _Sun_WorldSunDir);
  highp float tmpvar_20;
  tmpvar_20 = (dot (camera_7, _Sun_WorldSunDir) / r_11);
  highp vec4 tmpvar_21;
  highp float uMu_22;
  highp float uR_23;
  highp float tmpvar_24;
  tmpvar_24 = sqrt(((Rt * Rt) - (Rg * Rg)));
  highp float tmpvar_25;
  tmpvar_25 = sqrt(((r_11 * r_11) - (Rg * Rg)));
  highp float tmpvar_26;
  tmpvar_26 = (r_11 * (rMu_10 / r_11));
  highp float tmpvar_27;
  tmpvar_27 = (((tmpvar_26 * tmpvar_26) - (r_11 * r_11)) + (Rg * Rg));
  highp vec4 tmpvar_28;
  if (((tmpvar_26 < 0.0) && (tmpvar_27 > 0.0))) {
    highp vec4 tmpvar_29;
    tmpvar_29.xyz = vec3(1.0, 0.0, 0.0);
    tmpvar_29.w = (0.5 - (0.5 / RES_MU));
    tmpvar_28 = tmpvar_29;
  } else {
    highp vec4 tmpvar_30;
    tmpvar_30.x = -1.0;
    tmpvar_30.y = (tmpvar_24 * tmpvar_24);
    tmpvar_30.z = tmpvar_24;
    tmpvar_30.w = (0.5 + (0.5 / RES_MU));
    tmpvar_28 = tmpvar_30;
  };
  uR_23 = ((0.5 / RES_R) + ((tmpvar_25 / tmpvar_24) * (1.0 - (1.0/(RES_R)))));
  uMu_22 = (tmpvar_28.w + ((((tmpvar_26 * tmpvar_28.x) + sqrt((tmpvar_27 + tmpvar_28.y))) / (tmpvar_25 + tmpvar_28.z)) * (0.5 - (1.0/(RES_MU)))));
  highp float y_over_x_31;
  y_over_x_31 = (max (tmpvar_20, -0.1975) * 5.34962);
  highp float x_32;
  x_32 = (y_over_x_31 * inversesqrt(((y_over_x_31 * y_over_x_31) + 1.0)));
  highp float tmpvar_33;
  tmpvar_33 = ((0.5 / RES_MU_S) + (((((sign(x_32) * (1.5708 - (sqrt((1.0 - abs(x_32))) * (1.5708 + (abs(x_32) * (-0.214602 + (abs(x_32) * (0.0865667 + (abs(x_32) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
  highp float tmpvar_34;
  tmpvar_34 = (((tmpvar_19 + 1.0) / 2.0) * (RES_NU - 1.0));
  highp float tmpvar_35;
  tmpvar_35 = floor(tmpvar_34);
  highp float tmpvar_36;
  tmpvar_36 = (tmpvar_34 - tmpvar_35);
  highp float tmpvar_37;
  tmpvar_37 = (floor(((uR_23 * RES_R) - 1.0)) / RES_R);
  highp float tmpvar_38;
  tmpvar_38 = (floor((uR_23 * RES_R)) / RES_R);
  highp float tmpvar_39;
  tmpvar_39 = fract((uR_23 * RES_R));
  highp vec2 tmpvar_40;
  tmpvar_40.x = ((tmpvar_35 + tmpvar_33) / RES_NU);
  tmpvar_40.y = ((uMu_22 / RES_R) + tmpvar_37);
  lowp vec4 tmpvar_41;
  tmpvar_41 = texture (_Sky_Inscatter, tmpvar_40);
  highp vec2 tmpvar_42;
  tmpvar_42.x = (((tmpvar_35 + tmpvar_33) + 1.0) / RES_NU);
  tmpvar_42.y = ((uMu_22 / RES_R) + tmpvar_37);
  lowp vec4 tmpvar_43;
  tmpvar_43 = texture (_Sky_Inscatter, tmpvar_42);
  highp vec2 tmpvar_44;
  tmpvar_44.x = ((tmpvar_35 + tmpvar_33) / RES_NU);
  tmpvar_44.y = ((uMu_22 / RES_R) + tmpvar_38);
  lowp vec4 tmpvar_45;
  tmpvar_45 = texture (_Sky_Inscatter, tmpvar_44);
  highp vec2 tmpvar_46;
  tmpvar_46.x = (((tmpvar_35 + tmpvar_33) + 1.0) / RES_NU);
  tmpvar_46.y = ((uMu_22 / RES_R) + tmpvar_38);
  lowp vec4 tmpvar_47;
  tmpvar_47 = texture (_Sky_Inscatter, tmpvar_46);
  tmpvar_21 = ((((tmpvar_41 * (1.0 - tmpvar_36)) + (tmpvar_43 * tmpvar_36)) * (1.0 - tmpvar_39)) + (((tmpvar_45 * (1.0 - tmpvar_36)) + (tmpvar_47 * tmpvar_36)) * tmpvar_39));
  highp vec3 tmpvar_48;
  highp float y_over_x_49;
  y_over_x_49 = (((mu_9 + 0.15) / 1.15) * 14.1014);
  highp float x_50;
  x_50 = (y_over_x_49 * inversesqrt(((y_over_x_49 * y_over_x_49) + 1.0)));
  highp vec2 tmpvar_51;
  tmpvar_51.x = ((sign(x_50) * (1.5708 - (sqrt((1.0 - abs(x_50))) * (1.5708 + (abs(x_50) * (-0.214602 + (abs(x_50) * (0.0865667 + (abs(x_50) * -0.0310296))))))))) / 1.5);
  tmpvar_51.y = sqrt(((r_11 - Rg) / (Rt - Rg)));
  lowp vec4 tmpvar_52;
  tmpvar_52 = texture (_Sky_Transmittance, tmpvar_51);
  tmpvar_48 = tmpvar_52.xyz;
  extinction_8 = tmpvar_48;
  if ((r_11 <= Rt)) {
    result_12 = ((tmpvar_21.xyz * (0.0596831 * (1.0 + (tmpvar_19 * tmpvar_19)))) + ((((tmpvar_21.xyz * tmpvar_21.w) / max (tmpvar_21.x, 0.0001)) * (betaR.x / betaR)) * ((((0.119366 * (1.0 - (mieG * mieG))) * pow (((1.0 + (mieG * mieG)) - ((2.0 * mieG) * tmpvar_19)), -1.5)) * (1.0 + (tmpvar_19 * tmpvar_19))) / (2.0 + (mieG * mieG)))));
  } else {
    result_12 = vec3(0.0, 0.0, 0.0);
    extinction_8 = vec3(1.0, 1.0, 1.0);
  };
  highp vec3 tmpvar_53;
  tmpvar_53 = ((sunColor_2 * extinction_8) + (result_12 * _Sun_Intensity));
  highp vec3 tmpvar_54;
  tmpvar_54 = abs(tmpvar_53);
  if ((tmpvar_54.x <= _Alpha_Cutoff)) {
    highp vec3 L_55;
    highp vec3 tmpvar_56;
    tmpvar_56 = (tmpvar_53 * _Exposure);
    L_55 = tmpvar_56;
    highp float tmpvar_57;
    if ((tmpvar_56.x < 1.413)) {
      tmpvar_57 = pow ((tmpvar_56.x * 0.38317), 0.454545);
    } else {
      tmpvar_57 = (1.0 - exp(-(tmpvar_56.x)));
    };
    L_55.x = tmpvar_57;
    highp float tmpvar_58;
    if ((tmpvar_56.y < 1.413)) {
      tmpvar_58 = pow ((tmpvar_56.y * 0.38317), 0.454545);
    } else {
      tmpvar_58 = (1.0 - exp(-(tmpvar_56.y)));
    };
    L_55.y = tmpvar_58;
    highp float tmpvar_59;
    if ((tmpvar_56.z < 1.413)) {
      tmpvar_59 = pow ((tmpvar_56.z * 0.38317), 0.454545);
    } else {
      tmpvar_59 = (1.0 - exp(-(tmpvar_56.z)));
    };
    L_55.z = tmpvar_59;
    highp vec4 tmpvar_60;
    tmpvar_60.xyz = L_55;
    tmpvar_60.w = (tmpvar_54.x * _Alpha_Global);
    tmpvar_1 = tmpvar_60;
  } else {
    highp vec3 L_61;
    highp vec3 tmpvar_62;
    tmpvar_62 = (tmpvar_53 * _Exposure);
    L_61 = tmpvar_62;
    highp float tmpvar_63;
    if ((tmpvar_62.x < 1.413)) {
      tmpvar_63 = pow ((tmpvar_62.x * 0.38317), 0.454545);
    } else {
      tmpvar_63 = (1.0 - exp(-(tmpvar_62.x)));
    };
    L_61.x = tmpvar_63;
    highp float tmpvar_64;
    if ((tmpvar_62.y < 1.413)) {
      tmpvar_64 = pow ((tmpvar_62.y * 0.38317), 0.454545);
    } else {
      tmpvar_64 = (1.0 - exp(-(tmpvar_62.y)));
    };
    L_61.y = tmpvar_64;
    highp float tmpvar_65;
    if ((tmpvar_62.z < 1.413)) {
      tmpvar_65 = pow ((tmpvar_62.z * 0.38317), 0.454545);
    } else {
      tmpvar_65 = (1.0 - exp(-(tmpvar_62.z)));
    };
    L_61.z = tmpvar_65;
    highp vec4 tmpvar_66;
    tmpvar_66.xyz = L_61;
    tmpvar_66.w = _Alpha_Global;
    tmpvar_1 = tmpvar_66;
  };
  _glesFragData[0] = tmpvar_1;
}



#endif"
}
}
Program "fp" {
SubProgram "opengl " {
// Stats: 221 math, 6 textures
Float 0 [_Exposure]
Float 1 [Rg]
Float 2 [Rt]
Vector 3 [betaR]
Float 4 [mieG]
Float 5 [RES_R]
Float 6 [RES_MU]
Float 7 [RES_MU_S]
Float 8 [RES_NU]
Float 9 [_Sun_Intensity]
Float 10 [_Alpha_Cutoff]
Float 11 [_Alpha_Global]
Vector 12 [_Globals_WorldCameraPos]
Vector 13 [_Globals_Origin]
Vector 14 [_Sun_WorldSunDir]
SetTexture 0 [_Sun_Glare] 2D 0
SetTexture 1 [_Sky_Inscatter] 2D 1
SetTexture 2 [_Sky_Transmittance] 2D 2
"3.0-!!ARBfp1.0
PARAM c[24] = { program.local[0..14],
		{ 4, 0.5, 0, 2.2 },
		{ 1e+030, 1, 0.15000001, -0.01348047 },
		{ 12.262105, 0, 0.05747731, 0.1212391 },
		{ 0.1956359, 0.33299461, 0.99999559, 1.570796 },
		{ 0.66666669, -0.19750001, 5.3496246, 0.90909088 },
		{ 0.74000001, 1, 0, -1 },
		{ 0.059683114, 2, -1.5, 9.9999997e-005 },
		{ 0.11936623, 1.413, 0.38317001, 0.45454544 },
		{ 2.718282 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
MOV R0.xyz, c[13];
ADD R1.xyz, R0, c[12];
DP3 R0.y, R1, R1;
RSQ R4.x, R0.y;
RCP R0.w, R4.x;
DP3 R0.x, fragment.texcoord[1], fragment.texcoord[1];
RSQ R0.x, R0.x;
MUL R0.xyz, R0.x, fragment.texcoord[1];
DP3 R3.w, R1, R0;
MUL R1.w, R0, R0;
MAD R1.w, R3, R3, -R1;
MAD R1.w, c[2].x, c[2].x, R1;
RSQ R2.x, R1.w;
RCP R2.x, R2.x;
CMP R1.w, R1, c[16].x, R2.x;
ADD R1.w, -R3, -R1;
MAX R4.y, R1.w, c[15].z;
MAD R2.xyz, R4.y, R0, R1;
CMP R4.z, -R4.y, c[2].x, R0.w;
CMP R1.xyz, -R4.y, R2, R1;
DP3 R5.x, R0, c[14];
MOV R3.z, c[16].y;
DP3 R0.w, R1, c[14];
RCP R1.w, R4.z;
MUL R0.w, R0, R1;
MAX R0.w, R0, c[19].y;
MUL R0.w, R0, c[19].z;
ABS R1.x, R0.w;
MAX R1.y, R1.x, c[16];
RCP R1.z, R1.y;
MIN R1.y, R1.x, c[16];
MUL R1.y, R1, R1.z;
MUL R1.z, R1.y, R1.y;
MUL R1.w, R1.z, c[16];
ADD R1.w, R1, c[17].z;
MAD R1.w, R1, R1.z, -c[17];
MAD R1.w, R1, R1.z, c[18].x;
MAD R1.w, R1, R1.z, -c[18].y;
MAD R1.z, R1.w, R1, c[18];
MUL R1.y, R1.z, R1;
RCP R3.y, c[6].x;
ADD R1.z, -R1.y, c[18].w;
ADD R1.x, R1, -c[16].y;
CMP R1.x, -R1, R1.z, R1.y;
CMP R0.w, R0, -R1.x, R1.x;
MUL R0.w, R0, c[19];
ADD R1.z, R0.w, c[20].x;
RCP R1.x, c[7].x;
ADD R1.y, -R1.x, c[16];
ADD R0.w, R4.y, R3;
CMP R4.w, -R4.y, R0, R3;
MUL R2.w, c[1].x, c[1].x;
MAD R1.w, -R3.y, c[15].y, c[15].y;
MAD R1.x, R1.z, R1.y, R1;
ADD R0.y, -R3.z, c[8].x;
ADD R0.x, R5, c[16].y;
MUL R0.x, R0, R0.y;
MUL R2.y, R0.x, c[15];
FLR R2.z, R2.y;
MAD R3.x, R1, c[15].y, R2.z;
MUL R0.y, R4.z, R4.z;
MAD R0.x, R4.w, R4.w, -R0.y;
MAD R2.x, c[1], c[1], R0;
ADD R2.z, -R2, R2.y;
MOV R1.xyz, c[20].yzzw;
SLT R0.y, c[15].z, R2.x;
SLT R0.x, R4.w, c[15].z;
MUL R5.z, R0.x, R0.y;
MAD R0.x, c[2], c[2], -R2.w;
RSQ R5.y, R0.x;
RCP R0.z, R5.y;
MAD R0.w, R3.y, c[15].y, c[15].y;
MOV R0.x, c[20].w;
MUL R0.y, R0.z, R0.z;
CMP R0, -R5.z, R1, R0;
ADD R1.x, R0.y, R2;
RSQ R1.x, R1.x;
RCP R1.x, R1.x;
ADD R1.y, R3.x, c[16];
RCP R0.y, c[8].x;
MUL R2.x, R0.y, R1.y;
MAD R1.y, R4.z, R4.z, -R2.w;
RCP R2.w, c[5].x;
RSQ R1.y, R1.y;
RCP R1.y, R1.y;
ADD R0.z, R0, R1.y;
MUL R3.x, R3, R0.y;
ADD R1.w, -R2, c[16].y;
MUL R1.z, R5.y, R1.y;
MAD R0.x, R4.w, R0, R1;
MUL R1.z, R1, R1.w;
MAD R1.x, R2.w, c[15].y, R1.z;
MUL R5.y, R1.x, c[5].x;
RCP R0.z, R0.z;
MUL R0.x, R0, R0.z;
ADD R0.z, -R3.y, c[15].y;
MAD R3.y, R0.x, R0.z, R0.w;
ADD R1.x, R5.y, -c[16].y;
FLR R1.x, R1;
MUL R1.x, R1, R2.w;
MAD R1.y, R3, R2.w, R1.x;
FLR R5.z, R5.y;
MUL R5.z, R2.w, R5;
MAD R2.y, R3, R2.w, R5.z;
ADD R5.z, -R2, c[16].y;
MOV R3.y, R2;
MOV R0.y, R1;
MOV R0.x, R3;
MOV R1.x, R2;
TEX R1, R1, texture[1], 2D;
TEX R0, R0, texture[1], 2D;
MUL R1, R2.z, R1;
MAD R1, R0, R5.z, R1;
TEX R0, R2, texture[1], 2D;
MUL R2, R2.z, R0;
TEX R0, R3, texture[1], 2D;
MAD R0, R5.z, R0, R2;
FRC R2.x, R5.y;
MUL R0, R2.x, R0;
ADD R2.x, -R2, c[16].y;
MAD R0, R1, R2.x, R0;
MUL R1.xyz, R0, R0.w;
MUL R3.x, -c[4], c[4];
ADD R3.y, -R3.x, c[21];
RCP R0.w, c[2].x;
MAX R2.x, R0, c[21].w;
MUL R1.w, R4.x, R3;
MUL R0.w, R4, R0;
CMP R0.w, -R4.y, R0, R1;
RCP R1.w, R2.x;
MUL R2.xyz, R1, R1.w;
ADD R0.w, R0, c[16].z;
MUL R0.w, R0, c[17].x;
ABS R1.w, R0;
ADD R3.x, R3, c[16].y;
RCP R1.x, c[3].x;
RCP R1.z, c[3].z;
RCP R1.y, c[3].y;
MUL R1.xyz, R1, c[3].x;
MUL R1.xyz, R2, R1;
MAX R2.y, R1.w, c[16];
MUL R2.z, R5.x, c[4].x;
MUL R2.z, R2, c[21].y;
MAD R2.w, c[4].x, c[4].x, -R2.z;
MAD R2.z, R5.x, R5.x, c[16].y;
ADD R2.w, R2, c[16].y;
POW R2.w, R2.w, c[21].z;
MUL R2.w, R3.x, R2;
MIN R2.x, R1.w, c[16].y;
RCP R2.y, R2.y;
MUL R2.x, R2, R2.y;
MUL R2.y, R2.x, R2.x;
MUL R2.w, R2.z, R2;
RCP R3.x, R3.y;
MUL R2.w, R2, R3.x;
MUL R1.xyz, R2.w, R1;
MUL R2.w, R2.y, c[16];
MUL R1.xyz, R1, c[22].x;
MUL R0.xyz, R2.z, R0;
MAD R0.xyz, R0, c[21].x, R1;
ADD R2.w, R2, c[17].z;
MAD R2.z, R2.w, R2.y, -c[17].w;
MAD R1.y, R2.z, R2, c[18].x;
MAD R1.y, R1, R2, -c[18];
MAD R1.y, R1, R2, c[18].z;
MUL R1.y, R1, R2.x;
SGE R1.x, c[2], R4.z;
ABS R1.x, R1;
CMP R2.w, -R1.x, c[15].z, R3.z;
CMP R0.xyz, -R2.w, c[15].z, R0;
ADD R1.z, -R1.y, c[18].w;
ADD R1.x, R1.w, -c[16].y;
CMP R1.x, -R1, R1.z, R1.y;
CMP R0.w, R0, -R1.x, R1.x;
MOV R1.y, c[1].x;
ADD R1.x, -R1.y, c[2];
MUL R2.x, R0.w, c[19];
MUL R0.xyz, R0, c[9].x;
RCP R1.x, R1.x;
ADD R0.w, R4.z, -c[1].x;
MUL R0.w, R0, R1.x;
RSQ R0.w, R0.w;
RCP R2.y, R0.w;
MAD R1.xy, fragment.texcoord[2], c[15].x, c[15].y;
TEX R1.xyz, R1, texture[0], 2D;
CMP R1.xyz, -fragment.texcoord[2].z, R1, c[15].z;
MAX R1.xyz, R1, c[15].z;
TEX R2.xyz, R2, texture[2], 2D;
CMP R2.xyz, -R2.w, c[16].y, R2;
POW R1.x, R1.x, c[15].w;
POW R1.z, R1.z, c[15].w;
POW R1.y, R1.y, c[15].w;
MUL R1.xyz, R1, c[9].x;
MAD R1.xyz, R1, R2, R0;
MUL R0.xyz, R1, c[0].x;
POW R1.y, c[23].x, -R0.x;
ADD R0.w, R0.x, -c[22].y;
POW R1.z, c[23].x, -R0.z;
MUL R0.x, R0, c[22].z;
ADD R1.y, -R1, c[16];
POW R0.x, R0.x, c[22].w;
CMP R0.x, R0.w, R0, R1.y;
ABS R0.w, R1.x;
POW R1.y, c[23].x, -R0.y;
ADD R1.x, R0.y, -c[22].y;
MUL R0.y, R0, c[22].z;
ADD R1.y, -R1, c[16];
POW R0.y, R0.y, c[22].w;
CMP R0.y, R1.x, R0, R1;
SGE R1.x, c[10], R0.w;
ADD R1.y, R0.z, -c[22];
ABS R1.w, R1.x;
CMP R2.x, -R1.w, c[15].z, R3.z;
MUL R0.z, R0, c[22];
ADD R1.z, -R1, c[16].y;
POW R0.z, R0.z, c[22].w;
CMP R0.z, R1.y, R0, R1;
MOV R1.xyz, R0;
MUL R0.w, R0, c[11].x;
MOV R1.w, c[11].x;
CMP result.color, -R2.x, R1, R0;
END
# 221 instructions, 6 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 269 math, 6 textures, 2 branches
Float 0 [_Exposure]
Float 1 [Rg]
Float 2 [Rt]
Vector 3 [betaR]
Float 4 [mieG]
Float 5 [RES_R]
Float 6 [RES_MU]
Float 7 [RES_MU_S]
Float 8 [RES_NU]
Float 9 [_Sun_Intensity]
Float 10 [_Alpha_Cutoff]
Float 11 [_Alpha_Global]
Vector 12 [_Globals_WorldCameraPos]
Vector 13 [_Globals_Origin]
Vector 14 [_Sun_WorldSunDir]
SetTexture 0 [_Sun_Glare] 2D 0
SetTexture 1 [_Sky_Inscatter] 2D 1
SetTexture 2 [_Sky_Transmittance] 2D 2
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c15, 0.00000000, 4.00000000, 0.50000000, 2.20000005
def c16, 1000000015047466200000000000000.00000000, 1.00000000, 0.00000000, 0.15000001
def c17, 12.26193905, -1.00000000, -0.01348047, 0.05747731
def c18, -0.12123910, 0.19563590, -0.33299461, 0.99999559
def c19, 1.57079601, 0.66666669, -0.19750001, 5.34960032
def c20, 0.90909088, 0.74000001, 0.05968311, 2.00000000
def c21, -1.50000000, 0.00010000, 0.11936623, 0.38317001
def c22, 0.45454544, 2.71828198, -1.41299999, 0
dcl_texcoord1 v0.xyz
dcl_texcoord2 v1.xyz
mov r0.xyz, c12
add r1.xyz, c13, r0
dp3 r0.y, r1, r1
rsq r4.w, r0.y
rcp r0.w, r4.w
dp3 r0.x, v0, v0
rsq r0.x, r0.x
mul r0.xyz, r0.x, v0
dp3 r5.x, r1, r0
mul r1.w, r0, r0
mad r1.w, r5.x, r5.x, -r1
mad r1.w, c2.x, c2.x, r1
rsq r2.x, r1.w
rcp r2.x, r2.x
cmp r1.w, r1, r2.x, c16.x
add r1.w, -r5.x, -r1
max r4.z, r1.w, c15.x
mad r2.xyz, r4.z, r0, r1
cmp r1.xyz, -r4.z, r1, r2
cmp r3.z, -r4, r0.w, c2.x
dp3 r3.w, r0, c14
dp3 r0.w, r1, c14
rcp r1.w, r3.z
mul r0.w, r0, r1
max r0.w, r0, c19.z
mul r0.w, r0, c19
abs r1.x, r0.w
max r1.y, r1.x, c16
rcp r1.z, r1.y
min r1.y, r1.x, c16
mul r1.y, r1, r1.z
mul r1.z, r1.y, r1.y
mad r1.w, r1.z, c17.z, c17
mad r1.w, r1, r1.z, c18.x
mad r1.w, r1, r1.z, c18.y
mad r1.w, r1, r1.z, c18.z
mad r1.z, r1.w, r1, c18.w
mul r1.y, r1.z, r1
rcp r2.z, c6.x
mul r2.x, c1, c1
mad r1.w, -r2.z, c15.z, c15.z
add r1.z, -r1.y, c19.x
add r1.x, r1, c17.y
cmp r1.x, -r1, r1.y, r1.z
cmp r0.w, r0, r1.x, -r1.x
mov r1.x, c8
add r0.y, c17, r1.x
add r0.x, r3.w, c16.y
mul r0.y, r0.x, r0
mad r0.x, r0.w, c20, c20.y
mul r0.w, r0.y, c15.z
frc r5.w, r0
rcp r0.y, c7.x
add r0.z, -r0.y, c16.y
mad r0.x, r0, r0.z, r0.y
mad r0.z, c2.x, c2.x, -r2.x
rsq r2.y, r0.z
add r0.w, r0, -r5
mad r3.x, r0, c15.z, r0.w
add r0.x, r4.z, r5
cmp r5.y, -r4.z, r5.x, r0.x
mul r0.y, r3.z, r3.z
mad r0.y, r5, r5, -r0
mad r2.w, c1.x, c1.x, r0.y
mov r1.xyz, c16.yzzw
add r3.y, r3.x, c16
cmp r0.y, -r2.w, c16.z, c16
cmp r0.x, r5.y, c16.z, c16.y
mul_pp r4.x, r0, r0.y
rcp r0.z, r2.y
mad r0.w, r2.z, c15.z, c15.z
mov r0.x, c17.y
mul r0.y, r0.z, r0.z
cmp r0, -r4.x, r0, r1
rcp r1.z, c8.x
mul r1.x, r1.z, r3.y
add r0.y, r0, r2.w
mad r1.y, r3.z, r3.z, -r2.x
rsq r0.y, r0.y
rcp r0.y, r0.y
mad r0.y, r5, r0.x, r0
rcp r0.x, c5.x
rsq r1.y, r1.y
rcp r1.y, r1.y
add r0.z, r0, r1.y
rcp r0.z, r0.z
mul r0.y, r0, r0.z
add r0.z, -r2, c15
mad r0.y, r0, r0.z, r0.w
mul r3.x, r3, r1.z
mov r4.x, r1
add r1.w, -r0.x, c16.y
mul r1.y, r2, r1
mul r1.y, r1, r1.w
mad r1.y, r0.x, c15.z, r1
mul r1.y, r1, c5.x
add r1.w, r1.y, c17.y
frc r5.z, r1.y
frc r0.z, r1.w
add r0.z, r1.w, -r0
add r0.w, r1.y, -r5.z
mul r0.z, r0, r0.x
mad r4.y, r0, r0.x, r0.z
texld r2, r4, s1
mul r0.w, r0.x, r0
mad r1.y, r0, r0.x, r0.w
texld r0, r1, s1
mov r3.y, r1
texld r1, r3, s1
mul r2, r5.w, r2
add r3.y, -r5.w, c16
mul r0, r5.w, r0
mad r0, r3.y, r1, r0
mul r0, r5.z, r0
mov r1.y, r4
mov r1.x, r3
texld r1, r1, s1
mad r1, r1, r3.y, r2
add r2.x, -r5.z, c16.y
mad r1, r1, r2.x, r0
mul r0.xyz, r1, r1.w
rcp r0.w, c2.x
mul r1.w, r5.y, r0
mul r0.w, r4, r5.x
cmp r0.w, -r4.z, r0, r1
max r2.x, r1, c21.y
rcp r1.w, r2.x
mul r0.xyz, r0, r1.w
add r0.w, r0, c16
mul r1.w, r0, c17.x
abs r2.w, r1
rcp r2.x, c3.x
rcp r2.z, c3.z
rcp r2.y, c3.y
mul r2.xyz, r2, c3.x
mul r2.xyz, r0, r2
max r0.y, r2.w, c16
min r0.x, r2.w, c16.y
rcp r0.y, r0.y
mul r3.x, r0, r0.y
mul r0.z, r3.w, c4.x
mul r0.x, r0.z, c20.w
mad r0.y, c4.x, c4.x, -r0.x
mul r3.y, r3.x, r3.x
mad r0.x, r3.y, c17.z, c17.w
mad r4.x, r0, r3.y, c18
add r4.y, r0, c16
pow r0, r4.y, c21.x
mad r0.w, r3, r3, c16.y
mul r1.xyz, r0.w, r1
mad r4.x, r4, r3.y, c18.y
mov r0.z, r0.x
mul r0.y, -c4.x, c4.x
add r0.x, r0.y, c16.y
mul r0.x, r0, r0.z
mul r0.x, r0.w, r0
add r0.w, -r3.z, c2.x
add r0.y, -r0, c20.w
rcp r0.y, r0.y
mul r0.x, r0, r0.y
mul r0.xyz, r0.x, r2
mad r2.x, r4, r3.y, c18.z
mad r2.x, r2, r3.y, c18.w
mul r0.xyz, r0, c21.z
mad r0.xyz, r1, c20.z, r0
mul r2.x, r2, r3
add r1.y, -r2.x, c19.x
add r1.x, r2.w, c17.y
cmp r0.w, r0, c16.y, c16.z
abs_pp r2.w, r0
cmp r1.x, -r1, r2, r1.y
cmp r2.xyz, -r2.w, c15.x, r0
cmp r0.x, r1.w, r1, -r1
mul r1.x, r0, c19.y
mov r0.x, c2
add r0.z, -c1.x, r0.x
rcp r1.y, r0.z
add r0.w, r3.z, -c1.x
mul r0.w, r0, r1.y
rsq r0.w, r0.w
rcp r1.y, r0.w
texld r1.xyz, r1, s2
mad r0.xy, v1, c15.y, c15.z
texld r0.xyz, r0, s0
cmp r0.xyz, -v1.z, c15.x, r0
max r3.xyz, r0, c15.x
pow r0, r3.x, c15.w
cmp r4.xyz, -r2.w, c16.y, r1
mov r3.x, r0
pow r0, r3.y, c15.w
pow r1, r3.z, c15.w
mul r2.xyz, r2, c9.x
mov r3.z, r1
mov r3.y, r0
mul r0.xyz, r3, c9.x
mad r0.xyz, r0, r4, r2
abs r2.w, r0.x
if_le r2.w, c10.x
mul r2.xyz, r0, c0.x
pow r0, c22.y, -r2.x
mul r0.y, r2.x, c21.w
pow r1, r0.y, c22.x
add r0.y, -r0.x, c16
mov r0.z, r1.x
add r0.x, r2, c22.z
cmp oC0.x, r0, r0.y, r0.z
pow r0, c22.y, -r2.y
mul r0.y, r2, c21.w
pow r1, r0.y, c22.x
add r0.y, -r0.x, c16
mov r0.z, r1.x
add r0.x, r2.y, c22.z
cmp oC0.y, r0.x, r0, r0.z
pow r0, c22.y, -r2.z
mul r0.y, r2.z, c21.w
pow r1, r0.y, c22.x
add r0.y, -r0.x, c16
mov r0.z, r1.x
add r0.x, r2.z, c22.z
cmp oC0.z, r0.x, r0.y, r0
mul oC0.w, r2, c11.x
else
mul r2.xyz, r0, c0.x
pow r0, c22.y, -r2.x
mul r0.y, r2.x, c21.w
pow r1, r0.y, c22.x
add r0.y, -r0.x, c16
mov r0.z, r1.x
add r0.x, r2, c22.z
cmp oC0.x, r0, r0.y, r0.z
pow r0, c22.y, -r2.y
mul r0.y, r2, c21.w
pow r1, r0.y, c22.x
add r0.y, -r0.x, c16
mov r0.z, r1.x
add r0.x, r2.y, c22.z
cmp oC0.y, r0.x, r0, r0.z
pow r0, c22.y, -r2.z
mul r0.y, r2.z, c21.w
pow r1, r0.y, c22.x
add r0.y, -r0.x, c16
mov r0.z, r1.x
add r0.x, r2.z, c22.z
cmp oC0.z, r0.x, r0.y, r0
mov oC0.w, c11.x
endif
"
}
SubProgram "d3d11 " {
// Stats: 178 math, 6 textures, 3 branches
SetTexture 0 [_Sun_Glare] 2D 2
SetTexture 1 [_Sky_Inscatter] 2D 1
SetTexture 2 [_Sky_Transmittance] 2D 0
ConstBuffer "$Globals" 384
Float 16 [_Exposure]
Float 24 [Rg]
Float 28 [Rt]
Vector 48 [betaR] 3
Float 92 [mieG]
Float 112 [RES_R]
Float 116 [RES_MU]
Float 120 [RES_MU_S]
Float 124 [RES_NU]
Float 128 [_Sun_Intensity]
Float 132 [_Alpha_Cutoff]
Float 136 [_Alpha_Global]
Vector 272 [_Globals_WorldCameraPos] 3
Vector 288 [_Globals_Origin] 3
Vector 304 [_Sun_WorldSunDir] 3
BindCB  "$Globals" 0
"ps_4_0
eefiecedeiilnoafbjlfeekedmfcmgogbmclklbpabaaaaaaembkaaaaadaaaaaa
cmaaaaaaleaaaaaaoiaaaaaaejfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadaaaaaaheaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaheaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaahahaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcfmbjaaaaeaaaaaaafhagaaaafjaaaaaeegiocaaa
aaaaaaaabeaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaagcbaaaad
hcbabaaaacaaaaaagcbaaaadhcbabaaaadaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacahaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaacaaaaaaegbcbaaa
acaaaaaaeeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahhcaabaaa
aaaaaaaaagaabaaaaaaaaaaaegbcbaaaacaaaaaadbaaaaahicaabaaaaaaaaaaa
abeaaaaaaaaaaaaackbabaaaadaaaaaadcaaaaapdcaabaaaabaaaaaaegbabaaa
adaaaaaaaceaaaaaaaaaiaeaaaaaiaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaaaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaaaaaaaaaaagabaaaacaaaaaaabaaaaahhcaabaaaabaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaadeaaaaakhcaabaaaabaaaaaaegacbaaaabaaaaaa
aceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaacpaaaaafhcaabaaaabaaaaaa
egacbaaaabaaaaaadiaaaaakhcaabaaaabaaaaaaegacbaaaabaaaaaaaceaaaaa
mnmmameamnmmameamnmmameaaaaaaaaabjaaaaafhcaabaaaabaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaabaaaaaaegacbaaaabaaaaaaagiacaaaaaaaaaaa
aiaaaaaaaaaaaaajhcaabaaaacaaaaaaegiccaaaaaaaaaaabbaaaaaaegiccaaa
aaaaaaaabcaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaacaaaaaaegacbaaa
acaaaaaaelaaaaafbcaabaaaadaaaaaadkaabaaaaaaaaaaabaaaaaahccaabaaa
adaaaaaaegacbaaaacaaaaaaegacbaaaaaaaaaaaaoaaaaahecaabaaaadaaaaaa
bkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaaaaaaaaaabkaabaaa
adaaaaaabkaabaaaadaaaaaadkaabaiaebaaaaaaaaaaaaaadiaaaaajicaabaaa
abaaaaaackiacaaaaaaaaaaaabaaaaaackiacaaaaaaaaaaaabaaaaaadcaaaaal
icaabaaaaaaaaaaadkiacaaaaaaaaaaaabaaaaaadkiacaaaaaaaaaaaabaaaaaa
dkaabaaaaaaaaaaabnaaaaahicaabaaaacaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaaaaelaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadhaaaaakicaabaaa
aaaaaaaadkaabaaaacaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaamkpcejpb
aaaaaaaiicaabaaaaaaaaaaadkaabaaaaaaaaaaabkaabaiaebaaaaaaadaaaaaa
deaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaaaadbaaaaah
icaabaaaacaaaaaaabeaaaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaacaaaaaaaaaaaaah
ccaabaaaafaaaaaadkaabaaaaaaaaaaabkaabaaaadaaaaaaaoaaaaaiecaabaaa
afaaaaaabkaabaaaafaaaaaadkiacaaaaaaaaaaaabaaaaaadhaaaaajhcaabaaa
acaaaaaapgapbaaaacaaaaaaegacbaaaaeaaaaaaegacbaaaacaaaaaadgaaaaag
bcaabaaaafaaaaaadkiacaaaaaaaaaaaabaaaaaadhaaaaajhcaabaaaadaaaaaa
pgapbaaaacaaaaaaegacbaaaafaaaaaaegacbaaaadaaaaaabaaaaaaibcaabaaa
aaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaabdaaaaaabaaaaaaiccaabaaa
aaaaaaaaegacbaaaacaaaaaaegiccaaaaaaaaaaabdaaaaaaaoaaaaahccaabaaa
aaaaaaaabkaabaaaaaaaaaaaakaabaaaadaaaaaadcaaaaamecaabaaaaaaaaaaa
dkiacaaaaaaaaaaaabaaaaaadkiacaaaaaaaaaaaabaaaaaadkaabaiaebaaaaaa
abaaaaaaelaaaaafecaabaaaacaaaaaackaabaaaaaaaaaaadiaaaaahecaabaaa
aaaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaaaaaaaaaa
akaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaabaaaaaaelaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaakecaabaaaaaaaaaaabkaabaaa
adaaaaaabkaabaaaadaaaaaackaabaiaebaaaaaaaaaaaaaadcaaaaalecaabaaa
aaaaaaaackiacaaaaaaaaaaaabaaaaaackiacaaaaaaaaaaaabaaaaaackaabaaa
aaaaaaaadbaaaaahicaabaaaabaaaaaabkaabaaaadaaaaaaabeaaaaaaaaaaaaa
dbaaaaahicaabaaaadaaaaaaabeaaaaaaaaaaaaackaabaaaaaaaaaaaabaaaaah
icaabaaaabaaaaaadkaabaaaabaaaaaadkaabaaaadaaaaaaaoaaaaalhcaabaaa
aeaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaabgigcaaaaaaaaaaa
ahaaaaaaaaaaaaaiicaabaaaafaaaaaaakaabaiaebaaaaaaaeaaaaaaabeaaaaa
aaaaaadpdiaaaaahccaabaaaacaaaaaackaabaaaacaaaaaackaabaaaacaaaaaa
aaaaaaahicaabaaaacaaaaaaakaabaaaaeaaaaaaabeaaaaaaaaaaadpdgaaaaai
hcaabaaaafaaaaaaaceaaaaaaaaaiadpaaaaaaaaaaaaaaaaaaaaaaaadgaaaaaf
bcaabaaaacaaaaaaabeaaaaaaaaaialpdhaaaaajpcaabaaaafaaaaaapgapbaaa
abaaaaaaegaobaaaafaaaaaaegaobaaaacaaaaaaaoaaaaahicaabaaaabaaaaaa
dkaabaaaaaaaaaaackaabaaaacaaaaaaaoaaaaalhcaabaaaacaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpegiccaaaaaaaaaaaahaaaaaaaaaaaaal
hcaabaaaacaaaaaaegacbaiaebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaaadp
aaaaiadpaaaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaabaaaaaaakaabaaa
acaaaaaabkaabaaaaeaaaaaaaaaaaaahmcaabaaaaaaaaaaakgaobaaaaaaaaaaa
fgajbaaaafaaaaaaelaaaaafecaabaaaaaaaaaaackaabaaaaaaaaaaadcaaaaaj
ecaabaaaaaaaaaaabkaabaaaadaaaaaaakaabaaaafaaaaaackaabaaaaaaaaaaa
aoaaaaahecaabaaaaaaaaaaackaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaaj
ecaabaaaaaaaaaaackaabaaaaaaaaaaabkaabaaaacaaaaaadkaabaaaafaaaaaa
deaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaahbdneklodiaaaaah
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaabodakleaddaaaaaiicaabaaa
aaaaaaaabkaabaiaibaaaaaaaaaaaaaaabeaaaaaaaaaiadpdeaaaaaibcaabaaa
acaaaaaabkaabaiaibaaaaaaaaaaaaaaabeaaaaaaaaaiadpaoaaaaakbcaabaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaacaaaaaa
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaacaaaaaadiaaaaah
bcaabaaaacaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaajccaabaaa
acaaaaaaakaabaaaacaaaaaaabeaaaaafpkokkdmabeaaaaadgfkkolndcaaaaaj
ccaabaaaacaaaaaaakaabaaaacaaaaaabkaabaaaacaaaaaaabeaaaaaochgdido
dcaaaaajccaabaaaacaaaaaaakaabaaaacaaaaaabkaabaaaacaaaaaaabeaaaaa
aebnkjlodcaaaaajbcaabaaaacaaaaaaakaabaaaacaaaaaabkaabaaaacaaaaaa
abeaaaaadiphhpdpdiaaaaahccaabaaaacaaaaaadkaabaaaaaaaaaaaakaabaaa
acaaaaaadbaaaaaiicaabaaaacaaaaaaabeaaaaaaaaaiadpbkaabaiaibaaaaaa
aaaaaaaadcaaaaajccaabaaaacaaaaaabkaabaaaacaaaaaaabeaaaaaaaaaaama
abeaaaaanlapmjdpabaaaaahccaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaa
acaaaaaadcaaaaajicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaacaaaaaa
bkaabaaaacaaaaaaddaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdbaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaadhaaaaakccaabaaaaaaaaaaabkaabaaaaaaaaaaadkaabaiaebaaaaaa
aaaaaaaadkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaabkaabaaaaaaaaaaa
abeaaaaacolkgidpabeaaaaakehadndpdiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaaadpdcaaaaajccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaacaaaaaackaabaaaaeaaaaaaaaaaaaahicaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaaadpaaaaaaaibcaabaaaacaaaaaadkiacaaaaaaaaaaaahaaaaaa
abeaaaaaaaaaialpdiaaaaahccaabaaaacaaaaaadkaabaaaaaaaaaaaakaabaaa
acaaaaaaebaaaaafccaabaaaacaaaaaabkaabaaaacaaaaaadcaaaaakicaabaaa
aaaaaaaadkaabaaaaaaaaaaaakaabaaaacaaaaaabkaabaiaebaaaaaaacaaaaaa
diaaaaaibcaabaaaacaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaaahaaaaaa
dcaaaaakicaabaaaabaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaaahaaaaaa
abeaaaaaaaaaialpebaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaaaoaaaaai
icaabaaaabaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaaahaaaaaaebaaaaaf
ecaabaaaacaaaaaaakaabaaaacaaaaaaaoaaaaaiecaabaaaacaaaaaackaabaaa
acaaaaaaakiacaaaaaaaaaaaahaaaaaabkaaaaafbcaabaaaacaaaaaaakaabaaa
acaaaaaaaaaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaacaaaaaa
aoaaaaaiccaabaaaaeaaaaaabkaabaaaaaaaaaaadkiacaaaaaaaaaaaahaaaaaa
aoaaaaaiecaabaaaaaaaaaaackaabaaaaaaaaaaaakiacaaaaaaaaaaaahaaaaaa
aaaaaaahecaabaaaaeaaaaaadkaabaaaabaaaaaackaabaaaaaaaaaaaefaaaaaj
pcaabaaaafaaaaaajgafbaaaaeaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
aaaaaaaiicaabaaaabaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
aaaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpaoaaaaai
bcaabaaaaeaaaaaabkaabaaaaaaaaaaadkiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaagaaaaaaigaabaaaaeaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
diaaaaahpcaabaaaagaaaaaapgapbaaaaaaaaaaaegaobaaaagaaaaaadcaaaaaj
pcaabaaaafaaaaaaegaobaaaafaaaaaapgapbaaaabaaaaaaegaobaaaagaaaaaa
aaaaaaahicaabaaaaeaaaaaackaabaaaacaaaaaackaabaaaaaaaaaaaefaaaaaj
pcaabaaaagaaaaaangafbaaaaeaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
efaaaaajpcaabaaaaeaaaaaamgaabaaaaeaaaaaaeghobaaaabaaaaaaaagabaaa
abaaaaaadiaaaaahpcaabaaaaeaaaaaapgapbaaaaaaaaaaaegaobaaaaeaaaaaa
dcaaaaajpcaabaaaaeaaaaaaegaobaaaagaaaaaapgapbaaaabaaaaaaegaobaaa
aeaaaaaaaaaaaaaiccaabaaaaaaaaaaaakaabaiaebaaaaaaacaaaaaaabeaaaaa
aaaaiadpdiaaaaahpcaabaaaacaaaaaaagaabaaaacaaaaaaegaobaaaaeaaaaaa
dcaaaaajpcaabaaaacaaaaaaegaobaaaafaaaaaafgafbaaaaaaaaaaaegaobaaa
acaaaaaaaaaaaaajccaabaaaaaaaaaaaakaabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaabaaaaaaaaaaaaakecaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaa
abaaaaaadkiacaaaaaaaaaaaabaaaaaaaoaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaaelaaaaafccaabaaaaeaaaaaabkaabaaaaaaaaaaa
aaaaaaahccaabaaaaaaaaaaackaabaaaadaaaaaaabeaaaaajkjjbjdodiaaaaah
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaajfdbeeebddaaaaaiecaabaaa
aaaaaaaabkaabaiaibaaaaaaaaaaaaaaabeaaaaaaaaaiadpdeaaaaaiicaabaaa
aaaaaaaabkaabaiaibaaaaaaaaaaaaaaabeaaaaaaaaaiadpaoaaaaakicaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaa
diaaaaahecaabaaaaaaaaaaadkaabaaaaaaaaaaackaabaaaaaaaaaaadiaaaaah
icaabaaaaaaaaaaackaabaaaaaaaaaaackaabaaaaaaaaaaadcaaaaajicaabaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaafpkokkdmabeaaaaadgfkkolndcaaaaaj
icaabaaaabaaaaaadkaabaaaaaaaaaaadkaabaaaabaaaaaaabeaaaaaochgdido
dcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaadkaabaaaabaaaaaaabeaaaaa
aebnkjlodcaaaaajicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaabaaaaaa
abeaaaaadiphhpdpdiaaaaahicaabaaaabaaaaaadkaabaaaaaaaaaaackaabaaa
aaaaaaaadbaaaaaiccaabaaaadaaaaaaabeaaaaaaaaaiadpbkaabaiaibaaaaaa
aaaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaabaaaaaaabeaaaaaaaaaaama
abeaaaaanlapmjdpabaaaaahicaabaaaabaaaaaabkaabaaaadaaaaaadkaabaaa
abaaaaaadcaaaaajecaabaaaaaaaaaaackaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaabaaaaaaddaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdbaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaadhaaaaakccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaiaebaaaaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaahbcaabaaaaeaaaaaabkaabaaaaaaaaaaa
abeaaaaaklkkckdpefaaaaajpcaabaaaaeaaaaaaegaabaaaaeaaaaaaeghobaaa
acaaaaaaaagabaaaaaaaaaaabnaaaaaiccaabaaaaaaaaaaadkiacaaaaaaaaaaa
abaaaaaaakaabaaaadaaaaaadiaaaaahhcaabaaaadaaaaaapgapbaaaacaaaaaa
egacbaaaacaaaaaadeaaaaahecaabaaaaaaaaaaaakaabaaaacaaaaaaabeaaaaa
bhlhnbdiaoaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaakgakbaaaaaaaaaaa
aoaaaaajhcaabaaaafaaaaaaagiacaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaa
adaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaegacbaaaafaaaaaa
dcaaaaajecaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaamicaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaaafaaaaaa
dkiacaaaaaaaaaaaafaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaabaaaaaa
dkaabaaaabaaaaaaabeaaaaaeihgpedndcaaaaaodcaabaaaafaaaaaapgipcaaa
aaaaaaaaafaaaaaapgipcaaaaaaaaaaaafaaaaaaaceaaaaaaaaaiadpaaaaaaea
aaaaaaaaaaaaaaaaapaaaaaibcaabaaaaaaaaaaaagaabaaaaaaaaaaapgipcaaa
aaaaaaaaafaaaaaaaaaaaaaibcaabaaaaaaaaaaaakaabaiaebaaaaaaaaaaaaaa
akaabaaaafaaaaaacpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaak
jcaabaaaaaaaaaaaagaibaaaaaaaaaaaaceaaaaaaaaamalpaaaaaaaaaaaaaaaa
eihghednbjaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaadkaabaaaabaaaaaadiaaaaahbcaabaaaaaaaaaaa
ckaabaaaaaaaaaaaakaabaaaaaaaaaaaaoaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaaagaabaaaaaaaaaaa
egacbaaaadaaaaaadcaaaaajncaabaaaaaaaaaaaagajbaaaacaaaaaapgapbaaa
aaaaaaaaagajbaaaadaaaaaadhaaaaamhcaabaaaacaaaaaafgafbaaaaaaaaaaa
egacbaaaaeaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaaaaaabaaaaah
hcaabaaaaaaaaaaaigadbaaaaaaaaaaafgafbaaaaaaaaaaadiaaaaaihcaabaaa
aaaaaaaaegacbaaaaaaaaaaaagiacaaaaaaaaaaaaiaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaaegacbaaaaaaaaaaabnaaaaaj
icaabaaaaaaaaaaabkiacaaaaaaaaaaaaiaaaaaaakaabaiaibaaaaaaaaaaaaaa
bpaaaeaddkaabaaaaaaaaaaadiaaaaaihcaabaaaabaaaaaaegacbaaaaaaaaaaa
agiacaaaaaaaaaaaabaaaaaadbaaaaakhcaabaaaacaaaaaaegacbaaaabaaaaaa
aceaaaaacpnnledpcpnnledpcpnnledpaaaaaaaadiaaaaakpcaabaaaadaaaaaa
agafbaaaabaaaaaaaceaaaaanmcomedodlkklilpnmcomedodlkklilpcpaaaaaf
dcaabaaaabaaaaaaigaabaaaadaaaaaadiaaaaakdcaabaaaabaaaaaaegaabaaa
abaaaaaaaceaaaaacplkoidocplkoidoaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
abaaaaaaegaabaaaabaaaaaabjaaaaafdcaabaaaadaaaaaangafbaaaadaaaaaa
aaaaaaaldcaabaaaadaaaaaaegaabaiaebaaaaaaadaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaaaaaaaaaaaaadhaaaaajdccabaaaaaaaaaaaegaabaaaacaaaaaa
egaabaaaabaaaaaaegaabaaaadaaaaaadiaaaaakdcaabaaaabaaaaaakgakbaaa
abaaaaaaaceaaaaanmcomedodlkklilpaaaaaaaaaaaaaaaacpaaaaaficaabaaa
aaaaaaaaakaabaaaabaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
abeaaaaacplkoidobjaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaabjaaaaaf
bcaabaaaabaaaaaabkaabaaaabaaaaaaaaaaaaaibcaabaaaabaaaaaaakaabaia
ebaaaaaaabaaaaaaabeaaaaaaaaaiadpdhaaaaajeccabaaaaaaaaaaackaabaaa
acaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadiaaaaajiccabaaaaaaaaaaa
akaabaiaibaaaaaaaaaaaaaackiacaaaaaaaaaaaaiaaaaaadoaaaaabbcaaaaab
diaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaagiacaaaaaaaaaaaabaaaaaa
dbaaaaakhcaabaaaabaaaaaaegacbaaaaaaaaaaaaceaaaaacpnnledpcpnnledp
cpnnledpaaaaaaaadiaaaaakpcaabaaaacaaaaaaagafbaaaaaaaaaaaaceaaaaa
nmcomedodlkklilpnmcomedodlkklilpcpaaaaafdcaabaaaaaaaaaaaigaabaaa
acaaaaaadiaaaaakdcaabaaaaaaaaaaaegaabaaaaaaaaaaaaceaaaaacplkoido
cplkoidoaaaaaaaaaaaaaaaabjaaaaafdcaabaaaaaaaaaaaegaabaaaaaaaaaaa
bjaaaaafdcaabaaaacaaaaaangafbaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaa
egaabaiaebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaa
dhaaaaajdccabaaaaaaaaaaaegaabaaaabaaaaaaegaabaaaaaaaaaaaegaabaaa
acaaaaaadiaaaaakdcaabaaaaaaaaaaakgakbaaaaaaaaaaaaceaaaaanmcomedo
dlkklilpaaaaaaaaaaaaaaaacpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaa
diaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaacplkoidobjaaaaaf
bcaabaaaaaaaaaaaakaabaaaaaaaaaaabjaaaaafccaabaaaaaaaaaaabkaabaaa
aaaaaaaaaaaaaaaiccaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaaabeaaaaa
aaaaiadpdhaaaaajeccabaaaaaaaaaaackaabaaaabaaaaaaakaabaaaaaaaaaaa
bkaabaaaaaaaaaaadgaaaaagiccabaaaaaaaaaaackiacaaaaaaaaaaaaiaaaaaa
doaaaaabbfaaaaabdoaaaaab"
}
SubProgram "gles " {
"!!GLES"
}
SubProgram "glesdesktop " {
"!!GLES"
}
SubProgram "gles3 " {
"!!GLES3"
}
}
 }
}
}