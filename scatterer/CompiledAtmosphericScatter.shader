// Compiled shader for all platforms, uncompressed size: 157.4KB

Shader "Sky/AtmosphereImageEffect" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _SkyDome ("SkyDome", 2D) = "white" {}
 _Scale ("Scale", Vector) = (1,1,1,1)
}
SubShader { 
 Tags { "QUEUE"="Transparent" }


 // Stats for Vertex shader:
 //       d3d11 : 9 math
 //        d3d9 : 16 math, 1 branch
 //      opengl : 11 math
 // Stats for Fragment shader:
 //       d3d11 : 506 math, 2 texture, 8 branch
 //        d3d9 : 726 math, 58 texture, 6 branch
 Pass {
  Tags { "QUEUE"="Transparent" }
  ZTest Always
  ZWrite Off
  Cull Front
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "opengl " {
// Stats: 11 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 5 [_FrustumCorners]
"3.0-!!ARBvp1.0
PARAM c[9] = { { 0.1 },
		state.matrix.mvp,
		program.local[5..8] };
TEMP R0;
ADDRESS A0;
ARL A0.x, vertex.position.z;
MOV R0.xyw, vertex.position;
MOV R0.z, c[0].x;
DP4 result.position.w, R0, c[4];
DP4 result.position.z, R0, c[3];
DP4 result.position.y, R0, c[2];
DP4 result.position.x, R0, c[1];
MOV result.texcoord[2].xyz, c[A0.x + 5];
MOV result.texcoord[0].xy, vertex.texcoord[0];
MOV result.texcoord[1].xy, vertex.texcoord[0];
MOV result.texcoord[2].w, vertex.position.z;
END
# 11 instructions, 1 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 16 math, 1 branches
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [_FrustumCorners]
Vector 8 [_MainTex_TexelSize]
"vs_3_0
dcl_position0 v0
dcl_texcoord0 v1
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
def c9, 0.10000000, 0.00000000, 1.00000000, 0
mov r1.xyw, v0
mov r1.z, c9.x
dp4 r0.x, r1, c0
dp4 r0.w, r1, c3
dp4 r0.z, r1, c2
dp4 r0.y, r1, c1
mov r1.z, c9.y
mov r1.xy, v1
if_lt c8.y, r1.z
add r1.y, -v1, c9.z
mov r1.x, v1
endif
mova a0.x, v0.z
mov o0, r0
mov o3.xyz, c[a0.x + 4]
mov o1.xy, r1
mov o2.xy, v1
mov o3.w, v0.z
"
}
SubProgram "d3d11 " {
// Stats: 9 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "$Globals" 304
Matrix 160 [_FrustumCorners]
Vector 224 [_MainTex_TexelSize]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  "$Globals" 0
BindCB  "UnityPerDraw" 1
"vs_4_0
eefiecedppkkdcgmkanmhmfcbheloigjkomablpiabaaaaaaiiadaaaaadaaaaaa
cmaaaaaaiaaaaaaaaiabaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
heaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaaamadaaaaheaaaaaaacaaaaaa
aaaaaaaaadaaaaaaacaaaaaaapaaaaaafdfgfpfagphdgjhegjgpgoaafeeffied
epepfceeaaklklklfdeieefchiacaaaaeaaaabaajoaaaaaadfbiaaaabcaaaaaa
aaaaiadpaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaiadpaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaiadpaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaiadp
fjaaaaaeegiocaaaaaaaaaaaapaaaaaafjaaaaaeegiocaaaabaaaaaaaeaaaaaa
fpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagfaaaaadmccabaaaabaaaaaa
gfaaaaadpccabaaaacaaaaaagiaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaabaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaan
pcaabaaaaaaaaaaaegiocaaaabaaaaaaacaaaaaaaceaaaaamnmmmmdnmnmmmmdn
mnmmmmdnmnmmmmdnegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
abaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadbaaaaaibcaabaaa
aaaaaaaabkiacaaaaaaaaaaaaoaaaaaaabeaaaaaaaaaaaaaaaaaaaaiccaabaaa
aaaaaaaabkbabaiaebaaaaaaabaaaaaaabeaaaaaaaaaiadpdhaaaaajcccabaaa
abaaaaaaakaabaaaaaaaaaaabkaabaaaaaaaaaaabkbabaaaabaaaaaadgaaaaaf
nccabaaaabaaaaaaagbebaaaabaaaaaadgaaaaaficcabaaaacaaaaaackbabaaa
aaaaaaaablaaaaafbcaabaaaaaaaaaaackbabaaaaaaaaaaabbaaaaajbccabaaa
acaaaaaaegiocaaaaaaaaaaaakaaaaaaegjojaaaakaabaaaaaaaaaaabbaaaaaj
cccabaaaacaaaaaaegiocaaaaaaaaaaaalaaaaaaegjojaaaakaabaaaaaaaaaaa
bbaaaaajeccabaaaacaaaaaaegiocaaaaaaaaaaaamaaaaaaegjojaaaakaabaaa
aaaaaaaadoaaaaab"
}
SubProgram "gles " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 _FrustumCorners;
varying highp vec2 xlv_TEXCOORD0;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD2;
void main ()
{
  vec2 tmpvar_1;
  tmpvar_1 = _glesMultiTexCoord0.xy;
  highp vec4 tmpvar_2;
  tmpvar_2.xyw = _glesVertex.xyw;
  mediump float index_3;
  highp vec4 tmpvar_4;
  highp float tmpvar_5;
  tmpvar_5 = _glesVertex.z;
  index_3 = tmpvar_5;
  tmpvar_2.z = 0.1;
  int i_6;
  i_6 = int(index_3);
  mediump vec4 v_7;
  v_7.x = _FrustumCorners[0][i_6];
  v_7.y = _FrustumCorners[1][i_6];
  v_7.z = _FrustumCorners[2][i_6];
  v_7.w = _FrustumCorners[3][i_6];
  tmpvar_4.xyz = v_7.xyz;
  tmpvar_4.w = index_3;
  gl_Position = (glstate_matrix_mvp * tmpvar_2);
  xlv_TEXCOORD0 = tmpvar_1;
  xlv_TEXCOORD1 = tmpvar_1;
  xlv_TEXCOORD2 = tmpvar_4;
}



#endif
#ifdef FRAGMENT

#extension GL_EXT_shader_texture_lod : enable
uniform highp vec4 _ZBufferParams;
uniform sampler2D _Transmittance;
uniform sampler2D _Inscatter;
uniform highp float M_PI;
uniform highp float Rg;
uniform highp float Rt;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
uniform highp float RES_NU;
uniform highp vec3 SUN_DIR;
uniform highp float SUN_INTENSITY;
uniform highp vec3 betaR;
uniform highp float mieG;
uniform sampler2D _MainTex;
uniform highp float _Scale;
uniform highp float _global_alpha;
uniform highp float _Exposure;
uniform highp float _global_depth;
uniform highp vec3 _inCamPos;
uniform highp vec3 _Globals_Origin;
uniform sampler2D _CameraDepthTexture;
varying highp vec2 xlv_TEXCOORD0;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD2;
void main ()
{
  mediump vec4 tmpvar_1;
  highp float visib_2;
  highp vec4 col_3;
  lowp vec4 tmpvar_4;
  tmpvar_4 = texture2D (_MainTex, xlv_TEXCOORD0);
  col_3 = tmpvar_4;
  lowp vec4 tmpvar_5;
  tmpvar_5 = texture2D (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float tmpvar_6;
  highp float z_7;
  z_7 = tmpvar_5.x;
  tmpvar_6 = (1.0/(((_ZBufferParams.x * z_7) + _ZBufferParams.y)));
  highp vec3 tmpvar_8;
  tmpvar_8 = ((_inCamPos - _Globals_Origin) + (tmpvar_6 * xlv_TEXCOORD2).xyz);
  if ((tmpvar_6 == 1.0)) {
    tmpvar_1 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp vec3 camera_9;
    camera_9 = (_inCamPos - _Globals_Origin);
    highp vec3 _point_10;
    _point_10 = tmpvar_8;
    highp vec3 extinction_11;
    highp float mu_12;
    highp float rMu_13;
    highp float r_14;
    highp float d_15;
    highp vec3 result_16;
    result_16 = vec3(0.0, 0.0, 0.0);
    extinction_11 = vec3(1.0, 1.0, 1.0);
    highp vec3 tmpvar_17;
    tmpvar_17 = (tmpvar_8 - camera_9);
    highp float tmpvar_18;
    tmpvar_18 = (sqrt(dot (tmpvar_17, tmpvar_17)) * _Scale);
    d_15 = tmpvar_18;
    highp vec3 tmpvar_19;
    tmpvar_19 = (tmpvar_17 / tmpvar_18);
    highp float tmpvar_20;
    tmpvar_20 = (sqrt(dot (camera_9, camera_9)) * _Scale);
    r_14 = tmpvar_20;
    if (((tmpvar_20 / _Scale) < (0.9 * Rg))) {
      camera_9.y = (camera_9.y + Rg);
      _point_10.y = (tmpvar_8.y + Rg);
      r_14 = (sqrt(dot (camera_9, camera_9)) * _Scale);
    };
    highp float tmpvar_21;
    tmpvar_21 = dot (camera_9, tmpvar_19);
    rMu_13 = tmpvar_21;
    mu_12 = (tmpvar_21 / r_14);
    highp vec3 tmpvar_22;
    tmpvar_22 = (_point_10 - (tmpvar_19 * clamp (1.0, 0.0, tmpvar_18)));
    _point_10 = tmpvar_22;
    highp float tmpvar_23;
    tmpvar_23 = max ((-(tmpvar_21) - sqrt((((tmpvar_21 * tmpvar_21) - (r_14 * r_14)) + (Rt * Rt)))), 0.0);
    if (((tmpvar_23 > 0.0) && (tmpvar_23 < tmpvar_18))) {
      camera_9 = (camera_9 + (tmpvar_23 * tmpvar_19));
      highp float tmpvar_24;
      tmpvar_24 = (tmpvar_21 + tmpvar_23);
      rMu_13 = tmpvar_24;
      mu_12 = (tmpvar_24 / Rt);
      r_14 = Rt;
      d_15 = (tmpvar_18 - tmpvar_23);
    };
    if ((r_14 <= Rt)) {
      highp float muS1_25;
      highp float mu1_26;
      highp float r1_27;
      highp vec4 inScatter_28;
      highp float tmpvar_29;
      tmpvar_29 = dot (tmpvar_19, SUN_DIR);
      highp float tmpvar_30;
      tmpvar_30 = (dot (camera_9, SUN_DIR) / r_14);
      if ((r_14 < (Rg + 600.0))) {
        highp float tmpvar_31;
        tmpvar_31 = ((Rg + 600.0) / r_14);
        r_14 = (r_14 * tmpvar_31);
        rMu_13 = (rMu_13 * tmpvar_31);
        _point_10 = (tmpvar_22 * tmpvar_31);
      };
      highp float tmpvar_32;
      tmpvar_32 = sqrt(dot (_point_10, _point_10));
      r1_27 = tmpvar_32;
      highp float tmpvar_33;
      tmpvar_33 = (dot (_point_10, tmpvar_19) / tmpvar_32);
      mu1_26 = tmpvar_33;
      muS1_25 = (dot (_point_10, SUN_DIR) / tmpvar_32);
      if ((mu_12 > 0.0)) {
        highp vec3 tmpvar_34;
        highp float y_over_x_35;
        y_over_x_35 = (((mu_12 + 0.15) / 1.15) * 14.1014);
        highp float x_36;
        x_36 = (y_over_x_35 * inversesqrt(((y_over_x_35 * y_over_x_35) + 1.0)));
        highp vec4 tmpvar_37;
        tmpvar_37.zw = vec2(0.0, 0.0);
        tmpvar_37.x = ((sign(x_36) * (1.5708 - (sqrt((1.0 - abs(x_36))) * (1.5708 + (abs(x_36) * (-0.214602 + (abs(x_36) * (0.0865667 + (abs(x_36) * -0.0310296))))))))) / 1.5);
        tmpvar_37.y = sqrt(((r_14 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_38;
        tmpvar_38 = texture2DLodEXT (_Transmittance, tmpvar_37.xy, 0.0);
        tmpvar_34 = tmpvar_38.xyz;
        highp vec3 tmpvar_39;
        highp float y_over_x_40;
        y_over_x_40 = (((tmpvar_33 + 0.15) / 1.15) * 14.1014);
        highp float x_41;
        x_41 = (y_over_x_40 * inversesqrt(((y_over_x_40 * y_over_x_40) + 1.0)));
        highp vec4 tmpvar_42;
        tmpvar_42.zw = vec2(0.0, 0.0);
        tmpvar_42.x = ((sign(x_41) * (1.5708 - (sqrt((1.0 - abs(x_41))) * (1.5708 + (abs(x_41) * (-0.214602 + (abs(x_41) * (0.0865667 + (abs(x_41) * -0.0310296))))))))) / 1.5);
        tmpvar_42.y = sqrt(((tmpvar_32 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_43;
        tmpvar_43 = texture2DLodEXT (_Transmittance, tmpvar_42.xy, 0.0);
        tmpvar_39 = tmpvar_43.xyz;
        extinction_11 = min ((tmpvar_34 / tmpvar_39), vec3(1.0, 1.0, 1.0));
      } else {
        highp vec3 tmpvar_44;
        highp float y_over_x_45;
        y_over_x_45 = (((-(tmpvar_33) + 0.15) / 1.15) * 14.1014);
        highp float x_46;
        x_46 = (y_over_x_45 * inversesqrt(((y_over_x_45 * y_over_x_45) + 1.0)));
        highp vec4 tmpvar_47;
        tmpvar_47.zw = vec2(0.0, 0.0);
        tmpvar_47.x = ((sign(x_46) * (1.5708 - (sqrt((1.0 - abs(x_46))) * (1.5708 + (abs(x_46) * (-0.214602 + (abs(x_46) * (0.0865667 + (abs(x_46) * -0.0310296))))))))) / 1.5);
        tmpvar_47.y = sqrt(((tmpvar_32 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_48;
        tmpvar_48 = texture2DLodEXT (_Transmittance, tmpvar_47.xy, 0.0);
        tmpvar_44 = tmpvar_48.xyz;
        highp vec3 tmpvar_49;
        highp float y_over_x_50;
        y_over_x_50 = (((-(mu_12) + 0.15) / 1.15) * 14.1014);
        highp float x_51;
        x_51 = (y_over_x_50 * inversesqrt(((y_over_x_50 * y_over_x_50) + 1.0)));
        highp vec4 tmpvar_52;
        tmpvar_52.zw = vec2(0.0, 0.0);
        tmpvar_52.x = ((sign(x_51) * (1.5708 - (sqrt((1.0 - abs(x_51))) * (1.5708 + (abs(x_51) * (-0.214602 + (abs(x_51) * (0.0865667 + (abs(x_51) * -0.0310296))))))))) / 1.5);
        tmpvar_52.y = sqrt(((r_14 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_53;
        tmpvar_53 = texture2DLodEXT (_Transmittance, tmpvar_52.xy, 0.0);
        tmpvar_49 = tmpvar_53.xyz;
        extinction_11 = min ((tmpvar_44 / tmpvar_49), vec3(1.0, 1.0, 1.0));
      };
      highp float tmpvar_54;
      tmpvar_54 = -(sqrt((1.0 - ((Rg / r_14) * (Rg / r_14)))));
      highp float tmpvar_55;
      tmpvar_55 = abs((mu_12 - tmpvar_54));
      if ((tmpvar_55 < 0.004)) {
        highp vec4 inScatterA_56;
        highp vec4 inScatter0_57;
        highp float a_58;
        a_58 = (((mu_12 - tmpvar_54) + 0.004) / 0.008);
        highp float tmpvar_59;
        tmpvar_59 = (tmpvar_54 - 0.004);
        mu_12 = tmpvar_59;
        highp float tmpvar_60;
        tmpvar_60 = sqrt((((r_14 * r_14) + (d_15 * d_15)) + (((2.0 * r_14) * d_15) * tmpvar_59)));
        r1_27 = tmpvar_60;
        mu1_26 = (((r_14 * tmpvar_59) + d_15) / tmpvar_60);
        highp float uMu_61;
        highp float uR_62;
        highp float tmpvar_63;
        tmpvar_63 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_64;
        tmpvar_64 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_65;
        tmpvar_65 = (r_14 * tmpvar_59);
        highp float tmpvar_66;
        tmpvar_66 = (((tmpvar_65 * tmpvar_65) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_67;
        if (((tmpvar_65 < 0.0) && (tmpvar_66 > 0.0))) {
          highp vec4 tmpvar_68;
          tmpvar_68.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_68.w = (0.5 - (0.5 / RES_MU));
          tmpvar_67 = tmpvar_68;
        } else {
          highp vec4 tmpvar_69;
          tmpvar_69.x = -1.0;
          tmpvar_69.y = (tmpvar_63 * tmpvar_63);
          tmpvar_69.z = tmpvar_63;
          tmpvar_69.w = (0.5 + (0.5 / RES_MU));
          tmpvar_67 = tmpvar_69;
        };
        uR_62 = ((0.5 / RES_R) + ((tmpvar_64 / tmpvar_63) * (1.0 - (1.0/(RES_R)))));
        uMu_61 = (tmpvar_67.w + ((((tmpvar_65 * tmpvar_67.x) + sqrt((tmpvar_66 + tmpvar_67.y))) / (tmpvar_64 + tmpvar_67.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_70;
        y_over_x_70 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_71;
        x_71 = (y_over_x_70 * inversesqrt(((y_over_x_70 * y_over_x_70) + 1.0)));
        highp float tmpvar_72;
        tmpvar_72 = ((0.5 / RES_MU_S) + (((((sign(x_71) * (1.5708 - (sqrt((1.0 - abs(x_71))) * (1.5708 + (abs(x_71) * (-0.214602 + (abs(x_71) * (0.0865667 + (abs(x_71) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_73;
        tmpvar_73 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_74;
        tmpvar_74 = floor(tmpvar_73);
        highp float tmpvar_75;
        tmpvar_75 = (tmpvar_73 - tmpvar_74);
        highp float tmpvar_76;
        tmpvar_76 = (floor(((uR_62 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_77;
        tmpvar_77 = (floor((uR_62 * RES_R)) / RES_R);
        highp float tmpvar_78;
        tmpvar_78 = fract((uR_62 * RES_R));
        highp vec4 tmpvar_79;
        tmpvar_79.zw = vec2(0.0, 0.0);
        tmpvar_79.x = ((tmpvar_74 + tmpvar_72) / RES_NU);
        tmpvar_79.y = ((uMu_61 / RES_R) + tmpvar_76);
        lowp vec4 tmpvar_80;
        tmpvar_80 = texture2DLodEXT (_Inscatter, tmpvar_79.xy, 0.0);
        highp vec4 tmpvar_81;
        tmpvar_81.zw = vec2(0.0, 0.0);
        tmpvar_81.x = (((tmpvar_74 + tmpvar_72) + 1.0) / RES_NU);
        tmpvar_81.y = ((uMu_61 / RES_R) + tmpvar_76);
        lowp vec4 tmpvar_82;
        tmpvar_82 = texture2DLodEXT (_Inscatter, tmpvar_81.xy, 0.0);
        highp vec4 tmpvar_83;
        tmpvar_83.zw = vec2(0.0, 0.0);
        tmpvar_83.x = ((tmpvar_74 + tmpvar_72) / RES_NU);
        tmpvar_83.y = ((uMu_61 / RES_R) + tmpvar_77);
        lowp vec4 tmpvar_84;
        tmpvar_84 = texture2DLodEXT (_Inscatter, tmpvar_83.xy, 0.0);
        highp vec4 tmpvar_85;
        tmpvar_85.zw = vec2(0.0, 0.0);
        tmpvar_85.x = (((tmpvar_74 + tmpvar_72) + 1.0) / RES_NU);
        tmpvar_85.y = ((uMu_61 / RES_R) + tmpvar_77);
        lowp vec4 tmpvar_86;
        tmpvar_86 = texture2DLodEXT (_Inscatter, tmpvar_85.xy, 0.0);
        inScatter0_57 = ((((tmpvar_80 * (1.0 - tmpvar_75)) + (tmpvar_82 * tmpvar_75)) * (1.0 - tmpvar_78)) + (((tmpvar_84 * (1.0 - tmpvar_75)) + (tmpvar_86 * tmpvar_75)) * tmpvar_78));
        highp float uMu_87;
        highp float uR_88;
        highp float tmpvar_89;
        tmpvar_89 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_90;
        tmpvar_90 = sqrt(((tmpvar_60 * tmpvar_60) - (Rg * Rg)));
        highp float tmpvar_91;
        tmpvar_91 = (tmpvar_60 * mu1_26);
        highp float tmpvar_92;
        tmpvar_92 = (((tmpvar_91 * tmpvar_91) - (tmpvar_60 * tmpvar_60)) + (Rg * Rg));
        highp vec4 tmpvar_93;
        if (((tmpvar_91 < 0.0) && (tmpvar_92 > 0.0))) {
          highp vec4 tmpvar_94;
          tmpvar_94.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_94.w = (0.5 - (0.5 / RES_MU));
          tmpvar_93 = tmpvar_94;
        } else {
          highp vec4 tmpvar_95;
          tmpvar_95.x = -1.0;
          tmpvar_95.y = (tmpvar_89 * tmpvar_89);
          tmpvar_95.z = tmpvar_89;
          tmpvar_95.w = (0.5 + (0.5 / RES_MU));
          tmpvar_93 = tmpvar_95;
        };
        uR_88 = ((0.5 / RES_R) + ((tmpvar_90 / tmpvar_89) * (1.0 - (1.0/(RES_R)))));
        uMu_87 = (tmpvar_93.w + ((((tmpvar_91 * tmpvar_93.x) + sqrt((tmpvar_92 + tmpvar_93.y))) / (tmpvar_90 + tmpvar_93.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_96;
        y_over_x_96 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_97;
        x_97 = (y_over_x_96 * inversesqrt(((y_over_x_96 * y_over_x_96) + 1.0)));
        highp float tmpvar_98;
        tmpvar_98 = ((0.5 / RES_MU_S) + (((((sign(x_97) * (1.5708 - (sqrt((1.0 - abs(x_97))) * (1.5708 + (abs(x_97) * (-0.214602 + (abs(x_97) * (0.0865667 + (abs(x_97) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_99;
        tmpvar_99 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_100;
        tmpvar_100 = floor(tmpvar_99);
        highp float tmpvar_101;
        tmpvar_101 = (tmpvar_99 - tmpvar_100);
        highp float tmpvar_102;
        tmpvar_102 = (floor(((uR_88 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_103;
        tmpvar_103 = (floor((uR_88 * RES_R)) / RES_R);
        highp float tmpvar_104;
        tmpvar_104 = fract((uR_88 * RES_R));
        highp vec4 tmpvar_105;
        tmpvar_105.zw = vec2(0.0, 0.0);
        tmpvar_105.x = ((tmpvar_100 + tmpvar_98) / RES_NU);
        tmpvar_105.y = ((uMu_87 / RES_R) + tmpvar_102);
        lowp vec4 tmpvar_106;
        tmpvar_106 = texture2DLodEXT (_Inscatter, tmpvar_105.xy, 0.0);
        highp vec4 tmpvar_107;
        tmpvar_107.zw = vec2(0.0, 0.0);
        tmpvar_107.x = (((tmpvar_100 + tmpvar_98) + 1.0) / RES_NU);
        tmpvar_107.y = ((uMu_87 / RES_R) + tmpvar_102);
        lowp vec4 tmpvar_108;
        tmpvar_108 = texture2DLodEXT (_Inscatter, tmpvar_107.xy, 0.0);
        highp vec4 tmpvar_109;
        tmpvar_109.zw = vec2(0.0, 0.0);
        tmpvar_109.x = ((tmpvar_100 + tmpvar_98) / RES_NU);
        tmpvar_109.y = ((uMu_87 / RES_R) + tmpvar_103);
        lowp vec4 tmpvar_110;
        tmpvar_110 = texture2DLodEXT (_Inscatter, tmpvar_109.xy, 0.0);
        highp vec4 tmpvar_111;
        tmpvar_111.zw = vec2(0.0, 0.0);
        tmpvar_111.x = (((tmpvar_100 + tmpvar_98) + 1.0) / RES_NU);
        tmpvar_111.y = ((uMu_87 / RES_R) + tmpvar_103);
        lowp vec4 tmpvar_112;
        tmpvar_112 = texture2DLodEXT (_Inscatter, tmpvar_111.xy, 0.0);
        inScatterA_56 = max ((inScatter0_57 - (((((tmpvar_106 * (1.0 - tmpvar_101)) + (tmpvar_108 * tmpvar_101)) * (1.0 - tmpvar_104)) + (((tmpvar_110 * (1.0 - tmpvar_101)) + (tmpvar_112 * tmpvar_101)) * tmpvar_104)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0));
        highp float tmpvar_113;
        tmpvar_113 = (tmpvar_54 + 0.004);
        mu_12 = tmpvar_113;
        highp float tmpvar_114;
        tmpvar_114 = sqrt((((r_14 * r_14) + (d_15 * d_15)) + (((2.0 * r_14) * d_15) * tmpvar_113)));
        r1_27 = tmpvar_114;
        mu1_26 = (((r_14 * tmpvar_113) + d_15) / tmpvar_114);
        highp float uMu_115;
        highp float uR_116;
        highp float tmpvar_117;
        tmpvar_117 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_118;
        tmpvar_118 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_119;
        tmpvar_119 = (r_14 * tmpvar_113);
        highp float tmpvar_120;
        tmpvar_120 = (((tmpvar_119 * tmpvar_119) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_121;
        if (((tmpvar_119 < 0.0) && (tmpvar_120 > 0.0))) {
          highp vec4 tmpvar_122;
          tmpvar_122.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_122.w = (0.5 - (0.5 / RES_MU));
          tmpvar_121 = tmpvar_122;
        } else {
          highp vec4 tmpvar_123;
          tmpvar_123.x = -1.0;
          tmpvar_123.y = (tmpvar_117 * tmpvar_117);
          tmpvar_123.z = tmpvar_117;
          tmpvar_123.w = (0.5 + (0.5 / RES_MU));
          tmpvar_121 = tmpvar_123;
        };
        uR_116 = ((0.5 / RES_R) + ((tmpvar_118 / tmpvar_117) * (1.0 - (1.0/(RES_R)))));
        uMu_115 = (tmpvar_121.w + ((((tmpvar_119 * tmpvar_121.x) + sqrt((tmpvar_120 + tmpvar_121.y))) / (tmpvar_118 + tmpvar_121.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_124;
        y_over_x_124 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_125;
        x_125 = (y_over_x_124 * inversesqrt(((y_over_x_124 * y_over_x_124) + 1.0)));
        highp float tmpvar_126;
        tmpvar_126 = ((0.5 / RES_MU_S) + (((((sign(x_125) * (1.5708 - (sqrt((1.0 - abs(x_125))) * (1.5708 + (abs(x_125) * (-0.214602 + (abs(x_125) * (0.0865667 + (abs(x_125) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_127;
        tmpvar_127 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_128;
        tmpvar_128 = floor(tmpvar_127);
        highp float tmpvar_129;
        tmpvar_129 = (tmpvar_127 - tmpvar_128);
        highp float tmpvar_130;
        tmpvar_130 = (floor(((uR_116 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_131;
        tmpvar_131 = (floor((uR_116 * RES_R)) / RES_R);
        highp float tmpvar_132;
        tmpvar_132 = fract((uR_116 * RES_R));
        highp vec4 tmpvar_133;
        tmpvar_133.zw = vec2(0.0, 0.0);
        tmpvar_133.x = ((tmpvar_128 + tmpvar_126) / RES_NU);
        tmpvar_133.y = ((uMu_115 / RES_R) + tmpvar_130);
        lowp vec4 tmpvar_134;
        tmpvar_134 = texture2DLodEXT (_Inscatter, tmpvar_133.xy, 0.0);
        highp vec4 tmpvar_135;
        tmpvar_135.zw = vec2(0.0, 0.0);
        tmpvar_135.x = (((tmpvar_128 + tmpvar_126) + 1.0) / RES_NU);
        tmpvar_135.y = ((uMu_115 / RES_R) + tmpvar_130);
        lowp vec4 tmpvar_136;
        tmpvar_136 = texture2DLodEXT (_Inscatter, tmpvar_135.xy, 0.0);
        highp vec4 tmpvar_137;
        tmpvar_137.zw = vec2(0.0, 0.0);
        tmpvar_137.x = ((tmpvar_128 + tmpvar_126) / RES_NU);
        tmpvar_137.y = ((uMu_115 / RES_R) + tmpvar_131);
        lowp vec4 tmpvar_138;
        tmpvar_138 = texture2DLodEXT (_Inscatter, tmpvar_137.xy, 0.0);
        highp vec4 tmpvar_139;
        tmpvar_139.zw = vec2(0.0, 0.0);
        tmpvar_139.x = (((tmpvar_128 + tmpvar_126) + 1.0) / RES_NU);
        tmpvar_139.y = ((uMu_115 / RES_R) + tmpvar_131);
        lowp vec4 tmpvar_140;
        tmpvar_140 = texture2DLodEXT (_Inscatter, tmpvar_139.xy, 0.0);
        inScatter0_57 = ((((tmpvar_134 * (1.0 - tmpvar_129)) + (tmpvar_136 * tmpvar_129)) * (1.0 - tmpvar_132)) + (((tmpvar_138 * (1.0 - tmpvar_129)) + (tmpvar_140 * tmpvar_129)) * tmpvar_132));
        highp float uMu_141;
        highp float uR_142;
        highp float tmpvar_143;
        tmpvar_143 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_144;
        tmpvar_144 = sqrt(((tmpvar_114 * tmpvar_114) - (Rg * Rg)));
        highp float tmpvar_145;
        tmpvar_145 = (tmpvar_114 * mu1_26);
        highp float tmpvar_146;
        tmpvar_146 = (((tmpvar_145 * tmpvar_145) - (tmpvar_114 * tmpvar_114)) + (Rg * Rg));
        highp vec4 tmpvar_147;
        if (((tmpvar_145 < 0.0) && (tmpvar_146 > 0.0))) {
          highp vec4 tmpvar_148;
          tmpvar_148.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_148.w = (0.5 - (0.5 / RES_MU));
          tmpvar_147 = tmpvar_148;
        } else {
          highp vec4 tmpvar_149;
          tmpvar_149.x = -1.0;
          tmpvar_149.y = (tmpvar_143 * tmpvar_143);
          tmpvar_149.z = tmpvar_143;
          tmpvar_149.w = (0.5 + (0.5 / RES_MU));
          tmpvar_147 = tmpvar_149;
        };
        uR_142 = ((0.5 / RES_R) + ((tmpvar_144 / tmpvar_143) * (1.0 - (1.0/(RES_R)))));
        uMu_141 = (tmpvar_147.w + ((((tmpvar_145 * tmpvar_147.x) + sqrt((tmpvar_146 + tmpvar_147.y))) / (tmpvar_144 + tmpvar_147.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_150;
        y_over_x_150 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_151;
        x_151 = (y_over_x_150 * inversesqrt(((y_over_x_150 * y_over_x_150) + 1.0)));
        highp float tmpvar_152;
        tmpvar_152 = ((0.5 / RES_MU_S) + (((((sign(x_151) * (1.5708 - (sqrt((1.0 - abs(x_151))) * (1.5708 + (abs(x_151) * (-0.214602 + (abs(x_151) * (0.0865667 + (abs(x_151) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_153;
        tmpvar_153 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_154;
        tmpvar_154 = floor(tmpvar_153);
        highp float tmpvar_155;
        tmpvar_155 = (tmpvar_153 - tmpvar_154);
        highp float tmpvar_156;
        tmpvar_156 = (floor(((uR_142 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_157;
        tmpvar_157 = (floor((uR_142 * RES_R)) / RES_R);
        highp float tmpvar_158;
        tmpvar_158 = fract((uR_142 * RES_R));
        highp vec4 tmpvar_159;
        tmpvar_159.zw = vec2(0.0, 0.0);
        tmpvar_159.x = ((tmpvar_154 + tmpvar_152) / RES_NU);
        tmpvar_159.y = ((uMu_141 / RES_R) + tmpvar_156);
        lowp vec4 tmpvar_160;
        tmpvar_160 = texture2DLodEXT (_Inscatter, tmpvar_159.xy, 0.0);
        highp vec4 tmpvar_161;
        tmpvar_161.zw = vec2(0.0, 0.0);
        tmpvar_161.x = (((tmpvar_154 + tmpvar_152) + 1.0) / RES_NU);
        tmpvar_161.y = ((uMu_141 / RES_R) + tmpvar_156);
        lowp vec4 tmpvar_162;
        tmpvar_162 = texture2DLodEXT (_Inscatter, tmpvar_161.xy, 0.0);
        highp vec4 tmpvar_163;
        tmpvar_163.zw = vec2(0.0, 0.0);
        tmpvar_163.x = ((tmpvar_154 + tmpvar_152) / RES_NU);
        tmpvar_163.y = ((uMu_141 / RES_R) + tmpvar_157);
        lowp vec4 tmpvar_164;
        tmpvar_164 = texture2DLodEXT (_Inscatter, tmpvar_163.xy, 0.0);
        highp vec4 tmpvar_165;
        tmpvar_165.zw = vec2(0.0, 0.0);
        tmpvar_165.x = (((tmpvar_154 + tmpvar_152) + 1.0) / RES_NU);
        tmpvar_165.y = ((uMu_141 / RES_R) + tmpvar_157);
        lowp vec4 tmpvar_166;
        tmpvar_166 = texture2DLodEXT (_Inscatter, tmpvar_165.xy, 0.0);
        inScatter_28 = mix (inScatterA_56, max ((inScatter0_57 - (((((tmpvar_160 * (1.0 - tmpvar_155)) + (tmpvar_162 * tmpvar_155)) * (1.0 - tmpvar_158)) + (((tmpvar_164 * (1.0 - tmpvar_155)) + (tmpvar_166 * tmpvar_155)) * tmpvar_158)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0)), vec4(a_58));
      } else {
        highp vec4 inScatter0_1_167;
        highp float uMu_168;
        highp float uR_169;
        highp float tmpvar_170;
        tmpvar_170 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_171;
        tmpvar_171 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_172;
        tmpvar_172 = (r_14 * mu_12);
        highp float tmpvar_173;
        tmpvar_173 = (((tmpvar_172 * tmpvar_172) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_174;
        if (((tmpvar_172 < 0.0) && (tmpvar_173 > 0.0))) {
          highp vec4 tmpvar_175;
          tmpvar_175.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_175.w = (0.5 - (0.5 / RES_MU));
          tmpvar_174 = tmpvar_175;
        } else {
          highp vec4 tmpvar_176;
          tmpvar_176.x = -1.0;
          tmpvar_176.y = (tmpvar_170 * tmpvar_170);
          tmpvar_176.z = tmpvar_170;
          tmpvar_176.w = (0.5 + (0.5 / RES_MU));
          tmpvar_174 = tmpvar_176;
        };
        uR_169 = ((0.5 / RES_R) + ((tmpvar_171 / tmpvar_170) * (1.0 - (1.0/(RES_R)))));
        uMu_168 = (tmpvar_174.w + ((((tmpvar_172 * tmpvar_174.x) + sqrt((tmpvar_173 + tmpvar_174.y))) / (tmpvar_171 + tmpvar_174.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_177;
        y_over_x_177 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_178;
        x_178 = (y_over_x_177 * inversesqrt(((y_over_x_177 * y_over_x_177) + 1.0)));
        highp float tmpvar_179;
        tmpvar_179 = ((0.5 / RES_MU_S) + (((((sign(x_178) * (1.5708 - (sqrt((1.0 - abs(x_178))) * (1.5708 + (abs(x_178) * (-0.214602 + (abs(x_178) * (0.0865667 + (abs(x_178) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_180;
        tmpvar_180 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_181;
        tmpvar_181 = floor(tmpvar_180);
        highp float tmpvar_182;
        tmpvar_182 = (tmpvar_180 - tmpvar_181);
        highp float tmpvar_183;
        tmpvar_183 = (floor(((uR_169 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_184;
        tmpvar_184 = (floor((uR_169 * RES_R)) / RES_R);
        highp float tmpvar_185;
        tmpvar_185 = fract((uR_169 * RES_R));
        highp vec4 tmpvar_186;
        tmpvar_186.zw = vec2(0.0, 0.0);
        tmpvar_186.x = ((tmpvar_181 + tmpvar_179) / RES_NU);
        tmpvar_186.y = ((uMu_168 / RES_R) + tmpvar_183);
        lowp vec4 tmpvar_187;
        tmpvar_187 = texture2DLodEXT (_Inscatter, tmpvar_186.xy, 0.0);
        highp vec4 tmpvar_188;
        tmpvar_188.zw = vec2(0.0, 0.0);
        tmpvar_188.x = (((tmpvar_181 + tmpvar_179) + 1.0) / RES_NU);
        tmpvar_188.y = ((uMu_168 / RES_R) + tmpvar_183);
        lowp vec4 tmpvar_189;
        tmpvar_189 = texture2DLodEXT (_Inscatter, tmpvar_188.xy, 0.0);
        highp vec4 tmpvar_190;
        tmpvar_190.zw = vec2(0.0, 0.0);
        tmpvar_190.x = ((tmpvar_181 + tmpvar_179) / RES_NU);
        tmpvar_190.y = ((uMu_168 / RES_R) + tmpvar_184);
        lowp vec4 tmpvar_191;
        tmpvar_191 = texture2DLodEXT (_Inscatter, tmpvar_190.xy, 0.0);
        highp vec4 tmpvar_192;
        tmpvar_192.zw = vec2(0.0, 0.0);
        tmpvar_192.x = (((tmpvar_181 + tmpvar_179) + 1.0) / RES_NU);
        tmpvar_192.y = ((uMu_168 / RES_R) + tmpvar_184);
        lowp vec4 tmpvar_193;
        tmpvar_193 = texture2DLodEXT (_Inscatter, tmpvar_192.xy, 0.0);
        inScatter0_1_167 = ((((tmpvar_187 * (1.0 - tmpvar_182)) + (tmpvar_189 * tmpvar_182)) * (1.0 - tmpvar_185)) + (((tmpvar_191 * (1.0 - tmpvar_182)) + (tmpvar_193 * tmpvar_182)) * tmpvar_185));
        highp float uMu_194;
        highp float uR_195;
        highp float tmpvar_196;
        tmpvar_196 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_197;
        tmpvar_197 = sqrt(((r1_27 * r1_27) - (Rg * Rg)));
        highp float tmpvar_198;
        tmpvar_198 = (r1_27 * mu1_26);
        highp float tmpvar_199;
        tmpvar_199 = (((tmpvar_198 * tmpvar_198) - (r1_27 * r1_27)) + (Rg * Rg));
        highp vec4 tmpvar_200;
        if (((tmpvar_198 < 0.0) && (tmpvar_199 > 0.0))) {
          highp vec4 tmpvar_201;
          tmpvar_201.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_201.w = (0.5 - (0.5 / RES_MU));
          tmpvar_200 = tmpvar_201;
        } else {
          highp vec4 tmpvar_202;
          tmpvar_202.x = -1.0;
          tmpvar_202.y = (tmpvar_196 * tmpvar_196);
          tmpvar_202.z = tmpvar_196;
          tmpvar_202.w = (0.5 + (0.5 / RES_MU));
          tmpvar_200 = tmpvar_202;
        };
        uR_195 = ((0.5 / RES_R) + ((tmpvar_197 / tmpvar_196) * (1.0 - (1.0/(RES_R)))));
        uMu_194 = (tmpvar_200.w + ((((tmpvar_198 * tmpvar_200.x) + sqrt((tmpvar_199 + tmpvar_200.y))) / (tmpvar_197 + tmpvar_200.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_203;
        y_over_x_203 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_204;
        x_204 = (y_over_x_203 * inversesqrt(((y_over_x_203 * y_over_x_203) + 1.0)));
        highp float tmpvar_205;
        tmpvar_205 = ((0.5 / RES_MU_S) + (((((sign(x_204) * (1.5708 - (sqrt((1.0 - abs(x_204))) * (1.5708 + (abs(x_204) * (-0.214602 + (abs(x_204) * (0.0865667 + (abs(x_204) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_206;
        tmpvar_206 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_207;
        tmpvar_207 = floor(tmpvar_206);
        highp float tmpvar_208;
        tmpvar_208 = (tmpvar_206 - tmpvar_207);
        highp float tmpvar_209;
        tmpvar_209 = (floor(((uR_195 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_210;
        tmpvar_210 = (floor((uR_195 * RES_R)) / RES_R);
        highp float tmpvar_211;
        tmpvar_211 = fract((uR_195 * RES_R));
        highp vec4 tmpvar_212;
        tmpvar_212.zw = vec2(0.0, 0.0);
        tmpvar_212.x = ((tmpvar_207 + tmpvar_205) / RES_NU);
        tmpvar_212.y = ((uMu_194 / RES_R) + tmpvar_209);
        lowp vec4 tmpvar_213;
        tmpvar_213 = texture2DLodEXT (_Inscatter, tmpvar_212.xy, 0.0);
        highp vec4 tmpvar_214;
        tmpvar_214.zw = vec2(0.0, 0.0);
        tmpvar_214.x = (((tmpvar_207 + tmpvar_205) + 1.0) / RES_NU);
        tmpvar_214.y = ((uMu_194 / RES_R) + tmpvar_209);
        lowp vec4 tmpvar_215;
        tmpvar_215 = texture2DLodEXT (_Inscatter, tmpvar_214.xy, 0.0);
        highp vec4 tmpvar_216;
        tmpvar_216.zw = vec2(0.0, 0.0);
        tmpvar_216.x = ((tmpvar_207 + tmpvar_205) / RES_NU);
        tmpvar_216.y = ((uMu_194 / RES_R) + tmpvar_210);
        lowp vec4 tmpvar_217;
        tmpvar_217 = texture2DLodEXT (_Inscatter, tmpvar_216.xy, 0.0);
        highp vec4 tmpvar_218;
        tmpvar_218.zw = vec2(0.0, 0.0);
        tmpvar_218.x = (((tmpvar_207 + tmpvar_205) + 1.0) / RES_NU);
        tmpvar_218.y = ((uMu_194 / RES_R) + tmpvar_210);
        lowp vec4 tmpvar_219;
        tmpvar_219 = texture2DLodEXT (_Inscatter, tmpvar_218.xy, 0.0);
        inScatter_28 = max ((inScatter0_1_167 - (((((tmpvar_213 * (1.0 - tmpvar_208)) + (tmpvar_215 * tmpvar_208)) * (1.0 - tmpvar_211)) + (((tmpvar_217 * (1.0 - tmpvar_208)) + (tmpvar_219 * tmpvar_208)) * tmpvar_211)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0));
      };
      highp float t_220;
      t_220 = max (min ((tmpvar_30 / 0.02), 1.0), 0.0);
      inScatter_28.w = (inScatter_28.w * (t_220 * (t_220 * (3.0 - (2.0 * t_220)))));
      result_16 = ((inScatter_28.xyz * ((3.0 / (16.0 * M_PI)) * (1.0 + (tmpvar_29 * tmpvar_29)))) + ((((inScatter_28.xyz * inScatter_28.w) / max (inScatter_28.x, 0.0001)) * (betaR.x / betaR)) * (((((1.5 / (4.0 * M_PI)) * (1.0 - (mieG * mieG))) * pow (((1.0 + (mieG * mieG)) - ((2.0 * mieG) * tmpvar_29)), -1.5)) * (1.0 + (tmpvar_29 * tmpvar_29))) / (2.0 + (mieG * mieG)))));
    };
    col_3.xyz = ((col_3.xyz * extinction_11) + (_global_depth * (result_16 * SUN_INTENSITY)));
    visib_2 = 1.0;
    if ((tmpvar_6 <= 0.015)) {
      visib_2 = (tmpvar_6 / 0.015);
    };
    highp vec3 L_221;
    highp vec3 tmpvar_222;
    tmpvar_222 = (col_3.xyz * _Exposure);
    L_221 = tmpvar_222;
    highp float tmpvar_223;
    if ((tmpvar_222.x < 1.413)) {
      tmpvar_223 = pow ((tmpvar_222.x * 0.38317), 0.454545);
    } else {
      tmpvar_223 = (1.0 - exp(-(tmpvar_222.x)));
    };
    L_221.x = tmpvar_223;
    highp float tmpvar_224;
    if ((tmpvar_222.y < 1.413)) {
      tmpvar_224 = pow ((tmpvar_222.y * 0.38317), 0.454545);
    } else {
      tmpvar_224 = (1.0 - exp(-(tmpvar_222.y)));
    };
    L_221.y = tmpvar_224;
    highp float tmpvar_225;
    if ((tmpvar_222.z < 1.413)) {
      tmpvar_225 = pow ((tmpvar_222.z * 0.38317), 0.454545);
    } else {
      tmpvar_225 = (1.0 - exp(-(tmpvar_222.z)));
    };
    L_221.z = tmpvar_225;
    highp vec4 tmpvar_226;
    tmpvar_226.xyz = L_221;
    tmpvar_226.w = (_global_alpha * visib_2);
    tmpvar_1 = tmpvar_226;
  };
  gl_FragData[0] = tmpvar_1;
}



#endif"
}
SubProgram "glesdesktop " {
"!!GLES


#ifdef VERTEX

#ifndef SHADER_API_GLES
    #define SHADER_API_GLES 1
#endif
#ifndef SHADER_API_DESKTOP
    #define SHADER_API_DESKTOP 1
#endif
#define gl_Vertex _glesVertex
attribute vec4 _glesVertex;
#define gl_MultiTexCoord0 _glesMultiTexCoord0
attribute vec4 _glesMultiTexCoord0;
mat2 xll_transpose_mf2x2(mat2 m) {
  return mat2( m[0][0], m[1][0], m[0][1], m[1][1]);
}
mat3 xll_transpose_mf3x3(mat3 m) {
  return mat3( m[0][0], m[1][0], m[2][0],
               m[0][1], m[1][1], m[2][1],
               m[0][2], m[1][2], m[2][2]);
}
mat4 xll_transpose_mf4x4(mat4 m) {
  return mat4( m[0][0], m[1][0], m[2][0], m[3][0],
               m[0][1], m[1][1], m[2][1], m[3][1],
               m[0][2], m[1][2], m[2][2], m[3][2],
               m[0][3], m[1][3], m[2][3], m[3][3]);
}
vec2 xll_matrixindex_mf2x2_i (mat2 m, int i) { vec2 v; v.x=m[0][i]; v.y=m[1][i]; return v; }
vec3 xll_matrixindex_mf3x3_i (mat3 m, int i) { vec3 v; v.x=m[0][i]; v.y=m[1][i]; v.z=m[2][i]; return v; }
vec4 xll_matrixindex_mf4x4_i (mat4 m, int i) { vec4 v; v.x=m[0][i]; v.y=m[1][i]; v.z=m[2][i]; v.w=m[3][i]; return v; }
#if defined(SHADER_API_GLES) && defined(SHADER_API_DESKTOP)
vec2 xll_matrixindexdynamic_mf2x2_i (mat2 m, int i) {
 mat2 m2 = xll_transpose(m);
 return i==0?m2[0]:m2[1];
}
vec3 xll_matrixindexdynamic_mf3x3_i (mat3 m, int i) {
 mat3 m2 = xll_transpose(m);
 return i < 2?(i==0?m2[0]:m2[1]):(m2[2]);
}
vec4 xll_matrixindexdynamic_mf4x4_i (mat4 m, int i) {
 mat4 m2 = xll_transpose(m);
 return i < 2?(i==0?m2[0]:m2[1]):(i==3?m2[3]:m2[2]);
}
#else
vec2 xll_matrixindexdynamic_mf2x2_i (mat2 m, int i) { return xll_matrixindex_mf2x2_i (m, i); }
vec3 xll_matrixindexdynamic_mf3x3_i (mat3 m, int i) { return xll_matrixindex_mf3x3_i (m, i); }
vec4 xll_matrixindexdynamic_mf4x4_i (mat4 m, int i) { return xll_matrixindex_mf4x4_i (m, i); }
#endif
#line 221
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 275
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 271
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 45
struct v2f {
    highp vec4 pos;
    highp vec2 uv;
    highp vec2 uv_depth;
    highp vec4 interpolatedRay;
};
#line 16
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
#line 21
uniform highp vec3 _WorldSpaceCameraPos;
#line 27
uniform highp vec4 _ProjectionParams;
#line 33
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
#line 40
uniform highp vec4 unity_CameraWorldClipPlanes[6];
#line 53
uniform highp vec4 _WorldSpaceLightPos0;
uniform highp vec4 _LightPositionRange;
#line 58
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
uniform highp vec4 unity_4LightAtten0;
#line 63
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
#line 69
uniform highp vec4 unity_LightAtten[8];
uniform highp vec4 unity_SpotDirection[8];
#line 73
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 77
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 83
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 90
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 94
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 110
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 122
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
#line 133
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 149
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 173
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
#line 182
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 48
uniform lowp vec4 unity_ColorSpaceGrey;
#line 89
#line 104
#line 119
#line 125
#line 143
#line 175
#line 192
#line 227
#line 238
#line 248
#line 256
#line 280
#line 286
#line 296
#line 305
#line 312
#line 321
#line 329
#line 338
#line 357
#line 363
#line 376
#line 387
#line 392
#line 418
#line 434
#line 447
#line 35
uniform sampler2D _Transmittance;
uniform sampler2D _Inscatter;
uniform highp float M_PI;
#line 39
uniform highp vec3 EARTH_POS;
uniform highp float SCALE;
uniform highp float Rg;
uniform highp float Rt;
#line 43
uniform highp float RL;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
#line 47
uniform highp float RES_NU;
uniform highp vec3 SUN_DIR;
uniform highp float SUN_INTENSITY;
uniform highp vec3 betaR;
#line 51
uniform highp float mieG;
#line 62
#line 131
#line 28
uniform sampler2D _MainTex;
uniform sampler2D _SkyDome;
uniform highp float _Scale;
uniform highp float _global_alpha;
uniform highp float _Exposure;
#line 32
uniform highp float _global_depth;
uniform highp vec3 _inCamPos;
uniform highp vec3 _Globals_Origin;
uniform highp vec3 _CameraForwardDirection;
#line 36
uniform sampler2D _CameraDepthTexture;
uniform highp mat4 _FrustumCorners;
uniform highp vec4 _MainTex_TexelSize;
#line 40
uniform highp mat4 _Globals_CameraToWorld;
#line 53
#line 73
#line 82
#line 53
v2f vert( in appdata_img v ) {
    v2f o;
    mediump float index = v.vertex.z;
    #line 57
    v.vertex.z = 0.1;
    o.pos = (glstate_matrix_mvp * v.vertex);
    o.uv = v.texcoord.xy;
    o.uv_depth = v.texcoord.xy;
    #line 67
    o.interpolatedRay = xll_matrixindexdynamic_mf4x4_i (_FrustumCorners, int(index));
    o.interpolatedRay.w = index;
    return o;
}
varying highp vec2 xlv_TEXCOORD0;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD2;
void main() {
    v2f xl_retval;
    appdata_img xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.texcoord = vec2(gl_MultiTexCoord0);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec2(xl_retval.uv);
    xlv_TEXCOORD1 = vec2(xl_retval.uv_depth);
    xlv_TEXCOORD2 = vec4(xl_retval.interpolatedRay);
}
/* NOTE: GLSL optimization failed
0:0(0): error: no matching function for call to `xll_transpose(mat2)'
0:0(0): error: no matching function for call to `xll_transpose(mat3)'
0:0(0): error: no matching function for call to `xll_transpose(mat4)'
0:226(2): warning: empty declaration
0:279(2): warning: empty declaration
0:275(2): warning: empty declaration
0:51(2): warning: empty declaration
*/


#endif
#ifdef FRAGMENT

#extension GL_EXT_shader_texture_lod : enable
uniform highp vec4 _ZBufferParams;
uniform sampler2D _Transmittance;
uniform sampler2D _Inscatter;
uniform highp float M_PI;
uniform highp float Rg;
uniform highp float Rt;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
uniform highp float RES_NU;
uniform highp vec3 SUN_DIR;
uniform highp float SUN_INTENSITY;
uniform highp vec3 betaR;
uniform highp float mieG;
uniform sampler2D _MainTex;
uniform highp float _Scale;
uniform highp float _global_alpha;
uniform highp float _Exposure;
uniform highp float _global_depth;
uniform highp vec3 _inCamPos;
uniform highp vec3 _Globals_Origin;
uniform sampler2D _CameraDepthTexture;
varying highp vec2 xlv_TEXCOORD0;
varying highp vec2 xlv_TEXCOORD1;
varying highp vec4 xlv_TEXCOORD2;
void main ()
{
  mediump vec4 tmpvar_1;
  highp float visib_2;
  highp vec4 col_3;
  lowp vec4 tmpvar_4;
  tmpvar_4 = texture2D (_MainTex, xlv_TEXCOORD0);
  col_3 = tmpvar_4;
  lowp vec4 tmpvar_5;
  tmpvar_5 = texture2D (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float tmpvar_6;
  highp float z_7;
  z_7 = tmpvar_5.x;
  tmpvar_6 = (1.0/(((_ZBufferParams.x * z_7) + _ZBufferParams.y)));
  highp vec3 tmpvar_8;
  tmpvar_8 = ((_inCamPos - _Globals_Origin) + (tmpvar_6 * xlv_TEXCOORD2).xyz);
  if ((tmpvar_6 == 1.0)) {
    tmpvar_1 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp vec3 camera_9;
    camera_9 = (_inCamPos - _Globals_Origin);
    highp vec3 _point_10;
    _point_10 = tmpvar_8;
    highp vec3 extinction_11;
    highp float mu_12;
    highp float rMu_13;
    highp float r_14;
    highp float d_15;
    highp vec3 result_16;
    result_16 = vec3(0.0, 0.0, 0.0);
    extinction_11 = vec3(1.0, 1.0, 1.0);
    highp vec3 tmpvar_17;
    tmpvar_17 = (tmpvar_8 - camera_9);
    highp float tmpvar_18;
    tmpvar_18 = (sqrt(dot (tmpvar_17, tmpvar_17)) * _Scale);
    d_15 = tmpvar_18;
    highp vec3 tmpvar_19;
    tmpvar_19 = (tmpvar_17 / tmpvar_18);
    highp float tmpvar_20;
    tmpvar_20 = (sqrt(dot (camera_9, camera_9)) * _Scale);
    r_14 = tmpvar_20;
    if (((tmpvar_20 / _Scale) < (0.9 * Rg))) {
      camera_9.y = (camera_9.y + Rg);
      _point_10.y = (tmpvar_8.y + Rg);
      r_14 = (sqrt(dot (camera_9, camera_9)) * _Scale);
    };
    highp float tmpvar_21;
    tmpvar_21 = dot (camera_9, tmpvar_19);
    rMu_13 = tmpvar_21;
    mu_12 = (tmpvar_21 / r_14);
    highp vec3 tmpvar_22;
    tmpvar_22 = (_point_10 - (tmpvar_19 * clamp (1.0, 0.0, tmpvar_18)));
    _point_10 = tmpvar_22;
    highp float tmpvar_23;
    tmpvar_23 = max ((-(tmpvar_21) - sqrt((((tmpvar_21 * tmpvar_21) - (r_14 * r_14)) + (Rt * Rt)))), 0.0);
    if (((tmpvar_23 > 0.0) && (tmpvar_23 < tmpvar_18))) {
      camera_9 = (camera_9 + (tmpvar_23 * tmpvar_19));
      highp float tmpvar_24;
      tmpvar_24 = (tmpvar_21 + tmpvar_23);
      rMu_13 = tmpvar_24;
      mu_12 = (tmpvar_24 / Rt);
      r_14 = Rt;
      d_15 = (tmpvar_18 - tmpvar_23);
    };
    if ((r_14 <= Rt)) {
      highp float muS1_25;
      highp float mu1_26;
      highp float r1_27;
      highp vec4 inScatter_28;
      highp float tmpvar_29;
      tmpvar_29 = dot (tmpvar_19, SUN_DIR);
      highp float tmpvar_30;
      tmpvar_30 = (dot (camera_9, SUN_DIR) / r_14);
      if ((r_14 < (Rg + 600.0))) {
        highp float tmpvar_31;
        tmpvar_31 = ((Rg + 600.0) / r_14);
        r_14 = (r_14 * tmpvar_31);
        rMu_13 = (rMu_13 * tmpvar_31);
        _point_10 = (tmpvar_22 * tmpvar_31);
      };
      highp float tmpvar_32;
      tmpvar_32 = sqrt(dot (_point_10, _point_10));
      r1_27 = tmpvar_32;
      highp float tmpvar_33;
      tmpvar_33 = (dot (_point_10, tmpvar_19) / tmpvar_32);
      mu1_26 = tmpvar_33;
      muS1_25 = (dot (_point_10, SUN_DIR) / tmpvar_32);
      if ((mu_12 > 0.0)) {
        highp vec3 tmpvar_34;
        highp float y_over_x_35;
        y_over_x_35 = (((mu_12 + 0.15) / 1.15) * 14.1014);
        highp float x_36;
        x_36 = (y_over_x_35 * inversesqrt(((y_over_x_35 * y_over_x_35) + 1.0)));
        highp vec4 tmpvar_37;
        tmpvar_37.zw = vec2(0.0, 0.0);
        tmpvar_37.x = ((sign(x_36) * (1.5708 - (sqrt((1.0 - abs(x_36))) * (1.5708 + (abs(x_36) * (-0.214602 + (abs(x_36) * (0.0865667 + (abs(x_36) * -0.0310296))))))))) / 1.5);
        tmpvar_37.y = sqrt(((r_14 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_38;
        tmpvar_38 = texture2DLodEXT (_Transmittance, tmpvar_37.xy, 0.0);
        tmpvar_34 = tmpvar_38.xyz;
        highp vec3 tmpvar_39;
        highp float y_over_x_40;
        y_over_x_40 = (((tmpvar_33 + 0.15) / 1.15) * 14.1014);
        highp float x_41;
        x_41 = (y_over_x_40 * inversesqrt(((y_over_x_40 * y_over_x_40) + 1.0)));
        highp vec4 tmpvar_42;
        tmpvar_42.zw = vec2(0.0, 0.0);
        tmpvar_42.x = ((sign(x_41) * (1.5708 - (sqrt((1.0 - abs(x_41))) * (1.5708 + (abs(x_41) * (-0.214602 + (abs(x_41) * (0.0865667 + (abs(x_41) * -0.0310296))))))))) / 1.5);
        tmpvar_42.y = sqrt(((tmpvar_32 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_43;
        tmpvar_43 = texture2DLodEXT (_Transmittance, tmpvar_42.xy, 0.0);
        tmpvar_39 = tmpvar_43.xyz;
        extinction_11 = min ((tmpvar_34 / tmpvar_39), vec3(1.0, 1.0, 1.0));
      } else {
        highp vec3 tmpvar_44;
        highp float y_over_x_45;
        y_over_x_45 = (((-(tmpvar_33) + 0.15) / 1.15) * 14.1014);
        highp float x_46;
        x_46 = (y_over_x_45 * inversesqrt(((y_over_x_45 * y_over_x_45) + 1.0)));
        highp vec4 tmpvar_47;
        tmpvar_47.zw = vec2(0.0, 0.0);
        tmpvar_47.x = ((sign(x_46) * (1.5708 - (sqrt((1.0 - abs(x_46))) * (1.5708 + (abs(x_46) * (-0.214602 + (abs(x_46) * (0.0865667 + (abs(x_46) * -0.0310296))))))))) / 1.5);
        tmpvar_47.y = sqrt(((tmpvar_32 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_48;
        tmpvar_48 = texture2DLodEXT (_Transmittance, tmpvar_47.xy, 0.0);
        tmpvar_44 = tmpvar_48.xyz;
        highp vec3 tmpvar_49;
        highp float y_over_x_50;
        y_over_x_50 = (((-(mu_12) + 0.15) / 1.15) * 14.1014);
        highp float x_51;
        x_51 = (y_over_x_50 * inversesqrt(((y_over_x_50 * y_over_x_50) + 1.0)));
        highp vec4 tmpvar_52;
        tmpvar_52.zw = vec2(0.0, 0.0);
        tmpvar_52.x = ((sign(x_51) * (1.5708 - (sqrt((1.0 - abs(x_51))) * (1.5708 + (abs(x_51) * (-0.214602 + (abs(x_51) * (0.0865667 + (abs(x_51) * -0.0310296))))))))) / 1.5);
        tmpvar_52.y = sqrt(((r_14 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_53;
        tmpvar_53 = texture2DLodEXT (_Transmittance, tmpvar_52.xy, 0.0);
        tmpvar_49 = tmpvar_53.xyz;
        extinction_11 = min ((tmpvar_44 / tmpvar_49), vec3(1.0, 1.0, 1.0));
      };
      highp float tmpvar_54;
      tmpvar_54 = -(sqrt((1.0 - ((Rg / r_14) * (Rg / r_14)))));
      highp float tmpvar_55;
      tmpvar_55 = abs((mu_12 - tmpvar_54));
      if ((tmpvar_55 < 0.004)) {
        highp vec4 inScatterA_56;
        highp vec4 inScatter0_57;
        highp float a_58;
        a_58 = (((mu_12 - tmpvar_54) + 0.004) / 0.008);
        highp float tmpvar_59;
        tmpvar_59 = (tmpvar_54 - 0.004);
        mu_12 = tmpvar_59;
        highp float tmpvar_60;
        tmpvar_60 = sqrt((((r_14 * r_14) + (d_15 * d_15)) + (((2.0 * r_14) * d_15) * tmpvar_59)));
        r1_27 = tmpvar_60;
        mu1_26 = (((r_14 * tmpvar_59) + d_15) / tmpvar_60);
        highp float uMu_61;
        highp float uR_62;
        highp float tmpvar_63;
        tmpvar_63 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_64;
        tmpvar_64 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_65;
        tmpvar_65 = (r_14 * tmpvar_59);
        highp float tmpvar_66;
        tmpvar_66 = (((tmpvar_65 * tmpvar_65) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_67;
        if (((tmpvar_65 < 0.0) && (tmpvar_66 > 0.0))) {
          highp vec4 tmpvar_68;
          tmpvar_68.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_68.w = (0.5 - (0.5 / RES_MU));
          tmpvar_67 = tmpvar_68;
        } else {
          highp vec4 tmpvar_69;
          tmpvar_69.x = -1.0;
          tmpvar_69.y = (tmpvar_63 * tmpvar_63);
          tmpvar_69.z = tmpvar_63;
          tmpvar_69.w = (0.5 + (0.5 / RES_MU));
          tmpvar_67 = tmpvar_69;
        };
        uR_62 = ((0.5 / RES_R) + ((tmpvar_64 / tmpvar_63) * (1.0 - (1.0/(RES_R)))));
        uMu_61 = (tmpvar_67.w + ((((tmpvar_65 * tmpvar_67.x) + sqrt((tmpvar_66 + tmpvar_67.y))) / (tmpvar_64 + tmpvar_67.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_70;
        y_over_x_70 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_71;
        x_71 = (y_over_x_70 * inversesqrt(((y_over_x_70 * y_over_x_70) + 1.0)));
        highp float tmpvar_72;
        tmpvar_72 = ((0.5 / RES_MU_S) + (((((sign(x_71) * (1.5708 - (sqrt((1.0 - abs(x_71))) * (1.5708 + (abs(x_71) * (-0.214602 + (abs(x_71) * (0.0865667 + (abs(x_71) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_73;
        tmpvar_73 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_74;
        tmpvar_74 = floor(tmpvar_73);
        highp float tmpvar_75;
        tmpvar_75 = (tmpvar_73 - tmpvar_74);
        highp float tmpvar_76;
        tmpvar_76 = (floor(((uR_62 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_77;
        tmpvar_77 = (floor((uR_62 * RES_R)) / RES_R);
        highp float tmpvar_78;
        tmpvar_78 = fract((uR_62 * RES_R));
        highp vec4 tmpvar_79;
        tmpvar_79.zw = vec2(0.0, 0.0);
        tmpvar_79.x = ((tmpvar_74 + tmpvar_72) / RES_NU);
        tmpvar_79.y = ((uMu_61 / RES_R) + tmpvar_76);
        lowp vec4 tmpvar_80;
        tmpvar_80 = texture2DLodEXT (_Inscatter, tmpvar_79.xy, 0.0);
        highp vec4 tmpvar_81;
        tmpvar_81.zw = vec2(0.0, 0.0);
        tmpvar_81.x = (((tmpvar_74 + tmpvar_72) + 1.0) / RES_NU);
        tmpvar_81.y = ((uMu_61 / RES_R) + tmpvar_76);
        lowp vec4 tmpvar_82;
        tmpvar_82 = texture2DLodEXT (_Inscatter, tmpvar_81.xy, 0.0);
        highp vec4 tmpvar_83;
        tmpvar_83.zw = vec2(0.0, 0.0);
        tmpvar_83.x = ((tmpvar_74 + tmpvar_72) / RES_NU);
        tmpvar_83.y = ((uMu_61 / RES_R) + tmpvar_77);
        lowp vec4 tmpvar_84;
        tmpvar_84 = texture2DLodEXT (_Inscatter, tmpvar_83.xy, 0.0);
        highp vec4 tmpvar_85;
        tmpvar_85.zw = vec2(0.0, 0.0);
        tmpvar_85.x = (((tmpvar_74 + tmpvar_72) + 1.0) / RES_NU);
        tmpvar_85.y = ((uMu_61 / RES_R) + tmpvar_77);
        lowp vec4 tmpvar_86;
        tmpvar_86 = texture2DLodEXT (_Inscatter, tmpvar_85.xy, 0.0);
        inScatter0_57 = ((((tmpvar_80 * (1.0 - tmpvar_75)) + (tmpvar_82 * tmpvar_75)) * (1.0 - tmpvar_78)) + (((tmpvar_84 * (1.0 - tmpvar_75)) + (tmpvar_86 * tmpvar_75)) * tmpvar_78));
        highp float uMu_87;
        highp float uR_88;
        highp float tmpvar_89;
        tmpvar_89 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_90;
        tmpvar_90 = sqrt(((tmpvar_60 * tmpvar_60) - (Rg * Rg)));
        highp float tmpvar_91;
        tmpvar_91 = (tmpvar_60 * mu1_26);
        highp float tmpvar_92;
        tmpvar_92 = (((tmpvar_91 * tmpvar_91) - (tmpvar_60 * tmpvar_60)) + (Rg * Rg));
        highp vec4 tmpvar_93;
        if (((tmpvar_91 < 0.0) && (tmpvar_92 > 0.0))) {
          highp vec4 tmpvar_94;
          tmpvar_94.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_94.w = (0.5 - (0.5 / RES_MU));
          tmpvar_93 = tmpvar_94;
        } else {
          highp vec4 tmpvar_95;
          tmpvar_95.x = -1.0;
          tmpvar_95.y = (tmpvar_89 * tmpvar_89);
          tmpvar_95.z = tmpvar_89;
          tmpvar_95.w = (0.5 + (0.5 / RES_MU));
          tmpvar_93 = tmpvar_95;
        };
        uR_88 = ((0.5 / RES_R) + ((tmpvar_90 / tmpvar_89) * (1.0 - (1.0/(RES_R)))));
        uMu_87 = (tmpvar_93.w + ((((tmpvar_91 * tmpvar_93.x) + sqrt((tmpvar_92 + tmpvar_93.y))) / (tmpvar_90 + tmpvar_93.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_96;
        y_over_x_96 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_97;
        x_97 = (y_over_x_96 * inversesqrt(((y_over_x_96 * y_over_x_96) + 1.0)));
        highp float tmpvar_98;
        tmpvar_98 = ((0.5 / RES_MU_S) + (((((sign(x_97) * (1.5708 - (sqrt((1.0 - abs(x_97))) * (1.5708 + (abs(x_97) * (-0.214602 + (abs(x_97) * (0.0865667 + (abs(x_97) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_99;
        tmpvar_99 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_100;
        tmpvar_100 = floor(tmpvar_99);
        highp float tmpvar_101;
        tmpvar_101 = (tmpvar_99 - tmpvar_100);
        highp float tmpvar_102;
        tmpvar_102 = (floor(((uR_88 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_103;
        tmpvar_103 = (floor((uR_88 * RES_R)) / RES_R);
        highp float tmpvar_104;
        tmpvar_104 = fract((uR_88 * RES_R));
        highp vec4 tmpvar_105;
        tmpvar_105.zw = vec2(0.0, 0.0);
        tmpvar_105.x = ((tmpvar_100 + tmpvar_98) / RES_NU);
        tmpvar_105.y = ((uMu_87 / RES_R) + tmpvar_102);
        lowp vec4 tmpvar_106;
        tmpvar_106 = texture2DLodEXT (_Inscatter, tmpvar_105.xy, 0.0);
        highp vec4 tmpvar_107;
        tmpvar_107.zw = vec2(0.0, 0.0);
        tmpvar_107.x = (((tmpvar_100 + tmpvar_98) + 1.0) / RES_NU);
        tmpvar_107.y = ((uMu_87 / RES_R) + tmpvar_102);
        lowp vec4 tmpvar_108;
        tmpvar_108 = texture2DLodEXT (_Inscatter, tmpvar_107.xy, 0.0);
        highp vec4 tmpvar_109;
        tmpvar_109.zw = vec2(0.0, 0.0);
        tmpvar_109.x = ((tmpvar_100 + tmpvar_98) / RES_NU);
        tmpvar_109.y = ((uMu_87 / RES_R) + tmpvar_103);
        lowp vec4 tmpvar_110;
        tmpvar_110 = texture2DLodEXT (_Inscatter, tmpvar_109.xy, 0.0);
        highp vec4 tmpvar_111;
        tmpvar_111.zw = vec2(0.0, 0.0);
        tmpvar_111.x = (((tmpvar_100 + tmpvar_98) + 1.0) / RES_NU);
        tmpvar_111.y = ((uMu_87 / RES_R) + tmpvar_103);
        lowp vec4 tmpvar_112;
        tmpvar_112 = texture2DLodEXT (_Inscatter, tmpvar_111.xy, 0.0);
        inScatterA_56 = max ((inScatter0_57 - (((((tmpvar_106 * (1.0 - tmpvar_101)) + (tmpvar_108 * tmpvar_101)) * (1.0 - tmpvar_104)) + (((tmpvar_110 * (1.0 - tmpvar_101)) + (tmpvar_112 * tmpvar_101)) * tmpvar_104)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0));
        highp float tmpvar_113;
        tmpvar_113 = (tmpvar_54 + 0.004);
        mu_12 = tmpvar_113;
        highp float tmpvar_114;
        tmpvar_114 = sqrt((((r_14 * r_14) + (d_15 * d_15)) + (((2.0 * r_14) * d_15) * tmpvar_113)));
        r1_27 = tmpvar_114;
        mu1_26 = (((r_14 * tmpvar_113) + d_15) / tmpvar_114);
        highp float uMu_115;
        highp float uR_116;
        highp float tmpvar_117;
        tmpvar_117 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_118;
        tmpvar_118 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_119;
        tmpvar_119 = (r_14 * tmpvar_113);
        highp float tmpvar_120;
        tmpvar_120 = (((tmpvar_119 * tmpvar_119) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_121;
        if (((tmpvar_119 < 0.0) && (tmpvar_120 > 0.0))) {
          highp vec4 tmpvar_122;
          tmpvar_122.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_122.w = (0.5 - (0.5 / RES_MU));
          tmpvar_121 = tmpvar_122;
        } else {
          highp vec4 tmpvar_123;
          tmpvar_123.x = -1.0;
          tmpvar_123.y = (tmpvar_117 * tmpvar_117);
          tmpvar_123.z = tmpvar_117;
          tmpvar_123.w = (0.5 + (0.5 / RES_MU));
          tmpvar_121 = tmpvar_123;
        };
        uR_116 = ((0.5 / RES_R) + ((tmpvar_118 / tmpvar_117) * (1.0 - (1.0/(RES_R)))));
        uMu_115 = (tmpvar_121.w + ((((tmpvar_119 * tmpvar_121.x) + sqrt((tmpvar_120 + tmpvar_121.y))) / (tmpvar_118 + tmpvar_121.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_124;
        y_over_x_124 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_125;
        x_125 = (y_over_x_124 * inversesqrt(((y_over_x_124 * y_over_x_124) + 1.0)));
        highp float tmpvar_126;
        tmpvar_126 = ((0.5 / RES_MU_S) + (((((sign(x_125) * (1.5708 - (sqrt((1.0 - abs(x_125))) * (1.5708 + (abs(x_125) * (-0.214602 + (abs(x_125) * (0.0865667 + (abs(x_125) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_127;
        tmpvar_127 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_128;
        tmpvar_128 = floor(tmpvar_127);
        highp float tmpvar_129;
        tmpvar_129 = (tmpvar_127 - tmpvar_128);
        highp float tmpvar_130;
        tmpvar_130 = (floor(((uR_116 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_131;
        tmpvar_131 = (floor((uR_116 * RES_R)) / RES_R);
        highp float tmpvar_132;
        tmpvar_132 = fract((uR_116 * RES_R));
        highp vec4 tmpvar_133;
        tmpvar_133.zw = vec2(0.0, 0.0);
        tmpvar_133.x = ((tmpvar_128 + tmpvar_126) / RES_NU);
        tmpvar_133.y = ((uMu_115 / RES_R) + tmpvar_130);
        lowp vec4 tmpvar_134;
        tmpvar_134 = texture2DLodEXT (_Inscatter, tmpvar_133.xy, 0.0);
        highp vec4 tmpvar_135;
        tmpvar_135.zw = vec2(0.0, 0.0);
        tmpvar_135.x = (((tmpvar_128 + tmpvar_126) + 1.0) / RES_NU);
        tmpvar_135.y = ((uMu_115 / RES_R) + tmpvar_130);
        lowp vec4 tmpvar_136;
        tmpvar_136 = texture2DLodEXT (_Inscatter, tmpvar_135.xy, 0.0);
        highp vec4 tmpvar_137;
        tmpvar_137.zw = vec2(0.0, 0.0);
        tmpvar_137.x = ((tmpvar_128 + tmpvar_126) / RES_NU);
        tmpvar_137.y = ((uMu_115 / RES_R) + tmpvar_131);
        lowp vec4 tmpvar_138;
        tmpvar_138 = texture2DLodEXT (_Inscatter, tmpvar_137.xy, 0.0);
        highp vec4 tmpvar_139;
        tmpvar_139.zw = vec2(0.0, 0.0);
        tmpvar_139.x = (((tmpvar_128 + tmpvar_126) + 1.0) / RES_NU);
        tmpvar_139.y = ((uMu_115 / RES_R) + tmpvar_131);
        lowp vec4 tmpvar_140;
        tmpvar_140 = texture2DLodEXT (_Inscatter, tmpvar_139.xy, 0.0);
        inScatter0_57 = ((((tmpvar_134 * (1.0 - tmpvar_129)) + (tmpvar_136 * tmpvar_129)) * (1.0 - tmpvar_132)) + (((tmpvar_138 * (1.0 - tmpvar_129)) + (tmpvar_140 * tmpvar_129)) * tmpvar_132));
        highp float uMu_141;
        highp float uR_142;
        highp float tmpvar_143;
        tmpvar_143 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_144;
        tmpvar_144 = sqrt(((tmpvar_114 * tmpvar_114) - (Rg * Rg)));
        highp float tmpvar_145;
        tmpvar_145 = (tmpvar_114 * mu1_26);
        highp float tmpvar_146;
        tmpvar_146 = (((tmpvar_145 * tmpvar_145) - (tmpvar_114 * tmpvar_114)) + (Rg * Rg));
        highp vec4 tmpvar_147;
        if (((tmpvar_145 < 0.0) && (tmpvar_146 > 0.0))) {
          highp vec4 tmpvar_148;
          tmpvar_148.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_148.w = (0.5 - (0.5 / RES_MU));
          tmpvar_147 = tmpvar_148;
        } else {
          highp vec4 tmpvar_149;
          tmpvar_149.x = -1.0;
          tmpvar_149.y = (tmpvar_143 * tmpvar_143);
          tmpvar_149.z = tmpvar_143;
          tmpvar_149.w = (0.5 + (0.5 / RES_MU));
          tmpvar_147 = tmpvar_149;
        };
        uR_142 = ((0.5 / RES_R) + ((tmpvar_144 / tmpvar_143) * (1.0 - (1.0/(RES_R)))));
        uMu_141 = (tmpvar_147.w + ((((tmpvar_145 * tmpvar_147.x) + sqrt((tmpvar_146 + tmpvar_147.y))) / (tmpvar_144 + tmpvar_147.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_150;
        y_over_x_150 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_151;
        x_151 = (y_over_x_150 * inversesqrt(((y_over_x_150 * y_over_x_150) + 1.0)));
        highp float tmpvar_152;
        tmpvar_152 = ((0.5 / RES_MU_S) + (((((sign(x_151) * (1.5708 - (sqrt((1.0 - abs(x_151))) * (1.5708 + (abs(x_151) * (-0.214602 + (abs(x_151) * (0.0865667 + (abs(x_151) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_153;
        tmpvar_153 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_154;
        tmpvar_154 = floor(tmpvar_153);
        highp float tmpvar_155;
        tmpvar_155 = (tmpvar_153 - tmpvar_154);
        highp float tmpvar_156;
        tmpvar_156 = (floor(((uR_142 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_157;
        tmpvar_157 = (floor((uR_142 * RES_R)) / RES_R);
        highp float tmpvar_158;
        tmpvar_158 = fract((uR_142 * RES_R));
        highp vec4 tmpvar_159;
        tmpvar_159.zw = vec2(0.0, 0.0);
        tmpvar_159.x = ((tmpvar_154 + tmpvar_152) / RES_NU);
        tmpvar_159.y = ((uMu_141 / RES_R) + tmpvar_156);
        lowp vec4 tmpvar_160;
        tmpvar_160 = texture2DLodEXT (_Inscatter, tmpvar_159.xy, 0.0);
        highp vec4 tmpvar_161;
        tmpvar_161.zw = vec2(0.0, 0.0);
        tmpvar_161.x = (((tmpvar_154 + tmpvar_152) + 1.0) / RES_NU);
        tmpvar_161.y = ((uMu_141 / RES_R) + tmpvar_156);
        lowp vec4 tmpvar_162;
        tmpvar_162 = texture2DLodEXT (_Inscatter, tmpvar_161.xy, 0.0);
        highp vec4 tmpvar_163;
        tmpvar_163.zw = vec2(0.0, 0.0);
        tmpvar_163.x = ((tmpvar_154 + tmpvar_152) / RES_NU);
        tmpvar_163.y = ((uMu_141 / RES_R) + tmpvar_157);
        lowp vec4 tmpvar_164;
        tmpvar_164 = texture2DLodEXT (_Inscatter, tmpvar_163.xy, 0.0);
        highp vec4 tmpvar_165;
        tmpvar_165.zw = vec2(0.0, 0.0);
        tmpvar_165.x = (((tmpvar_154 + tmpvar_152) + 1.0) / RES_NU);
        tmpvar_165.y = ((uMu_141 / RES_R) + tmpvar_157);
        lowp vec4 tmpvar_166;
        tmpvar_166 = texture2DLodEXT (_Inscatter, tmpvar_165.xy, 0.0);
        inScatter_28 = mix (inScatterA_56, max ((inScatter0_57 - (((((tmpvar_160 * (1.0 - tmpvar_155)) + (tmpvar_162 * tmpvar_155)) * (1.0 - tmpvar_158)) + (((tmpvar_164 * (1.0 - tmpvar_155)) + (tmpvar_166 * tmpvar_155)) * tmpvar_158)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0)), vec4(a_58));
      } else {
        highp vec4 inScatter0_1_167;
        highp float uMu_168;
        highp float uR_169;
        highp float tmpvar_170;
        tmpvar_170 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_171;
        tmpvar_171 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_172;
        tmpvar_172 = (r_14 * mu_12);
        highp float tmpvar_173;
        tmpvar_173 = (((tmpvar_172 * tmpvar_172) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_174;
        if (((tmpvar_172 < 0.0) && (tmpvar_173 > 0.0))) {
          highp vec4 tmpvar_175;
          tmpvar_175.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_175.w = (0.5 - (0.5 / RES_MU));
          tmpvar_174 = tmpvar_175;
        } else {
          highp vec4 tmpvar_176;
          tmpvar_176.x = -1.0;
          tmpvar_176.y = (tmpvar_170 * tmpvar_170);
          tmpvar_176.z = tmpvar_170;
          tmpvar_176.w = (0.5 + (0.5 / RES_MU));
          tmpvar_174 = tmpvar_176;
        };
        uR_169 = ((0.5 / RES_R) + ((tmpvar_171 / tmpvar_170) * (1.0 - (1.0/(RES_R)))));
        uMu_168 = (tmpvar_174.w + ((((tmpvar_172 * tmpvar_174.x) + sqrt((tmpvar_173 + tmpvar_174.y))) / (tmpvar_171 + tmpvar_174.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_177;
        y_over_x_177 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_178;
        x_178 = (y_over_x_177 * inversesqrt(((y_over_x_177 * y_over_x_177) + 1.0)));
        highp float tmpvar_179;
        tmpvar_179 = ((0.5 / RES_MU_S) + (((((sign(x_178) * (1.5708 - (sqrt((1.0 - abs(x_178))) * (1.5708 + (abs(x_178) * (-0.214602 + (abs(x_178) * (0.0865667 + (abs(x_178) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_180;
        tmpvar_180 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_181;
        tmpvar_181 = floor(tmpvar_180);
        highp float tmpvar_182;
        tmpvar_182 = (tmpvar_180 - tmpvar_181);
        highp float tmpvar_183;
        tmpvar_183 = (floor(((uR_169 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_184;
        tmpvar_184 = (floor((uR_169 * RES_R)) / RES_R);
        highp float tmpvar_185;
        tmpvar_185 = fract((uR_169 * RES_R));
        highp vec4 tmpvar_186;
        tmpvar_186.zw = vec2(0.0, 0.0);
        tmpvar_186.x = ((tmpvar_181 + tmpvar_179) / RES_NU);
        tmpvar_186.y = ((uMu_168 / RES_R) + tmpvar_183);
        lowp vec4 tmpvar_187;
        tmpvar_187 = texture2DLodEXT (_Inscatter, tmpvar_186.xy, 0.0);
        highp vec4 tmpvar_188;
        tmpvar_188.zw = vec2(0.0, 0.0);
        tmpvar_188.x = (((tmpvar_181 + tmpvar_179) + 1.0) / RES_NU);
        tmpvar_188.y = ((uMu_168 / RES_R) + tmpvar_183);
        lowp vec4 tmpvar_189;
        tmpvar_189 = texture2DLodEXT (_Inscatter, tmpvar_188.xy, 0.0);
        highp vec4 tmpvar_190;
        tmpvar_190.zw = vec2(0.0, 0.0);
        tmpvar_190.x = ((tmpvar_181 + tmpvar_179) / RES_NU);
        tmpvar_190.y = ((uMu_168 / RES_R) + tmpvar_184);
        lowp vec4 tmpvar_191;
        tmpvar_191 = texture2DLodEXT (_Inscatter, tmpvar_190.xy, 0.0);
        highp vec4 tmpvar_192;
        tmpvar_192.zw = vec2(0.0, 0.0);
        tmpvar_192.x = (((tmpvar_181 + tmpvar_179) + 1.0) / RES_NU);
        tmpvar_192.y = ((uMu_168 / RES_R) + tmpvar_184);
        lowp vec4 tmpvar_193;
        tmpvar_193 = texture2DLodEXT (_Inscatter, tmpvar_192.xy, 0.0);
        inScatter0_1_167 = ((((tmpvar_187 * (1.0 - tmpvar_182)) + (tmpvar_189 * tmpvar_182)) * (1.0 - tmpvar_185)) + (((tmpvar_191 * (1.0 - tmpvar_182)) + (tmpvar_193 * tmpvar_182)) * tmpvar_185));
        highp float uMu_194;
        highp float uR_195;
        highp float tmpvar_196;
        tmpvar_196 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_197;
        tmpvar_197 = sqrt(((r1_27 * r1_27) - (Rg * Rg)));
        highp float tmpvar_198;
        tmpvar_198 = (r1_27 * mu1_26);
        highp float tmpvar_199;
        tmpvar_199 = (((tmpvar_198 * tmpvar_198) - (r1_27 * r1_27)) + (Rg * Rg));
        highp vec4 tmpvar_200;
        if (((tmpvar_198 < 0.0) && (tmpvar_199 > 0.0))) {
          highp vec4 tmpvar_201;
          tmpvar_201.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_201.w = (0.5 - (0.5 / RES_MU));
          tmpvar_200 = tmpvar_201;
        } else {
          highp vec4 tmpvar_202;
          tmpvar_202.x = -1.0;
          tmpvar_202.y = (tmpvar_196 * tmpvar_196);
          tmpvar_202.z = tmpvar_196;
          tmpvar_202.w = (0.5 + (0.5 / RES_MU));
          tmpvar_200 = tmpvar_202;
        };
        uR_195 = ((0.5 / RES_R) + ((tmpvar_197 / tmpvar_196) * (1.0 - (1.0/(RES_R)))));
        uMu_194 = (tmpvar_200.w + ((((tmpvar_198 * tmpvar_200.x) + sqrt((tmpvar_199 + tmpvar_200.y))) / (tmpvar_197 + tmpvar_200.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_203;
        y_over_x_203 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_204;
        x_204 = (y_over_x_203 * inversesqrt(((y_over_x_203 * y_over_x_203) + 1.0)));
        highp float tmpvar_205;
        tmpvar_205 = ((0.5 / RES_MU_S) + (((((sign(x_204) * (1.5708 - (sqrt((1.0 - abs(x_204))) * (1.5708 + (abs(x_204) * (-0.214602 + (abs(x_204) * (0.0865667 + (abs(x_204) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_206;
        tmpvar_206 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_207;
        tmpvar_207 = floor(tmpvar_206);
        highp float tmpvar_208;
        tmpvar_208 = (tmpvar_206 - tmpvar_207);
        highp float tmpvar_209;
        tmpvar_209 = (floor(((uR_195 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_210;
        tmpvar_210 = (floor((uR_195 * RES_R)) / RES_R);
        highp float tmpvar_211;
        tmpvar_211 = fract((uR_195 * RES_R));
        highp vec4 tmpvar_212;
        tmpvar_212.zw = vec2(0.0, 0.0);
        tmpvar_212.x = ((tmpvar_207 + tmpvar_205) / RES_NU);
        tmpvar_212.y = ((uMu_194 / RES_R) + tmpvar_209);
        lowp vec4 tmpvar_213;
        tmpvar_213 = texture2DLodEXT (_Inscatter, tmpvar_212.xy, 0.0);
        highp vec4 tmpvar_214;
        tmpvar_214.zw = vec2(0.0, 0.0);
        tmpvar_214.x = (((tmpvar_207 + tmpvar_205) + 1.0) / RES_NU);
        tmpvar_214.y = ((uMu_194 / RES_R) + tmpvar_209);
        lowp vec4 tmpvar_215;
        tmpvar_215 = texture2DLodEXT (_Inscatter, tmpvar_214.xy, 0.0);
        highp vec4 tmpvar_216;
        tmpvar_216.zw = vec2(0.0, 0.0);
        tmpvar_216.x = ((tmpvar_207 + tmpvar_205) / RES_NU);
        tmpvar_216.y = ((uMu_194 / RES_R) + tmpvar_210);
        lowp vec4 tmpvar_217;
        tmpvar_217 = texture2DLodEXT (_Inscatter, tmpvar_216.xy, 0.0);
        highp vec4 tmpvar_218;
        tmpvar_218.zw = vec2(0.0, 0.0);
        tmpvar_218.x = (((tmpvar_207 + tmpvar_205) + 1.0) / RES_NU);
        tmpvar_218.y = ((uMu_194 / RES_R) + tmpvar_210);
        lowp vec4 tmpvar_219;
        tmpvar_219 = texture2DLodEXT (_Inscatter, tmpvar_218.xy, 0.0);
        inScatter_28 = max ((inScatter0_1_167 - (((((tmpvar_213 * (1.0 - tmpvar_208)) + (tmpvar_215 * tmpvar_208)) * (1.0 - tmpvar_211)) + (((tmpvar_217 * (1.0 - tmpvar_208)) + (tmpvar_219 * tmpvar_208)) * tmpvar_211)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0));
      };
      highp float t_220;
      t_220 = max (min ((tmpvar_30 / 0.02), 1.0), 0.0);
      inScatter_28.w = (inScatter_28.w * (t_220 * (t_220 * (3.0 - (2.0 * t_220)))));
      result_16 = ((inScatter_28.xyz * ((3.0 / (16.0 * M_PI)) * (1.0 + (tmpvar_29 * tmpvar_29)))) + ((((inScatter_28.xyz * inScatter_28.w) / max (inScatter_28.x, 0.0001)) * (betaR.x / betaR)) * (((((1.5 / (4.0 * M_PI)) * (1.0 - (mieG * mieG))) * pow (((1.0 + (mieG * mieG)) - ((2.0 * mieG) * tmpvar_29)), -1.5)) * (1.0 + (tmpvar_29 * tmpvar_29))) / (2.0 + (mieG * mieG)))));
    };
    col_3.xyz = ((col_3.xyz * extinction_11) + (_global_depth * (result_16 * SUN_INTENSITY)));
    visib_2 = 1.0;
    if ((tmpvar_6 <= 0.015)) {
      visib_2 = (tmpvar_6 / 0.015);
    };
    highp vec3 L_221;
    highp vec3 tmpvar_222;
    tmpvar_222 = (col_3.xyz * _Exposure);
    L_221 = tmpvar_222;
    highp float tmpvar_223;
    if ((tmpvar_222.x < 1.413)) {
      tmpvar_223 = pow ((tmpvar_222.x * 0.38317), 0.454545);
    } else {
      tmpvar_223 = (1.0 - exp(-(tmpvar_222.x)));
    };
    L_221.x = tmpvar_223;
    highp float tmpvar_224;
    if ((tmpvar_222.y < 1.413)) {
      tmpvar_224 = pow ((tmpvar_222.y * 0.38317), 0.454545);
    } else {
      tmpvar_224 = (1.0 - exp(-(tmpvar_222.y)));
    };
    L_221.y = tmpvar_224;
    highp float tmpvar_225;
    if ((tmpvar_222.z < 1.413)) {
      tmpvar_225 = pow ((tmpvar_222.z * 0.38317), 0.454545);
    } else {
      tmpvar_225 = (1.0 - exp(-(tmpvar_222.z)));
    };
    L_221.z = tmpvar_225;
    highp vec4 tmpvar_226;
    tmpvar_226.xyz = L_221;
    tmpvar_226.w = (_global_alpha * visib_2);
    tmpvar_1 = tmpvar_226;
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
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 _FrustumCorners;
out highp vec2 xlv_TEXCOORD0;
out highp vec2 xlv_TEXCOORD1;
out highp vec4 xlv_TEXCOORD2;
void main ()
{
  vec2 tmpvar_1;
  tmpvar_1 = _glesMultiTexCoord0.xy;
  highp vec4 tmpvar_2;
  tmpvar_2.xyw = _glesVertex.xyw;
  mediump float index_3;
  highp vec4 tmpvar_4;
  highp float tmpvar_5;
  tmpvar_5 = _glesVertex.z;
  index_3 = tmpvar_5;
  tmpvar_2.z = 0.1;
  int i_6;
  i_6 = int(index_3);
  mediump vec4 v_7;
  v_7.x = _FrustumCorners[0][i_6];
  v_7.y = _FrustumCorners[1][i_6];
  v_7.z = _FrustumCorners[2][i_6];
  v_7.w = _FrustumCorners[3][i_6];
  tmpvar_4.xyz = v_7.xyz;
  tmpvar_4.w = index_3;
  gl_Position = (glstate_matrix_mvp * tmpvar_2);
  xlv_TEXCOORD0 = tmpvar_1;
  xlv_TEXCOORD1 = tmpvar_1;
  xlv_TEXCOORD2 = tmpvar_4;
}



#endif
#ifdef FRAGMENT

out mediump vec4 _glesFragData[4];
uniform highp vec4 _ZBufferParams;
uniform sampler2D _Transmittance;
uniform sampler2D _Inscatter;
uniform highp float M_PI;
uniform highp float Rg;
uniform highp float Rt;
uniform highp float RES_R;
uniform highp float RES_MU;
uniform highp float RES_MU_S;
uniform highp float RES_NU;
uniform highp vec3 SUN_DIR;
uniform highp float SUN_INTENSITY;
uniform highp vec3 betaR;
uniform highp float mieG;
uniform sampler2D _MainTex;
uniform highp float _Scale;
uniform highp float _global_alpha;
uniform highp float _Exposure;
uniform highp float _global_depth;
uniform highp vec3 _inCamPos;
uniform highp vec3 _Globals_Origin;
uniform sampler2D _CameraDepthTexture;
in highp vec2 xlv_TEXCOORD0;
in highp vec2 xlv_TEXCOORD1;
in highp vec4 xlv_TEXCOORD2;
void main ()
{
  mediump vec4 tmpvar_1;
  highp float visib_2;
  highp vec4 col_3;
  lowp vec4 tmpvar_4;
  tmpvar_4 = texture (_MainTex, xlv_TEXCOORD0);
  col_3 = tmpvar_4;
  lowp vec4 tmpvar_5;
  tmpvar_5 = texture (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float tmpvar_6;
  highp float z_7;
  z_7 = tmpvar_5.x;
  tmpvar_6 = (1.0/(((_ZBufferParams.x * z_7) + _ZBufferParams.y)));
  highp vec3 tmpvar_8;
  tmpvar_8 = ((_inCamPos - _Globals_Origin) + (tmpvar_6 * xlv_TEXCOORD2).xyz);
  if ((tmpvar_6 == 1.0)) {
    tmpvar_1 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp vec3 camera_9;
    camera_9 = (_inCamPos - _Globals_Origin);
    highp vec3 _point_10;
    _point_10 = tmpvar_8;
    highp vec3 extinction_11;
    highp float mu_12;
    highp float rMu_13;
    highp float r_14;
    highp float d_15;
    highp vec3 result_16;
    result_16 = vec3(0.0, 0.0, 0.0);
    extinction_11 = vec3(1.0, 1.0, 1.0);
    highp vec3 tmpvar_17;
    tmpvar_17 = (tmpvar_8 - camera_9);
    highp float tmpvar_18;
    tmpvar_18 = (sqrt(dot (tmpvar_17, tmpvar_17)) * _Scale);
    d_15 = tmpvar_18;
    highp vec3 tmpvar_19;
    tmpvar_19 = (tmpvar_17 / tmpvar_18);
    highp float tmpvar_20;
    tmpvar_20 = (sqrt(dot (camera_9, camera_9)) * _Scale);
    r_14 = tmpvar_20;
    if (((tmpvar_20 / _Scale) < (0.9 * Rg))) {
      camera_9.y = (camera_9.y + Rg);
      _point_10.y = (tmpvar_8.y + Rg);
      r_14 = (sqrt(dot (camera_9, camera_9)) * _Scale);
    };
    highp float tmpvar_21;
    tmpvar_21 = dot (camera_9, tmpvar_19);
    rMu_13 = tmpvar_21;
    mu_12 = (tmpvar_21 / r_14);
    highp vec3 tmpvar_22;
    tmpvar_22 = (_point_10 - (tmpvar_19 * clamp (1.0, 0.0, tmpvar_18)));
    _point_10 = tmpvar_22;
    highp float tmpvar_23;
    tmpvar_23 = max ((-(tmpvar_21) - sqrt((((tmpvar_21 * tmpvar_21) - (r_14 * r_14)) + (Rt * Rt)))), 0.0);
    if (((tmpvar_23 > 0.0) && (tmpvar_23 < tmpvar_18))) {
      camera_9 = (camera_9 + (tmpvar_23 * tmpvar_19));
      highp float tmpvar_24;
      tmpvar_24 = (tmpvar_21 + tmpvar_23);
      rMu_13 = tmpvar_24;
      mu_12 = (tmpvar_24 / Rt);
      r_14 = Rt;
      d_15 = (tmpvar_18 - tmpvar_23);
    };
    if ((r_14 <= Rt)) {
      highp float muS1_25;
      highp float mu1_26;
      highp float r1_27;
      highp vec4 inScatter_28;
      highp float tmpvar_29;
      tmpvar_29 = dot (tmpvar_19, SUN_DIR);
      highp float tmpvar_30;
      tmpvar_30 = (dot (camera_9, SUN_DIR) / r_14);
      if ((r_14 < (Rg + 600.0))) {
        highp float tmpvar_31;
        tmpvar_31 = ((Rg + 600.0) / r_14);
        r_14 = (r_14 * tmpvar_31);
        rMu_13 = (rMu_13 * tmpvar_31);
        _point_10 = (tmpvar_22 * tmpvar_31);
      };
      highp float tmpvar_32;
      tmpvar_32 = sqrt(dot (_point_10, _point_10));
      r1_27 = tmpvar_32;
      highp float tmpvar_33;
      tmpvar_33 = (dot (_point_10, tmpvar_19) / tmpvar_32);
      mu1_26 = tmpvar_33;
      muS1_25 = (dot (_point_10, SUN_DIR) / tmpvar_32);
      if ((mu_12 > 0.0)) {
        highp vec3 tmpvar_34;
        highp float y_over_x_35;
        y_over_x_35 = (((mu_12 + 0.15) / 1.15) * 14.1014);
        highp float x_36;
        x_36 = (y_over_x_35 * inversesqrt(((y_over_x_35 * y_over_x_35) + 1.0)));
        highp vec4 tmpvar_37;
        tmpvar_37.zw = vec2(0.0, 0.0);
        tmpvar_37.x = ((sign(x_36) * (1.5708 - (sqrt((1.0 - abs(x_36))) * (1.5708 + (abs(x_36) * (-0.214602 + (abs(x_36) * (0.0865667 + (abs(x_36) * -0.0310296))))))))) / 1.5);
        tmpvar_37.y = sqrt(((r_14 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_38;
        tmpvar_38 = textureLod (_Transmittance, tmpvar_37.xy, 0.0);
        tmpvar_34 = tmpvar_38.xyz;
        highp vec3 tmpvar_39;
        highp float y_over_x_40;
        y_over_x_40 = (((tmpvar_33 + 0.15) / 1.15) * 14.1014);
        highp float x_41;
        x_41 = (y_over_x_40 * inversesqrt(((y_over_x_40 * y_over_x_40) + 1.0)));
        highp vec4 tmpvar_42;
        tmpvar_42.zw = vec2(0.0, 0.0);
        tmpvar_42.x = ((sign(x_41) * (1.5708 - (sqrt((1.0 - abs(x_41))) * (1.5708 + (abs(x_41) * (-0.214602 + (abs(x_41) * (0.0865667 + (abs(x_41) * -0.0310296))))))))) / 1.5);
        tmpvar_42.y = sqrt(((tmpvar_32 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_43;
        tmpvar_43 = textureLod (_Transmittance, tmpvar_42.xy, 0.0);
        tmpvar_39 = tmpvar_43.xyz;
        extinction_11 = min ((tmpvar_34 / tmpvar_39), vec3(1.0, 1.0, 1.0));
      } else {
        highp vec3 tmpvar_44;
        highp float y_over_x_45;
        y_over_x_45 = (((-(tmpvar_33) + 0.15) / 1.15) * 14.1014);
        highp float x_46;
        x_46 = (y_over_x_45 * inversesqrt(((y_over_x_45 * y_over_x_45) + 1.0)));
        highp vec4 tmpvar_47;
        tmpvar_47.zw = vec2(0.0, 0.0);
        tmpvar_47.x = ((sign(x_46) * (1.5708 - (sqrt((1.0 - abs(x_46))) * (1.5708 + (abs(x_46) * (-0.214602 + (abs(x_46) * (0.0865667 + (abs(x_46) * -0.0310296))))))))) / 1.5);
        tmpvar_47.y = sqrt(((tmpvar_32 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_48;
        tmpvar_48 = textureLod (_Transmittance, tmpvar_47.xy, 0.0);
        tmpvar_44 = tmpvar_48.xyz;
        highp vec3 tmpvar_49;
        highp float y_over_x_50;
        y_over_x_50 = (((-(mu_12) + 0.15) / 1.15) * 14.1014);
        highp float x_51;
        x_51 = (y_over_x_50 * inversesqrt(((y_over_x_50 * y_over_x_50) + 1.0)));
        highp vec4 tmpvar_52;
        tmpvar_52.zw = vec2(0.0, 0.0);
        tmpvar_52.x = ((sign(x_51) * (1.5708 - (sqrt((1.0 - abs(x_51))) * (1.5708 + (abs(x_51) * (-0.214602 + (abs(x_51) * (0.0865667 + (abs(x_51) * -0.0310296))))))))) / 1.5);
        tmpvar_52.y = sqrt(((r_14 - Rg) / (Rt - Rg)));
        lowp vec4 tmpvar_53;
        tmpvar_53 = textureLod (_Transmittance, tmpvar_52.xy, 0.0);
        tmpvar_49 = tmpvar_53.xyz;
        extinction_11 = min ((tmpvar_44 / tmpvar_49), vec3(1.0, 1.0, 1.0));
      };
      highp float tmpvar_54;
      tmpvar_54 = -(sqrt((1.0 - ((Rg / r_14) * (Rg / r_14)))));
      highp float tmpvar_55;
      tmpvar_55 = abs((mu_12 - tmpvar_54));
      if ((tmpvar_55 < 0.004)) {
        highp vec4 inScatterA_56;
        highp vec4 inScatter0_57;
        highp float a_58;
        a_58 = (((mu_12 - tmpvar_54) + 0.004) / 0.008);
        highp float tmpvar_59;
        tmpvar_59 = (tmpvar_54 - 0.004);
        mu_12 = tmpvar_59;
        highp float tmpvar_60;
        tmpvar_60 = sqrt((((r_14 * r_14) + (d_15 * d_15)) + (((2.0 * r_14) * d_15) * tmpvar_59)));
        r1_27 = tmpvar_60;
        mu1_26 = (((r_14 * tmpvar_59) + d_15) / tmpvar_60);
        highp float uMu_61;
        highp float uR_62;
        highp float tmpvar_63;
        tmpvar_63 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_64;
        tmpvar_64 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_65;
        tmpvar_65 = (r_14 * tmpvar_59);
        highp float tmpvar_66;
        tmpvar_66 = (((tmpvar_65 * tmpvar_65) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_67;
        if (((tmpvar_65 < 0.0) && (tmpvar_66 > 0.0))) {
          highp vec4 tmpvar_68;
          tmpvar_68.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_68.w = (0.5 - (0.5 / RES_MU));
          tmpvar_67 = tmpvar_68;
        } else {
          highp vec4 tmpvar_69;
          tmpvar_69.x = -1.0;
          tmpvar_69.y = (tmpvar_63 * tmpvar_63);
          tmpvar_69.z = tmpvar_63;
          tmpvar_69.w = (0.5 + (0.5 / RES_MU));
          tmpvar_67 = tmpvar_69;
        };
        uR_62 = ((0.5 / RES_R) + ((tmpvar_64 / tmpvar_63) * (1.0 - (1.0/(RES_R)))));
        uMu_61 = (tmpvar_67.w + ((((tmpvar_65 * tmpvar_67.x) + sqrt((tmpvar_66 + tmpvar_67.y))) / (tmpvar_64 + tmpvar_67.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_70;
        y_over_x_70 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_71;
        x_71 = (y_over_x_70 * inversesqrt(((y_over_x_70 * y_over_x_70) + 1.0)));
        highp float tmpvar_72;
        tmpvar_72 = ((0.5 / RES_MU_S) + (((((sign(x_71) * (1.5708 - (sqrt((1.0 - abs(x_71))) * (1.5708 + (abs(x_71) * (-0.214602 + (abs(x_71) * (0.0865667 + (abs(x_71) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_73;
        tmpvar_73 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_74;
        tmpvar_74 = floor(tmpvar_73);
        highp float tmpvar_75;
        tmpvar_75 = (tmpvar_73 - tmpvar_74);
        highp float tmpvar_76;
        tmpvar_76 = (floor(((uR_62 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_77;
        tmpvar_77 = (floor((uR_62 * RES_R)) / RES_R);
        highp float tmpvar_78;
        tmpvar_78 = fract((uR_62 * RES_R));
        highp vec4 tmpvar_79;
        tmpvar_79.zw = vec2(0.0, 0.0);
        tmpvar_79.x = ((tmpvar_74 + tmpvar_72) / RES_NU);
        tmpvar_79.y = ((uMu_61 / RES_R) + tmpvar_76);
        lowp vec4 tmpvar_80;
        tmpvar_80 = textureLod (_Inscatter, tmpvar_79.xy, 0.0);
        highp vec4 tmpvar_81;
        tmpvar_81.zw = vec2(0.0, 0.0);
        tmpvar_81.x = (((tmpvar_74 + tmpvar_72) + 1.0) / RES_NU);
        tmpvar_81.y = ((uMu_61 / RES_R) + tmpvar_76);
        lowp vec4 tmpvar_82;
        tmpvar_82 = textureLod (_Inscatter, tmpvar_81.xy, 0.0);
        highp vec4 tmpvar_83;
        tmpvar_83.zw = vec2(0.0, 0.0);
        tmpvar_83.x = ((tmpvar_74 + tmpvar_72) / RES_NU);
        tmpvar_83.y = ((uMu_61 / RES_R) + tmpvar_77);
        lowp vec4 tmpvar_84;
        tmpvar_84 = textureLod (_Inscatter, tmpvar_83.xy, 0.0);
        highp vec4 tmpvar_85;
        tmpvar_85.zw = vec2(0.0, 0.0);
        tmpvar_85.x = (((tmpvar_74 + tmpvar_72) + 1.0) / RES_NU);
        tmpvar_85.y = ((uMu_61 / RES_R) + tmpvar_77);
        lowp vec4 tmpvar_86;
        tmpvar_86 = textureLod (_Inscatter, tmpvar_85.xy, 0.0);
        inScatter0_57 = ((((tmpvar_80 * (1.0 - tmpvar_75)) + (tmpvar_82 * tmpvar_75)) * (1.0 - tmpvar_78)) + (((tmpvar_84 * (1.0 - tmpvar_75)) + (tmpvar_86 * tmpvar_75)) * tmpvar_78));
        highp float uMu_87;
        highp float uR_88;
        highp float tmpvar_89;
        tmpvar_89 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_90;
        tmpvar_90 = sqrt(((tmpvar_60 * tmpvar_60) - (Rg * Rg)));
        highp float tmpvar_91;
        tmpvar_91 = (tmpvar_60 * mu1_26);
        highp float tmpvar_92;
        tmpvar_92 = (((tmpvar_91 * tmpvar_91) - (tmpvar_60 * tmpvar_60)) + (Rg * Rg));
        highp vec4 tmpvar_93;
        if (((tmpvar_91 < 0.0) && (tmpvar_92 > 0.0))) {
          highp vec4 tmpvar_94;
          tmpvar_94.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_94.w = (0.5 - (0.5 / RES_MU));
          tmpvar_93 = tmpvar_94;
        } else {
          highp vec4 tmpvar_95;
          tmpvar_95.x = -1.0;
          tmpvar_95.y = (tmpvar_89 * tmpvar_89);
          tmpvar_95.z = tmpvar_89;
          tmpvar_95.w = (0.5 + (0.5 / RES_MU));
          tmpvar_93 = tmpvar_95;
        };
        uR_88 = ((0.5 / RES_R) + ((tmpvar_90 / tmpvar_89) * (1.0 - (1.0/(RES_R)))));
        uMu_87 = (tmpvar_93.w + ((((tmpvar_91 * tmpvar_93.x) + sqrt((tmpvar_92 + tmpvar_93.y))) / (tmpvar_90 + tmpvar_93.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_96;
        y_over_x_96 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_97;
        x_97 = (y_over_x_96 * inversesqrt(((y_over_x_96 * y_over_x_96) + 1.0)));
        highp float tmpvar_98;
        tmpvar_98 = ((0.5 / RES_MU_S) + (((((sign(x_97) * (1.5708 - (sqrt((1.0 - abs(x_97))) * (1.5708 + (abs(x_97) * (-0.214602 + (abs(x_97) * (0.0865667 + (abs(x_97) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_99;
        tmpvar_99 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_100;
        tmpvar_100 = floor(tmpvar_99);
        highp float tmpvar_101;
        tmpvar_101 = (tmpvar_99 - tmpvar_100);
        highp float tmpvar_102;
        tmpvar_102 = (floor(((uR_88 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_103;
        tmpvar_103 = (floor((uR_88 * RES_R)) / RES_R);
        highp float tmpvar_104;
        tmpvar_104 = fract((uR_88 * RES_R));
        highp vec4 tmpvar_105;
        tmpvar_105.zw = vec2(0.0, 0.0);
        tmpvar_105.x = ((tmpvar_100 + tmpvar_98) / RES_NU);
        tmpvar_105.y = ((uMu_87 / RES_R) + tmpvar_102);
        lowp vec4 tmpvar_106;
        tmpvar_106 = textureLod (_Inscatter, tmpvar_105.xy, 0.0);
        highp vec4 tmpvar_107;
        tmpvar_107.zw = vec2(0.0, 0.0);
        tmpvar_107.x = (((tmpvar_100 + tmpvar_98) + 1.0) / RES_NU);
        tmpvar_107.y = ((uMu_87 / RES_R) + tmpvar_102);
        lowp vec4 tmpvar_108;
        tmpvar_108 = textureLod (_Inscatter, tmpvar_107.xy, 0.0);
        highp vec4 tmpvar_109;
        tmpvar_109.zw = vec2(0.0, 0.0);
        tmpvar_109.x = ((tmpvar_100 + tmpvar_98) / RES_NU);
        tmpvar_109.y = ((uMu_87 / RES_R) + tmpvar_103);
        lowp vec4 tmpvar_110;
        tmpvar_110 = textureLod (_Inscatter, tmpvar_109.xy, 0.0);
        highp vec4 tmpvar_111;
        tmpvar_111.zw = vec2(0.0, 0.0);
        tmpvar_111.x = (((tmpvar_100 + tmpvar_98) + 1.0) / RES_NU);
        tmpvar_111.y = ((uMu_87 / RES_R) + tmpvar_103);
        lowp vec4 tmpvar_112;
        tmpvar_112 = textureLod (_Inscatter, tmpvar_111.xy, 0.0);
        inScatterA_56 = max ((inScatter0_57 - (((((tmpvar_106 * (1.0 - tmpvar_101)) + (tmpvar_108 * tmpvar_101)) * (1.0 - tmpvar_104)) + (((tmpvar_110 * (1.0 - tmpvar_101)) + (tmpvar_112 * tmpvar_101)) * tmpvar_104)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0));
        highp float tmpvar_113;
        tmpvar_113 = (tmpvar_54 + 0.004);
        mu_12 = tmpvar_113;
        highp float tmpvar_114;
        tmpvar_114 = sqrt((((r_14 * r_14) + (d_15 * d_15)) + (((2.0 * r_14) * d_15) * tmpvar_113)));
        r1_27 = tmpvar_114;
        mu1_26 = (((r_14 * tmpvar_113) + d_15) / tmpvar_114);
        highp float uMu_115;
        highp float uR_116;
        highp float tmpvar_117;
        tmpvar_117 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_118;
        tmpvar_118 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_119;
        tmpvar_119 = (r_14 * tmpvar_113);
        highp float tmpvar_120;
        tmpvar_120 = (((tmpvar_119 * tmpvar_119) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_121;
        if (((tmpvar_119 < 0.0) && (tmpvar_120 > 0.0))) {
          highp vec4 tmpvar_122;
          tmpvar_122.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_122.w = (0.5 - (0.5 / RES_MU));
          tmpvar_121 = tmpvar_122;
        } else {
          highp vec4 tmpvar_123;
          tmpvar_123.x = -1.0;
          tmpvar_123.y = (tmpvar_117 * tmpvar_117);
          tmpvar_123.z = tmpvar_117;
          tmpvar_123.w = (0.5 + (0.5 / RES_MU));
          tmpvar_121 = tmpvar_123;
        };
        uR_116 = ((0.5 / RES_R) + ((tmpvar_118 / tmpvar_117) * (1.0 - (1.0/(RES_R)))));
        uMu_115 = (tmpvar_121.w + ((((tmpvar_119 * tmpvar_121.x) + sqrt((tmpvar_120 + tmpvar_121.y))) / (tmpvar_118 + tmpvar_121.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_124;
        y_over_x_124 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_125;
        x_125 = (y_over_x_124 * inversesqrt(((y_over_x_124 * y_over_x_124) + 1.0)));
        highp float tmpvar_126;
        tmpvar_126 = ((0.5 / RES_MU_S) + (((((sign(x_125) * (1.5708 - (sqrt((1.0 - abs(x_125))) * (1.5708 + (abs(x_125) * (-0.214602 + (abs(x_125) * (0.0865667 + (abs(x_125) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_127;
        tmpvar_127 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_128;
        tmpvar_128 = floor(tmpvar_127);
        highp float tmpvar_129;
        tmpvar_129 = (tmpvar_127 - tmpvar_128);
        highp float tmpvar_130;
        tmpvar_130 = (floor(((uR_116 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_131;
        tmpvar_131 = (floor((uR_116 * RES_R)) / RES_R);
        highp float tmpvar_132;
        tmpvar_132 = fract((uR_116 * RES_R));
        highp vec4 tmpvar_133;
        tmpvar_133.zw = vec2(0.0, 0.0);
        tmpvar_133.x = ((tmpvar_128 + tmpvar_126) / RES_NU);
        tmpvar_133.y = ((uMu_115 / RES_R) + tmpvar_130);
        lowp vec4 tmpvar_134;
        tmpvar_134 = textureLod (_Inscatter, tmpvar_133.xy, 0.0);
        highp vec4 tmpvar_135;
        tmpvar_135.zw = vec2(0.0, 0.0);
        tmpvar_135.x = (((tmpvar_128 + tmpvar_126) + 1.0) / RES_NU);
        tmpvar_135.y = ((uMu_115 / RES_R) + tmpvar_130);
        lowp vec4 tmpvar_136;
        tmpvar_136 = textureLod (_Inscatter, tmpvar_135.xy, 0.0);
        highp vec4 tmpvar_137;
        tmpvar_137.zw = vec2(0.0, 0.0);
        tmpvar_137.x = ((tmpvar_128 + tmpvar_126) / RES_NU);
        tmpvar_137.y = ((uMu_115 / RES_R) + tmpvar_131);
        lowp vec4 tmpvar_138;
        tmpvar_138 = textureLod (_Inscatter, tmpvar_137.xy, 0.0);
        highp vec4 tmpvar_139;
        tmpvar_139.zw = vec2(0.0, 0.0);
        tmpvar_139.x = (((tmpvar_128 + tmpvar_126) + 1.0) / RES_NU);
        tmpvar_139.y = ((uMu_115 / RES_R) + tmpvar_131);
        lowp vec4 tmpvar_140;
        tmpvar_140 = textureLod (_Inscatter, tmpvar_139.xy, 0.0);
        inScatter0_57 = ((((tmpvar_134 * (1.0 - tmpvar_129)) + (tmpvar_136 * tmpvar_129)) * (1.0 - tmpvar_132)) + (((tmpvar_138 * (1.0 - tmpvar_129)) + (tmpvar_140 * tmpvar_129)) * tmpvar_132));
        highp float uMu_141;
        highp float uR_142;
        highp float tmpvar_143;
        tmpvar_143 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_144;
        tmpvar_144 = sqrt(((tmpvar_114 * tmpvar_114) - (Rg * Rg)));
        highp float tmpvar_145;
        tmpvar_145 = (tmpvar_114 * mu1_26);
        highp float tmpvar_146;
        tmpvar_146 = (((tmpvar_145 * tmpvar_145) - (tmpvar_114 * tmpvar_114)) + (Rg * Rg));
        highp vec4 tmpvar_147;
        if (((tmpvar_145 < 0.0) && (tmpvar_146 > 0.0))) {
          highp vec4 tmpvar_148;
          tmpvar_148.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_148.w = (0.5 - (0.5 / RES_MU));
          tmpvar_147 = tmpvar_148;
        } else {
          highp vec4 tmpvar_149;
          tmpvar_149.x = -1.0;
          tmpvar_149.y = (tmpvar_143 * tmpvar_143);
          tmpvar_149.z = tmpvar_143;
          tmpvar_149.w = (0.5 + (0.5 / RES_MU));
          tmpvar_147 = tmpvar_149;
        };
        uR_142 = ((0.5 / RES_R) + ((tmpvar_144 / tmpvar_143) * (1.0 - (1.0/(RES_R)))));
        uMu_141 = (tmpvar_147.w + ((((tmpvar_145 * tmpvar_147.x) + sqrt((tmpvar_146 + tmpvar_147.y))) / (tmpvar_144 + tmpvar_147.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_150;
        y_over_x_150 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_151;
        x_151 = (y_over_x_150 * inversesqrt(((y_over_x_150 * y_over_x_150) + 1.0)));
        highp float tmpvar_152;
        tmpvar_152 = ((0.5 / RES_MU_S) + (((((sign(x_151) * (1.5708 - (sqrt((1.0 - abs(x_151))) * (1.5708 + (abs(x_151) * (-0.214602 + (abs(x_151) * (0.0865667 + (abs(x_151) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_153;
        tmpvar_153 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_154;
        tmpvar_154 = floor(tmpvar_153);
        highp float tmpvar_155;
        tmpvar_155 = (tmpvar_153 - tmpvar_154);
        highp float tmpvar_156;
        tmpvar_156 = (floor(((uR_142 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_157;
        tmpvar_157 = (floor((uR_142 * RES_R)) / RES_R);
        highp float tmpvar_158;
        tmpvar_158 = fract((uR_142 * RES_R));
        highp vec4 tmpvar_159;
        tmpvar_159.zw = vec2(0.0, 0.0);
        tmpvar_159.x = ((tmpvar_154 + tmpvar_152) / RES_NU);
        tmpvar_159.y = ((uMu_141 / RES_R) + tmpvar_156);
        lowp vec4 tmpvar_160;
        tmpvar_160 = textureLod (_Inscatter, tmpvar_159.xy, 0.0);
        highp vec4 tmpvar_161;
        tmpvar_161.zw = vec2(0.0, 0.0);
        tmpvar_161.x = (((tmpvar_154 + tmpvar_152) + 1.0) / RES_NU);
        tmpvar_161.y = ((uMu_141 / RES_R) + tmpvar_156);
        lowp vec4 tmpvar_162;
        tmpvar_162 = textureLod (_Inscatter, tmpvar_161.xy, 0.0);
        highp vec4 tmpvar_163;
        tmpvar_163.zw = vec2(0.0, 0.0);
        tmpvar_163.x = ((tmpvar_154 + tmpvar_152) / RES_NU);
        tmpvar_163.y = ((uMu_141 / RES_R) + tmpvar_157);
        lowp vec4 tmpvar_164;
        tmpvar_164 = textureLod (_Inscatter, tmpvar_163.xy, 0.0);
        highp vec4 tmpvar_165;
        tmpvar_165.zw = vec2(0.0, 0.0);
        tmpvar_165.x = (((tmpvar_154 + tmpvar_152) + 1.0) / RES_NU);
        tmpvar_165.y = ((uMu_141 / RES_R) + tmpvar_157);
        lowp vec4 tmpvar_166;
        tmpvar_166 = textureLod (_Inscatter, tmpvar_165.xy, 0.0);
        inScatter_28 = mix (inScatterA_56, max ((inScatter0_57 - (((((tmpvar_160 * (1.0 - tmpvar_155)) + (tmpvar_162 * tmpvar_155)) * (1.0 - tmpvar_158)) + (((tmpvar_164 * (1.0 - tmpvar_155)) + (tmpvar_166 * tmpvar_155)) * tmpvar_158)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0)), vec4(a_58));
      } else {
        highp vec4 inScatter0_1_167;
        highp float uMu_168;
        highp float uR_169;
        highp float tmpvar_170;
        tmpvar_170 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_171;
        tmpvar_171 = sqrt(((r_14 * r_14) - (Rg * Rg)));
        highp float tmpvar_172;
        tmpvar_172 = (r_14 * mu_12);
        highp float tmpvar_173;
        tmpvar_173 = (((tmpvar_172 * tmpvar_172) - (r_14 * r_14)) + (Rg * Rg));
        highp vec4 tmpvar_174;
        if (((tmpvar_172 < 0.0) && (tmpvar_173 > 0.0))) {
          highp vec4 tmpvar_175;
          tmpvar_175.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_175.w = (0.5 - (0.5 / RES_MU));
          tmpvar_174 = tmpvar_175;
        } else {
          highp vec4 tmpvar_176;
          tmpvar_176.x = -1.0;
          tmpvar_176.y = (tmpvar_170 * tmpvar_170);
          tmpvar_176.z = tmpvar_170;
          tmpvar_176.w = (0.5 + (0.5 / RES_MU));
          tmpvar_174 = tmpvar_176;
        };
        uR_169 = ((0.5 / RES_R) + ((tmpvar_171 / tmpvar_170) * (1.0 - (1.0/(RES_R)))));
        uMu_168 = (tmpvar_174.w + ((((tmpvar_172 * tmpvar_174.x) + sqrt((tmpvar_173 + tmpvar_174.y))) / (tmpvar_171 + tmpvar_174.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_177;
        y_over_x_177 = (max (tmpvar_30, -0.1975) * 5.34962);
        highp float x_178;
        x_178 = (y_over_x_177 * inversesqrt(((y_over_x_177 * y_over_x_177) + 1.0)));
        highp float tmpvar_179;
        tmpvar_179 = ((0.5 / RES_MU_S) + (((((sign(x_178) * (1.5708 - (sqrt((1.0 - abs(x_178))) * (1.5708 + (abs(x_178) * (-0.214602 + (abs(x_178) * (0.0865667 + (abs(x_178) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_180;
        tmpvar_180 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_181;
        tmpvar_181 = floor(tmpvar_180);
        highp float tmpvar_182;
        tmpvar_182 = (tmpvar_180 - tmpvar_181);
        highp float tmpvar_183;
        tmpvar_183 = (floor(((uR_169 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_184;
        tmpvar_184 = (floor((uR_169 * RES_R)) / RES_R);
        highp float tmpvar_185;
        tmpvar_185 = fract((uR_169 * RES_R));
        highp vec4 tmpvar_186;
        tmpvar_186.zw = vec2(0.0, 0.0);
        tmpvar_186.x = ((tmpvar_181 + tmpvar_179) / RES_NU);
        tmpvar_186.y = ((uMu_168 / RES_R) + tmpvar_183);
        lowp vec4 tmpvar_187;
        tmpvar_187 = textureLod (_Inscatter, tmpvar_186.xy, 0.0);
        highp vec4 tmpvar_188;
        tmpvar_188.zw = vec2(0.0, 0.0);
        tmpvar_188.x = (((tmpvar_181 + tmpvar_179) + 1.0) / RES_NU);
        tmpvar_188.y = ((uMu_168 / RES_R) + tmpvar_183);
        lowp vec4 tmpvar_189;
        tmpvar_189 = textureLod (_Inscatter, tmpvar_188.xy, 0.0);
        highp vec4 tmpvar_190;
        tmpvar_190.zw = vec2(0.0, 0.0);
        tmpvar_190.x = ((tmpvar_181 + tmpvar_179) / RES_NU);
        tmpvar_190.y = ((uMu_168 / RES_R) + tmpvar_184);
        lowp vec4 tmpvar_191;
        tmpvar_191 = textureLod (_Inscatter, tmpvar_190.xy, 0.0);
        highp vec4 tmpvar_192;
        tmpvar_192.zw = vec2(0.0, 0.0);
        tmpvar_192.x = (((tmpvar_181 + tmpvar_179) + 1.0) / RES_NU);
        tmpvar_192.y = ((uMu_168 / RES_R) + tmpvar_184);
        lowp vec4 tmpvar_193;
        tmpvar_193 = textureLod (_Inscatter, tmpvar_192.xy, 0.0);
        inScatter0_1_167 = ((((tmpvar_187 * (1.0 - tmpvar_182)) + (tmpvar_189 * tmpvar_182)) * (1.0 - tmpvar_185)) + (((tmpvar_191 * (1.0 - tmpvar_182)) + (tmpvar_193 * tmpvar_182)) * tmpvar_185));
        highp float uMu_194;
        highp float uR_195;
        highp float tmpvar_196;
        tmpvar_196 = sqrt(((Rt * Rt) - (Rg * Rg)));
        highp float tmpvar_197;
        tmpvar_197 = sqrt(((r1_27 * r1_27) - (Rg * Rg)));
        highp float tmpvar_198;
        tmpvar_198 = (r1_27 * mu1_26);
        highp float tmpvar_199;
        tmpvar_199 = (((tmpvar_198 * tmpvar_198) - (r1_27 * r1_27)) + (Rg * Rg));
        highp vec4 tmpvar_200;
        if (((tmpvar_198 < 0.0) && (tmpvar_199 > 0.0))) {
          highp vec4 tmpvar_201;
          tmpvar_201.xyz = vec3(1.0, 0.0, 0.0);
          tmpvar_201.w = (0.5 - (0.5 / RES_MU));
          tmpvar_200 = tmpvar_201;
        } else {
          highp vec4 tmpvar_202;
          tmpvar_202.x = -1.0;
          tmpvar_202.y = (tmpvar_196 * tmpvar_196);
          tmpvar_202.z = tmpvar_196;
          tmpvar_202.w = (0.5 + (0.5 / RES_MU));
          tmpvar_200 = tmpvar_202;
        };
        uR_195 = ((0.5 / RES_R) + ((tmpvar_197 / tmpvar_196) * (1.0 - (1.0/(RES_R)))));
        uMu_194 = (tmpvar_200.w + ((((tmpvar_198 * tmpvar_200.x) + sqrt((tmpvar_199 + tmpvar_200.y))) / (tmpvar_197 + tmpvar_200.z)) * (0.5 - (1.0/(RES_MU)))));
        highp float y_over_x_203;
        y_over_x_203 = (max (muS1_25, -0.1975) * 5.34962);
        highp float x_204;
        x_204 = (y_over_x_203 * inversesqrt(((y_over_x_203 * y_over_x_203) + 1.0)));
        highp float tmpvar_205;
        tmpvar_205 = ((0.5 / RES_MU_S) + (((((sign(x_204) * (1.5708 - (sqrt((1.0 - abs(x_204))) * (1.5708 + (abs(x_204) * (-0.214602 + (abs(x_204) * (0.0865667 + (abs(x_204) * -0.0310296))))))))) / 1.1) + 0.74) * 0.5) * (1.0 - (1.0/(RES_MU_S)))));
        highp float tmpvar_206;
        tmpvar_206 = (((tmpvar_29 + 1.0) / 2.0) * (RES_NU - 1.0));
        highp float tmpvar_207;
        tmpvar_207 = floor(tmpvar_206);
        highp float tmpvar_208;
        tmpvar_208 = (tmpvar_206 - tmpvar_207);
        highp float tmpvar_209;
        tmpvar_209 = (floor(((uR_195 * RES_R) - 1.0)) / RES_R);
        highp float tmpvar_210;
        tmpvar_210 = (floor((uR_195 * RES_R)) / RES_R);
        highp float tmpvar_211;
        tmpvar_211 = fract((uR_195 * RES_R));
        highp vec4 tmpvar_212;
        tmpvar_212.zw = vec2(0.0, 0.0);
        tmpvar_212.x = ((tmpvar_207 + tmpvar_205) / RES_NU);
        tmpvar_212.y = ((uMu_194 / RES_R) + tmpvar_209);
        lowp vec4 tmpvar_213;
        tmpvar_213 = textureLod (_Inscatter, tmpvar_212.xy, 0.0);
        highp vec4 tmpvar_214;
        tmpvar_214.zw = vec2(0.0, 0.0);
        tmpvar_214.x = (((tmpvar_207 + tmpvar_205) + 1.0) / RES_NU);
        tmpvar_214.y = ((uMu_194 / RES_R) + tmpvar_209);
        lowp vec4 tmpvar_215;
        tmpvar_215 = textureLod (_Inscatter, tmpvar_214.xy, 0.0);
        highp vec4 tmpvar_216;
        tmpvar_216.zw = vec2(0.0, 0.0);
        tmpvar_216.x = ((tmpvar_207 + tmpvar_205) / RES_NU);
        tmpvar_216.y = ((uMu_194 / RES_R) + tmpvar_210);
        lowp vec4 tmpvar_217;
        tmpvar_217 = textureLod (_Inscatter, tmpvar_216.xy, 0.0);
        highp vec4 tmpvar_218;
        tmpvar_218.zw = vec2(0.0, 0.0);
        tmpvar_218.x = (((tmpvar_207 + tmpvar_205) + 1.0) / RES_NU);
        tmpvar_218.y = ((uMu_194 / RES_R) + tmpvar_210);
        lowp vec4 tmpvar_219;
        tmpvar_219 = textureLod (_Inscatter, tmpvar_218.xy, 0.0);
        inScatter_28 = max ((inScatter0_1_167 - (((((tmpvar_213 * (1.0 - tmpvar_208)) + (tmpvar_215 * tmpvar_208)) * (1.0 - tmpvar_211)) + (((tmpvar_217 * (1.0 - tmpvar_208)) + (tmpvar_219 * tmpvar_208)) * tmpvar_211)) * extinction_11.xyzx)), vec4(0.0, 0.0, 0.0, 0.0));
      };
      highp float t_220;
      t_220 = max (min ((tmpvar_30 / 0.02), 1.0), 0.0);
      inScatter_28.w = (inScatter_28.w * (t_220 * (t_220 * (3.0 - (2.0 * t_220)))));
      result_16 = ((inScatter_28.xyz * ((3.0 / (16.0 * M_PI)) * (1.0 + (tmpvar_29 * tmpvar_29)))) + ((((inScatter_28.xyz * inScatter_28.w) / max (inScatter_28.x, 0.0001)) * (betaR.x / betaR)) * (((((1.5 / (4.0 * M_PI)) * (1.0 - (mieG * mieG))) * pow (((1.0 + (mieG * mieG)) - ((2.0 * mieG) * tmpvar_29)), -1.5)) * (1.0 + (tmpvar_29 * tmpvar_29))) / (2.0 + (mieG * mieG)))));
    };
    col_3.xyz = ((col_3.xyz * extinction_11) + (_global_depth * (result_16 * SUN_INTENSITY)));
    visib_2 = 1.0;
    if ((tmpvar_6 <= 0.015)) {
      visib_2 = (tmpvar_6 / 0.015);
    };
    highp vec3 L_221;
    highp vec3 tmpvar_222;
    tmpvar_222 = (col_3.xyz * _Exposure);
    L_221 = tmpvar_222;
    highp float tmpvar_223;
    if ((tmpvar_222.x < 1.413)) {
      tmpvar_223 = pow ((tmpvar_222.x * 0.38317), 0.454545);
    } else {
      tmpvar_223 = (1.0 - exp(-(tmpvar_222.x)));
    };
    L_221.x = tmpvar_223;
    highp float tmpvar_224;
    if ((tmpvar_222.y < 1.413)) {
      tmpvar_224 = pow ((tmpvar_222.y * 0.38317), 0.454545);
    } else {
      tmpvar_224 = (1.0 - exp(-(tmpvar_222.y)));
    };
    L_221.y = tmpvar_224;
    highp float tmpvar_225;
    if ((tmpvar_222.z < 1.413)) {
      tmpvar_225 = pow ((tmpvar_222.z * 0.38317), 0.454545);
    } else {
      tmpvar_225 = (1.0 - exp(-(tmpvar_222.z)));
    };
    L_221.z = tmpvar_225;
    highp vec4 tmpvar_226;
    tmpvar_226.xyz = L_221;
    tmpvar_226.w = (_global_alpha * visib_2);
    tmpvar_1 = tmpvar_226;
  };
  _glesFragData[0] = tmpvar_1;
}



#endif"
}
}
Program "fp" {
// Platform opengl had shader errors
//   <no keywords>
SubProgram "d3d9 " {
// Stats: 726 math, 58 textures, 6 branches
Vector 0 [_ZBufferParams]
Float 1 [M_PI]
Float 2 [Rg]
Float 3 [Rt]
Float 4 [RES_R]
Float 5 [RES_MU]
Float 6 [RES_MU_S]
Float 7 [RES_NU]
Vector 8 [SUN_DIR]
Float 9 [SUN_INTENSITY]
Vector 10 [betaR]
Float 11 [mieG]
Float 12 [_Scale]
Float 13 [_global_alpha]
Float 14 [_Exposure]
Float 15 [_global_depth]
Vector 16 [_inCamPos]
Vector 17 [_Globals_Origin]
SetTexture 0 [_MainTex] 2D 0
SetTexture 1 [_CameraDepthTexture] 2D 1
SetTexture 2 [_Transmittance] 2D 2
SetTexture 3 [_Inscatter] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c18, -1.00000000, 0.00000000, 1.00000000, 0.89999998
def c19, 600.00000000, 0.15000001, 12.26193905, -0.12123910
def c20, -0.01348047, 0.05747731, 0.19563590, -0.33299461
def c21, 0.99999559, 1.57079601, 0.66666669, 0.00400000
def c22, -0.19750001, 5.34960032, 0.90909088, 0.74000001
def c23, 0.50000000, 2.00000000, -0.00400000, 124.99999237
def c24, 16.00000000, 4.00000000, -1.50000000, 50.00000000
def c25, 2.00000000, 3.00000000, 0.00010000, 1.50000000
def c26, 66.66667175, -0.01500000, 0.38317001, 0.45454544
def c27, 2.71828198, -1.41299999, 0, 0
dcl_texcoord0 v0.xy
dcl_texcoord1 v1.xy
dcl_texcoord2 v2.xyz
texld r0.x, v1, s1
mad r0.x, r0, c0, c0.y
rcp r7.w, r0.x
add r0.w, r7, c18.x
abs r0.w, r0
cmp_pp oC0, -r0.w, c18.y, r1
mov r0.xyz, c16
add r0.xyz, -c17, r0
mad r1.xyz, r7.w, v2, r0
texld r0.xyz, v0, s0
cmp_pp r0.w, -r0, c18.y, c18.z
mov r7.xyz, r0
if_gt r0.w, c18.y
mov r0.xyz, c16
add r2.xyz, -c17, r0
add r0.xyz, -r2, r1
dp3 r0.w, r0, r0
rsq r3.x, r0.w
dp3 r0.w, r2, r2
rsq r0.w, r0.w
add r2.w, r2.y, c2.x
rcp r0.w, r0.w
mov r1.w, c2.x
mad r1.w, c18, -r1, r0
cmp r2.y, r1.w, r2, r2.w
rcp r2.w, r3.x
dp3 r3.y, r2, r2
rsq r3.x, r3.y
rcp r3.y, r3.x
mul r2.w, r2, c12.x
rcp r3.x, r2.w
mul r0.xyz, r0, r3.x
mul r0.w, r0, c12.x
mul r3.y, r3, c12.x
cmp r3.y, r1.w, r0.w, r3
dp3 r0.w, r2, r0
mul r3.x, r3.y, r3.y
mad r3.x, r0.w, r0.w, -r3
mad r3.x, c3, c3, r3
rsq r3.x, r3.x
rcp r3.x, r3.x
add r3.x, -r0.w, -r3
max r3.x, r3, c18.y
add r3.z, r3.x, -r2.w
cmp r3.w, r3.z, c18.y, c18.z
cmp r3.z, -r3.x, c18.y, c18
mul_pp r3.z, r3, r3.w
mad r4.xyz, r3.x, r0, r2
add r3.w, -r3.x, r2
cmp r11.y, -r3.z, r2.w, r3.w
cmp r2.xyz, -r3.z, r2, r4
add r3.x, r3, r0.w
rcp r4.x, r3.y
rcp r3.w, c3.x
mul r3.x, r3, r3.w
mul r0.w, r0, r4.x
cmp r8.w, -r3.z, r0, r3.x
cmp r11.z, -r3, r3.y, c3.x
mov_sat r0.w, r2
add r2.w, r1.y, c2.x
cmp r1.y, r1.w, r1, r2.w
mad r1.xyz, -r0, r0.w, r1
mov r3.xyz, c18.y
mov r8.xyz, c18.z
if_le r11.z, c3.x
mov r0.w, c2.x
add r2.w, c19.x, r0
rcp r1.w, r11.z
mul r0.w, r1, r2
add r3.x, r11.z, -r2.w
mul r4.xyz, r1, r0.w
cmp r1.xyz, r3.x, r1, r4
dp3 r0.w, r1, r1
rsq r0.w, r0.w
dp3 r3.y, r1, c8
dp3 r1.x, r0, r1
dp3 r9.w, r0, c8
mul r5.z, r0.w, r1.x
mul r5.w, r0, r3.y
rcp r9.y, r0.w
dp3 r0.w, r2, c8
mul r10.w, r0, r1
cmp r11.z, r3.x, r11, r2.w
if_gt r8.w, c18.y
add r0.x, r5.z, c19.y
mul r1.x, r0, c19.z
abs r1.y, r1.x
max r0.x, r1.y, c18.z
rcp r0.y, r0.x
min r0.x, r1.y, c18.z
mul r0.z, r0.x, r0.y
mul r0.y, r0.z, r0.z
mad r0.w, r0.y, c20.x, c20.y
mad r1.z, r0.w, r0.y, c19.w
add r0.x, r8.w, c19.y
mul r0.w, r0.x, c19.z
mad r0.x, r1.z, r0.y, c20.z
mad r1.z, r0.x, r0.y, c20.w
abs r1.w, r0
mad r1.z, r1, r0.y, c21.x
mul r1.z, r1, r0
max r0.x, r1.w, c18.z
rcp r0.y, r0.x
min r0.x, r1.w, c18.z
mul r0.y, r0.x, r0
mul r0.z, r0.y, r0.y
mad r0.x, r0.z, c20, c20.y
mad r0.x, r0, r0.z, c19.w
add r2.x, -r1.z, c21.y
add r1.y, r1, c18.x
cmp r1.y, -r1, r1.z, r2.x
cmp r1.y, r1.x, r1, -r1
mad r1.x, r0, r0.z, c20.z
mad r1.x, r1, r0.z, c20.w
mul r0.x, r1.y, c21.z
mad r0.z, r1.x, r0, c21.x
mul r2.x, r0.z, r0.y
mov r1.y, c3.x
add r1.x, -c2, r1.y
rcp r2.y, r1.x
add r1.y, r9, -c2.x
mul r1.x, r2.y, r1.y
rsq r0.y, r1.x
mov r0.z, c18.y
rcp r0.y, r0.y
texldl r1.xyz, r0.xyzz, s2
add r0.y, -r2.x, c21
add r0.x, r1.w, c18
cmp r0.x, -r0, r2, r0.y
add r0.y, r11.z, -c2.x
cmp r0.x, r0.w, r0, -r0
mul r0.y, r0, r2
rsq r0.y, r0.y
mul r0.x, r0, c21.z
mov r0.z, c18.y
rcp r0.y, r0.y
texldl r0.xyz, r0.xyzz, s2
rcp r1.x, r1.x
rcp r1.z, r1.z
rcp r1.y, r1.y
mul r0.xyz, r0, r1
min r8.xyz, r0, c18.z
else
add r0.x, -r8.w, c19.y
mul r1.x, r0, c19.z
abs r1.y, r1.x
max r0.x, r1.y, c18.z
rcp r0.y, r0.x
min r0.x, r1.y, c18.z
mul r0.z, r0.x, r0.y
mul r0.y, r0.z, r0.z
mad r0.w, r0.y, c20.x, c20.y
mad r1.z, r0.w, r0.y, c19.w
add r0.x, -r5.z, c19.y
mul r0.w, r0.x, c19.z
mad r0.x, r1.z, r0.y, c20.z
mad r1.z, r0.x, r0.y, c20.w
abs r1.w, r0
mad r1.z, r1, r0.y, c21.x
mul r1.z, r1, r0
max r0.x, r1.w, c18.z
rcp r0.y, r0.x
min r0.x, r1.w, c18.z
mul r0.y, r0.x, r0
mul r0.z, r0.y, r0.y
mad r0.x, r0.z, c20, c20.y
mad r0.x, r0, r0.z, c19.w
add r2.x, -r1.z, c21.y
add r1.y, r1, c18.x
cmp r1.y, -r1, r1.z, r2.x
cmp r1.y, r1.x, r1, -r1
mad r1.x, r0, r0.z, c20.z
mad r1.x, r1, r0.z, c20.w
mul r0.x, r1.y, c21.z
mad r0.z, r1.x, r0, c21.x
mul r2.x, r0.z, r0.y
mov r1.y, c3.x
add r1.x, -c2, r1.y
rcp r2.y, r1.x
add r1.y, r11.z, -c2.x
mul r1.x, r2.y, r1.y
rsq r0.y, r1.x
mov r0.z, c18.y
rcp r0.y, r0.y
texldl r1.xyz, r0.xyzz, s2
add r0.y, -r2.x, c21
add r0.x, r1.w, c18
cmp r0.x, -r0, r2, r0.y
add r0.y, r9, -c2.x
cmp r0.x, r0.w, r0, -r0
mul r0.y, r0, r2
rsq r0.y, r0.y
mul r0.x, r0, c21.z
mov r0.z, c18.y
rcp r0.y, r0.y
texldl r0.xyz, r0.xyzz, s2
rcp r1.x, r1.x
rcp r1.z, r1.z
rcp r1.y, r1.y
mul r0.xyz, r0, r1
min r8.xyz, r0, c18.z
endif
rcp r0.x, r11.z
mul r0.x, r0, c2
mad r0.x, -r0, r0, c18.z
rsq r0.x, r0.x
rcp r0.x, r0.x
add r0.y, r0.x, r8.w
abs r0.y, r0
mov r11.w, -r0.x
if_lt r0.y, c21.w
rcp r1.w, c5.x
rcp r12.y, c4.x
mul r12.z, c2.x, c2.x
add r9.y, r11.w, c21.w
rcp r6.y, c6.x
mul r0.x, r11.y, r11.y
mad r0.z, c3.x, c3.x, -r12
add r14.w, r11, c23.z
mov r2.xyz, c18.zyyw
mad r2.w, -r1, c23.x, c23.x
mad r3.w, r1, c23.x, c23.x
mad r0.y, r11.z, r9, r11
rsq r13.y, r0.z
mov r3.x, c18
add r13.z, -r1.w, c23.x
mul r10.z, r11, r14.w
mad r14.x, r11.z, r11.z, r0
mul r14.y, r11.z, r11
mul r0.x, r14.y, r9.y
mad r0.x, r0, c23.y, r14
rsq r0.x, r0.x
rcp r1.x, r0.x
mul r0.x, r0, r0.y
mul r1.y, r1.x, r0.x
mul r0.y, r1.x, r1.x
mad r0.y, r1, r1, -r0
mad r1.z, c2.x, c2.x, r0.y
mad r1.x, r1, r1, -r12.z
rsq r1.x, r1.x
add r13.w, -r12.y, c18.z
add r6.z, -r6.y, c18
mul r6.x, r11.z, r11.z
cmp r0.y, -r1.z, c18, c18.z
cmp r0.x, r1.y, c18.y, c18.z
mul_pp r0.x, r0, r0.y
rcp r0.y, r13.y
mul r3.y, r0, r0
mov r3.z, r0.y
cmp r0, -r0.x, r3, r2
add r0.y, r0, r1.z
rcp r1.x, r1.x
rsq r0.y, r0.y
rcp r0.y, r0.y
add r0.z, r0, r1.x
mad r0.x, r1.y, r0, r0.y
rcp r0.y, r0.z
mul r0.x, r0, r0.y
max r0.y, r5.w, c22.x
mad r0.x, r0, r13.z, r0.w
mul r0.z, r13.y, r1.x
mul r0.w, r0.z, r13
mad r1.x, r12.y, c23, r0.w
mul r1.z, r1.x, c4.x
frc r9.x, r1.z
add r1.w, r1.z, -r9.x
mul r1.w, r12.y, r1
mul r0.y, r0, c22
abs r0.z, r0.y
max r0.w, r0.z, c18.z
rcp r1.x, r0.w
min r0.w, r0.z, c18.z
mul r0.w, r0, r1.x
mul r1.x, r0.w, r0.w
mad r1.y, r1.x, c20.x, c20
mad r1.y, r1, r1.x, c19.w
mad r1.y, r1, r1.x, c20.z
mad r1.y, r1, r1.x, c20.w
mad r1.x, r1.y, r1, c21
mul r0.w, r1.x, r0
mad r5.y, r0.x, r12, r1.w
add r1.z, r1, c18.x
frc r1.w, r1.z
add r1.y, r1.z, -r1.w
rcp r5.w, c7.x
mul r1.y, r1, r12
add r1.x, -r0.w, c21.y
add r0.z, r0, c18.x
cmp r0.z, -r0, r0.w, r1.x
cmp r0.y, r0, r0.z, -r0.z
mov r0.w, c7.x
mad r0.y, r0, c22.z, c22.w
mad r0.y, r0, r6.z, r6
add r0.w, c18.x, r0
add r0.z, r9.w, c18
mul r0.z, r0, r0.w
mul r0.z, r0, c23.x
frc r12.w, r0.z
add r6.w, r0.z, -r12
mad r0.w, r0.y, c23.x, r6
mad r0.y, r0.x, r12, r1
add r0.x, r0.w, c18.z
mul r11.x, r5.w, r0
add r13.x, -r12.w, c18.z
mul r12.x, r0.w, r5.w
mov r0.z, c18.y
mov r0.x, r11
texldl r1, r0.xyzz, s3
mul r1, r12.w, r1
mov r5.z, c18.y
mov r5.x, r11
mov r0.z, c18.y
mov r0.x, r12
texldl r0, r0.xyzz, s3
mad r4, r0, r13.x, r1
texldl r1, r5.xyzz, s3
mul r5.x, r11.z, r9.y
mov r0.y, r5
mad r5.y, r5.x, r5.x, -r6.x
mul r1, r12.w, r1
mad r5.y, c2.x, c2.x, r5
mov r0.z, c18.y
mov r0.x, r12
texldl r0, r0.xyzz, s3
mad r0, r13.x, r0, r1
cmp r1.y, -r5, c18, c18.z
cmp r1.x, r5, c18.y, c18.z
mul_pp r5.z, r1.x, r1.y
mul r1, r9.x, r0
cmp r0, -r5.z, r3, r2
add r5.z, -r9.x, c18
mad r4, r4, r5.z, r1
add r1.x, r0.y, r5.y
max r0.y, r10.w, c22.x
mul r0.y, r0, c22
rsq r1.x, r1.x
rcp r1.x, r1.x
abs r1.y, r0
mad r1.w, r5.x, r0.x, r1.x
max r0.x, r1.y, c18.z
rcp r1.x, r0.x
min r0.x, r1.y, c18.z
mul r0.x, r0, r1
mad r1.z, r11, r11, -r12
rsq r1.x, r1.z
rcp r10.y, r1.x
mul r1.z, r0.x, r0.x
add r5.x, r0.z, r10.y
mad r1.x, r1.z, c20, c20.y
mad r0.z, r1.x, r1, c19.w
mad r0.z, r0, r1, c20
mad r0.z, r0, r1, c20.w
mad r0.z, r0, r1, c21.x
rcp r1.x, r5.x
mul r1.x, r1.w, r1
mad r1.x, r13.z, r1, r0.w
mul r0.z, r0, r0.x
mul r0.w, r13.y, r10.y
mul r0.x, r13.w, r0.w
mad r1.z, r12.y, c23.x, r0.x
add r0.w, -r0.z, c21.y
add r0.x, r1.y, c18
cmp r0.x, -r0, r0.z, r0.w
mul r0.z, r1, c4.x
cmp r0.x, r0.y, r0, -r0
add r0.y, r0.z, c18.x
frc r0.w, r0.y
add r0.y, r0, -r0.w
mul r9.z, r12.y, r0.y
mad r0.x, r0, c22.z, c22.w
mad r0.x, r6.z, r0, r6.y
mad r6.z, r0.x, c23.x, r6.w
add r0.x, r6.z, c18.z
mul r9.x, r5.w, r0
mul r10.x, r5.w, r6.z
frc r14.z, r0
add r0.x, r0.z, -r14.z
mul r9.y, r12, r0.x
mad r5.y, r12, r1.x, r9.z
mad r6.y, r12, r1.x, r9
mov r5.x, r9
mov r5.z, c18.y
texldl r0, r5.xyzz, s3
mul r1, r12.w, r0
mov r0.y, r5
mov r6.z, c18.y
mov r0.z, c18.y
mov r0.x, r10
texldl r0, r0.xyzz, s3
mad r5, r13.x, r0, r1
mad r1.x, r10.z, r10.z, -r6
mad r15.x, c2, c2, r1
cmp r1.x, r10.z, c18.y, c18.z
cmp r1.y, -r15.x, c18, c18.z
mov r0.y, r6
mov r6.x, r9
mul_pp r6.w, r1.x, r1.y
texldl r1, r6.xyzz, s3
cmp r6, -r6.w, r3, r2
mul r1, r12.w, r1
add r6.y, r6, r15.x
mov r0.x, r10
mov r0.z, c18.y
texldl r0, r0.xyzz, s3
mad r0, r13.x, r0, r1
add r1.y, r10, r6.z
rsq r1.x, r6.y
rcp r1.x, r1.x
mad r1.x, r10.z, r6, r1
rcp r1.y, r1.y
mul r1.x, r1, r1.y
mad r6.y, r13.z, r1.x, r6.w
mad r9.y, r12, r6, r9
mad r6.w, r11.z, r14, r11.y
mul r0, r14.z, r0
add r6.x, -r14.z, c18.z
mad r1, r5, r6.x, r0
mad r5.y, r12, r6, r9.z
mul r6.y, r14, r14.w
mad r6.y, r6, c23, r14.x
rsq r6.z, r6.y
rcp r6.y, r6.z
mul r6.z, r6, r6.w
mul r6.z, r6.y, r6
mul r6.w, r6.y, r6.y
mad r6.w, r6.z, r6.z, -r6
mov r5.z, c18.y
mov r5.x, r9
texldl r0, r5.xyzz, s3
mad r4, -r4, r8.xyzx, r1
mul r1, r12.w, r0
mov r0.y, r5
mad r6.w, c2.x, c2.x, r6
mov r0.z, c18.y
mov r0.x, r10
texldl r0, r0.xyzz, s3
mad r5, r13.x, r0, r1
mov r9.z, c18.y
texldl r1, r9.xyzz, s3
mul r1, r12.w, r1
mov r10.z, c18.y
mov r10.y, r9
texldl r0, r10.xyzz, s3
mad r0, r13.x, r0, r1
mul r0, r14.z, r0
mad r0, r6.x, r5, r0
cmp r1.y, -r6.w, c18, c18.z
cmp r1.x, r6.z, c18.y, c18.z
mul_pp r1.x, r1, r1.y
cmp r1, -r1.x, r3, r2
mad r2.x, r6.y, r6.y, -r12.z
add r1.y, r1, r6.w
rsq r2.x, r2.x
rcp r2.x, r2.x
mul r2.y, r13, r2.x
rsq r1.y, r1.y
rcp r1.y, r1.y
mad r1.x, r6.z, r1, r1.y
add r1.y, r1.z, r2.x
mul r2.y, r13.w, r2
mad r1.z, r12.y, c23.x, r2.y
mul r1.z, r1, c4.x
rcp r1.y, r1.y
mul r1.x, r1, r1.y
mad r1.x, r13.z, r1, r1.w
frc r5.w, r1.z
add r1.w, r1.z, -r5
add r1.y, r1.z, c18.x
mul r1.w, r12.y, r1
frc r1.z, r1.y
add r1.y, r1, -r1.z
mul r1.y, r12, r1
mad r11.y, r12, r1.x, r1.w
mad r5.y, r12, r1.x, r1
mov r11.z, c18.y
texldl r2, r11.xyzz, s3
mov r5.z, c18.y
mov r5.x, r11
texldl r1, r5.xyzz, s3
mul r2, r12.w, r2
mov r12.z, c18.y
mov r12.y, r11
texldl r3, r12.xyzz, s3
mad r2, r13.x, r3, r2
mul r3, r12.w, r1
mul r2, r5.w, r2
mov r1.y, r5
mov r1.z, c18.y
mov r1.x, r12
texldl r1, r1.xyzz, s3
mad r1, r13.x, r1, r3
add r3.x, -r5.w, c18.z
mad r1, r1, r3.x, r2
mad r0, r8.xyzx, -r1, r0
max r1, r4, c18.y
max r0, r0, c18.y
add r3, r1, -r0
add r2.x, r8.w, -r11.w
add r1.x, r2, c21.w
mul r1, r1.x, r3
mad r1, r1, c23.w, r0
else
rcp r3.x, c5.x
rcp r10.y, c4.x
mul r3.y, r11.z, r8.w
rcp r11.x, c6.x
mul r0.x, r11.z, r11.z
mad r0.x, r3.y, r3.y, -r0
mad r3.z, c2.x, c2.x, r0.x
mul r9.z, c2.x, c2.x
mad r0.x, c3, c3, -r9.z
rsq r10.x, r0.x
rcp r0.w, r10.x
mul r1.y, r0.w, r0.w
mov r1.z, r0.w
mad r1.w, r3.x, c23.x, c23.x
rcp r9.x, c7.x
cmp r0.y, r3, c18, c18.z
cmp r0.z, -r3, c18.y, c18
mul_pp r2.x, r0.y, r0.z
mov r1.x, c18
mad r0.w, -r3.x, c23.x, c23.x
mov r0.xyz, c18.zyyw
cmp r2, -r2.x, r1, r0
add r2.y, r2, r3.z
mad r3.z, r11, r11, -r9
rsq r2.y, r2.y
rcp r2.y, r2.y
mad r3.y, r3, r2.x, r2
rsq r3.z, r3.z
rcp r3.z, r3.z
add r2.y, r2.z, r3.z
max r2.x, r10.w, c22
mul r2.z, r2.x, c22.y
rcp r2.x, r2.y
abs r2.y, r2.z
mul r3.y, r3, r2.x
add r10.z, -r3.x, c23.x
mad r5.y, r10.z, r3, r2.w
max r2.x, r2.y, c18.z
rcp r2.w, r2.x
min r2.x, r2.y, c18.z
mul r2.x, r2, r2.w
mul r2.w, r10.x, r3.z
add r11.w, -r10.y, c18.z
mul r3.x, r11.w, r2.w
mad r3.y, r10, c23.x, r3.x
mul r4.x, r3.y, c4
mul r2.w, r2.x, r2.x
add r3.y, r4.x, c18.x
frc r12.x, r4
frc r3.z, r3.y
add r3.y, r3, -r3.z
mad r3.x, r2.w, c20, c20.y
mad r3.x, r3, r2.w, c19.w
mad r3.x, r3, r2.w, c20.z
mad r3.x, r3, r2.w, c20.w
mad r2.w, r3.x, r2, c21.x
mul r2.w, r2, r2.x
mul r3.x, r10.y, r3.y
mad r3.y, r10, r5, r3.x
add r4.x, r4, -r12
mul r6.y, r10, r4.x
mad r5.y, r10, r5, r6
add r3.x, -r2.w, c21.y
add r2.y, r2, c18.x
cmp r2.w, -r2.y, r2, r3.x
mov r2.x, c7
add r2.y, c18.x, r2.x
add r2.x, r9.w, c18.z
mul r2.x, r2, r2.y
cmp r2.y, r2.z, r2.w, -r2.w
mul r2.x, r2, c23
frc r8.w, r2.x
add r6.w, -r8, c18.z
add r11.z, r2.x, -r8.w
mov r6.y, r5
mad r2.y, r2, c22.z, c22.w
add r11.y, -r11.x, c18.z
mad r2.x, r11.y, r2.y, r11
mad r2.x, r2, c23, r11.z
add r2.z, r2.x, c18
mul r5.x, r9, r2.z
mul r6.x, r9, r2
mov r2.y, r3
mov r2.x, r6
mov r2.z, c18.y
texldl r2, r2.xyzz, s3
mov r3.x, r5
mov r3.z, c18.y
texldl r3, r3.xyzz, s3
mul r3, r8.w, r3
mad r4, r6.w, r2, r3
mov r6.z, c18.y
texldl r2, r6.xyzz, s3
mul r6.x, r9.y, r5.z
mul r3.x, r9.y, r9.y
mov r5.z, c18.y
mad r6.y, r6.x, r6.x, -r3.x
texldl r3, r5.xyzz, s3
mul r3, r8.w, r3
mad r5.y, c2.x, c2.x, r6
mad r2, r6.w, r2, r3
cmp r5.z, -r5.y, c18.y, c18
cmp r5.x, r6, c18.y, c18.z
mul_pp r5.x, r5, r5.z
cmp r0, -r5.x, r1, r0
mul r1, r12.x, r2
add r2.x, -r12, c18.z
mad r1, r4, r2.x, r1
add r0.y, r0, r5
rsq r2.x, r0.y
max r0.y, r5.w, c22.x
rcp r2.x, r2.x
mad r2.y, r9, r9, -r9.z
rsq r2.w, r2.y
rcp r2.w, r2.w
add r3.x, r0.z, r2.w
mul r0.y, r0, c22
mad r2.z, r6.x, r0.x, r2.x
abs r0.x, r0.y
max r2.x, r0, c18.z
rcp r2.y, r2.x
min r2.x, r0, c18.z
mul r2.x, r2, r2.y
mul r2.y, r2.x, r2.x
mad r0.z, r2.y, c20.x, c20.y
mad r0.z, r0, r2.y, c19.w
mad r0.z, r0, r2.y, c20
mad r0.z, r0, r2.y, c20.w
mad r0.z, r0, r2.y, c21.x
rcp r3.x, r3.x
mul r2.z, r2, r3.x
mad r3.x, r2.z, r10.z, r0.w
mul r0.w, r10.x, r2
mul r0.z, r0, r2.x
mul r0.w, r0, r11
mad r2.x, r10.y, c23, r0.w
mul r2.x, r2, c4
frc r4.w, r2.x
add r0.w, -r0.z, c21.y
add r0.x, r0, c18
cmp r0.x, -r0, r0.z, r0.w
cmp r0.x, r0.y, r0, -r0
add r0.y, r2.x, -r4.w
mul r0.y, r10, r0
mad r0.x, r0, c22.z, c22.w
mad r0.x, r0, r11.y, r11
mad r5.w, r0.x, c23.x, r11.z
add r0.z, r5.w, c18
mul r4.x, r9, r0.z
mad r4.y, r3.x, r10, r0
add r0.x, r2, c18
frc r0.y, r0.x
add r0.x, r0, -r0.y
mov r4.z, c18.y
texldl r2, r4.xyzz, s3
mul r0.x, r0, r10.y
mad r5.y, r3.x, r10, r0.x
mov r5.x, r4
mov r5.z, c18.y
texldl r0, r5.xyzz, s3
mul r3, r8.w, r2
mul r4.x, r5.w, r9
mov r4.z, c18.y
texldl r2, r4.xyzz, s3
mad r3, r6.w, r2, r3
mul r2, r8.w, r0
mov r0.y, r5
mov r0.z, c18.y
mov r0.x, r4
texldl r0, r0.xyzz, s3
mad r0, r0, r6.w, r2
mul r2, r4.w, r3
add r3.x, -r4.w, c18.z
mad r0, r0, r3.x, r2
mad r0, -r0, r8.xyzx, r1
max r1, r0, c18.y
endif
mul_sat r0.x, r10.w, c24.w
mad r0.y, -r0.x, c25.x, c25
mul r0.x, r0, r0
mul r0.x, r0, r0.y
mul r0.y, r1.w, r0.x
mul r2.xyz, r1, r0.y
mul r0.x, r9.w, c11
mul r0.x, r0, c23.y
mad r0.x, c11, c11, -r0
max r0.y, r1.x, c25.z
rcp r0.y, r0.y
mul r2.xyz, r2, r0.y
add r1.w, r0.x, c18.z
pow r0, r1.w, c24.z
mov r0.w, r0.x
mul r0.z, -c11.x, c11.x
add r0.y, r0.z, c18.z
mov r0.x, c1
mul r0.x, c24.y, r0
rcp r0.x, r0.x
mul r0.x, r0, r0.y
add r0.z, -r0, c23.y
mul r0.x, r0, r0.w
mad r0.y, r9.w, r9.w, c18.z
mul r0.w, r0.y, r0.x
mov r0.x, c1
rcp r0.z, r0.z
mul r0.x, c24, r0
rcp r0.x, r0.x
mul r0.z, r0.w, r0
rcp r3.x, c10.x
rcp r3.z, c10.z
rcp r3.y, c10.y
mul r3.xyz, r3, c10.x
mul r2.xyz, r2, r3
mul r2.xyz, r0.z, r2
mul r0.x, r0, r0.y
mul r2.xyz, r2, c25.w
mul r0.xyz, r0.x, r1
mad r3.xyz, r0, c25.y, r2
endif
mul r0.xyz, r3, c9.x
mul r0.xyz, r0, c15.x
mad r0.xyz, r7, r8, r0
mul r2.xyz, r0, c14.x
pow r1, c27.x, -r2.x
mul r3.x, r2, c26.z
pow r0, r3.x, c26.w
add r2.w, r2.x, c27.y
mov r0.y, r1.x
mov r0.z, r0.x
add r0.x, -r0.y, c18.z
cmp oC0.x, r2.w, r0, r0.z
pow r1, c27.x, -r2.y
mul r2.w, r2.y, c26.z
pow r0, r2.w, c26.w
mov r0.y, r1.x
mov r0.z, r0.x
add r2.x, r2.y, c27.y
add r0.x, -r0.y, c18.z
cmp oC0.y, r2.x, r0.x, r0.z
pow r0, c27.x, -r2.z
mul r0.y, r2.z, c26.z
pow r1, r0.y, c26.w
add r2.x, r2.z, c27.y
add r0.x, -r0, c18.z
mov r0.y, r1.x
mul r0.w, r7, c26.x
add r0.z, r7.w, c26.y
cmp r0.z, -r0, r0.w, c18
cmp oC0.z, r2.x, r0.x, r0.y
mul oC0.w, r0.z, c13.x
endif
"
}
SubProgram "d3d11 " {
// Stats: 506 math, 2 textures, 8 branches
SetTexture 0 [_MainTex] 2D 2
SetTexture 1 [_CameraDepthTexture] 2D 3
SetTexture 2 [_Transmittance] 2D 0
SetTexture 3 [_Inscatter] 2D 1
ConstBuffer "$Globals" 304
Float 16 [M_PI]
Float 36 [Rg]
Float 40 [Rt]
Float 48 [RES_R]
Float 52 [RES_MU]
Float 56 [RES_MU_S]
Float 60 [RES_NU]
Vector 64 [SUN_DIR] 3
Float 76 [SUN_INTENSITY]
Vector 80 [betaR] 3
Float 92 [mieG]
Float 96 [_Scale]
Float 100 [_global_alpha]
Float 104 [_Exposure]
Float 108 [_global_depth]
Vector 112 [_inCamPos] 3
Vector 128 [_Globals_Origin] 3
ConstBuffer "UnityPerCamera" 128
Vector 112 [_ZBufferParams]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedfmlciifbbpnkpggkgpmieflcelldmoiaabaaaaaabaekaaaaadaaaaaa
cmaaaaaaleaaaaaaoiaaaaaaejfdeheoiaaaaaaaaeaaaaaaaiaaaaaagiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaheaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaaheaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaa
amamaaaaheaaaaaaacaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaafdfgfpfa
gphdgjhegjgpgoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefccaejaaaaeaaaaaaaeibcaaaafjaaaaaeegiocaaa
aaaaaaaaajaaaaaafjaaaaaeegiocaaaabaaaaaaaiaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaagcbaaaadmcbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacbdaaaaaa
efaaaaajpcaabaaaaaaaaaaaogbkbaaaabaaaaaaeghobaaaabaaaaaaaagabaaa
adaaaaaadcaaaaalbcaabaaaaaaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaa
aaaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaaaaaaaaabiaaaaahccaabaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpbpaaaeadbkaabaaaaaaaaaaa
dgaaaaaipccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
doaaaaabbfaaaaabefaaaaajpcaabaaaabaaaaaaegbabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaacaaaaaaaaaaaaakhcaabaaaacaaaaaaegiccaaaaaaaaaaa
ahaaaaaaegiccaiaebaaaaaaaaaaaaaaaiaaaaaadiaaaaahocaabaaaaaaaaaaa
agaabaaaaaaaaaaaagbjbaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaagaabaaa
aaaaaaaaegbcbaaaacaaaaaaegacbaaaacaaaaaabaaaaaahicaabaaaabaaaaaa
jgahbaaaaaaaaaaajgahbaaaaaaaaaaaelaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaaibcaabaaaaeaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaa
agaaaaaaaoaaaaahocaabaaaaaaaaaaafgaobaaaaaaaaaaaagaabaaaaeaaaaaa
baaaaaahicaabaaaaeaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaaelaaaaaf
icaabaaaaeaaaaaadkaabaaaaeaaaaaadiaaaaaiecaabaaaafaaaaaadkaabaaa
aeaaaaaaakiacaaaaaaaaaaaagaaaaaaaoaaaaaiicaabaaaaeaaaaaackaabaaa
afaaaaaaakiacaaaaaaaaaaaagaaaaaadiaaaaaiicaabaaaafaaaaaabkiacaaa
aaaaaaaaacaaaaaaabeaaaaaggggggdpdbaaaaahicaabaaaaeaaaaaadkaabaaa
aeaaaaaadkaabaaaafaaaaaaaaaaaaaiicaabaaaacaaaaaabkaabaaaacaaaaaa
bkiacaaaaaaaaaaaacaaaaaaaaaaaaaiccaabaaaagaaaaaabkaabaaaadaaaaaa
bkiacaaaaaaaaaaaacaaaaaabaaaaaahicaabaaaafaaaaaaigadbaaaacaaaaaa
igadbaaaacaaaaaaelaaaaaficaabaaaafaaaaaadkaabaaaafaaaaaadiaaaaai
ecaabaaaagaaaaaadkaabaaaafaaaaaaakiacaaaaaaaaaaaagaaaaaadgaaaaaf
bcaabaaaagaaaaaadkaabaaaacaaaaaadgaaaaafbcaabaaaafaaaaaabkaabaaa
acaaaaaadgaaaaafccaabaaaafaaaaaabkaabaaaadaaaaaadhaaaaajhcaabaaa
afaaaaaapgapbaaaaeaaaaaaegacbaaaagaaaaaaegacbaaaafaaaaaadgaaaaaf
ccaabaaaacaaaaaaakaabaaaafaaaaaabaaaaaahicaabaaaacaaaaaaegacbaaa
acaaaaaajgahbaaaaaaaaaaaaoaaaaahecaabaaaaeaaaaaadkaabaaaacaaaaaa
ckaabaaaafaaaaaadiaaaaahccaabaaaadaaaaaackaabaaaafaaaaaackaabaaa
afaaaaaadcaaaaakccaabaaaadaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaa
bkaabaiaebaaaaaaadaaaaaadcaaaaalccaabaaaadaaaaaackiacaaaaaaaaaaa
acaaaaaackiacaaaaaaaaaaaacaaaaaabkaabaaaadaaaaaaelaaaaafccaabaaa
adaaaaaabkaabaaaadaaaaaaaaaaaaajccaabaaaadaaaaaadkaabaiaebaaaaaa
acaaaaaabkaabaiaebaaaaaaadaaaaaadeaaaaahccaabaaaadaaaaaabkaabaaa
adaaaaaaabeaaaaaaaaaaaaadbaaaaahicaabaaaaeaaaaaaabeaaaaaaaaaaaaa
bkaabaaaadaaaaaadbaaaaahbcaabaaaafaaaaaabkaabaaaadaaaaaaakaabaaa
aeaaaaaaabaaaaahicaabaaaaeaaaaaadkaabaaaaeaaaaaaakaabaaaafaaaaaa
aaaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaaaoaaaaai
ecaabaaaagaaaaaadkaabaaaacaaaaaackiacaaaaaaaaaaaacaaaaaadcaaaaal
bcaabaaaagaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaaagaaaaaabkaabaia
ebaaaaaaadaaaaaadgaaaaagccaabaaaagaaaaaackiacaaaaaaaaaaaacaaaaaa
dgaaaaafccaabaaaaeaaaaaackaabaaaafaaaaaadhaaaaajncaabaaaafaaaaaa
pgapbaaaaeaaaaaaagajbaaaagaaaaaaagajbaaaaeaaaaaabnaaaaaiicaabaaa
abaaaaaackiacaaaaaaaaaaaacaaaaaackaabaaaafaaaaaabpaaaeaddkaabaaa
abaaaaaaddaaaaahicaabaaaabaaaaaaakaabaaaaeaaaaaaabeaaaaaaaaaiadp
dgaaaaaficaabaaaadaaaaaabkaabaaaafaaaaaadcaaaaakhcaabaaaagaaaaaa
jgahbaiaebaaaaaaaaaaaaaapgapbaaaabaaaaaamgacbaaaadaaaaaadcaaaaaj
hcaabaaaadaaaaaafgafbaaaadaaaaaajgahbaaaaaaaaaaaegacbaaaacaaaaaa
dhaaaaajhcaabaaaacaaaaaapgapbaaaaeaaaaaaegacbaaaadaaaaaaegacbaaa
acaaaaaabaaaaaaiicaabaaaabaaaaaajgahbaaaaaaaaaaaegiccaaaaaaaaaaa
aeaaaaaabaaaaaaibcaabaaaacaaaaaaegacbaaaacaaaaaaegiccaaaaaaaaaaa
aeaaaaaaaaaaaaaiccaabaaaacaaaaaabkiacaaaaaaaaaaaacaaaaaaabeaaaaa
aaaabgeedbaaaaahecaabaaaacaaaaaackaabaaaafaaaaaabkaabaaaacaaaaaa
aoaaaaahdcaabaaaacaaaaaaegaabaaaacaaaaaakgakbaaaafaaaaaadiaaaaah
hcaabaaaadaaaaaafgafbaaaacaaaaaaegacbaaaagaaaaaaaaaaaaaiicaabaaa
adaaaaaabkiacaaaaaaaaaaaacaaaaaaabeaaaaaaaaabgeedgaaaaaficaabaaa
agaaaaaackaabaaaafaaaaaadhaaaaajpcaabaaaadaaaaaakgakbaaaacaaaaaa
egaobaaaadaaaaaaegaobaaaagaaaaaabaaaaaahccaabaaaacaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaelaaaaafecaabaaaacaaaaaabkaabaaaacaaaaaa
baaaaaahccaabaaaaaaaaaaaegacbaaaadaaaaaajgahbaaaaaaaaaaabaaaaaai
icaabaaaaaaaaaaaegacbaaaadaaaaaaegiccaaaaaaaaaaaaeaaaaaaaoaaaaah
mcaabaaaaaaaaaaafganbaaaaaaaaaaakgakbaaaacaaaaaadbaaaaahicaabaaa
acaaaaaaabeaaaaaaaaaaaaadkaabaaaafaaaaaabpaaaeaddkaabaaaacaaaaaa
aaaaaaajicaabaaaacaaaaaadkaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaa
acaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaacaaaaaa
ckiacaaaaaaaaaaaacaaaaaaaoaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
akaabaaaadaaaaaaelaaaaafccaabaaaaeaaaaaadkaabaaaacaaaaaaaaaaaaah
icaabaaaacaaaaaadkaabaaaafaaaaaaabeaaaaajkjjbjdodiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaaabeaaaaajfdbeeebddaaaaaiccaabaaaadaaaaaa
dkaabaiaibaaaaaaacaaaaaaabeaaaaaaaaaiadpdeaaaaaiecaabaaaadaaaaaa
dkaabaiaibaaaaaaacaaaaaaabeaaaaaaaaaiadpaoaaaaakecaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpckaabaaaadaaaaaadiaaaaah
ccaabaaaadaaaaaackaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahecaabaaa
adaaaaaabkaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaajecaabaaaaeaaaaaa
ckaabaaaadaaaaaaabeaaaaafpkokkdmabeaaaaadgfkkolndcaaaaajecaabaaa
aeaaaaaackaabaaaadaaaaaackaabaaaaeaaaaaaabeaaaaaochgdidodcaaaaaj
ecaabaaaaeaaaaaackaabaaaadaaaaaackaabaaaaeaaaaaaabeaaaaaaebnkjlo
dcaaaaajecaabaaaadaaaaaackaabaaaadaaaaaackaabaaaaeaaaaaaabeaaaaa
diphhpdpdiaaaaahecaabaaaaeaaaaaackaabaaaadaaaaaabkaabaaaadaaaaaa
dbaaaaaiicaabaaaaeaaaaaaabeaaaaaaaaaiadpdkaabaiaibaaaaaaacaaaaaa
dcaaaaajecaabaaaaeaaaaaackaabaaaaeaaaaaaabeaaaaaaaaaaamaabeaaaaa
nlapmjdpabaaaaahecaabaaaaeaaaaaadkaabaaaaeaaaaaackaabaaaaeaaaaaa
dcaaaaajccaabaaaadaaaaaabkaabaaaadaaaaaackaabaaaadaaaaaackaabaaa
aeaaaaaaddaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaiadp
dbaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaadkaabaiaebaaaaaaacaaaaaa
dhaaaaakicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaiaebaaaaaaadaaaaaa
bkaabaaaadaaaaaadiaaaaahbcaabaaaaeaaaaaadkaabaaaacaaaaaaabeaaaaa
klkkckdpeiaaaaalpcaabaaaaeaaaaaaegaabaaaaeaaaaaaeghobaaaacaaaaaa
aagabaaaaaaaaaaaabeaaaaaaaaaaaaaaaaaaaajicaabaaaacaaaaaackaabaaa
acaaaaaabkiacaiaebaaaaaaaaaaaaaaacaaaaaaaoaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaaakaabaaaadaaaaaaelaaaaafccaabaaaadaaaaaadkaabaaa
acaaaaaaaaaaaaahicaabaaaacaaaaaackaabaaaaaaaaaaaabeaaaaajkjjbjdo
diaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaajfdbeeebddaaaaai
ecaabaaaadaaaaaadkaabaiaibaaaaaaacaaaaaaabeaaaaaaaaaiadpdeaaaaai
icaabaaaaeaaaaaadkaabaiaibaaaaaaacaaaaaaabeaaaaaaaaaiadpaoaaaaak
icaabaaaaeaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aeaaaaaadiaaaaahecaabaaaadaaaaaackaabaaaadaaaaaadkaabaaaaeaaaaaa
diaaaaahicaabaaaaeaaaaaackaabaaaadaaaaaackaabaaaadaaaaaadcaaaaaj
ccaabaaaafaaaaaadkaabaaaaeaaaaaaabeaaaaafpkokkdmabeaaaaadgfkkoln
dcaaaaajccaabaaaafaaaaaadkaabaaaaeaaaaaabkaabaaaafaaaaaaabeaaaaa
ochgdidodcaaaaajccaabaaaafaaaaaadkaabaaaaeaaaaaabkaabaaaafaaaaaa
abeaaaaaaebnkjlodcaaaaajicaabaaaaeaaaaaadkaabaaaaeaaaaaabkaabaaa
afaaaaaaabeaaaaadiphhpdpdiaaaaahccaabaaaafaaaaaackaabaaaadaaaaaa
dkaabaaaaeaaaaaadbaaaaaiecaabaaaafaaaaaaabeaaaaaaaaaiadpdkaabaia
ibaaaaaaacaaaaaadcaaaaajccaabaaaafaaaaaabkaabaaaafaaaaaaabeaaaaa
aaaaaamaabeaaaaanlapmjdpabaaaaahccaabaaaafaaaaaackaabaaaafaaaaaa
bkaabaaaafaaaaaadcaaaaajecaabaaaadaaaaaackaabaaaadaaaaaadkaabaaa
aeaaaaaabkaabaaaafaaaaaaddaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
abeaaaaaaaaaiadpdbaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaadkaabaia
ebaaaaaaacaaaaaadhaaaaakicaabaaaacaaaaaadkaabaaaacaaaaaackaabaia
ebaaaaaaadaaaaaackaabaaaadaaaaaadiaaaaahbcaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaklkkckdpeiaaaaalpcaabaaaagaaaaaaegaabaaaadaaaaaa
eghobaaaacaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaaaoaaaaahhcaabaaa
adaaaaaaegacbaaaaeaaaaaaegacbaaaagaaaaaaddaaaaakhcaabaaaadaaaaaa
egacbaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaaaaabcaaaaab
aaaaaaajecaabaaaacaaaaaackaabaaaacaaaaaabkiacaiaebaaaaaaaaaaaaaa
acaaaaaaaaaaaaakicaabaaaacaaaaaabkiacaiaebaaaaaaaaaaaaaaacaaaaaa
ckiacaaaaaaaaaaaacaaaaaaaoaaaaahecaabaaaacaaaaaackaabaaaacaaaaaa
dkaabaaaacaaaaaaelaaaaafccaabaaaaeaaaaaackaabaaaacaaaaaaaaaaaaai
ecaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaaabeaaaaajkjjbjdodiaaaaah
ecaabaaaaaaaaaaackaabaaaaaaaaaaaabeaaaaajfdbeeebddaaaaaiecaabaaa
acaaaaaackaabaiaibaaaaaaaaaaaaaaabeaaaaaaaaaiadpdeaaaaaiecaabaaa
aeaaaaaackaabaiaibaaaaaaaaaaaaaaabeaaaaaaaaaiadpaoaaaaakecaabaaa
aeaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpckaabaaaaeaaaaaa
diaaaaahecaabaaaacaaaaaackaabaaaacaaaaaackaabaaaaeaaaaaadiaaaaah
ecaabaaaaeaaaaaackaabaaaacaaaaaackaabaaaacaaaaaadcaaaaajicaabaaa
aeaaaaaackaabaaaaeaaaaaaabeaaaaafpkokkdmabeaaaaadgfkkolndcaaaaaj
icaabaaaaeaaaaaackaabaaaaeaaaaaadkaabaaaaeaaaaaaabeaaaaaochgdido
dcaaaaajicaabaaaaeaaaaaackaabaaaaeaaaaaadkaabaaaaeaaaaaaabeaaaaa
aebnkjlodcaaaaajecaabaaaaeaaaaaackaabaaaaeaaaaaadkaabaaaaeaaaaaa
abeaaaaadiphhpdpdiaaaaahicaabaaaaeaaaaaackaabaaaacaaaaaackaabaaa
aeaaaaaadbaaaaaiccaabaaaafaaaaaaabeaaaaaaaaaiadpckaabaiaibaaaaaa
aaaaaaaadcaaaaajicaabaaaaeaaaaaadkaabaaaaeaaaaaaabeaaaaaaaaaaama
abeaaaaanlapmjdpabaaaaahicaabaaaaeaaaaaabkaabaaaafaaaaaadkaabaaa
aeaaaaaadcaaaaajecaabaaaacaaaaaackaabaaaacaaaaaackaabaaaaeaaaaaa
dkaabaaaaeaaaaaaddaaaaahecaabaaaaaaaaaaackaabaaaaaaaaaaaabeaaaaa
aaaaiadpdbaaaaaiecaabaaaaaaaaaaackaabaaaaaaaaaaackaabaiaebaaaaaa
aaaaaaaadhaaaaakecaabaaaaaaaaaaackaabaaaaaaaaaaackaabaiaebaaaaaa
acaaaaaackaabaaaacaaaaaadiaaaaahbcaabaaaaeaaaaaackaabaaaaaaaaaaa
abeaaaaaklkkckdpeiaaaaalpcaabaaaaeaaaaaaegaabaaaaeaaaaaaeghobaaa
acaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaa
dkaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaacaaaaaaaoaaaaahecaabaaa
aaaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaaelaaaaafccaabaaaagaaaaaa
ckaabaaaaaaaaaaaaaaaaaaiecaabaaaaaaaaaaadkaabaiaebaaaaaaafaaaaaa
abeaaaaajkjjbjdodiaaaaahecaabaaaaaaaaaaackaabaaaaaaaaaaaabeaaaaa
jfdbeeebddaaaaaiecaabaaaacaaaaaackaabaiaibaaaaaaaaaaaaaaabeaaaaa
aaaaiadpdeaaaaaiicaabaaaacaaaaaackaabaiaibaaaaaaaaaaaaaaabeaaaaa
aaaaiadpaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaacaaaaaadiaaaaahecaabaaaacaaaaaadkaabaaaacaaaaaa
ckaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaackaabaaaacaaaaaackaabaaa
acaaaaaadcaaaaajicaabaaaaeaaaaaadkaabaaaacaaaaaaabeaaaaafpkokkdm
abeaaaaadgfkkolndcaaaaajicaabaaaaeaaaaaadkaabaaaacaaaaaadkaabaaa
aeaaaaaaabeaaaaaochgdidodcaaaaajicaabaaaaeaaaaaadkaabaaaacaaaaaa
dkaabaaaaeaaaaaaabeaaaaaaebnkjlodcaaaaajicaabaaaacaaaaaadkaabaaa
acaaaaaadkaabaaaaeaaaaaaabeaaaaadiphhpdpdiaaaaahicaabaaaaeaaaaaa
dkaabaaaacaaaaaackaabaaaacaaaaaadbaaaaaiccaabaaaafaaaaaaabeaaaaa
aaaaiadpckaabaiaibaaaaaaaaaaaaaadcaaaaajicaabaaaaeaaaaaadkaabaaa
aeaaaaaaabeaaaaaaaaaaamaabeaaaaanlapmjdpabaaaaahicaabaaaaeaaaaaa
bkaabaaaafaaaaaadkaabaaaaeaaaaaadcaaaaajecaabaaaacaaaaaackaabaaa
acaaaaaadkaabaaaacaaaaaadkaabaaaaeaaaaaaddaaaaahecaabaaaaaaaaaaa
ckaabaaaaaaaaaaaabeaaaaaaaaaiadpdbaaaaaiecaabaaaaaaaaaaackaabaaa
aaaaaaaackaabaiaebaaaaaaaaaaaaaadhaaaaakecaabaaaaaaaaaaackaabaaa
aaaaaaaackaabaiaebaaaaaaacaaaaaackaabaaaacaaaaaadiaaaaahbcaabaaa
agaaaaaackaabaaaaaaaaaaaabeaaaaaklkkckdpeiaaaaalpcaabaaaagaaaaaa
egaabaaaagaaaaaaeghobaaaacaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaa
aoaaaaahhcaabaaaaeaaaaaaegacbaaaaeaaaaaaegacbaaaagaaaaaaddaaaaak
hcaabaaaadaaaaaaegacbaaaaeaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaaaaabfaaaaabaoaaaaaiecaabaaaaaaaaaaabkiacaaaaaaaaaaaacaaaaaa
dkaabaaaadaaaaaadcaaaaakecaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaa
ckaabaaaaaaaaaaaabeaaaaaaaaaiadpelaaaaafecaabaaaaaaaaaaackaabaaa
aaaaaaaaaaaaaaahecaabaaaacaaaaaackaabaaaaaaaaaaadkaabaaaafaaaaaa
dbaaaaaiicaabaaaacaaaaaackaabaiaibaaaaaaacaaaaaaabeaaaaagpbciddl
bpaaaeaddkaabaaaacaaaaaaaaaaaaahecaabaaaacaaaaaackaabaaaacaaaaaa
abeaaaaagpbciddldiaaaaahecaabaaaacaaaaaackaabaaaacaaaaaaabeaaaaa
pppppjecaaaaaaaldcaabaaaaeaaaaaakgakbaiaebaaaaaaaaaaaaaaaceaaaaa
gpbcidllgpbciddlaaaaaaaaaaaaaaaadiaaaaahecaabaaaaaaaaaaadkaabaaa
adaaaaaadkaabaaaadaaaaaadcaaaaajicaabaaaacaaaaaaakaabaaaafaaaaaa
akaabaaaafaaaaaackaabaaaaaaaaaaaapaaaaahecaabaaaaeaaaaaaagaabaaa
afaaaaaapgapbaaaadaaaaaadcaaaaajmcaabaaaaeaaaaaakgakbaaaaeaaaaaa
agaebaaaaeaaaaaapgapbaaaacaaaaaaelaaaaafmcaabaaaaeaaaaaakgaobaaa
aeaaaaaadiaaaaahgcaabaaaafaaaaaapgapbaaaadaaaaaaagabbaaaaeaaaaaa
dcaaaaajdcaabaaaaeaaaaaapgapbaaaadaaaaaaegaabaaaaeaaaaaaagaabaaa
afaaaaaadiaaaaajicaabaaaacaaaaaabkiacaaaaaaaaaaaacaaaaaabkiacaaa
aaaaaaaaacaaaaaadcaaaaambcaabaaaafaaaaaackiacaaaaaaaaaaaacaaaaaa
ckiacaaaaaaaaaaaacaaaaaadkaabaiaebaaaaaaacaaaaaaelaaaaafecaabaaa
agaaaaaaakaabaaaafaaaaaadcaaaaakbcaabaaaafaaaaaadkaabaaaadaaaaaa
dkaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaaelaaaaafbcaabaaaafaaaaaa
akaabaaaafaaaaaadcaaaaakdcaabaaaahaaaaaajgafbaaaafaaaaaajgafbaaa
afaaaaaakgakbaiaebaaaaaaaaaaaaaadcaaaaaldcaabaaaahaaaaaafgifcaaa
aaaaaaaaacaaaaaafgifcaaaaaaaaaaaacaaaaaaegaabaaaahaaaaaadbaaaaak
mcaabaaaahaaaaaafgajbaaaafaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaadbaaaaakdcaabaaaaiaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaegaabaaaahaaaaaaabaaaaahmcaabaaaahaaaaaakgaobaaaahaaaaaa
agaebaaaaiaaaaaaaoaaaaalhcaabaaaaiaaaaaaaceaaaaaaaaaaadpaaaaaadp
aaaaaadpaaaaaaaabgigcaaaaaaaaaaaadaaaaaaaaaaaaaiicaabaaaajaaaaaa
akaabaiaebaaaaaaaiaaaaaaabeaaaaaaaaaaadpdiaaaaahccaabaaaagaaaaaa
ckaabaaaagaaaaaackaabaaaagaaaaaaaaaaaaahicaabaaaagaaaaaaakaabaaa
aiaaaaaaabeaaaaaaaaaaadpdgaaaaaihcaabaaaajaaaaaaaceaaaaaaaaaiadp
aaaaaaaaaaaaaaaaaaaaaaaadgaaaaafbcaabaaaagaaaaaaabeaaaaaaaaaialp
dhaaaaajpcaabaaaakaaaaaakgakbaaaahaaaaaaigaobaaaajaaaaaaegaobaaa
agaaaaaaaoaaaaahecaabaaaaaaaaaaaakaabaaaafaaaaaackaabaaaagaaaaaa
aoaaaaalhcaabaaaalaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
egiccaaaaaaaaaaaadaaaaaaaaaaaaalhcaabaaaalaaaaaaegacbaiaebaaaaaa
alaaaaaaaceaaaaaaaaaiadpaaaaaadpaaaaiadpaaaaaaaadcaaaaajecaabaaa
aaaaaaaackaabaaaaaaaaaaaakaabaaaalaaaaaabkaabaaaaiaaaaaaaaaaaaah
bcaabaaaahaaaaaaakaabaaaahaaaaaabkaabaaaakaaaaaaelaaaaafbcaabaaa
ahaaaaaaakaabaaaahaaaaaadcaaaaajccaabaaaafaaaaaabkaabaaaafaaaaaa
akaabaaaakaaaaaaakaabaaaahaaaaaaaaaaaaahbcaabaaaahaaaaaaakaabaaa
afaaaaaackaabaaaakaaaaaaaoaaaaahccaabaaaafaaaaaabkaabaaaafaaaaaa
akaabaaaahaaaaaadcaaaaajccaabaaaafaaaaaabkaabaaaafaaaaaabkaabaaa
alaaaaaadkaabaaaakaaaaaadeaaaaahbcaabaaaahaaaaaaakaabaaaacaaaaaa
abeaaaaahbdneklodiaaaaahbcaabaaaahaaaaaaakaabaaaahaaaaaaabeaaaaa
bodakleaddaaaaaiecaabaaaahaaaaaaakaabaiaibaaaaaaahaaaaaaabeaaaaa
aaaaiadpdeaaaaaibcaabaaaaiaaaaaaakaabaiaibaaaaaaahaaaaaaabeaaaaa
aaaaiadpaoaaaaakbcaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaiaaaaaadiaaaaahecaabaaaahaaaaaackaabaaaahaaaaaa
akaabaaaaiaaaaaadiaaaaahbcaabaaaaiaaaaaackaabaaaahaaaaaackaabaaa
ahaaaaaadcaaaaajicaabaaaaiaaaaaaakaabaaaaiaaaaaaabeaaaaafpkokkdm
abeaaaaadgfkkolndcaaaaajicaabaaaaiaaaaaaakaabaaaaiaaaaaadkaabaaa
aiaaaaaaabeaaaaaochgdidodcaaaaajicaabaaaaiaaaaaaakaabaaaaiaaaaaa
dkaabaaaaiaaaaaaabeaaaaaaebnkjlodcaaaaajbcaabaaaaiaaaaaaakaabaaa
aiaaaaaadkaabaaaaiaaaaaaabeaaaaadiphhpdpdiaaaaahicaabaaaaiaaaaaa
ckaabaaaahaaaaaaakaabaaaaiaaaaaadbaaaaaibcaabaaaakaaaaaaabeaaaaa
aaaaiadpakaabaiaibaaaaaaahaaaaaadcaaaaajicaabaaaaiaaaaaadkaabaaa
aiaaaaaaabeaaaaaaaaaaamaabeaaaaanlapmjdpabaaaaahicaabaaaaiaaaaaa
akaabaaaakaaaaaadkaabaaaaiaaaaaadcaaaaajecaabaaaahaaaaaackaabaaa
ahaaaaaaakaabaaaaiaaaaaadkaabaaaaiaaaaaaddaaaaahbcaabaaaahaaaaaa
akaabaaaahaaaaaaabeaaaaaaaaaiadpdbaaaaaibcaabaaaahaaaaaaakaabaaa
ahaaaaaaakaabaiaebaaaaaaahaaaaaadhaaaaakbcaabaaaahaaaaaaakaabaaa
ahaaaaaackaabaiaebaaaaaaahaaaaaackaabaaaahaaaaaadcaaaaajbcaabaaa
ahaaaaaaakaabaaaahaaaaaaabeaaaaacolkgidpabeaaaaakehadndpdiaaaaah
bcaabaaaahaaaaaaakaabaaaahaaaaaaabeaaaaaaaaaaadpdcaaaaajbcaabaaa
ahaaaaaaakaabaaaahaaaaaackaabaaaalaaaaaackaabaaaaiaaaaaaaaaaaaah
ecaabaaaahaaaaaadkaabaaaabaaaaaaabeaaaaaaaaaiadpdiaaaaahecaabaaa
ahaaaaaackaabaaaahaaaaaaabeaaaaaaaaaaadpaaaaaaaibcaabaaaaiaaaaaa
dkiacaaaaaaaaaaaadaaaaaaabeaaaaaaaaaialpdiaaaaahicaabaaaaiaaaaaa
ckaabaaaahaaaaaaakaabaaaaiaaaaaaebaaaaaficaabaaaaiaaaaaadkaabaaa
aiaaaaaadcaaaaakecaabaaaahaaaaaackaabaaaahaaaaaaakaabaaaaiaaaaaa
dkaabaiaebaaaaaaaiaaaaaadiaaaaaibcaabaaaaiaaaaaackaabaaaaaaaaaaa
akiacaaaaaaaaaaaadaaaaaadcaaaaakecaabaaaaaaaaaaackaabaaaaaaaaaaa
akiacaaaaaaaaaaaadaaaaaaabeaaaaaaaaaialpebaaaaafecaabaaaaaaaaaaa
ckaabaaaaaaaaaaaaoaaaaaiecaabaaaaaaaaaaackaabaaaaaaaaaaaakiacaaa
aaaaaaaaadaaaaaaebaaaaafbcaabaaaakaaaaaaakaabaaaaiaaaaaaaoaaaaai
bcaabaaaakaaaaaaakaabaaaakaaaaaaakiacaaaaaaaaaaaadaaaaaabkaaaaaf
bcaabaaaaiaaaaaaakaabaaaaiaaaaaaaaaaaaahbcaabaaaahaaaaaaakaabaaa
ahaaaaaadkaabaaaaiaaaaaaaoaaaaaiccaabaaaamaaaaaaakaabaaaahaaaaaa
dkiacaaaaaaaaaaaadaaaaaaaoaaaaaiccaabaaaafaaaaaabkaabaaaafaaaaaa
akiacaaaaaaaaaaaadaaaaaaaaaaaaahbcaabaaaamaaaaaackaabaaaaaaaaaaa
bkaabaaaafaaaaaaeiaaaaalpcaabaaaanaaaaaabgafbaaaamaaaaaaeghobaaa
adaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaaaaaaaaaiccaabaaaakaaaaaa
ckaabaiaebaaaaaaahaaaaaaabeaaaaaaaaaiadpaaaaaaahbcaabaaaahaaaaaa
akaabaaaahaaaaaaabeaaaaaaaaaiadpaoaaaaaiecaabaaaamaaaaaaakaabaaa
ahaaaaaadkiacaaaaaaaaaaaadaaaaaaeiaaaaalpcaabaaaaoaaaaaacgakbaaa
amaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaah
pcaabaaaaoaaaaaakgakbaaaahaaaaaaegaobaaaaoaaaaaadcaaaaajpcaabaaa
anaaaaaaegaobaaaanaaaaaafgafbaaaakaaaaaaegaobaaaaoaaaaaaaaaaaaah
icaabaaaamaaaaaaakaabaaaakaaaaaabkaabaaaafaaaaaaeiaaaaalpcaabaaa
aoaaaaaangafbaaaamaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaa
aaaaaaaaeiaaaaalpcaabaaaapaaaaaaogakbaaaamaaaaaaeghobaaaadaaaaaa
aagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaahpcaabaaaapaaaaaakgakbaaa
ahaaaaaaegaobaaaapaaaaaadcaaaaajpcaabaaaaoaaaaaaegaobaaaaoaaaaaa
fgafbaaaakaaaaaaegaobaaaapaaaaaaaaaaaaaiccaabaaaafaaaaaaakaabaia
ebaaaaaaaiaaaaaaabeaaaaaaaaaiadpdiaaaaahpcaabaaaaoaaaaaaagaabaaa
aiaaaaaaegaobaaaaoaaaaaadcaaaaajpcaabaaaanaaaaaaegaobaaaanaaaaaa
fgafbaaaafaaaaaaegaobaaaaoaaaaaadiaaaaahmcaabaaaakaaaaaakgaobaaa
aeaaaaaakgaobaaaaeaaaaaadcaaaaakmcaabaaaaeaaaaaakgaobaaaaeaaaaaa
kgaobaaaaeaaaaaapgapbaiaebaaaaaaacaaaaaaelaaaaafmcaabaaaaeaaaaaa
kgaobaaaaeaaaaaadcaaaaakmcaabaaaakaaaaaaagaebaaaaeaaaaaaagaebaaa
aeaaaaaakgaobaiaebaaaaaaakaaaaaadcaaaaalmcaabaaaakaaaaaafgifcaaa
aaaaaaaaacaaaaaafgifcaaaaaaaaaaaacaaaaaakgaobaaaakaaaaaadbaaaaak
dcaabaaaaoaaaaaaegaabaaaaeaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaadbaaaaakmcaabaaaaoaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaakgaobaaaakaaaaaaabaaaaahdcaabaaaaoaaaaaaogakbaaaaoaaaaaa
egaabaaaaoaaaaaadhaaaaajpcaabaaaapaaaaaaagaabaaaaoaaaaaaigaobaaa
ajaaaaaaegaobaaaagaaaaaaaoaaaaahfcaabaaaaoaaaaaakgalbaaaaeaaaaaa
kgakbaaaagaaaaaadcaaaaajjcaabaaaalaaaaaaagaibaaaaoaaaaaaagaabaaa
alaaaaaafgafbaaaaiaaaaaaaaaaaaahicaabaaaacaaaaaackaabaaaakaaaaaa
bkaabaaaapaaaaaaelaaaaaficaabaaaacaaaaaadkaabaaaacaaaaaadcaaaaaj
icaabaaaacaaaaaaakaabaaaaeaaaaaaakaabaaaapaaaaaadkaabaaaacaaaaaa
aaaaaaahbcaabaaaaeaaaaaackaabaaaaeaaaaaackaabaaaapaaaaaaaoaaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaaakaabaaaaeaaaaaadcaaaaajicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaalaaaaaadkaabaaaapaaaaaadeaaaaah
bcaabaaaaeaaaaaadkaabaaaaaaaaaaaabeaaaaahbdneklodiaaaaahbcaabaaa
aeaaaaaaakaabaaaaeaaaaaaabeaaaaabodakleaddaaaaaiecaabaaaaeaaaaaa
akaabaiaibaaaaaaaeaaaaaaabeaaaaaaaaaiadpdeaaaaaibcaabaaaahaaaaaa
akaabaiaibaaaaaaaeaaaaaaabeaaaaaaaaaiadpaoaaaaakbcaabaaaahaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaahaaaaaadiaaaaah
ecaabaaaaeaaaaaackaabaaaaeaaaaaaakaabaaaahaaaaaadiaaaaahbcaabaaa
ahaaaaaackaabaaaaeaaaaaackaabaaaaeaaaaaadcaaaaajccaabaaaaiaaaaaa
akaabaaaahaaaaaaabeaaaaafpkokkdmabeaaaaadgfkkolndcaaaaajccaabaaa
aiaaaaaaakaabaaaahaaaaaabkaabaaaaiaaaaaaabeaaaaaochgdidodcaaaaaj
ccaabaaaaiaaaaaaakaabaaaahaaaaaabkaabaaaaiaaaaaaabeaaaaaaebnkjlo
dcaaaaajbcaabaaaahaaaaaaakaabaaaahaaaaaabkaabaaaaiaaaaaaabeaaaaa
diphhpdpdiaaaaahccaabaaaaiaaaaaackaabaaaaeaaaaaaakaabaaaahaaaaaa
dbaaaaaiecaabaaaakaaaaaaabeaaaaaaaaaiadpakaabaiaibaaaaaaaeaaaaaa
dcaaaaajccaabaaaaiaaaaaabkaabaaaaiaaaaaaabeaaaaaaaaaaamaabeaaaaa
nlapmjdpabaaaaahccaabaaaaiaaaaaackaabaaaakaaaaaabkaabaaaaiaaaaaa
dcaaaaajecaabaaaaeaaaaaackaabaaaaeaaaaaaakaabaaaahaaaaaabkaabaaa
aiaaaaaaddaaaaahbcaabaaaaeaaaaaaakaabaaaaeaaaaaaabeaaaaaaaaaiadp
dbaaaaaibcaabaaaaeaaaaaaakaabaaaaeaaaaaaakaabaiaebaaaaaaaeaaaaaa
dhaaaaakbcaabaaaaeaaaaaaakaabaaaaeaaaaaackaabaiaebaaaaaaaeaaaaaa
ckaabaaaaeaaaaaadcaaaaajbcaabaaaaeaaaaaaakaabaaaaeaaaaaaabeaaaaa
colkgidpabeaaaaakehadndpdiaaaaahbcaabaaaaeaaaaaackaabaaaalaaaaaa
akaabaaaaeaaaaaadcaaaaajbcaabaaaaeaaaaaaakaabaaaaeaaaaaaabeaaaaa
aaaaaadpckaabaaaaiaaaaaadiaaaaaigcaabaaaaiaaaaaaagadbaaaalaaaaaa
agiacaaaaaaaaaaaadaaaaaadcaaaaanfcaabaaaalaaaaaaagadbaaaalaaaaaa
agiacaaaaaaaaaaaadaaaaaaaceaaaaaaaaaialpaaaaaaaaaaaaialpaaaaaaaa
ebaaaaaffcaabaaaalaaaaaaagacbaaaalaaaaaaaoaaaaaifcaabaaaalaaaaaa
agacbaaaalaaaaaaagiacaaaaaaaaaaaadaaaaaaebaaaaaffcaabaaaaoaaaaaa
fgagbaaaaiaaaaaaaoaaaaaifcaabaaaaoaaaaaaagacbaaaaoaaaaaaagiacaaa
aaaaaaaaadaaaaaabkaaaaafgcaabaaaaiaaaaaafgagbaaaaiaaaaaaaaaaaaah
bcaabaaaaeaaaaaaakaabaaaaeaaaaaadkaabaaaaiaaaaaaaoaaaaaiccaabaaa
apaaaaaaakaabaaaaeaaaaaadkiacaaaaaaaaaaaadaaaaaaaoaaaaaiicaabaaa
acaaaaaadkaabaaaacaaaaaaakiacaaaaaaaaaaaadaaaaaaaaaaaaahbcaabaaa
apaaaaaaakaabaaaalaaaaaadkaabaaaacaaaaaaeiaaaaalpcaabaaabaaaaaaa
bgafbaaaapaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaa
aaaaaaahbcaabaaaaeaaaaaaakaabaaaaeaaaaaaabeaaaaaaaaaiadpaoaaaaai
ecaabaaaapaaaaaaakaabaaaaeaaaaaadkiacaaaaaaaaaaaadaaaaaaeiaaaaal
pcaabaaabbaaaaaacgakbaaaapaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaa
abeaaaaaaaaaaaaadiaaaaahpcaabaaabbaaaaaakgakbaaaahaaaaaaegaobaaa
bbaaaaaadcaaaaajpcaabaaabaaaaaaaegaobaaabaaaaaaafgafbaaaakaaaaaa
egaobaaabbaaaaaaaaaaaaahicaabaaaapaaaaaaakaabaaaaoaaaaaadkaabaaa
acaaaaaaeiaaaaalpcaabaaabbaaaaaangafbaaaapaaaaaaeghobaaaadaaaaaa
aagabaaaabaaaaaaabeaaaaaaaaaaaaaeiaaaaalpcaabaaabcaaaaaaogakbaaa
apaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaah
pcaabaaabcaaaaaakgakbaaaahaaaaaaegaobaaabcaaaaaadcaaaaajpcaabaaa
bbaaaaaaegaobaaabbaaaaaafgafbaaaakaaaaaaegaobaaabcaaaaaaaaaaaaal
fcaabaaaaeaaaaaafgagbaiaebaaaaaaaiaaaaaaaceaaaaaaaaaiadpaaaaaaaa
aaaaiadpaaaaaaaadiaaaaahpcaabaaabbaaaaaafgafbaaaaiaaaaaaegaobaaa
bbaaaaaadcaaaaajpcaabaaabaaaaaaaegaobaaabaaaaaaaagaabaaaaeaaaaaa
egaobaaabbaaaaaadcaaaaakpcaabaaaanaaaaaaegaobaiaebaaaaaabaaaaaaa
egacbaaaadaaaaaaegaobaaaanaaaaaadeaaaaakpcaabaaaanaaaaaaegaobaaa
anaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadhaaaaajpcaabaaa
baaaaaaapgapbaaaahaaaaaaigaobaaaajaaaaaaegaobaaaagaaaaaaaaaaaaah
icaabaaaacaaaaaabkaabaaaahaaaaaabkaabaaabaaaaaaaelaaaaaficaabaaa
acaaaaaadkaabaaaacaaaaaadcaaaaajicaabaaaacaaaaaackaabaaaafaaaaaa
akaabaaabaaaaaaadkaabaaaacaaaaaaaaaaaaahbcaabaaaaeaaaaaaakaabaaa
afaaaaaackaabaaabaaaaaaaaoaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
akaabaaaaeaaaaaadcaaaaajicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaa
alaaaaaadkaabaaabaaaaaaaaoaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaa
akiacaaaaaaaaaaaadaaaaaaaaaaaaahbcaabaaaamaaaaaackaabaaaaaaaaaaa
dkaabaaaacaaaaaaeiaaaaalpcaabaaabaaaaaaabgafbaaaamaaaaaaeghobaaa
adaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaaeiaaaaalpcaabaaabbaaaaaa
cgakbaaaamaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaa
diaaaaahpcaabaaabbaaaaaakgakbaaaahaaaaaaegaobaaabbaaaaaadcaaaaaj
pcaabaaabaaaaaaaegaobaaabaaaaaaafgafbaaaakaaaaaaegaobaaabbaaaaaa
aaaaaaahicaabaaaamaaaaaaakaabaaaakaaaaaadkaabaaaacaaaaaaeiaaaaal
pcaabaaabbaaaaaangafbaaaamaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaa
abeaaaaaaaaaaaaaeiaaaaalpcaabaaaamaaaaaaogakbaaaamaaaaaaeghobaaa
adaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaahpcaabaaaamaaaaaa
kgakbaaaahaaaaaaegaobaaaamaaaaaadcaaaaajpcaabaaaamaaaaaaegaobaaa
bbaaaaaafgafbaaaakaaaaaaegaobaaaamaaaaaadiaaaaahpcaabaaaamaaaaaa
agaabaaaaiaaaaaaegaobaaaamaaaaaadcaaaaajpcaabaaaamaaaaaaegaobaaa
baaaaaaafgafbaaaafaaaaaaegaobaaaamaaaaaadhaaaaajpcaabaaaagaaaaaa
fgafbaaaaoaaaaaaegaobaaaajaaaaaaegaobaaaagaaaaaaaaaaaaahecaabaaa
aaaaaaaabkaabaaaagaaaaaadkaabaaaakaaaaaaelaaaaafecaabaaaaaaaaaaa
ckaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaeaaaaaaakaabaaa
agaaaaaackaabaaaaaaaaaaaaaaaaaahicaabaaaacaaaaaadkaabaaaaeaaaaaa
ckaabaaaagaaaaaaaoaaaaahecaabaaaaaaaaaaackaabaaaaaaaaaaadkaabaaa
acaaaaaadcaaaaajecaabaaaaaaaaaaackaabaaaaaaaaaaabkaabaaaalaaaaaa
dkaabaaaagaaaaaaaoaaaaaiecaabaaaaaaaaaaackaabaaaaaaaaaaaakiacaaa
aaaaaaaaadaaaaaaaaaaaaahbcaabaaaapaaaaaackaabaaaalaaaaaackaabaaa
aaaaaaaaeiaaaaalpcaabaaaagaaaaaabgafbaaaapaaaaaaeghobaaaadaaaaaa
aagabaaaabaaaaaaabeaaaaaaaaaaaaaeiaaaaalpcaabaaaajaaaaaacgakbaaa
apaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaah
pcaabaaaajaaaaaakgakbaaaahaaaaaaegaobaaaajaaaaaadcaaaaajpcaabaaa
agaaaaaaegaobaaaagaaaaaafgafbaaaakaaaaaaegaobaaaajaaaaaaaaaaaaah
icaabaaaapaaaaaackaabaaaaoaaaaaackaabaaaaaaaaaaaeiaaaaalpcaabaaa
ajaaaaaangafbaaaapaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaa
aaaaaaaaeiaaaaalpcaabaaaalaaaaaaogakbaaaapaaaaaaeghobaaaadaaaaaa
aagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaahpcaabaaaahaaaaaakgakbaaa
ahaaaaaaegaobaaaalaaaaaadcaaaaajpcaabaaaahaaaaaaegaobaaaajaaaaaa
fgafbaaaakaaaaaaegaobaaaahaaaaaadiaaaaahpcaabaaaahaaaaaakgakbaaa
aiaaaaaaegaobaaaahaaaaaadcaaaaajpcaabaaaaeaaaaaaegaobaaaagaaaaaa
kgakbaaaaeaaaaaaegaobaaaahaaaaaadcaaaaakpcaabaaaaeaaaaaaegaobaia
ebaaaaaaaeaaaaaaegacbaaaadaaaaaaegaobaaaamaaaaaadeaaaaakpcaabaaa
aeaaaaaaegaobaaaaeaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaipcaabaaaaeaaaaaaegaobaiaebaaaaaaanaaaaaaegaobaaaaeaaaaaa
dcaaaaajpcaabaaaaeaaaaaakgakbaaaacaaaaaaegaobaaaaeaaaaaaegaobaaa
anaaaaaabcaaaaabdiaaaaajecaabaaaaaaaaaaabkiacaaaaaaaaaaaacaaaaaa
bkiacaaaaaaaaaaaacaaaaaadcaaaaamecaabaaaacaaaaaackiacaaaaaaaaaaa
acaaaaaackiacaaaaaaaaaaaacaaaaaackaabaiaebaaaaaaaaaaaaaaelaaaaaf
ecaabaaaagaaaaaackaabaaaacaaaaaadiaaaaahecaabaaaacaaaaaadkaabaaa
adaaaaaadkaabaaaadaaaaaadcaaaaakecaabaaaaaaaaaaadkaabaaaadaaaaaa
dkaabaaaadaaaaaackaabaiaebaaaaaaaaaaaaaaelaaaaafecaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaafaaaaaadkaabaaa
adaaaaaadcaaaaakecaabaaaacaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaa
ckaabaiaebaaaaaaacaaaaaadcaaaaalecaabaaaacaaaaaabkiacaaaaaaaaaaa
acaaaaaabkiacaaaaaaaaaaaacaaaaaackaabaaaacaaaaaadbaaaaahicaabaaa
adaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaaaadbaaaaahbcaabaaaafaaaaaa
abeaaaaaaaaaaaaackaabaaaacaaaaaaabaaaaahicaabaaaadaaaaaadkaabaaa
adaaaaaaakaabaaaafaaaaaaaoaaaaalhcaabaaaafaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaaadpaaaaaaaabgigcaaaaaaaaaaaadaaaaaaaaaaaaaiicaabaaa
ahaaaaaaakaabaiaebaaaaaaafaaaaaaabeaaaaaaaaaaadpdiaaaaahccaabaaa
agaaaaaackaabaaaagaaaaaackaabaaaagaaaaaaaaaaaaahicaabaaaagaaaaaa
akaabaaaafaaaaaaabeaaaaaaaaaaadpdgaaaaaihcaabaaaahaaaaaaaceaaaaa
aaaaiadpaaaaaaaaaaaaaaaaaaaaaaaadgaaaaafbcaabaaaagaaaaaaabeaaaaa
aaaaialpdhaaaaajpcaabaaaaiaaaaaapgapbaaaadaaaaaaigaobaaaahaaaaaa
egaobaaaagaaaaaaaoaaaaahicaabaaaadaaaaaackaabaaaaaaaaaaackaabaaa
agaaaaaaaoaaaaalhcaabaaaajaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpegiccaaaaaaaaaaaadaaaaaaaaaaaaalhcaabaaaajaaaaaaegacbaia
ebaaaaaaajaaaaaaaceaaaaaaaaaiadpaaaaaadpaaaaiadpaaaaaaaadcaaaaaj
icaabaaaadaaaaaadkaabaaaadaaaaaaakaabaaaajaaaaaabkaabaaaafaaaaaa
aaaaaaahecaabaaaacaaaaaackaabaaaacaaaaaabkaabaaaaiaaaaaaelaaaaaf
ecaabaaaacaaaaaackaabaaaacaaaaaadcaaaaajecaabaaaacaaaaaadkaabaaa
acaaaaaaakaabaaaaiaaaaaackaabaaaacaaaaaaaaaaaaahecaabaaaaaaaaaaa
ckaabaaaaaaaaaaackaabaaaaiaaaaaaaoaaaaahecaabaaaaaaaaaaackaabaaa
acaaaaaackaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaackaabaaaaaaaaaaa
bkaabaaaajaaaaaadkaabaaaaiaaaaaadeaaaaahecaabaaaacaaaaaaakaabaaa
acaaaaaaabeaaaaahbdneklodiaaaaahecaabaaaacaaaaaackaabaaaacaaaaaa
abeaaaaabodakleaddaaaaaiicaabaaaacaaaaaackaabaiaibaaaaaaacaaaaaa
abeaaaaaaaaaiadpdeaaaaaibcaabaaaafaaaaaackaabaiaibaaaaaaacaaaaaa
abeaaaaaaaaaiadpaoaaaaakbcaabaaaafaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpakaabaaaafaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaaakaabaaaafaaaaaadiaaaaahbcaabaaaafaaaaaadkaabaaaacaaaaaa
dkaabaaaacaaaaaadcaaaaajicaabaaaafaaaaaaakaabaaaafaaaaaaabeaaaaa
fpkokkdmabeaaaaadgfkkolndcaaaaajicaabaaaafaaaaaaakaabaaaafaaaaaa
dkaabaaaafaaaaaaabeaaaaaochgdidodcaaaaajicaabaaaafaaaaaaakaabaaa
afaaaaaadkaabaaaafaaaaaaabeaaaaaaebnkjlodcaaaaajbcaabaaaafaaaaaa
akaabaaaafaaaaaadkaabaaaafaaaaaaabeaaaaadiphhpdpdiaaaaahicaabaaa
afaaaaaadkaabaaaacaaaaaaakaabaaaafaaaaaadbaaaaaibcaabaaaaiaaaaaa
abeaaaaaaaaaiadpckaabaiaibaaaaaaacaaaaaadcaaaaajicaabaaaafaaaaaa
dkaabaaaafaaaaaaabeaaaaaaaaaaamaabeaaaaanlapmjdpabaaaaahicaabaaa
afaaaaaaakaabaaaaiaaaaaadkaabaaaafaaaaaadcaaaaajicaabaaaacaaaaaa
dkaabaaaacaaaaaaakaabaaaafaaaaaadkaabaaaafaaaaaaddaaaaahecaabaaa
acaaaaaackaabaaaacaaaaaaabeaaaaaaaaaiadpdbaaaaaiecaabaaaacaaaaaa
ckaabaaaacaaaaaackaabaiaebaaaaaaacaaaaaadhaaaaakecaabaaaacaaaaaa
ckaabaaaacaaaaaadkaabaiaebaaaaaaacaaaaaadkaabaaaacaaaaaadcaaaaaj
ecaabaaaacaaaaaackaabaaaacaaaaaaabeaaaaacolkgidpabeaaaaakehadndp
diaaaaahecaabaaaacaaaaaackaabaaaacaaaaaaabeaaaaaaaaaaadpdcaaaaaj
ecaabaaaacaaaaaackaabaaaacaaaaaackaabaaaajaaaaaackaabaaaafaaaaaa
aaaaaaahicaabaaaacaaaaaadkaabaaaabaaaaaaabeaaaaaaaaaiadpdiaaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaadpaaaaaaaibcaabaaa
afaaaaaadkiacaaaaaaaaaaaadaaaaaaabeaaaaaaaaaialpdiaaaaahicaabaaa
afaaaaaadkaabaaaacaaaaaaakaabaaaafaaaaaaebaaaaaficaabaaaafaaaaaa
dkaabaaaafaaaaaadcaaaaakicaabaaaacaaaaaadkaabaaaacaaaaaaakaabaaa
afaaaaaadkaabaiaebaaaaaaafaaaaaadiaaaaaibcaabaaaafaaaaaadkaabaaa
adaaaaaaakiacaaaaaaaaaaaadaaaaaadcaaaaakicaabaaaadaaaaaadkaabaaa
adaaaaaaakiacaaaaaaaaaaaadaaaaaaabeaaaaaaaaaialpebaaaaaficaabaaa
adaaaaaadkaabaaaadaaaaaaaoaaaaaiicaabaaaadaaaaaadkaabaaaadaaaaaa
akiacaaaaaaaaaaaadaaaaaaebaaaaafbcaabaaaaiaaaaaaakaabaaaafaaaaaa
aoaaaaaibcaabaaaaiaaaaaaakaabaaaaiaaaaaaakiacaaaaaaaaaaaadaaaaaa
bkaaaaafbcaabaaaafaaaaaaakaabaaaafaaaaaaaaaaaaahecaabaaaacaaaaaa
ckaabaaaacaaaaaadkaabaaaafaaaaaaaoaaaaaiccaabaaaakaaaaaackaabaaa
acaaaaaadkiacaaaaaaaaaaaadaaaaaaaoaaaaaiecaabaaaaaaaaaaackaabaaa
aaaaaaaaakiacaaaaaaaaaaaadaaaaaaaaaaaaahecaabaaaakaaaaaadkaabaaa
adaaaaaackaabaaaaaaaaaaaeiaaaaalpcaabaaaalaaaaaajgafbaaaakaaaaaa
eghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaaaaaaaaaiicaabaaa
adaaaaaadkaabaiaebaaaaaaacaaaaaaabeaaaaaaaaaiadpaaaaaaahecaabaaa
acaaaaaackaabaaaacaaaaaaabeaaaaaaaaaiadpaoaaaaaibcaabaaaakaaaaaa
ckaabaaaacaaaaaadkiacaaaaaaaaaaaadaaaaaaeiaaaaalpcaabaaaamaaaaaa
igaabaaaakaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaa
diaaaaahpcaabaaaamaaaaaapgapbaaaacaaaaaaegaobaaaamaaaaaadcaaaaaj
pcaabaaaalaaaaaaegaobaaaalaaaaaapgapbaaaadaaaaaaegaobaaaamaaaaaa
aaaaaaahicaabaaaakaaaaaaakaabaaaaiaaaaaackaabaaaaaaaaaaaeiaaaaal
pcaabaaaaiaaaaaangafbaaaakaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaa
abeaaaaaaaaaaaaaeiaaaaalpcaabaaaakaaaaaamgaabaaaakaaaaaaeghobaaa
adaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaahpcaabaaaakaaaaaa
pgapbaaaacaaaaaaegaobaaaakaaaaaadcaaaaajpcaabaaaaiaaaaaaegaobaaa
aiaaaaaapgapbaaaadaaaaaaegaobaaaakaaaaaaaaaaaaaiecaabaaaaaaaaaaa
akaabaiaebaaaaaaafaaaaaaabeaaaaaaaaaiadpdiaaaaahpcaabaaaaiaaaaaa
agaabaaaafaaaaaaegaobaaaaiaaaaaadcaaaaajpcaabaaaaiaaaaaaegaobaaa
alaaaaaakgakbaaaaaaaaaaaegaobaaaaiaaaaaadcaaaaamecaabaaaaaaaaaaa
bkiacaiaebaaaaaaaaaaaaaaacaaaaaabkiacaaaaaaaaaaaacaaaaaabkaabaaa
acaaaaaaelaaaaafecaabaaaaaaaaaaackaabaaaaaaaaaaadcaaaaakccaabaaa
acaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaiaebaaaaaaacaaaaaa
dcaaaaalccaabaaaacaaaaaabkiacaaaaaaaaaaaacaaaaaabkiacaaaaaaaaaaa
acaaaaaabkaabaaaacaaaaaadbaaaaahecaabaaaacaaaaaabkaabaaaaaaaaaaa
abeaaaaaaaaaaaaadbaaaaahbcaabaaaafaaaaaaabeaaaaaaaaaaaaabkaabaaa
acaaaaaaabaaaaahecaabaaaacaaaaaackaabaaaacaaaaaaakaabaaaafaaaaaa
dhaaaaajpcaabaaaahaaaaaakgakbaaaacaaaaaaegaobaaaahaaaaaaegaobaaa
agaaaaaaaoaaaaahecaabaaaacaaaaaackaabaaaaaaaaaaackaabaaaagaaaaaa
dcaaaaajecaabaaaacaaaaaackaabaaaacaaaaaaakaabaaaajaaaaaabkaabaaa
afaaaaaaaaaaaaahccaabaaaacaaaaaabkaabaaaacaaaaaabkaabaaaahaaaaaa
elaaaaafccaabaaaacaaaaaabkaabaaaacaaaaaadcaaaaajccaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakaabaaaahaaaaaabkaabaaaacaaaaaaaaaaaaahecaabaaa
aaaaaaaackaabaaaaaaaaaaackaabaaaahaaaaaaaoaaaaahccaabaaaaaaaaaaa
bkaabaaaaaaaaaaackaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaabkaabaaa
aaaaaaaabkaabaaaajaaaaaadkaabaaaahaaaaaadeaaaaahecaabaaaaaaaaaaa
dkaabaaaaaaaaaaaabeaaaaahbdneklodiaaaaahecaabaaaaaaaaaaackaabaaa
aaaaaaaaabeaaaaabodakleaddaaaaaiicaabaaaaaaaaaaackaabaiaibaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdeaaaaaiccaabaaaacaaaaaackaabaiaibaaaaaa
aaaaaaaaabeaaaaaaaaaiadpaoaaaaakccaabaaaacaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpbkaabaaaacaaaaaadiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaabkaabaaaacaaaaaadiaaaaahccaabaaaacaaaaaadkaabaaa
aaaaaaaadkaabaaaaaaaaaaadcaaaaajbcaabaaaafaaaaaabkaabaaaacaaaaaa
abeaaaaafpkokkdmabeaaaaadgfkkolndcaaaaajbcaabaaaafaaaaaabkaabaaa
acaaaaaaakaabaaaafaaaaaaabeaaaaaochgdidodcaaaaajbcaabaaaafaaaaaa
bkaabaaaacaaaaaaakaabaaaafaaaaaaabeaaaaaaebnkjlodcaaaaajccaabaaa
acaaaaaabkaabaaaacaaaaaaakaabaaaafaaaaaaabeaaaaadiphhpdpdiaaaaah
bcaabaaaafaaaaaadkaabaaaaaaaaaaabkaabaaaacaaaaaadbaaaaaiccaabaaa
afaaaaaaabeaaaaaaaaaiadpckaabaiaibaaaaaaaaaaaaaadcaaaaajbcaabaaa
afaaaaaaakaabaaaafaaaaaaabeaaaaaaaaaaamaabeaaaaanlapmjdpabaaaaah
bcaabaaaafaaaaaabkaabaaaafaaaaaaakaabaaaafaaaaaadcaaaaajicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkaabaaaacaaaaaaakaabaaaafaaaaaaddaaaaah
ecaabaaaaaaaaaaackaabaaaaaaaaaaaabeaaaaaaaaaiadpdbaaaaaiecaabaaa
aaaaaaaackaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaadhaaaaakecaabaaa
aaaaaaaackaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaajecaabaaaaaaaaaaackaabaaaaaaaaaaaabeaaaaacolkgidpabeaaaaa
kehadndpdiaaaaahecaabaaaaaaaaaaackaabaaaajaaaaaackaabaaaaaaaaaaa
dcaaaaajecaabaaaaaaaaaaackaabaaaaaaaaaaaabeaaaaaaaaaaadpckaabaaa
afaaaaaadiaaaaaiicaabaaaaaaaaaaackaabaaaacaaaaaaakiacaaaaaaaaaaa
adaaaaaadcaaaaakccaabaaaacaaaaaackaabaaaacaaaaaaakiacaaaaaaaaaaa
adaaaaaaabeaaaaaaaaaialpebaaaaafccaabaaaacaaaaaabkaabaaaacaaaaaa
ebaaaaafecaabaaaacaaaaaadkaabaaaaaaaaaaaaoaaaaaigcaabaaaacaaaaaa
fgagbaaaacaaaaaaagiacaaaaaaaaaaaadaaaaaabkaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaahecaabaaaaaaaaaaackaabaaaaaaaaaaadkaabaaa
afaaaaaaaoaaaaaiccaabaaaafaaaaaackaabaaaaaaaaaaadkiacaaaaaaaaaaa
adaaaaaaaoaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaa
adaaaaaaaaaaaaahmcaabaaaafaaaaaafgajbaaaacaaaaaafgafbaaaaaaaaaaa
eiaaaaalpcaabaaaagaaaaaajgafbaaaafaaaaaaeghobaaaadaaaaaaaagabaaa
abaaaaaaabeaaaaaaaaaaaaaaaaaaaahecaabaaaaaaaaaaackaabaaaaaaaaaaa
abeaaaaaaaaaiadpaoaaaaaibcaabaaaafaaaaaackaabaaaaaaaaaaadkiacaaa
aaaaaaaaadaaaaaaeiaaaaalpcaabaaaahaaaaaaigaabaaaafaaaaaaeghobaaa
adaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadiaaaaahpcaabaaaahaaaaaa
pgapbaaaacaaaaaaegaobaaaahaaaaaadcaaaaajpcaabaaaagaaaaaaegaobaaa
agaaaaaapgapbaaaadaaaaaaegaobaaaahaaaaaaeiaaaaalpcaabaaaahaaaaaa
ngafbaaaafaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaa
eiaaaaalpcaabaaaafaaaaaamgaabaaaafaaaaaaeghobaaaadaaaaaaaagabaaa
abaaaaaaabeaaaaaaaaaaaaadiaaaaahpcaabaaaafaaaaaapgapbaaaacaaaaaa
egaobaaaafaaaaaadcaaaaajpcaabaaaafaaaaaaegaobaaaahaaaaaapgapbaaa
adaaaaaaegaobaaaafaaaaaaaaaaaaaiccaabaaaaaaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahpcaabaaaafaaaaaapgapbaaaaaaaaaaa
egaobaaaafaaaaaadcaaaaajpcaabaaaafaaaaaaegaobaaaagaaaaaafgafbaaa
aaaaaaaaegaobaaaafaaaaaadcaaaaakpcaabaaaafaaaaaaegaobaiaebaaaaaa
afaaaaaaegacbaaaadaaaaaaegaobaaaaiaaaaaadeaaaaakpcaabaaaaeaaaaaa
egaobaaaafaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabfaaaaab
dicaaaahccaabaaaaaaaaaaaakaabaaaacaaaaaaabeaaaaaaaaaeiecdcaaaaaj
ecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaah
ccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaaaaaaaaaadiaaaaahccaabaaa
aaaaaaaabkaabaaaaaaaaaaadkaabaaaaeaaaaaadiaaaaahocaabaaaaaaaaaaa
fgafbaaaaaaaaaaaagajbaaaaeaaaaaadeaaaaahbcaabaaaacaaaaaaakaabaaa
aeaaaaaaabeaaaaabhlhnbdiaoaaaaahocaabaaaaaaaaaaafgaobaaaaaaaaaaa
agaabaaaacaaaaaaaoaaaaajhcaabaaaacaaaaaaagiacaaaaaaaaaaaafaaaaaa
egiccaaaaaaaaaaaafaaaaaadiaaaaahocaabaaaaaaaaaaafgaobaaaaaaaaaaa
agajbaaaacaaaaaadiaaaaaldcaabaaaacaaaaaaagiacaaaaaaaaaaaabaaaaaa
aceaaaaaaaaaiaebaaaaiaeaaaaaaaaaaaaaaaaaaoaaaaakdcaabaaaacaaaaaa
aceaaaaaaaaaeaeaaaaamadpaaaaaaaaaaaaaaaaegaabaaaacaaaaaadcaaaaaj
ecaabaaaacaaaaaadkaabaaaabaaaaaadkaabaaaabaaaaaaabeaaaaaaaaaiadp
dcaaaaamicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaa
aaaaaaaaafaaaaaaabeaaaaaaaaaiadpdiaaaaahdcaabaaaacaaaaaaogakbaaa
acaaaaaaegaabaaaacaaaaaadcaaaaaodcaabaaaafaaaaaapgipcaaaaaaaaaaa
afaaaaaapgipcaaaaaaaaaaaafaaaaaaaceaaaaaaaaaiadpaaaaaaeaaaaaaaaa
aaaaaaaaapaaaaaiicaabaaaabaaaaaapgapbaaaabaaaaaapgipcaaaaaaaaaaa
afaaaaaaaaaaaaaiicaabaaaabaaaaaadkaabaiaebaaaaaaabaaaaaaakaabaaa
afaaaaaacpaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahicaabaaa
abaaaaaadkaabaaaabaaaaaaabeaaaaaaaaamalpbjaaaaaficaabaaaabaaaaaa
dkaabaaaabaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaabkaabaaa
acaaaaaadiaaaaahicaabaaaabaaaaaackaabaaaacaaaaaadkaabaaaabaaaaaa
aoaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaabkaabaaaafaaaaaadiaaaaah
ocaabaaaaaaaaaaafgaobaaaaaaaaaaapgapbaaaabaaaaaadcaaaaajocaabaaa
aaaaaaaaagajbaaaaeaaaaaaagaabaaaacaaaaaafgaobaaaaaaaaaaabcaaaaab
dgaaaaaihcaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaaaaa
dgaaaaaiocaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
bfaaaaabdiaaaaaiocaabaaaaaaaaaaafgaobaaaaaaaaaaapgipcaaaaaaaaaaa
aeaaaaaadiaaaaaiocaabaaaaaaaaaaafgaobaaaaaaaaaaapgipcaaaaaaaaaaa
agaaaaaadcaaaaajocaabaaaaaaaaaaaagajbaaaabaaaaaaagajbaaaadaaaaaa
fgaobaaaaaaaaaaabnaaaaahbcaabaaaabaaaaaaabeaaaaaipmchfdmakaabaaa
aaaaaaaadiaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaafgffifec
dhaaaaajbcaabaaaaaaaaaaaakaabaaaabaaaaaaakaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaaiocaabaaaaaaaaaaafgaobaaaaaaaaaaakgikcaaaaaaaaaaa
agaaaaaadbaaaaakhcaabaaaabaaaaaajgahbaaaaaaaaaaaaceaaaaacpnnledp
cpnnledpcpnnledpaaaaaaaadiaaaaakpcaabaaaacaaaaaafgakbaaaaaaaaaaa
aceaaaaanmcomedodlkklilpnmcomedodlkklilpcpaaaaafgcaabaaaaaaaaaaa
agacbaaaacaaaaaadiaaaaakgcaabaaaaaaaaaaafgagbaaaaaaaaaaaaceaaaaa
aaaaaaaacplkoidocplkoidoaaaaaaaabjaaaaafgcaabaaaaaaaaaaafgagbaaa
aaaaaaaabjaaaaafdcaabaaaacaaaaaangafbaaaacaaaaaaaaaaaaaldcaabaaa
acaaaaaaegaabaiaebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaa
aaaaaaaadhaaaaajdccabaaaaaaaaaaaegaabaaaabaaaaaajgafbaaaaaaaaaaa
egaabaaaacaaaaaadiaaaaakgcaabaaaaaaaaaaapgapbaaaaaaaaaaaaceaaaaa
aaaaaaaanmcomedodlkklilpaaaaaaaacpaaaaafccaabaaaaaaaaaaabkaabaaa
aaaaaaaadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaacplkoido
bjaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaabjaaaaafecaabaaaaaaaaaaa
ckaabaaaaaaaaaaaaaaaaaaiecaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdhaaaaajeccabaaaaaaaaaaackaabaaaabaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaaiiccabaaaaaaaaaaaakaabaaaaaaaaaaa
bkiacaaaaaaaaaaaagaaaaaadoaaaaab"
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