Shader "EVE/Terrain/PQS/Sphere Projection SURFACE QUAD (AP) " {
Properties {
 _saturation ("Saturation", Float) = 1
 _contrast ("Contrast", Float) = 1
 _tintColor ("Colour Unsaturation (A = Factor)", Color) = (1,1,1,0)
 _texTiling ("Near Tiling", Float) = 1000
 _texPower ("Near Blend", Float) = 0.5
 _multiPower ("Far Blend", Float) = 0.5
 _groundTexStart ("NearFar Start", Float) = 2000
 _groundTexEnd ("NearFar Start", Float) = 10000
 _steepTiling ("Steep Tiling", Float) = 1
 _steepPower ("Steep Blend", Float) = 1
 _steepTexStart ("Steep Fade Start", Float) = 20000
 _steepTexEnd ("Steep Fade End", Float) = 30000
 _deepTex ("Deep ground", 2D) = "white" {}
 _deepMultiTex ("Deep MT", 2D) = "white" {}
 _deepMultiFactor ("Deep MT Tiling", Float) = 1
 _mainTex ("Main Texture", 2D) = "white" {}
 _mainMultiTex ("Main MT", 2D) = "white" {}
 _mainMultiFactor ("Main MT Tiling", Float) = 1
 _highTex ("High Ground", 2D) = "white" {}
 _highMultiTex ("High MT", 2D) = "white" {}
 _highMultiFactor ("High MT Tiling", Float) = 1
 _snowTex ("Snow", 2D) = "white" {}
 _snowMultiTex ("Snow MT", 2D) = "white" {}
 _snowMultiFactor ("Snow MT Tiling", Float) = 1
 _steepTex ("Steep Texture", 2D) = "white" {}
 _deepStart ("Deep Start", Float) = 0
 _deepEnd ("Deep End", Float) = 0.3
 _mainLoStart ("Main lower boundary start", Float) = 0
 _mainLoEnd ("Main lower boundary end", Float) = 0.5
 _mainHiStart ("Main upper boundary start", Float) = 0.3
 _mainHiEnd ("Main upper boundary end", Float) = 0.5
 _hiLoStart ("High lower boundary start", Float) = 0.6
 _hiLoEnd ("High lower boundary end", Float) = 0.6
 _hiHiStart ("High upper boundary start", Float) = 0.6
 _hiHiEnd ("High upper boundary end", Float) = 0.9
 _snowStart ("Snow Start", Float) = 0.9
 _snowEnd ("Snow End", Float) = 1
 _fogColor ("AP Fog Color", Color) = (0,0,1,1)
 _heightFallOff ("AP Height Fall Off", Float) = 1
 _globalDensity ("AP Global Density", Float) = 1
 _atmosphereDepth ("AP Atmosphere Depth", Float) = 1
 _fogColorRamp ("FogColorRamp", 2D) = "white" {}
 _PlanetOpacity ("PlanetOpacity", Float) = 1
}
SubShader { 
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardBase" "SHADOWSUPPORT"="true" "RenderType"="Opaque"}
  Blend OneMinusSrcAlpha SrcAlpha
Program "vp" {
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [unity_SHAr]
Vector 14 [unity_SHAg]
Vector 15 [unity_SHAb]
Vector 16 [unity_SHBr]
Vector 17 [unity_SHBg]
Vector 18 [unity_SHBb]
Vector 19 [unity_SHC]
Vector 20 [unity_Scale]
Vector 21 [_tintColor]
Float 22 [_steepPower]
Float 23 [_saturation]
Float 24 [_contrast]
Vector 25 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[27] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..25],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MUL R1.xyz, vertex.normal, c[20].w;
DP3 R3.w, R1, c[10];
DP3 R2.w, R1, c[11];
DP3 R0.w, R1, c[9];
MOV R0.x, R3.w;
MOV R0.y, R2.w;
MOV R0.z, c[0].y;
MUL R1, R0.wxyy, R0.xyyw;
DP4 R2.z, R0.wxyz, c[15];
DP4 R2.y, R0.wxyz, c[14];
DP4 R2.x, R0.wxyz, c[13];
DP4 R0.z, R1, c[18];
DP4 R0.y, R1, c[17];
DP4 R0.x, R1, c[16];
MUL R3.x, R3.w, R3.w;
MAD R1.x, R0.w, R0.w, -R3;
ADD R3.xyz, R2, R0;
MUL R2.xyz, R1.x, c[19];
MOV R1.yz, c[0].x;
DP3 R1.x, vertex.color, c[26];
ADD R0.xyz, vertex.color, -R1;
MAD R1.xyz, R0, c[23].x, R1;
ADD result.texcoord[5].xyz, R3, R2;
MAD R2.xyz, -c[21], c[21].w, R1;
MUL R1.xyz, c[21], c[21].w;
MOV R0.z, vertex.texcoord[1].x;
MOV R0.xy, vertex.texcoord[0];
DP3 R1.w, R0, R0;
RSQ R1.w, R1.w;
MUL R0.xyz, R1.w, R0;
MOV result.texcoord[0].xyz, R0;
ABS result.texcoord[1].xyz, R0;
DP3 result.texcoord[2].w, R0, c[25];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[22];
MIN R0.x, R0, c[0].y;
MAD result.texcoord[3].xyz, R2, c[24].x, R1;
MOV result.texcoord[4].z, R2.w;
MOV result.texcoord[4].y, R3.w;
MOV result.texcoord[4].x, R0.w;
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 47 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [unity_SHAr]
Vector 13 [unity_SHAg]
Vector 14 [unity_SHAb]
Vector 15 [unity_SHBr]
Vector 16 [unity_SHBg]
Vector 17 [unity_SHBb]
Vector 18 [unity_SHC]
Vector 19 [unity_Scale]
Vector 20 [_tintColor]
Float 21 [_steepPower]
Float 22 [_saturation]
Float 23 [_contrast]
Vector 24 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c25, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c26, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mul r1.xyz, v1, c19.w
dp3 r3.w, r1, c9
dp3 r2.w, r1, c10
dp3 r0.w, r1, c8
mov r0.x, r3.w
mov r0.y, r2.w
mov r0.z, c26.x
mul r1, r0.wxyy, r0.xyyw
dp4 r2.z, r0.wxyz, c14
dp4 r2.y, r0.wxyz, c13
dp4 r2.x, r0.wxyz, c12
dp4 r0.z, r1, c17
dp4 r0.y, r1, c16
dp4 r0.x, r1, c15
mul r3.x, r3.w, r3.w
mad r1.x, r0.w, r0.w, -r3
add r3.xyz, r2, r0
mul r2.xyz, r1.x, c18
mov r1.yz, c25.w
dp3 r1.x, v4, c25
add r0.xyz, v4, -r1
mad r1.xyz, r0, c22.x, r1
add o6.xyz, r3, r2
mad r2.xyz, -c20, c20.w, r1
mul r1.xyz, c20, c20.w
mov r0.z, v3.x
mov r0.xy, v2
dp3 r1.w, r0, r0
rsq r1.w, r1.w
mul r0.xyz, r1.w, r0
mov o1.xyz, r0
abs o2.xyz, r0
dp3 o3.w, r0, c24
dp4 r0.x, v0, c2
mad o4.xyz, r2, c23.x, r1
mov o5.z, r2.w
mov o5.y, r3.w
mov o5.x, r0.w
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c21
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 240
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 608 [unity_SHAr]
Vector 624 [unity_SHAg]
Vector 640 [unity_SHAb]
Vector 656 [unity_SHBr]
Vector 672 [unity_SHBg]
Vector 688 [unity_SHBb]
Vector 704 [unity_SHC]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedncdlmlbcjohogamfkenlgkkcfloiofilabaaaaaaaiaiaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcdmagaaaaeaaaabaaipabaaaafjaaaaaeegiocaaaaaaaaaaa
apaaaaaafjaaaaaeegiocaaaabaaaaaacnaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaad
dcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
hccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaa
gfaaaaadhccabaaaafaaaaaagfaaaaadhccabaaaagaaaaaagiaaaaacaeaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaa
aaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaaiiccabaaa
adaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaa
aaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaak
bcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdn
aaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaaegbcbaaa
afaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaiaebaaaaaa
aaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaj
hcaabaaaabaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaa
dcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaa
acaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaa
acaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaaamaaaaaa
agaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaa
acaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaafhccabaaa
afaaaaaaegacbaaaaaaaaaaadgaaaaaficaabaaaaaaaaaaaabeaaaaaaaaaiadp
bbaaaaaibcaabaaaabaaaaaaegiocaaaabaaaaaacgaaaaaaegaobaaaaaaaaaaa
bbaaaaaiccaabaaaabaaaaaaegiocaaaabaaaaaachaaaaaaegaobaaaaaaaaaaa
bbaaaaaiecaabaaaabaaaaaaegiocaaaabaaaaaaciaaaaaaegaobaaaaaaaaaaa
diaaaaahpcaabaaaacaaaaaajgacbaaaaaaaaaaaegakbaaaaaaaaaaabbaaaaai
bcaabaaaadaaaaaaegiocaaaabaaaaaacjaaaaaaegaobaaaacaaaaaabbaaaaai
ccaabaaaadaaaaaaegiocaaaabaaaaaackaaaaaaegaobaaaacaaaaaabbaaaaai
ecaabaaaadaaaaaaegiocaaaabaaaaaaclaaaaaaegaobaaaacaaaaaaaaaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaadiaaaaahccaabaaa
aaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaadcaaaaak
hccabaaaagaaaaaaegiccaaaabaaaaaacmaaaaaaagaabaaaaaaaaaaaegacbaaa
abaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Vector 13 [_tintColor]
Float 14 [_steepPower]
Float 15 [_saturation]
Float 16 [_contrast]
Vector 17 [_sunLightDirection]
Vector 18 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[20] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..18],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[19];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[15].x, R0;
MAD R2.xyz, -c[13], c[13].w, R0;
MUL R0.xyz, c[13], c[13].w;
MAD result.texcoord[3].xyz, R2, c[16].x, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[14];
MIN R0.x, R0, c[0].y;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[17];
MAD result.texcoord[4].xy, vertex.texcoord[1], c[18], c[18].zwzw;
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 26 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Vector 12 [_tintColor]
Float 13 [_steepPower]
Float 14 [_saturation]
Float 15 [_contrast]
Vector 16 [_sunLightDirection]
Vector 17 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
def c18, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c18.w
dp3 r0.x, v4, c18
add r1.xyz, v4, -r0
mad r0.xyz, r1, c14.x, r0
mad r2.xyz, -c12, c12.w, r0
mul r0.xyz, c12, c12.w
mad o4.xyz, r2, c15.x, r0
dp4 r0.x, v0, c2
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c16
mad o5.xy, v3, c17, c17.zwzw
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c13
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 256
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
Vector 240 [unity_LightmapST]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
BindCB  "$Globals" 0
BindCB  "UnityPerDraw" 1
"vs_4_0
eefiecedhpnnbghehiggjjehhkpfneckkmignoieabaaaaaaniafaaaaadaaaaaa
cmaaaaaapeaaaaaakmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheolaaaaaaaagaaaaaa
aiaaaaaajiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaakeaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaakeaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaakeaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklfdeieefcceaeaaaaeaaaabaaajabaaaafjaaaaae
egiocaaaaaaaaaaabaaaaaaafjaaaaaeegiocaaaabaaaaaaaiaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaa
fpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaad
hccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaadhccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaagiaaaaacacaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
abaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaa
aaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaaiiccabaaa
adaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaabaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaabaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaabaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaabaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaa
aaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaak
bcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdn
aaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaaegbcbaaa
afaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaiaebaaaaaa
aaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaj
hcaabaaaabaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaa
dcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaaldccabaaaafaaaaaaegbabaaaaeaaaaaaegiacaaa
aaaaaaaaapaaaaaaogikcaaaaaaaaaaaapaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Vector 13 [_tintColor]
Float 14 [_steepPower]
Float 15 [_saturation]
Float 16 [_contrast]
Vector 17 [_sunLightDirection]
Vector 18 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[20] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..18],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[19];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[15].x, R0;
MAD R2.xyz, -c[13], c[13].w, R0;
MUL R0.xyz, c[13], c[13].w;
MAD result.texcoord[3].xyz, R2, c[16].x, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[14];
MIN R0.x, R0, c[0].y;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[17];
MAD result.texcoord[4].xy, vertex.texcoord[1], c[18], c[18].zwzw;
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 26 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Vector 12 [_tintColor]
Float 13 [_steepPower]
Float 14 [_saturation]
Float 15 [_contrast]
Vector 16 [_sunLightDirection]
Vector 17 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
def c18, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c18.w
dp3 r0.x, v4, c18
add r1.xyz, v4, -r0
mad r0.xyz, r1, c14.x, r0
mad r2.xyz, -c12, c12.w, r0
mul r0.xyz, c12, c12.w
mad o4.xyz, r2, c15.x, r0
dp4 r0.x, v0, c2
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c16
mad o5.xy, v3, c17, c17.zwzw
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c13
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 256
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
Vector 240 [unity_LightmapST]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
BindCB  "$Globals" 0
BindCB  "UnityPerDraw" 1
"vs_4_0
eefiecedhpnnbghehiggjjehhkpfneckkmignoieabaaaaaaniafaaaaadaaaaaa
cmaaaaaapeaaaaaakmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheolaaaaaaaagaaaaaa
aiaaaaaajiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaakeaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaakeaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaakeaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklfdeieefcceaeaaaaeaaaabaaajabaaaafjaaaaae
egiocaaaaaaaaaaabaaaaaaafjaaaaaeegiocaaaabaaaaaaaiaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaa
fpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaad
hccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaadhccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaagiaaaaacacaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
abaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaa
aaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaaiiccabaaa
adaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaabaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaabaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaabaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaabaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaa
aaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaak
bcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdn
aaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaaegbcbaaa
afaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaiaebaaaaaa
aaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaj
hcaabaaaabaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaa
dcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaaldccabaaaafaaaaaaegbabaaaaeaaaaaaegiacaaa
aaaaaaaaapaaaaaaogikcaaaaaaaaaaaapaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_ProjectionParams]
Vector 14 [unity_SHAr]
Vector 15 [unity_SHAg]
Vector 16 [unity_SHAb]
Vector 17 [unity_SHBr]
Vector 18 [unity_SHBg]
Vector 19 [unity_SHBb]
Vector 20 [unity_SHC]
Vector 21 [unity_Scale]
Vector 22 [_tintColor]
Float 23 [_steepPower]
Float 24 [_saturation]
Float 25 [_contrast]
Vector 26 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[28] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..26],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MUL R0.xyz, vertex.normal, c[21].w;
DP3 R3.w, R0, c[10];
DP3 R2.w, R0, c[11];
DP3 R1.w, R0, c[9];
MOV R1.x, R3.w;
MOV R1.y, R2.w;
MOV R1.z, c[0].y;
MUL R0, R1.wxyy, R1.xyyw;
DP4 R2.z, R1.wxyz, c[16];
DP4 R2.y, R1.wxyz, c[15];
DP4 R2.x, R1.wxyz, c[14];
DP4 R1.z, R0, c[19];
DP4 R1.y, R0, c[18];
DP4 R1.x, R0, c[17];
ADD R3.xyz, R2, R1;
MUL R0.x, R3.w, R3.w;
MAD R0.w, R1, R1, -R0.x;
MUL R2.xyz, R0.w, c[20];
MOV R1.yz, c[0].x;
DP3 R1.x, vertex.color, c[27];
ADD R0.xyz, vertex.color, -R1;
MAD R0.xyz, R0, c[24].x, R1;
ADD result.texcoord[5].xyz, R3, R2;
MAD R3.xyz, -c[22], c[22].w, R0;
MUL R1.xyz, c[22], c[22].w;
MAD result.texcoord[3].xyz, R3, c[25].x, R1;
DP4 R0.w, vertex.position, c[8];
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
ADD result.texcoord[6].xy, R2, R2.z;
RSQ R2.x, R0.z;
MUL R1.xyz, R2.x, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[23];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[26];
MOV result.texcoord[6].zw, R0;
MOV result.texcoord[4].z, R2.w;
MOV result.texcoord[4].y, R3.w;
MOV result.texcoord[4].x, R1.w;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 52 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_SHAr]
Vector 15 [unity_SHAg]
Vector 16 [unity_SHAb]
Vector 17 [unity_SHBr]
Vector 18 [unity_SHBg]
Vector 19 [unity_SHBb]
Vector 20 [unity_SHC]
Vector 21 [unity_Scale]
Vector 22 [_tintColor]
Float 23 [_steepPower]
Float 24 [_saturation]
Float 25 [_contrast]
Vector 26 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c27, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c28, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mul r0.xyz, v1, c21.w
dp3 r3.w, r0, c9
dp3 r2.w, r0, c10
dp3 r1.w, r0, c8
mov r1.x, r3.w
mov r1.y, r2.w
mov r1.z, c28.x
mul r0, r1.wxyy, r1.xyyw
dp4 r2.z, r1.wxyz, c16
dp4 r2.y, r1.wxyz, c15
dp4 r2.x, r1.wxyz, c14
dp4 r1.z, r0, c19
dp4 r1.y, r0, c18
dp4 r1.x, r0, c17
add r3.xyz, r2, r1
mul r0.x, r3.w, r3.w
mad r0.w, r1, r1, -r0.x
mul r2.xyz, r0.w, c20
mov r1.yz, c27.w
dp3 r1.x, v4, c27
add r0.xyz, v4, -r1
mad r0.xyz, r0, c24.x, r1
add o6.xyz, r3, r2
mad r3.xyz, -c22, c22.w, r0
mul r1.xyz, c22, c22.w
mad o4.xyz, r3, c25.x, r1
dp4 r0.w, v0, c7
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c28.y
mul r2.y, r2, c12.x
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.z, r1, r1
mad o7.xy, r2.z, c13.zwzw, r2
rsq r2.x, r0.z
mul r1.xyz, r2.x, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c26
mov o7.zw, r0
mov o5.z, r2.w
mov o5.y, r3.w
mov o5.x, r1.w
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c23
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 304
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityLighting" 720
Vector 608 [unity_SHAr]
Vector 624 [unity_SHAg]
Vector 640 [unity_SHAb]
Vector 656 [unity_SHBr]
Vector 672 [unity_SHBg]
Vector 688 [unity_SHBb]
Vector 704 [unity_SHC]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedmehejaaohbmggodemngdfepdlmigdhndabaaaaaamiaiaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
oeagaaaaeaaaabaaljabaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaacnaaaaaafjaaaaae
egiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadhcbabaaa
acaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaad
pcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaad
hccabaaaaeaaaaaagfaaaaadhccabaaaafaaaaaagfaaaaadhccabaaaagaaaaaa
gfaaaaadpccabaaaahaaaaaagiaaaaacafaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaa
baaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
adaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaa
aeaaaaaaakiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaa
afaaaaaabaaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdo
kcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaa
abaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaa
bbaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaa
egiccaiaebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaa
aaaaaaaaahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaa
acaaaaaapgipcaaaadaaaaaabeaaaaaadiaaaaaihcaabaaaacaaaaaafgafbaaa
abaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaaklcaabaaaabaaaaaaegiicaaa
adaaaaaaamaaaaaaagaabaaaabaaaaaaegaibaaaacaaaaaadcaaaaakhcaabaaa
abaaaaaaegiccaaaadaaaaaaaoaaaaaakgakbaaaabaaaaaaegadbaaaabaaaaaa
dgaaaaafhccabaaaafaaaaaaegacbaaaabaaaaaadgaaaaaficaabaaaabaaaaaa
abeaaaaaaaaaiadpbbaaaaaibcaabaaaacaaaaaaegiocaaaacaaaaaacgaaaaaa
egaobaaaabaaaaaabbaaaaaiccaabaaaacaaaaaaegiocaaaacaaaaaachaaaaaa
egaobaaaabaaaaaabbaaaaaiecaabaaaacaaaaaaegiocaaaacaaaaaaciaaaaaa
egaobaaaabaaaaaadiaaaaahpcaabaaaadaaaaaajgacbaaaabaaaaaaegakbaaa
abaaaaaabbaaaaaibcaabaaaaeaaaaaaegiocaaaacaaaaaacjaaaaaaegaobaaa
adaaaaaabbaaaaaiccaabaaaaeaaaaaaegiocaaaacaaaaaackaaaaaaegaobaaa
adaaaaaabbaaaaaiecaabaaaaeaaaaaaegiocaaaacaaaaaaclaaaaaaegaobaaa
adaaaaaaaaaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaegacbaaaaeaaaaaa
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadcaaaaak
bcaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaaabaaaaaabkaabaiaebaaaaaa
abaaaaaadcaaaaakhccabaaaagaaaaaaegiccaaaacaaaaaacmaaaaaaagaabaaa
abaaaaaaegacbaaaacaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
akiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaa
aceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaahaaaaaa
kgaobaaaaaaaaaaaaaaaaaahdccabaaaahaaaaaakgakbaaaabaaaaaamgaabaaa
abaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Vector 13 [_ProjectionParams]
Vector 14 [_tintColor]
Float 15 [_steepPower]
Float 16 [_saturation]
Float 17 [_contrast]
Vector 18 [_sunLightDirection]
Vector 19 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[21] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..19],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
DP4 R0.w, vertex.position, c[8];
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[20];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[16].x, R0;
MAD R3.xyz, -c[14], c[14].w, R0;
MUL R1.xyz, c[14], c[14].w;
MAD result.texcoord[3].xyz, R3, c[17].x, R1;
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[15];
MIN R0.x, R0, c[0].y;
ADD result.texcoord[5].xy, R2, R2.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[18];
MOV result.texcoord[5].zw, R0;
MAD result.texcoord[4].xy, vertex.texcoord[1], c[19], c[19].zwzw;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 31 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [_tintColor]
Float 15 [_steepPower]
Float 16 [_saturation]
Float 17 [_contrast]
Vector 18 [_sunLightDirection]
Vector 19 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c20, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c21, 0.50000000, 0, 0, 0
dcl_position0 v0
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
dp4 r0.w, v0, c7
mov r0.yz, c20.w
dp3 r0.x, v4, c20
add r1.xyz, v4, -r0
mad r0.xyz, r1, c16.x, r0
mad r3.xyz, -c14, c14.w, r0
mul r1.xyz, c14, c14.w
mad o4.xyz, r3, c17.x, r1
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c21.x
mul r2.y, r2, c12.x
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mad o6.xy, r2.z, c13.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c18
mov o6.zw, r0
mad o5.xy, v3, c19, c19.zwzw
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c15
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 320
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedficlckbiiaadfbopnnoefhkinnaalpogabaaaaaajiagaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaadamaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaapaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmmaeaaaaeaaaabaaddabaaaafjaaaaaeegiocaaaaaaaaaaa
beaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
aiaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaa
gfaaaaadpccabaaaagaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaa
baaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaa
aeaaaaaaakiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaa
afaaaaaabaaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdo
kcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaa
abaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaa
bbaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaa
egiccaiaebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaa
aaaaaaaaahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadcaaaaaldccabaaaafaaaaaaegbabaaa
aeaaaaaaegiacaaaaaaaaaaabdaaaaaaogikcaaaaaaaaaaabdaaaaaadiaaaaai
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaak
ncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadp
aaaaaadpdgaaaaafmccabaaaagaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaa
agaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Vector 13 [_ProjectionParams]
Vector 14 [_tintColor]
Float 15 [_steepPower]
Float 16 [_saturation]
Float 17 [_contrast]
Vector 18 [_sunLightDirection]
Vector 19 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[21] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..19],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
DP4 R0.w, vertex.position, c[8];
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[20];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[16].x, R0;
MAD R3.xyz, -c[14], c[14].w, R0;
MUL R1.xyz, c[14], c[14].w;
MAD result.texcoord[3].xyz, R3, c[17].x, R1;
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[15];
MIN R0.x, R0, c[0].y;
ADD result.texcoord[5].xy, R2, R2.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[18];
MOV result.texcoord[5].zw, R0;
MAD result.texcoord[4].xy, vertex.texcoord[1], c[19], c[19].zwzw;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 31 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [_tintColor]
Float 15 [_steepPower]
Float 16 [_saturation]
Float 17 [_contrast]
Vector 18 [_sunLightDirection]
Vector 19 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c20, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c21, 0.50000000, 0, 0, 0
dcl_position0 v0
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
dp4 r0.w, v0, c7
mov r0.yz, c20.w
dp3 r0.x, v4, c20
add r1.xyz, v4, -r0
mad r0.xyz, r1, c16.x, r0
mad r3.xyz, -c14, c14.w, r0
mul r1.xyz, c14, c14.w
mad o4.xyz, r3, c17.x, r1
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c21.x
mul r2.y, r2, c12.x
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mad o6.xy, r2.z, c13.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c18
mov o6.zw, r0
mad o5.xy, v3, c19, c19.zwzw
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c15
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 320
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedficlckbiiaadfbopnnoefhkinnaalpogabaaaaaajiagaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaadamaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaapaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmmaeaaaaeaaaabaaddabaaaafjaaaaaeegiocaaaaaaaaaaa
beaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
aiaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaa
gfaaaaadpccabaaaagaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaa
baaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaa
aeaaaaaaakiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaa
afaaaaaabaaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdo
kcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaa
abaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaa
bbaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaa
egiccaiaebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaa
aaaaaaaaahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadcaaaaaldccabaaaafaaaaaaegbabaaa
aeaaaaaaegiacaaaaaaaaaaabdaaaaaaogikcaaaaaaaaaaabdaaaaaadiaaaaai
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaak
ncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadp
aaaaaadpdgaaaaafmccabaaaagaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaa
agaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [unity_4LightPosX0]
Vector 14 [unity_4LightPosY0]
Vector 15 [unity_4LightPosZ0]
Vector 16 [unity_4LightAtten0]
Vector 17 [unity_LightColor0]
Vector 18 [unity_LightColor1]
Vector 19 [unity_LightColor2]
Vector 20 [unity_LightColor3]
Vector 21 [unity_SHAr]
Vector 22 [unity_SHAg]
Vector 23 [unity_SHAb]
Vector 24 [unity_SHBr]
Vector 25 [unity_SHBg]
Vector 26 [unity_SHBb]
Vector 27 [unity_SHC]
Vector 28 [unity_Scale]
Vector 29 [_tintColor]
Float 30 [_steepPower]
Float 31 [_saturation]
Float 32 [_contrast]
Vector 33 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[35] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..33],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, vertex.normal, c[28].w;
DP3 R4.x, R3, c[9];
DP3 R3.w, R3, c[10];
DP3 R3.x, R3, c[11];
DP4 R0.x, vertex.position, c[10];
ADD R1, -R0.x, c[14];
MUL R2, R3.w, R1;
DP4 R0.x, vertex.position, c[9];
ADD R0, -R0.x, c[13];
MUL R1, R1, R1;
MOV R4.z, R3.x;
MOV R4.w, c[0].y;
MAD R2, R4.x, R0, R2;
DP4 R4.y, vertex.position, c[11];
MAD R1, R0, R0, R1;
ADD R0, -R4.y, c[15];
MAD R1, R0, R0, R1;
MAD R0, R3.x, R0, R2;
MUL R2, R1, c[16];
MOV R4.y, R3.w;
RSQ R1.x, R1.x;
RSQ R1.y, R1.y;
RSQ R1.w, R1.w;
RSQ R1.z, R1.z;
MUL R0, R0, R1;
ADD R1, R2, c[0].y;
DP4 R2.z, R4, c[23];
DP4 R2.y, R4, c[22];
DP4 R2.x, R4, c[21];
RCP R1.x, R1.x;
RCP R1.y, R1.y;
RCP R1.w, R1.w;
RCP R1.z, R1.z;
MAX R0, R0, c[0].x;
MUL R0, R0, R1;
MUL R1.xyz, R0.y, c[18];
MAD R1.xyz, R0.x, c[17], R1;
MAD R0.xyz, R0.z, c[19], R1;
MAD R1.xyz, R0.w, c[20], R0;
MUL R0, R4.xyzz, R4.yzzx;
MUL R1.w, R3, R3;
DP4 R4.w, R0, c[26];
DP4 R4.z, R0, c[25];
DP4 R4.y, R0, c[24];
MAD R1.w, R4.x, R4.x, -R1;
ADD R2.xyz, R2, R4.yzww;
MUL R0.xyz, R1.w, c[27];
ADD R4.yzw, R2.xxyz, R0.xxyz;
ADD result.texcoord[5].xyz, R4.yzww, R1;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[34];
ADD R2.xyz, vertex.color, -R0;
MAD R0.xyz, R2, c[31].x, R0;
MAD R0.xyz, -c[29], c[29].w, R0;
MUL R2.xyz, c[29], c[29].w;
MAD result.texcoord[3].xyz, R0, c[32].x, R2;
DP4 R0.x, vertex.position, c[3];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
MUL R0.y, vertex.texcoord[1], c[30].x;
MOV result.texcoord[2].z, -R0.x;
MIN R0.x, R0.y, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[33];
MOV result.texcoord[4].z, R3.x;
MOV result.texcoord[4].y, R3.w;
MOV result.texcoord[4].x, R4;
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 77 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [unity_4LightPosX0]
Vector 13 [unity_4LightPosY0]
Vector 14 [unity_4LightPosZ0]
Vector 15 [unity_4LightAtten0]
Vector 16 [unity_LightColor0]
Vector 17 [unity_LightColor1]
Vector 18 [unity_LightColor2]
Vector 19 [unity_LightColor3]
Vector 20 [unity_SHAr]
Vector 21 [unity_SHAg]
Vector 22 [unity_SHAb]
Vector 23 [unity_SHBr]
Vector 24 [unity_SHBg]
Vector 25 [unity_SHBb]
Vector 26 [unity_SHC]
Vector 27 [unity_Scale]
Vector 28 [_tintColor]
Float 29 [_steepPower]
Float 30 [_saturation]
Float 31 [_contrast]
Vector 32 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c33, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c34, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mul r3.xyz, v1, c27.w
dp3 r4.x, r3, c8
dp3 r3.w, r3, c9
dp3 r3.x, r3, c10
dp4 r0.x, v0, c9
add r1, -r0.x, c13
mul r2, r3.w, r1
dp4 r0.x, v0, c8
add r0, -r0.x, c12
mul r1, r1, r1
mov r4.z, r3.x
mov r4.w, c34.x
mad r2, r4.x, r0, r2
dp4 r4.y, v0, c10
mad r1, r0, r0, r1
add r0, -r4.y, c14
mad r1, r0, r0, r1
mad r0, r3.x, r0, r2
mul r2, r1, c15
mov r4.y, r3.w
rsq r1.x, r1.x
rsq r1.y, r1.y
rsq r1.w, r1.w
rsq r1.z, r1.z
mul r0, r0, r1
add r1, r2, c34.x
dp4 r2.z, r4, c22
dp4 r2.y, r4, c21
dp4 r2.x, r4, c20
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.w, r1.w
rcp r1.z, r1.z
max r0, r0, c33.w
mul r0, r0, r1
mul r1.xyz, r0.y, c17
mad r1.xyz, r0.x, c16, r1
mad r0.xyz, r0.z, c18, r1
mad r1.xyz, r0.w, c19, r0
mul r0, r4.xyzz, r4.yzzx
mul r1.w, r3, r3
dp4 r4.w, r0, c25
dp4 r4.z, r0, c24
dp4 r4.y, r0, c23
mad r1.w, r4.x, r4.x, -r1
add r2.xyz, r2, r4.yzww
mul r0.xyz, r1.w, c26
add r4.yzw, r2.xxyz, r0.xxyz
add o6.xyz, r4.yzww, r1
mov r0.yz, c33.w
dp3 r0.x, v4, c33
add r2.xyz, v4, -r0
mad r0.xyz, r2, c30.x, r0
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
mad r0.xyz, -c28, c28.w, r0
mul r2.xyz, c28, c28.w
mad o4.xyz, r0, c31.x, r2
dp4 r0.x, v0, c2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c32
mov o5.z, r3.x
mov o5.y, r3.w
mov o5.x, r4
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c29
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 240
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 32 [unity_4LightPosX0]
Vector 48 [unity_4LightPosY0]
Vector 64 [unity_4LightPosZ0]
Vector 80 [unity_4LightAtten0]
Vector 96 [unity_LightColor0]
Vector 112 [unity_LightColor1]
Vector 128 [unity_LightColor2]
Vector 144 [unity_LightColor3]
Vector 160 [unity_LightColor4]
Vector 176 [unity_LightColor5]
Vector 192 [unity_LightColor6]
Vector 208 [unity_LightColor7]
Vector 608 [unity_SHAr]
Vector 624 [unity_SHAg]
Vector 640 [unity_SHAb]
Vector 656 [unity_SHBr]
Vector 672 [unity_SHBg]
Vector 688 [unity_SHBb]
Vector 704 [unity_SHC]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedlapfmfdgddjifpahbbaipcnfjkmgpijfabaaaaaafialaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcimajaaaaeaaaabaagdacaaaafjaaaaaeegiocaaaaaaaaaaa
apaaaaaafjaaaaaeegiocaaaabaaaaaacnaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaad
dcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
hccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaa
gfaaaaadhccabaaaafaaaaaagfaaaaadhccabaaaagaaaaaagiaaaaacagaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaa
aaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaaiiccabaaa
adaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaa
aaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaak
bcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdn
aaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaaegbcbaaa
afaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaiaebaaaaaa
aaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaj
hcaabaaaabaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaa
dcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaa
acaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaa
acaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaaamaaaaaa
agaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaa
acaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaafhccabaaa
afaaaaaaegacbaaaaaaaaaaadgaaaaaficaabaaaaaaaaaaaabeaaaaaaaaaiadp
bbaaaaaibcaabaaaabaaaaaaegiocaaaabaaaaaacgaaaaaaegaobaaaaaaaaaaa
bbaaaaaiccaabaaaabaaaaaaegiocaaaabaaaaaachaaaaaaegaobaaaaaaaaaaa
bbaaaaaiecaabaaaabaaaaaaegiocaaaabaaaaaaciaaaaaaegaobaaaaaaaaaaa
diaaaaahpcaabaaaacaaaaaajgacbaaaaaaaaaaaegakbaaaaaaaaaaabbaaaaai
bcaabaaaadaaaaaaegiocaaaabaaaaaacjaaaaaaegaobaaaacaaaaaabbaaaaai
ccaabaaaadaaaaaaegiocaaaabaaaaaackaaaaaaegaobaaaacaaaaaabbaaaaai
ecaabaaaadaaaaaaegiocaaaabaaaaaaclaaaaaaegaobaaaacaaaaaaaaaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaadiaaaaahicaabaaa
aaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaa
akaabaaaaaaaaaaaakaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaadcaaaaak
hcaabaaaabaaaaaaegiccaaaabaaaaaacmaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaafgbfbaaaaaaaaaaaegiccaaaacaaaaaa
anaaaaaadcaaaaakhcaabaaaacaaaaaaegiccaaaacaaaaaaamaaaaaaagbabaaa
aaaaaaaaegacbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaaegiccaaaacaaaaaa
aoaaaaaakgbkbaaaaaaaaaaaegacbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaa
egiccaaaacaaaaaaapaaaaaapgbpbaaaaaaaaaaaegacbaaaacaaaaaaaaaaaaaj
pcaabaaaadaaaaaafgafbaiaebaaaaaaacaaaaaaegiocaaaabaaaaaaadaaaaaa
diaaaaahpcaabaaaaeaaaaaafgafbaaaaaaaaaaaegaobaaaadaaaaaadiaaaaah
pcaabaaaadaaaaaaegaobaaaadaaaaaaegaobaaaadaaaaaaaaaaaaajpcaabaaa
afaaaaaaagaabaiaebaaaaaaacaaaaaaegiocaaaabaaaaaaacaaaaaaaaaaaaaj
pcaabaaaacaaaaaakgakbaiaebaaaaaaacaaaaaaegiocaaaabaaaaaaaeaaaaaa
dcaaaaajpcaabaaaaeaaaaaaegaobaaaafaaaaaaagaabaaaaaaaaaaaegaobaaa
aeaaaaaadcaaaaajpcaabaaaaaaaaaaaegaobaaaacaaaaaakgakbaaaaaaaaaaa
egaobaaaaeaaaaaadcaaaaajpcaabaaaadaaaaaaegaobaaaafaaaaaaegaobaaa
afaaaaaaegaobaaaadaaaaaadcaaaaajpcaabaaaacaaaaaaegaobaaaacaaaaaa
egaobaaaacaaaaaaegaobaaaadaaaaaaeeaaaaafpcaabaaaadaaaaaaegaobaaa
acaaaaaadcaaaaanpcaabaaaacaaaaaaegaobaaaacaaaaaaegiocaaaabaaaaaa
afaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpaoaaaaakpcaabaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpegaobaaaacaaaaaa
diaaaaahpcaabaaaaaaaaaaaegaobaaaaaaaaaaaegaobaaaadaaaaaadeaaaaak
pcaabaaaaaaaaaaaegaobaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaadiaaaaahpcaabaaaaaaaaaaaegaobaaaacaaaaaaegaobaaaaaaaaaaa
diaaaaaihcaabaaaacaaaaaafgafbaaaaaaaaaaaegiccaaaabaaaaaaahaaaaaa
dcaaaaakhcaabaaaacaaaaaaegiccaaaabaaaaaaagaaaaaaagaabaaaaaaaaaaa
egacbaaaacaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaabaaaaaaaiaaaaaa
kgakbaaaaaaaaaaaegacbaaaacaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaa
abaaaaaaajaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaahhccabaaa
agaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_ProjectionParams]
Vector 14 [unity_4LightPosX0]
Vector 15 [unity_4LightPosY0]
Vector 16 [unity_4LightPosZ0]
Vector 17 [unity_4LightAtten0]
Vector 18 [unity_LightColor0]
Vector 19 [unity_LightColor1]
Vector 20 [unity_LightColor2]
Vector 21 [unity_LightColor3]
Vector 22 [unity_SHAr]
Vector 23 [unity_SHAg]
Vector 24 [unity_SHAb]
Vector 25 [unity_SHBr]
Vector 26 [unity_SHBg]
Vector 27 [unity_SHBb]
Vector 28 [unity_SHC]
Vector 29 [unity_Scale]
Vector 30 [_tintColor]
Float 31 [_steepPower]
Float 32 [_saturation]
Float 33 [_contrast]
Vector 34 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[36] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..34],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
MUL R3.xyz, vertex.normal, c[29].w;
DP3 R5.x, R3, c[11];
DP3 R4.x, R3, c[9];
DP3 R3.w, R3, c[10];
DP4 R0.x, vertex.position, c[10];
ADD R1, -R0.x, c[15];
MUL R2, R3.w, R1;
DP4 R0.x, vertex.position, c[9];
ADD R0, -R0.x, c[14];
MUL R1, R1, R1;
MOV R4.z, R5.x;
MOV R4.w, c[0].y;
MAD R2, R4.x, R0, R2;
DP4 R4.y, vertex.position, c[11];
MAD R1, R0, R0, R1;
ADD R0, -R4.y, c[16];
MAD R1, R0, R0, R1;
MAD R0, R5.x, R0, R2;
MUL R2, R1, c[17];
MOV R4.y, R3.w;
RSQ R1.x, R1.x;
RSQ R1.y, R1.y;
RSQ R1.w, R1.w;
RSQ R1.z, R1.z;
MUL R0, R0, R1;
ADD R1, R2, c[0].y;
DP4 R2.z, R4, c[24];
DP4 R2.y, R4, c[23];
DP4 R2.x, R4, c[22];
RCP R1.x, R1.x;
RCP R1.y, R1.y;
RCP R1.w, R1.w;
RCP R1.z, R1.z;
MAX R0, R0, c[0].x;
MUL R0, R0, R1;
MUL R1.xyz, R0.y, c[19];
MAD R1.xyz, R0.x, c[18], R1;
MAD R0.xyz, R0.z, c[20], R1;
MUL R1, R4.xyzz, R4.yzzx;
MAD R0.xyz, R0.w, c[21], R0;
MUL R0.w, R3, R3;
MAD R0.w, R4.x, R4.x, -R0;
MUL R4.yzw, R0.w, c[28].xxyz;
DP4 R3.z, R1, c[27];
DP4 R3.y, R1, c[26];
DP4 R3.x, R1, c[25];
ADD R3.xyz, R2, R3;
ADD R3.xyz, R3, R4.yzww;
ADD result.texcoord[5].xyz, R3, R0;
DP4 R0.w, vertex.position, c[8];
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R3.xyz, R0.xyww, c[0].z;
MOV R1.yz, c[0].x;
DP3 R1.x, vertex.color, c[35];
ADD R2.xyz, vertex.color, -R1;
MAD R1.xyz, R2, c[32].x, R1;
MAD R1.xyz, -c[30], c[30].w, R1;
MUL R2.xyz, c[30], c[30].w;
MAD result.texcoord[3].xyz, R1, c[33].x, R2;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.x, vertex.position, c[3];
MOV R2.x, R3;
MUL R2.y, R3, c[13].x;
MUL R0.y, vertex.texcoord[1], c[31].x;
MOV result.texcoord[2].z, -R0.x;
MIN R0.x, R0.y, c[0].y;
ADD result.texcoord[6].xy, R2, R3.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[34];
MOV result.texcoord[6].zw, R0;
MOV result.texcoord[4].z, R5.x;
MOV result.texcoord[4].y, R3.w;
MOV result.texcoord[4].x, R4;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 83 instructions, 6 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_4LightPosX0]
Vector 15 [unity_4LightPosY0]
Vector 16 [unity_4LightPosZ0]
Vector 17 [unity_4LightAtten0]
Vector 18 [unity_LightColor0]
Vector 19 [unity_LightColor1]
Vector 20 [unity_LightColor2]
Vector 21 [unity_LightColor3]
Vector 22 [unity_SHAr]
Vector 23 [unity_SHAg]
Vector 24 [unity_SHAb]
Vector 25 [unity_SHBr]
Vector 26 [unity_SHBg]
Vector 27 [unity_SHBb]
Vector 28 [unity_SHC]
Vector 29 [unity_Scale]
Vector 30 [_tintColor]
Float 31 [_steepPower]
Float 32 [_saturation]
Float 33 [_contrast]
Vector 34 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c35, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c36, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mul r3.xyz, v1, c29.w
dp3 r5.x, r3, c10
dp3 r4.x, r3, c8
dp3 r3.w, r3, c9
dp4 r0.x, v0, c9
add r1, -r0.x, c15
mul r2, r3.w, r1
dp4 r0.x, v0, c8
add r0, -r0.x, c14
mul r1, r1, r1
mov r4.z, r5.x
mov r4.w, c36.x
mad r2, r4.x, r0, r2
dp4 r4.y, v0, c10
mad r1, r0, r0, r1
add r0, -r4.y, c16
mad r1, r0, r0, r1
mad r0, r5.x, r0, r2
mul r2, r1, c17
mov r4.y, r3.w
rsq r1.x, r1.x
rsq r1.y, r1.y
rsq r1.w, r1.w
rsq r1.z, r1.z
mul r0, r0, r1
add r1, r2, c36.x
dp4 r2.z, r4, c24
dp4 r2.y, r4, c23
dp4 r2.x, r4, c22
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.w, r1.w
rcp r1.z, r1.z
max r0, r0, c35.w
mul r0, r0, r1
mul r1.xyz, r0.y, c19
mad r1.xyz, r0.x, c18, r1
mad r0.xyz, r0.z, c20, r1
mul r1, r4.xyzz, r4.yzzx
mad r0.xyz, r0.w, c21, r0
mul r0.w, r3, r3
mad r0.w, r4.x, r4.x, -r0
mul r4.yzw, r0.w, c28.xxyz
dp4 r3.z, r1, c27
dp4 r3.y, r1, c26
dp4 r3.x, r1, c25
add r3.xyz, r2, r3
add r3.xyz, r3, r4.yzww
add o6.xyz, r3, r0
dp4 r0.w, v0, c7
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r3.xyz, r0.xyww, c36.y
mov r1.yz, c35.w
dp3 r1.x, v4, c35
add r2.xyz, v4, -r1
mad r1.xyz, r2, c32.x, r1
mad r1.xyz, -c30, c30.w, r1
mul r2.xyz, c30, c30.w
mad o4.xyz, r1, c33.x, r2
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mov r2.x, r3
mul r2.y, r3, c12.x
mad o7.xy, r3.z, c13.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c34
mov o7.zw, r0
mov o5.z, r5.x
mov o5.y, r3.w
mov o5.x, r4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c31
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 304
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityLighting" 720
Vector 32 [unity_4LightPosX0]
Vector 48 [unity_4LightPosY0]
Vector 64 [unity_4LightPosZ0]
Vector 80 [unity_4LightAtten0]
Vector 96 [unity_LightColor0]
Vector 112 [unity_LightColor1]
Vector 128 [unity_LightColor2]
Vector 144 [unity_LightColor3]
Vector 160 [unity_LightColor4]
Vector 176 [unity_LightColor5]
Vector 192 [unity_LightColor6]
Vector 208 [unity_LightColor7]
Vector 608 [unity_SHAr]
Vector 624 [unity_SHAg]
Vector 640 [unity_SHAb]
Vector 656 [unity_SHBr]
Vector 672 [unity_SHBg]
Vector 688 [unity_SHBb]
Vector 704 [unity_SHC]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefieceddkklgdmdooelocfbicgkgekgiffkijgaabaaaaaabiamaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
deakaaaaeaaaabaainacaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaacnaaaaaafjaaaaae
egiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadhcbabaaa
acaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaad
pcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaad
hccabaaaaeaaaaaagfaaaaadhccabaaaafaaaaaagfaaaaadhccabaaaagaaaaaa
gfaaaaadpccabaaaahaaaaaagiaaaaacahaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaa
baaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
adaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaa
aeaaaaaaakiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaa
afaaaaaabaaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdo
kcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaa
abaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaa
bbaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaa
egiccaiaebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaa
aaaaaaaaahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaa
acaaaaaapgipcaaaadaaaaaabeaaaaaadiaaaaaihcaabaaaacaaaaaafgafbaaa
abaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaaklcaabaaaabaaaaaaegiicaaa
adaaaaaaamaaaaaaagaabaaaabaaaaaaegaibaaaacaaaaaadcaaaaakhcaabaaa
abaaaaaaegiccaaaadaaaaaaaoaaaaaakgakbaaaabaaaaaaegadbaaaabaaaaaa
dgaaaaafhccabaaaafaaaaaaegacbaaaabaaaaaadgaaaaaficaabaaaabaaaaaa
abeaaaaaaaaaiadpbbaaaaaibcaabaaaacaaaaaaegiocaaaacaaaaaacgaaaaaa
egaobaaaabaaaaaabbaaaaaiccaabaaaacaaaaaaegiocaaaacaaaaaachaaaaaa
egaobaaaabaaaaaabbaaaaaiecaabaaaacaaaaaaegiocaaaacaaaaaaciaaaaaa
egaobaaaabaaaaaadiaaaaahpcaabaaaadaaaaaajgacbaaaabaaaaaaegakbaaa
abaaaaaabbaaaaaibcaabaaaaeaaaaaaegiocaaaacaaaaaacjaaaaaaegaobaaa
adaaaaaabbaaaaaiccaabaaaaeaaaaaaegiocaaaacaaaaaackaaaaaaegaobaaa
adaaaaaabbaaaaaiecaabaaaaeaaaaaaegiocaaaacaaaaaaclaaaaaaegaobaaa
adaaaaaaaaaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaegacbaaaaeaaaaaa
diaaaaahicaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadcaaaaak
icaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaaabaaaaaadkaabaiaebaaaaaa
abaaaaaadcaaaaakhcaabaaaacaaaaaaegiccaaaacaaaaaacmaaaaaapgapbaaa
abaaaaaaegacbaaaacaaaaaadiaaaaaihcaabaaaadaaaaaafgbfbaaaaaaaaaaa
egiccaaaadaaaaaaanaaaaaadcaaaaakhcaabaaaadaaaaaaegiccaaaadaaaaaa
amaaaaaaagbabaaaaaaaaaaaegacbaaaadaaaaaadcaaaaakhcaabaaaadaaaaaa
egiccaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaaegacbaaaadaaaaaadcaaaaak
hcaabaaaadaaaaaaegiccaaaadaaaaaaapaaaaaapgbpbaaaaaaaaaaaegacbaaa
adaaaaaaaaaaaaajpcaabaaaaeaaaaaafgafbaiaebaaaaaaadaaaaaaegiocaaa
acaaaaaaadaaaaaadiaaaaahpcaabaaaafaaaaaafgafbaaaabaaaaaaegaobaaa
aeaaaaaadiaaaaahpcaabaaaaeaaaaaaegaobaaaaeaaaaaaegaobaaaaeaaaaaa
aaaaaaajpcaabaaaagaaaaaaagaabaiaebaaaaaaadaaaaaaegiocaaaacaaaaaa
acaaaaaaaaaaaaajpcaabaaaadaaaaaakgakbaiaebaaaaaaadaaaaaaegiocaaa
acaaaaaaaeaaaaaadcaaaaajpcaabaaaafaaaaaaegaobaaaagaaaaaaagaabaaa
abaaaaaaegaobaaaafaaaaaadcaaaaajpcaabaaaabaaaaaaegaobaaaadaaaaaa
kgakbaaaabaaaaaaegaobaaaafaaaaaadcaaaaajpcaabaaaaeaaaaaaegaobaaa
agaaaaaaegaobaaaagaaaaaaegaobaaaaeaaaaaadcaaaaajpcaabaaaadaaaaaa
egaobaaaadaaaaaaegaobaaaadaaaaaaegaobaaaaeaaaaaaeeaaaaafpcaabaaa
aeaaaaaaegaobaaaadaaaaaadcaaaaanpcaabaaaadaaaaaaegaobaaaadaaaaaa
egiocaaaacaaaaaaafaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
aoaaaaakpcaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
egaobaaaadaaaaaadiaaaaahpcaabaaaabaaaaaaegaobaaaabaaaaaaegaobaaa
aeaaaaaadeaaaaakpcaabaaaabaaaaaaegaobaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadiaaaaahpcaabaaaabaaaaaaegaobaaaadaaaaaa
egaobaaaabaaaaaadiaaaaaihcaabaaaadaaaaaafgafbaaaabaaaaaaegiccaaa
acaaaaaaahaaaaaadcaaaaakhcaabaaaadaaaaaaegiccaaaacaaaaaaagaaaaaa
agaabaaaabaaaaaaegacbaaaadaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaa
acaaaaaaaiaaaaaakgakbaaaabaaaaaaegacbaaaadaaaaaadcaaaaakhcaabaaa
abaaaaaaegiccaaaacaaaaaaajaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaa
aaaaaaahhccabaaaagaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaadiaaaaai
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaak
ncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadp
aaaaaadpdgaaaaafmccabaaaahaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaa
ahaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_WorldSpaceLightPos0]
Vector 1 [_LightColor0]
Float 2 [_texTiling]
Float 3 [_texPower]
Float 4 [_groundTexStart]
Float 5 [_groundTexEnd]
Float 6 [_steepTiling]
Float 7 [_steepTexStart]
Float 8 [_steepTexEnd]
Float 9 [_multiPower]
Float 10 [_deepMultiFactor]
Float 11 [_mainMultiFactor]
Float 12 [_highMultiFactor]
Float 13 [_snowMultiFactor]
Float 14 [_deepStart]
Float 15 [_deepEnd]
Float 16 [_mainLoStart]
Float 17 [_mainLoEnd]
Float 18 [_mainHiStart]
Float 19 [_mainHiEnd]
Float 20 [_hiLoStart]
Float 21 [_hiLoEnd]
Float 22 [_hiHiStart]
Float 23 [_hiHiEnd]
Float 24 [_snowStart]
Float 25 [_snowEnd]
Float 26 [_heightDensityAtViewer]
Float 27 [_globalDensity]
Float 28 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
"3.0-!!ARBfp1.0
PARAM c[31] = { program.local[0..28],
		{ 0, 2.718282, 1, 0.5 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[4];
ADD R0.x, -R0, c[5];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[4];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[2].x;
MAD R1.x, -R0.w, c[30], c[30].y;
MUL R0.w, R0, R0;
MOV R2.w, c[14].x;
ADD R2.w, -R2, c[15].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[14].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[29].z;
MOV R0.z, c[18].x;
ADD R0.z, -R0, c[19].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[18].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[16].x;
ADD R0.y, -R0, c[17].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[16].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[9].x;
MAD R2.x, -R1.w, c[30], c[30].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[30].x, c[30];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[3].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[30].x, c[30];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[29].z;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[22].x;
ADD R0.y, -R0, c[23].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[22].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[20];
ADD R0.x, -R0, c[21];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[30].x, c[30].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[30], c[30].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[24].x;
ADD R3.y, -R3, c[25].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[24];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[30].x, c[30];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[10].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[13].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[7].x;
ADD R0.w, -R0, c[8].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[7].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[6].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MUL_SAT R0.w, R0, R1;
MAD R1.x, -R0.w, c[30], c[30].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[29].z;
ADD R1.xyz, R0, -R2;
MUL R0.y, fragment.texcoord[2].x, R0.w;
MAD R1.xyz, R0.y, R1, R2;
MUL R0.x, fragment.texcoord[2].z, c[27];
MUL R0.w, R0.x, c[26].x;
POW R0.w, c[29].y, R0.w;
MOV R0.y, c[29].w;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
ADD R0.x, -R0.w, c[29].z;
MAD R1.xyz, R0.x, R2, R1;
MUL R0.xyz, R1, fragment.texcoord[5];
DP3 R0.w, fragment.texcoord[4], c[0];
MUL R1.xyz, R1, c[1];
MAX R0.w, R0, c[29].x;
MUL R1.xyz, R0.w, R1;
MAD result.color.xyz, R1, c[30].x, R0;
MOV result.color.w, c[28].x;
END
# 165 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_WorldSpaceLightPos0]
Vector 1 [_LightColor0]
Float 2 [_texTiling]
Float 3 [_texPower]
Float 4 [_groundTexStart]
Float 5 [_groundTexEnd]
Float 6 [_steepTiling]
Float 7 [_steepTexStart]
Float 8 [_steepTexEnd]
Float 9 [_multiPower]
Float 10 [_deepMultiFactor]
Float 11 [_mainMultiFactor]
Float 12 [_highMultiFactor]
Float 13 [_snowMultiFactor]
Float 14 [_deepStart]
Float 15 [_deepEnd]
Float 16 [_mainLoStart]
Float 17 [_mainLoEnd]
Float 18 [_mainHiStart]
Float 19 [_mainHiEnd]
Float 20 [_hiLoStart]
Float 21 [_hiLoEnd]
Float 22 [_hiHiStart]
Float 23 [_hiHiEnd]
Float 24 [_snowStart]
Float 25 [_snowEnd]
Float 26 [_heightDensityAtViewer]
Float 27 [_globalDensity]
Float 28 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
def c29, 0.00000000, 2.71828198, 1.00000000, 0.50000000
def c30, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
mov r0.x, c5
add r0.w, -c4.x, r0.x
mul r0.xyz, v0, c2.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c4.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c30.x, c30.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c29.z
mov r1.z, c19.x
add r1.z, -c18.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c18.x
mul_sat r1.w, r1.z, r1
mov r1.y, c17.x
add r1.y, -c16.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c16.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c9.x
mad r2.w, -r1, c30.x, c30.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c30.x, c30
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c3.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c15.x
add r2.w, -c14.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c14.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c30.x, c30
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c29.z
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c23.x
add r1.y, -c22.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c22.x
mul_sat r1.z, r1.y, r1
mov r1.x, c21
add r1.x, -c20, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c20
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c30.x, c30.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c30, c30.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c25.x
add r3.x, -c24, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c24
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c30.x, c30
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c10.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c11.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c13.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c6.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mul r0.x, v2.z, c27
mul r1.w, r0.x, c26.x
mov r0.y, c8.x
add r0.y, -c7.x, r0
rcp r0.y, r0.y
add r0.x, v2.z, -c7
mul_sat r2.x, r0, r0.y
pow r0, c29.y, r1.w
mad r0.z, -r2.x, c30.x, c30.y
mul r0.y, r2.x, r2.x
mad r0.y, -r0, r0.z, c29.z
mov r0.w, r0.x
mul r0.y, v2.x, r0
mad r1.xyz, r0.y, r1, r3
mov r0.y, c29.w
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r2.xyz, r0, -r1
add r0.x, -r0.w, c29.z
mad r1.xyz, r0.x, r2, r1
mul_pp r0.xyz, r1, v5
dp3_pp r0.w, v4, c0
mul_pp r1.xyz, r1, c1
max_pp r0.w, r0, c29.x
mul_pp r1.xyz, r0.w, r1
mad_pp oC0.xyz, r1, c30.x, r0
mov_pp oC0.w, c28.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
ConstBuffer "$Globals" 240
Vector 16 [_LightColor0]
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
"ps_4_0
eefiecedinmcjdaiknnalkehpckbimfobalpegkhabaaaaaadibhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcaabgaaaaeaaaaaaaiaafaaaa
fjaaaaaeegiocaaaaaaaaaaaaoaaaaaafjaaaaaeegiocaaaabaaaaaaabaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaad
aagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaadaagabaaaahaaaaaa
fkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaa
acaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaaeaahabaaa
aeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaaeaahabaaa
agaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaaeaahabaaa
aiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaagcbaaaadhcbabaaa
abaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaad
hcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagcbaaaadhcbabaaaagaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaakbcaabaaaaaaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaaaiaaaaaaaoaaaaak
bcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
aaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaadkiacaiaebaaaaaa
aaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaakccaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaaaaaaaaaaaaaaaaj
ecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaaaaaaaaaaaeaaaaaa
dicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaaaaaaaaaadcaaaaaj
ecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaaaaaaaaaa
diaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaaaaaaaaaaagaaaaaa
diaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaadkaabaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaaaaaaaaaaakaabaaa
aaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaa
aaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaaaiaaaaaaakiacaaa
aaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaaabaaaaaafgbfbaaa
adaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaahccaabaaaabaaaaaa
bkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaabkaabaaa
abaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaabaaaaaa
bkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaa
abaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaabkiacaiaebaaaaaa
aaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaakicaabaaaabaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaabaaaaaadicaaaah
ecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaadcaaaaakccaabaaa
abaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaiaebaaaaaaabaaaaaa
diaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaaabaaaaaadiaaaaah
ccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaadiaaaaaihcaabaaa
acaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaaj
hcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaafgbfbaaaacaaaaaa
egacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaaabaaaaaaegacbaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaaaeaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaaafaaaaaakgbkbaaa
acaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaajncaabaaaabaaaaaa
agajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaaaaaaaaakicaabaaa
acaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaaaaaaaaaaakaaaaaa
aoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaaadaaaaaangifcaia
ebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
bkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
dkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaa
adaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaajaaaaaa
ckiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaahbcaabaaaadaaaaaa
bkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaaakaabaaa
adaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaadaaaaaa
akaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaaacaaaaaabkaabaaa
adaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaadiaaaaahbcaabaaa
adaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaa
bkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaa
acaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaahocaabaaaadaaaaaa
agajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaajocaabaaaadaaaaaa
agajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaaefaaaaajpcaabaaa
aeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaafgaobaaaadaaaaaa
dcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaaadaaaaaaagaobaaa
abaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
ckiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaajccaabaaaadaaaaaa
bkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaadicaaaahbcaabaaa
adaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaa
akaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaa
adaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaahbcaabaaaadaaaaaa
akaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaaaaaaaaaafgagbaaa
aaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaa
acaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaaagaobaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaaaaaaaaaaagaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaaeaaaaaaaagabaaa
abaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaaeaaaaaaaagabaaa
abaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaaeaaaaaa
aagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaa
acaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaa
egbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgafbaaaabaaaaaa
igadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaafgifcaaa
aaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
agaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
agaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaaaagabaaaahaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaaaagabaaaahaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaaahaaaaaaaagabaaa
ahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaacaaaaaafgbfbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaacaaaaaa
fgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
aeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaadcaaaaajhcaabaaa
aaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaaaeaaaaaadiaaaaai
hcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaaafaaaaaaefaaaaaj
pcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaaaagabaaaaiaaaaaa
diaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaaaagabaaaaiaaaaaa
efaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaaaiaaaaaaaagabaaa
aiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaakgbkbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaabaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
icaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaaaaaaaaaaafaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaabaaaaaadcaaaaaj
icaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakbabaaaadaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaadgaaaaafccaabaaa
abaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaaaaaaaaaackbabaaa
adaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaaaaaaaaaadkaabaaa
aaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
aaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadiaaaaaihcaabaaaabaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaa
abaaaaaadiaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaaagaaaaaa
baaaaaaiicaabaaaaaaaaaaaegbcbaaaafaaaaaaegiccaaaabaaaaaaaaaaaaaa
deaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaaaaaaaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaajhccabaaa
aaaaaaaaegacbaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaag
iccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [unity_Lightmap] 2D 10
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R1.w, c[27].x, R0.w;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
TEX R0, fragment.texcoord[4], texture[10], 2D;
ADD R1.w, -R1, c[27].y;
MAD R1.xyz, R1.w, R2, R1;
MUL R0.xyz, R0.w, R0;
MUL R0.xyz, R0, R1;
MUL result.color.xyz, R0, c[28].y;
MOV result.color.w, c[26].x;
END
# 171 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [unity_Lightmap] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c27, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xy
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
mov r0.w, c6.x
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c5.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c5.x
mul_sat r0.w, r0, r1.x
add r1.xyz, r0, -r3
mul r0.x, v2.z, c25
mul r1.w, r0.x, c24.x
mul r0.y, r0.w, r0.w
mad r0.z, -r0.w, c28.x, c28.y
mad r2.x, -r0.y, r0.z, c27.y
pow r0, c27.x, r1.w
mul r0.y, v2.x, r2.x
mov r1.w, r0.x
mad r1.xyz, r0.y, r1, r3
texld r0, v4, s10
mov r2.y, c27.z
mov r2.x, v2.w
texld r2.xyz, r2, s9
add r2.xyz, r2, -r1
add r1.w, -r1, c27.y
mad r1.xyz, r1.w, r2, r1
mul_pp r0.xyz, r0.w, r0
mul_pp r0.xyz, r0, r1
mul_pp oC0.xyz, r0, c27.w
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [unity_Lightmap] 2D 10
ConstBuffer "$Globals" 256
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedejeanjkgkiplelmmbghbiakdbcaadflkabaaaaaaoabgaaaaadaaaaaa
cmaaaaaaoeaaaaaabiabaaaaejfdeheolaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaakeaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaakeaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmabfaaaa
eaaaaaaahaafaaaafjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaa
fkaaaaadaagabaaaagaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaa
aiaaaaaafkaaaaadaagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaae
aahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaae
aahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaae
aahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaae
aahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaae
aahabaaaakaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaa
acaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaad
dcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaa
aiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
aiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaa
aaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
afaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaafaaaaaa
eghobaaaakaaaaaaaagabaaaakaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaa
abaaaaaaabeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgapbaaaaaaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
abaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaadoaaaaab
"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [unity_Lightmap] 2D 10
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R1.w, c[27].x, R0.w;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
TEX R0, fragment.texcoord[4], texture[10], 2D;
ADD R1.w, -R1, c[27].y;
MAD R1.xyz, R1.w, R2, R1;
MUL R0.xyz, R0.w, R0;
MUL R0.xyz, R0, R1;
MUL result.color.xyz, R0, c[28].y;
MOV result.color.w, c[26].x;
END
# 171 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [unity_Lightmap] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c27, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xy
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
mov r0.w, c6.x
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c5.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c5.x
mul_sat r0.w, r0, r1.x
add r1.xyz, r0, -r3
mul r0.x, v2.z, c25
mul r1.w, r0.x, c24.x
mul r0.y, r0.w, r0.w
mad r0.z, -r0.w, c28.x, c28.y
mad r2.x, -r0.y, r0.z, c27.y
pow r0, c27.x, r1.w
mul r0.y, v2.x, r2.x
mov r1.w, r0.x
mad r1.xyz, r0.y, r1, r3
texld r0, v4, s10
mov r2.y, c27.z
mov r2.x, v2.w
texld r2.xyz, r2, s9
add r2.xyz, r2, -r1
add r1.w, -r1, c27.y
mad r1.xyz, r1.w, r2, r1
mul_pp r0.xyz, r0.w, r0
mul_pp r0.xyz, r0, r1
mul_pp oC0.xyz, r0, c27.w
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [unity_Lightmap] 2D 10
ConstBuffer "$Globals" 256
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedejeanjkgkiplelmmbghbiakdbcaadflkabaaaaaaoabgaaaaadaaaaaa
cmaaaaaaoeaaaaaabiabaaaaejfdeheolaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaakeaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaakeaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmabfaaaa
eaaaaaaahaafaaaafjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaa
fkaaaaadaagabaaaagaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaa
aiaaaaaafkaaaaadaagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaae
aahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaae
aahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaae
aahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaae
aahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaae
aahabaaaakaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaa
acaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaad
dcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaa
aiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
aiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaa
aaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
afaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaafaaaaaa
eghobaaaakaaaaaaaagabaaaakaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaa
abaaaaaaabeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgapbaaaaaaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
abaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaadoaaaaab
"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_WorldSpaceLightPos0]
Vector 1 [_LightColor0]
Float 2 [_texTiling]
Float 3 [_texPower]
Float 4 [_groundTexStart]
Float 5 [_groundTexEnd]
Float 6 [_steepTiling]
Float 7 [_steepTexStart]
Float 8 [_steepTexEnd]
Float 9 [_multiPower]
Float 10 [_deepMultiFactor]
Float 11 [_mainMultiFactor]
Float 12 [_highMultiFactor]
Float 13 [_snowMultiFactor]
Float 14 [_deepStart]
Float 15 [_deepEnd]
Float 16 [_mainLoStart]
Float 17 [_mainLoEnd]
Float 18 [_mainHiStart]
Float 19 [_mainHiEnd]
Float 20 [_hiLoStart]
Float 21 [_hiLoEnd]
Float 22 [_hiHiStart]
Float 23 [_hiHiEnd]
Float 24 [_snowStart]
Float 25 [_snowEnd]
Float 26 [_heightDensityAtViewer]
Float 27 [_globalDensity]
Float 28 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_ShadowMapTexture] 2D 10
"3.0-!!ARBfp1.0
PARAM c[31] = { program.local[0..28],
		{ 0, 2.718282, 1, 0.5 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[4];
ADD R0.x, -R0, c[5];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[4];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[2].x;
MAD R1.x, -R0.w, c[30], c[30].y;
MUL R0.w, R0, R0;
MOV R2.w, c[14].x;
ADD R2.w, -R2, c[15].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[14].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[29].z;
MOV R0.z, c[18].x;
ADD R0.z, -R0, c[19].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[18].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[16].x;
ADD R0.y, -R0, c[17].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[16].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[9].x;
MAD R2.x, -R1.w, c[30], c[30].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[30].x, c[30];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[3].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[30].x, c[30];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[29].z;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[22].x;
ADD R0.y, -R0, c[23].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[22].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[20];
ADD R0.x, -R0, c[21];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[30].x, c[30].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[30], c[30].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[24].x;
ADD R3.y, -R3, c[25].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[24];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[30].x, c[30];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[10].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[13].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[7].x;
ADD R0.w, -R0, c[8].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[7].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[6].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MUL_SAT R0.w, R0, R1;
MAD R1.x, -R0.w, c[30], c[30].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[29].z;
ADD R1.xyz, R0, -R2;
MUL R0.y, fragment.texcoord[2].x, R0.w;
MAD R1.xyz, R0.y, R1, R2;
MUL R0.x, fragment.texcoord[2].z, c[27];
MUL R0.w, R0.x, c[26].x;
MOV R0.y, c[29].w;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
POW R0.w, c[29].y, R0.w;
ADD R0.x, -R0.w, c[29].z;
MAD R0.xyz, R0.x, R2, R1;
MUL R2.xyz, R0, fragment.texcoord[5];
MUL R1.xyz, R0, c[1];
DP3 R0.y, fragment.texcoord[4], c[0];
MAX R0.y, R0, c[29].x;
TXP R0.x, fragment.texcoord[6], texture[10], 2D;
MUL R0.x, R0.y, R0;
MUL R0.xyz, R0.x, R1;
MAD result.color.xyz, R0, c[30].x, R2;
MOV result.color.w, c[28].x;
END
# 167 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_WorldSpaceLightPos0]
Vector 1 [_LightColor0]
Float 2 [_texTiling]
Float 3 [_texPower]
Float 4 [_groundTexStart]
Float 5 [_groundTexEnd]
Float 6 [_steepTiling]
Float 7 [_steepTexStart]
Float 8 [_steepTexEnd]
Float 9 [_multiPower]
Float 10 [_deepMultiFactor]
Float 11 [_mainMultiFactor]
Float 12 [_highMultiFactor]
Float 13 [_snowMultiFactor]
Float 14 [_deepStart]
Float 15 [_deepEnd]
Float 16 [_mainLoStart]
Float 17 [_mainLoEnd]
Float 18 [_mainHiStart]
Float 19 [_mainHiEnd]
Float 20 [_hiLoStart]
Float 21 [_hiLoEnd]
Float 22 [_hiHiStart]
Float 23 [_hiHiEnd]
Float 24 [_snowStart]
Float 25 [_snowEnd]
Float 26 [_heightDensityAtViewer]
Float 27 [_globalDensity]
Float 28 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_ShadowMapTexture] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c29, 0.00000000, 2.71828198, 1.00000000, 0.50000000
def c30, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
dcl_texcoord6 v6
mov r0.x, c5
add r0.w, -c4.x, r0.x
mul r0.xyz, v0, c2.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c4.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c30.x, c30.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c29.z
mov r1.z, c19.x
add r1.z, -c18.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c18.x
mul_sat r1.w, r1.z, r1
mov r1.y, c17.x
add r1.y, -c16.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c16.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c9.x
mad r2.w, -r1, c30.x, c30.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c30.x, c30
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c3.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c15.x
add r2.w, -c14.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c14.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c30.x, c30
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c29.z
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c23.x
add r1.y, -c22.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c22.x
mul_sat r1.z, r1.y, r1
mov r1.x, c21
add r1.x, -c20, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c20
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c30.x, c30.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c30, c30.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c25.x
add r3.x, -c24, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c24
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c30.x, c30
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c10.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c11.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c13.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c6.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mul r0.x, v2.z, c27
mul r1.w, r0.x, c26.x
mov r0.y, c8.x
add r0.y, -c7.x, r0
rcp r0.y, r0.y
add r0.x, v2.z, -c7
mul_sat r2.x, r0, r0.y
pow r0, c29.y, r1.w
mad r0.z, -r2.x, c30.x, c30.y
mul r0.y, r2.x, r2.x
mad r0.y, -r0, r0.z, c29.z
mul r0.y, v2.x, r0
mad r1.xyz, r0.y, r1, r3
mov r0.w, r0.x
mov r0.y, c29.w
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r2.xyz, r0, -r1
add r0.x, -r0.w, c29.z
mad r0.xyz, r0.x, r2, r1
mul_pp r2.xyz, r0, v5
mul_pp r1.xyz, r0, c1
dp3_pp r0.y, v4, c0
max_pp r0.y, r0, c29.x
texldp r0.x, v6, s10
mul_pp r0.x, r0.y, r0
mul_pp r0.xyz, r0.x, r1
mad_pp oC0.xyz, r0, c30.x, r2
mov_pp oC0.w, c28.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_deepTex] 2D 1
SetTexture 1 [_mainTex] 2D 3
SetTexture 2 [_highTex] 2D 5
SetTexture 3 [_snowTex] 2D 7
SetTexture 4 [_deepMultiTex] 2D 2
SetTexture 5 [_mainMultiTex] 2D 4
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 8
SetTexture 8 [_steepTex] 2D 9
SetTexture 9 [_fogColorRamp] 2D 10
SetTexture 10 [_ShadowMapTexture] 2D 0
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
Float 276 [_PlanetOpacity]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
"ps_4_0
eefiecedpndicjhhiidpfkojcbpjgkbpdledimjeabaaaaaalibhaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaapalaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcgibgaaaaeaaaaaaajkafaaaafjaaaaaeegiocaaa
aaaaaaaabcaaaaaafjaaaaaeegiocaaaabaaaaaaabaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaa
fkaaaaadaagabaaaagaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaa
aiaaaaaafkaaaaadaagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaae
aahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaae
aahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaae
aahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaae
aahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaae
aahabaaaakaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaa
acaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaad
hcbabaaaafaaaaaagcbaaaadhcbabaaaagaaaaaagcbaaaadlcbabaaaahaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaakbcaabaaaaaaaaaaa
dkiacaiaebaaaaaaaaaaaaaaalaaaaaaakiacaaaaaaaaaaaamaaaaaaaoaaaaak
bcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
aaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaadkiacaiaebaaaaaa
aaaaaaaaalaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaackiacaiaebaaaaaa
aaaaaaaaaiaaaaaadkiacaaaaaaaaaaaaiaaaaaaaoaaaaakccaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaaaaaaaaaaaaaaaaj
ecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaaaaaaaaaaaiaaaaaa
dicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaaaaaaaaaadcaaaaaj
ecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaaaaaaaaaa
diaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaaaaaaaaaaakaaaaaa
diaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaaiaaaaaa
dcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaiaaaaaadkaabaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaaaaaaaaaaakaabaaa
aaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaa
aaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaaakiacaaa
aaaaaaaaanaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaaabaaaaaafgbfbaaa
adaaaaaafgincaiaebaaaaaaaaaaaaaaamaaaaaadicaaaahccaabaaaabaaaaaa
bkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaabkaabaaa
abaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaabaaaaaa
bkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaa
abaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaabkiacaiaebaaaaaa
aaaaaaaaamaaaaaackiacaaaaaaaaaaaamaaaaaaaoaaaaakicaabaaaabaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaabaaaaaadicaaaah
ecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaadcaaaaakccaabaaa
abaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaiaebaaaaaaabaaaaaa
diaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaaabaaaaaadiaaaaah
ccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaadiaaaaaihcaabaaa
acaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaiaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadcaaaaaj
hcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaafgbfbaaaacaaaaaa
egacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaaabaaaaaaegacbaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaaaaaaaaa
aagabaaaabaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaaaeaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaaeghobaaaaaaaaaaa
aagabaaaabaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaaafaaaaaakgbkbaaa
acaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaajncaabaaaabaaaaaa
agajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaaaaaaaaakicaabaaa
acaaaaaadkiacaiaebaaaaaaaaaaaaaaanaaaaaaakiacaaaaaaaaaaaaoaaaaaa
aoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaaadaaaaaangifcaia
ebaaaaaaaaaaaaaaanaaaaaadicaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
bkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaa
dkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaa
adaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaanaaaaaa
ckiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaadaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaahbcaabaaaadaaaaaa
bkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaaakaabaaa
adaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaadaaaaaa
akaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaaacaaaaaabkaabaaa
adaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaadiaaaaahbcaabaaa
adaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaa
bkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaa
acaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadiaaaaahocaabaaaadaaaaaa
agajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadcaaaaajocaabaaaadaaaaaa
agajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaaefaaaaajpcaabaaa
aeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaafgaobaaaadaaaaaa
dcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaaadaaaaaaagaobaaa
abaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaaoaaaaaa
ckiacaaaaaaaaaaaaoaaaaaaaoaaaaakbcaabaaaadaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaajccaabaaaadaaaaaa
bkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaaoaaaaaadicaaaahbcaabaaa
adaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaa
akaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaa
adaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaahbcaabaaaadaaaaaa
akaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaaaaaaaaaafgagbaaa
aaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaadaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaadaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaa
acaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaaagaobaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaaaaaaaaaaakaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaaeaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaaeaaaaaaaagabaaa
acaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaaeaaaaaa
aagabaaaacaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaa
acaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaa
egbcbaaaabaaaaaaagiacaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgafbaaaabaaaaaa
igadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaafgifcaaa
aaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
agaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
agaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaagaaaaaaaagabaaaagaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaaalaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaaaagabaaaaiaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaaaagabaaaaiaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaaahaaaaaaaagabaaa
aiaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaacaaaaaafgbfbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaacaaaaaa
fgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
aeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaadcaaaaajhcaabaaa
aaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaaaeaaaaaadiaaaaai
hcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaaajaaaaaaefaaaaaj
pcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaaaagabaaaajaaaaaa
diaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaaaagabaaaajaaaaaa
efaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaaaiaaaaaaaagabaaa
ajaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaakgbkbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaabaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaackiacaiaebaaaaaa
aaaaaaaaajaaaaaadkiacaaaaaaaaaaaajaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
icaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaaaaaaaaaaajaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaabaaaaaadcaaaaaj
icaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakbabaaaadaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaadgaaaaafccaabaaa
abaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaajaaaaaaaagabaaaakaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaaaaaaaaaackbabaaa
adaaaaaaakiacaaaaaaaaaaabbaaaaaadiaaaaaiicaabaaaaaaaaaaadkaabaaa
aaaaaaaabkiacaaaaaaaaaaabaaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
aaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadiaaaaaihcaabaaaabaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaa
abaaaaaadiaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaaagaaaaaa
aoaaaaahdcaabaaaacaaaaaaegbabaaaahaaaaaapgbpbaaaahaaaaaaefaaaaaj
pcaabaaaacaaaaaaegaabaaaacaaaaaaeghobaaaakaaaaaaaagabaaaaaaaaaaa
baaaaaaiicaabaaaaaaaaaaaegbcbaaaafaaaaaaegiccaaaabaaaaaaaaaaaaaa
deaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaaaaapaaaaah
icaabaaaaaaaaaaapgapbaaaaaaaaaaaagaabaaaacaaaaaadcaaaaajhccabaaa
aaaaaaaaegacbaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaag
iccabaaaaaaaaaaabkiacaaaaaaaaaaabbaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_ShadowMapTexture] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MUL R2.w, R0, R4.x;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
TXP R4.x, fragment.texcoord[5], texture[10], 2D;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
TEX R0, fragment.texcoord[4], texture[11], 2D;
MUL R3.xyz, R0.w, R0;
MUL R0.xyz, R0, R4.x;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[27].x, R0.w;
ADD R0.w, -R0, c[27].y;
MUL R3.xyz, R3, c[28].y;
MUL R0.xyz, R0, c[27].w;
MIN R0.xyz, R3, R0;
MUL R3.xyz, R3, R4.x;
MAX R0.xyz, R0, R3;
MAD R1.xyz, R0.w, R2, R1;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 177 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_ShadowMapTexture] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
def c27, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xy
dcl_texcoord5 v5
mov r0.x, c3
add r0.x, -c2, r0
rcp r0.y, r0.x
add r0.x, v2.z, -c2
mul_sat r0.w, r0.x, r0.y
mul r2.xyz, v0, c0.x
mad r1.x, -r0.w, c28, c28.y
mul r0.w, r0, r0
texld r0.xyz, r2.zyzw, s1
mul r3.w, r0, r1.x
mul r1.xyz, v1.x, r0
texld r0.xyz, r2, s1
mad r1.xyz, v1.z, r0, r1
add r0.x, -r3.w, c27.y
mov r0.z, c17.x
add r0.z, -c16.x, r0
rcp r0.w, r0.z
add r0.z, v2.y, -c16.x
mul_sat r0.w, r0.z, r0
mov r0.y, c15.x
add r0.y, -c14.x, r0
rcp r0.z, r0.y
add r0.y, v2, -c14.x
mul_sat r0.y, r0, r0.z
mul r3.w, r3, c7.x
mad r1.w, -r0, c28.x, c28.y
mul r0.z, r0.w, r0.w
mul r0.w, r0.z, r1
mul r0.z, r0.y, r0.y
mad r0.y, -r0, c28.x, c28
mad r1.w, r0.z, r0.y, -r0
mul r0.w, r0.x, c1.x
mul r2.w, r0, r1
texld r0.xyz, r2.zxzw, s1
mad r0.xyz, v1.y, r0, r1
mul r3.xyz, r0, r2.w
texld r0.xyz, r2.zyzw, s0
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
mul r0.xyz, v1.x, r0
texld r1.xyz, r2, s0
mul_sat r2.w, r2, r4.x
mad r1.xyz, v1.z, r1, r0
mad r0.y, -r2.w, c28.x, c28
mul r0.x, r2.w, r2.w
mad r4.y, -r0.x, r0, c27
texld r0.xyz, r2.zxzw, s0
mul r2.w, r0, r4.y
mad r1.xyz, v1.y, r0, r1
mad r1.xyz, r1, r2.w, r3
texld r0.xyz, r2.zyzw, s2
mul r3.xyz, v1.x, r0
texld r0.xyz, r2, s2
mad r3.xyz, v1.z, r0, r3
mov r0.y, c21.x
add r0.y, -c20.x, r0
rcp r0.z, r0.y
add r0.y, v2, -c20.x
mul_sat r0.z, r0.y, r0
mov r0.x, c19
add r0.x, -c18, r0
rcp r0.y, r0.x
add r0.x, v2.y, -c18
mul_sat r0.x, r0, r0.y
mad r2.w, -r0.z, c28.x, c28.y
mul r0.y, r0.z, r0.z
mul r0.z, r0.y, r2.w
mul r0.y, r0.x, r0.x
mad r0.x, -r0, c28, c28.y
mad r2.w, r0.y, r0.x, -r0.z
texld r0.xyz, r2.zxzw, s2
mad r0.xyz, v1.y, r0, r3
mul r3.x, r0.w, r2.w
mad r3.xyz, r0, r3.x, r1
texld r0.xyz, r2.zyzw, s3
texld r1.xyz, r2, s3
mul r0.xyz, v1.x, r0
mad r1.xyz, v1.z, r1, r0
texld r0.xyz, r2.zxzw, s3
mov r2.y, c23.x
add r2.y, -c22.x, r2
rcp r2.y, r2.y
add r2.x, v2.y, -c22
mul_sat r4.x, r2, r2.y
mad r2.xyz, v1.y, r0, r1
mul r1.xyz, v0, c8.x
mad r0.y, -r4.x, c28.x, c28
mul r0.x, r4, r4
mul r4.x, r0, r0.y
mul r4.z, r0.w, r4.x
mad r2.xyz, r2, r4.z, r3
texld r0.xyz, r1.zyzw, s4
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s4
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s4
mul r1.xyz, v0, c9.x
mad r3.xyz, v1.y, r0, r3
mul r4.y, r3.w, r4
texld r0.xyz, r1.zyzw, s5
mad r2.xyz, r3, r4.y, r2
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s5
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s5
mul r1.xyz, v0, c10.x
mad r3.xyz, v1.y, r0, r3
mul r1.w, r3, r1
mad r2.xyz, r3, r1.w, r2
texld r0.xyz, r1.zyzw, s6
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s6
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s6
mul r1.xyz, v0, c11.x
mad r3.xyz, v1.y, r0, r3
mul r1.w, r3, r2
mad r2.xyz, r3, r1.w, r2
texld r0.xyz, r1.zyzw, s7
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s7
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s7
mad r0.xyz, v1.y, r0, r3
mul r1.x, r3.w, r4
mad r0.xyz, r0, r1.x, r2
mad r1.xyz, v3, r0, -v3
add r0.x, r0.w, r3.w
mov r0.w, c6.x
add r0.w, -c5.x, r0
rcp r1.w, r0.w
add r0.w, v2.z, -c5.x
mul r2.xyz, v0, c4.x
mad r3.xyz, r0.x, r1, v3
texld r0.xyz, r2.zyzw, s8
mul_sat r0.w, r0, r1
texld r1.xyz, r2, s8
mul r0.xyz, v1.x, r0
mad r1.xyz, v1.z, r1, r0
texld r0.xyz, r2.zxzw, s8
mad r0.xyz, r0, v1.y, r1
mad r1.x, -r0.w, c28, c28.y
mul r0.w, r0, r0
mad r0.w, -r0, r1.x, c27.y
add r1.xyz, r0, -r3
mul r0.x, v2, r0.w
mad r3.xyz, r0.x, r1, r3
mul r0.w, v2.z, c25.x
mul r0.w, r0, c24.x
pow r1, c27.x, r0.w
texldp r2.x, v5, s10
mov r0.y, c27.z
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r4.xyz, r0, -r3
texld r0, v4, s11
mul_pp r1.yzw, r0.xxyz, r2.x
mul_pp r0.xyz, r0.w, r0
mul_pp r0.xyz, r0, c27.w
mul_pp r1.yzw, r1, c28.x
min_pp r1.yzw, r0.xxyz, r1
mov r0.w, r1.x
mul_pp r0.xyz, r0, r2.x
max_pp r0.xyz, r1.yzww, r0
add r0.w, -r0, c27.y
mad r1.xyz, r0.w, r4, r3
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_deepTex] 2D 1
SetTexture 1 [_mainTex] 2D 3
SetTexture 2 [_highTex] 2D 5
SetTexture 3 [_snowTex] 2D 7
SetTexture 4 [_deepMultiTex] 2D 2
SetTexture 5 [_mainMultiTex] 2D 4
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 8
SetTexture 8 [_steepTex] 2D 9
SetTexture 9 [_fogColorRamp] 2D 10
SetTexture 10 [_ShadowMapTexture] 2D 0
SetTexture 11 [unity_Lightmap] 2D 11
ConstBuffer "$Globals" 320
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
Float 276 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedkfmbpoajcjkfcpfpcafbgnlpafomlplfabaaaaaaombhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadadaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apalaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefclebgaaaaeaaaaaaaknafaaaa
fjaaaaaeegiocaaaaaaaaaaabcaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
fibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaa
fibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaa
fibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaa
fibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaa
fibiaaaeaahabaaaakaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaaffffaaaa
gcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaa
adaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaaddcbabaaaafaaaaaagcbaaaad
lcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaaakiacaaaaaaaaaaa
amaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaalaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaiaaaaaadkiacaaaaaaaaaaaaiaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaiaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaakaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaiaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaiaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaamaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaamaaaaaackiacaaaaaaaaaaaamaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaiaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
adaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaanaaaaaaakiacaaa
aaaaaaaaaoaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaanaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaanaaaaaackiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
afaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaaoaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaaoaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaakaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaalaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
aeaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
alaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
ajaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaajaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaajaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaajaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaajaaaaaadkiacaaaaaaaaaaaajaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaajaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaakaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaabbaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaabaaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaaagaaaaaa
pgbpbaaaagaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
akaaaaaaaagabaaaaaaaaaaaaaaaaaahicaabaaaaaaaaaaaakaabaaaabaaaaaa
akaabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaafaaaaaaeghobaaa
alaaaaaaaagabaaaalaaaaaadiaaaaahocaabaaaabaaaaaapgapbaaaaaaaaaaa
agajbaaaacaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgapbaaaaaaaaaaa
ddaaaaahocaabaaaabaaaaaafgaobaaaabaaaaaaagajbaaaacaaaaaadiaaaaah
hcaabaaaacaaaaaaagaabaaaabaaaaaaegacbaaaacaaaaaadeaaaaahhcaabaaa
abaaaaaajgahbaaaabaaaaaaegacbaaaacaaaaaadiaaaaahhccabaaaaaaaaaaa
egacbaaaaaaaaaaaegacbaaaabaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaa
aaaaaaaabbaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_ShadowMapTexture] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MUL R2.w, R0, R4.x;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
TXP R4.x, fragment.texcoord[5], texture[10], 2D;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
TEX R0, fragment.texcoord[4], texture[11], 2D;
MUL R3.xyz, R0.w, R0;
MUL R0.xyz, R0, R4.x;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[27].x, R0.w;
ADD R0.w, -R0, c[27].y;
MUL R3.xyz, R3, c[28].y;
MUL R0.xyz, R0, c[27].w;
MIN R0.xyz, R3, R0;
MUL R3.xyz, R3, R4.x;
MAX R0.xyz, R0, R3;
MAD R1.xyz, R0.w, R2, R1;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 177 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_ShadowMapTexture] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
def c27, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xy
dcl_texcoord5 v5
mov r0.x, c3
add r0.x, -c2, r0
rcp r0.y, r0.x
add r0.x, v2.z, -c2
mul_sat r0.w, r0.x, r0.y
mul r2.xyz, v0, c0.x
mad r1.x, -r0.w, c28, c28.y
mul r0.w, r0, r0
texld r0.xyz, r2.zyzw, s1
mul r3.w, r0, r1.x
mul r1.xyz, v1.x, r0
texld r0.xyz, r2, s1
mad r1.xyz, v1.z, r0, r1
add r0.x, -r3.w, c27.y
mov r0.z, c17.x
add r0.z, -c16.x, r0
rcp r0.w, r0.z
add r0.z, v2.y, -c16.x
mul_sat r0.w, r0.z, r0
mov r0.y, c15.x
add r0.y, -c14.x, r0
rcp r0.z, r0.y
add r0.y, v2, -c14.x
mul_sat r0.y, r0, r0.z
mul r3.w, r3, c7.x
mad r1.w, -r0, c28.x, c28.y
mul r0.z, r0.w, r0.w
mul r0.w, r0.z, r1
mul r0.z, r0.y, r0.y
mad r0.y, -r0, c28.x, c28
mad r1.w, r0.z, r0.y, -r0
mul r0.w, r0.x, c1.x
mul r2.w, r0, r1
texld r0.xyz, r2.zxzw, s1
mad r0.xyz, v1.y, r0, r1
mul r3.xyz, r0, r2.w
texld r0.xyz, r2.zyzw, s0
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
mul r0.xyz, v1.x, r0
texld r1.xyz, r2, s0
mul_sat r2.w, r2, r4.x
mad r1.xyz, v1.z, r1, r0
mad r0.y, -r2.w, c28.x, c28
mul r0.x, r2.w, r2.w
mad r4.y, -r0.x, r0, c27
texld r0.xyz, r2.zxzw, s0
mul r2.w, r0, r4.y
mad r1.xyz, v1.y, r0, r1
mad r1.xyz, r1, r2.w, r3
texld r0.xyz, r2.zyzw, s2
mul r3.xyz, v1.x, r0
texld r0.xyz, r2, s2
mad r3.xyz, v1.z, r0, r3
mov r0.y, c21.x
add r0.y, -c20.x, r0
rcp r0.z, r0.y
add r0.y, v2, -c20.x
mul_sat r0.z, r0.y, r0
mov r0.x, c19
add r0.x, -c18, r0
rcp r0.y, r0.x
add r0.x, v2.y, -c18
mul_sat r0.x, r0, r0.y
mad r2.w, -r0.z, c28.x, c28.y
mul r0.y, r0.z, r0.z
mul r0.z, r0.y, r2.w
mul r0.y, r0.x, r0.x
mad r0.x, -r0, c28, c28.y
mad r2.w, r0.y, r0.x, -r0.z
texld r0.xyz, r2.zxzw, s2
mad r0.xyz, v1.y, r0, r3
mul r3.x, r0.w, r2.w
mad r3.xyz, r0, r3.x, r1
texld r0.xyz, r2.zyzw, s3
texld r1.xyz, r2, s3
mul r0.xyz, v1.x, r0
mad r1.xyz, v1.z, r1, r0
texld r0.xyz, r2.zxzw, s3
mov r2.y, c23.x
add r2.y, -c22.x, r2
rcp r2.y, r2.y
add r2.x, v2.y, -c22
mul_sat r4.x, r2, r2.y
mad r2.xyz, v1.y, r0, r1
mul r1.xyz, v0, c8.x
mad r0.y, -r4.x, c28.x, c28
mul r0.x, r4, r4
mul r4.x, r0, r0.y
mul r4.z, r0.w, r4.x
mad r2.xyz, r2, r4.z, r3
texld r0.xyz, r1.zyzw, s4
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s4
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s4
mul r1.xyz, v0, c9.x
mad r3.xyz, v1.y, r0, r3
mul r4.y, r3.w, r4
texld r0.xyz, r1.zyzw, s5
mad r2.xyz, r3, r4.y, r2
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s5
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s5
mul r1.xyz, v0, c10.x
mad r3.xyz, v1.y, r0, r3
mul r1.w, r3, r1
mad r2.xyz, r3, r1.w, r2
texld r0.xyz, r1.zyzw, s6
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s6
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s6
mul r1.xyz, v0, c11.x
mad r3.xyz, v1.y, r0, r3
mul r1.w, r3, r2
mad r2.xyz, r3, r1.w, r2
texld r0.xyz, r1.zyzw, s7
mul r3.xyz, v1.x, r0
texld r0.xyz, r1, s7
mad r3.xyz, v1.z, r0, r3
texld r0.xyz, r1.zxzw, s7
mad r0.xyz, v1.y, r0, r3
mul r1.x, r3.w, r4
mad r0.xyz, r0, r1.x, r2
mad r1.xyz, v3, r0, -v3
add r0.x, r0.w, r3.w
mov r0.w, c6.x
add r0.w, -c5.x, r0
rcp r1.w, r0.w
add r0.w, v2.z, -c5.x
mul r2.xyz, v0, c4.x
mad r3.xyz, r0.x, r1, v3
texld r0.xyz, r2.zyzw, s8
mul_sat r0.w, r0, r1
texld r1.xyz, r2, s8
mul r0.xyz, v1.x, r0
mad r1.xyz, v1.z, r1, r0
texld r0.xyz, r2.zxzw, s8
mad r0.xyz, r0, v1.y, r1
mad r1.x, -r0.w, c28, c28.y
mul r0.w, r0, r0
mad r0.w, -r0, r1.x, c27.y
add r1.xyz, r0, -r3
mul r0.x, v2, r0.w
mad r3.xyz, r0.x, r1, r3
mul r0.w, v2.z, c25.x
mul r0.w, r0, c24.x
pow r1, c27.x, r0.w
texldp r2.x, v5, s10
mov r0.y, c27.z
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r4.xyz, r0, -r3
texld r0, v4, s11
mul_pp r1.yzw, r0.xxyz, r2.x
mul_pp r0.xyz, r0.w, r0
mul_pp r0.xyz, r0, c27.w
mul_pp r1.yzw, r1, c28.x
min_pp r1.yzw, r0.xxyz, r1
mov r0.w, r1.x
mul_pp r0.xyz, r0, r2.x
max_pp r0.xyz, r1.yzww, r0
add r0.w, -r0, c27.y
mad r1.xyz, r0.w, r4, r3
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
SetTexture 0 [_deepTex] 2D 1
SetTexture 1 [_mainTex] 2D 3
SetTexture 2 [_highTex] 2D 5
SetTexture 3 [_snowTex] 2D 7
SetTexture 4 [_deepMultiTex] 2D 2
SetTexture 5 [_mainMultiTex] 2D 4
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 8
SetTexture 8 [_steepTex] 2D 9
SetTexture 9 [_fogColorRamp] 2D 10
SetTexture 10 [_ShadowMapTexture] 2D 0
SetTexture 11 [unity_Lightmap] 2D 11
ConstBuffer "$Globals" 320
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
Float 276 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedkfmbpoajcjkfcpfpcafbgnlpafomlplfabaaaaaaombhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadadaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apalaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefclebgaaaaeaaaaaaaknafaaaa
fjaaaaaeegiocaaaaaaaaaaabcaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
fibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaa
fibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaa
fibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaa
fibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaa
fibiaaaeaahabaaaakaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaaffffaaaa
gcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaa
adaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaaddcbabaaaafaaaaaagcbaaaad
lcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaaakiacaaaaaaaaaaa
amaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaalaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaiaaaaaadkiacaaaaaaaaaaaaiaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaiaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaakaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaiaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaiaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaamaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaamaaaaaackiacaaaaaaaaaaaamaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaiaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
adaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaanaaaaaaakiacaaa
aaaaaaaaaoaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaanaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaanaaaaaackiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaafaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
afaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaaoaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaaoaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaakaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaalaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaaeaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
aeaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
alaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
ajaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaajaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaajaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaajaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaajaaaaaadkiacaaaaaaaaaaaajaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaajaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaakaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaabbaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaabaaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaaagaaaaaa
pgbpbaaaagaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
akaaaaaaaagabaaaaaaaaaaaaaaaaaahicaabaaaaaaaaaaaakaabaaaabaaaaaa
akaabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaafaaaaaaeghobaaa
alaaaaaaaagabaaaalaaaaaadiaaaaahocaabaaaabaaaaaapgapbaaaaaaaaaaa
agajbaaaacaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgapbaaaaaaaaaaa
ddaaaaahocaabaaaabaaaaaafgaobaaaabaaaaaaagajbaaaacaaaaaadiaaaaah
hcaabaaaacaaaaaaagaabaaaabaaaaaaegacbaaaacaaaaaadeaaaaahhcaabaaa
abaaaaaajgahbaaaabaaaaaaegacbaaaacaaaaaadiaaaaahhccabaaaaaaaaaaa
egacbaaaaaaaaaaaegacbaaaabaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaa
aaaaaaaabbaaaaaadoaaaaab"
}
}
 }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardAdd" }
  ZWrite Off
  Fog {
   Color (0,0,0,0)
  }
  Blend One One
Program "vp" {
SubProgram "opengl " {
Keywords { "POINT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Matrix 13 [_LightMatrix0]
Vector 17 [_WorldSpaceLightPos0]
Vector 18 [unity_Scale]
Vector 19 [_tintColor]
Float 20 [_steepPower]
Float 21 [_saturation]
Float 22 [_contrast]
Vector 23 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..23],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[24];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[21].x, R0;
MAD R2.xyz, -c[19], c[19].w, R0;
MUL R0.xyz, c[19], c[19].w;
MAD result.texcoord[3].xyz, R2, c[22].x, R0;
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP4 R0.z, vertex.position, c[11];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP4 R0.w, vertex.position, c[12];
DP4 result.texcoord[6].z, R0, c[15];
DP4 result.texcoord[6].y, R0, c[14];
DP4 result.texcoord[6].x, R0, c[13];
ADD result.texcoord[5].xyz, -R0, c[17];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[20];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[23];
MUL R1.xyz, vertex.normal, c[18].w;
DP3 result.texcoord[4].z, R1, c[11];
DP3 result.texcoord[4].y, R1, c[10];
DP3 result.texcoord[4].x, R1, c[9];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 37 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_LightMatrix0]
Vector 16 [_WorldSpaceLightPos0]
Vector 17 [unity_Scale]
Vector 18 [_tintColor]
Float 19 [_steepPower]
Float 20 [_saturation]
Float 21 [_contrast]
Vector 22 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c23, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c23.w
dp3 r0.x, v4, c23
add r1.xyz, v4, -r0
mad r0.xyz, r1, c20.x, r0
mad r2.xyz, -c18, c18.w, r0
mul r0.xyz, c18, c18.w
mad o4.xyz, r2, c21.x, r0
dp4 r0.x, v0, c8
dp4 r0.z, v0, c10
dp4 r0.y, v0, c9
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
dp4 r0.w, v0, c11
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c22
mul r1.xyz, v1, c17.w
dp4 o7.z, r0, c14
dp4 o7.y, r0, c13
dp4 o7.x, r0, c12
add o6.xyz, -r0, c16
dp4 r0.x, v0, c2
dp3 o5.z, r1, c10
dp3 o5.y, r1, c9
dp3 o5.x, r1, c8
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c19
"
}
SubProgram "d3d11 " {
Keywords { "POINT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 304
Matrix 48 [_LightMatrix0]
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedlffkeoapfcedocgibbgagifoafnjkpjbabaaaaaaimaiaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
ahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
kiagaaaaeaaaabaakkabaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaae
egiocaaaabaaaaaaabaaaaaafjaaaaaeegiocaaaacaaaaaabfaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaa
fpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaa
gfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaa
afaaaaaagfaaaaadhccabaaaagaaaaaagfaaaaadhccabaaaahaaaaaagiaaaaac
acaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaacaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaai
iccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaaaaaaaaadiaaaaai
bcaabaaaaaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaak
bcaabaaaaaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaa
ahaaaaaadkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaa
akaabaiaebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaa
akiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaa
baaaaaakbcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdp
nfhiojdnaaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaa
egbcbaaaafaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaia
ebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaaaaaaaaaa
diaaaaajhcaabaaaabaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaaaaaaaaaa
ahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaaegacbaaa
aaaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaa
pgipcaaaacaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaa
egiccaaaacaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaa
amaaaaaaagaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhccabaaaafaaaaaa
egiccaaaacaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadiaaaaai
hcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiccaaaacaaaaaaanaaaaaadcaaaaak
hcaabaaaaaaaaaaaegiccaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaa
aaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaaaoaaaaaakgbkbaaa
aaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaa
apaaaaaapgbpbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaajhccabaaaagaaaaaa
egacbaiaebaaaaaaaaaaaaaaegiccaaaabaaaaaaaaaaaaaadiaaaaaipcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaanaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaa
aaaaaaaaegiccaaaaaaaaaaaaeaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaa
aaaaaaaaadaaaaaaagaabaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegiccaaaaaaaaaaaafaaaaaakgakbaaaaaaaaaaaegacbaaaabaaaaaa
dcaaaaakhccabaaaahaaaaaaegiccaaaaaaaaaaaagaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_WorldSpaceLightPos0]
Vector 14 [unity_Scale]
Vector 15 [_tintColor]
Float 16 [_steepPower]
Float 17 [_saturation]
Float 18 [_contrast]
Vector 19 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[21] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..19],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[20];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[17].x, R0;
MAD R2.xyz, -c[15], c[15].w, R0;
MUL R0.xyz, c[15], c[15].w;
MAD result.texcoord[3].xyz, R2, c[18].x, R0;
MUL R0.xyz, vertex.normal, c[14].w;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP3 result.texcoord[4].z, R0, c[11];
DP3 result.texcoord[4].y, R0, c[10];
DP3 result.texcoord[4].x, R0, c[9];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[16];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[19];
MOV result.texcoord[5].xyz, c[13];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 30 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_WorldSpaceLightPos0]
Vector 13 [unity_Scale]
Vector 14 [_tintColor]
Float 15 [_steepPower]
Float 16 [_saturation]
Float 17 [_contrast]
Vector 18 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c19, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c19.w
dp3 r0.x, v4, c19
add r1.xyz, v4, -r0
mad r0.xyz, r1, c16.x, r0
mad r2.xyz, -c14, c14.w, r0
mul r0.xyz, c14, c14.w
mad o4.xyz, r2, c17.x, r0
mul r0.xyz, v1, c13.w
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
dp3 o5.z, r0, c10
dp3 o5.y, r0, c9
dp3 o5.x, r0, c8
dp4 r0.x, v0, c2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c18
mov o6.xyz, c12
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c15
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 240
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedhmielebmkbikacjmkmjijkfloecpaocjabaaaaaajeagaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmiaeaaaaeaaaabaadcabaaaafjaaaaaeegiocaaaaaaaaaaa
apaaaaaafjaaaaaeegiocaaaabaaaaaaabaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaad
dcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
hccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaa
gfaaaaadhccabaaaafaaaaaagfaaaaadhccabaaaagaaaaaagiaaaaacacaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaa
aaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaaiiccabaaa
adaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaa
aaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaak
bcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdn
aaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaaegbcbaaa
afaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaiaebaaaaaa
aaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaj
hcaabaaaabaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaa
dcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaa
acaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaa
acaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaaamaaaaaa
agaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhccabaaaafaaaaaaegiccaaa
acaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaaghccabaaa
agaaaaaaegiccaaaabaaaaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Matrix 13 [_LightMatrix0]
Vector 17 [_WorldSpaceLightPos0]
Vector 18 [unity_Scale]
Vector 19 [_tintColor]
Float 20 [_steepPower]
Float 21 [_saturation]
Float 22 [_contrast]
Vector 23 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..23],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[24];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[21].x, R0;
MAD R2.xyz, -c[19], c[19].w, R0;
MUL R0.xyz, c[19], c[19].w;
MAD result.texcoord[3].xyz, R2, c[22].x, R0;
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP4 R0.z, vertex.position, c[11];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP4 R0.w, vertex.position, c[12];
DP4 result.texcoord[6].w, R0, c[16];
DP4 result.texcoord[6].z, R0, c[15];
DP4 result.texcoord[6].y, R0, c[14];
DP4 result.texcoord[6].x, R0, c[13];
ADD result.texcoord[5].xyz, -R0, c[17];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[20];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[23];
MUL R1.xyz, vertex.normal, c[18].w;
DP3 result.texcoord[4].z, R1, c[11];
DP3 result.texcoord[4].y, R1, c[10];
DP3 result.texcoord[4].x, R1, c[9];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 38 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_LightMatrix0]
Vector 16 [_WorldSpaceLightPos0]
Vector 17 [unity_Scale]
Vector 18 [_tintColor]
Float 19 [_steepPower]
Float 20 [_saturation]
Float 21 [_contrast]
Vector 22 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c23, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c23.w
dp3 r0.x, v4, c23
add r1.xyz, v4, -r0
mad r0.xyz, r1, c20.x, r0
mad r2.xyz, -c18, c18.w, r0
mul r0.xyz, c18, c18.w
mad o4.xyz, r2, c21.x, r0
dp4 r0.x, v0, c8
dp4 r0.z, v0, c10
dp4 r0.y, v0, c9
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
dp4 r0.w, v0, c11
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c22
mul r1.xyz, v1, c17.w
dp4 o7.w, r0, c15
dp4 o7.z, r0, c14
dp4 o7.y, r0, c13
dp4 o7.x, r0, c12
add o6.xyz, -r0, c16
dp4 r0.x, v0, c2
dp3 o5.z, r1, c10
dp3 o5.y, r1, c9
dp3 o5.x, r1, c8
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c19
"
}
SubProgram "d3d11 " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 304
Matrix 48 [_LightMatrix0]
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecednempioekggaajafhbmkmlnlojbndefadabaaaaaaimaiaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
kiagaaaaeaaaabaakkabaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaae
egiocaaaabaaaaaaabaaaaaafjaaaaaeegiocaaaacaaaaaabfaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaa
fpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaa
gfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaa
afaaaaaagfaaaaadhccabaaaagaaaaaagfaaaaadpccabaaaahaaaaaagiaaaaac
acaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaacaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaai
iccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaaaaaaaaadiaaaaai
bcaabaaaaaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaak
bcaabaaaaaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaa
ahaaaaaadkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaa
akaabaiaebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaa
akiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaa
baaaaaakbcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdp
nfhiojdnaaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaa
egbcbaaaafaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaia
ebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaaaaaaaaaa
diaaaaajhcaabaaaabaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaaaaaaaaaa
ahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaaegacbaaa
aaaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaa
pgipcaaaacaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaa
egiccaaaacaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaa
amaaaaaaagaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhccabaaaafaaaaaa
egiccaaaacaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadiaaaaai
hcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiccaaaacaaaaaaanaaaaaadcaaaaak
hcaabaaaaaaaaaaaegiccaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaa
aaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaaaoaaaaaakgbkbaaa
aaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaa
apaaaaaapgbpbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaajhccabaaaagaaaaaa
egacbaiaebaaaaaaaaaaaaaaegiccaaaabaaaaaaaaaaaaaadiaaaaaipcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaanaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaaipcaabaaaabaaaaaafgafbaaa
aaaaaaaaegiocaaaaaaaaaaaaeaaaaaadcaaaaakpcaabaaaabaaaaaaegiocaaa
aaaaaaaaadaaaaaaagaabaaaaaaaaaaaegaobaaaabaaaaaadcaaaaakpcaabaaa
abaaaaaaegiocaaaaaaaaaaaafaaaaaakgakbaaaaaaaaaaaegaobaaaabaaaaaa
dcaaaaakpccabaaaahaaaaaaegiocaaaaaaaaaaaagaaaaaapgapbaaaaaaaaaaa
egaobaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Matrix 13 [_LightMatrix0]
Vector 17 [_WorldSpaceLightPos0]
Vector 18 [unity_Scale]
Vector 19 [_tintColor]
Float 20 [_steepPower]
Float 21 [_saturation]
Float 22 [_contrast]
Vector 23 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..23],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[24];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[21].x, R0;
MAD R2.xyz, -c[19], c[19].w, R0;
MUL R0.xyz, c[19], c[19].w;
MAD result.texcoord[3].xyz, R2, c[22].x, R0;
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP4 R0.z, vertex.position, c[11];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP4 R0.w, vertex.position, c[12];
DP4 result.texcoord[6].z, R0, c[15];
DP4 result.texcoord[6].y, R0, c[14];
DP4 result.texcoord[6].x, R0, c[13];
ADD result.texcoord[5].xyz, -R0, c[17];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[20];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[23];
MUL R1.xyz, vertex.normal, c[18].w;
DP3 result.texcoord[4].z, R1, c[11];
DP3 result.texcoord[4].y, R1, c[10];
DP3 result.texcoord[4].x, R1, c[9];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 37 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_LightMatrix0]
Vector 16 [_WorldSpaceLightPos0]
Vector 17 [unity_Scale]
Vector 18 [_tintColor]
Float 19 [_steepPower]
Float 20 [_saturation]
Float 21 [_contrast]
Vector 22 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c23, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c23.w
dp3 r0.x, v4, c23
add r1.xyz, v4, -r0
mad r0.xyz, r1, c20.x, r0
mad r2.xyz, -c18, c18.w, r0
mul r0.xyz, c18, c18.w
mad o4.xyz, r2, c21.x, r0
dp4 r0.x, v0, c8
dp4 r0.z, v0, c10
dp4 r0.y, v0, c9
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
dp4 r0.w, v0, c11
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c22
mul r1.xyz, v1, c17.w
dp4 o7.z, r0, c14
dp4 o7.y, r0, c13
dp4 o7.x, r0, c12
add o6.xyz, -r0, c16
dp4 r0.x, v0, c2
dp3 o5.z, r1, c10
dp3 o5.y, r1, c9
dp3 o5.x, r1, c8
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c19
"
}
SubProgram "d3d11 " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 304
Matrix 48 [_LightMatrix0]
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedlffkeoapfcedocgibbgagifoafnjkpjbabaaaaaaimaiaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
ahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
kiagaaaaeaaaabaakkabaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaae
egiocaaaabaaaaaaabaaaaaafjaaaaaeegiocaaaacaaaaaabfaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaa
fpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaa
gfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaa
afaaaaaagfaaaaadhccabaaaagaaaaaagfaaaaadhccabaaaahaaaaaagiaaaaac
acaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaacaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaai
iccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaaaaaaaaadiaaaaai
bcaabaaaaaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaak
bcaabaaaaaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaa
ahaaaaaadkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaa
akaabaiaebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaa
akiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaa
baaaaaakbcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdp
nfhiojdnaaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaa
egbcbaaaafaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaia
ebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaaaaaaaaaa
diaaaaajhcaabaaaabaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaaaaaaaaaa
ahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaaegacbaaa
aaaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaa
pgipcaaaacaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaa
egiccaaaacaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaa
amaaaaaaagaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhccabaaaafaaaaaa
egiccaaaacaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadiaaaaai
hcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiccaaaacaaaaaaanaaaaaadcaaaaak
hcaabaaaaaaaaaaaegiccaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaa
aaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaaaoaaaaaakgbkbaaa
aaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaa
apaaaaaapgbpbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaajhccabaaaagaaaaaa
egacbaiaebaaaaaaaaaaaaaaegiccaaaabaaaaaaaaaaaaaadiaaaaaipcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaanaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaa
aaaaaaaaegiccaaaaaaaaaaaaeaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaa
aaaaaaaaadaaaaaaagaabaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegiccaaaaaaaaaaaafaaaaaakgakbaaaaaaaaaaaegacbaaaabaaaaaa
dcaaaaakhccabaaaahaaaaaaegiccaaaaaaaaaaaagaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Matrix 13 [_LightMatrix0]
Vector 17 [_WorldSpaceLightPos0]
Vector 18 [unity_Scale]
Vector 19 [_tintColor]
Float 20 [_steepPower]
Float 21 [_saturation]
Float 22 [_contrast]
Vector 23 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..23],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[24];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[21].x, R0;
MAD R2.xyz, -c[19], c[19].w, R0;
MUL R0.xyz, c[19], c[19].w;
MAD result.texcoord[3].xyz, R2, c[22].x, R0;
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP4 R0.w, vertex.position, c[12];
DP4 R0.z, vertex.position, c[11];
DP4 result.texcoord[6].y, R0, c[14];
DP4 result.texcoord[6].x, R0, c[13];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[20];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[23];
MUL R1.xyz, vertex.normal, c[18].w;
DP3 result.texcoord[4].z, R1, c[11];
DP3 result.texcoord[4].y, R1, c[10];
DP3 result.texcoord[4].x, R1, c[9];
MOV result.texcoord[5].xyz, c[17];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 36 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_LightMatrix0]
Vector 16 [_WorldSpaceLightPos0]
Vector 17 [unity_Scale]
Vector 18 [_tintColor]
Float 19 [_steepPower]
Float 20 [_saturation]
Float 21 [_contrast]
Vector 22 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c23, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c23.w
dp3 r0.x, v4, c23
add r1.xyz, v4, -r0
mad r0.xyz, r1, c20.x, r0
mad r2.xyz, -c18, c18.w, r0
mul r0.xyz, c18, c18.w
mad o4.xyz, r2, c21.x, r0
dp4 r0.x, v0, c8
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
dp4 r0.w, v0, c11
dp4 r0.z, v0, c10
dp4 r0.y, v0, c9
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c22
mul r1.xyz, v1, c17.w
dp4 o7.y, r0, c13
dp4 o7.x, r0, c12
dp4 r0.x, v0, c2
dp3 o5.z, r1, c10
dp3 o5.y, r1, c9
dp3 o5.x, r1, c8
mov o6.xyz, c16
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c19
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 304
Matrix 48 [_LightMatrix0]
Vector 112 [_tintColor]
Float 144 [_steepPower]
Float 280 [_saturation]
Float 284 [_contrast]
Vector 288 [_sunLightDirection]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityLighting" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedpnbphajdniekhgacgbjadbgnmnnbjmnnabaaaaaaoiahaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
adamaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
aeagaaaaeaaaabaaibabaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaae
egiocaaaabaaaaaaabaaaaaafjaaaaaeegiocaaaacaaaaaabfaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaa
fpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaa
gfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaa
afaaaaaagfaaaaadhccabaaaagaaaaaagfaaaaaddccabaaaahaaaaaagiaaaaac
acaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaacaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaabaaaaaai
iccabaaaadaaaaaaegiccaaaaaaaaaaabcaaaaaaegacbaaaaaaaaaaadiaaaaai
bcaabaaaaaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaak
bcaabaaaaaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaa
ahaaaaaadkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaageccabaaaadaaaaaa
akaabaiaebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaa
akiacaaaaaaaaaaaajaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaa
baaaaaakbcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdp
nfhiojdnaaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaiaebaaaaaaaaaaaaaa
egbcbaaaafaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaaaaaaaaaabbaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegiccaia
ebaaaaaaaaaaaaaaahaaaaaapgipcaaaaaaaaaaaahaaaaaaegacbaaaaaaaaaaa
diaaaaajhcaabaaaabaaaaaapgipcaaaaaaaaaaaahaaaaaaegiccaaaaaaaaaaa
ahaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaabbaaaaaaegacbaaa
aaaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaa
pgipcaaaacaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaa
egiccaaaacaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaacaaaaaa
amaaaaaaagaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhccabaaaafaaaaaa
egiccaaaacaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaag
hccabaaaagaaaaaaegiccaaaabaaaaaaaaaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaacaaaaaaanaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaacaaaaaaamaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaaoaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaapaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadiaaaaaidcaabaaaabaaaaaafgafbaaaaaaaaaaa
egiacaaaaaaaaaaaaeaaaaaadcaaaaakdcaabaaaaaaaaaaaegiacaaaaaaaaaaa
adaaaaaaagaabaaaaaaaaaaaegaabaaaabaaaaaadcaaaaakdcaabaaaaaaaaaaa
egiacaaaaaaaaaaaafaaaaaakgakbaaaaaaaaaaaegaabaaaaaaaaaaadcaaaaak
dccabaaaahaaaaaaegiacaaaaaaaaaaaagaaaaaapgapbaaaaaaaaaaaegaabaaa
aaaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Keywords { "POINT" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTexture0] 2D 10
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 0, 2.718282, 1, 0.5 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[3];
ADD R0.x, -R0, c[4];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[3];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[1].x;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MOV R2.w, c[13].x;
ADD R2.w, -R2, c[14].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[13].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[28].z;
MOV R0.z, c[17].x;
ADD R0.z, -R0, c[18].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[17].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[15].x;
ADD R0.y, -R0, c[16].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[15].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[8].x;
MAD R2.x, -R1.w, c[29], c[29].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[29].x, c[29];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[2].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[29].x, c[29];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[28].z;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[21].x;
ADD R0.y, -R0, c[22].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[21].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[19];
ADD R0.x, -R0, c[20];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[19];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[29].x, c[29].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[29], c[29].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[23].x;
ADD R3.y, -R3, c[24].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[23];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[29].x, c[29];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[9].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[6].x;
ADD R0.w, -R0, c[7].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[6].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[5].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL_SAT R0.w, R0, R1;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28].z;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[26].x;
MUL R0.w, R0, c[25].x;
POW R1.w, c[28].y, R0.w;
DP3 R0.w, fragment.texcoord[5], fragment.texcoord[5];
RSQ R0.w, R0.w;
MOV R0.y, c[28].w;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R0.xyz, R0, -R1;
ADD R1.w, -R1, c[28].z;
MAD R0.xyz, R1.w, R0, R1;
MUL R1.xyz, R0.w, fragment.texcoord[5];
DP3 R0.w, fragment.texcoord[6], fragment.texcoord[6];
DP3 R1.x, fragment.texcoord[4], R1;
MUL R0.xyz, R0, c[0];
TEX R0.w, R0.w, texture[10], 2D;
MAX R1.x, R1, c[28];
MUL R0.w, R1.x, R0;
MUL R0.xyz, R0.w, R0;
MUL result.color.xyz, R0, c[29].x;
MOV result.color.w, c[28].x;
END
# 170 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTexture0] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c27, 0.00000000, 2.71828198, 1.00000000, 0.50000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
dcl_texcoord6 v6.xyz
mov r0.x, c4
add r0.w, -c3.x, r0.x
mul r0.xyz, v0, c1.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c3.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.z
mov r1.z, c18.x
add r1.z, -c17.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c17.x
mul_sat r1.w, r1.z, r1
mov r1.y, c16.x
add r1.y, -c15.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c15.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c8.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c2.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c14.x
add r2.w, -c13.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c13.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.z
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c22.x
add r1.y, -c21.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c21.x
mul_sat r1.z, r1.y, r1
mov r1.x, c20
add r1.x, -c19, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c19
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c24.x
add r3.x, -c23, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c23
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c9.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c10.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c5.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mul r0.x, v2.z, c26
mul r1.w, r0.x, c25.x
mov r0.y, c7.x
add r0.y, -c6.x, r0
rcp r0.y, r0.y
add r0.x, v2.z, -c6
mul_sat r2.x, r0, r0.y
pow r0, c27.y, r1.w
mad r0.z, -r2.x, c28.x, c28.y
mul r0.y, r2.x, r2.x
mad r0.y, -r0, r0.z, c27.z
mul r0.y, v2.x, r0
mad r1.xyz, r0.y, r1, r3
mov r0.w, r0.x
mov r0.y, c27.w
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r2.xyz, r0, -r1
add r0.y, -r0.w, c27.z
mad r1.xyz, r0.y, r2, r1
dp3_pp r0.x, v5, v5
rsq_pp r0.x, r0.x
mul_pp r2.xyz, r0.x, v5
dp3 r0.x, v6, v6
dp3_pp r0.y, v4, r2
max_pp r0.y, r0, c27.x
texld r0.x, r0.x, s10
mul_pp r1.xyz, r1, c0
mul_pp r0.x, r0.y, r0
mul_pp r0.xyz, r0.x, r1
mul_pp oC0.xyz, r0, c28.x
mov_pp oC0.w, c27.x
"
}
SubProgram "d3d11 " {
Keywords { "POINT" }
SetTexture 0 [_deepTex] 2D 1
SetTexture 1 [_mainTex] 2D 3
SetTexture 2 [_highTex] 2D 5
SetTexture 3 [_snowTex] 2D 7
SetTexture 4 [_deepMultiTex] 2D 2
SetTexture 5 [_mainMultiTex] 2D 4
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 8
SetTexture 8 [_steepTex] 2D 9
SetTexture 9 [_fogColorRamp] 2D 10
SetTexture 10 [_LightTexture0] 2D 0
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedeigiphbjlkkioamlfhilgkepmjknpelcabaaaaaamibhaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaahahaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefchibgaaaaeaaaaaaajoafaaaafjaaaaaeegiocaaa
aaaaaaaabcaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaad
aagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaa
fkaaaaadaagabaaaakaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaae
aahabaaaadaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaae
aahabaaaafaaaaaaffffaaaafibiaaaeaahabaaaagaaaaaaffffaaaafibiaaae
aahabaaaahaaaaaaffffaaaafibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaae
aahabaaaajaaaaaaffffaaaafibiaaaeaahabaaaakaaaaaaffffaaaagcbaaaad
hcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaa
gcbaaaadhcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagcbaaaadhcbabaaa
agaaaaaagcbaaaadhcbabaaaahaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
agaaaaaaaaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaa
akiacaaaaaaaaaaaamaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaa
bkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaadicaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
bkaabaiaebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaak
ccaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaaaiaaaaaadkiacaaaaaaaaaaa
aiaaaaaaaoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpbkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaa
aaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaa
bkaabaaaaaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaackiacaaaaaaaaaaaakaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaa
aaaaaaaabkiacaaaaaaaaaaaaiaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaa
aaaaaaaaaiaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaa
abaaaaaackaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaia
ebaaaaaaaaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaa
abaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaa
aaaaaaajmcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaa
amaaaaaadicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaa
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaak
icaabaaaabaaaaaabkiacaiaebaaaaaaaaaaaaaaamaaaaaackiacaaaaaaaaaaa
amaaaaaaaoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaa
ckaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaa
ckaabaaaabaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaabkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaa
aaaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaa
aaaaaaaaaiaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaa
acaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaaeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaa
adaaaaaakgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaa
ggakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaa
aeaaaaaaegacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaa
egaabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaa
dcaaaaajhcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
aeaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaa
agajbaaaadaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaa
anaaaaaaakiacaaaaaaaaaaaaoaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaa
adaaaaaafgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaanaaaaaadicaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaa
bkiacaiaebaaaaaaaaaaaaaaanaaaaaackiacaaaaaaaaaaaanaaaaaaaoaaaaak
ccaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
adaaaaaadicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
dcaaaaakicaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaia
ebaaaaaaacaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaa
acaaaaaadiaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
afaaaaaadiaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
afaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaa
fgaobaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
acaaaaaaaagabaaaafaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaa
fgbfbaaaacaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaa
adaaaaaaagaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaoaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaak
bcaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
adaaaaaaaaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaaoaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaa
adaaaaaadcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaa
adaaaaaadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
diaaaaahgcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaa
ahaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaa
kgakbaaaaaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaapgipcaaaaaaaaaaaakaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaeaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
alaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaaeaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaa
afaaaaaaaagabaaaaeaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
afaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaa
acaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaa
acaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
kgikcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaaiaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaaiaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaa
acaaaaaaeghobaaaahaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaaaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaa
dcaaaaakhcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaia
ebaaaaaaaeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaa
fgifcaaaaaaaaaaaajaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaajaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaajaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaa
abaaaaaaeghobaaaaiaaaaaaaagabaaaajaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaadaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaak
icaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaaajaaaaaadkiacaaaaaaaaaaa
ajaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaa
ckiacaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaa
dkbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaaj
pcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaakaaaaaa
aaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaa
diaaaaaiicaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaabbaaaaaa
diaaaaaiicaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaabaaaaaaa
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaia
ebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaa
egacbaaaaaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaa
egbcbaaaagaaaaaaegbcbaaaagaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaa
aaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegbcbaaaagaaaaaa
baaaaaahicaabaaaaaaaaaaaegbcbaaaafaaaaaaegacbaaaabaaaaaadeaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaaaabaaaaaahbcaabaaa
abaaaaaaegbcbaaaahaaaaaaegbcbaaaahaaaaaaefaaaaajpcaabaaaabaaaaaa
agaabaaaabaaaaaaeghobaaaakaaaaaaaagabaaaaaaaaaaaapaaaaahicaabaaa
aaaaaaaapgapbaaaaaaaaaaaagaabaaaabaaaaaadiaaaaahhccabaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaa
aaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 0, 2.718282, 1, 0.5 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[3];
ADD R0.x, -R0, c[4];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[3];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[1].x;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MOV R2.w, c[13].x;
ADD R2.w, -R2, c[14].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[13].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[28].z;
MOV R0.z, c[17].x;
ADD R0.z, -R0, c[18].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[17].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[15].x;
ADD R0.y, -R0, c[16].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[15].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[8].x;
MAD R2.x, -R1.w, c[29], c[29].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[29].x, c[29];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[2].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[29].x, c[29];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[28].z;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[21].x;
ADD R0.y, -R0, c[22].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[21].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[19];
ADD R0.x, -R0, c[20];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[19];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[29].x, c[29].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[29], c[29].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[23].x;
ADD R3.y, -R3, c[24].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[23];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[29].x, c[29];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[9].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[6].x;
ADD R0.w, -R0, c[7].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[6].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[5].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MUL_SAT R0.w, R0, R1;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28].z;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[26].x;
MUL R0.w, R0, c[25].x;
POW R0.w, c[28].y, R0.w;
MOV R0.y, c[28].w;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R0.xyz, R0, -R1;
ADD R0.w, -R0, c[28].z;
MAD R1.xyz, R0.w, R0, R1;
MOV R2.xyz, fragment.texcoord[5];
DP3 R0.x, fragment.texcoord[4], R2;
MUL R1.xyz, R1, c[0];
MAX R0.x, R0, c[28];
MUL R0.xyz, R0.x, R1;
MUL result.color.xyz, R0, c[29].x;
MOV result.color.w, c[28].x;
END
# 165 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
def c27, 0.00000000, 2.71828198, 1.00000000, 0.50000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
mov r0.x, c4
add r0.w, -c3.x, r0.x
mul r0.xyz, v0, c1.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c3.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.z
mov r1.z, c18.x
add r1.z, -c17.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c17.x
mul_sat r1.w, r1.z, r1
mov r1.y, c16.x
add r1.y, -c15.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c15.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c8.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c2.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c14.x
add r2.w, -c13.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c13.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.z
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c22.x
add r1.y, -c21.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c21.x
mul_sat r1.z, r1.y, r1
mov r1.x, c20
add r1.x, -c19, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c19
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c24.x
add r3.x, -c23, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c23
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c9.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c10.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c5.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mul r0.x, v2.z, c26
mul r1.w, r0.x, c25.x
mov r0.y, c7.x
add r0.y, -c6.x, r0
rcp r0.y, r0.y
add r0.x, v2.z, -c6
mul_sat r2.x, r0, r0.y
pow r0, c27.y, r1.w
mad r0.z, -r2.x, c28.x, c28.y
mul r0.y, r2.x, r2.x
mad r0.y, -r0, r0.z, c27.z
mov r0.w, r0.x
mul r0.y, v2.x, r0
mad r1.xyz, r0.y, r1, r3
mov r0.y, c27.w
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r0.xyz, r0, -r1
add r0.w, -r0, c27.z
mad r1.xyz, r0.w, r0, r1
mov_pp r2.xyz, v5
dp3_pp r0.x, v4, r2
mul_pp r1.xyz, r1, c0
max_pp r0.x, r0, c27
mul_pp r0.xyz, r0.x, r1
mul_pp oC0.xyz, r0, c28.x
mov_pp oC0.w, c27.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
ConstBuffer "$Globals" 240
Vector 16 [_LightColor0]
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedlbohmpemigeahjacpjeiakamjhefndghabaaaaaapmbgaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmebfaaaaeaaaaaaahbafaaaa
fjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaa
afaaaaaaffffaaaafibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaa
ahaaaaaaffffaaaafibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaa
ajaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaa
gcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaadhcbabaaa
afaaaaaagcbaaaadhcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
agaaaaaaaaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaa
akiacaaaaaaaaaaaaiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaa
bkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
bkaabaiaebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaak
ccaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaa
aeaaaaaaaoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpbkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaa
aaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaa
bkaabaaaaaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaackiacaaaaaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaa
aaaaaaaabkiacaaaaaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaa
abaaaaaackaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaia
ebaaaaaaaaaaaaaaaiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaa
abaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaa
aaaaaaajmcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaa
aiaaaaaadicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaa
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaak
icaabaaaabaaaaaabkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaa
aiaaaaaaaoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaa
ckaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaa
ckaabaaaabaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaabkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaa
aaaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaa
aaaaaaaaaeaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaa
acaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaaeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaa
adaaaaaakgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaa
ggakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaa
aeaaaaaaegacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaa
egaabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
dcaaaaajhcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
aeaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaa
agajbaaaadaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaa
ajaaaaaaakiacaaaaaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaa
adaaaaaafgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaa
bkiacaiaebaaaaaaaaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaak
ccaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
adaaaaaadicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
dcaaaaakicaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaia
ebaaaaaaacaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaa
acaaaaaadiaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadiaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaa
fgaobaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
acaaaaaaaagabaaaaeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaa
fgbfbaaaacaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaa
adaaaaaaagaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaa
bkiacaiaebaaaaaaaaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaak
bcaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
adaaaaaaaaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaa
adaaaaaadcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaa
adaaaaaadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
diaaaaahgcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaa
agaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaa
kgakbaaaaaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaapgipcaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaadaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaadaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaa
afaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
afaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaa
acaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaa
acaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
kgikcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaa
acaaaaaaeghobaaaahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaaaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaa
dcaaaaakhcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaia
ebaaaaaaaeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaa
fgifcaaaaaaaaaaaafaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaa
abaaaaaaeghobaaaaiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaadaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaak
icaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaa
afaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaa
dkbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaaj
pcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaa
aaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaa
diaaaaaiicaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaa
diaaaaaiicaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaa
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaia
ebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaa
egacbaaaaaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaa
egbcbaaaafaaaaaaegbcbaaaagaaaaaadeaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaaaaaaaaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaaaaaaaaadiaaaaahhccabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "SPOT" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTexture0] 2D 10
SetTexture 11 [_LightTextureB0] 2D 11
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 0, 0.5, 2.718282, 1 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[3];
ADD R0.x, -R0, c[4];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[3];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[1].x;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MOV R2.w, c[13].x;
ADD R2.w, -R2, c[14].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[13].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[28].w;
MOV R0.z, c[17].x;
ADD R0.z, -R0, c[18].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[17].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[15].x;
ADD R0.y, -R0, c[16].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[15].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[8].x;
MAD R2.x, -R1.w, c[29], c[29].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[29].x, c[29];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[2].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[29].x, c[29];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[28].w;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[21].x;
ADD R0.y, -R0, c[22].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[21].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[19];
ADD R0.x, -R0, c[20];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[19];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[29].x, c[29].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[29], c[29].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[23].x;
ADD R3.y, -R3, c[24].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[23];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[29].x, c[29];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[9].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[6].x;
ADD R0.w, -R0, c[7].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[6].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[5].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL_SAT R0.w, R0, R1;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28];
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[26].x;
MUL R0.w, R0, c[25].x;
POW R1.w, c[28].z, R0.w;
DP3 R0.w, fragment.texcoord[5], fragment.texcoord[5];
ADD R1.w, -R1, c[28];
RSQ R0.w, R0.w;
MOV R0.y, c[28];
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R0.xyz, R0, -R1;
MAD R0.xyz, R1.w, R0, R1;
MUL R1.xyz, R0.w, fragment.texcoord[5];
DP3 R1.x, fragment.texcoord[4], R1;
RCP R0.w, fragment.texcoord[6].w;
MAD R1.zw, fragment.texcoord[6].xyxy, R0.w, c[28].y;
DP3 R1.y, fragment.texcoord[6], fragment.texcoord[6];
TEX R0.w, R1.zwzw, texture[10], 2D;
TEX R1.w, R1.y, texture[11], 2D;
SLT R1.y, c[28].x, fragment.texcoord[6].z;
MUL R0.w, R1.y, R0;
MUL R1.y, R0.w, R1.w;
MAX R0.w, R1.x, c[28].x;
MUL R0.xyz, R0, c[0];
MUL R0.w, R0, R1.y;
MUL R0.xyz, R0.w, R0;
MUL result.color.xyz, R0, c[29].x;
MOV result.color.w, c[28].x;
END
# 176 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "SPOT" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTexture0] 2D 10
SetTexture 11 [_LightTextureB0] 2D 11
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
def c27, 0.00000000, 1.00000000, 0.50000000, 2.71828198
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
dcl_texcoord6 v6
mov r0.x, c4
add r0.w, -c3.x, r0.x
mul r0.xyz, v0, c1.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c3.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c18.x
add r1.z, -c17.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c17.x
mul_sat r1.w, r1.z, r1
mov r1.y, c16.x
add r1.y, -c15.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c15.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c8.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c2.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c14.x
add r2.w, -c13.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c13.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c22.x
add r1.y, -c21.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c21.x
mul_sat r1.z, r1.y, r1
mov r1.x, c20
add r1.x, -c19, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c19
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c24.x
add r3.x, -c23, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c23
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c9.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c10.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c5.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mul r0.x, v2.z, c26
mul r1.w, r0.x, c25.x
mov r0.y, c7.x
add r0.y, -c6.x, r0
rcp r0.y, r0.y
add r0.x, v2.z, -c6
mul_sat r2.x, r0, r0.y
pow r0, c27.w, r1.w
mad r0.z, -r2.x, c28.x, c28.y
mul r0.y, r2.x, r2.x
mad r0.y, -r0, r0.z, c27
mov r0.w, r0.x
mul r0.y, v2.x, r0
mad r1.xyz, r0.y, r1, r3
mov r0.y, c27.z
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r2.xyz, r0, -r1
add r0.y, -r0.w, c27
mad r1.xyz, r0.y, r2, r1
dp3_pp r0.x, v5, v5
rsq_pp r0.x, r0.x
mul_pp r0.xyz, r0.x, v5
dp3_pp r0.y, v4, r0
dp3 r0.x, v6, v6
mul_pp r2.xyz, r1, c0
rcp r0.w, v6.w
mad r1.xy, v6, r0.w, c27.z
texld r0.x, r0.x, s11
texld r0.w, r1, s10
cmp r0.z, -v6, c27.x, c27.y
mul_pp r0.z, r0, r0.w
mul_pp r0.z, r0, r0.x
max_pp r0.x, r0.y, c27
mul_pp r0.x, r0, r0.z
mul_pp r0.xyz, r0.x, r2
mul_pp oC0.xyz, r0, c28.x
mov_pp oC0.w, c27.x
"
}
SubProgram "d3d11 " {
Keywords { "SPOT" }
SetTexture 0 [_deepTex] 2D 2
SetTexture 1 [_mainTex] 2D 4
SetTexture 2 [_highTex] 2D 6
SetTexture 3 [_snowTex] 2D 8
SetTexture 4 [_deepMultiTex] 2D 3
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 7
SetTexture 7 [_snowMultiTex] 2D 9
SetTexture 8 [_steepTex] 2D 10
SetTexture 9 [_fogColorRamp] 2D 11
SetTexture 10 [_LightTexture0] 2D 0
SetTexture 11 [_LightTextureB0] 2D 1
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedgabkiafckfknmiadldnnghggallkddhpabaaaaaalmbiaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaapapaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcgmbhaaaaeaaaaaaanlafaaaafjaaaaaeegiocaaa
aaaaaaaabcaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaad
aagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaa
fkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaa
acaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaaeaahabaaa
aeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaaeaahabaaa
agaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaaeaahabaaa
aiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaaeaahabaaa
akaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaaffffaaaagcbaaaadhcbabaaa
abaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaad
hcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagcbaaaadhcbabaaaagaaaaaa
gcbaaaadpcbabaaaahaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaa
aaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaaakiacaaa
aaaaaaaaamaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaa
adaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaadicaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaia
ebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaaiaaaaaadkiacaaaaaaaaaaaaiaaaaaa
aoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
bkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaaiaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
bkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaa
bkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckiacaaaaaaaaaaaakaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaa
bkiacaaaaaaaaaaaaiaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaa
aiaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaa
ckaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaa
aaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaa
aaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaabaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaaj
mcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaamaaaaaa
dicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaaj
icaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaa
abaaaaaabkiacaiaebaaaaaaaaaaaaaaamaaaaaackiacaaaaaaaaaaaamaaaaaa
aoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
bkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
aiaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaaeaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaaeaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaa
eghobaaaabaaaaaaaagabaaaaeaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaa
kgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaaeaaaaaa
egacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaaeaaaaaa
egacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaadcaaaaaj
hcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaa
adaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaanaaaaaa
akiacaaaaaaaaaaaaoaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaa
fgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaanaaaaaadicaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaa
dkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaanaaaaaackiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaa
dicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaak
icaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaa
acaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaa
diaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaagaaaaaa
diaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaagaaaaaa
dcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaa
aagabaaaagaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaa
acaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaa
agaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaaoaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaakbcaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaa
aaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaa
aoaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaah
gcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaiaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaiaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaiaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaa
aaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
pgipcaaaaaaaaaaaakaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaadaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaalaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
afaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
afaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaafaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
acaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaa
egbcbaaaabaaaaaafgifcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaahaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaa
aaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaajaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaajaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaajaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaak
hcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaa
aeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaa
aaaaaaaaajaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaakaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaakaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaakaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaajaaaaaadkiacaaaaaaaaaaaajaaaaaa
aoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaa
dkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaa
adaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaa
abaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaalaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaai
icaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaabbaaaaaadiaaaaai
icaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaabaaaaaaadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaabaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaa
ahaaaaaapgbpbaaaahaaaaaaaaaaaaakdcaabaaaabaaaaaaegaabaaaabaaaaaa
aceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaakaaaaaaaagabaaaaaaaaaaadbaaaaahicaabaaa
aaaaaaaaabeaaaaaaaaaaaaackbabaaaahaaaaaaabaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaa
abaaaaaadkaabaaaaaaaaaaabaaaaaahbcaabaaaabaaaaaaegbcbaaaahaaaaaa
egbcbaaaahaaaaaaefaaaaajpcaabaaaabaaaaaaagaabaaaabaaaaaaeghobaaa
alaaaaaaaagabaaaabaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akaabaaaabaaaaaabaaaaaahbcaabaaaabaaaaaaegbcbaaaagaaaaaaegbcbaaa
agaaaaaaeeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaaagaabaaaabaaaaaaegbcbaaaagaaaaaabaaaaaahbcaabaaaabaaaaaa
egbcbaaaafaaaaaaegacbaaaabaaaaaadeaaaaahbcaabaaaabaaaaaaakaabaaa
abaaaaaaabeaaaaaaaaaaaaaapaaaaahicaabaaaaaaaaaaaagaabaaaabaaaaaa
pgapbaaaaaaaaaaadiaaaaahhccabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "POINT_COOKIE" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTextureB0] 2D 10
SetTexture 11 [_LightTexture0] CUBE 11
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 0, 2.718282, 1, 0.5 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[3];
ADD R0.x, -R0, c[4];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[3];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[1].x;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MOV R2.w, c[13].x;
ADD R2.w, -R2, c[14].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[13].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[28].z;
MOV R0.z, c[17].x;
ADD R0.z, -R0, c[18].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[17].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[15].x;
ADD R0.y, -R0, c[16].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[15].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[8].x;
MAD R2.x, -R1.w, c[29], c[29].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[29].x, c[29];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[2].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[29].x, c[29];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[28].z;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[21].x;
ADD R0.y, -R0, c[22].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[21].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[19];
ADD R0.x, -R0, c[20];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[19];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[29].x, c[29].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[29], c[29].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[23].x;
ADD R3.y, -R3, c[24].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[23];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[29].x, c[29];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[9].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[6].x;
ADD R0.w, -R0, c[7].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[6].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[5].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL_SAT R0.w, R0, R1;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28].z;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[26].x;
MUL R1.w, R0, c[25].x;
DP3 R0.w, fragment.texcoord[5], fragment.texcoord[5];
POW R1.w, c[28].y, R1.w;
RSQ R0.w, R0.w;
ADD R1.w, -R1, c[28].z;
MOV R0.y, c[28].w;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R0.xyz, R0, -R1;
MAD R0.xyz, R1.w, R0, R1;
MUL R1.xyz, R0.w, fragment.texcoord[5];
DP3 R1.x, fragment.texcoord[4], R1;
DP3 R1.y, fragment.texcoord[6], fragment.texcoord[6];
TEX R0.w, fragment.texcoord[6], texture[11], CUBE;
TEX R1.w, R1.y, texture[10], 2D;
MUL R1.y, R1.w, R0.w;
MAX R0.w, R1.x, c[28].x;
MUL R0.xyz, R0, c[0];
MUL R0.w, R0, R1.y;
MUL R0.xyz, R0.w, R0;
MUL result.color.xyz, R0, c[29].x;
MOV result.color.w, c[28].x;
END
# 172 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT_COOKIE" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTextureB0] 2D 10
SetTexture 11 [_LightTexture0] CUBE 11
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_cube s11
def c27, 0.00000000, 2.71828198, 1.00000000, 0.50000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
dcl_texcoord6 v6.xyz
mov r0.x, c4
add r0.w, -c3.x, r0.x
mul r0.xyz, v0, c1.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c3.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.z
mov r1.z, c18.x
add r1.z, -c17.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c17.x
mul_sat r1.w, r1.z, r1
mov r1.y, c16.x
add r1.y, -c15.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c15.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c8.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c2.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c14.x
add r2.w, -c13.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c13.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.z
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c22.x
add r1.y, -c21.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c21.x
mul_sat r1.z, r1.y, r1
mov r1.x, c20
add r1.x, -c19, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c19
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c24.x
add r3.x, -c23, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c23
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c9.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c10.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c5.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mov r0.w, c7.x
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c6.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c6.x
mul_sat r0.w, r0, r1.x
add r1.xyz, r0, -r3
mul r0.x, v2.z, c26
mul r0.y, r0.w, r0.w
mad r0.z, -r0.w, c28.x, c28.y
mad r2.x, -r0.y, r0.z, c27.z
mul r1.w, r0.x, c25.x
pow r0, c27.y, r1.w
mul r0.y, v2.x, r2.x
mad r2.xyz, r0.y, r1, r3
mov r0.y, r0.x
dp3_pp r0.x, v5, v5
add r0.y, -r0, c27.z
mov r1.y, c27.w
mov r1.x, v2.w
texld r1.xyz, r1, s9
add r1.xyz, r1, -r2
mad r1.xyz, r0.y, r1, r2
rsq_pp r0.x, r0.x
mul_pp r0.xyz, r0.x, v5
dp3_pp r0.y, v4, r0
dp3 r0.x, v6, v6
texld r0.x, r0.x, s10
texld r0.w, v6, s11
mul r0.z, r0.x, r0.w
max_pp r0.x, r0.y, c27
mul_pp r1.xyz, r1, c0
mul_pp r0.x, r0, r0.z
mul_pp r0.xyz, r0.x, r1
mul_pp oC0.xyz, r0, c28.x
mov_pp oC0.w, c27.x
"
}
SubProgram "d3d11 " {
Keywords { "POINT_COOKIE" }
SetTexture 0 [_deepTex] 2D 2
SetTexture 1 [_mainTex] 2D 4
SetTexture 2 [_highTex] 2D 6
SetTexture 3 [_snowTex] 2D 8
SetTexture 4 [_deepMultiTex] 2D 3
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 7
SetTexture 7 [_snowMultiTex] 2D 9
SetTexture 8 [_steepTex] 2D 10
SetTexture 9 [_fogColorRamp] 2D 11
SetTexture 10 [_LightTextureB0] 2D 1
SetTexture 11 [_LightTexture0] CUBE 0
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedleapnmeedidoncnchilecheokidmbcjjabaaaaaacebiaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaahahaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcnebgaaaaeaaaaaaalfafaaaafjaaaaaeegiocaaa
aaaaaaaabcaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaad
aagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaa
fkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaa
acaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaaeaahabaaa
aeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaaeaahabaaa
agaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaaeaahabaaa
aiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaaeaahabaaa
akaaaaaaffffaaaafidaaaaeaahabaaaalaaaaaaffffaaaagcbaaaadhcbabaaa
abaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaad
hcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagcbaaaadhcbabaaaagaaaaaa
gcbaaaadhcbabaaaahaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaa
aaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaaakiacaaa
aaaaaaaaamaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaa
adaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaadicaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaia
ebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaaiaaaaaadkiacaaaaaaaaaaaaiaaaaaa
aoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
bkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaaiaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
bkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaa
bkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckiacaaaaaaaaaaaakaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaa
bkiacaaaaaaaaaaaaiaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaa
aiaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaa
ckaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaa
aaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaa
aaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaaabaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaaj
mcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaamaaaaaa
dicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaaj
icaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaa
abaaaaaabkiacaiaebaaaaaaaaaaaaaaamaaaaaackiacaaaaaaaaaaaamaaaaaa
aoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
bkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
aiaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaaeaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaaeaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaa
eghobaaaabaaaaaaaagabaaaaeaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaa
kgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaaeaaaaaa
egacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaaeaaaaaa
egacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaadcaaaaaj
hcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaa
adaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaanaaaaaa
akiacaaaaaaaaaaaaoaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaa
fgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaanaaaaaadicaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaa
dkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaanaaaaaackiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaa
dicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaak
icaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaa
acaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaa
diaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaagaaaaaa
diaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaagaaaaaa
dcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaa
aagabaaaagaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaa
acaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaa
agaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaaoaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaakbcaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaa
aaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaa
aoaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaah
gcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaiaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaiaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaiaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaa
aaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
pgipcaaaaaaaaaaaakaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaadaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaalaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
afaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
afaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaafaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
acaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaa
egbcbaaaabaaaaaafgifcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaahaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaa
aaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaajaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaajaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaajaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaak
hcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaa
aeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaa
aaaaaaaaajaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaakaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaakaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaakaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaajaaaaaadkiacaaaaaaaaaaaajaaaaaa
aoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaa
dkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaa
adaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaa
abaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaalaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaai
icaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaabbaaaaaadiaaaaai
icaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaabaaaaaaadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaaegbcbaaa
agaaaaaaegbcbaaaagaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegbcbaaaagaaaaaabaaaaaah
icaabaaaaaaaaaaaegbcbaaaafaaaaaaegacbaaaabaaaaaadeaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaaaabaaaaaahbcaabaaaabaaaaaa
egbcbaaaahaaaaaaegbcbaaaahaaaaaaefaaaaajpcaabaaaabaaaaaaagaabaaa
abaaaaaaeghobaaaakaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaa
egbcbaaaahaaaaaaeghobaaaalaaaaaaaagabaaaaaaaaaaadiaaaaahbcaabaaa
abaaaaaaakaabaaaabaaaaaadkaabaaaacaaaaaaapaaaaahicaabaaaaaaaaaaa
pgapbaaaaaaaaaaaagaabaaaabaaaaaadiaaaaahhccabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaa
doaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL_COOKIE" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTexture0] 2D 10
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 0, 2.718282, 1, 0.5 },
		{ 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[3];
ADD R0.x, -R0, c[4];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].z, -c[3];
MUL_SAT R0.w, R0.x, R0.y;
MUL R3.xyz, fragment.texcoord[0], c[1].x;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MOV R2.w, c[13].x;
ADD R2.w, -R2, c[14].x;
RCP R4.x, R2.w;
ADD R2.w, fragment.texcoord[2].y, -c[13].x;
MUL_SAT R2.w, R2, R4.x;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R0.w, R0, R1.x;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.x, -R0.w, c[28].z;
MOV R0.z, c[17].x;
ADD R0.z, -R0, c[18].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[17].x;
MUL_SAT R1.w, R0.z, R1;
MOV R0.y, c[15].x;
ADD R0.y, -R0, c[16].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[15].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.w, R0, c[8].x;
MAD R2.x, -R1.w, c[29], c[29].y;
MUL R0.z, R1.w, R1.w;
MUL R1.w, R0.z, R2.x;
MUL R0.z, R0.y, R0.y;
MAD R0.y, -R0, c[29].x, c[29];
MAD R3.w, R0.z, R0.y, -R1;
MUL R1.w, R0.x, c[2].x;
MUL R2.x, R1.w, R3.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.xyz, R0, R2.x;
TEX R2.xyz, R3.zyzw, texture[0], 2D;
TEX R0.xyz, R3, texture[0], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R0, R2;
MAD R0.y, -R2.w, c[29].x, c[29];
MUL R0.x, R2.w, R2.w;
MAD R4.x, -R0, R0.y, c[28].z;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, R1, R4.x;
MAD R2.xyz, fragment.texcoord[1].y, R0, R2;
TEX R0.xyz, R3.zyzw, texture[2], 2D;
MAD R2.xyz, R2, R2.w, R1;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[2], 2D;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R0.y, c[21].x;
ADD R0.y, -R0, c[22].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[21].x;
MUL_SAT R0.z, R0.y, R0;
MOV R0.x, c[19];
ADD R0.x, -R0, c[20];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].y, -c[19];
MUL_SAT R0.x, R0, R0.y;
MAD R2.w, -R0.z, c[29].x, c[29].y;
MUL R0.y, R0.z, R0.z;
MUL R0.z, R0.y, R2.w;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[29], c[29].y;
MAD R2.w, R0.y, R0.x, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R2.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MOV R3.y, c[23].x;
ADD R3.y, -R3, c[24].x;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
RCP R3.y, R3.y;
ADD R3.x, fragment.texcoord[2].y, -c[23];
MUL_SAT R3.x, R3, R3.y;
MAD R0.y, -R3.x, c[29].x, c[29];
MUL R0.x, R3, R3;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[9].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R4.x, R0.w, R4;
MAD R0.xyz, R0, R4.x, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R3.w, R0, R3;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MAD R0.xyz, R3, R3.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[12].x;
MUL R2.w, R0, R2;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[6].x;
ADD R0.w, -R0, c[7].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[6].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[5].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
MUL_SAT R0.w, R0, R1;
MAD R1.x, -R0.w, c[29], c[29].y;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28].z;
ADD R1.xyz, R0, -R2;
MUL R0.y, fragment.texcoord[2].x, R0.w;
MAD R1.xyz, R0.y, R1, R2;
MUL R0.x, fragment.texcoord[2].z, c[26];
MUL R0.w, R0.x, c[25].x;
POW R0.w, c[28].y, R0.w;
MOV R0.y, c[28].w;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
ADD R0.x, -R0.w, c[28].z;
MAD R0.xyz, R0.x, R2, R1;
MOV R1.xyz, fragment.texcoord[5];
DP3 R1.x, fragment.texcoord[4], R1;
MUL R0.xyz, R0, c[0];
TEX R0.w, fragment.texcoord[6], texture[10], 2D;
MAX R1.x, R1, c[28];
MUL R0.w, R1.x, R0;
MUL R0.xyz, R0.w, R0;
MUL result.color.xyz, R0, c[29].x;
MOV result.color.w, c[28].x;
END
# 167 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL_COOKIE" }
Vector 0 [_LightColor0]
Float 1 [_texTiling]
Float 2 [_texPower]
Float 3 [_groundTexStart]
Float 4 [_groundTexEnd]
Float 5 [_steepTiling]
Float 6 [_steepTexStart]
Float 7 [_steepTexEnd]
Float 8 [_multiPower]
Float 9 [_deepMultiFactor]
Float 10 [_mainMultiFactor]
Float 11 [_highMultiFactor]
Float 12 [_snowMultiFactor]
Float 13 [_deepStart]
Float 14 [_deepEnd]
Float 15 [_mainLoStart]
Float 16 [_mainLoEnd]
Float 17 [_mainHiStart]
Float 18 [_mainHiEnd]
Float 19 [_hiLoStart]
Float 20 [_hiLoEnd]
Float 21 [_hiHiStart]
Float 22 [_hiHiEnd]
Float 23 [_snowStart]
Float 24 [_snowEnd]
Float 25 [_heightDensityAtViewer]
Float 26 [_globalDensity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightTexture0] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c27, 0.00000000, 2.71828198, 1.00000000, 0.50000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
dcl_texcoord6 v6.xy
mov r0.x, c4
add r0.w, -c3.x, r0.x
mul r0.xyz, v0, c1.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c3.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.z
mov r1.z, c18.x
add r1.z, -c17.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c17.x
mul_sat r1.w, r1.z, r1
mov r1.y, c16.x
add r1.y, -c15.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c15.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c8.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c2.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c14.x
add r2.w, -c13.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c13.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.z
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c22.x
add r1.y, -c21.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c21.x
mul_sat r1.z, r1.y, r1
mov r1.x, c20
add r1.x, -c19, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c19
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c24.x
add r3.x, -c23, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c23
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c9.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c10.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c12.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c5.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mul r0.x, v2.z, c26
mul r1.w, r0.x, c25.x
mov r0.y, c7.x
add r0.y, -c6.x, r0
rcp r0.y, r0.y
add r0.x, v2.z, -c6
mul_sat r2.x, r0, r0.y
pow r0, c27.y, r1.w
mad r0.z, -r2.x, c28.x, c28.y
mul r0.y, r2.x, r2.x
mad r0.y, -r0, r0.z, c27.z
mov r0.w, r0.x
mul r0.y, v2.x, r0
mad r1.xyz, r0.y, r1, r3
mov r0.y, c27.w
mov r0.x, v2.w
texld r0.xyz, r0, s9
add r2.xyz, r0, -r1
add r0.x, -r0.w, c27.z
mad r0.xyz, r0.x, r2, r1
mov_pp r1.xyz, v5
dp3_pp r1.x, v4, r1
mul_pp r0.xyz, r0, c0
texld r0.w, v6, s10
max_pp r1.x, r1, c27
mul_pp r0.w, r1.x, r0
mul_pp r0.xyz, r0.w, r0
mul_pp oC0.xyz, r0, c28.x
mov_pp oC0.w, c27.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL_COOKIE" }
SetTexture 0 [_deepTex] 2D 1
SetTexture 1 [_mainTex] 2D 3
SetTexture 2 [_highTex] 2D 5
SetTexture 3 [_snowTex] 2D 7
SetTexture 4 [_deepMultiTex] 2D 2
SetTexture 5 [_mainMultiTex] 2D 4
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 8
SetTexture 8 [_steepTex] 2D 9
SetTexture 9 [_fogColorRamp] 2D 10
SetTexture 10 [_LightTexture0] 2D 0
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Float 128 [_texTiling]
Float 132 [_texPower]
Float 136 [_groundTexStart]
Float 140 [_groundTexEnd]
Float 148 [_steepTiling]
Float 152 [_steepTexStart]
Float 156 [_steepTexEnd]
Float 168 [_multiPower]
Float 172 [_deepMultiFactor]
Float 176 [_mainMultiFactor]
Float 180 [_highMultiFactor]
Float 184 [_snowMultiFactor]
Float 188 [_deepStart]
Float 192 [_deepEnd]
Float 196 [_mainLoStart]
Float 200 [_mainLoEnd]
Float 204 [_mainHiStart]
Float 208 [_mainHiEnd]
Float 212 [_hiLoStart]
Float 216 [_hiLoEnd]
Float 220 [_hiHiStart]
Float 224 [_hiHiEnd]
Float 228 [_snowStart]
Float 232 [_snowEnd]
Float 260 [_heightDensityAtViewer]
Float 272 [_globalDensity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedooiclgpfmmklbmgdjnhkonkjblekgmooabaaaaaagabhaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaadadaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcbabgaaaaeaaaaaaaieafaaaafjaaaaaeegiocaaa
aaaaaaaabcaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaad
aagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaa
fkaaaaadaagabaaaakaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaae
aahabaaaadaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaae
aahabaaaafaaaaaaffffaaaafibiaaaeaahabaaaagaaaaaaffffaaaafibiaaae
aahabaaaahaaaaaaffffaaaafibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaae
aahabaaaajaaaaaaffffaaaafibiaaaeaahabaaaakaaaaaaffffaaaagcbaaaad
hcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaa
gcbaaaadhcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagcbaaaadhcbabaaa
agaaaaaagcbaaaaddcbabaaaahaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
agaaaaaaaaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaa
akiacaaaaaaaaaaaamaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaa
bkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaalaaaaaadicaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
bkaabaiaebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaak
ccaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaaaiaaaaaadkiacaaaaaaaaaaa
aiaaaaaaaoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpbkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaa
aaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaa
bkaabaaaaaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaackiacaaaaaaaaaaaakaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaa
aaaaaaaabkiacaaaaaaaaaaaaiaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaa
aaaaaaaaaiaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaa
abaaaaaackaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaia
ebaaaaaaaaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakccaabaaa
abaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaa
aaaaaaajmcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaa
amaaaaaadicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaa
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaak
icaabaaaabaaaaaabkiacaiaebaaaaaaaaaaaaaaamaaaaaackiacaaaaaaaaaaa
amaaaaaaaoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaa
ckaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaa
ckaabaaaabaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaabkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaa
aaaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaa
aaaaaaaaaiaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaa
acaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaaeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaa
adaaaaaakgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaa
ggakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaa
aeaaaaaaegacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaa
egaabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaa
dcaaaaajhcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
aeaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaa
agajbaaaadaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaa
anaaaaaaakiacaaaaaaaaaaaaoaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaa
adaaaaaafgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaanaaaaaadicaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaacaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaa
bkiacaiaebaaaaaaaaaaaaaaanaaaaaackiacaaaaaaaaaaaanaaaaaaaoaaaaak
ccaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
adaaaaaadicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
dcaaaaakicaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaia
ebaaaaaaacaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaa
acaaaaaadiaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
afaaaaaadiaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
afaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaa
fgaobaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
acaaaaaaaagabaaaafaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaa
fgbfbaaaacaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaa
adaaaaaaagaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaoaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaak
bcaabaaaadaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
adaaaaaaaaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaaoaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaa
adaaaaaadcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaa
adaaaaaadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
diaaaaahgcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaahaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaa
ahaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaa
kgakbaaaaaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaapgipcaaaaaaaaaaaakaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaeaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
alaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaaeaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaa
afaaaaaaaagabaaaaeaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
afaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaa
acaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaagaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaa
acaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
kgikcaaaaaaaaaaaalaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaaiaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaaiaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaa
acaaaaaaeghobaaaahaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaaaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaa
dcaaaaakhcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaia
ebaaaaaaaeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaa
fgifcaaaaaaaaaaaajaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaajaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaajaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaa
abaaaaaaeghobaaaaiaaaaaaaagabaaaajaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaadaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaak
icaabaaaaaaaaaaackiacaiaebaaaaaaaaaaaaaaajaaaaaadkiacaaaaaaaaaaa
ajaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaa
ckiacaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaa
dkbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaaj
pcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaakaaaaaa
aaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaa
diaaaaaiicaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaabbaaaaaa
diaaaaaiicaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaabaaaaaaa
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaia
ebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaa
egacbaaaaaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaa
egbcbaaaafaaaaaaegbcbaaaagaaaaaadeaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaahaaaaaa
eghobaaaakaaaaaaaagabaaaaaaaaaaaapaaaaahicaabaaaaaaaaaaapgapbaaa
aaaaaaaapgapbaaaabaaaaaadiaaaaahhccabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaadoaaaaab
"
}
}
 }
 Pass {
  Name "PREPASS"
  Tags { "LIGHTMODE"="PrePassBase" }
  Fog { Mode Off }
  Blend OneMinusSrcAlpha SrcAlpha
Program "vp" {
SubProgram "opengl " {
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [unity_Scale]
Vector 14 [_tintColor]
Float 15 [_steepPower]
Float 16 [_saturation]
Float 17 [_contrast]
Vector 18 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[20] = { { 0, 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..18],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[19];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[16].x, R0;
MAD R2.xyz, -c[14], c[14].w, R0;
MUL R0.xyz, c[14], c[14].w;
MAD result.texcoord[3].xyz, R2, c[17].x, R0;
MUL R0.xyz, vertex.normal, c[13].w;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.w, R1, R1;
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP3 result.texcoord[4].z, R0, c[11];
DP3 result.texcoord[4].y, R0, c[10];
DP3 result.texcoord[4].x, R0, c[9];
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[15];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[18];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 29 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [unity_Scale]
Vector 13 [_tintColor]
Float 14 [_steepPower]
Float 15 [_saturation]
Float 16 [_contrast]
Vector 17 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
def c18, 0.29899999, 0.58700001, 0.11400000, 0.00000000
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mov r0.yz, c18.w
dp3 r0.x, v4, c18
add r1.xyz, v4, -r0
mad r0.xyz, r1, c15.x, r0
mad r2.xyz, -c13, c13.w, r0
mul r0.xyz, c13, c13.w
mad o4.xyz, r2, c16.x, r0
mul r0.xyz, v1, c12.w
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.w, r1, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r1
dp3 o5.z, r0, c10
dp3 o5.y, r0, c9
dp3 o5.x, r0, c8
dp4 r0.x, v0, c2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c17
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c14
"
}
SubProgram "d3d11 " {
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 240
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerDraw" 1
"vs_4_0
eefiecediioaabeefcjbdanainnfgahcogjjiibmabaaaaaaeiagaaaaadaaaaaa
cmaaaaaapeaaaaaakmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheolaaaaaaaagaaaaaa
aiaaaaaajiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaakeaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaakeaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaakeaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklfdeieefcjeaeaaaaeaaaabaacfabaaaafjaaaaae
egiocaaaaaaaaaaaapaaaaaafjaaaaaeegiocaaaabaaaaaabfaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaa
fpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaa
gfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaa
afaaaaaagiaaaaacacaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaa
egiocaaaabaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
aaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaabaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pccabaaaaaaaaaaaegiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaa
aaaaaaaadgaaaaafdcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaa
aaaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaah
hcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaafhccabaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaa
aaaaaaaabaaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaa
aaaaaaaadiaaaaaibcaabaaaaaaaaaaabkbabaaaaaaaaaaackiacaaaabaaaaaa
afaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaabaaaaaaaeaaaaaaakbabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaabaaaaaa
agaaaaaackbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
ckiacaaaabaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaag
eccabaaaadaaaaaaakaabaiaebaaaaaaaaaaaaaadicaaaaibccabaaaadaaaaaa
bkbabaaaaeaaaaaaakiacaaaaaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaa
dkbabaaaafaaaaaabaaaaaakbcaabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaa
ihbgjjdokcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaabaaaaaaigacbaia
ebaaaaaaaaaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaaaaaaaaakgikcaaa
aaaaaaaaanaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaa
aaaaaaaaegiccaiaebaaaaaaaaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaa
egacbaaaaaaaaaaadiaaaaajhcaabaaaabaaaaaapgipcaaaaaaaaaaaadaaaaaa
egiccaaaaaaaaaaaadaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaa
anaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaa
egbcbaaaacaaaaaapgipcaaaabaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaa
fgafbaaaaaaaaaaaegiccaaaabaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaa
egiicaaaabaaaaaaamaaaaaaagaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaak
hccabaaaafaaaaaaegiccaaaabaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaa
aaaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
"3.0-!!ARBfp1.0
PARAM c[27] = { program.local[0..25],
		{ 0, 0.5 } };
MAD result.color.xyz, fragment.texcoord[4], c[26].y, c[26].y;
MOV result.color.w, c[26].x;
END
# 2 instructions, 0 R-regs
"
}
SubProgram "d3d9 " {
"ps_3_0
def c0, 0.50000000, 0.00000000, 0, 0
dcl_texcoord4 v4.xyz
mad_pp oC0.xyz, v4, c0.x, c0.x
mov_pp oC0.w, c0.y
"
}
SubProgram "d3d11 " {
"ps_4_0
eefiecedmhjejidinmkcieimpogleacmpcodciadabaaaaaajeabaaaaadaaaaaa
cmaaaaaaoeaaaaaabiabaaaaejfdeheolaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahaaaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaakeaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaaaaaakeaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcheaaaaaa
eaaaaaaabnaaaaaagcbaaaadhcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaa
dcaaaaaphccabaaaaaaaaaaaegbcbaaaafaaaaaaaceaaaaaaaaaaadpaaaaaadp
aaaaaadpaaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaadgaaaaaf
iccabaaaaaaaaaaaabeaaaaaaaaaaaaadoaaaaab"
}
}
 }
 Pass {
  Name "PREPASS"
  Tags { "LIGHTMODE"="PrePassFinal" }
  ZWrite Off
  Blend OneMinusSrcAlpha SrcAlpha
Program "vp" {
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_ProjectionParams]
Vector 14 [unity_SHAr]
Vector 15 [unity_SHAg]
Vector 16 [unity_SHAb]
Vector 17 [unity_SHBr]
Vector 18 [unity_SHBg]
Vector 19 [unity_SHBb]
Vector 20 [unity_SHC]
Vector 21 [unity_Scale]
Vector 22 [_tintColor]
Float 23 [_steepPower]
Float 24 [_saturation]
Float 25 [_contrast]
Vector 26 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[28] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..26],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MUL R1.xyz, vertex.normal, c[21].w;
DP3 R2.w, R1, c[10];
DP3 R0.x, R1, c[9];
DP3 R0.z, R1, c[11];
MOV R0.y, R2.w;
MUL R1, R0.xyzz, R0.yzzx;
MOV R0.w, c[0].y;
DP4 R2.z, R0, c[16];
DP4 R2.y, R0, c[15];
DP4 R2.x, R0, c[14];
MUL R0.y, R2.w, R2.w;
DP4 R3.z, R1, c[19];
DP4 R3.y, R1, c[18];
DP4 R3.x, R1, c[17];
ADD R3.xyz, R2, R3;
MAD R0.w, R0.x, R0.x, -R0.y;
MUL R2.xyz, R0.w, c[20];
MOV R1.yz, c[0].x;
DP3 R1.x, vertex.color, c[27];
ADD R0.xyz, vertex.color, -R1;
MAD R0.xyz, R0, c[24].x, R1;
ADD result.texcoord[5].xyz, R3, R2;
MAD R3.xyz, -c[22], c[22].w, R0;
MUL R1.xyz, c[22], c[22].w;
MAD result.texcoord[3].xyz, R3, c[25].x, R1;
DP4 R0.w, vertex.position, c[8];
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[23];
MIN R0.x, R0, c[0].y;
ADD result.texcoord[4].xy, R2, R2.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[26];
MOV result.texcoord[4].zw, R0;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 48 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_SHAr]
Vector 15 [unity_SHAg]
Vector 16 [unity_SHAb]
Vector 17 [unity_SHBr]
Vector 18 [unity_SHBg]
Vector 19 [unity_SHBb]
Vector 20 [unity_SHC]
Vector 21 [unity_Scale]
Vector 22 [_tintColor]
Float 23 [_steepPower]
Float 24 [_saturation]
Float 25 [_contrast]
Vector 26 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c27, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c28, 0.50000000, 1.00000000, 0, 0
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mul r1.xyz, v1, c21.w
dp3 r2.w, r1, c9
dp3 r0.x, r1, c8
dp3 r0.z, r1, c10
mov r0.y, r2.w
mul r1, r0.xyzz, r0.yzzx
mov r0.w, c28.y
dp4 r2.z, r0, c16
dp4 r2.y, r0, c15
dp4 r2.x, r0, c14
mul r0.y, r2.w, r2.w
dp4 r3.z, r1, c19
dp4 r3.y, r1, c18
dp4 r3.x, r1, c17
add r3.xyz, r2, r3
mad r0.w, r0.x, r0.x, -r0.y
mul r2.xyz, r0.w, c20
mov r1.yz, c27.w
dp3 r1.x, v4, c27
add r0.xyz, v4, -r1
mad r0.xyz, r0, c24.x, r1
add o6.xyz, r3, r2
mad r3.xyz, -c22, c22.w, r0
mul r1.xyz, c22, c22.w
mad o4.xyz, r3, c25.x, r1
dp4 r0.w, v0, c7
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c28.x
mul r2.y, r2, c12.x
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mad o5.xy, r2.z, c13.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c26
mov o5.zw, r0
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c23
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 256
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityLighting" 720
Vector 608 [unity_SHAr]
Vector 624 [unity_SHAg]
Vector 640 [unity_SHAb]
Vector 656 [unity_SHBr]
Vector 672 [unity_SHBg]
Vector 688 [unity_SHBb]
Vector 704 [unity_SHC]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecediaopilbocfpijkddffjbjealccfllkloabaaaaaajaaiaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmeagaaaaeaaaabaalbabaaaafjaaaaaeegiocaaaaaaaaaaa
apaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
cnaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaa
aeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaadpccabaaa
adaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaagfaaaaad
hccabaaaagaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaa
abaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaabaaaaaai
iccabaaaadaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaabaaaaaadiaaaaai
bcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaak
bcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaackbabaaa
aaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaa
ahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaaadaaaaaa
akaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaa
akiacaaaaaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaa
baaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdp
nfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaaabaaaaaa
egbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaaanaaaaaa
egacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaaegiccaia
ebaaaaaaaaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaa
diaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaa
adaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
akiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaa
aceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaafaaaaaa
kgaobaaaaaaaaaaaaaaaaaahdccabaaaafaaaaaakgakbaaaabaaaaaamgaabaaa
abaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaaadaaaaaa
beaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaaadaaaaaa
anaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaadaaaaaaamaaaaaaagaabaaa
aaaaaaaaegaibaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaa
aoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaaficaabaaaaaaaaaaa
abeaaaaaaaaaiadpbbaaaaaibcaabaaaabaaaaaaegiocaaaacaaaaaacgaaaaaa
egaobaaaaaaaaaaabbaaaaaiccaabaaaabaaaaaaegiocaaaacaaaaaachaaaaaa
egaobaaaaaaaaaaabbaaaaaiecaabaaaabaaaaaaegiocaaaacaaaaaaciaaaaaa
egaobaaaaaaaaaaadiaaaaahpcaabaaaacaaaaaajgacbaaaaaaaaaaaegakbaaa
aaaaaaaabbaaaaaibcaabaaaadaaaaaaegiocaaaacaaaaaacjaaaaaaegaobaaa
acaaaaaabbaaaaaiccaabaaaadaaaaaaegiocaaaacaaaaaackaaaaaaegaobaaa
acaaaaaabbaaaaaiecaabaaaadaaaaaaegiocaaaacaaaaaaclaaaaaaegaobaaa
acaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaa
diaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaak
bcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaadcaaaaakhccabaaaagaaaaaaegiccaaaacaaaaaacmaaaaaaagaabaaa
aaaaaaaaegacbaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_ProjectionParams]
Vector 14 [unity_ShadowFadeCenterAndType]
Vector 15 [_tintColor]
Float 16 [_steepPower]
Float 17 [_saturation]
Float 18 [_contrast]
Vector 19 [_sunLightDirection]
Vector 20 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[22] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..20],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
DP4 R0.w, vertex.position, c[8];
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[21];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[17].x, R0;
MAD R3.xyz, -c[15], c[15].w, R0;
MUL R1.xyz, c[15], c[15].w;
MAD result.texcoord[3].xyz, R3, c[18].x, R1;
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MOV R0.x, c[0].y;
ADD R0.x, R0, -c[14].w;
MUL result.texcoord[6].w, -R0.y, R0.x;
MUL R0.x, vertex.texcoord[1].y, c[16];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[19];
DP4 R1.z, vertex.position, c[11];
DP4 R1.x, vertex.position, c[9];
DP4 R1.y, vertex.position, c[10];
ADD R1.xyz, R1, -c[14];
ADD result.texcoord[4].xy, R2, R2.z;
MOV result.texcoord[4].zw, R0;
MUL result.texcoord[6].xyz, R1, c[14].w;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[20], c[20].zwzw;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 39 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_ShadowFadeCenterAndType]
Vector 15 [_tintColor]
Float 16 [_steepPower]
Float 17 [_saturation]
Float 18 [_contrast]
Vector 19 [_sunLightDirection]
Vector 20 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c21, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c22, 0.50000000, 1.00000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v1
dcl_texcoord1 v2
dcl_color0 v3
dp4 r0.w, v0, c7
mov r0.yz, c21.w
dp3 r0.x, v3, c21
add r1.xyz, v3, -r0
mad r0.xyz, r1, c17.x, r0
mad r3.xyz, -c15, c15.w, r0
mul r1.xyz, c15, c15.w
mad o4.xyz, r3, c18.x, r1
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c22.x
mul r2.y, r2, c12.x
mov r1.z, v2.x
mov r1.xy, v1
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
mov r0.x, c14.w
add r0.y, c22, -r0.x
dp4 r0.x, v0, c2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c19
dp4 r1.z, v0, c10
dp4 r1.x, v0, c8
dp4 r1.y, v0, c9
add r1.xyz, r1, -c14
mad o5.xy, r2.z, c13.zwzw, r2
mov o5.zw, r0
mul o7.xyz, r1, c14.w
mad o6.xy, v2, c20, c20.zwzw
mul o7.w, -r0.x, r0.y
mov o3.z, -r0.x
mov o3.y, v3.w
mul_sat o3.x, v2.y, c16
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 288
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
Vector 240 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityShadows" 416
Vector 400 [unity_ShadowFadeCenterAndType]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityShadows" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedmpgepgcogoooojpddhemjijjjgmbkehhabaaaaaaomahaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaadamaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
aiagaaaaeaaaabaaicabaaaafjaaaaaeegiocaaaaaaaaaaabaaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaabkaaaaaafjaaaaae
egiocaaaadaaaaaabaaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaae
pccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaa
acaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
pccabaaaafaaaaaagfaaaaaddccabaaaagaaaaaagfaaaaadpccabaaaahaaaaaa
giaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaa
adaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaaaaaaaaa
agbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaabaaaaaa
egbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaa
abaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaa
egacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaabaaaaaadgaaaaag
hccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaabaaaaaaiiccabaaaadaaaaaa
egiccaaaaaaaaaaaaoaaaaaaegacbaaaabaaaaaadiaaaaaibcaabaaaabaaaaaa
bkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakbcaabaaaabaaaaaa
ckiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaak
bcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaa
aaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaaadaaaaaaakaabaiaebaaaaaa
abaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaaaaaaaaaa
afaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaakbcaabaaa
acaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdnaaaaaaaa
dgaaaaaigcaabaaaacaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaiocaabaaaabaaaaaaagakbaiaebaaaaaaacaaaaaaagbjbaaaafaaaaaa
dcaaaaakocaabaaaabaaaaaakgikcaaaaaaaaaaaanaaaaaafgaobaaaabaaaaaa
agajbaaaacaaaaaadcaaaaamocaabaaaabaaaaaaagijcaiaebaaaaaaaaaaaaaa
adaaaaaapgipcaaaaaaaaaaaadaaaaaafgaobaaaabaaaaaadiaaaaajhcaabaaa
acaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaadcaaaaak
hccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaajgahbaaaabaaaaaaegacbaaa
acaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaa
afaaaaaadiaaaaakncaabaaaacaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadp
aaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaafaaaaaakgaobaaaaaaaaaaa
aaaaaaahdccabaaaafaaaaaakgakbaaaacaaaaaamgaabaaaacaaaaaadcaaaaal
dccabaaaagaaaaaaegbabaaaaeaaaaaaegiacaaaaaaaaaaaapaaaaaaogikcaaa
aaaaaaaaapaaaaaaaaaaaaajbcaabaaaaaaaaaaadkiacaiaebaaaaaaacaaaaaa
bjaaaaaaabeaaaaaaaaaiadpdiaaaaaiiccabaaaahaaaaaaakaabaaaaaaaaaaa
akaabaiaebaaaaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaafgbfbaaaaaaaaaaa
egiccaaaadaaaaaaanaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaa
amaaaaaaagbabaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaa
egiccaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaak
hcaabaaaaaaaaaaaegiccaaaadaaaaaaapaaaaaapgbpbaaaaaaaaaaaegacbaaa
aaaaaaaaaaaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaiaebaaaaaa
acaaaaaabjaaaaaadiaaaaaihccabaaaahaaaaaaegacbaaaaaaaaaaapgipcaaa
acaaaaaabjaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Vector 9 [_ProjectionParams]
Vector 10 [_tintColor]
Float 11 [_steepPower]
Float 12 [_saturation]
Float 13 [_contrast]
Vector 14 [_sunLightDirection]
Vector 15 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[17] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..15],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
DP4 R0.w, vertex.position, c[8];
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[16];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[12].x, R0;
MAD R3.xyz, -c[10], c[10].w, R0;
MUL R1.xyz, c[10], c[10].w;
MAD result.texcoord[3].xyz, R3, c[13].x, R1;
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[9].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[11];
MIN R0.x, R0, c[0].y;
ADD result.texcoord[4].xy, R2, R2.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[14];
MOV result.texcoord[4].zw, R0;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[15], c[15].zwzw;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 31 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Vector 8 [_ProjectionParams]
Vector 9 [_ScreenParams]
Vector 10 [_tintColor]
Float 11 [_steepPower]
Float 12 [_saturation]
Float 13 [_contrast]
Vector 14 [_sunLightDirection]
Vector 15 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c16, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c17, 0.50000000, 0, 0, 0
dcl_position0 v0
dcl_texcoord0 v1
dcl_texcoord1 v2
dcl_color0 v3
dp4 r0.w, v0, c7
mov r0.yz, c16.w
dp3 r0.x, v3, c16
add r1.xyz, v3, -r0
mad r0.xyz, r1, c12.x, r0
mad r3.xyz, -c10, c10.w, r0
mul r1.xyz, c10, c10.w
mad o4.xyz, r3, c13.x, r1
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c17.x
mul r2.y, r2, c8.x
mov r1.z, v2.x
mov r1.xy, v1
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mad o5.xy, r2.z, c9.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c14
mov o5.zw, r0
mad o6.xy, v2, c15, c15.zwzw
mov o3.z, -r0.x
mov o3.y, v3.w
mul_sat o3.x, v2.y, c11
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 288
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
Vector 240 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedgakggokkhagccfhfdhgidfcppnnppgfaabaaaaaajiagaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmmaeaaaaeaaaabaaddabaaaafjaaaaaeegiocaaaaaaaaaaa
baaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
aiaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaa
gfaaaaaddccabaaaagaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaa
baaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaa
aeaaaaaaakiacaaaaaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaa
afaaaaaabaaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdo
kcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaa
abaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaa
anaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaa
egiccaiaebaaaaaaaaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaa
aaaaaaaaadaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaa
aaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaa
afaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaafaaaaaakgakbaaaabaaaaaa
mgaabaaaabaaaaaadcaaaaaldccabaaaagaaaaaaegbabaaaaeaaaaaaegiacaaa
aaaaaaaaapaaaaaaogikcaaaaaaaaaaaapaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_ProjectionParams]
Vector 14 [unity_SHAr]
Vector 15 [unity_SHAg]
Vector 16 [unity_SHAb]
Vector 17 [unity_SHBr]
Vector 18 [unity_SHBg]
Vector 19 [unity_SHBb]
Vector 20 [unity_SHC]
Vector 21 [unity_Scale]
Vector 22 [_tintColor]
Float 23 [_steepPower]
Float 24 [_saturation]
Float 25 [_contrast]
Vector 26 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[28] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..26],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MUL R1.xyz, vertex.normal, c[21].w;
DP3 R2.w, R1, c[10];
DP3 R0.x, R1, c[9];
DP3 R0.z, R1, c[11];
MOV R0.y, R2.w;
MUL R1, R0.xyzz, R0.yzzx;
MOV R0.w, c[0].y;
DP4 R2.z, R0, c[16];
DP4 R2.y, R0, c[15];
DP4 R2.x, R0, c[14];
MUL R0.y, R2.w, R2.w;
DP4 R3.z, R1, c[19];
DP4 R3.y, R1, c[18];
DP4 R3.x, R1, c[17];
ADD R3.xyz, R2, R3;
MAD R0.w, R0.x, R0.x, -R0.y;
MUL R2.xyz, R0.w, c[20];
MOV R1.yz, c[0].x;
DP3 R1.x, vertex.color, c[27];
ADD R0.xyz, vertex.color, -R1;
MAD R0.xyz, R0, c[24].x, R1;
ADD result.texcoord[5].xyz, R3, R2;
MAD R3.xyz, -c[22], c[22].w, R0;
MUL R1.xyz, c[22], c[22].w;
MAD result.texcoord[3].xyz, R3, c[25].x, R1;
DP4 R0.w, vertex.position, c[8];
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[23];
MIN R0.x, R0, c[0].y;
ADD result.texcoord[4].xy, R2, R2.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[26];
MOV result.texcoord[4].zw, R0;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 48 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_SHAr]
Vector 15 [unity_SHAg]
Vector 16 [unity_SHAb]
Vector 17 [unity_SHBr]
Vector 18 [unity_SHBg]
Vector 19 [unity_SHBb]
Vector 20 [unity_SHC]
Vector 21 [unity_Scale]
Vector 22 [_tintColor]
Float 23 [_steepPower]
Float 24 [_saturation]
Float 25 [_contrast]
Vector 26 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c27, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c28, 0.50000000, 1.00000000, 0, 0
dcl_position0 v0
dcl_normal0 v1
dcl_texcoord0 v2
dcl_texcoord1 v3
dcl_color0 v4
mul r1.xyz, v1, c21.w
dp3 r2.w, r1, c9
dp3 r0.x, r1, c8
dp3 r0.z, r1, c10
mov r0.y, r2.w
mul r1, r0.xyzz, r0.yzzx
mov r0.w, c28.y
dp4 r2.z, r0, c16
dp4 r2.y, r0, c15
dp4 r2.x, r0, c14
mul r0.y, r2.w, r2.w
dp4 r3.z, r1, c19
dp4 r3.y, r1, c18
dp4 r3.x, r1, c17
add r3.xyz, r2, r3
mad r0.w, r0.x, r0.x, -r0.y
mul r2.xyz, r0.w, c20
mov r1.yz, c27.w
dp3 r1.x, v4, c27
add r0.xyz, v4, -r1
mad r0.xyz, r0, c24.x, r1
add o6.xyz, r3, r2
mad r3.xyz, -c22, c22.w, r0
mul r1.xyz, c22, c22.w
mad o4.xyz, r3, c25.x, r1
dp4 r0.w, v0, c7
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c28.x
mul r2.y, r2, c12.x
mov r1.z, v3.x
mov r1.xy, v2
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mad o5.xy, r2.z, c13.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c26
mov o5.zw, r0
mov o3.z, -r0.x
mov o3.y, v4.w
mul_sat o3.x, v3.y, c23
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 256
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityLighting" 720
Vector 608 [unity_SHAr]
Vector 624 [unity_SHAg]
Vector 640 [unity_SHAb]
Vector 656 [unity_SHBr]
Vector 672 [unity_SHBg]
Vector 688 [unity_SHBb]
Vector 704 [unity_SHC]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecediaopilbocfpijkddffjbjealccfllkloabaaaaaajaaiaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmeagaaaaeaaaabaalbabaaaafjaaaaaeegiocaaaaaaaaaaa
apaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
cnaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaa
aeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaadpccabaaa
adaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaagfaaaaad
hccabaaaagaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaa
abaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaabaaaaaai
iccabaaaadaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaabaaaaaadiaaaaai
bcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaak
bcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaackbabaaa
aaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaa
ahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaaadaaaaaa
akaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaa
akiacaaaaaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaa
baaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdp
nfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaaabaaaaaa
egbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaaanaaaaaa
egacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaaegiccaia
ebaaaaaaaaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaa
diaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaa
adaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
akiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaa
aceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaafaaaaaa
kgaobaaaaaaaaaaaaaaaaaahdccabaaaafaaaaaakgakbaaaabaaaaaamgaabaaa
abaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaaadaaaaaa
beaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaaadaaaaaa
anaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaadaaaaaaamaaaaaaagaabaaa
aaaaaaaaegaibaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaa
aoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaaficaabaaaaaaaaaaa
abeaaaaaaaaaiadpbbaaaaaibcaabaaaabaaaaaaegiocaaaacaaaaaacgaaaaaa
egaobaaaaaaaaaaabbaaaaaiccaabaaaabaaaaaaegiocaaaacaaaaaachaaaaaa
egaobaaaaaaaaaaabbaaaaaiecaabaaaabaaaaaaegiocaaaacaaaaaaciaaaaaa
egaobaaaaaaaaaaadiaaaaahpcaabaaaacaaaaaajgacbaaaaaaaaaaaegakbaaa
aaaaaaaabbaaaaaibcaabaaaadaaaaaaegiocaaaacaaaaaacjaaaaaaegaobaaa
acaaaaaabbaaaaaiccaabaaaadaaaaaaegiocaaaacaaaaaackaaaaaaegaobaaa
acaaaaaabbaaaaaiecaabaaaadaaaaaaegiocaaaacaaaaaaclaaaaaaegaobaaa
acaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaa
diaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaak
bcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaadcaaaaakhccabaaaagaaaaaaegiccaaaacaaaaaacmaaaaaaagaabaaa
aaaaaaaaegacbaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 9 [_Object2World]
Vector 13 [_ProjectionParams]
Vector 14 [unity_ShadowFadeCenterAndType]
Vector 15 [_tintColor]
Float 16 [_steepPower]
Float 17 [_saturation]
Float 18 [_contrast]
Vector 19 [_sunLightDirection]
Vector 20 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[22] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..20],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
DP4 R0.w, vertex.position, c[8];
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[21];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[17].x, R0;
MAD R3.xyz, -c[15], c[15].w, R0;
MUL R1.xyz, c[15], c[15].w;
MAD result.texcoord[3].xyz, R3, c[18].x, R1;
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[13].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MOV R0.x, c[0].y;
ADD R0.x, R0, -c[14].w;
MUL result.texcoord[6].w, -R0.y, R0.x;
MUL R0.x, vertex.texcoord[1].y, c[16];
MIN R0.x, R0, c[0].y;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[19];
DP4 R1.z, vertex.position, c[11];
DP4 R1.x, vertex.position, c[9];
DP4 R1.y, vertex.position, c[10];
ADD R1.xyz, R1, -c[14];
ADD result.texcoord[4].xy, R2, R2.z;
MOV result.texcoord[4].zw, R0;
MUL result.texcoord[6].xyz, R1, c[14].w;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[20], c[20].zwzw;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 39 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Vector 12 [_ProjectionParams]
Vector 13 [_ScreenParams]
Vector 14 [unity_ShadowFadeCenterAndType]
Vector 15 [_tintColor]
Float 16 [_steepPower]
Float 17 [_saturation]
Float 18 [_contrast]
Vector 19 [_sunLightDirection]
Vector 20 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c21, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c22, 0.50000000, 1.00000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v1
dcl_texcoord1 v2
dcl_color0 v3
dp4 r0.w, v0, c7
mov r0.yz, c21.w
dp3 r0.x, v3, c21
add r1.xyz, v3, -r0
mad r0.xyz, r1, c17.x, r0
mad r3.xyz, -c15, c15.w, r0
mul r1.xyz, c15, c15.w
mad o4.xyz, r3, c18.x, r1
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c22.x
mul r2.y, r2, c12.x
mov r1.z, v2.x
mov r1.xy, v1
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
mov r0.x, c14.w
add r0.y, c22, -r0.x
dp4 r0.x, v0, c2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c19
dp4 r1.z, v0, c10
dp4 r1.x, v0, c8
dp4 r1.y, v0, c9
add r1.xyz, r1, -c14
mad o5.xy, r2.z, c13.zwzw, r2
mov o5.zw, r0
mul o7.xyz, r1, c14.w
mad o6.xy, v2, c20, c20.zwzw
mul o7.w, -r0.x, r0.y
mov o3.z, -r0.x
mov o3.y, v3.w
mul_sat o3.x, v2.y, c16
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 288
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
Vector 240 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityShadows" 416
Vector 400 [unity_ShadowFadeCenterAndType]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityShadows" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedmpgepgcogoooojpddhemjijjjgmbkehhabaaaaaaomahaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaadamaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
aiagaaaaeaaaabaaicabaaaafjaaaaaeegiocaaaaaaaaaaabaaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaabkaaaaaafjaaaaae
egiocaaaadaaaaaabaaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaaddcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaae
pccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaa
acaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
pccabaaaafaaaaaagfaaaaaddccabaaaagaaaaaagfaaaaadpccabaaaahaaaaaa
giaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaa
adaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaaaaaaaaa
agbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaabaaaaaa
egbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaa
abaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaa
egacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaaegacbaaaabaaaaaadgaaaaag
hccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaabaaaaaaiiccabaaaadaaaaaa
egiccaaaaaaaaaaaaoaaaaaaegacbaaaabaaaaaadiaaaaaibcaabaaaabaaaaaa
bkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakbcaabaaaabaaaaaa
ckiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaak
bcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaa
aaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaaadaaaaaaakaabaiaebaaaaaa
abaaaaaadicaaaaibccabaaaadaaaaaabkbabaaaaeaaaaaaakiacaaaaaaaaaaa
afaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaaafaaaaaabaaaaaakbcaabaaa
acaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdokcefbgdpnfhiojdnaaaaaaaa
dgaaaaaigcaabaaaacaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaiocaabaaaabaaaaaaagakbaiaebaaaaaaacaaaaaaagbjbaaaafaaaaaa
dcaaaaakocaabaaaabaaaaaakgikcaaaaaaaaaaaanaaaaaafgaobaaaabaaaaaa
agajbaaaacaaaaaadcaaaaamocaabaaaabaaaaaaagijcaiaebaaaaaaaaaaaaaa
adaaaaaapgipcaaaaaaaaaaaadaaaaaafgaobaaaabaaaaaadiaaaaajhcaabaaa
acaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaaaaaaaaaaadaaaaaadcaaaaak
hccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaajgahbaaaabaaaaaaegacbaaa
acaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaa
afaaaaaadiaaaaakncaabaaaacaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadp
aaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaafaaaaaakgaobaaaaaaaaaaa
aaaaaaahdccabaaaafaaaaaakgakbaaaacaaaaaamgaabaaaacaaaaaadcaaaaal
dccabaaaagaaaaaaegbabaaaaeaaaaaaegiacaaaaaaaaaaaapaaaaaaogikcaaa
aaaaaaaaapaaaaaaaaaaaaajbcaabaaaaaaaaaaadkiacaiaebaaaaaaacaaaaaa
bjaaaaaaabeaaaaaaaaaiadpdiaaaaaiiccabaaaahaaaaaaakaabaaaaaaaaaaa
akaabaiaebaaaaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaafgbfbaaaaaaaaaaa
egiccaaaadaaaaaaanaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaa
amaaaaaaagbabaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaa
egiccaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaak
hcaabaaaaaaaaaaaegiccaaaadaaaaaaapaaaaaapgbpbaaaaaaaaaaaegacbaaa
aaaaaaaaaaaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaiaebaaaaaa
acaaaaaabjaaaaaadiaaaaaihccabaaaahaaaaaaegacbaaaaaaaaaaapgipcaaa
acaaaaaabjaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Vector 9 [_ProjectionParams]
Vector 10 [_tintColor]
Float 11 [_steepPower]
Float 12 [_saturation]
Float 13 [_contrast]
Vector 14 [_sunLightDirection]
Vector 15 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[17] = { { 0, 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..15],
		{ 0.29899999, 0.58700001, 0.114 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
DP4 R0.w, vertex.position, c[8];
MOV R0.yz, c[0].x;
DP3 R0.x, vertex.color, c[16];
ADD R1.xyz, vertex.color, -R0;
MAD R0.xyz, R1, c[12].x, R0;
MAD R3.xyz, -c[10], c[10].w, R0;
MUL R1.xyz, c[10], c[10].w;
MAD result.texcoord[3].xyz, R3, c[13].x, R1;
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R2.xyz, R0.xyww, c[0].z;
MUL R2.y, R2, c[9].x;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
DP3 R0.z, R1, R1;
RSQ R1.w, R0.z;
MUL R1.xyz, R1.w, R1;
DP4 R0.z, vertex.position, c[7];
MOV result.position, R0;
DP4 R0.y, vertex.position, c[3];
MUL R0.x, vertex.texcoord[1].y, c[11];
MIN R0.x, R0, c[0].y;
ADD result.texcoord[4].xy, R2, R2.z;
MOV result.texcoord[0].xyz, R1;
ABS result.texcoord[1].xyz, R1;
DP3 result.texcoord[2].w, R1, c[14];
MOV result.texcoord[4].zw, R0;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[15], c[15].zwzw;
MOV result.texcoord[2].z, -R0.y;
MOV result.texcoord[2].y, vertex.color.w;
MAX result.texcoord[2].x, R0, c[0];
END
# 31 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Vector 8 [_ProjectionParams]
Vector 9 [_ScreenParams]
Vector 10 [_tintColor]
Float 11 [_steepPower]
Float 12 [_saturation]
Float 13 [_contrast]
Vector 14 [_sunLightDirection]
Vector 15 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c16, 0.29899999, 0.58700001, 0.11400000, 0.00000000
def c17, 0.50000000, 0, 0, 0
dcl_position0 v0
dcl_texcoord0 v1
dcl_texcoord1 v2
dcl_color0 v3
dp4 r0.w, v0, c7
mov r0.yz, c16.w
dp3 r0.x, v3, c16
add r1.xyz, v3, -r0
mad r0.xyz, r1, c12.x, r0
mad r3.xyz, -c10, c10.w, r0
mul r1.xyz, c10, c10.w
mad o4.xyz, r3, c13.x, r1
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c17.x
mul r2.y, r2, c8.x
mov r1.z, v2.x
mov r1.xy, v1
dp3 r0.z, r1, r1
rsq r1.w, r0.z
mul r1.xyz, r1.w, r1
dp4 r0.z, v0, c6
mov o0, r0
dp4 r0.x, v0, c2
mad o5.xy, r2.z, c9.zwzw, r2
mov o1.xyz, r1
abs o2.xyz, r1
dp3 o3.w, r1, c14
mov o5.zw, r0
mad o6.xy, v2, c15, c15.zwzw
mov o3.z, -r0.x
mov o3.y, v3.w
mul_sat o3.x, v2.y, c11
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
ConstBuffer "$Globals" 288
Vector 48 [_tintColor]
Float 80 [_steepPower]
Float 216 [_saturation]
Float 220 [_contrast]
Vector 224 [_sunLightDirection]
Vector 240 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedgakggokkhagccfhfdhgidfcppnnppgfaabaaaaaajiagaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahaaaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaahaiaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
apaaaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaagaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmmaeaaaaeaaaabaaddabaaaafjaaaaaeegiocaaaaaaaaaaa
baaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
aiaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaafpaaaaadpcbabaaaafaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaadhccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaa
gfaaaaaddccabaaaagaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaadgaaaaafhccabaaaabaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaa
baaaaaaiiccabaaaadaaaaaaegiccaaaaaaaaaaaaoaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaageccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadicaaaaibccabaaaadaaaaaabkbabaaa
aeaaaaaaakiacaaaaaaaaaaaafaaaaaadgaaaaafcccabaaaadaaaaaadkbabaaa
afaaaaaabaaaaaakbcaabaaaabaaaaaaegbcbaaaafaaaaaaaceaaaaaihbgjjdo
kcefbgdpnfhiojdnaaaaaaaadgaaaaaigcaabaaaabaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaaigacbaiaebaaaaaa
abaaaaaaegbcbaaaafaaaaaadcaaaaakhcaabaaaabaaaaaakgikcaaaaaaaaaaa
anaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaadcaaaaamhcaabaaaabaaaaaa
egiccaiaebaaaaaaaaaaaaaaadaaaaaapgipcaaaaaaaaaaaadaaaaaaegacbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaapgipcaaaaaaaaaaaadaaaaaaegiccaaa
aaaaaaaaadaaaaaadcaaaaakhccabaaaaeaaaaaapgipcaaaaaaaaaaaanaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaa
aaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaa
afaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaafaaaaaakgakbaaaabaaaaaa
mgaabaaaabaaaaaadcaaaaaldccabaaaagaaaaaaegbabaaaaeaaaaaaegiacaaa
aaaaaaaaapaaaaaaogikcaaaaaaaaaaaapaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
TXP R0.xyz, fragment.texcoord[4], texture[10], 2D;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[27].x, R0.w;
MOV R2.y, c[27].z;
MOV R2.x, fragment.texcoord[2].w;
TEX R2.xyz, R2, texture[9], 2D;
ADD R2.xyz, R2, -R1;
ADD R0.w, -R0, c[27].y;
LG2 R0.x, R0.x;
LG2 R0.y, R0.y;
LG2 R0.z, R0.z;
ADD R0.xyz, -R0, fragment.texcoord[5];
MAD R1.xyz, R0.w, R2, R1;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 173 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c27, 2.71828198, 1.00000000, 0.50000000, 0
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4
dcl_texcoord5 v5.xyz
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
mov r0.w, c6.x
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c5.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c5.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c28, c28.y
mul r0.w, r0, r0
mad r1.x, -r0.w, r1, c27.y
mul r0.w, v2.z, c25.x
add r0.xyz, r0, -r3
mul r1.x, v2, r1
mad r1.xyz, r1.x, r0, r3
mul r1.w, r0, c24.x
pow r0, c27.x, r1.w
mov r0.w, r0.x
mov r2.y, c27.z
mov r2.x, v2.w
texld r2.xyz, r2, s9
add r3.xyz, r2, -r1
texldp r2.xyz, v4, s10
add r0.w, -r0, c27.y
log_pp r0.x, r2.x
log_pp r0.z, r2.z
log_pp r0.y, r2.y
add_pp r0.xyz, -r0, v5
mad r1.xyz, r0.w, r3, r1
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
ConstBuffer "$Globals" 256
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedfkgaagimhegpljgnpekdhhcidlkafhfkabaaaaaabmbhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcoebfaaaaeaaaaaaahjafaaaa
fjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafibiaaaeaahabaaaaaaaaaaa
ffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaa
ffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaa
ffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaaeaahabaaaagaaaaaa
ffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaaeaahabaaaaiaaaaaa
ffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaaeaahabaaaakaaaaaa
ffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaad
pcbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaadlcbabaaaafaaaaaa
gcbaaaadhcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaa
aaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaa
aaaaaaaaaiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaa
adaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaia
ebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaa
aoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
bkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
bkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaa
bkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckiacaaaaaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaa
bkiacaaaaaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaa
aeaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaa
ckaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaa
aaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaa
aaaaaaaaaiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaaj
mcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaa
dicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaaj
icaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaa
abaaaaaabkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaa
aoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
bkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
aeaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaa
eghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaa
kgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaa
egacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaa
egacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaaj
hcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaa
adaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaa
akiacaaaaaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaa
fgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaa
dkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaa
dicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaak
icaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaa
acaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaa
diaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaa
diaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaa
dcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaa
aagabaaaaeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaa
acaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaa
agaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaa
aaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaa
akaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaah
gcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaa
aaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
pgipcaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
acaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaa
egbcbaaaabaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaa
aaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaak
hcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaa
aeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaa
aaaaaaaaafaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaa
aoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaa
dkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaa
adaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaa
abaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaai
icaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaai
icaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaa
afaaaaaapgbpbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaakaaaaaaaagabaaaakaaaaaacpaaaaafhcaabaaaabaaaaaaegacbaaa
abaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaabaaaaaaegbcbaaa
agaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaa
dgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
Vector 27 [unity_LightmapFade]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
SetTexture 12 [unity_LightmapInd] 2D 12
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[28];
ADD R0.y, R1.w, c[29].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[28].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[28].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[28];
ADD R2.x, R2, c[29];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[29].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[28].w;
ADD R0.y, R4.x, c[29].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[28].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[28].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[29].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[28].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[29].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[28].w;
ADD R0.y, R3.x, c[29].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
MAD R2.xyz, R1, R4.z, R2;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MUL R3.xyz, fragment.texcoord[0], c[4].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[28];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[29];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R2.xyz, R0.x, R1, R2;
TEX R1, fragment.texcoord[5], texture[12], 2D;
MUL R1.xyz, R1.w, R1;
MUL R1.xyz, R1, c[29].y;
MOV R0.y, c[28].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R3.xyz, R0, -R2;
TEX R0, fragment.texcoord[5], texture[11], 2D;
MUL R0.xyz, R0.w, R0;
MAD R4.xyz, R0, c[29].y, -R1;
TXP R0.xyz, fragment.texcoord[4], texture[10], 2D;
DP4 R0.w, fragment.texcoord[6], fragment.texcoord[6];
RSQ R0.w, R0.w;
RCP R0.w, R0.w;
MAD_SAT R0.w, R0, c[27].z, c[27];
MAD R1.xyz, R0.w, R4, R1;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[28].x, R0.w;
LG2 R0.x, R0.x;
LG2 R0.y, R0.y;
LG2 R0.z, R0.z;
ADD R0.xyz, -R0, R1;
ADD R0.w, -R0, c[28].y;
MAD R1.xyz, R0.w, R3, R2;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 184 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
Vector 27 [unity_LightmapFade]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
SetTexture 12 [unity_LightmapInd] 2D 12
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
dcl_2d s12
def c28, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c29, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
dcl_texcoord6 v6
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c29.x, c29.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c28.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c29.x, c29.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c29.x, c29
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c29.x, c29
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c28.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c29.x, c29.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c29, c29.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c29.x, c29
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
mad r1.xyz, r1, r4.x, r3
texld r2.xyz, r0.zyzw, s5
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
mad r1.xyz, r3, r2.w, r1
texld r2.xyz, r0.zyzw, s7
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r1.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r1, v3
texld r1.xyz, r0.zyzw, s8
texld r2.xyz, r0, s8
mov r0.y, c6.x
add r0.w, -c5.x, r0.y
rcp r1.w, r0.w
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r2, r1
texld r0.xyz, r0.zxzw, s8
add r0.w, v2.z, -c5.x
mad r0.xyz, r0, v1.y, r1
mul_sat r0.w, r0, r1
mad r1.x, -r0.w, c29, c29.y
mul r0.w, r0, r0
mad r0.w, -r0, r1.x, c28.y
add r1.xyz, r0, -r3
mul r0.x, v2, r0.w
mad r1.xyz, r0.x, r1, r3
texld r0, v5, s12
mul_pp r3.xyz, r0.w, r0
texld r0, v5, s11
mul_pp r0.xyz, r0.w, r0
mul_pp r3.xyz, r3, c28.w
mad_pp r4.xyz, r0, c28.w, -r3
mul r0.x, v2.z, c25
dp4 r0.y, v6, v6
rsq r0.y, r0.y
rcp r2.w, r0.y
mul r1.w, r0.x, c24.x
pow r0, c28.x, r1.w
mad_sat r0.y, r2.w, c27.z, c27.w
mad_pp r4.xyz, r0.y, r4, r3
texldp r3.xyz, v4, s10
mov r0.w, r0.x
mov r2.y, c28.z
mov r2.x, v2.w
texld r2.xyz, r2, s9
add r2.xyz, r2, -r1
add r0.w, -r0, c28.y
log_pp r0.x, r3.x
log_pp r0.y, r3.y
log_pp r0.z, r3.z
add_pp r0.xyz, -r0, r4
mad r1.xyz, r0.w, r2, r1
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
SetTexture 12 [unity_LightmapInd] 2D 12
ConstBuffer "$Globals" 288
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
Vector 256 [unity_LightmapFade]
BindCB  "$Globals" 0
"ps_4_0
eefiecedndbaniohkbjnodecmpdhldodgjhfmhjiabaaaaaalmbiaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapalaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
adadaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaapapaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcgmbhaaaaeaaaaaaanlafaaaafjaaaaaeegiocaaa
aaaaaaaabbaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaad
aagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaa
fkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaafkaaaaadaagabaaa
amaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaa
ffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaa
ffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaa
ffffaaaafibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaa
ffffaaaafibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaa
ffffaaaafibiaaaeaahabaaaakaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaa
ffffaaaafibiaaaeaahabaaaamaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaa
aeaaaaaagcbaaaadlcbabaaaafaaaaaagcbaaaaddcbabaaaagaaaaaagcbaaaad
pcbabaaaahaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaa
aiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
aiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaa
aaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
afaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaabbaaaaahicaabaaaaaaaaaaaegbobaaaahaaaaaa
egbobaaaahaaaaaaelaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadccaaaal
icaabaaaaaaaaaaadkaabaaaaaaaaaaackiacaaaaaaaaaaabaaaaaaadkiacaaa
aaaaaaaabaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaagaaaaaaeghobaaa
amaaaaaaaagabaaaamaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaa
abeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgapbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaagaaaaaaeghobaaaalaaaaaa
aagabaaaalaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdcaaaaakhcaabaaaacaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaa
egacbaiaebaaaaaaabaaaaaadcaaaaajhcaabaaaabaaaaaapgapbaaaaaaaaaaa
egacbaaaacaaaaaaegacbaaaabaaaaaaaoaaaaahdcaabaaaacaaaaaaegbabaaa
afaaaaaapgbpbaaaafaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaacaaaaaa
eghobaaaakaaaaaaaagabaaaakaaaaaacpaaaaafhcaabaaaacaaaaaaegacbaaa
acaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaiaebaaaaaa
acaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaa
dgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MUL R3.xyz, fragment.texcoord[0], c[4].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R2.xyz, R0.x, R1, R2;
TXP R1.xyz, fragment.texcoord[4], texture[10], 2D;
LG2 R1.x, R1.x;
LG2 R1.y, R1.y;
LG2 R1.z, R1.z;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R3.xyz, R0, -R2;
TEX R0, fragment.texcoord[5], texture[11], 2D;
MUL R0.xyz, R0.w, R0;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[27].x, R0.w;
MAD R0.xyz, R0, c[28].y, -R1;
ADD R0.w, -R0, c[27].y;
MAD R1.xyz, R0.w, R3, R2;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 175 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
def c27, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mov r0.w, c6.x
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c5.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c5.x
mul_sat r0.w, r0, r1.x
add r1.xyz, r0, -r3
mul r0.x, v2.z, c25
mul r1.w, r0.x, c24.x
mul r0.y, r0.w, r0.w
mad r0.z, -r0.w, c28.x, c28.y
mad r2.x, -r0.y, r0.z, c27.y
pow r0, c27.x, r1.w
mul r0.y, v2.x, r2.x
mad r2.xyz, r0.y, r1, r3
mov r1.w, r0.x
texld r0, v5, s11
mul_pp r0.xyz, r0.w, r0
mov r1.y, c27.z
mov r1.x, v2.w
texld r1.xyz, r1, s9
add r3.xyz, r1, -r2
texldp r1.xyz, v4, s10
log_pp r1.x, r1.x
log_pp r1.z, r1.z
log_pp r1.y, r1.y
mad_pp r0.xyz, r0, c27.w, -r1
add r0.w, -r1, c27.y
mad r1.xyz, r0.w, r3, r2
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
ConstBuffer "$Globals" 288
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedjdcihhagkhhbdiicdipbkhbmokcegckoabaaaaaaiabhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
adadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefceibgaaaaeaaaaaaajcafaaaa
fjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
fibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaa
fibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaa
fibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaa
fibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaa
fibiaaaeaahabaaaakaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaaffffaaaa
gcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaa
adaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaadlcbabaaaafaaaaaagcbaaaad
dcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaa
aiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
aiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaa
aaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
afaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaaafaaaaaa
pgbpbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
akaaaaaaaagabaaaakaaaaaacpaaaaafhcaabaaaabaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaaegbabaaaagaaaaaaeghobaaaalaaaaaaaagabaaa
alaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaaeb
dcaaaaakhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaacaaaaaaegacbaia
ebaaaaaaabaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
abaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaadoaaaaab
"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[27].x, R0.w;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
TXP R0.xyz, fragment.texcoord[4], texture[10], 2D;
ADD R0.w, -R0, c[27].y;
ADD R0.xyz, R0, fragment.texcoord[5];
MAD R1.xyz, R0.w, R2, R1;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 170 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
def c27, 2.71828198, 1.00000000, 0.50000000, 0
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4
dcl_texcoord5 v5.xyz
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mad r1.xyz, v1.z, r1, r2
mov r0.w, c6.x
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c5.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c5.x
mul_sat r0.w, r0, r1.x
add r1.xyz, r0, -r3
mul r0.x, v2.z, c25
mul r0.y, r0.w, r0.w
mad r0.z, -r0.w, c28.x, c28.y
mad r2.x, -r0.y, r0.z, c27.y
mul r1.w, r0.x, c24.x
pow r0, c27.x, r1.w
mul r0.y, v2.x, r2.x
mov r0.w, r0.x
mad r1.xyz, r0.y, r1, r3
texldp r0.xyz, v4, s10
mov r2.y, c27.z
mov r2.x, v2.w
texld r2.xyz, r2, s9
add r2.xyz, r2, -r1
add r0.w, -r0, c27.y
add_pp r0.xyz, r0, v5
mad r1.xyz, r0.w, r2, r1
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
ConstBuffer "$Globals" 256
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedekkakgmmckniphdifngnjeafiipfloecabaaaaaaaebhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmmbfaaaaeaaaaaaahdafaaaa
fjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafibiaaaeaahabaaaaaaaaaaa
ffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaa
ffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaa
ffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaafibiaaaeaahabaaaagaaaaaa
ffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaafibiaaaeaahabaaaaiaaaaaa
ffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaafibiaaaeaahabaaaakaaaaaa
ffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaad
pcbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaadlcbabaaaafaaaaaa
gcbaaaadhcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaa
aaaaaaakbcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaa
aaaaaaaaaiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaa
adaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaia
ebaaaaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaa
aoaaaaakccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
bkaabaaaaaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
bkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaa
bkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaackaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckiacaaaaaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaa
bkiacaaaaaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaa
aeaaaaaadkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaa
ckaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaa
aaaaaaaaakaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaa
aaaaaaaaaiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaaj
mcaabaaaabaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaa
dicaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaaj
icaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaa
abaaaaaabkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaa
aoaaaaakicaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaabaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaa
abaaaaaadcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
bkaabaiaebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaa
bkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaa
aeaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaa
aagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaa
eghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaa
kgakbaaaabaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaa
egacbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaa
egacbaaaafaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaaj
hcaabaaaaeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaa
dcaaaaajncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaa
adaaaaaaaaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaa
akiacaaaaaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaa
fgbfbaaaadaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaa
dkaabaaaacaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaa
acaaaaaadkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaa
dicaaaahbcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaak
icaabaaaacaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaa
acaaaaaadiaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaa
diaaaaahicaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaa
diaaaaahocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaa
dcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaa
aagabaaaaeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaa
acaaaaaafgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaa
agaabaaaadaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaia
ebaaaaaaaaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaa
adaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaa
aaaaaaajccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaa
akaaaaaadicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaa
dcaaaaajccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaa
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaah
gcaabaaaaaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaa
adaaaaaaggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaaj
pcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaa
egacbaaaacaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaa
aaaaaaaaagaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaa
pgipcaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaa
adaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaa
acaaaaaaeghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaa
aagabaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaa
acaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
acaaaaaafgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaa
egbcbaaaabaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaa
egacbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaa
aaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaa
eghobaaaahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaak
hcaabaaaaaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaa
aeaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egbcbaaaaeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaa
aaaaaaaaafaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaa
eghobaaaaiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaabaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaa
aaaaaaaackiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaa
aoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
dkaabaaaaaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaia
ebaaaaaaaaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaabaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaa
dkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaakbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaa
adaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaa
abaaaaaaegaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaai
icaabaaaaaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaai
icaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaa
afaaaaaapgbpbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaakaaaaaaaagabaaaakaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaa
abaaaaaaegbcbaaaagaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaa
doaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
Vector 27 [unity_LightmapFade]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
SetTexture 12 [unity_LightmapInd] 2D 12
"3.0-!!ARBfp1.0
PARAM c[30] = { program.local[0..27],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[28];
ADD R0.y, R1.w, c[29].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[28].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[28].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[28];
ADD R2.x, R2, c[29];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[29].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[28].w;
ADD R0.y, R4.x, c[29].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[28].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[28].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[29].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[28].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[29].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[28].w;
ADD R0.y, R3.x, c[29].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MUL R3.xyz, fragment.texcoord[0], c[4].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[28];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[29];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[28].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R2.xyz, R0.x, R1, R2;
TEX R1, fragment.texcoord[5], texture[12], 2D;
MUL R1.xyz, R1.w, R1;
MUL R1.xyz, R1, c[29].y;
MOV R0.y, c[28].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R3.xyz, R0, -R2;
TEX R0, fragment.texcoord[5], texture[11], 2D;
MUL R0.xyz, R0.w, R0;
DP4 R0.w, fragment.texcoord[6], fragment.texcoord[6];
RSQ R0.w, R0.w;
RCP R1.w, R0.w;
MUL R0.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R0, c[24].x;
POW R0.w, c[28].x, R0.w;
MAD R0.xyz, R0, c[29].y, -R1;
MAD_SAT R1.w, R1, c[27].z, c[27];
MAD R1.xyz, R1.w, R0, R1;
TXP R0.xyz, fragment.texcoord[4], texture[10], 2D;
ADD R0.xyz, R0, R1;
ADD R0.w, -R0, c[28].y;
MAD R1.xyz, R0.w, R3, R2;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 181 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
Vector 27 [unity_LightmapFade]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
SetTexture 12 [unity_LightmapInd] 2D 12
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
dcl_2d s12
def c28, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c29, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
dcl_texcoord6 v6
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c29.x, c29.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c28.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c29.x, c29.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c29.x, c29
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c29.x, c29
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c28.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c29.x, c29.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c29, c29.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c29.x, c29
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
mad r1.xyz, r1, r4.x, r3
texld r2.xyz, r0.zyzw, s5
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
mad r1.xyz, r3, r2.w, r1
texld r2.xyz, r0.zyzw, s7
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r1.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r1, v3
texld r1.xyz, r0.zyzw, s8
texld r2.xyz, r0, s8
mov r0.y, c6.x
add r0.w, -c5.x, r0.y
rcp r1.w, r0.w
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r2, r1
texld r0.xyz, r0.zxzw, s8
add r0.w, v2.z, -c5.x
mad r0.xyz, r0, v1.y, r1
mul_sat r0.w, r0, r1
mad r1.x, -r0.w, c29, c29.y
mul r0.w, r0, r0
mad r0.w, -r0, r1.x, c28.y
add r1.xyz, r0, -r3
mul r0.x, v2, r0.w
mad r1.xyz, r0.x, r1, r3
texld r0, v5, s12
mul_pp r3.xyz, r0.w, r0
texld r0, v5, s11
mul_pp r0.xyz, r0.w, r0
mul_pp r3.xyz, r3, c28.w
mad_pp r4.xyz, r0, c28.w, -r3
mul r0.x, v2.z, c25
dp4 r0.y, v6, v6
rsq r2.w, r0.y
mul r1.w, r0.x, c24.x
pow r0, c28.x, r1.w
rcp r0.y, r2.w
mad_sat r0.y, r0, c27.z, c27.w
mov r0.w, r0.x
mad_pp r3.xyz, r0.y, r4, r3
texldp r0.xyz, v4, s10
mov r2.y, c28.z
mov r2.x, v2.w
texld r2.xyz, r2, s9
add r2.xyz, r2, -r1
add r0.w, -r0, c28.y
add_pp r0.xyz, r0, r3
mad r1.xyz, r0.w, r2, r1
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
SetTexture 12 [unity_LightmapInd] 2D 12
ConstBuffer "$Globals" 288
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
Vector 256 [unity_LightmapFade]
BindCB  "$Globals" 0
"ps_4_0
eefiecedepkjmbhhieaacnddphpodnocllhknjpfabaaaaaakebiaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapalaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
adadaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaahaaaaaaapapaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcfebhaaaaeaaaaaaanfafaaaafjaaaaaeegiocaaa
aaaaaaaabbaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaaagaaaaaafkaaaaad
aagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaadaagabaaaajaaaaaa
fkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaafkaaaaadaagabaaa
amaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaa
ffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaa
ffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaa
ffffaaaafibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaa
ffffaaaafibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaa
ffffaaaafibiaaaeaahabaaaakaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaa
ffffaaaafibiaaaeaahabaaaamaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaa
aeaaaaaagcbaaaadlcbabaaaafaaaaaagcbaaaaddcbabaaaagaaaaaagcbaaaad
pcbabaaaahaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaa
aiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
aiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaa
aaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
afaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaabbaaaaahicaabaaaaaaaaaaaegbobaaaahaaaaaa
egbobaaaahaaaaaaelaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadccaaaal
icaabaaaaaaaaaaadkaabaaaaaaaaaaackiacaaaaaaaaaaabaaaaaaadkiacaaa
aaaaaaaabaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaagaaaaaaeghobaaa
amaaaaaaaagabaaaamaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaa
abeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgapbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaagaaaaaaeghobaaaalaaaaaa
aagabaaaalaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdcaaaaakhcaabaaaacaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaa
egacbaiaebaaaaaaabaaaaaadcaaaaajhcaabaaaabaaaaaapgapbaaaaaaaaaaa
egacbaaaacaaaaaaegacbaaaabaaaaaaaoaaaaahdcaabaaaacaaaaaaegbabaaa
afaaaaaapgbpbaaaafaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaacaaaaaa
eghobaaaakaaaaaaaagabaaaakaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaaanaaaaaa
doaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"3.0-!!ARBfp1.0
PARAM c[29] = { program.local[0..26],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, fragment.texcoord[0], c[0].x;
TEX R1.xyz, R3.zyzw, texture[1], 2D;
MOV R0.w, c[2].x;
ADD R0.w, -R0, c[3].x;
RCP R1.w, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[2].x;
MUL_SAT R0.w, R0, R1;
TEX R0.xyz, R3, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R1.w, -R0, c[27];
ADD R0.y, R1.w, c[28].x;
MUL R0.x, R0.w, R0.w;
MUL R0.w, R0.x, R0.y;
ADD R0.x, -R0.w, c[27].y;
MOV R0.y, c[16].x;
ADD R0.z, -R0.y, c[17].x;
RCP R1.w, R0.z;
ADD R0.z, fragment.texcoord[2].y, -c[16].x;
MUL_SAT R1.w, R0.z, R1;
MUL R2.x, -R1.w, c[27].w;
MOV R0.y, c[14].x;
ADD R0.y, -R0, c[15].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[14].x;
MUL_SAT R0.y, R0, R0.z;
MUL R0.z, R1.w, R1.w;
MUL R1.w, -R0.y, c[27];
ADD R2.x, R2, c[28];
MUL R0.w, R0, c[7].x;
MUL R0.z, R0, R2.x;
MUL R0.y, R0, R0;
ADD R1.w, R1, c[28].x;
MAD R2.w, R0.y, R1, -R0.z;
MUL R1.w, R0.x, c[1].x;
MUL R2.x, R1.w, R2.w;
TEX R0.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R2.xyz, R0, R2.x;
MOV R1.x, c[12];
ADD R3.w, -R1.x, c[13].x;
TEX R1.xyz, R3.zyzw, texture[0], 2D;
RCP R4.x, R3.w;
ADD R3.w, fragment.texcoord[2].y, -c[12].x;
MUL_SAT R3.w, R3, R4.x;
TEX R0.xyz, R3, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MUL R4.x, -R3.w, c[27].w;
ADD R0.y, R4.x, c[28].x;
MUL R0.x, R3.w, R3.w;
MAD R3.w, -R0.x, R0.y, c[27].y;
TEX R0.xyz, R3.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R3.w;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[2], 2D;
TEX R0.xyz, R3, texture[2], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R1.xyz, fragment.texcoord[1].z, R0, R1;
MOV R4.x, c[20];
ADD R0.x, -R4, c[21];
RCP R0.z, R0.x;
MOV R0.y, c[18].x;
ADD R0.x, fragment.texcoord[2].y, -c[20];
MUL_SAT R0.x, R0, R0.z;
MUL R4.x, -R0, c[27].w;
ADD R0.y, -R0, c[19].x;
RCP R0.z, R0.y;
ADD R0.y, fragment.texcoord[2], -c[18].x;
MUL_SAT R0.y, R0, R0.z;
ADD R0.z, R4.x, c[28].x;
MUL R0.x, R0, R0;
MUL R0.z, R0.x, R0;
MUL R4.x, -R0.y, c[27].w;
MUL R0.x, R0.y, R0.y;
ADD R0.y, R4.x, c[28].x;
MAD R4.x, R0, R0.y, -R0.z;
TEX R0.xyz, R3.zxzw, texture[2], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R0, R1;
MUL R1.x, R1.w, R4;
MAD R2.xyz, R0, R1.x, R2;
TEX R1.xyz, R3.zyzw, texture[3], 2D;
TEX R0.xyz, R3, texture[3], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
MOV R3.y, c[22].x;
ADD R3.y, -R3, c[23].x;
RCP R1.y, R3.y;
ADD R1.x, fragment.texcoord[2].y, -c[22];
MUL_SAT R3.y, R1.x, R1;
TEX R1.xyz, R3.zxzw, texture[3], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R0;
MUL R3.x, -R3.y, c[27].w;
ADD R0.y, R3.x, c[28].x;
MUL R0.x, R3.y, R3.y;
MUL R4.y, R0.x, R0;
MUL R3.xyz, fragment.texcoord[0], c[8].x;
MUL R4.z, R1.w, R4.y;
TEX R0.xyz, R3.zyzw, texture[4], 2D;
MAD R2.xyz, R1, R4.z, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[4], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R1, R0;
MUL R1.xyz, fragment.texcoord[0], c[9].x;
MUL R3.w, R0, R3;
MAD R0.xyz, R0, R3.w, R2;
TEX R3.xyz, R1.zyzw, texture[5], 2D;
TEX R2.xyz, R1, texture[5], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R3;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[5], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[10].x;
MUL R2.w, R0, R2;
MAD R0.xyz, R3, R2.w, R0;
TEX R2.xyz, R1.zyzw, texture[6], 2D;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[6], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[6], 2D;
MAD R3.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[0], c[11].x;
MUL R2.w, R0, R4.x;
TEX R2.xyz, R1.zyzw, texture[7], 2D;
MAD R0.xyz, R3, R2.w, R0;
MUL R3.xyz, fragment.texcoord[1].x, R2;
TEX R2.xyz, R1, texture[7], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R2, R3;
TEX R1.xyz, R1.zxzw, texture[7], 2D;
MAD R1.xyz, fragment.texcoord[1].y, R1, R2;
MUL R2.x, R0.w, R4.y;
MAD R0.xyz, R1, R2.x, R0;
MAD R1.xyz, fragment.texcoord[3], R0, -fragment.texcoord[3];
ADD R0.x, R1.w, R0.w;
MOV R0.w, c[5].x;
MAD R2.xyz, R0.x, R1, fragment.texcoord[3];
MUL R3.xyz, fragment.texcoord[0], c[4].x;
TEX R1.xyz, R3.zyzw, texture[8], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R1;
TEX R0.xyz, R3, texture[8], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
ADD R0.w, -R0, c[6].x;
RCP R1.x, R0.w;
ADD R0.w, fragment.texcoord[2].z, -c[5].x;
MUL_SAT R0.w, R0, R1.x;
TEX R1.xyz, R3.zxzw, texture[8], 2D;
MUL R1.w, -R0, c[27];
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.x, R1.w, c[28];
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1.x, c[27].y;
ADD R1.xyz, R0, -R2;
MUL R0.x, fragment.texcoord[2], R0.w;
MAD R1.xyz, R0.x, R1, R2;
MOV R0.y, c[27].z;
MOV R0.x, fragment.texcoord[2].w;
TEX R0.xyz, R0, texture[9], 2D;
ADD R2.xyz, R0, -R1;
TEX R0, fragment.texcoord[5], texture[11], 2D;
MUL R3.xyz, R0.w, R0;
MUL R1.w, fragment.texcoord[2].z, c[25].x;
MUL R0.w, R1, c[24].x;
TXP R0.xyz, fragment.texcoord[4], texture[10], 2D;
POW R0.w, c[27].x, R0.w;
ADD R0.w, -R0, c[27].y;
MAD R0.xyz, R3, c[28].y, R0;
MAD R1.xyz, R0.w, R2, R1;
MUL result.color.xyz, R1, R0;
MOV result.color.w, c[26].x;
END
# 172 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Float 0 [_texTiling]
Float 1 [_texPower]
Float 2 [_groundTexStart]
Float 3 [_groundTexEnd]
Float 4 [_steepTiling]
Float 5 [_steepTexStart]
Float 6 [_steepTexEnd]
Float 7 [_multiPower]
Float 8 [_deepMultiFactor]
Float 9 [_mainMultiFactor]
Float 10 [_highMultiFactor]
Float 11 [_snowMultiFactor]
Float 12 [_deepStart]
Float 13 [_deepEnd]
Float 14 [_mainLoStart]
Float 15 [_mainLoEnd]
Float 16 [_mainHiStart]
Float 17 [_mainHiEnd]
Float 18 [_hiLoStart]
Float 19 [_hiLoEnd]
Float 20 [_hiHiStart]
Float 21 [_hiHiEnd]
Float 22 [_snowStart]
Float 23 [_snowEnd]
Float 24 [_heightDensityAtViewer]
Float 25 [_globalDensity]
Float 26 [_PlanetOpacity]
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 1
SetTexture 2 [_highTex] 2D 2
SetTexture 3 [_snowTex] 2D 3
SetTexture 4 [_deepMultiTex] 2D 4
SetTexture 5 [_mainMultiTex] 2D 5
SetTexture 6 [_highMultiTex] 2D 6
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
dcl_2d s6
dcl_2d s7
dcl_2d s8
dcl_2d s9
dcl_2d s10
dcl_2d s11
def c27, 2.71828198, 1.00000000, 0.50000000, 8.00000000
def c28, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
mov r0.x, c3
add r0.w, -c2.x, r0.x
mul r0.xyz, v0, c0.x
rcp r1.x, r0.w
texld r3.xyz, r0.zyzw, s0
add r0.w, v2.z, -c2.x
mul_sat r0.w, r0, r1.x
texld r1.xyz, r0.zyzw, s1
mad r1.w, -r0, c28.x, c28.y
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r2.xyz, v1.z, r1, r2
mul r0.w, r0, r0
mul r0.w, r0, r1
add r1.x, -r0.w, c27.y
mov r1.z, c17.x
add r1.z, -c16.x, r1
rcp r1.w, r1.z
add r1.z, v2.y, -c16.x
mul_sat r1.w, r1.z, r1
mov r1.y, c15.x
add r1.y, -c14.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c14.x
mul_sat r1.y, r1, r1.z
mul r0.w, r0, c7.x
mad r2.w, -r1, c28.x, c28.y
mul r1.z, r1.w, r1.w
mul r1.w, r1.z, r2
mul r1.z, r1.y, r1.y
mad r1.y, -r1, c28.x, c28
mad r3.w, r1.z, r1.y, -r1
mul r1.w, r1.x, c1.x
mul r2.w, r1, r3
texld r1.xyz, r0.zxzw, s1
mad r1.xyz, v1.y, r1, r2
mul r2.xyz, r1, r2.w
mov r2.w, c13.x
add r2.w, -c12.x, r2
rcp r4.x, r2.w
add r2.w, v2.y, -c12.x
texld r1.xyz, r0, s0
mul r3.xyz, v1.x, r3
mul_sat r2.w, r2, r4.x
mad r3.xyz, v1.z, r1, r3
mad r1.y, -r2.w, c28.x, c28
mul r1.x, r2.w, r2.w
mad r4.x, -r1, r1.y, c27.y
mul r2.w, r1, r4.x
texld r1.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r1, r3
mad r3.xyz, r3, r2.w, r2
texld r1.xyz, r0.zyzw, s2
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s2
mad r2.xyz, v1.z, r1, r2
mov r1.y, c21.x
add r1.y, -c20.x, r1
rcp r1.z, r1.y
add r1.y, v2, -c20.x
mul_sat r1.z, r1.y, r1
mov r1.x, c19
add r1.x, -c18, r1
rcp r1.y, r1.x
add r1.x, v2.y, -c18
mul_sat r1.x, r1, r1.y
mad r2.w, -r1.z, c28.x, c28.y
mul r1.y, r1.z, r1.z
mul r1.z, r1.y, r2.w
mul r1.y, r1.x, r1.x
mad r1.x, -r1, c28, c28.y
mad r2.w, r1.y, r1.x, -r1.z
texld r1.xyz, r0.zxzw, s2
mad r1.xyz, v1.y, r1, r2
mul r2.x, r1.w, r2.w
mad r2.xyz, r1, r2.x, r3
texld r1.xyz, r0.zyzw, s3
texld r3.xyz, r0, s3
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r3, r1
mov r0.y, c23.x
add r3.x, -c22, r0.y
rcp r3.y, r3.x
add r3.x, v2.y, -c22
texld r0.xyz, r0.zxzw, s3
mul_sat r4.y, r3.x, r3
mad r3.xyz, v1.y, r0, r1
mad r0.y, -r4, c28.x, c28
mul r0.x, r4.y, r4.y
mul r4.y, r0.x, r0
mul r0.xyz, v0, c8.x
mul r4.z, r1.w, r4.y
mad r3.xyz, r3, r4.z, r2
texld r1.xyz, r0.zyzw, s4
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s4
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s4
mad r1.xyz, v1.y, r0, r1
mul r0.xyz, v0, c9.x
mul r4.x, r0.w, r4
texld r2.xyz, r0.zyzw, s5
mad r1.xyz, r1, r4.x, r3
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s5
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s5
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c10.x
mul r3.w, r0, r3
texld r2.xyz, r0.zyzw, s6
mad r1.xyz, r3, r3.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s6
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s6
mad r3.xyz, v1.y, r0, r2
mul r0.xyz, v0, c11.x
mul r2.w, r0, r2
texld r2.xyz, r0.zyzw, s7
mad r1.xyz, r3, r2.w, r1
mul r3.xyz, v1.x, r2
texld r2.xyz, r0, s7
mad r2.xyz, v1.z, r2, r3
texld r0.xyz, r0.zxzw, s7
mad r0.xyz, v1.y, r0, r2
mul r2.x, r0.w, r4.y
mad r0.xyz, r0, r2.x, r1
mad r2.xyz, v3, r0, -v3
mul r0.xyz, v0, c4.x
add r0.w, r1, r0
mad r3.xyz, r0.w, r2, v3
texld r1.xyz, r0.zyzw, s8
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s8
mov r0.w, c6.x
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s8
mad r0.xyz, r0, v1.y, r1
add r0.w, -c5.x, r0
rcp r1.x, r0.w
add r0.w, v2.z, -c5.x
mul_sat r0.w, r0, r1.x
add r1.xyz, r0, -r3
mul r0.x, v2.z, c25
mul r1.w, r0.x, c24.x
mul r0.y, r0.w, r0.w
mad r0.z, -r0.w, c28.x, c28.y
mad r2.x, -r0.y, r0.z, c27.y
pow r0, c27.x, r1.w
mul r0.y, v2.x, r2.x
mad r2.xyz, r0.y, r1, r3
mov r1.w, r0.x
texld r0, v5, s11
mul_pp r0.xyz, r0.w, r0
mov r1.y, c27.z
mov r1.x, v2.w
texld r1.xyz, r1, s9
add r3.xyz, r1, -r2
texldp r1.xyz, v4, s10
mad_pp r0.xyz, r0, c27.w, r1
add r0.w, -r1, c27.y
mad r1.xyz, r0.w, r3, r2
mul_pp oC0.xyz, r1, r0
mov_pp oC0.w, c26.x
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
SetTexture 0 [_deepTex] 2D 0
SetTexture 1 [_mainTex] 2D 2
SetTexture 2 [_highTex] 2D 4
SetTexture 3 [_snowTex] 2D 6
SetTexture 4 [_deepMultiTex] 2D 1
SetTexture 5 [_mainMultiTex] 2D 3
SetTexture 6 [_highMultiTex] 2D 5
SetTexture 7 [_snowMultiTex] 2D 7
SetTexture 8 [_steepTex] 2D 8
SetTexture 9 [_fogColorRamp] 2D 9
SetTexture 10 [_LightBuffer] 2D 10
SetTexture 11 [unity_Lightmap] 2D 11
ConstBuffer "$Globals" 288
Float 64 [_texTiling]
Float 68 [_texPower]
Float 72 [_groundTexStart]
Float 76 [_groundTexEnd]
Float 84 [_steepTiling]
Float 88 [_steepTexStart]
Float 92 [_steepTexEnd]
Float 104 [_multiPower]
Float 108 [_deepMultiFactor]
Float 112 [_mainMultiFactor]
Float 116 [_highMultiFactor]
Float 120 [_snowMultiFactor]
Float 124 [_deepStart]
Float 128 [_deepEnd]
Float 132 [_mainLoStart]
Float 136 [_mainLoEnd]
Float 140 [_mainHiStart]
Float 144 [_mainHiEnd]
Float 148 [_hiLoStart]
Float 152 [_hiLoEnd]
Float 156 [_hiHiStart]
Float 160 [_hiHiEnd]
Float 164 [_snowStart]
Float 168 [_snowEnd]
Float 196 [_heightDensityAtViewer]
Float 208 [_globalDensity]
Float 212 [_PlanetOpacity]
BindCB  "$Globals" 0
"ps_4_0
eefiecedlbilobgbmiedglapmicebgnimabnjcboabaaaaaagibhaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahahaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaagaaaaaa
adadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcdabgaaaaeaaaaaaaimafaaaa
fjaaaaaeegiocaaaaaaaaaaaaoaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaa
fkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaafkaaaaadaagabaaa
agaaaaaafkaaaaadaagabaaaahaaaaaafkaaaaadaagabaaaaiaaaaaafkaaaaad
aagabaaaajaaaaaafkaaaaadaagabaaaakaaaaaafkaaaaadaagabaaaalaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
fibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaa
fibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaa
fibiaaaeaahabaaaagaaaaaaffffaaaafibiaaaeaahabaaaahaaaaaaffffaaaa
fibiaaaeaahabaaaaiaaaaaaffffaaaafibiaaaeaahabaaaajaaaaaaffffaaaa
fibiaaaeaahabaaaakaaaaaaffffaaaafibiaaaeaahabaaaalaaaaaaffffaaaa
gcbaaaadhcbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaa
adaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaadlcbabaaaafaaaaaagcbaaaad
dcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacagaaaaaaaaaaaaak
bcaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaaahaaaaaaakiacaaaaaaaaaaa
aiaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaabkbabaaaadaaaaaa
dkiacaiaebaaaaaaaaaaaaaaahaaaaaadicaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaajccaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaabkaabaiaebaaaaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaakccaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaaeaaaaaadkiacaaaaaaaaaaaaeaaaaaaaoaaaaak
ccaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaa
aaaaaaaaaaaaaaajecaabaaaaaaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaaeaaaaaadicaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaackaabaaa
aaaaaaaadcaaaaajecaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackaabaiaebaaaaaaaaaaaaaabkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaa
ckaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaackiacaaa
aaaaaaaaagaaaaaadiaaaaaiecaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaa
aaaaaaaaaeaaaaaadcaaaaakicaabaaaaaaaaaaabkiacaaaaaaaaaaaaeaaaaaa
dkaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaabaaaaaackaabaaa
aaaaaaaaakaabaaaaaaaaaaadiaaaaahbcaabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaakccaabaaaabaaaaaadkiacaiaebaaaaaaaaaaaaaa
aiaaaaaaakiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaabaaaaaaaaaaaaajmcaabaaa
abaaaaaafgbfbaaaadaaaaaafgincaiaebaaaaaaaaaaaaaaaiaaaaaadicaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaadcaaaaajicaabaaa
abaaaaaabkaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
ccaabaaaabaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaa
abaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaaaaaaaaakicaabaaaabaaaaaa
bkiacaiaebaaaaaaaaaaaaaaaiaaaaaackiacaaaaaaaaaaaaiaaaaaaaoaaaaak
icaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
abaaaaaadicaaaahecaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajicaabaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahecaabaaaabaaaaaackaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaakccaabaaaabaaaaaadkaabaaaabaaaaaackaabaaaabaaaaaabkaabaia
ebaaaaaaabaaaaaadiaaaaahecaabaaaabaaaaaackaabaaaaaaaaaaabkaabaaa
abaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaaaaaaaaabkaabaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaaeaaaaaa
efaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaa
abaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
fgbfbaaaacaaaaaaegacbaaaadaaaaaadiaaaaahhcaabaaaadaaaaaakgakbaaa
abaaaaaaegacbaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaggakbaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaafaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaaeaaaaaaegacbaaa
afaaaaaakgbkbaaaacaaaaaaegacbaaaaeaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaa
aeaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaaeaaaaaadcaaaaaj
ncaabaaaabaaaaaaagajbaaaaeaaaaaaagaabaaaabaaaaaaagajbaaaadaaaaaa
aaaaaaakicaabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaaajaaaaaaakiacaaa
aaaaaaaaakaaaaaaaoaaaaakicaabaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaacaaaaaaaaaaaaajdcaabaaaadaaaaaafgbfbaaa
adaaaaaangifcaiaebaaaaaaaaaaaaaaajaaaaaadicaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaabkaabaaaadaaaaaadcaaaaajccaabaaaadaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaahicaabaaaacaaaaaa
dkaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaabkaabaaaadaaaaaaaaaaaaakccaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaajaaaaaackiacaaaaaaaaaaaajaaaaaaaoaaaaakccaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpbkaabaaaadaaaaaadicaaaah
bcaabaaaadaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaajccaabaaa
adaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadcaaaaakicaabaaa
acaaaaaabkaabaaaadaaaaaaakaabaaaadaaaaaadkaabaiaebaaaaaaacaaaaaa
diaaaaahbcaabaaaadaaaaaackaabaaaaaaaaaaadkaabaaaacaaaaaadiaaaaah
icaabaaaacaaaaaabkaabaaaaaaaaaaadkaabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaggakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadiaaaaah
ocaabaaaadaaaaaaagajbaaaaeaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
aeaaaaaaegaabaaaacaaaaaaeghobaaaacaaaaaaaagabaaaaeaaaaaadcaaaaaj
ocaabaaaadaaaaaaagajbaaaaeaaaaaakgbkbaaaacaaaaaafgaobaaaadaaaaaa
efaaaaajpcaabaaaaeaaaaaacgakbaaaacaaaaaaeghobaaaacaaaaaaaagabaaa
aeaaaaaadcaaaaajocaabaaaadaaaaaaagajbaaaaeaaaaaafgbfbaaaacaaaaaa
fgaobaaaadaaaaaadcaaaaajncaabaaaabaaaaaafgaobaaaadaaaaaaagaabaaa
adaaaaaaagaobaaaabaaaaaaaaaaaaakbcaabaaaadaaaaaabkiacaiaebaaaaaa
aaaaaaaaakaaaaaackiacaaaaaaaaaaaakaaaaaaaoaaaaakbcaabaaaadaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaaadaaaaaaaaaaaaaj
ccaabaaaadaaaaaabkbabaaaadaaaaaabkiacaiaebaaaaaaaaaaaaaaakaaaaaa
dicaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadcaaaaaj
ccaabaaaadaaaaaaakaabaaaadaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahbcaabaaaadaaaaaaakaabaaaadaaaaaaakaabaaaadaaaaaadiaaaaah
bcaabaaaadaaaaaaakaabaaaadaaaaaabkaabaaaadaaaaaadiaaaaahgcaabaaa
aaaaaaaafgagbaaaaaaaaaaaagaabaaaadaaaaaaefaaaaajpcaabaaaadaaaaaa
ggakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaa
egaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaaefaaaaajpcaabaaa
afaaaaaacgakbaaaacaaaaaaeghobaaaadaaaaaaaagabaaaagaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaa
acaaaaaadcaaaaajncaabaaaabaaaaaaagajbaaaacaaaaaakgakbaaaaaaaaaaa
agaobaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaapgipcaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaa
aeaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaa
eghobaaaaeaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
aeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajncaabaaa
abaaaaaaagajbaaaacaaaaaaagaabaaaaaaaaaaaagaobaaaabaaaaaadiaaaaai
hcaabaaaacaaaaaaegbcbaaaabaaaaaaagiacaaaaaaaaaaaahaaaaaaefaaaaaj
pcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
diaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaafaaaaaaaagabaaaadaaaaaa
efaaaaajpcaabaaaafaaaaaacgakbaaaacaaaaaaeghobaaaafaaaaaaaagabaaa
adaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaa
egacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaa
fgafbaaaabaaaaaaigadbaaaabaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaa
abaaaaaafgifcaaaaaaaaaaaahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadiaaaaahhcaabaaaadaaaaaa
egacbaaaadaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaa
acaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaaefaaaaajpcaabaaaafaaaaaa
cgakbaaaacaaaaaaeghobaaaagaaaaaaaagabaaaafaaaaaadcaaaaajhcaabaaa
acaaaaaaegacbaaaaeaaaaaakgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaaj
hcaabaaaacaaaaaaegacbaaaafaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaaegbcbaaaabaaaaaakgikcaaaaaaaaaaa
ahaaaaaaefaaaaajpcaabaaaadaaaaaaggakbaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaadiaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaaeaaaaaaegaabaaaacaaaaaaeghobaaaahaaaaaa
aagabaaaahaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaacaaaaaaeghobaaa
ahaaaaaaaagabaaaahaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaaeaaaaaa
kgbkbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
acaaaaaafgbfbaaaacaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaacaaaaaafgafbaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaeaaaaaaegacbaaaaaaaaaaaegbcbaiaebaaaaaaaeaaaaaa
dcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaa
aeaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaaabaaaaaafgifcaaaaaaaaaaa
afaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaabaaaaaaeghobaaaaiaaaaaa
aagabaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaacgakbaaaabaaaaaaeghobaaa
aiaaaaaaaagabaaaaiaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaa
abaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaakicaabaaaaaaaaaaa
ckiacaiaebaaaaaaaaaaaaaaafaaaaaadkiacaaaaaaaaaaaafaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajicaabaaaabaaaaaackbabaaaadaaaaaackiacaiaebaaaaaa
aaaaaaaaafaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
abaaaaaadcaaaaajicaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaa
akbabaaaadaaaaaadcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaadaaaaaa
dgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaa
egaabaaaabaaaaaaeghobaaaajaaaaaaaagabaaaajaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaackbabaaaadaaaaaaakiacaaaaaaaaaaaanaaaaaadiaaaaaiicaabaaa
aaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaadlkklidpbjaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaaafaaaaaa
pgbpbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
akaaaaaaaagabaaaakaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaagaaaaaa
eghobaaaalaaaaaaaagabaaaalaaaaaadiaaaaahicaabaaaaaaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaaaebdcaaaaajhcaabaaaabaaaaaapgapbaaaaaaaaaaa
egacbaaaacaaaaaaegacbaaaabaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaabaaaaaadgaaaaagiccabaaaaaaaaaaabkiacaaaaaaaaaaa
anaaaaaadoaaaaab"
}
}
 }
}
Fallback "Diffuse"
}