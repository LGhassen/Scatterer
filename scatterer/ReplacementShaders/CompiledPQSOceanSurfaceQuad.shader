Shader "EVE/Terrain/PQS/Ocean Surface Quad" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _ColorFromSpace ("Color From Space", Color) = (1,1,1,1)
 _SpecColor ("Specular Color", Color) = (1,1,1,1)
 _Shininess ("Shininess", Range(0.01,1)) = 0.078125
 _Gloss ("Gloss", Range(0.01,1)) = 0.078125
 _tiling ("Tex Tiling", Float) = 1
 _WaterTex ("Tex0", 2D) = "white" {}
 _WaterTex1 ("Tex1", 2D) = "white" {}
 _bTiling ("Normal Tiling", Float) = 1
 _BumpMap ("Normalmap0", 2D) = "bump" {}
 _displacement ("Water Movement", Float) = 1
 _dispFreq ("Water Freq", Float) = 1
 _Mix ("Mix", Float) = 1
 _oceanOpacity ("Opacity", Float) = 1
 _falloffPower ("Falloff Power", Float) = 1
 _falloffExp ("Falloff Exp", Float) = 2
 _fogColor ("AP Fog Color", Color) = (0,0,1,1)
 _heightFallOff ("AP Height Fall Off", Float) = 1
 _globalDensity ("AP Global Density", Float) = 1
 _atmosphereDepth ("AP Atmosphere Depth", Float) = 1
 _fogColorRamp ("FogColorRamp", 2D) = "white" {}
 _fadeStart ("FadeStart", Float) = 1
 _fadeEnd ("FadeEnd", Float) = 1
 _PlanetOpacity ("PlanetOpacity", Float) = 1
}
SubShader { 
ZWrite On
Tags { "RenderType"="Opaque"}
 LOD 400
 GrabPass {
 }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardBase" "SHADOWSUPPORT"="true"}
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_WorldSpaceLightPos0]
Vector 19 [unity_SHAr]
Vector 20 [unity_SHAg]
Vector 21 [unity_SHAb]
Vector 22 [unity_SHBr]
Vector 23 [unity_SHBg]
Vector 24 [unity_SHBb]
Vector 25 [unity_SHC]
Vector 26 [unity_Scale]
Vector 27 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[28] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..27] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MUL R1.xyz, vertex.normal, c[26].w;
DP3 R2.w, R1, c[10];
DP3 R0.x, R1, c[9];
MOV R0.y, R2.w;
DP3 R0.z, R1, c[11];
MOV R0.w, c[0].x;
MUL R1, R0.xyzz, R0.yzzx;
DP4 R2.z, R0, c[21];
DP4 R2.y, R0, c[20];
DP4 R2.x, R0, c[19];
MUL R0.w, R2, R2;
MAD R0.w, R0.x, R0.x, -R0;
DP4 R0.z, R1, c[24];
DP4 R0.y, R1, c[23];
DP4 R0.x, R1, c[22];
ADD R0.xyz, R2, R0;
MUL R1.xyz, R0.w, c[25];
ADD result.texcoord[5].xyz, R0, R1;
MOV R2.xyz, c[17];
MOV R2.w, c[0].x;
DP4 R0.z, R2, c[15];
DP4 R0.y, R2, c[14];
DP4 R0.x, R2, c[13];
MAD R0.xyz, R0, c[26].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R2.xyz, R1, vertex.attrib[14].w;
RSQ R0.w, R0.w;
MUL R3.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R3;
MOV R1, c[18];
DP4 R3.z, R1, c[15];
DP4 R3.x, R1, c[13];
DP4 R3.y, R1, c[14];
DP3 result.texcoord[0].y, R0, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
MOV R0.z, vertex.texcoord[1].x;
MOV R0.xy, vertex.texcoord[0];
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
DP3 result.texcoord[4].y, R2, R3;
DP3 result.texcoord[4].z, vertex.normal, R3;
DP3 result.texcoord[4].x, vertex.attrib[14], R3;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[27];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 57 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_WorldSpaceLightPos0]
Vector 18 [unity_SHAr]
Vector 19 [unity_SHAg]
Vector 20 [unity_SHAb]
Vector 21 [unity_SHBr]
Vector 22 [unity_SHBg]
Vector 23 [unity_SHBb]
Vector 24 [unity_SHC]
Vector 25 [unity_Scale]
Vector 26 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c27, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mul r1.xyz, v2, c25.w
dp3 r2.w, r1, c9
dp3 r0.x, r1, c8
mov r0.y, r2.w
dp3 r0.z, r1, c10
mov r0.w, c27.x
mul r1, r0.xyzz, r0.yzzx
dp4 r2.z, r0, c20
dp4 r2.y, r0, c19
dp4 r2.x, r0, c18
mul r0.w, r2, r2
mad r0.w, r0.x, r0.x, -r0
dp4 r0.z, r1, c23
dp4 r0.y, r1, c22
dp4 r0.x, r1, c21
mul r1.xyz, r0.w, c24
add r0.xyz, r2, r0
add o6.xyz, r0, r1
mov r0.w, c27.x
mov r0.xyz, c16
dp4 r1.z, r0, c14
dp4 r1.y, r0, c13
dp4 r1.x, r0, c12
mad r2.xyz, r1, c25.w, -v0
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r0, v1.w
dp3 r0.w, r2, r2
rsq r0.x, r0.w
mul r1.xyz, r0.x, r2
dp3 r2.w, v2, r1
mov r0, c14
dp4 r4.z, c17, r0
mov r0, c13
dp4 r4.y, c17, r0
mov r1, c12
dp4 r4.x, c17, r1
mov r0.z, v4.x
mov r0.xy, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
dp3 o1.y, r2, r3
dp3 o5.y, r3, r4
add o2.w, -r2, c27.x
dp3 o1.z, v2, r2
dp3 o1.x, r2, v1
dp3 o5.z, v2, r4
dp3 o5.x, v1, r4
abs o2.xyz, r0
mov o3, r0
dp3 o4.x, r0, c26
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 304
Vector 272 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
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
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecednooobahekjfpiedhfgonkklbfooogbddabaaaaaameajaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcpiahaaaaeaaaabaapoabaaaafjaaaaaeegiocaaaaaaaaaaa
bcaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
cnaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadhccabaaaabaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaa
acaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
hccabaaaafaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadgaaaaafdcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaaf
ecaabaaaaaaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaai
iccabaaaabaaaaaaegiccaaaaaaaaaaabbaaaaaaegacbaaaaaaaaaaadiaaaaah
hcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaa
abaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaabaaaaaa
diaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgbpbaaaabaaaaaadiaaaaaj
hcaabaaaacaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaa
dcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaa
aeaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaa
bcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
acaaaaaaegacbaaaacaaaaaaegiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaa
acaaaaaaegacbaaaacaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaa
aaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaa
baaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaaegacbaaaacaaaaaabaaaaaah
eccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaaacaaaaaabaaaaaahicaabaaa
aaaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaaaaaaaaaegacbaaa
acaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaacaaaaaaegbcbaaaacaaaaaa
aaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaadgaaaaafhccabaaa
adaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaabkbabaaaaaaaaaaa
ckiacaaaadaaaaaaafaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaadaaaaaa
aeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
ckiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaak
bcaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaa
aaaaaaaadgaaaaagiccabaaaadaaaaaaakaabaiaebaaaaaaaaaaaaaadiaaaaaj
hcaabaaaaaaaaaaafgifcaaaacaaaaaaaaaaaaaaegiccaaaadaaaaaabbaaaaaa
dcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaacaaaaaa
aaaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaa
bcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaa
aaaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaaacaaaaaaaaaaaaaaegacbaaa
aaaaaaaabaaaaaahcccabaaaaeaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
baaaaaahbccabaaaaeaaaaaaegbcbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaah
eccabaaaaeaaaaaaegbcbaaaacaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaa
aaaaaaaaegbcbaaaacaaaaaapgipcaaaadaaaaaabeaaaaaadiaaaaaihcaabaaa
abaaaaaafgafbaaaaaaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaaklcaabaaa
aaaaaaaaegiicaaaadaaaaaaamaaaaaaagaabaaaaaaaaaaaegaibaaaabaaaaaa
dcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaaaoaaaaaakgakbaaaaaaaaaaa
egadbaaaaaaaaaaadgaaaaaficaabaaaaaaaaaaaabeaaaaaaaaaiadpbbaaaaai
bcaabaaaabaaaaaaegiocaaaacaaaaaacgaaaaaaegaobaaaaaaaaaaabbaaaaai
ccaabaaaabaaaaaaegiocaaaacaaaaaachaaaaaaegaobaaaaaaaaaaabbaaaaai
ecaabaaaabaaaaaaegiocaaaacaaaaaaciaaaaaaegaobaaaaaaaaaaadiaaaaah
pcaabaaaacaaaaaajgacbaaaaaaaaaaaegakbaaaaaaaaaaabbaaaaaibcaabaaa
adaaaaaaegiocaaaacaaaaaacjaaaaaaegaobaaaacaaaaaabbaaaaaiccaabaaa
adaaaaaaegiocaaaacaaaaaackaaaaaaegaobaaaacaaaaaabbaaaaaiecaabaaa
adaaaaaaegiocaaaacaaaaaaclaaaaaaegaobaaaacaaaaaaaaaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaadiaaaaahccaabaaaaaaaaaaa
bkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaadcaaaaakhccabaaa
afaaaaaaegiccaaaacaaaaaacmaaaaaaagaabaaaaaaaaaaaegacbaaaabaaaaaa
doaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 19 [unity_Scale]
Vector 20 [_sunLightDirection]
Vector 21 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[22] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..21] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.xyz, c[17];
MOV R1.w, c[0].x;
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[19].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[20];
MAD result.texcoord[4].xy, vertex.texcoord[1], c[21], c[21].zwzw;
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 33 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [unity_Scale]
Vector 18 [_sunLightDirection]
Vector 19 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
def c20, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.xyz, c16
mov r1.w, c20.x
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c17.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
mov r0.z, v4.x
mov r0.xy, v3
add o2.w, -r0, c20.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r1.x, v0, c2
mov r0.w, -r1.x
abs o2.xyz, r0
mov o3, r0
dp3 o4.x, r0, c18
mad o5.xy, v4, c19, c19.zwzw
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 320
Vector 272 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedljeonobbnjkofhiplboblekmgmmendcjabaaaaaaliagaaaaadaaaaaa
cmaaaaaapeaaaaaakmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheolaaaaaaaagaaaaaa
aiaaaaaajiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaakeaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaakeaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklfdeieefcaeafaaaaeaaaabaaebabaaaafjaaaaae
egiocaaaaaaaaaaabeaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaae
egiocaaaacaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaa
abaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaaddccabaaaaeaaaaaagiaaaaacadaaaaaadiaaaaai
pcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaaacaaaaaa
adaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaaaaaaaaa
egbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaabaaaaaaiiccabaaaabaaaaaaegiccaaaaaaaaaaabbaaaaaa
egacbaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaa
acaaaaaadcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaa
egacbaiaebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgbpbaaaabaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaabaaaaaaaeaaaaaa
egiccaaaacaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaacaaaaaa
baaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaa
acaaaaaaegiccaaaacaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaa
acaaaaaaaaaaaaaihcaabaaaacaaaaaaegacbaaaacaaaaaaegiccaaaacaaaaaa
bdaaaaaadcaaaaalhcaabaaaacaaaaaaegacbaaaacaaaaaapgipcaaaacaaaaaa
beaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaabaaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaa
egacbaaaacaaaaaabaaaaaaheccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaa
acaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaaaaaaaaaegacbaaaacaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
abaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaa
aaaaaaaadgaaaaafhccabaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaagiccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadcaaaaaldccabaaaaeaaaaaaegbabaaaaeaaaaaaegiacaaa
aaaaaaaabdaaaaaaogikcaaaaaaaaaaabdaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 19 [unity_Scale]
Vector 20 [_sunLightDirection]
Vector 21 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[22] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..21] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.xyz, c[17];
MOV R1.w, c[0].x;
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[19].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[20];
MAD result.texcoord[4].xy, vertex.texcoord[1], c[21], c[21].zwzw;
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 33 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [unity_Scale]
Vector 18 [_sunLightDirection]
Vector 19 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
def c20, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.xyz, c16
mov r1.w, c20.x
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c17.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
mov r0.z, v4.x
mov r0.xy, v3
add o2.w, -r0, c20.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r1.x, v0, c2
mov r0.w, -r1.x
abs o2.xyz, r0
mov o3, r0
dp3 o4.x, r0, c18
mad o5.xy, v4, c19, c19.zwzw
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 320
Vector 272 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedljeonobbnjkofhiplboblekmgmmendcjabaaaaaaliagaaaaadaaaaaa
cmaaaaaapeaaaaaakmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheolaaaaaaaagaaaaaa
aiaaaaaajiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaakeaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaakeaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklfdeieefcaeafaaaaeaaaabaaebabaaaafjaaaaae
egiocaaaaaaaaaaabeaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaae
egiocaaaacaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaa
abaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaaddccabaaaaeaaaaaagiaaaaacadaaaaaadiaaaaai
pcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaakgbkbaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaaacaaaaaa
adaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaaaaaaaaa
egbabaaaadaaaaaadgaaaaafecaabaaaaaaaaaaaakbabaaaaeaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaabaaaaaaiiccabaaaabaaaaaaegiccaaaaaaaaaaabbaaaaaa
egacbaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaa
acaaaaaadcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaa
egacbaiaebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgbpbaaaabaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaabaaaaaaaeaaaaaa
egiccaaaacaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaacaaaaaa
baaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaa
acaaaaaaegiccaaaacaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaa
acaaaaaaaaaaaaaihcaabaaaacaaaaaaegacbaaaacaaaaaaegiccaaaacaaaaaa
bdaaaaaadcaaaaalhcaabaaaacaaaaaaegacbaaaacaaaaaapgipcaaaacaaaaaa
beaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaabaaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaa
egacbaaaacaaaaaabaaaaaaheccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaa
acaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaaaaaaaaaegacbaaaacaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
abaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaa
aaaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaa
aaaaaaaadgaaaaafhccabaaaadaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaacaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaa
akaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaahaaaaaa
dkbabaaaaaaaaaaaakaabaaaaaaaaaaadgaaaaagiccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadcaaaaaldccabaaaaeaaaaaaegbabaaaaeaaaaaaegiacaaa
aaaaaaaabdaaaaaaogikcaaaaaaaaaaabdaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 19 [_WorldSpaceLightPos0]
Vector 20 [unity_SHAr]
Vector 21 [unity_SHAg]
Vector 22 [unity_SHAb]
Vector 23 [unity_SHBr]
Vector 24 [unity_SHBg]
Vector 25 [unity_SHBb]
Vector 26 [unity_SHC]
Vector 27 [unity_Scale]
Vector 28 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[29] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..28] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MUL R1.xyz, vertex.normal, c[27].w;
DP3 R2.w, R1, c[10];
DP3 R0.x, R1, c[9];
MOV R0.y, R2.w;
DP3 R0.z, R1, c[11];
MOV R0.w, c[0].x;
MUL R1, R0.xyzz, R0.yzzx;
DP4 R2.z, R0, c[22];
DP4 R2.y, R0, c[21];
DP4 R2.x, R0, c[20];
MUL R0.w, R2, R2;
MAD R0.w, R0.x, R0.x, -R0;
DP4 R0.z, R1, c[25];
DP4 R0.y, R1, c[24];
DP4 R0.x, R1, c[23];
ADD R0.xyz, R2, R0;
MUL R1.xyz, R0.w, c[26];
ADD result.texcoord[5].xyz, R0, R1;
MOV R2.xyz, c[17];
MOV R2.w, c[0].x;
DP4 R0.z, R2, c[15];
DP4 R0.y, R2, c[14];
DP4 R0.x, R2, c[13];
MAD R0.xyz, R0, c[27].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R2.xyz, R1, vertex.attrib[14].w;
RSQ R0.w, R0.w;
MUL R3.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R3;
MOV R1, c[19];
ADD result.texcoord[1].w, -R0, c[0].x;
DP4 R3.z, R1, c[15];
DP4 R3.x, R1, c[13];
DP4 R3.y, R1, c[14];
DP3 result.texcoord[0].y, R0, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R0.w, vertex.position, c[8];
DP4 R0.z, vertex.position, c[7];
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R1.xyz, R0.xyww, c[0].y;
MUL R1.y, R1, c[18].x;
ADD result.texcoord[6].xy, R1, R1.z;
MOV result.position, R0;
MOV R1.xy, vertex.texcoord[0];
MOV R1.z, vertex.texcoord[1].x;
DP3 R1.w, R1, R1;
RSQ R0.x, R1.w;
MUL R1.xyz, R0.x, R1;
DP4 R0.y, vertex.position, c[3];
MOV R1.w, -R0.y;
DP3 result.texcoord[4].y, R2, R3;
DP3 result.texcoord[4].z, vertex.normal, R3;
DP3 result.texcoord[4].x, vertex.attrib[14], R3;
ABS result.texcoord[1].xyz, R1;
MOV result.texcoord[2], R1;
DP3 result.texcoord[3].x, R1, c[28];
MOV result.texcoord[6].zw, R0;
END
# 62 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [_WorldSpaceLightPos0]
Vector 20 [unity_SHAr]
Vector 21 [unity_SHAg]
Vector 22 [unity_SHAb]
Vector 23 [unity_SHBr]
Vector 24 [unity_SHBg]
Vector 25 [unity_SHBb]
Vector 26 [unity_SHC]
Vector 27 [unity_Scale]
Vector 28 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c29, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mul r1.xyz, v2, c27.w
dp3 r2.w, r1, c9
dp3 r0.x, r1, c8
mov r0.y, r2.w
dp3 r0.z, r1, c10
mov r0.w, c29.x
mul r1, r0.xyzz, r0.yzzx
dp4 r2.z, r0, c22
dp4 r2.y, r0, c21
dp4 r2.x, r0, c20
mul r0.w, r2, r2
mad r0.w, r0.x, r0.x, -r0
dp4 r0.z, r1, c25
dp4 r0.y, r1, c24
dp4 r0.x, r1, c23
mul r1.xyz, r0.w, c26
add r0.xyz, r2, r0
add o6.xyz, r0, r1
mov r0.w, c29.x
mov r0.xyz, c16
dp4 r1.z, r0, c14
dp4 r1.y, r0, c13
dp4 r1.x, r0, c12
mad r2.xyz, r1, c27.w, -v0
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r0, v1.w
dp3 r0.w, r2, r2
rsq r0.x, r0.w
mul r1.xyz, r0.x, r2
dp3 r2.w, v2, r1
mov r1, c12
mov r0, c14
dp4 r4.z, c19, r0
mov r0, c13
dp4 r4.y, c19, r0
dp4 r4.x, c19, r1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r1.xyz, r0.xyww, c29.y
mul r1.y, r1, c17.x
mad o7.xy, r1.z, c18.zwzw, r1
mov o0, r0
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.x, r1.w
mul r1.xyz, r0.x, r1
dp4 r0.y, v0, c2
mov r1.w, -r0.y
dp3 o1.y, r2, r3
dp3 o5.y, r3, r4
add o2.w, -r2, c29.x
dp3 o1.z, v2, r2
dp3 o1.x, r2, v1
dp3 o5.z, v2, r4
dp3 o5.x, v1, r4
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c28
mov o7.zw, r0
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 368
Vector 336 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
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
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedpifkljmeldkbmmcmefkeonicpodchhbhabaaaaaaheakaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
jaaiaaaaeaaaabaaceacaaaafjaaaaaeegiocaaaaaaaaaaabgaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaacnaaaaaafjaaaaae
egiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaa
abaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
bcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaaafaaaaaa
gfaaaaadpccabaaaagaaaaaagiaaaaacafaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaiiccabaaaabaaaaaa
egiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaa
jgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaajgbebaaa
acaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaacaaaaaadiaaaaahhcaabaaa
acaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaadiaaaaajhcaabaaaadaaaaaa
fgifcaaaabaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaa
adaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaa
adaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaaa
adaaaaaaegiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaaadaaaaaaegacbaaa
adaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
cccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbccabaaa
abaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaabaaaaaaheccabaaaabaaaaaa
egbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaa
diaaaaahhcaabaaaadaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
acaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaa
acaaaaaaegacbaiaibaaaaaaabaaaaaadgaaaaafhccabaaaadaaaaaaegacbaaa
abaaaaaadiaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaa
afaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaa
aaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaa
agaaaaaackbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaa
ckiacaaaadaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaag
iccabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaadiaaaaajhcaabaaaabaaaaaa
fgifcaaaacaaaaaaaaaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaa
abaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaacaaaaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaalhcaabaaaabaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaa
acaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaalhcaabaaaabaaaaaaegiccaaa
adaaaaaabdaaaaaapgipcaaaacaaaaaaaaaaaaaaegacbaaaabaaaaaabaaaaaah
cccabaaaaeaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaabaaaaaahbccabaaa
aeaaaaaaegbcbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaheccabaaaaeaaaaaa
egbcbaaaacaaaaaaegacbaaaabaaaaaadiaaaaaihcaabaaaabaaaaaaegbcbaaa
acaaaaaapgipcaaaadaaaaaabeaaaaaadiaaaaaihcaabaaaacaaaaaafgafbaaa
abaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaaklcaabaaaabaaaaaaegiicaaa
adaaaaaaamaaaaaaagaabaaaabaaaaaaegaibaaaacaaaaaadcaaaaakhcaabaaa
abaaaaaaegiccaaaadaaaaaaaoaaaaaakgakbaaaabaaaaaaegadbaaaabaaaaaa
dgaaaaaficaabaaaabaaaaaaabeaaaaaaaaaiadpbbaaaaaibcaabaaaacaaaaaa
egiocaaaacaaaaaacgaaaaaaegaobaaaabaaaaaabbaaaaaiccaabaaaacaaaaaa
egiocaaaacaaaaaachaaaaaaegaobaaaabaaaaaabbaaaaaiecaabaaaacaaaaaa
egiocaaaacaaaaaaciaaaaaaegaobaaaabaaaaaadiaaaaahpcaabaaaadaaaaaa
jgacbaaaabaaaaaaegakbaaaabaaaaaabbaaaaaibcaabaaaaeaaaaaaegiocaaa
acaaaaaacjaaaaaaegaobaaaadaaaaaabbaaaaaiccaabaaaaeaaaaaaegiocaaa
acaaaaaackaaaaaaegaobaaaadaaaaaabbaaaaaiecaabaaaaeaaaaaaegiocaaa
acaaaaaaclaaaaaaegaobaaaadaaaaaaaaaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaegacbaaaaeaaaaaadiaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaa
bkaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaa
abaaaaaabkaabaiaebaaaaaaabaaaaaadcaaaaakhccabaaaafaaaaaaegiccaaa
acaaaaaacmaaaaaaagaabaaaabaaaaaaegacbaaaacaaaaaadiaaaaaiccaabaaa
aaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaa
abaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadp
dgaaaaafmccabaaaagaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaagaaaaaa
kgakbaaaabaaaaaamgaabaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 20 [unity_Scale]
Vector 21 [_sunLightDirection]
Vector 22 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[23] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..22] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.w, c[0].x;
MOV R1.xyz, c[17];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[20].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[18].x;
MOV R0.x, R2;
ADD result.texcoord[5].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[21];
MOV result.texcoord[5].zw, R1;
MAD result.texcoord[4].xy, vertex.texcoord[1], c[22], c[22].zwzw;
END
# 39 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [unity_Scale]
Vector 20 [_sunLightDirection]
Vector 21 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c22, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c22.x
mov r1.xyz, c16
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c19.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
add o2.w, -r0, c22.x
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c22.y
mov o0, r0
mul r1.y, r2, c17.x
mov r1.x, r2
mad o6.xy, r2.z, c18.zwzw, r1
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.x, r1.w
mul r1.xyz, r0.x, r1
dp4 r0.y, v0, c2
mov r1.w, -r0.y
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c20
mov o6.zw, r0
mad o5.xy, v4, c21, c21.zwzw
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 384
Vector 336 [_sunLightDirection]
Vector 368 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedoddegclpgifoakbnbknedcfedipalmooabaaaaaagiahaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaadamaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcjmafaaaaeaaaabaaghabaaaafjaaaaaeegiocaaaaaaaaaaa
biaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaaabaaaaaafpaaaaad
hcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
iccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaaddccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaagiaaaaacaeaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaa
dgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaa
baaaaaaiiccabaaaabaaaaaaegiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaa
diaaaaahhcaabaaaacaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaak
hcaabaaaacaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaa
acaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaa
diaaaaajhcaabaaaadaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaacaaaaaa
bbaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaacaaaaaabaaaaaaaagiacaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaa
acaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaai
hcaabaaaadaaaaaaegacbaaaadaaaaaaegiccaaaacaaaaaabdaaaaaadcaaaaal
hcaabaaaadaaaaaaegacbaaaadaaaaaapgipcaaaacaaaaaabeaaaaaaegbcbaia
ebaaaaaaaaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaa
adaaaaaabaaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaa
baaaaaaheccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaa
abaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaabaaaaaa
egacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaacaaaaaaegbcbaaa
acaaaaaaaaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaa
aaaaiadpdgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaadgaaaaaf
hccabaaaadaaaaaaegacbaaaabaaaaaadiaaaaaibcaabaaaabaaaaaabkbabaaa
aaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaa
abaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaahaaaaaadkbabaaaaaaaaaaa
akaabaaaabaaaaaadgaaaaagiccabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaa
dcaaaaaldccabaaaaeaaaaaaegbabaaaaeaaaaaaegiacaaaaaaaaaaabhaaaaaa
ogikcaaaaaaaaaaabhaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
akiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaa
aceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaafaaaaaa
kgaobaaaaaaaaaaaaaaaaaahdccabaaaafaaaaaakgakbaaaabaaaaaamgaabaaa
abaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 20 [unity_Scale]
Vector 21 [_sunLightDirection]
Vector 22 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[23] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..22] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.w, c[0].x;
MOV R1.xyz, c[17];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[20].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[18].x;
MOV R0.x, R2;
ADD result.texcoord[5].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[21];
MOV result.texcoord[5].zw, R1;
MAD result.texcoord[4].xy, vertex.texcoord[1], c[22], c[22].zwzw;
END
# 39 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [unity_Scale]
Vector 20 [_sunLightDirection]
Vector 21 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c22, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c22.x
mov r1.xyz, c16
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c19.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
add o2.w, -r0, c22.x
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c22.y
mov o0, r0
mul r1.y, r2, c17.x
mov r1.x, r2
mad o6.xy, r2.z, c18.zwzw, r1
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.x, r1.w
mul r1.xyz, r0.x, r1
dp4 r0.y, v0, c2
mov r1.w, -r0.y
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c20
mov o6.zw, r0
mad o5.xy, v4, c21, c21.zwzw
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 384
Vector 336 [_sunLightDirection]
Vector 368 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedoddegclpgifoakbnbknedcfedipalmooabaaaaaagiahaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaadamaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcjmafaaaaeaaaabaaghabaaaafjaaaaaeegiocaaaaaaaaaaa
biaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaaabaaaaaafpaaaaad
hcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
iccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaaddccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaagiaaaaacaeaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaa
dgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaa
baaaaaaiiccabaaaabaaaaaaegiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaa
diaaaaahhcaabaaaacaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaak
hcaabaaaacaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaa
acaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaa
diaaaaajhcaabaaaadaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaacaaaaaa
bbaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaacaaaaaabaaaaaaaagiacaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaa
acaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaai
hcaabaaaadaaaaaaegacbaaaadaaaaaaegiccaaaacaaaaaabdaaaaaadcaaaaal
hcaabaaaadaaaaaaegacbaaaadaaaaaapgipcaaaacaaaaaabeaaaaaaegbcbaia
ebaaaaaaaaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaa
adaaaaaabaaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaa
baaaaaaheccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaa
abaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaabaaaaaa
egacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaacaaaaaaegbcbaaa
acaaaaaaaaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaa
aaaaiadpdgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaadgaaaaaf
hccabaaaadaaaaaaegacbaaaabaaaaaadiaaaaaibcaabaaaabaaaaaabkbabaaa
aaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaa
abaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaahaaaaaadkbabaaaaaaaaaaa
akaabaaaabaaaaaadgaaaaagiccabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaa
dcaaaaaldccabaaaaeaaaaaaegbabaaaaeaaaaaaegiacaaaaaaaaaaabhaaaaaa
ogikcaaaaaaaaaaabhaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaa
akiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaa
aceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaafaaaaaa
kgaobaaaaaaaaaaaaaaaaaahdccabaaaafaaaaaakgakbaaaabaaaaaamgaabaaa
abaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_WorldSpaceLightPos0]
Vector 19 [unity_4LightPosX0]
Vector 20 [unity_4LightPosY0]
Vector 21 [unity_4LightPosZ0]
Vector 22 [unity_4LightAtten0]
Vector 23 [unity_LightColor0]
Vector 24 [unity_LightColor1]
Vector 25 [unity_LightColor2]
Vector 26 [unity_LightColor3]
Vector 27 [unity_SHAr]
Vector 28 [unity_SHAg]
Vector 29 [unity_SHAb]
Vector 30 [unity_SHBr]
Vector 31 [unity_SHBg]
Vector 32 [unity_SHBb]
Vector 33 [unity_SHC]
Vector 34 [unity_Scale]
Vector 35 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[36] = { { 1, 0 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..35] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, vertex.normal, c[34].w;
DP4 R0.x, vertex.position, c[10];
ADD R1, -R0.x, c[20];
DP3 R3.w, R3, c[10];
DP3 R4.x, R3, c[9];
DP3 R3.x, R3, c[11];
MUL R2, R3.w, R1;
DP4 R0.x, vertex.position, c[9];
ADD R0, -R0.x, c[19];
MUL R1, R1, R1;
MOV R4.z, R3.x;
MAD R2, R4.x, R0, R2;
MOV R4.w, c[0].x;
DP4 R4.y, vertex.position, c[11];
MAD R1, R0, R0, R1;
ADD R0, -R4.y, c[21];
MAD R1, R0, R0, R1;
MAD R0, R3.x, R0, R2;
MUL R2, R1, c[22];
MOV R4.y, R3.w;
RSQ R1.x, R1.x;
RSQ R1.y, R1.y;
RSQ R1.w, R1.w;
RSQ R1.z, R1.z;
MUL R0, R0, R1;
ADD R1, R2, c[0].x;
MOV R2.w, c[0].x;
RCP R1.x, R1.x;
RCP R1.y, R1.y;
RCP R1.w, R1.w;
RCP R1.z, R1.z;
MAX R0, R0, c[0].y;
MUL R0, R0, R1;
MUL R1.xyz, R0.y, c[24];
MAD R1.xyz, R0.x, c[23], R1;
MAD R0.xyz, R0.z, c[25], R1;
MAD R1.xyz, R0.w, c[26], R0;
MUL R0, R4.xyzz, R4.yzzx;
MUL R1.w, R3, R3;
DP4 R3.z, R0, c[32];
DP4 R3.y, R0, c[31];
DP4 R3.x, R0, c[30];
MAD R1.w, R4.x, R4.x, -R1;
MUL R0.xyz, R1.w, c[33];
DP4 R2.z, R4, c[29];
DP4 R2.y, R4, c[28];
DP4 R2.x, R4, c[27];
ADD R2.xyz, R2, R3;
ADD R0.xyz, R2, R0;
ADD result.texcoord[5].xyz, R0, R1;
MOV R2.xyz, c[17];
DP4 R0.z, R2, c[15];
DP4 R0.y, R2, c[14];
DP4 R0.x, R2, c[13];
MAD R0.xyz, R0, c[34].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R2.xyz, R1, vertex.attrib[14].w;
RSQ R0.w, R0.w;
MUL R3.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R3;
MOV R1, c[18];
DP4 R3.z, R1, c[15];
DP4 R3.x, R1, c[13];
DP4 R3.y, R1, c[14];
DP3 result.texcoord[0].y, R0, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
MOV R0.z, vertex.texcoord[1].x;
MOV R0.xy, vertex.texcoord[0];
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
DP3 result.texcoord[4].y, R2, R3;
DP3 result.texcoord[4].z, vertex.normal, R3;
DP3 result.texcoord[4].x, vertex.attrib[14], R3;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[35];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 88 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_WorldSpaceLightPos0]
Vector 18 [unity_4LightPosX0]
Vector 19 [unity_4LightPosY0]
Vector 20 [unity_4LightPosZ0]
Vector 21 [unity_4LightAtten0]
Vector 22 [unity_LightColor0]
Vector 23 [unity_LightColor1]
Vector 24 [unity_LightColor2]
Vector 25 [unity_LightColor3]
Vector 26 [unity_SHAr]
Vector 27 [unity_SHAg]
Vector 28 [unity_SHAb]
Vector 29 [unity_SHBr]
Vector 30 [unity_SHBg]
Vector 31 [unity_SHBb]
Vector 32 [unity_SHC]
Vector 33 [unity_Scale]
Vector 34 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c35, 1.00000000, 0.00000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mul r3.xyz, v2, c33.w
dp4 r0.x, v0, c9
add r1, -r0.x, c19
dp3 r3.w, r3, c9
dp3 r4.x, r3, c8
dp3 r3.x, r3, c10
mul r2, r3.w, r1
dp4 r0.x, v0, c8
add r0, -r0.x, c18
mul r1, r1, r1
mov r4.z, r3.x
mad r2, r4.x, r0, r2
mov r4.w, c35.x
dp4 r4.y, v0, c10
mad r1, r0, r0, r1
add r0, -r4.y, c20
mad r1, r0, r0, r1
mad r0, r3.x, r0, r2
mul r2, r1, c21
mov r4.y, r3.w
rsq r1.x, r1.x
rsq r1.y, r1.y
rsq r1.w, r1.w
rsq r1.z, r1.z
mul r0, r0, r1
add r1, r2, c35.x
dp4 r2.z, r4, c28
dp4 r2.y, r4, c27
dp4 r2.x, r4, c26
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.w, r1.w
rcp r1.z, r1.z
max r0, r0, c35.y
mul r0, r0, r1
mul r1.xyz, r0.y, c23
mad r1.xyz, r0.x, c22, r1
mad r0.xyz, r0.z, c24, r1
mad r1.xyz, r0.w, c25, r0
mul r0, r4.xyzz, r4.yzzx
mul r1.w, r3, r3
dp4 r3.z, r0, c31
dp4 r3.y, r0, c30
dp4 r3.x, r0, c29
mad r1.w, r4.x, r4.x, -r1
mul r0.xyz, r1.w, c32
add r2.xyz, r2, r3
add r0.xyz, r2, r0
add o6.xyz, r0, r1
mov r1.w, c35.x
mov r1.xyz, c16
dp4 r0.z, r1, c14
dp4 r0.y, r1, c13
dp4 r0.x, r1, c12
mad r2.xyz, r0, c33.w, -v0
mov r1.xyz, v1
mul r1.xyz, v2.zxyw, r1.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r0, v1.w
dp3 r0.w, r2, r2
rsq r0.x, r0.w
mul r1.xyz, r0.x, r2
dp3 r2.w, v2, r1
mov r0, c14
dp4 r4.z, c17, r0
mov r0, c13
dp4 r4.y, c17, r0
mov r1, c12
dp4 r4.x, c17, r1
mov r0.z, v4.x
mov r0.xy, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
dp3 o1.y, r2, r3
dp3 o5.y, r3, r4
add o2.w, -r2, c35.x
dp3 o1.z, v2, r2
dp3 o1.x, r2, v1
dp3 o5.z, v2, r4
dp3 o5.x, v1, r4
abs o2.xyz, r0
mov o3, r0
dp3 o4.x, r0, c34
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 304
Vector 272 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
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
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedegmboplkablniaiipgcjlfmmlldhlbohabaaaaaabeanaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefceialaaaaeaaaabaancacaaaafjaaaaaeegiocaaaaaaaaaaa
bcaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
cnaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadhccabaaaabaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaa
acaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
hccabaaaafaaaaaagiaaaaacagaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadgaaaaafdcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaaf
ecaabaaaaaaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaai
iccabaaaabaaaaaaegiccaaaaaaaaaaabbaaaaaaegacbaaaaaaaaaaadiaaaaah
hcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaa
abaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaabaaaaaa
diaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgbpbaaaabaaaaaadiaaaaaj
hcaabaaaacaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaa
dcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaa
aeaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaa
bcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
acaaaaaaegacbaaaacaaaaaaegiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaa
acaaaaaaegacbaaaacaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaa
aaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaa
baaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaaegacbaaaacaaaaaabaaaaaah
eccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaaacaaaaaabaaaaaahicaabaaa
aaaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaaaaaaaaaegacbaaa
acaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaacaaaaaaegbcbaaaacaaaaaa
aaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
dgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaaaaaaaaadgaaaaafhccabaaa
adaaaaaaegacbaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaabkbabaaaaaaaaaaa
ckiacaaaadaaaaaaafaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaadaaaaaa
aeaaaaaaakbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
ckiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaak
bcaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaa
aaaaaaaadgaaaaagiccabaaaadaaaaaaakaabaiaebaaaaaaaaaaaaaadiaaaaaj
hcaabaaaaaaaaaaafgifcaaaacaaaaaaaaaaaaaaegiccaaaadaaaaaabbaaaaaa
dcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaacaaaaaa
aaaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaa
bcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaa
aaaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaaacaaaaaaaaaaaaaaegacbaaa
aaaaaaaabaaaaaahcccabaaaaeaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
baaaaaahbccabaaaaeaaaaaaegbcbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaah
eccabaaaaeaaaaaaegbcbaaaacaaaaaaegacbaaaaaaaaaaadgaaaaaficaabaaa
aaaaaaaaabeaaaaaaaaaiadpdiaaaaaihcaabaaaabaaaaaaegbcbaaaacaaaaaa
pgipcaaaadaaaaaabeaaaaaadiaaaaaihcaabaaaacaaaaaafgafbaaaabaaaaaa
egiccaaaadaaaaaaanaaaaaadcaaaaaklcaabaaaabaaaaaaegiicaaaadaaaaaa
amaaaaaaagaabaaaabaaaaaaegaibaaaacaaaaaadcaaaaakhcaabaaaaaaaaaaa
egiccaaaadaaaaaaaoaaaaaakgakbaaaabaaaaaaegadbaaaabaaaaaabbaaaaai
bcaabaaaabaaaaaaegiocaaaacaaaaaacgaaaaaaegaobaaaaaaaaaaabbaaaaai
ccaabaaaabaaaaaaegiocaaaacaaaaaachaaaaaaegaobaaaaaaaaaaabbaaaaai
ecaabaaaabaaaaaaegiocaaaacaaaaaaciaaaaaaegaobaaaaaaaaaaadiaaaaah
pcaabaaaacaaaaaajgacbaaaaaaaaaaaegakbaaaaaaaaaaabbaaaaaibcaabaaa
adaaaaaaegiocaaaacaaaaaacjaaaaaaegaobaaaacaaaaaabbaaaaaiccaabaaa
adaaaaaaegiocaaaacaaaaaackaaaaaaegaobaaaacaaaaaabbaaaaaiecaabaaa
adaaaaaaegiocaaaacaaaaaaclaaaaaaegaobaaaacaaaaaaaaaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaadiaaaaahicaabaaaaaaaaaaa
bkaabaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaadcaaaaakhcaabaaa
abaaaaaaegiccaaaacaaaaaacmaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaa
diaaaaaihcaabaaaacaaaaaafgbfbaaaaaaaaaaaegiccaaaadaaaaaaanaaaaaa
dcaaaaakhcaabaaaacaaaaaaegiccaaaadaaaaaaamaaaaaaagbabaaaaaaaaaaa
egacbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaaegiccaaaadaaaaaaaoaaaaaa
kgbkbaaaaaaaaaaaegacbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaaegiccaaa
adaaaaaaapaaaaaapgbpbaaaaaaaaaaaegacbaaaacaaaaaaaaaaaaajpcaabaaa
adaaaaaafgafbaiaebaaaaaaacaaaaaaegiocaaaacaaaaaaadaaaaaadiaaaaah
pcaabaaaaeaaaaaafgafbaaaaaaaaaaaegaobaaaadaaaaaadiaaaaahpcaabaaa
adaaaaaaegaobaaaadaaaaaaegaobaaaadaaaaaaaaaaaaajpcaabaaaafaaaaaa
agaabaiaebaaaaaaacaaaaaaegiocaaaacaaaaaaacaaaaaaaaaaaaajpcaabaaa
acaaaaaakgakbaiaebaaaaaaacaaaaaaegiocaaaacaaaaaaaeaaaaaadcaaaaaj
pcaabaaaaeaaaaaaegaobaaaafaaaaaaagaabaaaaaaaaaaaegaobaaaaeaaaaaa
dcaaaaajpcaabaaaaaaaaaaaegaobaaaacaaaaaakgakbaaaaaaaaaaaegaobaaa
aeaaaaaadcaaaaajpcaabaaaadaaaaaaegaobaaaafaaaaaaegaobaaaafaaaaaa
egaobaaaadaaaaaadcaaaaajpcaabaaaacaaaaaaegaobaaaacaaaaaaegaobaaa
acaaaaaaegaobaaaadaaaaaaeeaaaaafpcaabaaaadaaaaaaegaobaaaacaaaaaa
dcaaaaanpcaabaaaacaaaaaaegaobaaaacaaaaaaegiocaaaacaaaaaaafaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpaoaaaaakpcaabaaaacaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpegaobaaaacaaaaaadiaaaaah
pcaabaaaaaaaaaaaegaobaaaaaaaaaaaegaobaaaadaaaaaadeaaaaakpcaabaaa
aaaaaaaaegaobaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
diaaaaahpcaabaaaaaaaaaaaegaobaaaacaaaaaaegaobaaaaaaaaaaadiaaaaai
hcaabaaaacaaaaaafgafbaaaaaaaaaaaegiccaaaacaaaaaaahaaaaaadcaaaaak
hcaabaaaacaaaaaaegiccaaaacaaaaaaagaaaaaaagaabaaaaaaaaaaaegacbaaa
acaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaaaiaaaaaakgakbaaa
aaaaaaaaegacbaaaacaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaacaaaaaa
ajaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaahhccabaaaafaaaaaa
egacbaaaaaaaaaaaegacbaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 19 [_WorldSpaceLightPos0]
Vector 20 [unity_4LightPosX0]
Vector 21 [unity_4LightPosY0]
Vector 22 [unity_4LightPosZ0]
Vector 23 [unity_4LightAtten0]
Vector 24 [unity_LightColor0]
Vector 25 [unity_LightColor1]
Vector 26 [unity_LightColor2]
Vector 27 [unity_LightColor3]
Vector 28 [unity_SHAr]
Vector 29 [unity_SHAg]
Vector 30 [unity_SHAb]
Vector 31 [unity_SHBr]
Vector 32 [unity_SHBg]
Vector 33 [unity_SHBb]
Vector 34 [unity_SHC]
Vector 35 [unity_Scale]
Vector 36 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[37] = { { 1, 0, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..36] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MUL R3.xyz, vertex.normal, c[35].w;
DP4 R0.x, vertex.position, c[10];
ADD R1, -R0.x, c[21];
DP3 R3.w, R3, c[10];
DP3 R4.x, R3, c[9];
DP3 R3.x, R3, c[11];
MUL R2, R3.w, R1;
DP4 R0.x, vertex.position, c[9];
ADD R0, -R0.x, c[20];
MUL R1, R1, R1;
MOV R4.z, R3.x;
MAD R2, R4.x, R0, R2;
MOV R4.w, c[0].x;
DP4 R4.y, vertex.position, c[11];
MAD R1, R0, R0, R1;
ADD R0, -R4.y, c[22];
MAD R1, R0, R0, R1;
MAD R0, R3.x, R0, R2;
MUL R2, R1, c[23];
MOV R4.y, R3.w;
RSQ R1.x, R1.x;
RSQ R1.y, R1.y;
RSQ R1.w, R1.w;
RSQ R1.z, R1.z;
MUL R0, R0, R1;
ADD R1, R2, c[0].x;
MOV R2.w, c[0].x;
RCP R1.x, R1.x;
RCP R1.y, R1.y;
RCP R1.w, R1.w;
RCP R1.z, R1.z;
MAX R0, R0, c[0].y;
MUL R0, R0, R1;
MUL R1.xyz, R0.y, c[25];
MAD R1.xyz, R0.x, c[24], R1;
MAD R0.xyz, R0.z, c[26], R1;
MAD R1.xyz, R0.w, c[27], R0;
MUL R0, R4.xyzz, R4.yzzx;
MUL R1.w, R3, R3;
DP4 R3.z, R0, c[33];
DP4 R3.y, R0, c[32];
DP4 R3.x, R0, c[31];
MAD R1.w, R4.x, R4.x, -R1;
MUL R0.xyz, R1.w, c[34];
DP4 R2.z, R4, c[30];
DP4 R2.y, R4, c[29];
DP4 R2.x, R4, c[28];
ADD R2.xyz, R2, R3;
ADD R0.xyz, R2, R0;
ADD result.texcoord[5].xyz, R0, R1;
MOV R2.xyz, c[17];
DP4 R0.z, R2, c[15];
DP4 R0.y, R2, c[14];
DP4 R0.x, R2, c[13];
MAD R0.xyz, R0, c[35].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R2.xyz, R1, vertex.attrib[14].w;
RSQ R0.w, R0.w;
MUL R3.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R3;
MOV R1, c[19];
ADD result.texcoord[1].w, -R0, c[0].x;
DP4 R3.z, R1, c[15];
DP4 R3.x, R1, c[13];
DP4 R3.y, R1, c[14];
DP3 result.texcoord[0].y, R0, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R0.w, vertex.position, c[8];
DP4 R0.z, vertex.position, c[7];
DP4 R0.x, vertex.position, c[5];
DP4 R0.y, vertex.position, c[6];
MUL R1.xyz, R0.xyww, c[0].z;
MUL R1.y, R1, c[18].x;
ADD result.texcoord[6].xy, R1, R1.z;
MOV result.position, R0;
MOV R1.xy, vertex.texcoord[0];
MOV R1.z, vertex.texcoord[1].x;
DP3 R1.w, R1, R1;
RSQ R0.x, R1.w;
MUL R1.xyz, R0.x, R1;
DP4 R0.y, vertex.position, c[3];
MOV R1.w, -R0.y;
DP3 result.texcoord[4].y, R2, R3;
DP3 result.texcoord[4].z, vertex.normal, R3;
DP3 result.texcoord[4].x, vertex.attrib[14], R3;
ABS result.texcoord[1].xyz, R1;
MOV result.texcoord[2], R1;
DP3 result.texcoord[3].x, R1, c[36];
MOV result.texcoord[6].zw, R0;
END
# 93 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [_WorldSpaceLightPos0]
Vector 20 [unity_4LightPosX0]
Vector 21 [unity_4LightPosY0]
Vector 22 [unity_4LightPosZ0]
Vector 23 [unity_4LightAtten0]
Vector 24 [unity_LightColor0]
Vector 25 [unity_LightColor1]
Vector 26 [unity_LightColor2]
Vector 27 [unity_LightColor3]
Vector 28 [unity_SHAr]
Vector 29 [unity_SHAg]
Vector 30 [unity_SHAb]
Vector 31 [unity_SHBr]
Vector 32 [unity_SHBg]
Vector 33 [unity_SHBb]
Vector 34 [unity_SHC]
Vector 35 [unity_Scale]
Vector 36 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c37, 1.00000000, 0.00000000, 0.50000000, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mul r3.xyz, v2, c35.w
dp4 r0.x, v0, c9
add r1, -r0.x, c21
dp3 r3.w, r3, c9
dp3 r4.x, r3, c8
dp3 r3.x, r3, c10
mul r2, r3.w, r1
dp4 r0.x, v0, c8
add r0, -r0.x, c20
mul r1, r1, r1
mov r4.z, r3.x
mad r2, r4.x, r0, r2
mov r4.w, c37.x
dp4 r4.y, v0, c10
mad r1, r0, r0, r1
add r0, -r4.y, c22
mad r1, r0, r0, r1
mad r0, r3.x, r0, r2
mul r2, r1, c23
mov r4.y, r3.w
rsq r1.x, r1.x
rsq r1.y, r1.y
rsq r1.w, r1.w
rsq r1.z, r1.z
mul r0, r0, r1
add r1, r2, c37.x
dp4 r2.z, r4, c30
dp4 r2.y, r4, c29
dp4 r2.x, r4, c28
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.w, r1.w
rcp r1.z, r1.z
max r0, r0, c37.y
mul r0, r0, r1
mul r1.xyz, r0.y, c25
mad r1.xyz, r0.x, c24, r1
mad r0.xyz, r0.z, c26, r1
mad r1.xyz, r0.w, c27, r0
mul r0, r4.xyzz, r4.yzzx
mul r1.w, r3, r3
dp4 r3.z, r0, c33
dp4 r3.y, r0, c32
dp4 r3.x, r0, c31
mad r1.w, r4.x, r4.x, -r1
mul r0.xyz, r1.w, c34
add r2.xyz, r2, r3
add r0.xyz, r2, r0
add o6.xyz, r0, r1
mov r1.w, c37.x
mov r1.xyz, c16
dp4 r0.z, r1, c14
dp4 r0.y, r1, c13
dp4 r0.x, r1, c12
mad r2.xyz, r0, c35.w, -v0
mov r1.xyz, v1
mul r1.xyz, v2.zxyw, r1.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r1
mul r3.xyz, r0, v1.w
dp3 r0.w, r2, r2
rsq r0.x, r0.w
mul r1.xyz, r0.x, r2
dp3 r2.w, v2, r1
mov r1, c12
mov r0, c14
dp4 r4.z, c19, r0
mov r0, c13
dp4 r4.y, c19, r0
dp4 r4.x, c19, r1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r1.xyz, r0.xyww, c37.z
mul r1.y, r1, c17.x
mad o7.xy, r1.z, c18.zwzw, r1
mov o0, r0
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.x, r1.w
mul r1.xyz, r0.x, r1
dp4 r0.y, v0, c2
mov r1.w, -r0.y
dp3 o1.y, r2, r3
dp3 o5.y, r3, r4
add o2.w, -r2, c37.x
dp3 o1.z, v2, r2
dp3 o1.x, r2, v1
dp3 o5.z, v2, r4
dp3 o5.x, v1, r4
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c36
mov o7.zw, r0
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 368
Vector 336 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
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
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedmapgelcolapkmdhcbhhhneifoilkkgbcabaaaaaameanaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
oaalaaaaeaaaabaapiacaaaafjaaaaaeegiocaaaaaaaaaaabgaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaacnaaaaaafjaaaaae
egiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaa
abaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
bcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaaafaaaaaa
gfaaaaadpccabaaaagaaaaaagiaaaaacahaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaiiccabaaaabaaaaaa
egiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaa
jgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaajgbebaaa
acaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaacaaaaaadiaaaaahhcaabaaa
acaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaadiaaaaajhcaabaaaadaaaaaa
fgifcaaaabaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaa
adaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaa
adaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaaa
adaaaaaaegiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaaadaaaaaaegacbaaa
adaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
cccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbccabaaa
abaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaabaaaaaaheccabaaaabaaaaaa
egbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaa
diaaaaahhcaabaaaadaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
acaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaa
acaaaaaaegacbaiaibaaaaaaabaaaaaadgaaaaafhccabaaaadaaaaaaegacbaaa
abaaaaaadiaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaa
afaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaa
aaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaa
agaaaaaackbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaa
ckiacaaaadaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaag
iccabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaadiaaaaajhcaabaaaabaaaaaa
fgifcaaaacaaaaaaaaaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaa
abaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaacaaaaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaalhcaabaaaabaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaa
acaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaalhcaabaaaabaaaaaaegiccaaa
adaaaaaabdaaaaaapgipcaaaacaaaaaaaaaaaaaaegacbaaaabaaaaaabaaaaaah
cccabaaaaeaaaaaaegacbaaaacaaaaaaegacbaaaabaaaaaabaaaaaahbccabaaa
aeaaaaaaegbcbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaheccabaaaaeaaaaaa
egbcbaaaacaaaaaaegacbaaaabaaaaaadgaaaaaficaabaaaabaaaaaaabeaaaaa
aaaaiadpdiaaaaaihcaabaaaacaaaaaaegbcbaaaacaaaaaapgipcaaaadaaaaaa
beaaaaaadiaaaaaihcaabaaaadaaaaaafgafbaaaacaaaaaaegiccaaaadaaaaaa
anaaaaaadcaaaaaklcaabaaaacaaaaaaegiicaaaadaaaaaaamaaaaaaagaabaaa
acaaaaaaegaibaaaadaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaaadaaaaaa
aoaaaaaakgakbaaaacaaaaaaegadbaaaacaaaaaabbaaaaaibcaabaaaacaaaaaa
egiocaaaacaaaaaacgaaaaaaegaobaaaabaaaaaabbaaaaaiccaabaaaacaaaaaa
egiocaaaacaaaaaachaaaaaaegaobaaaabaaaaaabbaaaaaiecaabaaaacaaaaaa
egiocaaaacaaaaaaciaaaaaaegaobaaaabaaaaaadiaaaaahpcaabaaaadaaaaaa
jgacbaaaabaaaaaaegakbaaaabaaaaaabbaaaaaibcaabaaaaeaaaaaaegiocaaa
acaaaaaacjaaaaaaegaobaaaadaaaaaabbaaaaaiccaabaaaaeaaaaaaegiocaaa
acaaaaaackaaaaaaegaobaaaadaaaaaabbaaaaaiecaabaaaaeaaaaaaegiocaaa
acaaaaaaclaaaaaaegaobaaaadaaaaaaaaaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaegacbaaaaeaaaaaadiaaaaahicaabaaaabaaaaaabkaabaaaabaaaaaa
bkaabaaaabaaaaaadcaaaaakicaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaa
abaaaaaadkaabaiaebaaaaaaabaaaaaadcaaaaakhcaabaaaacaaaaaaegiccaaa
acaaaaaacmaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaihcaabaaa
adaaaaaafgbfbaaaaaaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaakhcaabaaa
adaaaaaaegiccaaaadaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaaadaaaaaa
dcaaaaakhcaabaaaadaaaaaaegiccaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egacbaaaadaaaaaadcaaaaakhcaabaaaadaaaaaaegiccaaaadaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegacbaaaadaaaaaaaaaaaaajpcaabaaaaeaaaaaafgafbaia
ebaaaaaaadaaaaaaegiocaaaacaaaaaaadaaaaaadiaaaaahpcaabaaaafaaaaaa
fgafbaaaabaaaaaaegaobaaaaeaaaaaadiaaaaahpcaabaaaaeaaaaaaegaobaaa
aeaaaaaaegaobaaaaeaaaaaaaaaaaaajpcaabaaaagaaaaaaagaabaiaebaaaaaa
adaaaaaaegiocaaaacaaaaaaacaaaaaaaaaaaaajpcaabaaaadaaaaaakgakbaia
ebaaaaaaadaaaaaaegiocaaaacaaaaaaaeaaaaaadcaaaaajpcaabaaaafaaaaaa
egaobaaaagaaaaaaagaabaaaabaaaaaaegaobaaaafaaaaaadcaaaaajpcaabaaa
abaaaaaaegaobaaaadaaaaaakgakbaaaabaaaaaaegaobaaaafaaaaaadcaaaaaj
pcaabaaaaeaaaaaaegaobaaaagaaaaaaegaobaaaagaaaaaaegaobaaaaeaaaaaa
dcaaaaajpcaabaaaadaaaaaaegaobaaaadaaaaaaegaobaaaadaaaaaaegaobaaa
aeaaaaaaeeaaaaafpcaabaaaaeaaaaaaegaobaaaadaaaaaadcaaaaanpcaabaaa
adaaaaaaegaobaaaadaaaaaaegiocaaaacaaaaaaafaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpaoaaaaakpcaabaaaadaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpegaobaaaadaaaaaadiaaaaahpcaabaaaabaaaaaa
egaobaaaabaaaaaaegaobaaaaeaaaaaadeaaaaakpcaabaaaabaaaaaaegaobaaa
abaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadiaaaaahpcaabaaa
abaaaaaaegaobaaaadaaaaaaegaobaaaabaaaaaadiaaaaaihcaabaaaadaaaaaa
fgafbaaaabaaaaaaegiccaaaacaaaaaaahaaaaaadcaaaaakhcaabaaaadaaaaaa
egiccaaaacaaaaaaagaaaaaaagaabaaaabaaaaaaegacbaaaadaaaaaadcaaaaak
hcaabaaaabaaaaaaegiccaaaacaaaaaaaiaaaaaakgakbaaaabaaaaaaegacbaaa
adaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaaacaaaaaaajaaaaaapgapbaaa
abaaaaaaegacbaaaabaaaaaaaaaaaaahhccabaaaafaaaaaaegacbaaaabaaaaaa
egacbaaaacaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaa
abaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaa
aaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaagaaaaaakgaobaaa
aaaaaaaaaaaaaaahdccabaaaagaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaa
doaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_oceanOpacity]
Float 10 [_falloffPower]
Float 11 [_falloffExp]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 0, 128 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[2], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
DP3 R1.w, fragment.texcoord[0], fragment.texcoord[0];
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.xyz, R0, -R2;
MAD R1.xyz, R1, c[3].x, R2;
MOV R0.x, c[12];
ADD R0.x, -R0, c[13];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].w, -c[12];
MUL R1.xyz, R1, c[5];
MUL_SAT R0.w, R0.x, R0.y;
ADD R0.xyz, R1, -c[6];
MUL R1.y, -R0.w, c[19].w;
MUL R1.x, R0.w, R0.w;
ADD R1.y, R1, c[20].x;
MAD R2.w, -R1.x, R1.y, c[19].y;
MAD R0.xyz, R2.w, R0, c[6];
MUL R0.w, fragment.texcoord[2].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].x, R0.w;
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].y;
MAD R1.xyz, R0.w, R1, R0;
MUL R0.w, fragment.texcoord[2], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].x, R0.w;
RSQ R1.w, R1.w;
ADD R0.w, -R0, c[19].y;
MOV R0.y, c[19].z;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
MAD R1.xyz, R0.w, R2, R1;
MOV R0.xyz, fragment.texcoord[4];
MAD R0.xyz, R1.w, fragment.texcoord[0], R0;
DP3 R0.x, R0, R0;
MOV R1.w, c[19].y;
ADD R3.x, R1.w, -c[18];
RSQ R0.x, R0.x;
MUL R0.x, R0, R0.z;
MAX R0.y, fragment.texcoord[4].z, c[20];
MUL R2.xyz, R1, c[1];
MUL R2.xyz, R2, R0.y;
MAX R0.y, R0.x, c[20];
MOV R0.x, c[20].z;
MUL R0.x, R0, c[7];
POW R3.y, R0.y, R0.x;
MUL R3.z, R3.x, c[8].x;
MUL R3.y, R3, R3.z;
MOV R0, c[2];
MUL R0.xyz, R0, c[1];
MAD R0.xyz, R0, R3.y, R2;
MUL R0.xyz, R0, c[19].w;
MAD result.color.xyz, R1, fragment.texcoord[5], R0;
DP3 R3.z, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.x, R3.z;
MAD R2.x, fragment.texcoord[0].z, -R2, c[19].y;
MUL R0.x, R2, c[10];
ADD R0.y, R1.w, -c[9].x;
POW R0.x, R0.x, c[11].x;
MAD R0.x, R0, R0.y, c[9];
MUL R0.y, R0.w, c[1].w;
ADD R0.x, R0, -c[19].y;
MUL R0.y, R3, R0;
MAD R0.x, R2.w, R0, c[19].y;
MAD result.color.w, R3.x, R0.x, R0.y;
END
# 79 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_oceanOpacity]
Float 10 [_falloffPower]
Float 11 [_falloffExp]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c19, 2.71828198, 0.50000000, 2.00000000, 3.00000000
def c20, 0.00000000, 128.00000000, 1.00000000, -1.00000000
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
mov r0.x, c4
mul r0.x, c0.w, r0
mad r0.xyz, v2, c14.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mad r1.xyz, r1, c3.x, r3
mov r0.x, c13
add r0.x, -c12, r0
rcp r0.y, r0.x
mul r1.xyz, r1, c5
add r0.x, v2.w, -c12
mul_sat r0.x, r0, r0.y
mad r0.y, -r0.x, c19.z, c19.w
mul r0.x, r0, r0
mad r1.w, -r0.x, r0.y, c20.z
mul r0.x, v2, c17
mul r3.x, r0, c16
add r1.xyz, r1, -c6
mad r1.xyz, r1.w, r1, c6
pow r0, c19.x, r3.x
mul r2.w, v2, c17.x
mul r0.y, r2.w, c16.x
pow r3, c19.x, r0.y
mov r0.w, r3.x
add r2.xyz, -r1, c15
add r0.x, -r0, c20.z
mad r1.xyz, r0.x, r2, r1
add r0.w, -r0, c20.z
mov r3.y, c18.x
mov r0.y, c19
mov r0.x, v3
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r0.w, r0, r1
dp3_pp r0.w, v0, v0
rsq_pp r0.w, r0.w
mov_pp r0.xyz, v4
mad_pp r0.xyz, r0.w, v0, r0
dp3_pp r0.x, r0, r0
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r2.w, r0.x, c20.x
max_pp r0.y, v4.z, c20.x
mul_pp r2.xyz, r1, c1
mul_pp r2.xyz, r2, r0.y
mov_pp r0.y, c7.x
mul_pp r3.x, c20.y, r0.y
pow r0, r2.w, r3.x
add r2.w, c20.z, -r3.y
mul r0.y, r2.w, c8.x
mul r3.x, r0, r0.y
dp3 r0.w, v0, v0
mov_pp r0.xyz, c1
rsq r0.w, r0.w
mad r0.w, v0.z, -r0, c20.z
mul_pp r0.xyz, c2, r0
mad r0.xyz, r0, r3.x, r2
mul r2.xyz, r0, c19.z
mul r3.y, r0.w, c10.x
pow r0, r3.y, c11.x
mov r0.z, r0.x
mov r0.y, c9.x
add r0.y, c20.z, -r0
mad r0.y, r0.z, r0, c9.x
mov_pp r0.x, c1.w
mul_pp r0.z, c2.w, r0.x
add r0.x, r0.y, c20.w
mul r0.y, r3.x, r0.z
mad r0.x, r1.w, r0, c20.z
mad_pp oC0.xyz, r1, v5, r2
mad oC0.w, r2, r0.x, r0.y
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 128 [_Shininess]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedlafpmidnflfbkeaogdnmeldfklddfmodabaaaaaaliakaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefciaajaaaaeaaaaaaagaacaaaa
fjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaa
ffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaa
gcbaaaadicbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaa
adaaaaaagcbaaaadhcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagfaaaaad
pccabaaaaaaaaaaagiaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaa
aaaaaaaaadaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
egbcbaaaadaaaaaafgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaaj
pcaabaaaabaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
diaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaacaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaa
aagabaaaabaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaa
acaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaa
aaaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaadaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaaaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaak
hcaabaaaaaaaaaaaagiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaa
afaaaaaaegiccaiaebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaa
dkiacaiaebaaaaaaaaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajbcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaa
aaaaaaaaamaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaaegiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaa
acaaaaaamgbabaaaadaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaa
acaaaaaaegaabaaaacaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaa
acaaaaaaegaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaa
bjaaaaafdcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaa
egaabaiaebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaa
dcaaaaajhcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaa
abaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaacaaaaaaaagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaa
acaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaabaaaaaa
egacbaaaaaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaabaaaaaa
egbcbaaaabaaaaaaegbcbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadcaaaaajhcaabaaaacaaaaaaegbcbaaaabaaaaaapgapbaaaabaaaaaa
egbcbaaaaeaaaaaadcaaaaakicaabaaaabaaaaaackbabaiaebaaaaaaabaaaaaa
dkaabaaaabaaaaaaabeaaaaaaaaaiadpdiaaaaaiicaabaaaabaaaaaadkaabaaa
abaaaaaabkiacaaaaaaaaaaaamaaaaaacpaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaaiicaabaaaabaaaaaadkaabaaaabaaaaaackiacaaaaaaaaaaa
amaaaaaabjaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaabaaaaaahbcaabaaa
acaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaafbcaabaaaacaaaaaa
akaabaaaacaaaaaadiaaaaahbcaabaaaacaaaaaaakaabaaaacaaaaaackaabaaa
acaaaaaadeaaaaahbcaabaaaacaaaaaaakaabaaaacaaaaaaabeaaaaaaaaaaaaa
cpaaaaafbcaabaaaacaaaaaaakaabaaaacaaaaaadiaaaaaiccaabaaaacaaaaaa
akiacaaaaaaaaaaaaiaaaaaaabeaaaaaaaaaaaeddiaaaaahbcaabaaaacaaaaaa
akaabaaaacaaaaaabkaabaaaacaaaaaabjaaaaafbcaabaaaacaaaaaaakaabaaa
acaaaaaaaaaaaaajccaabaaaacaaaaaaakiacaiaebaaaaaaaaaaaaaabcaaaaaa
abeaaaaaaaaaiadpdiaaaaaiecaabaaaacaaaaaabkaabaaaacaaaaaabkiacaaa
aaaaaaaaaiaaaaaadiaaaaahbcaabaaaacaaaaaackaabaaaacaaaaaaakaabaaa
acaaaaaadiaaaaajpcaabaaaadaaaaaaegiocaaaaaaaaaaaabaaaaaaegiocaaa
aaaaaaaaacaaaaaadiaaaaahpcaabaaaadaaaaaaagaabaaaacaaaaaaegaobaaa
adaaaaaadeaaaaahbcaabaaaacaaaaaackbabaaaaeaaaaaaabeaaaaaaaaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaabaaaaaaagaabaaaacaaaaaaegacbaaa
adaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
dcaaaaajhccabaaaaaaaaaaaegacbaaaaaaaaaaaegbcbaaaafaaaaaaegacbaaa
abaaaaaaaaaaaaajbcaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaaamaaaaaa
abeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaadkaabaaaabaaaaaaakaabaaa
aaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajiccabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaacaaaaaadkaabaaaadaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Float 1 [_Mix]
Float 2 [_displacement]
Vector 3 [_Color]
Vector 4 [_ColorFromSpace]
Float 5 [_oceanOpacity]
Float 6 [_falloffPower]
Float 7 [_falloffExp]
Float 8 [_fadeStart]
Float 9 [_fadeEnd]
Float 10 [_tiling]
Vector 11 [_fogColor]
Float 12 [_heightDensityAtViewer]
Float 13 [_globalDensity]
Float 14 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [unity_Lightmap] 2D 3
"3.0-!!ARBfp1.0
PARAM c[17] = { program.local[0..14],
		{ 2, 3, 1, 2.718282 },
		{ 0.5, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[2];
MAD R3.xyz, fragment.texcoord[2], c[10].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[8].x;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[1].x, R2;
ADD R0.w, -R0, c[9].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[3];
ADD R0.w, fragment.texcoord[2], -c[8].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.x, -R0.w, c[15], c[15].y;
MUL R0.w, R0, R0;
MAD R1.w, -R0, R1.x, c[15].z;
ADD R0.xyz, R0, -c[4];
MAD R0.xyz, R1.w, R0, c[4];
MUL R0.w, fragment.texcoord[2].x, c[13].x;
MUL R0.w, R0, c[12].x;
POW R2.x, c[15].w, R0.w;
MUL R0.w, fragment.texcoord[2], c[13].x;
MUL R0.w, R0, c[12].x;
POW R0.w, c[15].w, R0.w;
ADD R1.xyz, -R0, c[11];
ADD R2.x, -R2, c[15].z;
MAD R1.xyz, R2.x, R1, R0;
ADD R0.w, -R0, c[15].z;
MOV R0.y, c[16].x;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R0.xyz, R0, -R1;
MAD R1.xyz, R0.w, R0, R1;
TEX R0, fragment.texcoord[4], texture[3], 2D;
MUL R0.xyz, R0.w, R0;
MUL R0.xyz, R0, R1;
DP3 R2.x, fragment.texcoord[0], fragment.texcoord[0];
RSQ R0.w, R2.x;
MAD R1.x, -R0.w, fragment.texcoord[0].z, c[15].z;
MOV R0.w, c[15].z;
MUL result.color.xyz, R0, c[16].y;
MUL R1.x, R1, c[6];
ADD R1.y, R0.w, -c[5].x;
POW R1.x, R1.x, c[7].x;
MAD R1.x, R1, R1.y, c[5];
ADD R0.x, R1, -c[15].z;
ADD R0.y, R0.w, -c[14].x;
MAD R0.x, R1.w, R0, c[15].z;
MUL result.color.w, R0.x, R0.y;
END
# 59 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Float 1 [_Mix]
Float 2 [_displacement]
Vector 3 [_Color]
Vector 4 [_ColorFromSpace]
Float 5 [_oceanOpacity]
Float 6 [_falloffPower]
Float 7 [_falloffExp]
Float 8 [_fadeStart]
Float 9 [_fadeEnd]
Float 10 [_tiling]
Vector 11 [_fogColor]
Float 12 [_heightDensityAtViewer]
Float 13 [_globalDensity]
Float 14 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [unity_Lightmap] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c15, 2.00000000, 3.00000000, 1.00000000, -1.00000000
def c16, 2.71828198, 0.50000000, 8.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4.xy
mov r0.x, c2
mul r0.x, c0.w, r0
mad r0.xyz, v2, c10.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c9.x
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c1.x, r3
add r0.w, -c8.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c3
add r0.w, v2, -c8.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c15, c15.y
mul r0.w, r0, r0
add r0.xyz, r0, -c4
mad r1.w, -r0, r1.x, c15.z
mad r1.xyz, r1.w, r0, c4
mul r0.x, v2, c13
mul r3.x, r0, c12
pow r0, c16.x, r3.x
mul r2.w, v2, c13.x
mul r0.y, r2.w, c12.x
pow r3, c16.x, r0.y
mov r0.w, r3.x
add r2.xyz, -r1, c11
add r0.x, -r0, c15.z
mad r1.xyz, r0.x, r2, r1
dp3 r2.x, v0, v0
add r0.w, -r0, c15.z
mov r0.y, c16
mov r0.x, v3
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r0.w, r0, r1
texld r0, v4, s3
mul_pp r0.xyz, r0.w, r0
mul_pp r0.xyz, r0, r1
rsq r2.x, r2.x
mad r0.w, -r2.x, v0.z, c15.z
mul r1.x, r0.w, c6
mul_pp oC0.xyz, r0, c16.z
pow r0, r1.x, c7.x
mov r0.y, c5.x
add r0.y, c15.z, -r0
mad r0.x, r0, r0.y, c5
mov r0.y, c14.x
add r0.x, r0, c15.w
add r0.y, c15.z, -r0
mad r0.x, r1.w, r0, c15.z
mul oC0.w, r0.x, r0.y
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [unity_Lightmap] 2D 3
ConstBuffer "$Globals" 320
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedendnhlhpcmcljbdfkondcenhgmikknfmabaaaaaabeajaaaaadaaaaaa
cmaaaaaaoeaaaaaabiabaaaaejfdeheolaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaakeaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaakeaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaakeaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcpeahaaaa
eaaaaaaapnabaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaa
abaaaaaaacaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaa
acaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaagcbaaaadhcbabaaa
abaaaaaagcbaaaadicbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaad
pcbabaaaadaaaaaagcbaaaaddcbabaaaaeaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaa
dkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaa
fgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
ggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaa
egaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaaj
pcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
agiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaaegiccaia
ebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaa
aaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
bcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaaj
bcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaa
adaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaa
acaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaa
acaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
acaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
bcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaa
aaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaa
aagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaaeaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaa
abaaaaaaabeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgapbaaaabaaaaaadiaaaaahhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
abaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaabaaaaaaegbcbaaaabaaaaaa
eeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaa
ckbabaiaebaaaaaaabaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaai
bcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaacpaaaaaf
bcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaaakaabaaa
aaaaaaaackiacaaaaaaaaaaaamaaaaaabjaaaaafbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaaaaaaaajccaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaaamaaaaaa
abeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaaa
aaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaajccaabaaaaaaaaaaaakiacaia
ebaaaaaaaaaaaaaabcaaaaaaabeaaaaaaaaaiadpdiaaaaahiccabaaaaaaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [unity_Lightmap] 2D 3
SetTexture 4 [unity_LightmapInd] 2D 4
"3.0-!!ARBfp1.0
PARAM c[23] = { program.local[0..17],
		{ 2, 3, 1, 2.718282 },
		{ 0.5, 0.57735026, 8, 0 },
		{ -0.40824828, -0.70710677, 0.57735026, 128 },
		{ 0.81649655, 0, 0.57735026 },
		{ -0.40824831, 0.70710677, 0.57735026 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[13].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[2].x, R2;
MUL R1.xyz, R0, c[4];
MOV R0.x, c[11];
ADD R1.w, -R0.x, c[12].x;
RCP R2.x, R1.w;
TEX R0, fragment.texcoord[4], texture[4], 2D;
ADD R1.w, fragment.texcoord[2], -c[11].x;
MUL_SAT R1.w, R1, R2.x;
MUL R0.xyz, R0.w, R0;
MAD R2.x, -R1.w, c[18], c[18].y;
MUL R0.w, R1, R1;
MAD R0.w, -R0, R2.x, c[18].z;
MUL R2.xyz, R0, c[19].z;
MUL R3.xyz, R2.y, c[22];
MAD R3.xyz, R2.x, c[21], R3;
ADD R1.xyz, R1, -c[5];
MAD R1.xyz, R0.w, R1, c[5];
MUL R1.w, fragment.texcoord[2].x, c[16].x;
MUL R1.w, R1, c[15].x;
POW R2.w, c[18].w, R1.w;
MAD R3.xyz, R2.z, c[20], R3;
DP3 R1.w, R3, R3;
RSQ R1.w, R1.w;
MUL R3.xyz, R1.w, R3;
MUL R1.w, fragment.texcoord[2], c[16].x;
ADD R0.xyz, -R1, c[14];
ADD R2.w, -R2, c[18].z;
MAD R1.xyz, R2.w, R0, R1;
DP3 R2.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.w, R2.w;
MAD R3.xyz, fragment.texcoord[0], R2.w, R3;
MUL R1.w, R1, c[15].x;
POW R2.w, c[18].w, R1.w;
DP3 R3.x, R3, R3;
RSQ R1.w, R3.x;
MOV R3.x, c[18].z;
ADD R2.w, -R2, c[18].z;
DP3 R2.x, R2, c[19].y;
ADD R3.w, R3.x, -c[8].x;
MOV R0.y, c[19].x;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R0.xyz, R0, -R1;
MAD R0.xyz, R2.w, R0, R1;
MUL R2.w, R1, R3.z;
TEX R1, fragment.texcoord[4], texture[3], 2D;
MUL R1.xyz, R1.w, R1;
MUL R1.xyz, R1, R2.x;
MUL R1.xyz, R1, c[19].z;
MOV R1.w, c[20];
MUL R1.w, R1, c[6].x;
MAX R2.w, R2, c[19];
POW R2.w, R2.w, R1.w;
DP3 R1.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R3.y, R1.w;
MAD R3.z, -R3.y, fragment.texcoord[0], c[18];
ADD R1.w, R3.x, -c[17].x;
MUL R3.z, R3, c[9].x;
POW R3.x, R3.z, c[10].x;
MAD R3.x, R3, R3.w, c[8];
ADD R3.x, R3, -c[18].z;
MAD R0.w, R0, R3.x, c[18].z;
MUL R2.xyz, R1, c[1];
MUL R3.y, R1.w, c[7].x;
MUL R2.xyz, R2, R3.y;
MUL R2.xyz, R2, R2.w;
MAD result.color.xyz, R0, R1, R2;
MUL result.color.w, R0, R1;
END
# 84 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [unity_Lightmap] 2D 3
SetTexture 4 [unity_LightmapInd] 2D 4
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c18, 2.00000000, 3.00000000, 1.00000000, -1.00000000
def c19, 2.71828198, 0.50000000, 8.00000000, 0.57735026
def c20, -0.40824831, 0.70710677, 0.57735026, 0.00000000
def c21, 0.81649655, 0.00000000, 0.57735026, 128.00000000
def c22, -0.40824828, -0.70710677, 0.57735026, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4.xy
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c13.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c2.x, r3
mul r1.xyz, r0, c4
texld r0, v4, s4
mul_pp r2.xyz, r0.w, r0
mov r0.x, c12
add r0.w, -c11.x, r0.x
mul_pp r4.xyz, r2, c19.z
rcp r1.w, r0.w
mul r0.xyz, r4.y, c20
add r0.w, v2, -c11.x
mul_sat r0.w, r0, r1
mad r1.w, -r0, c18.x, c18.y
mul r0.w, r0, r0
mad r1.w, -r0, r1, c18.z
add r1.xyz, r1, -c5
mad r1.xyz, r1.w, r1, c5
mad r0.xyz, r4.x, c21, r0
mad r0.xyz, r4.z, c22, r0
dp3 r2.w, r0, r0
rsq r3.x, r2.w
mul r0.w, v2.x, c16.x
add r2.xyz, -r1, c14
mul r2.w, r0, c15.x
mul r3.xyz, r3.x, r0
pow r0, c19.x, r2.w
dp3_pp r0.y, v0, v0
rsq_pp r0.y, r0.y
mad_pp r3.xyz, v0, r0.y, r3
dp3_pp r0.y, r3, r3
rsq_pp r0.w, r0.y
add r0.x, -r0, c18.z
mad r1.xyz, r0.x, r2, r1
mov r0.y, c19
mov r0.x, v3
texld r0.xyz, r0, s2
add r2.xyz, r0, -r1
mul_pp r0.y, r0.w, r3.z
max_pp r2.w, r0.y, c20
mul r0.x, v2.w, c16
mul r0.x, r0, c15
pow r3, c19.x, r0.x
mov_pp r0.y, c6.x
mul_pp r3.y, c21.w, r0
pow r0, r2.w, r3.y
mov r0.y, r3.x
add r0.y, -r0, c18.z
mad r1.xyz, r0.y, r2, r1
mov r2.w, r0.x
texld r0, v4, s3
dp3 r2.x, v0, v0
mul_pp r0.xyz, r0.w, r0
rsq r2.x, r2.x
mad r0.w, -r2.x, v0.z, c18.z
dp3_pp r2.x, r4, c19.w
mul_pp r2.xyz, r0, r2.x
mul r3.x, r0.w, c9
pow r0, r3.x, c10.x
mov r0.y, r0.x
mul_pp r2.xyz, r2, c19.z
mov r0.z, c17.x
add r0.w, c18.z, -r0.z
mov r0.x, c8
add r0.x, c18.z, -r0
mul_pp r3.xyz, r2, c1
mad r3.w, r0.y, r0.x, c8.x
mul r0.z, r0.w, c7.x
mul_pp r0.xyz, r3, r0.z
add r3.x, r3.w, c18.w
mul r0.xyz, r0, r2.w
mad r1.w, r1, r3.x, c18.z
mad_pp oC0.xyz, r1, r2, r0
mul oC0.w, r1, r0
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [unity_Lightmap] 2D 3
SetTexture 4 [unity_LightmapInd] 2D 4
ConstBuffer "$Globals" 320
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 128 [_Shininess]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedbolhaobebaamdkjjbcojgmcedanmdpdoabaaaaaabeamaaaaadaaaaaa
cmaaaaaaoeaaaaaabiabaaaaejfdeheolaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaakeaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaakeaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaakeaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcpeakaaaa
eaaaaaaalnacaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaa
abaaaaaaacaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaa
aeaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaa
ffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaa
ffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaa
gcbaaaadicbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaa
adaaaaaagcbaaaaddcbabaaaaeaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
aeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaadkiacaaa
abaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaafgifcaaa
aaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaggakbaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
egacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaakgbkbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaagiacaaa
aaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaa
aaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaaegiccaiaebaaaaaa
aaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaajbcaabaaa
abaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaadicaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaajbcaabaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaa
aaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaa
aaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaaadaaaaaa
agiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaaacaaaaaa
fgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaaacaaaaaa
aceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaaacaaaaaa
egaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaaacaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaaaaaaaaaa
agaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaa
abaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadp
efaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaaaagabaaa
acaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaaabaaaaaa
egacbaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaaeaaaaaaeghobaaa
adaaaaaaaagabaaaadaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaa
abeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgapbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaaeaaaaaaeghobaaaaeaaaaaa
aagabaaaaeaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgapbaaaabaaaaaa
baaaaaakicaabaaaabaaaaaaaceaaaaadkmnbddpdkmnbddpdkmnbddpaaaaaaaa
egacbaaaacaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaaegacbaaa
abaaaaaadiaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaa
diaaaaaihcaabaaaabaaaaaaegacbaaaabaaaaaaegiccaaaaaaaaaaaacaaaaaa
diaaaaakhcaabaaaadaaaaaafgafbaaaacaaaaaaaceaaaaaomafnblopdaedfdp
dkmnbddpaaaaaaaadcaaaaamlcaabaaaacaaaaaaagaabaaaacaaaaaaaceaaaaa
olaffbdpaaaaaaaaaaaaaaaadkmnbddpegaibaaaadaaaaaadcaaaaamhcaabaaa
acaaaaaakgakbaaaacaaaaaaaceaaaaaolafnblopdaedflpdkmnbddpaaaaaaaa
egadbaaaacaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaacaaaaaaegacbaaa
acaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaabaaaaaahicaabaaa
acaaaaaaegbcbaaaabaaaaaaegbcbaaaabaaaaaaeeaaaaaficaabaaaacaaaaaa
dkaabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaapgapbaaaacaaaaaaegbcbaaa
abaaaaaadcaaaaakicaabaaaacaaaaaackbabaiaebaaaaaaabaaaaaadkaabaaa
acaaaaaaabeaaaaaaaaaiadpdiaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaa
bkiacaaaaaaaaaaaamaaaaaacpaaaaaficaabaaaacaaaaaadkaabaaaacaaaaaa
diaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaackiacaaaaaaaaaaaamaaaaaa
bjaaaaaficaabaaaacaaaaaadkaabaaaacaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaacaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaa
abaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaaficaabaaaabaaaaaa
dkaabaaaabaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaackaabaaa
acaaaaaadeaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaaabeaaaaaaaaaaaaa
cpaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaaibcaabaaaacaaaaaa
akiacaaaaaaaaaaaaiaaaaaaabeaaaaaaaaaaaeddiaaaaahicaabaaaabaaaaaa
dkaabaaaabaaaaaaakaabaaaacaaaaaabjaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaaaaaaaaajbcaabaaaacaaaaaaakiacaiaebaaaaaaaaaaaaaabcaaaaaa
abeaaaaaaaaaiadpdiaaaaaiccaabaaaacaaaaaaakaabaaaacaaaaaabkiacaaa
aaaaaaaaaiaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaafgafbaaa
acaaaaaadcaaaaajhccabaaaaaaaaaaaegacbaaaabaaaaaapgapbaaaabaaaaaa
egacbaaaaaaaaaaaaaaaaaajbcaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaa
amaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaadkaabaaaacaaaaaa
akaabaaaaaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahiccabaaaaaaaaaaa
akaabaaaacaaaaaaakaabaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_oceanOpacity]
Float 10 [_falloffPower]
Float 11 [_falloffExp]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_ShadowMapTexture] 2D 3
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 2.718282, 1, 0.5, 2 },
		{ 3, 0, 128 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[2], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
DP3 R1.w, fragment.texcoord[0], fragment.texcoord[0];
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.xyz, R0, -R2;
MAD R1.xyz, R1, c[3].x, R2;
MOV R0.x, c[12];
ADD R0.x, -R0, c[13];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].w, -c[12];
MUL R1.xyz, R1, c[5];
MUL_SAT R0.w, R0.x, R0.y;
ADD R0.xyz, R1, -c[6];
MUL R1.y, -R0.w, c[19].w;
MUL R1.x, R0.w, R0.w;
ADD R1.y, R1, c[20].x;
MAD R2.w, -R1.x, R1.y, c[19].y;
MAD R0.xyz, R2.w, R0, c[6];
MUL R0.w, fragment.texcoord[2].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].x, R0.w;
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].y;
MAD R1.xyz, R0.w, R1, R0;
MUL R0.w, fragment.texcoord[2], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].x, R0.w;
RSQ R1.w, R1.w;
ADD R0.w, -R0, c[19].y;
TXP R4.x, fragment.texcoord[6], texture[3], 2D;
MOV R0.y, c[19].z;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
MAD R1.xyz, R0.w, R2, R1;
MOV R0.xyz, fragment.texcoord[4];
MAD R0.xyz, R1.w, fragment.texcoord[0], R0;
DP3 R0.x, R0, R0;
MOV R1.w, c[19].y;
ADD R3.x, R1.w, -c[18];
RSQ R0.x, R0.x;
MUL R0.x, R0, R0.z;
MAX R0.y, fragment.texcoord[4].z, c[20];
MUL R2.xyz, R1, c[1];
MUL R2.xyz, R2, R0.y;
MAX R0.y, R0.x, c[20];
MOV R0.x, c[20].z;
MUL R0.x, R0, c[7];
POW R3.y, R0.y, R0.x;
MOV R0, c[2];
MUL R3.z, R3.x, c[8].x;
MUL R3.y, R3, R3.z;
MUL R0.xyz, R0, c[1];
MAD R0.xyz, R0, R3.y, R2;
MUL R2.y, R4.x, c[19].w;
MUL R0.xyz, R0, R2.y;
MAD result.color.xyz, R1, fragment.texcoord[5], R0;
DP3 R2.x, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.x, R2.x;
MAD R0.x, fragment.texcoord[0].z, -R2, c[19].y;
MUL R0.x, R0, c[10];
ADD R0.y, R1.w, -c[9].x;
POW R0.x, R0.x, c[11].x;
MAD R0.x, R0, R0.y, c[9];
MUL R0.z, R0.w, c[1].w;
MUL R0.y, R3, R0.z;
ADD R0.x, R0, -c[19].y;
MUL R0.y, R4.x, R0;
MAD R0.x, R2.w, R0, c[19].y;
MAD result.color.w, R3.x, R0.x, R0.y;
END
# 82 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_oceanOpacity]
Float 10 [_falloffPower]
Float 11 [_falloffExp]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_ShadowMapTexture] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c19, 2.71828198, 0.50000000, 2.00000000, 3.00000000
def c20, 0.00000000, 128.00000000, 1.00000000, -1.00000000
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
dcl_texcoord6 v6
mov r0.x, c4
mul r0.x, c0.w, r0
mad r2.xyz, v2, c14.x, r0.x
texld r0.xyz, r2.zyzw, s0
mul r1.xyz, v1.x, r0
texld r0.xyz, r2, s0
mad r3.xyz, v1.z, r0, r1
texld r0.xyz, r2.zxzw, s0
mad r3.xyz, v1.y, r0, r3
texld r1.xyz, r2.zyzw, s1
texld r0.xyz, r2, s1
mul r1.xyz, v1.x, r1
mad r1.xyz, v1.z, r0, r1
texld r0.xyz, r2.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mad r1.xyz, r1, c3.x, r3
mov r0.x, c13
add r0.x, -c12, r0
rcp r0.y, r0.x
mul r1.xyz, r1, c5
add r0.x, v2.w, -c12
mul_sat r0.x, r0, r0.y
mad r0.y, -r0.x, c19.z, c19.w
mul r0.x, r0, r0
mad r2.w, -r0.x, r0.y, c20.z
add r1.xyz, r1, -c6
mad r3.xyz, r2.w, r1, c6
mul r0.x, v2, c17
mul r1.y, r0.x, c16.x
pow r0, c19.x, r1.y
mul r1.x, v2.w, c17
mul r0.y, r1.x, c16.x
pow r1, c19.x, r0.y
mov r0.w, r1.x
add r2.xyz, -r3, c15
add r0.x, -r0, c20.z
mad r2.xyz, r0.x, r2, r3
add r0.w, -r0, c20.z
mov r3.y, c18.x
mov r0.y, c19
mov r0.x, v3
texld r0.xyz, r0, s2
add r0.xyz, r0, -r2
mad r2.xyz, r0.w, r0, r2
dp3_pp r0.w, v0, v0
rsq_pp r0.w, r0.w
mov_pp r0.xyz, v4
mad_pp r0.xyz, r0.w, v0, r0
dp3_pp r0.x, r0, r0
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r1.w, r0.x, c20.x
max_pp r0.y, v4.z, c20.x
mul_pp r1.xyz, r2, c1
mul_pp r1.xyz, r1, r0.y
mov_pp r0.y, c7.x
mul_pp r3.x, c20.y, r0.y
pow r0, r1.w, r3.x
add r1.w, c20.z, -r3.y
mul r0.y, r1.w, c8.x
mul r3.w, r0.x, r0.y
mov_pp r0.xyz, c1
mul_pp r0.xyz, c2, r0
mad r3.xyz, r0, r3.w, r1
texldp r1.x, v6, s3
dp3 r0.w, v0, v0
rsq r0.w, r0.w
mad r0.x, v0.z, -r0.w, c20.z
mul r1.z, r0.x, c10.x
pow r0, r1.z, c11.x
mov r0.z, r0.x
mov_pp r0.x, c1.w
mul_pp r1.y, r1.x, c19.z
mul r3.xyz, r3, r1.y
mov r0.y, c9.x
add r0.y, c20.z, -r0
mul_pp r0.w, c2, r0.x
mad r0.x, r0.z, r0.y, c9
mul r0.y, r3.w, r0.w
add r0.x, r0, c20.w
mul r0.y, r1.x, r0
mad r0.x, r2.w, r0, c20.z
mad_pp oC0.xyz, r2, v5, r3
mad oC0.w, r1, r0.x, r0.y
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_WaterTex] 2D 1
SetTexture 1 [_WaterTex1] 2D 2
SetTexture 2 [_fogColorRamp] 2D 3
SetTexture 3 [_ShadowMapTexture] 2D 0
ConstBuffer "$Globals" 368
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 192 [_Shininess]
Float 196 [_Gloss]
Float 256 [_oceanOpacity]
Float 260 [_falloffPower]
Float 264 [_falloffExp]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecednlkofgedgkdlaehghfomkikefkoaabplabaaaaaahaalaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaaneaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahahaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaaapalaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefccaakaaaaeaaaaaaaiiacaaaafjaaaaaeegiocaaa
aaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadhcbabaaa
aeaaaaaagcbaaaadhcbabaaaafaaaaaagcbaaaadlcbabaaaagaaaaaagfaaaaad
pccabaaaaaaaaaaagiaaaaacafaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaa
aaaaaaaaahaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
egbcbaaaadaaaaaafgifcaaaaaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaaj
pcaabaaaabaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaa
diaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaaj
pcaabaaaacaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaa
aagabaaaacaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaa
acaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaa
acaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaa
aaaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaa
egacbaaaadaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaaaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaai
hcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaak
hcaabaaaaaaaaaaaagiacaaaaaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaa
ajaaaaaaegiccaiaebaaaaaaaaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaa
dkiacaiaebaaaaaaaaaaaaaabaaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaaaaaaaajbcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaa
aaaaaaaabaaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaama
abeaaaaaaaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaa
egacbaaaaaaaaaaaegiccaaaaaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaa
acaaaaaamgbabaaaadaaaaaaagiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaa
acaaaaaaegaabaaaacaaaaaafgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaa
acaaaaaaegaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaa
bjaaaaafdcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaa
egaabaiaebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaa
dcaaaaajhcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaafbcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaa
abaaaaaaabeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaacaaaaaaaagabaaaadaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaa
acaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaa
egacbaaaaaaaaaaaegbcbaaaafaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaabaaaaaaegbcbaaa
abaaaaaaegbcbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaa
dcaaaaajhcaabaaaacaaaaaaegbcbaaaabaaaaaapgapbaaaabaaaaaaegbcbaaa
aeaaaaaadcaaaaakicaabaaaabaaaaaackbabaiaebaaaaaaabaaaaaadkaabaaa
abaaaaaaabeaaaaaaaaaiadpdiaaaaaiicaabaaaabaaaaaadkaabaaaabaaaaaa
bkiacaaaaaaaaaaabaaaaaaacpaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaa
diaaaaaiicaabaaaabaaaaaadkaabaaaabaaaaaackiacaaaaaaaaaaabaaaaaaa
bjaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaabaaaaaahbcaabaaaacaaaaaa
egacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaafbcaabaaaacaaaaaaakaabaaa
acaaaaaadiaaaaahbcaabaaaacaaaaaaakaabaaaacaaaaaackaabaaaacaaaaaa
deaaaaahbcaabaaaacaaaaaaakaabaaaacaaaaaaabeaaaaaaaaaaaaacpaaaaaf
bcaabaaaacaaaaaaakaabaaaacaaaaaadiaaaaaiccaabaaaacaaaaaaakiacaaa
aaaaaaaaamaaaaaaabeaaaaaaaaaaaeddiaaaaahbcaabaaaacaaaaaaakaabaaa
acaaaaaabkaabaaaacaaaaaabjaaaaafbcaabaaaacaaaaaaakaabaaaacaaaaaa
aaaaaaajccaabaaaacaaaaaaakiacaiaebaaaaaaaaaaaaaabgaaaaaaabeaaaaa
aaaaiadpdiaaaaaiecaabaaaacaaaaaabkaabaaaacaaaaaabkiacaaaaaaaaaaa
amaaaaaadiaaaaahbcaabaaaacaaaaaackaabaaaacaaaaaaakaabaaaacaaaaaa
diaaaaajpcaabaaaadaaaaaaegiocaaaaaaaaaaaabaaaaaaegiocaaaaaaaaaaa
acaaaaaadiaaaaahpcaabaaaadaaaaaaagaabaaaacaaaaaaegaobaaaadaaaaaa
deaaaaahbcaabaaaacaaaaaackbabaaaaeaaaaaaabeaaaaaaaaaaaaadcaaaaaj
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaadaaaaaa
aoaaaaahfcaabaaaacaaaaaaagbbbaaaagaaaaaapgbpbaaaagaaaaaaefaaaaaj
pcaabaaaaeaaaaaaigaabaaaacaaaaaaeghobaaaadaaaaaaaagabaaaaaaaaaaa
aaaaaaahbcaabaaaacaaaaaaakaabaaaaeaaaaaaakaabaaaaeaaaaaadiaaaaah
ecaabaaaacaaaaaadkaabaaaadaaaaaaakaabaaaaeaaaaaadcaaaaajhccabaaa
aaaaaaaaegacbaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaaaaaaaaj
bcaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaabaaaaaaaabeaaaaaaaaaiadp
dcaaaaakbcaabaaaaaaaaaaadkaabaaaabaaaaaaakaabaaaaaaaaaaaakiacaaa
aaaaaaaabaaaaaaaaaaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaa
aaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajiccabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaaa
acaaaaaackaabaaaacaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Float 1 [_Mix]
Float 2 [_displacement]
Vector 3 [_Color]
Vector 4 [_ColorFromSpace]
Float 5 [_oceanOpacity]
Float 6 [_falloffPower]
Float 7 [_falloffExp]
Float 8 [_fadeStart]
Float 9 [_fadeEnd]
Float 10 [_tiling]
Vector 11 [_fogColor]
Float 12 [_heightDensityAtViewer]
Float 13 [_globalDensity]
Float 14 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_ShadowMapTexture] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
"3.0-!!ARBfp1.0
PARAM c[17] = { program.local[0..14],
		{ 2, 3, 1, 2.718282 },
		{ 0.5, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[2];
MAD R3.xyz, fragment.texcoord[2], c[10].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[8].x;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[1].x, R2;
ADD R0.w, -R0, c[9].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[3];
ADD R0.w, fragment.texcoord[2], -c[8].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.x, -R0.w, c[15], c[15].y;
MUL R0.w, R0, R0;
MAD R1.w, -R0, R1.x, c[15].z;
ADD R0.xyz, R0, -c[4];
MAD R0.xyz, R1.w, R0, c[4];
MUL R0.w, fragment.texcoord[2].x, c[13].x;
MUL R0.w, R0, c[12].x;
POW R2.x, c[15].w, R0.w;
MUL R0.w, fragment.texcoord[2], c[13].x;
MUL R0.w, R0, c[12].x;
POW R0.w, c[15].w, R0.w;
ADD R1.xyz, -R0, c[11];
ADD R2.x, -R2, c[15].z;
MAD R1.xyz, R2.x, R1, R0;
ADD R0.w, -R0, c[15].z;
TXP R3.x, fragment.texcoord[5], texture[3], 2D;
MOV R0.y, c[16].x;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R0.xyz, R0, -R1;
MAD R1.xyz, R0.w, R0, R1;
TEX R0, fragment.texcoord[4], texture[4], 2D;
MUL R2.xyz, R0, R3.x;
MUL R0.xyz, R0.w, R0;
DP3 R0.w, fragment.texcoord[0], fragment.texcoord[0];
MUL R0.xyz, R0, c[16].y;
MUL R2.xyz, R2, c[15].x;
MIN R2.xyz, R0, R2;
MUL R0.xyz, R0, R3.x;
MAX R0.xyz, R2, R0;
RSQ R0.w, R0.w;
MAD R2.x, -R0.w, fragment.texcoord[0].z, c[15].z;
MOV R0.w, c[15].z;
MUL result.color.xyz, R1, R0;
MUL R2.x, R2, c[6];
ADD R2.y, R0.w, -c[5].x;
POW R2.x, R2.x, c[7].x;
MAD R2.x, R2, R2.y, c[5];
ADD R0.x, R2, -c[15].z;
ADD R0.y, R0.w, -c[14].x;
MAD R0.x, R1.w, R0, c[15].z;
MUL result.color.w, R0.x, R0.y;
END
# 65 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
Vector 0 [_SinTime]
Float 1 [_Mix]
Float 2 [_displacement]
Vector 3 [_Color]
Vector 4 [_ColorFromSpace]
Float 5 [_oceanOpacity]
Float 6 [_falloffPower]
Float 7 [_falloffExp]
Float 8 [_fadeStart]
Float 9 [_fadeEnd]
Float 10 [_tiling]
Vector 11 [_fogColor]
Float 12 [_heightDensityAtViewer]
Float 13 [_globalDensity]
Float 14 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_ShadowMapTexture] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c15, 2.00000000, 3.00000000, 1.00000000, -1.00000000
def c16, 2.71828198, 0.50000000, 8.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4.xy
dcl_texcoord5 v5
mov r0.x, c2
mul r0.x, c0.w, r0
mad r0.xyz, v2, c10.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c9.x
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c1.x, r3
add r0.w, -c8.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c3
add r0.w, v2, -c8.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c15, c15.y
mul r0.w, r0, r0
add r0.xyz, r0, -c4
mad r1.w, -r0, r1.x, c15.z
mad r1.xyz, r1.w, r0, c4
mul r0.x, v2, c13
mul r3.x, r0, c12
pow r0, c16.x, r3.x
mul r2.w, v2, c13.x
mul r0.y, r2.w, c12.x
pow r3, c16.x, r0.y
mov r0.w, r3.x
add r2.xyz, -r1, c11
add r0.x, -r0, c15.z
mad r1.xyz, r0.x, r2, r1
add r0.w, -r0, c15.z
texldp r3.x, v5, s3
mov r0.y, c16
mov r0.x, v3
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r0.w, r0, r1
texld r0, v4, s4
mul_pp r2.xyz, r0.w, r0
mul_pp r0.xyz, r0, r3.x
dp3 r0.w, v0, v0
rsq r0.w, r0.w
mul_pp r2.xyz, r2, c16.z
mul_pp r0.xyz, r0, c15.x
min_pp r0.xyz, r2, r0
mul_pp r2.xyz, r2, r3.x
max_pp r0.xyz, r0, r2
mad r0.w, -r0, v0.z, c15.z
mul r2.x, r0.w, c6
mul_pp oC0.xyz, r1, r0
pow r0, r2.x, c7.x
mov r0.y, c5.x
add r0.y, c15.z, -r0
mad r0.x, r0, r0.y, c5
mov r0.y, c14.x
add r0.x, r0, c15.w
add r0.y, c15.z, -r0
mad r0.x, r1.w, r0, c15.z
mul oC0.w, r0.x, r0.y
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" }
SetTexture 0 [_WaterTex] 2D 1
SetTexture 1 [_WaterTex1] 2D 2
SetTexture 2 [_fogColorRamp] 2D 3
SetTexture 3 [_ShadowMapTexture] 2D 0
SetTexture 4 [unity_Lightmap] 2D 4
ConstBuffer "$Globals" 384
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 256 [_oceanOpacity]
Float 260 [_falloffPower]
Float 264 [_falloffExp]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecednngnpghakaohjapmdpgpdapngoimmajgabaaaaaacaakaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaadadaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
apalaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcoiaiaaaaeaaaaaaadkacaaaa
fjaaaaaeegiocaaaaaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaae
aahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaae
aahabaaaaeaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaa
abaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaad
dcbabaaaaeaaaaaagcbaaaadlcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaahaaaaaa
dkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaa
fgifcaaaaaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
ggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaa
egaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaaj
pcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaabaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
agiacaaaaaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaajaaaaaaegiccaia
ebaaaaaaaaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaa
aaaaaaaabaaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
bcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaabaaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaaj
bcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaa
adaaaaaaagiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaa
acaaaaaafgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaa
acaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
acaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
bcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaa
aaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaa
aagabaaaadaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaaafaaaaaa
pgbpbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
adaaaaaaaagabaaaaaaaaaaaaaaaaaahccaabaaaabaaaaaaakaabaaaabaaaaaa
akaabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaaeaaaaaaeghobaaa
aeaaaaaaaagabaaaaeaaaaaadiaaaaahocaabaaaabaaaaaafgafbaaaabaaaaaa
agajbaaaacaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaa
ddaaaaahocaabaaaabaaaaaafgaobaaaabaaaaaaagajbaaaacaaaaaadiaaaaah
hcaabaaaacaaaaaaagaabaaaabaaaaaaegacbaaaacaaaaaadeaaaaahhcaabaaa
abaaaaaajgahbaaaabaaaaaaegacbaaaacaaaaaadiaaaaahhccabaaaaaaaaaaa
egacbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaa
abaaaaaaegbcbaaaabaaaaaaeeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaackbabaiaebaaaaaaabaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaiadpdiaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaa
aaaaaaaabaaaaaaacpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaai
bcaabaaaaaaaaaaaakaabaaaaaaaaaaackiacaaaaaaaaaaabaaaaaaabjaaaaaf
bcaabaaaaaaaaaaaakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaaakiacaia
ebaaaaaaaaaaaaaabaaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaa
akaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaabaaaaaaaaaaaaaah
bcaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaa
aaaaaaaadkaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpaaaaaaaj
ccaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaabgaaaaaaabeaaaaaaaaaiadp
diaaaaahiccabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaadoaaaaab
"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_ShadowMapTexture] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"3.0-!!ARBfp1.0
PARAM c[23] = { program.local[0..17],
		{ 2, 3, 1, 2.718282 },
		{ 0.5, 0.57735026, 8, 0 },
		{ -0.40824828, -0.70710677, 0.57735026, 128 },
		{ 0.81649655, 0, 0.57735026 },
		{ -0.40824831, 0.70710677, 0.57735026 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R1.xyz, fragment.texcoord[2], c[13].x, R0.x;
TEX R0.xyz, R1.zyzw, texture[0], 2D;
MOV R0.w, c[11].x;
ADD R0.w, -R0, c[12].x;
MUL R1.w, fragment.texcoord[2].x, c[16].x;
MUL R1.w, R1, c[15].x;
POW R1.w, c[18].w, R1.w;
TEX R2.xyz, R1, texture[0], 2D;
MUL R0.xyz, fragment.texcoord[1].x, R0;
MAD R0.xyz, fragment.texcoord[1].z, R2, R0;
TEX R3.xyz, R1.zxzw, texture[0], 2D;
MAD R0.xyz, fragment.texcoord[1].y, R3, R0;
TEX R2.xyz, R1.zyzw, texture[1], 2D;
TEX R3.xyz, R1, texture[1], 2D;
MUL R2.xyz, fragment.texcoord[1].x, R2;
MAD R2.xyz, fragment.texcoord[1].z, R3, R2;
TEX R1.xyz, R1.zxzw, texture[1], 2D;
MAD R1.xyz, R1, fragment.texcoord[1].y, R2;
ADD R1.xyz, R1, -R0;
MAD R0.xyz, R1, c[2].x, R0;
TEX R2, fragment.texcoord[4], texture[5], 2D;
MUL R0.xyz, R0, c[4];
MUL R2.xyz, R2.w, R2;
RCP R0.w, R0.w;
ADD R1.x, fragment.texcoord[2].w, -c[11];
MUL_SAT R1.x, R1, R0.w;
MAD R0.w, -R1.x, c[18].x, c[18].y;
MUL R1.x, R1, R1;
MAD R0.w, -R1.x, R0, c[18].z;
ADD R0.xyz, R0, -c[5];
MAD R0.xyz, R0.w, R0, c[5];
ADD R1.xyz, -R0, c[14];
ADD R1.w, -R1, c[18].z;
MAD R0.xyz, R1.w, R1, R0;
MUL R1.xyz, R2, c[19].z;
MUL R3.xyz, R1.y, c[22];
MAD R3.xyz, R1.x, c[21], R3;
DP3 R1.y, R1, c[19].y;
MUL R1.w, fragment.texcoord[2], c[16].x;
MUL R1.w, R1, c[15].x;
POW R2.w, c[18].w, R1.w;
MAD R3.xyz, R1.z, c[20], R3;
DP3 R1.w, R3, R3;
ADD R2.w, -R2, c[18].z;
RSQ R1.w, R1.w;
TXP R1.x, fragment.texcoord[5], texture[3], 2D;
MOV R2.y, c[19].x;
MOV R2.x, fragment.texcoord[3];
TEX R2.xyz, R2, texture[2], 2D;
ADD R2.xyz, R2, -R0;
MAD R0.xyz, R2.w, R2, R0;
MUL R2.xyz, R1.w, R3;
DP3 R2.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R1.w, R2.w;
MAD R2.xyz, fragment.texcoord[0], R1.w, R2;
DP3 R1.w, R2, R2;
TEX R3, fragment.texcoord[4], texture[4], 2D;
MUL R2.xyw, R3.w, R3.xyzz;
MUL R2.xyw, R2, R1.y;
MUL R3.xyz, R3, R1.x;
MUL R2.xyw, R2, c[19].z;
RSQ R1.w, R1.w;
MUL R2.z, R1.w, R2;
MUL R3.xyz, R3, c[18].x;
MIN R3.xyz, R2.xyww, R3;
MUL R1.xyz, R2.xyww, R1.x;
MAX R1.xyz, R3, R1;
MOV R3.x, c[20].w;
MUL R1.w, R3.x, c[6].x;
MAX R2.z, R2, c[19].w;
POW R1.w, R2.z, R1.w;
MUL R2.xyz, R2.xyww, c[1];
DP3 R2.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R3.y, R2.w;
MOV R3.x, c[18].z;
ADD R2.w, R3.x, -c[17].x;
MAD R3.z, -R3.y, fragment.texcoord[0], c[18];
MUL R3.y, R2.w, c[7].x;
MUL R3.z, R3, c[9].x;
MUL R2.xyz, R2, R3.y;
MUL R2.xyz, R2, R1.w;
ADD R3.x, R3, -c[8];
POW R3.z, R3.z, c[10].x;
MAD R3.x, R3.z, R3, c[8];
ADD R3.x, R3, -c[18].z;
MAD R0.w, R0, R3.x, c[18].z;
MAD result.color.xyz, R0, R1, R2;
MUL result.color.w, R0, R2;
END
# 90 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_ShadowMapTexture] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
def c18, 2.00000000, 3.00000000, 1.00000000, -1.00000000
def c19, 2.71828198, 0.50000000, 8.00000000, 0.57735026
def c20, -0.40824831, 0.70710677, 0.57735026, 0.00000000
def c21, 0.81649655, 0.00000000, 0.57735026, 128.00000000
def c22, -0.40824828, -0.70710677, 0.57735026, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4.xy
dcl_texcoord5 v5
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c13.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mad r1.xyz, r1, c2.x, r3
mov r0.x, c12
add r0.x, -c11, r0
rcp r0.y, r0.x
mul r1.xyz, r1, c4
add r0.x, v2.w, -c11
mul_sat r0.x, r0, r0.y
mad r0.y, -r0.x, c18.x, c18
mul r0.x, r0, r0
mad r2.w, -r0.x, r0.y, c18.z
add r1.xyz, r1, -c5
mad r2.xyz, r2.w, r1, c5
mul r1.x, v2, c16
mul r3.w, r1.x, c15.x
pow r1, c19.x, r3.w
texld r0, v4, s5
mul_pp r0.xyz, r0.w, r0
mul_pp r0.xyz, r0, c19.z
mov r0.w, r1.x
mul r4.xyz, r0.y, c20
mad r1.xyz, r0.x, c21, r4
mad r1.xyz, r0.z, c22, r1
dp3 r1.w, r1, r1
rsq r1.w, r1.w
add r3.xyz, -r2, c14
add r0.w, -r0, c18.z
mad r2.xyz, r0.w, r3, r2
mul r0.w, v2, c16.x
mul r0.w, r0, c15.x
mul r4.xyz, r1.w, r1
pow r1, c19.x, r0.w
dp3_pp r0.w, v0, v0
rsq_pp r0.w, r0.w
mad_pp r4.xyz, v0, r0.w, r4
dp3_pp r1.w, r4, r4
mov r0.w, r1.x
texldp r4.x, v5, s3
add r0.w, -r0, c18.z
mov r3.y, c19
mov r3.x, v3
texld r3.xyz, r3, s2
add r3.xyz, r3, -r2
mad r1.xyz, r0.w, r3, r2
texld r3, v4, s4
mul_pp r2.xyz, r3.w, r3
rsq_pp r0.w, r1.w
dp3_pp r0.x, r0, c19.w
mul_pp r0.xyz, r2, r0.x
mul_pp r2.xyz, r0, c19.z
mul_pp r3.xyz, r3, r4.x
mul_pp r0.xyz, r3, c18.x
min_pp r0.xyz, r2, r0
mul_pp r3.xyz, r2, r4.x
mul_pp r0.w, r0, r4.z
max_pp r4.xyz, r0, r3
mov_pp r0.y, c6.x
dp3 r0.x, v0, v0
rsq r0.x, r0.x
max_pp r3.x, r0.w, c20.w
mul_pp r3.y, c21.w, r0
mad r1.w, -r0.x, v0.z, c18.z
pow r0, r3.x, r3.y
mul r0.y, r1.w, c9.x
pow r3, r0.y, c10.x
mov r0.w, r0.x
mul_pp r0.xyz, r2, c1
mov r1.w, c17.x
add r1.w, c18.z, -r1
mov r2.x, c8
mul r2.z, r1.w, c7.x
mul_pp r0.xyz, r0, r2.z
mul r0.xyz, r0, r0.w
mov r2.y, r3.x
add r2.x, c18.z, -r2
mad r2.x, r2.y, r2, c8
add r2.x, r2, c18.w
mad r0.w, r2, r2.x, c18.z
mad_pp oC0.xyz, r1, r4, r0
mul oC0.w, r0, r1
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" "SHADOWS_SCREEN" "LIGHTMAP_ON" "DIRLIGHTMAP_ON" }
SetTexture 0 [_WaterTex] 2D 1
SetTexture 1 [_WaterTex1] 2D 2
SetTexture 2 [_fogColorRamp] 2D 3
SetTexture 3 [_ShadowMapTexture] 2D 0
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
ConstBuffer "$Globals" 384
Vector 32 [_SpecColor]
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 192 [_Shininess]
Float 196 [_Gloss]
Float 256 [_oceanOpacity]
Float 260 [_falloffPower]
Float 264 [_falloffExp]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedbbaabllgfnahmgghiogbikhocbogonjdabaaaaaacaanaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaadadaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
apalaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcoialaaaaeaaaaaaapkacaaaa
fjaaaaaeegiocaaaaaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaad
aagabaaaafaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaa
afaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaaddcbabaaa
aeaaaaaagcbaaaadlcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
afaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaahaaaaaadkiacaaa
abaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaafgifcaaa
aaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaggakbaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadiaaaaahhcaabaaaabaaaaaa
egacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
abaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
abaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaakgbkbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaagiacaaa
aaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaa
aaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaajaaaaaaegiccaiaebaaaaaa
aaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaa
baaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaajbcaabaaa
abaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaabaaaaaaadicaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaajbcaabaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaa
aaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaa
aaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaaadaaaaaa
agiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaaacaaaaaa
fgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaaacaaaaaa
aceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaaacaaaaaa
egaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaaacaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaaaaaaaaaa
agaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaa
abaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadp
efaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaaaagabaaa
adaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaaabaaaaaa
egacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaaafaaaaaapgbpbaaa
afaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaadaaaaaa
aagabaaaaaaaaaaaaaaaaaahccaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaaeaaaaaaeghobaaaaeaaaaaa
aagabaaaaeaaaaaadiaaaaahocaabaaaabaaaaaafgafbaaaabaaaaaaagajbaaa
acaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaaaeb
diaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgapbaaaacaaaaaaefaaaaaj
pcaabaaaadaaaaaaegbabaaaaeaaaaaaeghobaaaafaaaaaaaagabaaaafaaaaaa
diaaaaahicaabaaaacaaaaaadkaabaaaadaaaaaaabeaaaaaaaaaaaebdiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaapgapbaaaacaaaaaabaaaaaakicaabaaa
acaaaaaaaceaaaaadkmnbddpdkmnbddpdkmnbddpaaaaaaaaegacbaaaadaaaaaa
diaaaaahhcaabaaaacaaaaaapgapbaaaacaaaaaaegacbaaaacaaaaaaddaaaaah
ocaabaaaabaaaaaafgaobaaaabaaaaaaagajbaaaacaaaaaadiaaaaahhcaabaaa
aeaaaaaaagaabaaaabaaaaaaegacbaaaacaaaaaadiaaaaaihcaabaaaacaaaaaa
egacbaaaacaaaaaaegiccaaaaaaaaaaaacaaaaaadeaaaaahhcaabaaaabaaaaaa
jgahbaaaabaaaaaaegacbaaaaeaaaaaadiaaaaahhcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaabaaaaaadiaaaaakhcaabaaaabaaaaaafgafbaaaadaaaaaa
aceaaaaaomafnblopdaedfdpdkmnbddpaaaaaaaadcaaaaamhcaabaaaabaaaaaa
agaabaaaadaaaaaaaceaaaaaolaffbdpaaaaaaaadkmnbddpaaaaaaaaegacbaaa
abaaaaaadcaaaaamhcaabaaaabaaaaaakgakbaaaadaaaaaaaceaaaaaolafnblo
pdaedflpdkmnbddpaaaaaaaaegacbaaaabaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaabaaaaaahicaabaaaacaaaaaaegbcbaaaabaaaaaaegbcbaaaabaaaaaa
eeaaaaaficaabaaaacaaaaaadkaabaaaacaaaaaadiaaaaahhcaabaaaadaaaaaa
pgapbaaaacaaaaaaegbcbaaaabaaaaaadcaaaaakicaabaaaacaaaaaackbabaia
ebaaaaaaabaaaaaadkaabaaaacaaaaaaabeaaaaaaaaaiadpdiaaaaaiicaabaaa
acaaaaaadkaabaaaacaaaaaabkiacaaaaaaaaaaabaaaaaaacpaaaaaficaabaaa
acaaaaaadkaabaaaacaaaaaadiaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaa
ckiacaaaaaaaaaaabaaaaaaabjaaaaaficaabaaaacaaaaaadkaabaaaacaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaabaaaaaapgapbaaaabaaaaaaegacbaaa
adaaaaaabaaaaaahbcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahbcaabaaaabaaaaaa
akaabaaaabaaaaaackaabaaaabaaaaaadeaaaaahbcaabaaaabaaaaaaakaabaaa
abaaaaaaabeaaaaaaaaaaaaacpaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaa
diaaaaaiccaabaaaabaaaaaaakiacaaaaaaaaaaaamaaaaaaabeaaaaaaaaaaaed
diaaaaahbcaabaaaabaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaabjaaaaaf
bcaabaaaabaaaaaaakaabaaaabaaaaaaaaaaaaajccaabaaaabaaaaaaakiacaia
ebaaaaaaaaaaaaaabgaaaaaaabeaaaaaaaaaiadpdiaaaaaiecaabaaaabaaaaaa
bkaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahhcaabaaaacaaaaaa
kgakbaaaabaaaaaaegacbaaaacaaaaaadcaaaaajhccabaaaaaaaaaaaegacbaaa
acaaaaaaagaabaaaabaaaaaaegacbaaaaaaaaaaaaaaaaaajbcaabaaaaaaaaaaa
akiacaiaebaaaaaaaaaaaaaabaaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaa
aaaaaaaadkaabaaaacaaaaaaakaabaaaaaaaaaaaakiacaaaaaaaaaaabaaaaaaa
aaaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaaj
bcaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadp
diaaaaahiccabaaaaaaaaaaabkaabaaaabaaaaaaakaabaaaaaaaaaaadoaaaaab
"
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
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Matrix 17 [_LightMatrix0]
Vector 21 [_WorldSpaceCameraPos]
Vector 22 [_WorldSpaceLightPos0]
Vector 23 [unity_Scale]
Vector 24 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..24] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R1.w, c[0].x;
MOV R1.xyz, c[21];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[23].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
MUL R3.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R3.xyz, vertex.normal.yzxw, R1.zxyw, -R3;
MOV R1, c[22];
MUL R3.xyz, R3, vertex.attrib[14].w;
DP3 R0.w, R2, vertex.normal;
DP3 result.texcoord[4].y, R0, R3;
DP3 result.texcoord[4].z, R0, vertex.normal;
DP3 result.texcoord[4].x, R0, vertex.attrib[14];
DP4 R4.z, R1, c[15];
DP4 R4.x, R1, c[13];
DP4 R4.y, R1, c[14];
MAD R1.xyz, R4, c[23].w, -vertex.position;
DP3 result.texcoord[3].y, R1, R3;
DP3 result.texcoord[3].z, vertex.normal, R1;
DP3 result.texcoord[3].x, R1, vertex.attrib[14];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[0].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
MOV result.texcoord[1], R0;
ABS result.texcoord[0].xyz, R0;
DP3 result.texcoord[2].x, R0, c[24];
DP4 R0.w, vertex.position, c[12];
DP4 R0.z, vertex.position, c[11];
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP4 result.texcoord[5].z, R0, c[19];
DP4 result.texcoord[5].y, R0, c[18];
DP4 result.texcoord[5].x, R0, c[17];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 47 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Matrix 16 [_LightMatrix0]
Vector 20 [_WorldSpaceCameraPos]
Vector 21 [_WorldSpaceLightPos0]
Vector 22 [unity_Scale]
Vector 23 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c24, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c24.x
mov r1.xyz, c20
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r2.xyz, r0, c22.w, -v0
dp3 r0.x, r2, r2
rsq r0.w, r0.x
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r4.xyz, r1, v1.w
mul r3.xyz, r0.w, r2
mov r0, c14
dp4 r5.z, c21, r0
mov r0, c13
mov r1, c12
dp4 r5.x, c21, r1
dp4 r5.y, c21, r0
mad r0.xyz, r5, c22.w, -v0
dp3 r0.w, r3, v2
dp3 o4.y, r0, r4
dp3 o4.z, v2, r0
dp3 o4.x, r0, v1
mov r0.z, v4.x
mov r0.xy, v3
add o1.w, -r0, c24.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
mov o2, r0
abs o1.xyz, r0
dp3 o3.x, r0, c23
dp4 r0.w, v0, c11
dp4 r0.z, v0, c10
dp4 r0.x, v0, c8
dp4 r0.y, v0, c9
dp3 o5.y, r2, r4
dp3 o5.z, r2, v2
dp3 o5.x, r2, v1
dp4 o6.z, r0, c18
dp4 o6.y, r0, c17
dp4 o6.x, r0, c16
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "POINT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 368
Matrix 48 [_LightMatrix0]
Vector 336 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedmjjamcgnclbpaponefpcjpeflileiljhabaaaaaabiajaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaapaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
abaoaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoabaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcemahaaaaeaaaabaandabaaaafjaaaaaeegiocaaaaaaaaaaa
bgaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
abaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadpccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadbccabaaa
adaaaaaagfaaaaadoccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
hccabaaaafaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadiaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaa
aaaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaa
aaaaaaaaaaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaadaaaaaa
bdaaaaaadcaaaaalhcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaadaaaaaa
beaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaabaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
abaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdgaaaaafdcaabaaa
abaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaadgaaaaaghccabaaaabaaaaaaegacbaiaibaaaaaa
abaaaaaadgaaaaafhccabaaaacaaaaaaegacbaaaabaaaaaabaaaaaaibccabaaa
adaaaaaaegiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakicaabaaa
aaaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaa
dkbabaaaaaaaaaaadkaabaaaaaaaaaaadgaaaaagiccabaaaacaaaaaadkaabaia
ebaaaaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaa
acaaaaaadcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaa
egacbaiaebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgbpbaaaabaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaacaaaaaaaaaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaacaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaa
acaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaa
acaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaa
acaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaaegacbaaa
acaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
eccabaaaadaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaabaaaaaahcccabaaa
aeaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaahcccabaaaadaaaaaa
egbcbaaaabaaaaaaegacbaaaacaaaaaabaaaaaahiccabaaaadaaaaaaegbcbaaa
acaaaaaaegacbaaaacaaaaaabaaaaaahbccabaaaaeaaaaaaegbcbaaaabaaaaaa
egacbaaaaaaaaaaabaaaaaaheccabaaaaeaaaaaaegbcbaaaacaaaaaaegacbaaa
aaaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaadaaaaaa
anaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaamaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaa
aoaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaapaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaai
hcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaaaaaaaaaaaeaaaaaadcaaaaak
hcaabaaaabaaaaaaegiccaaaaaaaaaaaadaaaaaaagaabaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaakgakbaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaakhccabaaaafaaaaaaegiccaaaaaaaaaaa
agaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_World2Object]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Vector 15 [unity_Scale]
Vector 16 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[17] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..16] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R1.w, c[0].x;
MOV R1.xyz, c[13];
DP4 R0.z, R1, c[11];
DP4 R0.x, R1, c[9];
DP4 R0.y, R1, c[10];
MAD R0.xyz, R0, c[15].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
MUL R3.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R3.xyz, vertex.normal.yzxw, R1.zxyw, -R3;
MOV R1, c[14];
MUL R3.xyz, R3, vertex.attrib[14].w;
DP3 R0.w, R2, vertex.normal;
DP4 R4.z, R1, c[11];
DP4 R4.y, R1, c[10];
DP4 R4.x, R1, c[9];
DP3 result.texcoord[4].y, R0, R3;
DP3 result.texcoord[4].z, R0, vertex.normal;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[0].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
DP3 result.texcoord[4].x, R0, vertex.attrib[14];
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
DP3 result.texcoord[3].y, R4, R3;
DP3 result.texcoord[3].z, vertex.normal, R4;
DP3 result.texcoord[3].x, R4, vertex.attrib[14];
ABS result.texcoord[0].xyz, R0;
MOV result.texcoord[1], R0;
DP3 result.texcoord[2].x, R0, c[16];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 39 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_World2Object]
Vector 12 [_WorldSpaceCameraPos]
Vector 13 [_WorldSpaceLightPos0]
Vector 14 [unity_Scale]
Vector 15 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
def c16, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c16.x
mov r1.xyz, c12
dp4 r0.z, r1, c10
dp4 r0.x, r1, c8
dp4 r0.y, r1, c9
mad r2.xyz, r0, c14.w, -v0
dp3 r0.x, r2, r2
rsq r0.w, r0.x
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r4.xyz, r1, v1.w
mul r3.xyz, r0.w, r2
mov r0, c10
dp4 r5.z, c13, r0
mov r0, c8
dp4 r5.x, c13, r0
mov r1, c9
dp4 r5.y, c13, r1
dp3 r0.x, r3, v2
add o1.w, -r0.x, c16.x
mov r0.z, v4.x
mov r0.xy, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
dp3 o4.y, r5, r4
dp3 o5.y, r2, r4
dp3 o4.z, v2, r5
dp3 o4.x, r5, v1
dp3 o5.z, r2, v2
dp3 o5.x, r2, v1
abs o1.xyz, r0
mov o2, r0
dp3 o3.x, r0, c15
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 304
Vector 272 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedffjagfgmlojlkmjhjkgcjlopcglahnkfabaaaaaajiahaaaaadaaaaaa
cmaaaaaapeaaaaaakmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheolaaaaaaaagaaaaaa
aiaaaaaajiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaakeaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaapaaaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
abaoaaaakeaaaaaaadaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoabaaaakeaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklfdeieefcoeafaaaaeaaaabaahjabaaaafjaaaaae
egiocaaaaaaaaaaabcaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaae
egiocaaaacaaaaaaabaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaa
fpaaaaaddcbabaaaadaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaadpccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaa
gfaaaaadbccabaaaadaaaaaagfaaaaadoccabaaaadaaaaaagfaaaaadhccabaaa
aeaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaa
egiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaa
aaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaa
aaaaaaaadiaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaa
adaaaaaabbaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaabaaaaaaa
agiacaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaaaaaaaaaa
egiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaa
aaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaadaaaaaabdaaaaaa
dcaaaaalhcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaadaaaaaabeaaaaaa
egbcbaiaebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaah
hcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaahicaabaaa
aaaaaaaaegacbaaaabaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaaabaaaaaa
dkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdgaaaaafdcaabaaaabaaaaaa
egbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaa
aaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaa
egacbaaaabaaaaaadgaaaaaghccabaaaabaaaaaaegacbaiaibaaaaaaabaaaaaa
dgaaaaafhccabaaaacaaaaaaegacbaaaabaaaaaabaaaaaaibccabaaaadaaaaaa
egiccaaaaaaaaaaabbaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaaaaaaaaaa
bkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakicaabaaaaaaaaaaa
ckiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaadkaabaaa
aaaaaaaadcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaa
aaaaaaaadkaabaaaaaaaaaaadgaaaaagiccabaaaacaaaaaadkaabaiaebaaaaaa
aaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaa
dcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaia
ebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgbpbaaa
abaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaacaaaaaaaaaaaaaaegiccaaa
adaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabaaaaaaa
agiacaaaacaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaa
egiccaaaadaaaaaabcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaaacaaaaaa
dcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaaacaaaaaa
aaaaaaaaegacbaaaacaaaaaabaaaaaaheccabaaaadaaaaaaegacbaaaabaaaaaa
egacbaaaacaaaaaabaaaaaahcccabaaaaeaaaaaaegacbaaaabaaaaaaegacbaaa
aaaaaaaabaaaaaahcccabaaaadaaaaaaegbcbaaaabaaaaaaegacbaaaacaaaaaa
baaaaaahiccabaaaadaaaaaaegbcbaaaacaaaaaaegacbaaaacaaaaaabaaaaaah
bccabaaaaeaaaaaaegbcbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaaheccabaaa
aeaaaaaaegbcbaaaacaaaaaaegacbaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Matrix 17 [_LightMatrix0]
Vector 21 [_WorldSpaceCameraPos]
Vector 22 [_WorldSpaceLightPos0]
Vector 23 [unity_Scale]
Vector 24 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..24] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R1.w, c[0].x;
MOV R1.xyz, c[21];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[23].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
MUL R3.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R3.xyz, vertex.normal.yzxw, R1.zxyw, -R3;
MOV R1, c[22];
MUL R3.xyz, R3, vertex.attrib[14].w;
DP3 R0.w, R2, vertex.normal;
DP3 result.texcoord[4].y, R0, R3;
DP3 result.texcoord[4].z, R0, vertex.normal;
DP3 result.texcoord[4].x, R0, vertex.attrib[14];
DP4 R4.z, R1, c[15];
DP4 R4.x, R1, c[13];
DP4 R4.y, R1, c[14];
MAD R1.xyz, R4, c[23].w, -vertex.position;
DP3 result.texcoord[3].y, R1, R3;
DP3 result.texcoord[3].z, vertex.normal, R1;
DP3 result.texcoord[3].x, R1, vertex.attrib[14];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[0].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
MOV result.texcoord[1], R0;
DP4 R0.w, vertex.position, c[12];
ABS result.texcoord[0].xyz, R0;
DP3 result.texcoord[2].x, R0, c[24];
DP4 R0.z, vertex.position, c[11];
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP4 result.texcoord[5].w, R0, c[20];
DP4 result.texcoord[5].z, R0, c[19];
DP4 result.texcoord[5].y, R0, c[18];
DP4 result.texcoord[5].x, R0, c[17];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 48 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Matrix 16 [_LightMatrix0]
Vector 20 [_WorldSpaceCameraPos]
Vector 21 [_WorldSpaceLightPos0]
Vector 22 [unity_Scale]
Vector 23 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c24, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c24.x
mov r1.xyz, c20
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r2.xyz, r0, c22.w, -v0
dp3 r0.x, r2, r2
rsq r0.w, r0.x
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r4.xyz, r1, v1.w
mul r3.xyz, r0.w, r2
mov r0, c14
dp4 r5.z, c21, r0
mov r0, c13
mov r1, c12
dp4 r5.x, c21, r1
dp4 r5.y, c21, r0
mad r0.xyz, r5, c22.w, -v0
dp3 r0.w, r3, v2
dp3 o4.y, r0, r4
dp3 o4.z, v2, r0
dp3 o4.x, r0, v1
mov r0.z, v4.x
mov r0.xy, v3
add o1.w, -r0, c24.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
mov o2, r0
dp4 r0.w, v0, c11
abs o1.xyz, r0
dp3 o3.x, r0, c23
dp4 r0.z, v0, c10
dp4 r0.x, v0, c8
dp4 r0.y, v0, c9
dp3 o5.y, r2, r4
dp3 o5.z, r2, v2
dp3 o5.x, r2, v1
dp4 o6.w, r0, c19
dp4 o6.z, r0, c18
dp4 o6.y, r0, c17
dp4 o6.x, r0, c16
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 368
Matrix 48 [_LightMatrix0]
Vector 336 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedilfkohdnipdmodbnplbggfgennjgenmkabaaaaaabiajaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaapaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
abaoaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoabaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcemahaaaaeaaaabaandabaaaafjaaaaaeegiocaaaaaaaaaaa
bgaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
abaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadpccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadbccabaaa
adaaaaaagfaaaaadoccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
pccabaaaafaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadiaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaa
aaaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaa
aaaaaaaaaaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaadaaaaaa
bdaaaaaadcaaaaalhcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaadaaaaaa
beaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaabaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
abaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdgaaaaafdcaabaaa
abaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaadgaaaaaghccabaaaabaaaaaaegacbaiaibaaaaaa
abaaaaaadgaaaaafhccabaaaacaaaaaaegacbaaaabaaaaaabaaaaaaibccabaaa
adaaaaaaegiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakicaabaaa
aaaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaa
dkbabaaaaaaaaaaadkaabaaaaaaaaaaadgaaaaagiccabaaaacaaaaaadkaabaia
ebaaaaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaa
acaaaaaadcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaa
egacbaiaebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgbpbaaaabaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaacaaaaaaaaaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaacaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaa
acaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaa
acaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaa
acaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaaegacbaaa
acaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
eccabaaaadaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaabaaaaaahcccabaaa
aeaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaahcccabaaaadaaaaaa
egbcbaaaabaaaaaaegacbaaaacaaaaaabaaaaaahiccabaaaadaaaaaaegbcbaaa
acaaaaaaegacbaaaacaaaaaabaaaaaahbccabaaaaeaaaaaaegbcbaaaabaaaaaa
egacbaaaaaaaaaaabaaaaaaheccabaaaaeaaaaaaegbcbaaaacaaaaaaegacbaaa
aaaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaadaaaaaa
anaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaamaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaa
aoaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaapaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaai
pcaabaaaabaaaaaafgafbaaaaaaaaaaaegiocaaaaaaaaaaaaeaaaaaadcaaaaak
pcaabaaaabaaaaaaegiocaaaaaaaaaaaadaaaaaaagaabaaaaaaaaaaaegaobaaa
abaaaaaadcaaaaakpcaabaaaabaaaaaaegiocaaaaaaaaaaaafaaaaaakgakbaaa
aaaaaaaaegaobaaaabaaaaaadcaaaaakpccabaaaafaaaaaaegiocaaaaaaaaaaa
agaaaaaapgapbaaaaaaaaaaaegaobaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Matrix 17 [_LightMatrix0]
Vector 21 [_WorldSpaceCameraPos]
Vector 22 [_WorldSpaceLightPos0]
Vector 23 [unity_Scale]
Vector 24 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..24] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R1.w, c[0].x;
MOV R1.xyz, c[21];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[23].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
MUL R3.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R3.xyz, vertex.normal.yzxw, R1.zxyw, -R3;
MOV R1, c[22];
MUL R3.xyz, R3, vertex.attrib[14].w;
DP3 R0.w, R2, vertex.normal;
DP3 result.texcoord[4].y, R0, R3;
DP3 result.texcoord[4].z, R0, vertex.normal;
DP3 result.texcoord[4].x, R0, vertex.attrib[14];
DP4 R4.z, R1, c[15];
DP4 R4.x, R1, c[13];
DP4 R4.y, R1, c[14];
MAD R1.xyz, R4, c[23].w, -vertex.position;
DP3 result.texcoord[3].y, R1, R3;
DP3 result.texcoord[3].z, vertex.normal, R1;
DP3 result.texcoord[3].x, R1, vertex.attrib[14];
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[0].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
MOV result.texcoord[1], R0;
ABS result.texcoord[0].xyz, R0;
DP3 result.texcoord[2].x, R0, c[24];
DP4 R0.w, vertex.position, c[12];
DP4 R0.z, vertex.position, c[11];
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP4 result.texcoord[5].z, R0, c[19];
DP4 result.texcoord[5].y, R0, c[18];
DP4 result.texcoord[5].x, R0, c[17];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 47 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Matrix 16 [_LightMatrix0]
Vector 20 [_WorldSpaceCameraPos]
Vector 21 [_WorldSpaceLightPos0]
Vector 22 [unity_Scale]
Vector 23 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c24, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c24.x
mov r1.xyz, c20
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r2.xyz, r0, c22.w, -v0
dp3 r0.x, r2, r2
rsq r0.w, r0.x
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r4.xyz, r1, v1.w
mul r3.xyz, r0.w, r2
mov r0, c14
dp4 r5.z, c21, r0
mov r0, c13
mov r1, c12
dp4 r5.x, c21, r1
dp4 r5.y, c21, r0
mad r0.xyz, r5, c22.w, -v0
dp3 r0.w, r3, v2
dp3 o4.y, r0, r4
dp3 o4.z, v2, r0
dp3 o4.x, r0, v1
mov r0.z, v4.x
mov r0.xy, v3
add o1.w, -r0, c24.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
mov o2, r0
abs o1.xyz, r0
dp3 o3.x, r0, c23
dp4 r0.w, v0, c11
dp4 r0.z, v0, c10
dp4 r0.x, v0, c8
dp4 r0.y, v0, c9
dp3 o5.y, r2, r4
dp3 o5.z, r2, v2
dp3 o5.x, r2, v1
dp4 o6.z, r0, c18
dp4 o6.y, r0, c17
dp4 o6.x, r0, c16
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 368
Matrix 48 [_LightMatrix0]
Vector 336 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedmjjamcgnclbpaponefpcjpeflileiljhabaaaaaabiajaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaapaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
abaoaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoabaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcemahaaaaeaaaabaandabaaaafjaaaaaeegiocaaaaaaaaaaa
bgaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
abaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadpccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadbccabaaa
adaaaaaagfaaaaadoccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
hccabaaaafaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadiaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaa
aaaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaa
aaaaaaaaaaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaadaaaaaa
bdaaaaaadcaaaaalhcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaadaaaaaa
beaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaabaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
abaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdgaaaaafdcaabaaa
abaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaadgaaaaaghccabaaaabaaaaaaegacbaiaibaaaaaa
abaaaaaadgaaaaafhccabaaaacaaaaaaegacbaaaabaaaaaabaaaaaaibccabaaa
adaaaaaaegiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakicaabaaa
aaaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaa
dkbabaaaaaaaaaaadkaabaaaaaaaaaaadgaaaaagiccabaaaacaaaaaadkaabaia
ebaaaaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaa
acaaaaaadcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaa
egacbaiaebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgbpbaaaabaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaacaaaaaaaaaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaacaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaa
acaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaa
acaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaa
acaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaaegacbaaa
acaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
eccabaaaadaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaabaaaaaahcccabaaa
aeaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaahcccabaaaadaaaaaa
egbcbaaaabaaaaaaegacbaaaacaaaaaabaaaaaahiccabaaaadaaaaaaegbcbaaa
acaaaaaaegacbaaaacaaaaaabaaaaaahbccabaaaaeaaaaaaegbcbaaaabaaaaaa
egacbaaaaaaaaaaabaaaaaaheccabaaaaeaaaaaaegbcbaaaacaaaaaaegacbaaa
aaaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaadaaaaaa
anaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaamaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaa
aoaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaapaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaai
hcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaaaaaaaaaaaeaaaaaadcaaaaak
hcaabaaaabaaaaaaegiccaaaaaaaaaaaadaaaaaaagaabaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaakgakbaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaakhccabaaaafaaaaaaegiccaaaaaaaaaaa
agaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Matrix 17 [_LightMatrix0]
Vector 21 [_WorldSpaceCameraPos]
Vector 22 [_WorldSpaceLightPos0]
Vector 23 [unity_Scale]
Vector 24 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[25] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..24] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R1.w, c[0].x;
MOV R1.xyz, c[21];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[23].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
MUL R3.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R3.xyz, vertex.normal.yzxw, R1.zxyw, -R3;
MOV R1, c[22];
MUL R3.xyz, R3, vertex.attrib[14].w;
DP3 R0.w, R2, vertex.normal;
DP4 R4.z, R1, c[15];
DP4 R4.y, R1, c[14];
DP4 R4.x, R1, c[13];
DP3 result.texcoord[4].y, R0, R3;
DP3 result.texcoord[4].z, R0, vertex.normal;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[0].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
DP3 result.texcoord[4].x, R0, vertex.attrib[14];
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
MOV result.texcoord[1], R0;
ABS result.texcoord[0].xyz, R0;
DP3 result.texcoord[2].x, R0, c[24];
DP4 R0.w, vertex.position, c[12];
DP4 R0.z, vertex.position, c[11];
DP4 R0.x, vertex.position, c[9];
DP4 R0.y, vertex.position, c[10];
DP3 result.texcoord[3].y, R4, R3;
DP3 result.texcoord[3].z, vertex.normal, R4;
DP3 result.texcoord[3].x, R4, vertex.attrib[14];
DP4 result.texcoord[5].y, R0, c[18];
DP4 result.texcoord[5].x, R0, c[17];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 45 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Matrix 16 [_LightMatrix0]
Vector 20 [_WorldSpaceCameraPos]
Vector 21 [_WorldSpaceLightPos0]
Vector 22 [unity_Scale]
Vector 23 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c24, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c24.x
mov r1.xyz, c20
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r2.xyz, r0, c22.w, -v0
dp3 r0.x, r2, r2
rsq r0.w, r0.x
mov r0.xyz, v1
mul r1.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r1.xyz, v2.yzxw, r0.zxyw, -r1
mul r4.xyz, r1, v1.w
mul r3.xyz, r0.w, r2
mov r0, c14
dp4 r5.z, c21, r0
mov r0, c12
dp4 r5.x, c21, r0
mov r1, c13
dp4 r5.y, c21, r1
dp3 r0.x, r3, v2
add o1.w, -r0.x, c24.x
mov r0.z, v4.x
mov r0.xy, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
mov o2, r0
abs o1.xyz, r0
dp3 o3.x, r0, c23
dp4 r0.w, v0, c11
dp4 r0.z, v0, c10
dp4 r0.x, v0, c8
dp4 r0.y, v0, c9
dp3 o4.y, r5, r4
dp3 o5.y, r2, r4
dp3 o4.z, v2, r5
dp3 o4.x, r5, v1
dp3 o5.z, r2, v2
dp3 o5.x, r2, v1
dp4 o6.y, r0, c17
dp4 o6.x, r0, c16
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 368
Matrix 48 [_LightMatrix0]
Vector 336 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityLighting" 720
Vector 0 [_WorldSpaceLightPos0]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedcacfmifkgjaeibehblejafdiajkbdmemabaaaaaaomaiaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaapaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
abaoaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoabaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefccaahaaaaeaaaabaamiabaaaafjaaaaaeegiocaaaaaaaaaaa
bgaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
abaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadpccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadbccabaaa
adaaaaaagfaaaaadoccabaaaadaaaaaagfaaaaadhccabaaaaeaaaaaagfaaaaad
dccabaaaafaaaaaagiaaaaacadaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpccabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadiaaaaajhcaabaaaaaaaaaaafgifcaaaabaaaaaaaeaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaadcaaaaalhcaabaaa
aaaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaa
aaaaaaaaaaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaadaaaaaa
bdaaaaaadcaaaaalhcaabaaaaaaaaaaaegacbaaaaaaaaaaapgipcaaaadaaaaaa
beaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaabaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
abaaaaaadkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdgaaaaafdcaabaaa
abaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaa
aaaaaaaaegacbaaaabaaaaaadgaaaaaghccabaaaabaaaaaaegacbaiaibaaaaaa
abaaaaaadgaaaaafhccabaaaacaaaaaaegacbaaaabaaaaaabaaaaaaibccabaaa
adaaaaaaegiccaaaaaaaaaaabfaaaaaaegacbaaaabaaaaaadiaaaaaiicaabaaa
aaaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakicaabaaa
aaaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaa
dkaabaaaaaaaaaaadcaaaaakicaabaaaaaaaaaaackiacaaaadaaaaaaahaaaaaa
dkbabaaaaaaaaaaadkaabaaaaaaaaaaadgaaaaagiccabaaaacaaaaaadkaabaia
ebaaaaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaajgbebaaaabaaaaaacgbjbaaa
acaaaaaadcaaaaakhcaabaaaabaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaa
egacbaiaebaaaaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaa
pgbpbaaaabaaaaaadiaaaaajhcaabaaaacaaaaaafgifcaaaacaaaaaaaaaaaaaa
egiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaa
baaaaaaaagiacaaaacaaaaaaaaaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaa
acaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaacaaaaaaaaaaaaaaegacbaaa
acaaaaaadcaaaaalhcaabaaaacaaaaaaegiccaaaadaaaaaabdaaaaaapgipcaaa
acaaaaaaaaaaaaaaegacbaaaacaaaaaabaaaaaaheccabaaaadaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaabaaaaaahcccabaaaaeaaaaaaegacbaaaabaaaaaa
egacbaaaaaaaaaaabaaaaaahcccabaaaadaaaaaaegbcbaaaabaaaaaaegacbaaa
acaaaaaabaaaaaahiccabaaaadaaaaaaegbcbaaaacaaaaaaegacbaaaacaaaaaa
baaaaaahbccabaaaaeaaaaaaegbcbaaaabaaaaaaegacbaaaaaaaaaaabaaaaaah
eccabaaaaeaaaaaaegbcbaaaacaaaaaaegacbaaaaaaaaaaadiaaaaaipcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiocaaaadaaaaaaanaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaamaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaaidcaabaaaabaaaaaafgafbaaa
aaaaaaaaegiacaaaaaaaaaaaaeaaaaaadcaaaaakdcaabaaaaaaaaaaaegiacaaa
aaaaaaaaadaaaaaaagaabaaaaaaaaaaaegaabaaaabaaaaaadcaaaaakdcaabaaa
aaaaaaaaegiacaaaaaaaaaaaafaaaaaakgakbaaaaaaaaaaaegaabaaaaaaaaaaa
dcaaaaakdccabaaaafaaaaaaegiacaaaaaaaaaaaagaaaaaapgapbaaaaaaaaaaa
egaabaaaaaaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Keywords { "POINT" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTexture0] 2D 3
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 0, 128, 1, 2.718282 },
		{ 0.5, 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[1], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[12].x;
DP3 R1.w, fragment.texcoord[4], fragment.texcoord[4];
MAD R2.xyz, fragment.texcoord[0].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[0].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[3].x, R2;
ADD R0.w, -R0, c[13].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[5];
ADD R0.w, fragment.texcoord[1], -c[12].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.y, -R0.w, c[20], c[20].z;
MUL R1.x, R0.w, R0.w;
MUL R0.w, fragment.texcoord[1].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
MAD R1.x, -R1, R1.y, c[19].z;
ADD R0.xyz, R0, -c[6];
MAD R0.xyz, R1.x, R0, c[6];
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].z;
MAD R1.xyz, R0.w, R1, R0;
DP3 R0.w, fragment.texcoord[3], fragment.texcoord[3];
RSQ R1.w, R1.w;
MOV R0.y, c[20].x;
MOV R0.x, fragment.texcoord[2];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, fragment.texcoord[3];
MAD R3.xyz, R1.w, fragment.texcoord[4], R0;
MUL R0.w, fragment.texcoord[1], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
ADD R0.x, -R0.w, c[19].z;
MAD R1.xyz, R0.x, R2, R1;
DP3 R0.x, R3, R3;
RSQ R0.x, R0.x;
MUL R1.xyz, R1, c[1];
MUL R0.w, R0.x, R3.z;
MAX R0.y, R0.z, c[19].x;
MUL R0.xyz, R1, R0.y;
MAX R1.y, R0.w, c[19].x;
MOV R1.x, c[19].z;
MOV R0.w, c[19].y;
ADD R1.x, R1, -c[18];
MUL R0.w, R0, c[7].x;
POW R0.w, R1.y, R0.w;
MUL R1.x, R1, c[8];
MUL R1.w, R0, R1.x;
DP3 R0.w, fragment.texcoord[5], fragment.texcoord[5];
MOV R1.xyz, c[2];
TEX R0.w, R0.w, texture[3], 2D;
MUL R1.xyz, R1, c[1];
MUL R0.w, R0, c[20].y;
MAD R0.xyz, R1, R1.w, R0;
MUL result.color.xyz, R0, R0.w;
MOV result.color.w, c[19].x;
END
# 71 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_fadeStart]
Float 10 [_fadeEnd]
Float 11 [_tiling]
Vector 12 [_fogColor]
Float 13 [_heightDensityAtViewer]
Float 14 [_globalDensity]
Float 15 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTexture0] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c16, 0.00000000, 128.00000000, 1.00000000, 2.71828198
def c17, 0.50000000, 2.00000000, 3.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1
dcl_texcoord2 v2.x
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
mov r0.x, c4
mul r0.x, c0.w, r0
mad r0.xyz, v1, c11.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v0.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c10.x
mad r3.xyz, v0.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v0.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v0.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c3.x, r3
add r0.w, -c9.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c5
add r0.w, v1, -c9.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c17.y, c17.z
mul r0.w, r0, r0
add r0.xyz, r0, -c6
mad r0.w, -r0, r1.x, c16.z
mad r1.xyz, r0.w, r0, c6
mul r0.x, v1, c14
mul r2.w, r0.x, c13.x
pow r0, c16.w, r2.w
mul r1.w, v1, c14.x
mul r0.y, r1.w, c13.x
pow r3, c16.w, r0.y
mov r0.w, r3.x
add r1.w, -r0, c16.z
add r2.xyz, -r1, c12
add r0.x, -r0, c16.z
mad r1.xyz, r0.x, r2, r1
dp3_pp r0.w, v3, v3
mov r0.y, c17.x
mov r0.x, v2
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r1.w, r0, r1
rsq_pp r0.y, r0.w
dp3_pp r0.x, v4, v4
mul_pp r2.xyz, r0.y, v3
rsq_pp r0.x, r0.x
mad_pp r0.xyz, r0.x, v4, r2
dp3_pp r0.x, r0, r0
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r0.y, r2.z, c16.x
mul_pp r1.xyz, r1, c1
mul_pp r1.xyz, r1, r0.y
mov_pp r0.y, c7.x
mul_pp r2.x, c16.y, r0.y
max_pp r1.w, r0.x, c16.x
pow r0, r1.w, r2.x
mov r2.y, c15.x
add r0.y, c16.z, -r2
mul r0.y, r0, c8.x
mul r0.y, r0.x, r0
dp3 r0.x, v5, v5
texld r0.x, r0.x, s3
mov_pp r2.xyz, c1
mul_pp r0.w, r0.x, c17.y
mul_pp r2.xyz, c2, r2
mad r0.xyz, r2, r0.y, r1
mul oC0.xyz, r0, r0.w
mov_pp oC0.w, c16.x
"
}
SubProgram "d3d11 " {
Keywords { "POINT" }
SetTexture 0 [_WaterTex] 2D 1
SetTexture 1 [_WaterTex1] 2D 2
SetTexture 2 [_fogColorRamp] 2D 3
SetTexture 3 [_LightTexture0] 2D 0
ConstBuffer "$Globals" 368
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 192 [_Shininess]
Float 196 [_Gloss]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedlelighepnfgbhdajiahncnbiebmidgfkabaaaaaacmakaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaababaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoaoaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcpeaiaaaaeaaaaaaadnacaaaa
fjaaaaaeegiocaaaaaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaa
fibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaa
fibiaaaeaahabaaaadaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaad
pcbabaaaacaaaaaagcbaaaadbcbabaaaadaaaaaagcbaaaadocbabaaaadaaaaaa
gcbaaaadhcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagfaaaaadpccabaaa
aaaaaaaagiaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaa
ahaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
acaaaaaafgifcaaaaaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaa
abaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadiaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaabaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaabaaaaaa
egacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaabaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaabaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaafgbfbaaaabaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaagiacaaaaaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaajaaaaaa
egiccaiaebaaaaaaaaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaia
ebaaaaaaaaaaaaaabaaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaakicaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaa
aaaaaaajbcaabaaaabaaaaaadkbabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaa
baaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaaacaaaaaa
mgbabaaaacaaaaaaagiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaaacaaaaaa
egaabaaaacaaaaaafgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaaacaaaaaa
egaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaaf
dcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaia
ebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaaj
hcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaafbcaabaaaabaaaaaaakbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaa
abeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
acaaaaaaaagabaaaadaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaaegbcbaaa
aeaaaaaaegbcbaaaaeaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
baaaaaahbcaabaaaabaaaaaajgbhbaaaadaaaaaajgbhbaaaadaaaaaaeeaaaaaf
bcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaagaabaaa
abaaaaaajgbhbaaaadaaaaaadcaaaaajlcaabaaaabaaaaaaegbibaaaaeaaaaaa
pgapbaaaaaaaaaaaegaibaaaabaaaaaadeaaaaahicaabaaaaaaaaaaackaabaaa
abaaaaaaabeaaaaaaaaaaaaabaaaaaahbcaabaaaabaaaaaaegadbaaaabaaaaaa
egadbaaaabaaaaaaeeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaah
bcaabaaaabaaaaaaakaabaaaabaaaaaadkaabaaaabaaaaaadeaaaaahbcaabaaa
abaaaaaaakaabaaaabaaaaaaabeaaaaaaaaaaaaacpaaaaafbcaabaaaabaaaaaa
akaabaaaabaaaaaadiaaaaaiccaabaaaabaaaaaaakiacaaaaaaaaaaaamaaaaaa
abeaaaaaaaaaaaeddiaaaaahbcaabaaaabaaaaaaakaabaaaabaaaaaabkaabaaa
abaaaaaabjaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaaaaaaaaajccaabaaa
abaaaaaaakiacaiaebaaaaaaaaaaaaaabgaaaaaaabeaaaaaaaaaiadpdiaaaaai
ccaabaaaabaaaaaabkaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaah
bcaabaaaabaaaaaabkaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaajocaabaaa
abaaaaaaagijcaaaaaaaaaaaabaaaaaaagijcaaaaaaaaaaaacaaaaaadiaaaaah
hcaabaaaabaaaaaaagaabaaaabaaaaaajgahbaaaabaaaaaadcaaaaajhcaabaaa
aaaaaaaaegacbaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaah
icaabaaaaaaaaaaaegbcbaaaafaaaaaaegbcbaaaafaaaaaaefaaaaajpcaabaaa
abaaaaaapgapbaaaaaaaaaaaeghobaaaadaaaaaaaagabaaaaaaaaaaaaaaaaaah
icaabaaaaaaaaaaaakaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahhccabaaa
aaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaa
abeaaaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 0, 128, 1, 2.718282 },
		{ 0.5, 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[1], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[12].x;
DP3 R1.w, fragment.texcoord[4], fragment.texcoord[4];
MAD R2.xyz, fragment.texcoord[0].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[0].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[3].x, R2;
ADD R0.w, -R0, c[13].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[5];
ADD R0.w, fragment.texcoord[1], -c[12].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.y, -R0.w, c[20], c[20].z;
MUL R1.x, R0.w, R0.w;
MUL R0.w, fragment.texcoord[1].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
MAD R1.x, -R1, R1.y, c[19].z;
ADD R0.xyz, R0, -c[6];
MAD R0.xyz, R1.x, R0, c[6];
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].z;
MAD R1.xyz, R0.w, R1, R0;
MUL R0.w, fragment.texcoord[1], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
RSQ R1.w, R1.w;
ADD R0.w, -R0, c[19].z;
MOV R0.y, c[20].x;
MOV R0.x, fragment.texcoord[2];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
MAD R1.xyz, R0.w, R2, R1;
MOV R0.xyz, fragment.texcoord[3];
MAD R0.xyz, R1.w, fragment.texcoord[4], R0;
DP3 R0.x, R0, R0;
RSQ R0.x, R0.x;
MUL R1.xyz, R1, c[1];
MUL R0.w, R0.x, R0.z;
MAX R0.y, fragment.texcoord[3].z, c[19].x;
MUL R0.xyz, R1, R0.y;
MAX R1.y, R0.w, c[19].x;
MOV R0.w, c[19].y;
MOV R1.x, c[19].z;
ADD R1.x, R1, -c[18];
MUL R0.w, R0, c[7].x;
POW R0.w, R1.y, R0.w;
MUL R1.w, R1.x, c[8].x;
MOV R1.xyz, c[2];
MUL R0.w, R0, R1;
MUL R1.xyz, R1, c[1];
MAD R0.xyz, R1, R0.w, R0;
MUL result.color.xyz, R0, c[20].y;
MOV result.color.w, c[19].x;
END
# 66 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_fadeStart]
Float 10 [_fadeEnd]
Float 11 [_tiling]
Vector 12 [_fogColor]
Float 13 [_heightDensityAtViewer]
Float 14 [_globalDensity]
Float 15 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c16, 0.00000000, 128.00000000, 1.00000000, 2.71828198
def c17, 0.50000000, 2.00000000, 3.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1
dcl_texcoord2 v2.x
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
mov r0.x, c4
mul r0.x, c0.w, r0
mad r0.xyz, v1, c11.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v0.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c10.x
mad r3.xyz, v0.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v0.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v0.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c3.x, r3
add r0.w, -c9.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c5
add r0.w, v1, -c9.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c17.y, c17.z
mul r0.w, r0, r0
add r0.xyz, r0, -c6
mad r0.w, -r0, r1.x, c16.z
mad r1.xyz, r0.w, r0, c6
mul r0.x, v1, c14
mul r2.w, r0.x, c13.x
pow r0, c16.w, r2.w
mul r1.w, v1, c14.x
mul r0.y, r1.w, c13.x
pow r3, c16.w, r0.y
mov r0.w, r3.x
add r2.xyz, -r1, c12
add r0.x, -r0, c16.z
mad r1.xyz, r0.x, r2, r1
add r0.w, -r0, c16.z
mov r0.y, c17.x
mov r0.x, v2
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r0.xyz, r0.w, r0, r1
mul_pp r1.xyz, r0, c1
dp3_pp r0.w, v4, v4
rsq_pp r0.w, r0.w
mov_pp r0.xyz, v3
mad_pp r0.xyz, r0.w, v4, r0
dp3_pp r0.x, r0, r0
max_pp r0.w, v3.z, c16.x
mov_pp r0.y, c7.x
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r1.w, r0.x, c16.x
mul_pp r1.xyz, r1, r0.w
mul_pp r2.x, c16.y, r0.y
pow r0, r1.w, r2.x
mov r0.y, c15.x
add r0.y, c16.z, -r0
mov r1.w, r0.x
mul r0.w, r0.y, c8.x
mov_pp r0.xyz, c1
mul r0.w, r1, r0
mul_pp r0.xyz, c2, r0
mad r0.xyz, r0, r0.w, r1
mul oC0.xyz, r0, c17.y
mov_pp oC0.w, c16.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
ConstBuffer "$Globals" 304
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 128 [_Shininess]
Float 132 [_Gloss]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedhpobdmhkgnbmnnfgfnchjpfhdailklgoabaaaaaaeeajaaaaadaaaaaa
cmaaaaaaoeaaaaaabiabaaaaejfdeheolaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaakeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapahaaaakeaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaakeaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaababaaaakeaaaaaa
adaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoaoaaaakeaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcceaiaaaa
eaaaaaaaajacaaaafjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaa
abaaaaaaacaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaagcbaaaad
hcbabaaaabaaaaaagcbaaaadpcbabaaaacaaaaaagcbaaaadbcbabaaaadaaaaaa
gcbaaaadocbabaaaadaaaaaagcbaaaadhcbabaaaaeaaaaaagfaaaaadpccabaaa
aaaaaaaagiaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaa
adaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
acaaaaaafgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaa
abaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaabaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaa
abaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaabaaaaaa
egacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaabaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaabaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaafgbfbaaaabaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaagiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaa
egiccaiaebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaia
ebaaaaaaaaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaa
aaaaaaajbcaabaaaabaaaaaadkbabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaa
mgbabaaaacaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaa
egaabaaaacaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaa
egaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaaf
dcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaia
ebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaaj
hcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaafbcaabaaaabaaaaaaakbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaa
abeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
acaaaaaaaagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaaegbcbaaa
aeaaaaaaegbcbaaaaeaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaajhcaabaaaabaaaaaaegbcbaaaaeaaaaaapgapbaaaaaaaaaaajgbhbaaa
adaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaackaabaaaabaaaaaadeaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaaaaacpaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaaibcaabaaaabaaaaaaakiacaaaaaaaaaaaaiaaaaaaabeaaaaaaaaaaaed
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaabjaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaaaaaaaaajbcaabaaaabaaaaaaakiacaia
ebaaaaaaaaaaaaaabcaaaaaaabeaaaaaaaaaiadpdiaaaaaibcaabaaaabaaaaaa
akaabaaaabaaaaaabkiacaaaaaaaaaaaaiaaaaaadiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakaabaaaabaaaaaadiaaaaajhcaabaaaabaaaaaaegiccaaa
aaaaaaaaabaaaaaaegiccaaaaaaaaaaaacaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaaaaaaaaaegacbaaaabaaaaaadeaaaaahicaabaaaaaaaaaaadkbabaaa
adaaaaaaabeaaaaaaaaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaabaaaaaaaaaaaaahhccabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaa
doaaaaab"
}
SubProgram "opengl " {
Keywords { "SPOT" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTexture0] 2D 3
SetTexture 4 [_LightTextureB0] 2D 4
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 0, 128, 1, 2.718282 },
		{ 0.5, 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[1], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[12].x;
DP3 R1.w, fragment.texcoord[4], fragment.texcoord[4];
MAD R2.xyz, fragment.texcoord[0].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[0].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[3].x, R2;
ADD R0.w, -R0, c[13].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[5];
ADD R0.w, fragment.texcoord[1], -c[12].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.y, -R0.w, c[20], c[20].z;
MUL R1.x, R0.w, R0.w;
MUL R0.w, fragment.texcoord[1].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
MAD R1.x, -R1, R1.y, c[19].z;
ADD R0.xyz, R0, -c[6];
MAD R0.xyz, R1.x, R0, c[6];
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].z;
MAD R1.xyz, R0.w, R1, R0;
DP3 R0.w, fragment.texcoord[3], fragment.texcoord[3];
RSQ R1.w, R1.w;
MOV R0.y, c[20].x;
MOV R0.x, fragment.texcoord[2];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, fragment.texcoord[3];
MAD R3.xyz, R1.w, fragment.texcoord[4], R0;
MUL R0.w, fragment.texcoord[1], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
ADD R0.x, -R0.w, c[19].z;
MAD R1.xyz, R0.x, R2, R1;
RCP R0.w, fragment.texcoord[5].w;
MAD R2.xy, fragment.texcoord[5], R0.w, c[20].x;
TEX R0.w, R2, texture[3], 2D;
DP3 R0.x, R3, R3;
RSQ R0.x, R0.x;
DP3 R1.w, fragment.texcoord[5], fragment.texcoord[5];
SLT R2.x, c[19], fragment.texcoord[5].z;
MAX R0.y, R0.z, c[19].x;
MUL R0.x, R0, R3.z;
MAX R0.z, R0.x, c[19].x;
MUL R1.xyz, R1, c[1];
MUL R1.xyz, R1, R0.y;
MOV R0.y, c[19].z;
MOV R0.x, c[19].y;
ADD R0.y, R0, -c[18].x;
MUL R0.x, R0, c[7];
MUL R0.y, R0, c[8].x;
POW R0.x, R0.z, R0.x;
MUL R2.z, R0.x, R0.y;
MOV R0.xyz, c[2];
MUL R0.xyz, R0, c[1];
TEX R1.w, R1.w, texture[4], 2D;
MUL R0.w, R2.x, R0;
MUL R0.w, R0, R1;
MUL R0.w, R0, c[20].y;
MAD R0.xyz, R0, R2.z, R1;
MUL result.color.xyz, R0, R0.w;
MOV result.color.w, c[19].x;
END
# 77 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "SPOT" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_fadeStart]
Float 10 [_fadeEnd]
Float 11 [_tiling]
Vector 12 [_fogColor]
Float 13 [_heightDensityAtViewer]
Float 14 [_globalDensity]
Float 15 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTexture0] 2D 3
SetTexture 4 [_LightTextureB0] 2D 4
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c16, 0.00000000, 128.00000000, 1.00000000, 2.71828198
def c17, 0.50000000, 2.00000000, 3.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1
dcl_texcoord2 v2.x
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5
mov r0.x, c4
mul r0.x, c0.w, r0
mad r0.xyz, v1, c11.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v0.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c10.x
mad r3.xyz, v0.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v0.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v0.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c3.x, r3
add r0.w, -c9.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c5
add r0.w, v1, -c9.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c17.y, c17.z
mul r0.w, r0, r0
add r0.xyz, r0, -c6
mad r0.w, -r0, r1.x, c16.z
mad r1.xyz, r0.w, r0, c6
mul r0.x, v1, c14
mul r2.w, r0.x, c13.x
pow r0, c16.w, r2.w
mul r1.w, v1, c14.x
mul r0.y, r1.w, c13.x
pow r3, c16.w, r0.y
mov r0.w, r3.x
add r1.w, -r0, c16.z
add r2.xyz, -r1, c12
add r0.x, -r0, c16.z
mad r1.xyz, r0.x, r2, r1
dp3_pp r0.w, v3, v3
mov r0.y, c17.x
mov r0.x, v2
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r1.w, r0, r1
rsq_pp r0.y, r0.w
dp3_pp r0.x, v4, v4
mul_pp r1.xyz, r1, c1
mul_pp r2.xyz, r0.y, v3
rsq_pp r0.x, r0.x
mad_pp r0.xyz, r0.x, v4, r2
dp3_pp r0.x, r0, r0
max_pp r0.y, r2.z, c16.x
mul_pp r2.xyz, r1, r0.y
mov_pp r0.y, c7.x
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r1.x, r0, c16
mul_pp r1.y, c16, r0
pow r0, r1.x, r1.y
mov r1.z, c15.x
add r0.y, c16.z, -r1.z
mul r0.y, r0, c8.x
mul r0.y, r0.x, r0
rcp r0.x, v5.w
mad r3.xy, v5, r0.x, c17.x
mov_pp r1.xyz, c1
dp3 r0.x, v5, v5
texld r0.w, r3, s3
cmp r0.z, -v5, c16.x, c16
mul_pp r0.z, r0, r0.w
texld r0.x, r0.x, s4
mul_pp r0.x, r0.z, r0
mul_pp r0.w, r0.x, c17.y
mul_pp r1.xyz, c2, r1
mad r0.xyz, r1, r0.y, r2
mul oC0.xyz, r0, r0.w
mov_pp oC0.w, c16.x
"
}
SubProgram "d3d11 " {
Keywords { "SPOT" }
SetTexture 0 [_WaterTex] 2D 2
SetTexture 1 [_WaterTex1] 2D 3
SetTexture 2 [_fogColorRamp] 2D 4
SetTexture 3 [_LightTexture0] 2D 0
SetTexture 4 [_LightTextureB0] 2D 1
ConstBuffer "$Globals" 368
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 192 [_Shininess]
Float 196 [_Gloss]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefieceddimkblomdfpjhngdlbepjafhkilanmpfabaaaaaaaealaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaababaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoaoaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
apapaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmmajaaaaeaaaaaaahdacaaaa
fjaaaaaeegiocaaaaaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaae
aahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafibiaaae
aahabaaaaeaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadpcbabaaa
acaaaaaagcbaaaadbcbabaaaadaaaaaagcbaaaadocbabaaaadaaaaaagcbaaaad
hcbabaaaaeaaaaaagcbaaaadpcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaahaaaaaa
dkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaacaaaaaa
fgifcaaaaaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
ggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaagbabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaa
egaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaakgbkbaaaabaaaaaaegacbaaaabaaaaaaefaaaaaj
pcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaabaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaacaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
abaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaacaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaabaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaafgbfbaaaabaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
agiacaaaaaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaajaaaaaaegiccaia
ebaaaaaaaaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaa
aaaaaaaabaaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
bcaabaaaabaaaaaadkbabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaabaaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaaj
bcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaa
acaaaaaaagiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaa
acaaaaaafgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaa
acaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
acaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
bcaabaaaabaaaaaaakbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaa
aaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaa
aagabaaaaeaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaaegbcbaaaaeaaaaaa
egbcbaaaaeaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaabaaaaaah
bcaabaaaabaaaaaajgbhbaaaadaaaaaajgbhbaaaadaaaaaaeeaaaaafbcaabaaa
abaaaaaaakaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaagaabaaaabaaaaaa
jgbhbaaaadaaaaaadcaaaaajlcaabaaaabaaaaaaegbibaaaaeaaaaaapgapbaaa
aaaaaaaaegaibaaaabaaaaaadeaaaaahicaabaaaaaaaaaaackaabaaaabaaaaaa
abeaaaaaaaaaaaaabaaaaaahbcaabaaaabaaaaaaegadbaaaabaaaaaaegadbaaa
abaaaaaaeeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahbcaabaaa
abaaaaaaakaabaaaabaaaaaadkaabaaaabaaaaaadeaaaaahbcaabaaaabaaaaaa
akaabaaaabaaaaaaabeaaaaaaaaaaaaacpaaaaafbcaabaaaabaaaaaaakaabaaa
abaaaaaadiaaaaaiccaabaaaabaaaaaaakiacaaaaaaaaaaaamaaaaaaabeaaaaa
aaaaaaeddiaaaaahbcaabaaaabaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaa
bjaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaaaaaaaaajccaabaaaabaaaaaa
akiacaiaebaaaaaaaaaaaaaabgaaaaaaabeaaaaaaaaaiadpdiaaaaaiccaabaaa
abaaaaaabkaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahbcaabaaa
abaaaaaabkaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaajocaabaaaabaaaaaa
agijcaaaaaaaaaaaabaaaaaaagijcaaaaaaaaaaaacaaaaaadiaaaaahhcaabaaa
abaaaaaaagaabaaaabaaaaaajgahbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaaaoaaaaahdcaabaaa
abaaaaaaegbabaaaafaaaaaapgbpbaaaafaaaaaaaaaaaaakdcaabaaaabaaaaaa
egaabaaaabaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaaaaefaaaaaj
pcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaadaaaaaaaagabaaaaaaaaaaa
dbaaaaahicaabaaaaaaaaaaaabeaaaaaaaaaaaaackbabaaaafaaaaaaabaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahicaabaaa
aaaaaaaadkaabaaaabaaaaaadkaabaaaaaaaaaaabaaaaaahbcaabaaaabaaaaaa
egbcbaaaafaaaaaaegbcbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaaagaabaaa
abaaaaaaeghobaaaaeaaaaaaaagabaaaabaaaaaaapaaaaahicaabaaaaaaaaaaa
pgapbaaaaaaaaaaaagaabaaaabaaaaaadiaaaaahhccabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaa
doaaaaab"
}
SubProgram "opengl " {
Keywords { "POINT_COOKIE" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTextureB0] 2D 3
SetTexture 4 [_LightTexture0] CUBE 4
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 0, 128, 1, 2.718282 },
		{ 0.5, 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[1], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[12].x;
DP3 R1.w, fragment.texcoord[4], fragment.texcoord[4];
MAD R2.xyz, fragment.texcoord[0].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[0].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[3].x, R2;
ADD R0.w, -R0, c[13].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[5];
ADD R0.w, fragment.texcoord[1], -c[12].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.y, -R0.w, c[20], c[20].z;
MUL R1.x, R0.w, R0.w;
MUL R0.w, fragment.texcoord[1].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
MAD R1.x, -R1, R1.y, c[19].z;
ADD R0.xyz, R0, -c[6];
MAD R0.xyz, R1.x, R0, c[6];
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].z;
MAD R1.xyz, R0.w, R1, R0;
DP3 R0.w, fragment.texcoord[3], fragment.texcoord[3];
RSQ R1.w, R1.w;
MOV R0.y, c[20].x;
MOV R0.x, fragment.texcoord[2];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, fragment.texcoord[3];
MAD R3.xyz, R1.w, fragment.texcoord[4], R0;
MUL R0.w, fragment.texcoord[1], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
ADD R0.x, -R0.w, c[19].z;
MAD R1.xyz, R0.x, R2, R1;
DP3 R0.x, R3, R3;
RSQ R0.x, R0.x;
DP3 R1.w, fragment.texcoord[5], fragment.texcoord[5];
MAX R0.y, R0.z, c[19].x;
MUL R0.x, R0, R3.z;
MAX R0.z, R0.x, c[19].x;
MUL R1.xyz, R1, c[1];
MUL R1.xyz, R1, R0.y;
MOV R0.y, c[19].z;
MOV R0.x, c[19].y;
ADD R0.y, R0, -c[18].x;
MUL R0.x, R0, c[7];
MUL R0.y, R0, c[8].x;
POW R0.x, R0.z, R0.x;
MUL R2.x, R0, R0.y;
MOV R0.xyz, c[2];
MUL R0.xyz, R0, c[1];
TEX R0.w, fragment.texcoord[5], texture[4], CUBE;
TEX R1.w, R1.w, texture[3], 2D;
MUL R0.w, R1, R0;
MUL R0.w, R0, c[20].y;
MAD R0.xyz, R0, R2.x, R1;
MUL result.color.xyz, R0, R0.w;
MOV result.color.w, c[19].x;
END
# 73 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "POINT_COOKIE" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_fadeStart]
Float 10 [_fadeEnd]
Float 11 [_tiling]
Vector 12 [_fogColor]
Float 13 [_heightDensityAtViewer]
Float 14 [_globalDensity]
Float 15 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTextureB0] 2D 3
SetTexture 4 [_LightTexture0] CUBE 4
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_cube s4
def c16, 0.00000000, 128.00000000, 1.00000000, 2.71828198
def c17, 0.50000000, 2.00000000, 3.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1
dcl_texcoord2 v2.x
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
mov r0.x, c4
mul r0.x, c0.w, r0
mad r0.xyz, v1, c11.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v0.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c10.x
mad r3.xyz, v0.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v0.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v0.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c3.x, r3
add r0.w, -c9.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c5
add r0.w, v1, -c9.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c17.y, c17.z
mul r0.w, r0, r0
add r0.xyz, r0, -c6
mad r0.w, -r0, r1.x, c16.z
mad r1.xyz, r0.w, r0, c6
mul r0.x, v1, c14
mul r2.w, r0.x, c13.x
pow r0, c16.w, r2.w
mul r1.w, v1, c14.x
mul r0.y, r1.w, c13.x
pow r3, c16.w, r0.y
mov r0.w, r3.x
add r1.w, -r0, c16.z
add r2.xyz, -r1, c12
add r0.x, -r0, c16.z
mad r1.xyz, r0.x, r2, r1
dp3_pp r0.w, v3, v3
mov r0.y, c17.x
mov r0.x, v2
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r1.w, r0, r1
rsq_pp r0.y, r0.w
dp3_pp r0.x, v4, v4
mul_pp r1.xyz, r1, c1
mul_pp r2.xyz, r0.y, v3
rsq_pp r0.x, r0.x
mad_pp r0.xyz, r0.x, v4, r2
dp3_pp r0.x, r0, r0
max_pp r0.y, r2.z, c16.x
mul_pp r2.xyz, r1, r0.y
mov_pp r0.y, c7.x
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r1.x, r0, c16
mul_pp r1.y, c16, r0
pow r0, r1.x, r1.y
mov r1.z, c15.x
add r0.y, c16.z, -r1.z
mul r0.y, r0, c8.x
mul r1.w, r0.x, r0.y
dp3 r0.x, v5, v5
texld r0.x, r0.x, s3
texld r0.w, v5, s4
mul r0.w, r0.x, r0
mov_pp r1.xyz, c1
mul_pp r0.xyz, c2, r1
mul_pp r0.w, r0, c17.y
mad r0.xyz, r0, r1.w, r2
mul oC0.xyz, r0, r0.w
mov_pp oC0.w, c16.x
"
}
SubProgram "d3d11 " {
Keywords { "POINT_COOKIE" }
SetTexture 0 [_WaterTex] 2D 2
SetTexture 1 [_WaterTex1] 2D 3
SetTexture 2 [_fogColorRamp] 2D 4
SetTexture 3 [_LightTextureB0] 2D 1
SetTexture 4 [_LightTexture0] CUBE 0
ConstBuffer "$Globals" 368
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 192 [_Shininess]
Float 196 [_Gloss]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedamkmfkebappdgpbkmbinklfefodkdjljabaaaaaagmakaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaababaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoaoaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcdeajaaaaeaaaaaaaenacaaaa
fjaaaaaeegiocaaaaaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaafibiaaae
aahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaafidaaaae
aahabaaaaeaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadpcbabaaa
acaaaaaagcbaaaadbcbabaaaadaaaaaagcbaaaadocbabaaaadaaaaaagcbaaaad
hcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaahaaaaaa
dkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaacaaaaaa
fgifcaaaaaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
ggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaagbabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaa
egaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaakgbkbaaaabaaaaaaegacbaaaabaaaaaaefaaaaaj
pcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaadaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaabaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaacaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
abaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaacaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaacaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaabaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaafgbfbaaaabaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
agiacaaaaaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaajaaaaaaegiccaia
ebaaaaaaaaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaa
aaaaaaaabaaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
bcaabaaaabaaaaaadkbabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaabaaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaaj
bcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaa
acaaaaaaagiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaa
acaaaaaafgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaa
acaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
acaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
bcaabaaaabaaaaaaakbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaa
aaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaa
aagabaaaaeaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaaegbcbaaaaeaaaaaa
egbcbaaaaeaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaabaaaaaah
bcaabaaaabaaaaaajgbhbaaaadaaaaaajgbhbaaaadaaaaaaeeaaaaafbcaabaaa
abaaaaaaakaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaaagaabaaaabaaaaaa
jgbhbaaaadaaaaaadcaaaaajlcaabaaaabaaaaaaegbibaaaaeaaaaaapgapbaaa
aaaaaaaaegaibaaaabaaaaaadeaaaaahicaabaaaaaaaaaaackaabaaaabaaaaaa
abeaaaaaaaaaaaaabaaaaaahbcaabaaaabaaaaaaegadbaaaabaaaaaaegadbaaa
abaaaaaaeeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahbcaabaaa
abaaaaaaakaabaaaabaaaaaadkaabaaaabaaaaaadeaaaaahbcaabaaaabaaaaaa
akaabaaaabaaaaaaabeaaaaaaaaaaaaacpaaaaafbcaabaaaabaaaaaaakaabaaa
abaaaaaadiaaaaaiccaabaaaabaaaaaaakiacaaaaaaaaaaaamaaaaaaabeaaaaa
aaaaaaeddiaaaaahbcaabaaaabaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaa
bjaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaaaaaaaaajccaabaaaabaaaaaa
akiacaiaebaaaaaaaaaaaaaabgaaaaaaabeaaaaaaaaaiadpdiaaaaaiccaabaaa
abaaaaaabkaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahbcaabaaa
abaaaaaabkaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaajocaabaaaabaaaaaa
agijcaaaaaaaaaaaabaaaaaaagijcaaaaaaaaaaaacaaaaaadiaaaaahhcaabaaa
abaaaaaaagaabaaaabaaaaaajgahbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaahicaabaaa
aaaaaaaaegbcbaaaafaaaaaaegbcbaaaafaaaaaaefaaaaajpcaabaaaabaaaaaa
pgapbaaaaaaaaaaaeghobaaaadaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaaegbcbaaaafaaaaaaeghobaaaaeaaaaaaaagabaaaaaaaaaaaapaaaaah
icaabaaaaaaaaaaaagaabaaaabaaaaaapgapbaaaacaaaaaadiaaaaahhccabaaa
aaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaa
abeaaaaaaaaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "DIRECTIONAL_COOKIE" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 12 [_fadeStart]
Float 13 [_fadeEnd]
Float 14 [_tiling]
Vector 15 [_fogColor]
Float 16 [_heightDensityAtViewer]
Float 17 [_globalDensity]
Float 18 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTexture0] 2D 3
"3.0-!!ARBfp1.0
PARAM c[21] = { program.local[0..18],
		{ 0, 128, 1, 2.718282 },
		{ 0.5, 2, 3 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[4];
MAD R3.xyz, fragment.texcoord[1], c[14].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[12].x;
DP3 R1.w, fragment.texcoord[4], fragment.texcoord[4];
MAD R2.xyz, fragment.texcoord[0].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[0].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[0].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[0].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[3].x, R2;
ADD R0.w, -R0, c[13].x;
RCP R1.x, R0.w;
MUL R0.xyz, R0, c[5];
ADD R0.w, fragment.texcoord[1], -c[12].x;
MUL_SAT R0.w, R0, R1.x;
MAD R1.y, -R0.w, c[20], c[20].z;
MUL R1.x, R0.w, R0.w;
MUL R0.w, fragment.texcoord[1].x, c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
MAD R1.x, -R1, R1.y, c[19].z;
ADD R0.xyz, R0, -c[6];
MAD R0.xyz, R1.x, R0, c[6];
ADD R1.xyz, -R0, c[15];
ADD R0.w, -R0, c[19].z;
MAD R1.xyz, R0.w, R1, R0;
MUL R0.w, fragment.texcoord[1], c[17].x;
MUL R0.w, R0, c[16].x;
POW R0.w, c[19].w, R0.w;
RSQ R1.w, R1.w;
ADD R0.w, -R0, c[19].z;
MOV R0.y, c[20].x;
MOV R0.x, fragment.texcoord[2];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
MAD R1.xyz, R0.w, R2, R1;
MOV R0.xyz, fragment.texcoord[3];
MAD R0.xyz, R1.w, fragment.texcoord[4], R0;
DP3 R0.x, R0, R0;
RSQ R0.x, R0.x;
MUL R1.xyz, R1, c[1];
MUL R0.w, R0.x, R0.z;
MAX R0.y, fragment.texcoord[3].z, c[19].x;
MUL R0.xyz, R1, R0.y;
MAX R1.y, R0.w, c[19].x;
MOV R1.x, c[19].z;
MOV R0.w, c[19].y;
ADD R1.x, R1, -c[18];
MUL R0.w, R0, c[7].x;
POW R0.w, R1.y, R0.w;
MUL R1.x, R1, c[8];
MUL R1.w, R0, R1.x;
MOV R1.xyz, c[2];
TEX R0.w, fragment.texcoord[5], texture[3], 2D;
MUL R1.xyz, R1, c[1];
MUL R0.w, R0, c[20].y;
MAD R0.xyz, R1, R1.w, R0;
MUL result.color.xyz, R0, R0.w;
MOV result.color.w, c[19].x;
END
# 68 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "DIRECTIONAL_COOKIE" }
Vector 0 [_SinTime]
Vector 1 [_LightColor0]
Vector 2 [_SpecColor]
Float 3 [_Mix]
Float 4 [_displacement]
Vector 5 [_Color]
Vector 6 [_ColorFromSpace]
Float 7 [_Shininess]
Float 8 [_Gloss]
Float 9 [_fadeStart]
Float 10 [_fadeEnd]
Float 11 [_tiling]
Vector 12 [_fogColor]
Float 13 [_heightDensityAtViewer]
Float 14 [_globalDensity]
Float 15 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightTexture0] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c16, 0.00000000, 128.00000000, 1.00000000, 2.71828198
def c17, 0.50000000, 2.00000000, 3.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1
dcl_texcoord2 v2.x
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xy
mov r0.x, c4
mul r0.x, c0.w, r0
mad r0.xyz, v1, c11.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v0.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mov r0.w, c10.x
mad r3.xyz, v0.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v0.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v0.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v0.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c3.x, r3
add r0.w, -c9.x, r0
rcp r1.x, r0.w
mul r0.xyz, r0, c5
add r0.w, v1, -c9.x
mul_sat r0.w, r0, r1.x
mad r1.x, -r0.w, c17.y, c17.z
mul r0.w, r0, r0
add r0.xyz, r0, -c6
mad r0.w, -r0, r1.x, c16.z
mad r1.xyz, r0.w, r0, c6
mul r0.x, v1, c14
mul r2.w, r0.x, c13.x
add r2.xyz, -r1, c12
pow r0, c16.w, r2.w
mul r1.w, v1, c14.x
mul r0.y, r1.w, c13.x
pow r3, c16.w, r0.y
add r0.x, -r0, c16.z
mad r1.xyz, r0.x, r2, r1
mov r0.w, r3.x
add r0.w, -r0, c16.z
mov r0.y, c17.x
mov r0.x, v2
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r1.xyz, r0.w, r0, r1
dp3_pp r0.w, v4, v4
rsq_pp r0.w, r0.w
mov_pp r0.xyz, v3
mad_pp r0.xyz, r0.w, v4, r0
dp3_pp r0.x, r0, r0
rsq_pp r0.x, r0.x
mul_pp r0.x, r0, r0.z
max_pp r1.w, r0.x, c16.x
max_pp r0.y, v3.z, c16.x
mul_pp r1.xyz, r1, c1
mul_pp r1.xyz, r1, r0.y
mov_pp r0.y, c7.x
mul_pp r2.x, c16.y, r0.y
pow r0, r1.w, r2.x
mov r2.y, c15.x
add r0.y, c16.z, -r2
mul r0.y, r0, c8.x
mul r1.w, r0.x, r0.y
mov_pp r0.xyz, c1
texld r0.w, v5, s3
mul_pp r0.xyz, c2, r0
mul_pp r0.w, r0, c17.y
mad r0.xyz, r0, r1.w, r1
mul oC0.xyz, r0, r0.w
mov_pp oC0.w, c16.x
"
}
SubProgram "d3d11 " {
Keywords { "DIRECTIONAL_COOKIE" }
SetTexture 0 [_WaterTex] 2D 1
SetTexture 1 [_WaterTex1] 2D 2
SetTexture 2 [_fogColorRamp] 2D 3
SetTexture 3 [_LightTexture0] 2D 0
ConstBuffer "$Globals" 368
Vector 16 [_LightColor0]
Vector 32 [_SpecColor]
Float 112 [_Mix]
Float 116 [_displacement]
Vector 144 [_Color]
Vector 160 [_ColorFromSpace]
Float 192 [_Shininess]
Float 196 [_Gloss]
Float 268 [_fadeStart]
Float 272 [_fadeEnd]
Float 276 [_tiling]
Vector 288 [_fogColor]
Float 308 [_heightDensityAtViewer]
Float 320 [_globalDensity]
Float 352 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedghcjfmndkfdcmffadjffkhbaopgflpcnabaaaaaameajaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaababaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoaoaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahahaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
adadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcimaiaaaaeaaaaaaacdacaaaa
fjaaaaaeegiocaaaaaaaaaaabhaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaa
fibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaa
fibiaaaeaahabaaaadaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaad
pcbabaaaacaaaaaagcbaaaadbcbabaaaadaaaaaagcbaaaadocbabaaaadaaaaaa
gcbaaaadhcbabaaaaeaaaaaagcbaaaaddcbabaaaafaaaaaagfaaaaadpccabaaa
aaaaaaaagiaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaa
ahaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
acaaaaaafgifcaaaaaaaaaaabbaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaa
abaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadiaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaabaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaa
acaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaabaaaaaa
egacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaabaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaabaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaabaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaafgbfbaaaabaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaagiacaaaaaaaaaaaahaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaajaaaaaa
egiccaiaebaaaaaaaaaaaaaaakaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaia
ebaaaaaaaaaaaaaabaaaaaaaakiacaaaaaaaaaaabbaaaaaaaoaaaaakicaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaa
aaaaaaajbcaabaaaabaaaaaadkbabaaaacaaaaaadkiacaiaebaaaaaaaaaaaaaa
baaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaakaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegiccaaaaaaaaaaabcaaaaaadiaaaaaidcaabaaaacaaaaaa
mgbabaaaacaaaaaaagiacaaaaaaaaaaabeaaaaaadiaaaaaidcaabaaaacaaaaaa
egaabaaaacaaaaaafgifcaaaaaaaaaaabdaaaaaadiaaaaakdcaabaaaacaaaaaa
egaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaaf
dcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaia
ebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaaj
hcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaafbcaabaaaabaaaaaaakbabaaaadaaaaaadgaaaaafccaabaaaabaaaaaa
abeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
acaaaaaaaagabaaaadaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaihcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaabaaaaaabaaaaaahicaabaaaaaaaaaaaegbcbaaa
aeaaaaaaegbcbaaaaeaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaajhcaabaaaabaaaaaaegbcbaaaaeaaaaaapgapbaaaaaaaaaaajgbhbaaa
adaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaackaabaaaabaaaaaadeaaaaahicaabaaaaaaaaaaadkaabaaa
aaaaaaaaabeaaaaaaaaaaaaacpaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaaibcaabaaaabaaaaaaakiacaaaaaaaaaaaamaaaaaaabeaaaaaaaaaaaed
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaabjaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaaaaaaaaajbcaabaaaabaaaaaaakiacaia
ebaaaaaaaaaaaaaabgaaaaaaabeaaaaaaaaaiadpdiaaaaaibcaabaaaabaaaaaa
akaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaadiaaaaahicaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakaabaaaabaaaaaadiaaaaajhcaabaaaabaaaaaaegiccaaa
aaaaaaaaabaaaaaaegiccaaaaaaaaaaaacaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaaaaaaaaaegacbaaaabaaaaaadeaaaaahicaabaaaaaaaaaaadkbabaaa
adaaaaaaabeaaaaaaaaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaa
afaaaaaaeghobaaaadaaaaaaaagabaaaaaaaaaaaaaaaaaahicaabaaaaaaaaaaa
dkaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhccabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaaaaa
doaaaaab"
}
}
 }
 Pass {
  Name "PREPASS"
  Tags { "LIGHTMODE"="PrePassBase" }
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "opengl " {
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [unity_Scale]
Vector 19 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[20] = { { 1 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..19] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.xyz, c[17];
MOV R1.w, c[0].x;
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R1.xyz, R0, c[18].w, -vertex.position;
DP3 R0.w, R1, R1;
MOV R0.xyz, vertex.attrib[14];
RSQ R0.w, R0.w;
MUL R1.xyz, R0.w, R1;
DP3 R0.w, R1, vertex.normal;
MUL R2.xyz, vertex.normal.zxyw, R0.yzxw;
MAD R0.xyz, vertex.normal.yzxw, R0.zxyw, -R2;
MUL R0.xyz, R0, vertex.attrib[14].w;
DP3 R1.y, R0, c[9];
DP3 R1.x, vertex.attrib[14], c[9];
DP3 R1.z, vertex.normal, c[9];
MUL result.texcoord[3].xyz, R1, c[18].w;
DP3 R1.y, R0, c[10];
DP3 R0.y, R0, c[11];
DP3 R1.x, vertex.attrib[14], c[10];
DP3 R1.z, vertex.normal, c[10];
MUL result.texcoord[4].xyz, R1, c[18].w;
MOV R1.z, vertex.texcoord[1].x;
MOV R1.xy, vertex.texcoord[0];
ADD result.texcoord[0].w, -R0, c[0].x;
DP3 R0.w, R1, R1;
DP3 R0.x, vertex.attrib[14], c[11];
DP3 R0.z, vertex.normal, c[11];
MUL result.texcoord[5].xyz, R0, c[18].w;
RSQ R0.x, R0.w;
MUL R0.xyz, R0.x, R1;
DP4 R0.w, vertex.position, c[3];
MOV R0.w, -R0;
ABS result.texcoord[0].xyz, R0;
MOV result.texcoord[1], R0;
DP3 result.texcoord[2].x, R0, c[19];
DP4 result.position.w, vertex.position, c[8];
DP4 result.position.z, vertex.position, c[7];
DP4 result.position.y, vertex.position, c[6];
DP4 result.position.x, vertex.position, c[5];
END
# 41 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [unity_Scale]
Vector 18 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c19, 1.00000000, 0, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.xyz, c16
mov r1.w, c19.x
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c17.w, -v0
dp3 r0.x, r1, r1
rsq r0.w, r0.x
mul r1.xyz, r0.w, r1
dp3 r0.w, r1, v2
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 r1.y, r0, c8
dp3 r1.x, v1, c8
dp3 r1.z, v2, c8
mul o4.xyz, r1, c17.w
dp3 r1.y, r0, c9
dp3 r0.y, r0, c10
dp3 r1.x, v1, c9
dp3 r1.z, v2, c9
mul o5.xyz, r1, c17.w
mov r1.z, v4.x
mov r1.xy, v3
add o1.w, -r0, c19.x
dp3 r0.w, r1, r1
dp3 r0.x, v1, c10
dp3 r0.z, v2, c10
mul o6.xyz, r0, c17.w
rsq r0.x, r0.w
mul r0.xyz, r0.x, r1
dp4 r0.w, v0, c2
mov r0.w, -r0
abs o1.xyz, r0
mov o2, r0
dp3 o3.x, r0, c18
dp4 o0.w, v0, c7
dp4 o0.z, v0, c6
dp4 o0.y, v0, c5
dp4 o0.x, v0, c4
"
}
SubProgram "d3d11 " {
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 304
Vector 272 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedhhoibeljofhhnmhmpefkhemdikljbgchabaaaaaajaaiaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaalmaaaaaaabaaaaaaaaaaaaaa
adaaaaaaacaaaaaaapaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaa
abaoaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoabaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaahaiaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcmeagaaaaeaaaabaalbabaaaafjaaaaaeegiocaaaaaaaaaaa
bcaaaaaafjaaaaaeegiocaaaabaaaaaaafaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaaabaaaaaafpaaaaad
hcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaadbcbabaaaaeaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadpccabaaaabaaaaaagfaaaaad
pccabaaaacaaaaaagfaaaaadbccabaaaadaaaaaagfaaaaadoccabaaaadaaaaaa
gfaaaaadhccabaaaaeaaaaaagfaaaaadhccabaaaafaaaaaagiaaaaacadaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaajhcaabaaa
aaaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaacaaaaaabbaaaaaadcaaaaal
hcaabaaaaaaaaaaaegiccaaaacaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaa
egacbaaaaaaaaaaadcaaaaalhcaabaaaaaaaaaaaegiccaaaacaaaaaabcaaaaaa
kgikcaaaabaaaaaaaeaaaaaaegacbaaaaaaaaaaaaaaaaaaihcaabaaaaaaaaaaa
egacbaaaaaaaaaaaegiccaaaacaaaaaabdaaaaaadcaaaaalhcaabaaaaaaaaaaa
egacbaaaaaaaaaaapgipcaaaacaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaa
baaaaaahicaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
icaabaaaaaaaaaaadkaabaaaaaaaaaaadiaaaaahhcaabaaaaaaaaaaapgapbaaa
aaaaaaaaegacbaaaaaaaaaaabaaaaaahbcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egbcbaaaacaaaaaaaaaaaaaiiccabaaaabaaaaaaakaabaiaebaaaaaaaaaaaaaa
abeaaaaaaaaaiadpdgaaaaafdcaabaaaaaaaaaaaegbabaaaadaaaaaadgaaaaaf
ecaabaaaaaaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaaaaaaaaaegacbaaa
aaaaaaaaegacbaaaaaaaaaaaeeaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaa
diaaaaahhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaag
hccabaaaabaaaaaaegacbaiaibaaaaaaaaaaaaaadgaaaaafhccabaaaacaaaaaa
egacbaaaaaaaaaaabaaaaaaibccabaaaadaaaaaaegiccaaaaaaaaaaabbaaaaaa
egacbaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaabkbabaaaaaaaaaaackiacaaa
acaaaaaaafaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaaacaaaaaaaeaaaaaa
akbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaaaaaaaaaackiacaaa
acaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaa
aaaaaaaackiacaaaacaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaaaaaaaaa
dgaaaaagiccabaaaacaaaaaaakaabaiaebaaaaaaaaaaaaaadiaaaaahhcaabaaa
aaaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaaaaaaaaa
jgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaadiaaaaah
hcaabaaaaaaaaaaaegacbaaaaaaaaaaapgbpbaaaabaaaaaadgaaaaagbcaabaaa
abaaaaaaakiacaaaacaaaaaaamaaaaaadgaaaaagccaabaaaabaaaaaaakiacaaa
acaaaaaaanaaaaaadgaaaaagecaabaaaabaaaaaaakiacaaaacaaaaaaaoaaaaaa
baaaaaahecaabaaaacaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaah
ccaabaaaacaaaaaaegbcbaaaabaaaaaaegacbaaaabaaaaaabaaaaaahicaabaaa
acaaaaaaegbcbaaaacaaaaaaegacbaaaabaaaaaadiaaaaaioccabaaaadaaaaaa
fgaobaaaacaaaaaapgipcaaaacaaaaaabeaaaaaadgaaaaagbcaabaaaabaaaaaa
bkiacaaaacaaaaaaamaaaaaadgaaaaagccaabaaaabaaaaaabkiacaaaacaaaaaa
anaaaaaadgaaaaagecaabaaaabaaaaaabkiacaaaacaaaaaaaoaaaaaabaaaaaah
ccaabaaaacaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaahbcaabaaa
acaaaaaaegbcbaaaabaaaaaaegacbaaaabaaaaaabaaaaaahecaabaaaacaaaaaa
egbcbaaaacaaaaaaegacbaaaabaaaaaadiaaaaaihccabaaaaeaaaaaaegacbaaa
acaaaaaapgipcaaaacaaaaaabeaaaaaadgaaaaagbcaabaaaabaaaaaackiacaaa
acaaaaaaamaaaaaadgaaaaagccaabaaaabaaaaaackiacaaaacaaaaaaanaaaaaa
dgaaaaagecaabaaaabaaaaaackiacaaaacaaaaaaaoaaaaaabaaaaaahccaabaaa
aaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaahbcaabaaaaaaaaaaa
egbcbaaaabaaaaaaegacbaaaabaaaaaabaaaaaahecaabaaaaaaaaaaaegbcbaaa
acaaaaaaegacbaaaabaaaaaadiaaaaaihccabaaaafaaaaaaegacbaaaaaaaaaaa
pgipcaaaacaaaaaabeaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Float 2 [_Shininess]
"3.0-!!ARBfp1.0
PARAM c[9] = { program.local[0..7],
		{ 0.5 } };
TEMP R0;
MOV R0.z, fragment.texcoord[5];
MOV R0.x, fragment.texcoord[3].z;
MOV R0.y, fragment.texcoord[4].z;
MAD result.color.xyz, R0, c[8].x, c[8].x;
MOV result.color.w, c[2].x;
END
# 5 instructions, 1 R-regs
"
}
SubProgram "d3d9 " {
Float 0 [_Shininess]
"ps_3_0
def c1, 0.50000000, 0, 0, 0
dcl_texcoord3 v3.xyz
dcl_texcoord4 v4.xyz
dcl_texcoord5 v5.xyz
mov_pp r0.z, v5
mov_pp r0.x, v3.z
mov_pp r0.y, v4.z
mad_pp oC0.xyz, r0, c1.x, c1.x
mov_pp oC0.w, c0.x
"
}
SubProgram "d3d11 " {
ConstBuffer "$Globals" 304
Float 128 [_Shininess]
BindCB  "$Globals" 0
"ps_4_0
eefieceddccobfekicjkmlgfiohpkhalkaanfndbabaaaaaabmacaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapaaaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaabaaaaaalmaaaaaa
adaaaaaaaaaaaaaaadaaaaaaadaaaaaaaoaiaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaahaeaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahaeaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcoeaaaaaaeaaaaaaadjaaaaaa
fjaaaaaeegiocaaaaaaaaaaaajaaaaaagcbaaaadicbabaaaadaaaaaagcbaaaad
ecbabaaaaeaaaaaagcbaaaadecbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacabaaaaaadgaaaaafbcaabaaaaaaaaaaadkbabaaaadaaaaaadgaaaaaf
ccaabaaaaaaaaaaackbabaaaaeaaaaaadgaaaaafecaabaaaaaaaaaaackbabaaa
afaaaaaadcaaaaaphccabaaaaaaaaaaaegacbaaaaaaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaaadpaaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaa
dgaaaaagiccabaaaaaaaaaaaakiacaaaaaaaaaaaaiaaaaaadoaaaaab"
}
}
 }
 Pass {
  Name "PREPASS"
  Tags { "LIGHTMODE"="PrePassFinal" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
Program "vp" {
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 19 [unity_SHAr]
Vector 20 [unity_SHAg]
Vector 21 [unity_SHAb]
Vector 22 [unity_SHBr]
Vector 23 [unity_SHBg]
Vector 24 [unity_SHBb]
Vector 25 [unity_SHC]
Vector 26 [unity_Scale]
Vector 27 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[28] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..27] };
TEMP R0;
TEMP R1;
TEMP R2;
MUL R1.xyz, vertex.normal, c[26].w;
DP3 R2.w, R1, c[10];
DP3 R0.x, R1, c[9];
DP3 R0.z, R1, c[11];
MOV R0.y, R2.w;
MOV R0.w, c[0].x;
MUL R1, R0.xyzz, R0.yzzx;
DP4 R2.z, R0, c[21];
DP4 R2.y, R0, c[20];
DP4 R2.x, R0, c[19];
MUL R0.w, R2, R2;
MAD R0.w, R0.x, R0.x, -R0;
DP4 R0.z, R1, c[24];
DP4 R0.y, R1, c[23];
DP4 R0.x, R1, c[22];
ADD R0.xyz, R2, R0;
MUL R1.xyz, R0.w, c[25];
ADD result.texcoord[5].xyz, R0, R1;
MOV R1.w, c[0].x;
MOV R1.xyz, c[17];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[26].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[18].x;
MOV R0.x, R2;
ADD result.texcoord[4].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[27];
MOV result.texcoord[4].zw, R1;
END
# 56 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [unity_SHAr]
Vector 20 [unity_SHAg]
Vector 21 [unity_SHAb]
Vector 22 [unity_SHBr]
Vector 23 [unity_SHBg]
Vector 24 [unity_SHBb]
Vector 25 [unity_SHC]
Vector 26 [unity_Scale]
Vector 27 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c28, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mul r1.xyz, v2, c26.w
dp3 r2.w, r1, c9
dp3 r0.x, r1, c8
dp3 r0.z, r1, c10
mov r0.y, r2.w
mul r1, r0.xyzz, r0.yzzx
mov r0.w, c28.x
dp4 r2.z, r0, c21
dp4 r2.y, r0, c20
dp4 r2.x, r0, c19
mul r0.y, r2.w, r2.w
dp4 r3.z, r1, c24
dp4 r3.y, r1, c23
dp4 r3.x, r1, c22
add r1.xyz, r2, r3
mad r0.x, r0, r0, -r0.y
mul r2.xyz, r0.x, c25
add o6.xyz, r1, r2
mov r1.xyz, v1
mul r2.xyz, v2.zxyw, r1.yzxw
mov r1.xyz, v1
mad r1.xyz, v2.yzxw, r1.zxyw, -r2
mul r1.xyz, r1, v1.w
mov r0.w, c28.x
mov r0.xyz, c16
dp4 r3.z, r0, c14
dp4 r3.x, r0, c12
dp4 r3.y, r0, c13
mad r0.xyz, r3, c26.w, -v0
dp3 o1.y, r0, r1
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r0
dp3 r0.w, v2, r2
dp3 o1.z, v2, r0
dp3 o1.x, r0, v1
dp4 r1.w, v0, c7
dp4 r1.z, v0, c6
dp4 r1.x, v0, c4
dp4 r1.y, v0, c5
mul r2.xyz, r1.xyww, c28.y
mov o0, r1
mul r0.y, r2, c17.x
mov r0.x, r2
mad o5.xy, r2.z, c18.zwzw, r0
mov r0.xy, v3
mov r0.z, v4.x
add o2.w, -r0, c28.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
abs o2.xyz, r0
mov o3, r0
dp3 o4.x, r0, c27
mov o5.zw, r1
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 320
Vector 272 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
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
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecednkpnnndnaddeafcllkjmcankkpinipgjabaaaaaafeajaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefciiahaaaaeaaaabaaocabaaaafjaaaaaeegiocaaaaaaaaaaa
bcaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
cnaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadhccabaaaabaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaa
acaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadpccabaaaaeaaaaaagfaaaaad
hccabaaaafaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaiiccabaaaabaaaaaaegiccaaa
aaaaaaaabbaaaaaaegacbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaajgbebaaa
abaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaajgbebaaaacaaaaaa
cgbjbaaaabaaaaaaegacbaiaebaaaaaaacaaaaaadiaaaaahhcaabaaaacaaaaaa
egacbaaaacaaaaaapgbpbaaaabaaaaaadiaaaaajhcaabaaaadaaaaaafgifcaaa
abaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaadaaaaaa
egiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaadaaaaaa
dcaaaaalhcaabaaaadaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaa
aeaaaaaaegacbaaaadaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaaaadaaaaaa
egiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaaadaaaaaaegacbaaaadaaaaaa
pgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahcccabaaa
abaaaaaaegacbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbccabaaaabaaaaaa
egbcbaaaabaaaaaaegacbaaaadaaaaaabaaaaaaheccabaaaabaaaaaaegbcbaaa
acaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaa
egacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaah
hcaabaaaacaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaa
abaaaaaaegacbaaaacaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaaacaaaaaa
dkaabaiaebaaaaaaabaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaaacaaaaaa
egacbaiaibaaaaaaabaaaaaadgaaaaafhccabaaaadaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
adaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaagiccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaa
aaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaa
aeaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaaeaaaaaakgakbaaaabaaaaaa
mgaabaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaa
adaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaa
adaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaadaaaaaaamaaaaaa
agaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaa
adaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaaficaabaaa
aaaaaaaaabeaaaaaaaaaiadpbbaaaaaibcaabaaaabaaaaaaegiocaaaacaaaaaa
cgaaaaaaegaobaaaaaaaaaaabbaaaaaiccaabaaaabaaaaaaegiocaaaacaaaaaa
chaaaaaaegaobaaaaaaaaaaabbaaaaaiecaabaaaabaaaaaaegiocaaaacaaaaaa
ciaaaaaaegaobaaaaaaaaaaadiaaaaahpcaabaaaacaaaaaajgacbaaaaaaaaaaa
egakbaaaaaaaaaaabbaaaaaibcaabaaaadaaaaaaegiocaaaacaaaaaacjaaaaaa
egaobaaaacaaaaaabbaaaaaiccaabaaaadaaaaaaegiocaaaacaaaaaackaaaaaa
egaobaaaacaaaaaabbaaaaaiecaabaaaadaaaaaaegiocaaaacaaaaaaclaaaaaa
egaobaaaacaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
adaaaaaadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaia
ebaaaaaaaaaaaaaadcaaaaakhccabaaaafaaaaaaegiccaaaacaaaaaacmaaaaaa
agaabaaaaaaaaaaaegacbaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 19 [unity_ShadowFadeCenterAndType]
Vector 20 [unity_Scale]
Vector 21 [_sunLightDirection]
Vector 22 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[23] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..22] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.w, c[0].x;
MOV R1.xyz, c[17];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[20].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[18].x;
MOV R0.x, R2;
ADD result.texcoord[4].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[21];
DP4 R0.x, vertex.position, c[9];
DP4 R0.z, vertex.position, c[11];
DP4 R0.y, vertex.position, c[10];
ADD R2.xyz, R0, -c[19];
MOV R0.x, c[0];
ADD R0.x, R0, -c[19].w;
MOV result.texcoord[4].zw, R1;
MUL result.texcoord[6].xyz, R2, c[19].w;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[22], c[22].zwzw;
MUL result.texcoord[6].w, -R1.x, R0.x;
END
# 47 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [unity_ShadowFadeCenterAndType]
Vector 20 [unity_Scale]
Vector 21 [_sunLightDirection]
Vector 22 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c23, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c23.x
mov r1.xyz, c16
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c20.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
add o2.w, -r0, c23.x
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c23.y
mov o0, r0
mul r1.y, r2, c17.x
mov r1.x, r2
mad o5.xy, r2.z, c18.zwzw, r1
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.y, r1.w
mul r1.xyz, r0.y, r1
dp4 r0.x, v0, c2
mov r1.w, -r0.x
mov r0.y, c19.w
add r0.y, c23.x, -r0
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c21
dp4 r1.z, v0, c10
dp4 r1.x, v0, c8
dp4 r1.y, v0, c9
add r1.xyz, r1, -c19
mov o5.zw, r0
mul o7.xyz, r1, c19.w
mad o6.xy, v4, c22, c22.zwzw
mul o7.w, -r0.x, r0.y
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 352
Vector 272 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityShadows" 416
Vector 400 [unity_ShadowFadeCenterAndType]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityShadows" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedjpmokdpiiapdmlfllcccahnpmemdiohdabaaaaaaliaiaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapaaaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadamaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
neagaaaaeaaaabaalfabaaaafjaaaaaeegiocaaaaaaaaaaabeaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaabkaaaaaafjaaaaae
egiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaa
abaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadpccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaa
gfaaaaadpccabaaaagaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaiiccabaaaabaaaaaa
egiccaaaaaaaaaaabbaaaaaaegacbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaa
jgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaajgbebaaa
acaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaacaaaaaadiaaaaahhcaabaaa
acaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaadiaaaaajhcaabaaaadaaaaaa
fgifcaaaabaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaa
adaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaa
adaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaaa
adaaaaaaegiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaaadaaaaaaegacbaaa
adaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
cccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbccabaaa
abaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaabaaaaaaheccabaaaabaaaaaa
egbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbcaabaaaacaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaeeaaaaafbcaabaaaacaaaaaaakaabaaaacaaaaaa
diaaaaahhcaabaaaacaaaaaaagaabaaaacaaaaaaegacbaaaadaaaaaabaaaaaah
bcaabaaaacaaaaaaegacbaaaacaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
acaaaaaaakaabaiaebaaaaaaacaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaa
acaaaaaaegacbaiaibaaaaaaabaaaaaadiaaaaaibcaabaaaacaaaaaabkbabaaa
aaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakbcaabaaaacaaaaaackiacaaa
adaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaacaaaaaadcaaaaakbcaabaaa
acaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaacaaaaaa
dcaaaaakbcaabaaaacaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaaaaaaaaaa
akaabaaaacaaaaaadgaaaaagicaabaaaabaaaaaaakaabaiaebaaaaaaacaaaaaa
dgaaaaafpccabaaaadaaaaaaegaobaaaabaaaaaadiaaaaaiccaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaacaaaaaa
agahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaaf
mccabaaaaeaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaaeaaaaaakgakbaaa
acaaaaaamgaabaaaacaaaaaadcaaaaaldccabaaaafaaaaaaegbabaaaaeaaaaaa
egiacaaaaaaaaaaabdaaaaaaogikcaaaaaaaaaaabdaaaaaaaaaaaaajbcaabaaa
aaaaaaaadkiacaiaebaaaaaaacaaaaaabjaaaaaaabeaaaaaaaaaiadpdiaaaaah
iccabaaaagaaaaaaakaabaaaaaaaaaaadkaabaaaabaaaaaadiaaaaaihcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaakhcaabaaa
aaaaaaaaegiccaaaadaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaaaaaaaaaa
dcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaiaebaaaaaaacaaaaaabjaaaaaadiaaaaaihccabaaaagaaaaaa
egacbaaaaaaaaaaapgipcaaaacaaaaaabjaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_World2Object]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_ProjectionParams]
Vector 15 [unity_Scale]
Vector 16 [_sunLightDirection]
Vector 17 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[18] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..17] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.w, c[0].x;
MOV R1.xyz, c[13];
DP4 R0.z, R1, c[11];
DP4 R0.x, R1, c[9];
DP4 R0.y, R1, c[10];
MAD R0.xyz, R0, c[15].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[14].x;
MOV R0.x, R2;
ADD result.texcoord[4].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[16];
MOV result.texcoord[4].zw, R1;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[17], c[17].zwzw;
END
# 39 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_World2Object]
Vector 12 [_WorldSpaceCameraPos]
Vector 13 [_ProjectionParams]
Vector 14 [_ScreenParams]
Vector 15 [unity_Scale]
Vector 16 [_sunLightDirection]
Vector 17 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c18, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c18.x
mov r1.xyz, c12
dp4 r0.z, r1, c10
dp4 r0.x, r1, c8
dp4 r0.y, r1, c9
mad r1.xyz, r0, c15.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
add o2.w, -r0, c18.x
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c18.y
mov o0, r0
mul r1.y, r2, c13.x
mov r1.x, r2
mad o5.xy, r2.z, c14.zwzw, r1
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.x, r1.w
mul r1.xyz, r0.x, r1
dp4 r0.y, v0, c2
mov r1.w, -r0.y
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c16
mov o5.zw, r0
mad o6.xy, v4, c17, c17.zwzw
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 352
Vector 272 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedbcfmodgpaogobicjacmmlofomgkcokhjabaaaaaagiahaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcjmafaaaaeaaaabaaghabaaaafjaaaaaeegiocaaaaaaaaaaa
beaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaaabaaaaaafpaaaaad
hcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
iccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaadpccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaagiaaaaacaeaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaa
dgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaa
baaaaaaiiccabaaaabaaaaaaegiccaaaaaaaaaaabbaaaaaaegacbaaaabaaaaaa
diaaaaahhcaabaaaacaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaak
hcaabaaaacaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaa
acaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaa
diaaaaajhcaabaaaadaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaacaaaaaa
bbaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaacaaaaaabaaaaaaaagiacaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaa
acaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaai
hcaabaaaadaaaaaaegacbaaaadaaaaaaegiccaaaacaaaaaabdaaaaaadcaaaaal
hcaabaaaadaaaaaaegacbaaaadaaaaaapgipcaaaacaaaaaabeaaaaaaegbcbaia
ebaaaaaaaaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaa
adaaaaaabaaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaa
baaaaaaheccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaa
abaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaabaaaaaa
egacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaacaaaaaaegbcbaaa
acaaaaaaaaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaa
aaaaiadpdgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaadgaaaaaf
hccabaaaadaaaaaaegacbaaaabaaaaaadiaaaaaibcaabaaaabaaaaaabkbabaaa
aaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaa
abaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaahaaaaaadkbabaaaaaaaaaaa
akaabaaaabaaaaaadgaaaaagiccabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaa
diaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaa
diaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaa
aaaaaadpaaaaaadpdgaaaaafmccabaaaaeaaaaaakgaobaaaaaaaaaaaaaaaaaah
dccabaaaaeaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaadcaaaaaldccabaaa
afaaaaaaegbabaaaaeaaaaaaegiacaaaaaaaaaaabdaaaaaaogikcaaaaaaaaaaa
bdaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 19 [unity_SHAr]
Vector 20 [unity_SHAg]
Vector 21 [unity_SHAb]
Vector 22 [unity_SHBr]
Vector 23 [unity_SHBg]
Vector 24 [unity_SHBb]
Vector 25 [unity_SHC]
Vector 26 [unity_Scale]
Vector 27 [_sunLightDirection]
"3.0-!!ARBvp1.0
PARAM c[28] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..27] };
TEMP R0;
TEMP R1;
TEMP R2;
MUL R1.xyz, vertex.normal, c[26].w;
DP3 R2.w, R1, c[10];
DP3 R0.x, R1, c[9];
DP3 R0.z, R1, c[11];
MOV R0.y, R2.w;
MOV R0.w, c[0].x;
MUL R1, R0.xyzz, R0.yzzx;
DP4 R2.z, R0, c[21];
DP4 R2.y, R0, c[20];
DP4 R2.x, R0, c[19];
MUL R0.w, R2, R2;
MAD R0.w, R0.x, R0.x, -R0;
DP4 R0.z, R1, c[24];
DP4 R0.y, R1, c[23];
DP4 R0.x, R1, c[22];
ADD R0.xyz, R2, R0;
MUL R1.xyz, R0.w, c[25];
ADD result.texcoord[5].xyz, R0, R1;
MOV R1.w, c[0].x;
MOV R1.xyz, c[17];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[26].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[18].x;
MOV R0.x, R2;
ADD result.texcoord[4].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[27];
MOV result.texcoord[4].zw, R1;
END
# 56 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [unity_SHAr]
Vector 20 [unity_SHAg]
Vector 21 [unity_SHAb]
Vector 22 [unity_SHBr]
Vector 23 [unity_SHBg]
Vector 24 [unity_SHBb]
Vector 25 [unity_SHC]
Vector 26 [unity_Scale]
Vector 27 [_sunLightDirection]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c28, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mul r1.xyz, v2, c26.w
dp3 r2.w, r1, c9
dp3 r0.x, r1, c8
dp3 r0.z, r1, c10
mov r0.y, r2.w
mul r1, r0.xyzz, r0.yzzx
mov r0.w, c28.x
dp4 r2.z, r0, c21
dp4 r2.y, r0, c20
dp4 r2.x, r0, c19
mul r0.y, r2.w, r2.w
dp4 r3.z, r1, c24
dp4 r3.y, r1, c23
dp4 r3.x, r1, c22
add r1.xyz, r2, r3
mad r0.x, r0, r0, -r0.y
mul r2.xyz, r0.x, c25
add o6.xyz, r1, r2
mov r1.xyz, v1
mul r2.xyz, v2.zxyw, r1.yzxw
mov r1.xyz, v1
mad r1.xyz, v2.yzxw, r1.zxyw, -r2
mul r1.xyz, r1, v1.w
mov r0.w, c28.x
mov r0.xyz, c16
dp4 r3.z, r0, c14
dp4 r3.x, r0, c12
dp4 r3.y, r0, c13
mad r0.xyz, r3, c26.w, -v0
dp3 o1.y, r0, r1
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r0
dp3 r0.w, v2, r2
dp3 o1.z, v2, r0
dp3 o1.x, r0, v1
dp4 r1.w, v0, c7
dp4 r1.z, v0, c6
dp4 r1.x, v0, c4
dp4 r1.y, v0, c5
mul r2.xyz, r1.xyww, c28.y
mov o0, r1
mul r0.y, r2, c17.x
mov r0.x, r2
mad o5.xy, r2.z, c18.zwzw, r0
mov r0.xy, v3
mov r0.z, v4.x
add o2.w, -r0, c28.x
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp4 r1.x, v0, c2
mov r0.w, -r1.x
abs o2.xyz, r0
mov o3, r0
dp3 o4.x, r0, c27
mov o5.zw, r1
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 320
Vector 272 [_sunLightDirection]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
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
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityLighting" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecednkpnnndnaddeafcllkjmcankkpinipgjabaaaaaafeajaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapabaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaahaiaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefciiahaaaaeaaaabaaocabaaaafjaaaaaeegiocaaaaaaaaaaa
bcaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
cnaaaaaafjaaaaaeegiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaa
fpaaaaadpcbabaaaabaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaa
adaaaaaafpaaaaadbcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaadhccabaaaabaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaa
acaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaadpccabaaaaeaaaaaagfaaaaad
hccabaaaafaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaa
aaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
adaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaaaaaaaaaa
egaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaaf
dcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaaakbabaaa
aeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaa
eeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
pgapbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaiiccabaaaabaaaaaaegiccaaa
aaaaaaaabbaaaaaaegacbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaajgbebaaa
abaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaajgbebaaaacaaaaaa
cgbjbaaaabaaaaaaegacbaiaebaaaaaaacaaaaaadiaaaaahhcaabaaaacaaaaaa
egacbaaaacaaaaaapgbpbaaaabaaaaaadiaaaaajhcaabaaaadaaaaaafgifcaaa
abaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaaadaaaaaa
egiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaaadaaaaaa
dcaaaaalhcaabaaaadaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaaabaaaaaa
aeaaaaaaegacbaaaadaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaaaadaaaaaa
egiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaaadaaaaaaegacbaaaadaaaaaa
pgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaahcccabaaa
abaaaaaaegacbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbccabaaaabaaaaaa
egbcbaaaabaaaaaaegacbaaaadaaaaaabaaaaaaheccabaaaabaaaaaaegbcbaaa
acaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaa
egacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaah
hcaabaaaacaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaahicaabaaa
abaaaaaaegacbaaaacaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaaacaaaaaa
dkaabaiaebaaaaaaabaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaaacaaaaaa
egacbaiaibaaaaaaabaaaaaadgaaaaafhccabaaaadaaaaaaegacbaaaabaaaaaa
diaaaaaibcaabaaaabaaaaaabkbabaaaaaaaaaaackiacaaaadaaaaaaafaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaaeaaaaaaakbabaaaaaaaaaaa
akaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaaadaaaaaaagaaaaaa
ckbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
adaaaaaaahaaaaaadkbabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaagiccabaaa
adaaaaaaakaabaiaebaaaaaaabaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaa
aaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaa
aaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaa
aeaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaaeaaaaaakgakbaaaabaaaaaa
mgaabaaaabaaaaaadiaaaaaihcaabaaaaaaaaaaaegbcbaaaacaaaaaapgipcaaa
adaaaaaabeaaaaaadiaaaaaihcaabaaaabaaaaaafgafbaaaaaaaaaaaegiccaaa
adaaaaaaanaaaaaadcaaaaaklcaabaaaaaaaaaaaegiicaaaadaaaaaaamaaaaaa
agaabaaaaaaaaaaaegaibaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaa
adaaaaaaaoaaaaaakgakbaaaaaaaaaaaegadbaaaaaaaaaaadgaaaaaficaabaaa
aaaaaaaaabeaaaaaaaaaiadpbbaaaaaibcaabaaaabaaaaaaegiocaaaacaaaaaa
cgaaaaaaegaobaaaaaaaaaaabbaaaaaiccaabaaaabaaaaaaegiocaaaacaaaaaa
chaaaaaaegaobaaaaaaaaaaabbaaaaaiecaabaaaabaaaaaaegiocaaaacaaaaaa
ciaaaaaaegaobaaaaaaaaaaadiaaaaahpcaabaaaacaaaaaajgacbaaaaaaaaaaa
egakbaaaaaaaaaaabbaaaaaibcaabaaaadaaaaaaegiocaaaacaaaaaacjaaaaaa
egaobaaaacaaaaaabbaaaaaiccaabaaaadaaaaaaegiocaaaacaaaaaackaaaaaa
egaobaaaacaaaaaabbaaaaaiecaabaaaadaaaaaaegiocaaaacaaaaaaclaaaaaa
egaobaaaacaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
adaaaaaadiaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaabkaabaaaaaaaaaaa
dcaaaaakbcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaaaaaaaaabkaabaia
ebaaaaaaaaaaaaaadcaaaaakhccabaaaafaaaaaaegiccaaaacaaaaaacmaaaaaa
agaabaaaaaaaaaaaegacbaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_Object2World]
Matrix 13 [_World2Object]
Vector 17 [_WorldSpaceCameraPos]
Vector 18 [_ProjectionParams]
Vector 19 [unity_ShadowFadeCenterAndType]
Vector 20 [unity_Scale]
Vector 21 [_sunLightDirection]
Vector 22 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[23] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..22] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.w, c[0].x;
MOV R1.xyz, c[17];
DP4 R0.z, R1, c[15];
DP4 R0.x, R1, c[13];
DP4 R0.y, R1, c[14];
MAD R0.xyz, R0, c[20].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[18].x;
MOV R0.x, R2;
ADD result.texcoord[4].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[21];
DP4 R0.x, vertex.position, c[9];
DP4 R0.z, vertex.position, c[11];
DP4 R0.y, vertex.position, c[10];
ADD R2.xyz, R0, -c[19];
MOV R0.x, c[0];
ADD R0.x, R0, -c[19].w;
MOV result.texcoord[4].zw, R1;
MUL result.texcoord[6].xyz, R2, c[19].w;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[22], c[22].zwzw;
MUL result.texcoord[6].w, -R1.x, R0.x;
END
# 47 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_Object2World]
Matrix 12 [_World2Object]
Vector 16 [_WorldSpaceCameraPos]
Vector 17 [_ProjectionParams]
Vector 18 [_ScreenParams]
Vector 19 [unity_ShadowFadeCenterAndType]
Vector 20 [unity_Scale]
Vector 21 [_sunLightDirection]
Vector 22 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
dcl_texcoord6 o7
def c23, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c23.x
mov r1.xyz, c16
dp4 r0.z, r1, c14
dp4 r0.x, r1, c12
dp4 r0.y, r1, c13
mad r1.xyz, r0, c20.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
add o2.w, -r0, c23.x
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c23.y
mov o0, r0
mul r1.y, r2, c17.x
mov r1.x, r2
mad o5.xy, r2.z, c18.zwzw, r1
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.y, r1.w
mul r1.xyz, r0.y, r1
dp4 r0.x, v0, c2
mov r1.w, -r0.x
mov r0.y, c19.w
add r0.y, c23.x, -r0
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c21
dp4 r1.z, v0, c10
dp4 r1.x, v0, c8
dp4 r1.y, v0, c9
add r1.xyz, r1, -c19
mov o5.zw, r0
mul o7.xyz, r1, c19.w
mad o6.xy, v4, c22, c22.zwzw
mul o7.w, -r0.x, r0.y
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 352
Vector 272 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityShadows" 416
Vector 400 [unity_ShadowFadeCenterAndType]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 192 [_Object2World]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityShadows" 2
BindCB  "UnityPerDraw" 3
"vs_4_0
eefiecedjpmokdpiiapdmlfllcccahnpmemdiohdabaaaaaaliaiaaaaadaaaaaa
cmaaaaaapeaaaaaanmabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheooaaaaaaaaiaaaaaa
aiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaaneaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaaneaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapaaaaaaneaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadamaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefc
neagaaaaeaaaabaalfabaaaafjaaaaaeegiocaaaaaaaaaaabeaaaaaafjaaaaae
egiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaabkaaaaaafjaaaaae
egiocaaaadaaaaaabfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaa
abaaaaaafpaaaaadhcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaad
dcbabaaaaeaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaa
abaaaaaagfaaaaadiccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaad
pccabaaaadaaaaaagfaaaaadpccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaa
gfaaaaadpccabaaaagaaaaaagiaaaaacaeaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaadaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaadaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaadaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaadaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaadgaaaaafecaabaaaabaaaaaa
akbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaa
abaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaabaaaaaaiiccabaaaabaaaaaa
egiccaaaaaaaaaaabbaaaaaaegacbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaa
jgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaakhcaabaaaacaaaaaajgbebaaa
acaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaaacaaaaaadiaaaaahhcaabaaa
acaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaadiaaaaajhcaabaaaadaaaaaa
fgifcaaaabaaaaaaaeaaaaaaegiccaaaadaaaaaabbaaaaaadcaaaaalhcaabaaa
adaaaaaaegiccaaaadaaaaaabaaaaaaaagiacaaaabaaaaaaaeaaaaaaegacbaaa
adaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaadaaaaaabcaaaaaakgikcaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaaa
adaaaaaaegiccaaaadaaaaaabdaaaaaadcaaaaalhcaabaaaadaaaaaaegacbaaa
adaaaaaapgipcaaaadaaaaaabeaaaaaaegbcbaiaebaaaaaaaaaaaaaabaaaaaah
cccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbccabaaa
abaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaabaaaaaaheccabaaaabaaaaaa
egbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaahbcaabaaaacaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaeeaaaaafbcaabaaaacaaaaaaakaabaaaacaaaaaa
diaaaaahhcaabaaaacaaaaaaagaabaaaacaaaaaaegacbaaaadaaaaaabaaaaaah
bcaabaaaacaaaaaaegacbaaaacaaaaaaegbcbaaaacaaaaaaaaaaaaaiiccabaaa
acaaaaaaakaabaiaebaaaaaaacaaaaaaabeaaaaaaaaaiadpdgaaaaaghccabaaa
acaaaaaaegacbaiaibaaaaaaabaaaaaadiaaaaaibcaabaaaacaaaaaabkbabaaa
aaaaaaaackiacaaaadaaaaaaafaaaaaadcaaaaakbcaabaaaacaaaaaackiacaaa
adaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaacaaaaaadcaaaaakbcaabaaa
acaaaaaackiacaaaadaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaacaaaaaa
dcaaaaakbcaabaaaacaaaaaackiacaaaadaaaaaaahaaaaaadkbabaaaaaaaaaaa
akaabaaaacaaaaaadgaaaaagicaabaaaabaaaaaaakaabaiaebaaaaaaacaaaaaa
dgaaaaafpccabaaaadaaaaaaegaobaaaabaaaaaadiaaaaaiccaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaadiaaaaakncaabaaaacaaaaaa
agahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaaaaaaaadpaaaaaadpdgaaaaaf
mccabaaaaeaaaaaakgaobaaaaaaaaaaaaaaaaaahdccabaaaaeaaaaaakgakbaaa
acaaaaaamgaabaaaacaaaaaadcaaaaaldccabaaaafaaaaaaegbabaaaaeaaaaaa
egiacaaaaaaaaaaabdaaaaaaogikcaaaaaaaaaaabdaaaaaaaaaaaaajbcaabaaa
aaaaaaaadkiacaiaebaaaaaaacaaaaaabjaaaaaaabeaaaaaaaaaiadpdiaaaaah
iccabaaaagaaaaaaakaabaaaaaaaaaaadkaabaaaabaaaaaadiaaaaaihcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiccaaaadaaaaaaanaaaaaadcaaaaakhcaabaaa
aaaaaaaaegiccaaaadaaaaaaamaaaaaaagbabaaaaaaaaaaaegacbaaaaaaaaaaa
dcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaaaoaaaaaakgbkbaaaaaaaaaaa
egacbaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegiccaaaadaaaaaaapaaaaaa
pgbpbaaaaaaaaaaaegacbaaaaaaaaaaaaaaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaiaebaaaaaaacaaaaaabjaaaaaadiaaaaaihccabaaaagaaaaaa
egacbaaaaaaaaaaapgipcaaaacaaaaaabjaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" ATTR14
Matrix 9 [_World2Object]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_ProjectionParams]
Vector 15 [unity_Scale]
Vector 16 [_sunLightDirection]
Vector 17 [unity_LightmapST]
"3.0-!!ARBvp1.0
PARAM c[18] = { { 1, 0.5 },
		state.matrix.modelview[0],
		state.matrix.mvp,
		program.local[9..17] };
TEMP R0;
TEMP R1;
TEMP R2;
MOV R1.w, c[0].x;
MOV R1.xyz, c[13];
DP4 R0.z, R1, c[11];
DP4 R0.x, R1, c[9];
DP4 R0.y, R1, c[10];
MAD R0.xyz, R0, c[15].w, -vertex.position;
DP3 R0.w, R0, R0;
MOV R1.xyz, vertex.attrib[14];
MUL R2.xyz, vertex.normal.zxyw, R1.yzxw;
MAD R1.xyz, vertex.normal.yzxw, R1.zxyw, -R2;
MUL R1.xyz, R1, vertex.attrib[14].w;
DP3 result.texcoord[0].y, R0, R1;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
DP3 R0.w, vertex.normal, R2;
DP3 result.texcoord[0].z, vertex.normal, R0;
DP3 result.texcoord[0].x, R0, vertex.attrib[14];
DP4 R1.w, vertex.position, c[8];
DP4 R1.z, vertex.position, c[7];
DP4 R1.x, vertex.position, c[5];
DP4 R1.y, vertex.position, c[6];
MUL R2.xyz, R1.xyww, c[0].y;
MOV result.position, R1;
MUL R0.y, R2, c[14].x;
MOV R0.x, R2;
ADD result.texcoord[4].xy, R0, R2.z;
MOV R0.xy, vertex.texcoord[0];
MOV R0.z, vertex.texcoord[1].x;
ADD result.texcoord[1].w, -R0, c[0].x;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
DP4 R1.x, vertex.position, c[3];
MOV R0.w, -R1.x;
ABS result.texcoord[1].xyz, R0;
MOV result.texcoord[2], R0;
DP3 result.texcoord[3].x, R0, c[16];
MOV result.texcoord[4].zw, R1;
MAD result.texcoord[5].xy, vertex.texcoord[1], c[17], c[17].zwzw;
END
# 39 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
Matrix 0 [glstate_matrix_modelview0]
Matrix 4 [glstate_matrix_mvp]
Matrix 8 [_World2Object]
Vector 12 [_WorldSpaceCameraPos]
Vector 13 [_ProjectionParams]
Vector 14 [_ScreenParams]
Vector 15 [unity_Scale]
Vector 16 [_sunLightDirection]
Vector 17 [unity_LightmapST]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_texcoord1 o2
dcl_texcoord2 o3
dcl_texcoord3 o4
dcl_texcoord4 o5
dcl_texcoord5 o6
def c18, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
dcl_tangent0 v1
dcl_normal0 v2
dcl_texcoord0 v3
dcl_texcoord1 v4
mov r1.w, c18.x
mov r1.xyz, c12
dp4 r0.z, r1, c10
dp4 r0.x, r1, c8
dp4 r0.y, r1, c9
mad r1.xyz, r0, c15.w, -v0
dp3 r0.w, r1, r1
mov r0.xyz, v1
mul r2.xyz, v2.zxyw, r0.yzxw
mov r0.xyz, v1
mad r0.xyz, v2.yzxw, r0.zxyw, -r2
mul r0.xyz, r0, v1.w
dp3 o1.y, r1, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r1
dp3 r0.w, v2, r2
add o2.w, -r0, c18.x
dp3 o1.z, v2, r1
dp3 o1.x, r1, v1
dp4 r0.w, v0, c7
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mul r2.xyz, r0.xyww, c18.y
mov o0, r0
mul r1.y, r2, c13.x
mov r1.x, r2
mad o5.xy, r2.z, c14.zwzw, r1
mov r1.xy, v3
mov r1.z, v4.x
dp3 r1.w, r1, r1
rsq r0.x, r1.w
mul r1.xyz, r0.x, r1
dp4 r0.y, v0, c2
mov r1.w, -r0.y
abs o2.xyz, r1
mov o3, r1
dp3 o4.x, r1, c16
mov o5.zw, r0
mad o6.xy, v4, c17, c17.zwzw
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Bind "vertex" Vertex
Bind "color" Color
Bind "normal" Normal
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "tangent" TexCoord2
ConstBuffer "$Globals" 352
Vector 272 [_sunLightDirection]
Vector 304 [unity_LightmapST]
ConstBuffer "UnityPerCamera" 128
Vector 64 [_WorldSpaceCameraPos] 3
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
Matrix 64 [glstate_matrix_modelview0]
Matrix 256 [_World2Object]
Vector 320 [unity_Scale]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
BindCB  "UnityPerDraw" 2
"vs_4_0
eefiecedbcfmodgpaogobicjacmmlofomgkcokhjabaaaaaagiahaaaaadaaaaaa
cmaaaaaapeaaaaaameabaaaaejfdeheomaaaaaaaagaaaaaaaiaaaaaajiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaakbaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaakjaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
ahahaaaalaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaadaaaaaaapadaaaalaaaaaaa
abaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapadaaaaljaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapaaaaaafaepfdejfeejepeoaafeebeoehefeofeaaeoepfc
enebemaafeeffiedepepfceeaaedepemepfcaaklepfdeheomiaaaaaaahaaaaaa
aiaaaaaalaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaabaaaaaaahaiaaaalmaaaaaaadaaaaaaaaaaaaaa
adaaaaaaabaaaaaaaiahaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaalmaaaaaaacaaaaaaaaaaaaaaadaaaaaaadaaaaaaapaaaaaalmaaaaaa
aeaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapaaaaaalmaaaaaaafaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadamaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklfdeieefcjmafaaaaeaaaabaaghabaaaafjaaaaaeegiocaaaaaaaaaaa
beaaaaaafjaaaaaeegiocaaaabaaaaaaagaaaaaafjaaaaaeegiocaaaacaaaaaa
bfaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaadpcbabaaaabaaaaaafpaaaaad
hcbabaaaacaaaaaafpaaaaaddcbabaaaadaaaaaafpaaaaaddcbabaaaaeaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadhccabaaaabaaaaaagfaaaaad
iccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaadpccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaagiaaaaacaeaaaaaa
diaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaacaaaaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaaaaaaaaaagbabaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaacaaaaaaacaaaaaa
kgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
acaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdcaabaaaabaaaaaaegbabaaaadaaaaaa
dgaaaaafecaabaaaabaaaaaaakbabaaaaeaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaabaaaaaaegacbaaaabaaaaaa
baaaaaaiiccabaaaabaaaaaaegiccaaaaaaaaaaabbaaaaaaegacbaaaabaaaaaa
diaaaaahhcaabaaaacaaaaaajgbebaaaabaaaaaacgbjbaaaacaaaaaadcaaaaak
hcaabaaaacaaaaaajgbebaaaacaaaaaacgbjbaaaabaaaaaaegacbaiaebaaaaaa
acaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaapgbpbaaaabaaaaaa
diaaaaajhcaabaaaadaaaaaafgifcaaaabaaaaaaaeaaaaaaegiccaaaacaaaaaa
bbaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaaacaaaaaabaaaaaaaagiacaaa
abaaaaaaaeaaaaaaegacbaaaadaaaaaadcaaaaalhcaabaaaadaaaaaaegiccaaa
acaaaaaabcaaaaaakgikcaaaabaaaaaaaeaaaaaaegacbaaaadaaaaaaaaaaaaai
hcaabaaaadaaaaaaegacbaaaadaaaaaaegiccaaaacaaaaaabdaaaaaadcaaaaal
hcaabaaaadaaaaaaegacbaaaadaaaaaapgipcaaaacaaaaaabeaaaaaaegbcbaia
ebaaaaaaaaaaaaaabaaaaaahcccabaaaabaaaaaaegacbaaaacaaaaaaegacbaaa
adaaaaaabaaaaaahbccabaaaabaaaaaaegbcbaaaabaaaaaaegacbaaaadaaaaaa
baaaaaaheccabaaaabaaaaaaegbcbaaaacaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaa
abaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaabaaaaaa
egacbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaacaaaaaaegbcbaaa
acaaaaaaaaaaaaaiiccabaaaacaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaa
aaaaiadpdgaaaaaghccabaaaacaaaaaaegacbaiaibaaaaaaabaaaaaadgaaaaaf
hccabaaaadaaaaaaegacbaaaabaaaaaadiaaaaaibcaabaaaabaaaaaabkbabaaa
aaaaaaaackiacaaaacaaaaaaafaaaaaadcaaaaakbcaabaaaabaaaaaackiacaaa
acaaaaaaaeaaaaaaakbabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaa
abaaaaaackiacaaaacaaaaaaagaaaaaackbabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaakbcaabaaaabaaaaaackiacaaaacaaaaaaahaaaaaadkbabaaaaaaaaaaa
akaabaaaabaaaaaadgaaaaagiccabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaa
diaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaabaaaaaaafaaaaaa
diaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaaaa
aaaaaadpaaaaaadpdgaaaaafmccabaaaaeaaaaaakgaobaaaaaaaaaaaaaaaaaah
dccabaaaaeaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaadcaaaaaldccabaaa
afaaaaaaegbabaaaaeaaaaaaegiacaaaaaaaaaaabdaaaaaaogikcaaaaaaaaaaa
bdaaaaaadoaaaaab"
}
}
Program "fp" {
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
"3.0-!!ARBfp1.0
PARAM c[19] = { program.local[0..16],
		{ 1, 2, 3, 2.718282 },
		{ 0.5 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[12].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MUL R2.w, fragment.texcoord[2], c[15].x;
MUL R2.w, R2, c[14].x;
POW R2.w, c[17].w, R2.w;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.xyz, R0, -R2;
MAD R1.xyz, R1, c[2].x, R2;
MOV R0.x, c[10];
ADD R0.x, -R0, c[11];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].w, -c[10];
MUL R1.xyz, R1, c[4];
MUL_SAT R0.w, R0.x, R0.y;
ADD R0.xyz, R1, -c[5];
MUL R1.x, R0.w, R0.w;
MAD R0.w, -R0, c[17].y, c[17].z;
MAD R1.w, -R1.x, R0, c[17].x;
MUL R1.y, fragment.texcoord[2].x, c[15].x;
MUL R0.w, R1.y, c[14].x;
MAD R0.xyz, R1.w, R0, c[5];
POW R0.w, c[17].w, R0.w;
ADD R1.xyz, -R0, c[13];
ADD R0.w, -R0, c[17].x;
MAD R1.xyz, R0.w, R1, R0;
TXP R0, fragment.texcoord[4], texture[3], 2D;
ADD R2.w, -R2, c[17].x;
DP3 R3.x, fragment.texcoord[0], fragment.texcoord[0];
MOV R2.y, c[18].x;
MOV R2.x, fragment.texcoord[3];
TEX R2.xyz, R2, texture[2], 2D;
ADD R2.xyz, R2, -R1;
MAD R1.xyz, R2.w, R2, R1;
LG2 R2.w, R0.w;
RSQ R0.w, R3.x;
MOV R3.x, c[17];
MAD R3.y, -R0.w, fragment.texcoord[0].z, c[17].x;
ADD R0.w, R3.x, -c[16].x;
MUL R3.y, R3, c[8].x;
ADD R3.z, R3.x, -c[7].x;
POW R3.x, R3.y, c[9].x;
MAD R3.y, R3.x, R3.z, c[7].x;
MUL R3.x, R0.w, c[6];
ADD R3.y, R3, -c[17].x;
MAD R1.w, R1, R3.y, c[17].x;
MUL R2.w, -R2, R3.x;
MUL R0.w, R0, R1;
LG2 R0.x, R0.x;
LG2 R0.y, R0.y;
LG2 R0.z, R0.z;
ADD R0.xyz, -R0, fragment.texcoord[5];
MUL R2.xyz, R0, c[1];
MUL R2.xyz, R2.w, R2;
MAD result.color.xyz, R1, R0, R2;
MAD result.color.w, R2, c[1], R0;
END
# 67 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c17, 1.00000000, 2.00000000, 3.00000000, -1.00000000
def c18, 2.71828198, 0.50000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4
dcl_texcoord5 v5.xyz
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c12.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mad r1.xyz, r1, c2.x, r3
mov r0.x, c11
add r0.x, -c10, r0
rcp r0.y, r0.x
mul r1.xyz, r1, c4
add r0.x, v2.w, -c10
mul_sat r0.x, r0, r0.y
mad r0.y, -r0.x, c17, c17.z
mul r0.x, r0, r0
mad r1.w, -r0.x, r0.y, c17.x
mul r0.x, v2, c15
mul r2.w, r0.x, c14.x
pow r0, c18.x, r2.w
mul r0.y, v2.w, c15.x
add r1.xyz, r1, -c5
mad r1.xyz, r1.w, r1, c5
add r2.xyz, -r1, c13
mul r2.w, r0.y, c14.x
mov r3.x, r0
pow r0, c18.x, r2.w
add r0.y, -r3.x, c17.x
mad r1.xyz, r0.y, r2, r1
dp3 r0.z, v0, v0
rsq r0.w, r0.z
mad r0.w, -r0, v0.z, c17.x
add r0.x, -r0, c17
mov r2.y, c18
mov r2.x, v3
texld r2.xyz, r2, s2
add r3.xyz, r2, -r1
texldp r2, v4, s3
mad r1.xyz, r0.x, r3, r1
log_pp r0.x, r2.x
log_pp r0.y, r2.y
log_pp r0.z, r2.z
add_pp r2.xyz, -r0, v5
mul r3.w, r0, c8.x
pow r0, r3.w, c9.x
mov r0.y, c7.x
add r0.y, c17.x, -r0
mad r0.y, r0.x, r0, c7.x
mov r0.x, c16
add r0.w, c17.x, -r0.x
add r0.y, r0, c17.w
mad r1.w, r1, r0.y, c17.x
mul r0.y, r0.w, c6.x
log_pp r0.x, r2.w
mul_pp r2.w, -r0.x, r0.y
mul_pp r3.xyz, r2, c1
mul_pp r0.xyz, r2.w, r3
mul r0.w, r0, r1
mad_pp oC0.xyz, r1, r2, r0
mad_pp oC0.w, r2, c1, r0
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
ConstBuffer "$Globals" 320
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefieceddagbbgmbpfiokciooglbhdoiehphilnoabaaaaaapmajaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcmeaiaaaaeaaaaaaadbacaaaa
fjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaa
fibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaa
fibiaaaeaahabaaaadaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaad
icbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaa
gcbaaaadlcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagfaaaaadpccabaaa
aaaaaaaagiaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaa
adaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
adaaaaaafgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaa
abaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaa
abaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaa
egacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaagiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaa
egiccaiaebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaia
ebaaaaaaaaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaa
aaaaaaajbcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaa
mgbabaaaadaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaa
egaabaaaacaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaa
egaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaaf
dcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaia
ebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaaj
hcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaafbcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaa
abeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
acaaaaaaaagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaa
aeaaaaaapgbpbaaaaeaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaacpaaaaafpcaabaaaabaaaaaaegaobaaa
abaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaabaaaaaaegbcbaaa
afaaaaaadiaaaaaihcaabaaaacaaaaaaegacbaaaabaaaaaaegiccaaaaaaaaaaa
acaaaaaaaaaaaaajicaabaaaacaaaaaaakiacaiaebaaaaaaaaaaaaaabcaaaaaa
abeaaaaaaaaaiadpdiaaaaaibcaabaaaadaaaaaadkaabaaaacaaaaaabkiacaaa
aaaaaaaaaiaaaaaadiaaaaaiicaabaaaabaaaaaadkaabaiaebaaaaaaabaaaaaa
akaabaaaadaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaabaaaaaaegacbaaa
acaaaaaadiaaaaaiicaabaaaabaaaaaadkaabaaaabaaaaaadkiacaaaaaaaaaaa
acaaaaaadcaaaaajhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaa
egacbaaaacaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaabaaaaaaegbcbaaa
abaaaaaaeeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaa
aaaaaaaackbabaiaebaaaaaaabaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadp
diaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaa
cpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaa
akaabaaaaaaaaaaackiacaaaaaaaaaaaamaaaaaabjaaaaafbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaa
amaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaaakaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajiccabaaaaaaaaaaa
akaabaaaaaaaaaaadkaabaaaacaaaaaadkaabaaaabaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
Vector 17 [unity_LightmapFade]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"3.0-!!ARBfp1.0
PARAM c[20] = { program.local[0..17],
		{ 1, 2, 3, 2.718282 },
		{ 0.5, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[12].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.xyz, R0, -R2;
MAD R1.xyz, R1, c[2].x, R2;
MUL R0.z, fragment.texcoord[2].x, c[15].x;
MUL R0.w, R0.z, c[14].x;
MOV R0.x, c[10];
ADD R0.x, -R0, c[11];
RCP R0.y, R0.x;
MUL R1.xyz, R1, c[4];
ADD R0.x, fragment.texcoord[2].w, -c[10];
MUL_SAT R0.x, R0, R0.y;
MUL R0.y, R0.x, R0.x;
MAD R0.x, -R0, c[18].y, c[18].z;
POW R0.w, c[18].w, R0.w;
MAD R2.w, -R0.y, R0.x, c[18].x;
ADD R1.xyz, R1, -c[5];
MAD R0.xyz, R2.w, R1, c[5];
ADD R1.xyz, -R0, c[13];
ADD R0.w, -R0, c[18].x;
MAD R2.xyz, R0.w, R1, R0;
TEX R1, fragment.texcoord[5], texture[5], 2D;
MUL R1.xyz, R1.w, R1;
MUL R1.xyz, R1, c[19].y;
MOV R0.y, c[19].x;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R3.xyz, R0, -R2;
TEX R0, fragment.texcoord[5], texture[4], 2D;
MUL R0.xyz, R0.w, R0;
DP4 R0.w, fragment.texcoord[6], fragment.texcoord[6];
RSQ R0.w, R0.w;
RCP R1.w, R0.w;
MAD R4.xyz, R0, c[19].y, -R1;
TXP R0, fragment.texcoord[4], texture[3], 2D;
MAD_SAT R1.w, R1, c[17].z, c[17];
MAD R1.xyz, R1.w, R4, R1;
MUL R1.w, fragment.texcoord[2], c[15].x;
MUL R1.w, R1, c[14].x;
POW R1.w, c[18].w, R1.w;
LG2 R0.x, R0.x;
LG2 R0.y, R0.y;
LG2 R0.z, R0.z;
ADD R0.xyz, -R0, R1;
ADD R1.x, -R1.w, c[18];
MAD R1.xyz, R1.x, R3, R2;
DP3 R3.x, fragment.texcoord[0], fragment.texcoord[0];
LG2 R1.w, R0.w;
RSQ R0.w, R3.x;
MOV R3.x, c[18];
MAD R3.y, -R0.w, fragment.texcoord[0].z, c[18].x;
ADD R0.w, R3.x, -c[16].x;
MUL R3.y, R3, c[8].x;
ADD R3.z, R3.x, -c[7].x;
POW R3.x, R3.y, c[9].x;
MAD R3.y, R3.x, R3.z, c[7].x;
MUL R3.x, R0.w, c[6];
ADD R3.y, R3, -c[18].x;
MAD R2.w, R2, R3.y, c[18].x;
MUL R1.w, -R1, R3.x;
MUL R2.xyz, R0, c[1];
MUL R2.xyz, R1.w, R2;
MUL R0.w, R0, R2;
MAD result.color.xyz, R1, R0, R2;
MAD result.color.w, R1, c[1], R0;
END
# 78 instructions, 5 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
Vector 17 [unity_LightmapFade]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
def c18, 1.00000000, 2.00000000, 3.00000000, -1.00000000
def c19, 2.71828198, 0.50000000, 8.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
dcl_texcoord6 v6
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c12.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mad r1.xyz, r1, c2.x, r3
mov r0.x, c11
add r0.x, -c10, r0
rcp r0.y, r0.x
mul r1.xyz, r1, c4
add r0.x, v2.w, -c10
mul_sat r0.x, r0, r0.y
mad r0.y, -r0.x, c18, c18.z
mul r0.x, r0, r0
mad r2.w, -r0.x, r0.y, c18.x
mul r0.x, v2, c15
mul r3.x, r0, c14
add r1.xyz, r1, -c5
mad r1.xyz, r2.w, r1, c5
pow r0, c19.x, r3.x
mul r1.w, v2, c15.x
mul r0.y, r1.w, c14.x
pow r3, c19.x, r0.y
mov r0.w, r3.x
add r2.xyz, -r1, c13
add r0.x, -r0, c18
mad r1.xyz, r0.x, r2, r1
add r0.w, -r0, c18.x
mov r0.y, c19
mov r0.x, v3
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r2.xyz, r0.w, r0, r1
texld r0, v5, s4
texld r1, v5, s5
mul_pp r0.xyz, r0.w, r0
mul_pp r1.xyz, r1.w, r1
mul_pp r3.xyz, r1, c19.z
texldp r1, v4, s3
dp4 r0.w, v6, v6
rsq r0.w, r0.w
rcp r0.w, r0.w
mad_pp r0.xyz, r0, c19.z, -r3
mad_sat r0.w, r0, c17.z, c17
mad_pp r0.xyz, r0.w, r0, r3
dp3 r0.w, v0, v0
rsq r0.w, r0.w
mad r0.w, -r0, v0.z, c18.x
log_pp r1.x, r1.x
log_pp r1.y, r1.y
log_pp r1.z, r1.z
add_pp r1.xyz, -r1, r0
mul r3.w, r0, c8.x
pow r0, r3.w, c9.x
mov r0.y, c7.x
add r0.y, c18.x, -r0
mad r0.y, r0.x, r0, c7.x
mov r0.x, c16
add r0.w, c18.x, -r0.x
add r0.y, r0, c18.w
mad r2.w, r2, r0.y, c18.x
mul r0.y, r0.w, c6.x
log_pp r0.x, r1.w
mul_pp r1.w, -r0.x, r0.y
mul_pp r3.xyz, r1, c1
mul_pp r0.xyz, r1.w, r3
mul r0.w, r0, r2
mad_pp oC0.xyz, r2, r1, r0
mad_pp oC0.w, r1, c1, r0
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_OFF" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
ConstBuffer "$Globals" 352
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
Vector 320 [unity_LightmapFade]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecededfiddnjfgegbghbbhgcfapkcefejbolabaaaaaajmalaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaaneaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaapalaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
adadaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaaapapaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcemakaaaaeaaaaaaajdacaaaafjaaaaaeegiocaaa
aaaaaaaabfaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
fibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaa
fibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaa
gcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaaabaaaaaagcbaaaadhcbabaaa
acaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadlcbabaaaaeaaaaaagcbaaaad
dcbabaaaafaaaaaagcbaaaadpcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaa
dkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaa
fgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
ggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaa
egaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaaj
pcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
agiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaaegiccaia
ebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaa
aaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
bcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaaj
bcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaa
adaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaa
acaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaa
acaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
acaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
bcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaa
aaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaa
aagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaabbaaaaahbcaabaaaabaaaaaaegbobaaaagaaaaaa
egbobaaaagaaaaaaelaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadccaaaal
bcaabaaaabaaaaaaakaabaaaabaaaaaackiacaaaaaaaaaaabeaaaaaadkiacaaa
aaaaaaaabeaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaafaaaaaaeghobaaa
afaaaaaaaagabaaaafaaaaaadiaaaaahccaabaaaabaaaaaadkaabaaaacaaaaaa
abeaaaaaaaaaaaebdiaaaaahocaabaaaabaaaaaaagajbaaaacaaaaaafgafbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaafaaaaaaeghobaaaaeaaaaaa
aagabaaaaeaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdcaaaaakhcaabaaaacaaaaaapgapbaaaacaaaaaaegacbaaaacaaaaaa
jgahbaiaebaaaaaaabaaaaaadcaaaaajhcaabaaaabaaaaaaagaabaaaabaaaaaa
egacbaaaacaaaaaajgahbaaaabaaaaaaaoaaaaahdcaabaaaacaaaaaaegbabaaa
aeaaaaaapgbpbaaaaeaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaacaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaacpaaaaafpcaabaaaacaaaaaaegaobaaa
acaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaiaebaaaaaa
acaaaaaadiaaaaaihcaabaaaacaaaaaaegacbaaaabaaaaaaegiccaaaaaaaaaaa
acaaaaaaaaaaaaajicaabaaaabaaaaaaakiacaiaebaaaaaaaaaaaaaabcaaaaaa
abeaaaaaaaaaiadpdiaaaaaibcaabaaaadaaaaaadkaabaaaabaaaaaabkiacaaa
aaaaaaaaaiaaaaaadiaaaaaiicaabaaaacaaaaaadkaabaiaebaaaaaaacaaaaaa
akaabaaaadaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaacaaaaaaegacbaaa
acaaaaaadiaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaadkiacaaaaaaaaaaa
acaaaaaadcaaaaajhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaa
egacbaaaacaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaabaaaaaaegbcbaaa
abaaaaaaeeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakbcaabaaa
aaaaaaaackbabaiaebaaaaaaabaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadp
diaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaaaaaaaaaaamaaaaaa
cpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaibcaabaaaaaaaaaaa
akaabaaaaaaaaaaackiacaaaaaaaaaaaamaaaaaabjaaaaafbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaa
amaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaaakaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajiccabaaaaaaaaaaa
akaabaaaaaaaaaaadkaabaaaabaaaaaadkaabaaaacaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"3.0-!!ARBfp1.0
PARAM c[22] = { program.local[0..17],
		{ 0.57735026, 8, -0.40824828, -0.70710677 },
		{ 0.81649655, 0, 0.57735026, 128 },
		{ -0.40824831, 0.70710677, 0.57735026, 1 },
		{ 2, 3, 2.718282, 0.5 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[13].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[2].x, R2;
MUL R1.xyz, R0, c[4];
MOV R0.x, c[11];
ADD R1.w, -R0.x, c[12].x;
RCP R2.x, R1.w;
TEX R0, fragment.texcoord[5], texture[5], 2D;
ADD R1.w, fragment.texcoord[2], -c[11].x;
MUL_SAT R1.w, R1, R2.x;
MUL R0.xyz, R0.w, R0;
MAD R2.x, -R1.w, c[21], c[21].y;
MUL R0.w, R1, R1;
MAD R2.w, -R0, R2.x, c[20];
ADD R1.xyz, R1, -c[5];
MAD R2.xyz, R2.w, R1, c[5];
MUL R1.xyz, R0, c[18].y;
MUL R0.xyz, R1.y, c[20];
MAD R0.xyz, R1.x, c[19], R0;
MUL R0.w, fragment.texcoord[2].x, c[16].x;
MUL R0.w, R0, c[15].x;
POW R1.w, c[21].z, R0.w;
MAD R0.xyz, R1.z, c[18].zwxw, R0;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
MUL R0.w, fragment.texcoord[2], c[16].x;
MUL R0.w, R0, c[15].x;
ADD R3.xyz, -R2, c[14];
ADD R1.w, -R1, c[20];
MAD R2.xyz, R1.w, R3, R2;
DP3 R1.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R1.w, R1.w;
MAD R0.xyz, R1.w, fragment.texcoord[0], R0;
DP3 R0.x, R0, R0;
POW R0.w, c[21].z, R0.w;
ADD R0.y, -R0.w, c[20].w;
MOV R3.y, c[21].w;
MOV R3.x, fragment.texcoord[3];
TEX R3.xyz, R3, texture[2], 2D;
ADD R3.xyz, R3, -R2;
MAD R2.xyz, R0.y, R3, R2;
RSQ R0.y, R0.x;
MUL R0.y, R0, R0.z;
MOV R0.x, c[19].w;
MUL R0.z, R0.x, c[6].x;
MAX R0.x, R0.y, c[19].y;
POW R1.w, R0.x, R0.z;
TEX R0, fragment.texcoord[5], texture[4], 2D;
MUL R0.xyz, R0.w, R0;
DP3 R1.x, R1, c[18].x;
MUL R1.xyz, R0, R1.x;
TXP R0, fragment.texcoord[4], texture[3], 2D;
MUL R1.xyz, R1, c[18].y;
LG2 R0.x, R0.x;
LG2 R0.y, R0.y;
LG2 R0.z, R0.z;
LG2 R0.w, R0.w;
ADD R0, -R0, R1;
MOV R1.x, c[20].w;
DP3 R1.y, fragment.texcoord[0], fragment.texcoord[0];
RSQ R1.y, R1.y;
MAD R1.z, fragment.texcoord[0], -R1.y, c[20].w;
ADD R1.w, R1.x, -c[17].x;
MUL R1.y, R1.w, c[7].x;
ADD R3.x, R1, -c[8];
MUL R1.z, R1, c[9].x;
POW R1.x, R1.z, c[10].x;
MAD R1.x, R1, R3, c[8];
ADD R3.x, R1, -c[20].w;
MUL R0.w, R0, R1.y;
MUL R1.xyz, R0, c[1];
MAD R2.w, R2, R3.x, c[20];
MUL R1.xyz, R0.w, R1;
MUL R1.w, R1, R2;
MAD result.color.xyz, R0, R2, R1;
MAD result.color.w, R0, c[1], R1;
END
# 91 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
def c18, 8.00000000, 0.57735026, -0.40824831, 0.70710677
def c19, 0.81649655, 0.00000000, 0.57735026, 128.00000000
def c20, -0.40824828, -0.70710677, 0.57735026, 1.00000000
def c21, 2.00000000, 3.00000000, -1.00000000, 2.71828198
def c22, 0.50000000, 0, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c13.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c2.x, r3
mul r1.xyz, r0, c4
texld r0, v5, s5
mul_pp r0.xyz, r0.w, r0
add r2.xyz, r1, -c5
mul_pp r1.xyz, r0, c18.x
mul r0.xyz, r1.y, c18.zwyw
mad r0.xyz, r1.x, c19, r0
mov r0.w, c12.x
add r0.w, -c11.x, r0
rcp r1.w, r0.w
add r0.w, v2, -c11.x
mul_sat r0.w, r0, r1
mad r1.w, -r0, c21.x, c21.y
mul r0.w, r0, r0
mad r4.w, -r0, r1, c20
mad r3.xyz, r4.w, r2, c5
mad r0.xyz, r1.z, c20, r0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r1.w, v2.x, c16.x
add r4.xyz, -r3, c14
mul r2.xyz, r0.w, r0
mul r1.w, r1, c15.x
pow r0, c21.w, r1.w
dp3_pp r0.y, v0, v0
rsq_pp r0.y, r0.y
mad_pp r0.yzw, r0.y, v0.xxyz, r2.xxyz
mov r1.w, r0.x
dp3_pp r0.x, r0.yzww, r0.yzww
add r0.y, -r1.w, c20.w
mad r2.xyz, r0.y, r4, r3
rsq_pp r1.w, r0.x
mov_pp r2.w, c6.x
mov r0.y, c22.x
mov r0.x, v3
texld r0.xyz, r0, s2
add r4.xyz, r0, -r2
mul_pp r0.x, r1.w, r0.w
mul r0.y, v2.w, c16.x
max_pp r1.w, r0.x, c19.y
mul r3.x, r0.y, c15
pow r0, c21.w, r3.x
mul_pp r0.y, c19.w, r2.w
pow r3, r1.w, r0.y
mov r2.w, r3
dp3 r0.w, v0, v0
rsq r1.w, r0.w
add r0.x, -r0, c20.w
dp3_pp r0.w, r1, c18.y
mad r0.xyz, r0.x, r4, r2
texld r3, v5, s4
mul_pp r2.xyz, r3.w, r3
texldp r3, v4, s3
mul_pp r1.xyz, r2, r0.w
mad r1.w, v0.z, -r1, c20
mul r0.w, r1, c9.x
mul_pp r2.xyz, r1, c18.x
pow r1, r0.w, c10.x
mov r1.z, r1.x
mov r1.x, c8
add r1.y, c20.w, -r1.x
mov r0.w, c17.x
add r0.w, c20, -r0
mul r1.x, r0.w, c7
mad r1.y, r1.z, r1, c8.x
log_pp r3.x, r3.x
log_pp r3.y, r3.y
log_pp r3.z, r3.z
log_pp r3.w, r3.w
add_pp r2, -r3, r2
mul_pp r1.w, r2, r1.x
add r2.w, r1.y, c21.z
mul_pp r1.xyz, r2, c1
mad r2.w, r4, r2, c20
mul_pp r1.xyz, r1.w, r1
mul r0.w, r0, r2
mad_pp oC0.xyz, r2, r0, r1
mad_pp oC0.w, r1, c1, r0
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_OFF" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
ConstBuffer "$Globals" 352
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 128 [_Shininess]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedpachmllamdcccaljmehbfnamgmadkiffabaaaaaapaamaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
adadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefclialaaaaeaaaaaaaooacaaaa
fjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaad
aagabaaaafaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaa
afaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadlcbabaaa
aeaaaaaagcbaaaaddcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
afaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaadkiacaaa
abaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaafgifcaaa
aaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaggakbaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
egacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaakgbkbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaagiacaaa
aaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaa
aaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaaegiccaiaebaaaaaa
aaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaajbcaabaaa
abaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaadicaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaajbcaabaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaa
aaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaa
aaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaaadaaaaaa
agiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaaacaaaaaa
fgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaaacaaaaaa
aceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaaacaaaaaa
egaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaaacaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaaaaaaaaaa
agaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaa
abaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadp
efaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaaaagabaaa
acaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaaabaaaaaa
egacbaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaafaaaaaaeghobaaa
afaaaaaaaagabaaaafaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaa
abeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgapbaaa
abaaaaaadiaaaaakhcaabaaaacaaaaaafgafbaaaabaaaaaaaceaaaaaomafnblo
pdaedfdpdkmnbddpaaaaaaaadcaaaaamhcaabaaaacaaaaaaagaabaaaabaaaaaa
aceaaaaaolaffbdpaaaaaaaadkmnbddpaaaaaaaaegacbaaaacaaaaaadcaaaaam
hcaabaaaacaaaaaakgakbaaaabaaaaaaaceaaaaaolafnblopdaedflpdkmnbddp
aaaaaaaaegacbaaaacaaaaaabaaaaaakbcaabaaaabaaaaaaaceaaaaadkmnbddp
dkmnbddpdkmnbddpaaaaaaaaegacbaaaabaaaaaabaaaaaahccaabaaaabaaaaaa
egacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaafccaabaaaabaaaaaabkaabaaa
abaaaaaabaaaaaahecaabaaaabaaaaaaegbcbaaaabaaaaaaegbcbaaaabaaaaaa
eeaaaaafecaabaaaabaaaaaackaabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaa
kgakbaaaabaaaaaaegbcbaaaabaaaaaadcaaaaakecaabaaaabaaaaaackbabaia
ebaaaaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaiadpdiaaaaaiecaabaaa
abaaaaaackaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaacpaaaaafecaabaaa
abaaaaaackaabaaaabaaaaaadiaaaaaiecaabaaaabaaaaaackaabaaaabaaaaaa
ckiacaaaaaaaaaaaamaaaaaabjaaaaafecaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaacaaaaaafgafbaaaabaaaaaaegacbaaa
adaaaaaabaaaaaahccaabaaaabaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaa
eeaaaaafccaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaa
bkaabaaaabaaaaaackaabaaaacaaaaaadeaaaaahccaabaaaabaaaaaabkaabaaa
abaaaaaaabeaaaaaaaaaaaaacpaaaaafccaabaaaabaaaaaabkaabaaaabaaaaaa
diaaaaaiicaabaaaabaaaaaaakiacaaaaaaaaaaaaiaaaaaaabeaaaaaaaaaaaed
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaabjaaaaaf
icaabaaaacaaaaaabkaabaaaabaaaaaaaoaaaaahkcaabaaaabaaaaaaagbebaaa
aeaaaaaapgbpbaaaaeaaaaaaefaaaaajpcaabaaaadaaaaaangafbaaaabaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaacpaaaaafpcaabaaaadaaaaaaegaobaaa
adaaaaaaefaaaaajpcaabaaaaeaaaaaaegbabaaaafaaaaaaeghobaaaaeaaaaaa
aagabaaaaeaaaaaadiaaaaahccaabaaaabaaaaaadkaabaaaaeaaaaaaabeaaaaa
aaaaaaebdiaaaaahhcaabaaaaeaaaaaaegacbaaaaeaaaaaafgafbaaaabaaaaaa
diaaaaahhcaabaaaacaaaaaaagaabaaaabaaaaaaegacbaaaaeaaaaaaaaaaaaai
pcaabaaaacaaaaaaegaobaaaacaaaaaaegaobaiaebaaaaaaadaaaaaadiaaaaai
lcaabaaaabaaaaaaegaibaaaacaaaaaaegiicaaaaaaaaaaaacaaaaaaaaaaaaaj
bcaabaaaadaaaaaaakiacaiaebaaaaaaaaaaaaaabcaaaaaaabeaaaaaaaaaiadp
diaaaaaiccaabaaaadaaaaaaakaabaaaadaaaaaabkiacaaaaaaaaaaaaiaaaaaa
diaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaaadaaaaaadiaaaaah
lcaabaaaabaaaaaaegambaaaabaaaaaapgapbaaaacaaaaaadiaaaaaiicaabaaa
acaaaaaadkaabaaaacaaaaaadkiacaaaaaaaaaaaacaaaaaadcaaaaajhccabaaa
aaaaaaaaegacbaaaaaaaaaaaegacbaaaacaaaaaaegadbaaaabaaaaaaaaaaaaaj
bcaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaaamaaaaaaabeaaaaaaaaaiadp
dcaaaaakbcaabaaaaaaaaaaackaabaaaabaaaaaaakaabaaaaaaaaaaaakiacaaa
aaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaa
aaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaajiccabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaa
adaaaaaadkaabaaaacaaaaaadoaaaaab"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
"3.0-!!ARBfp1.0
PARAM c[19] = { program.local[0..16],
		{ 1, 2, 3, 2.718282 },
		{ 0.5 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[12].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MUL R0.w, fragment.texcoord[2].x, c[15].x;
MUL R0.w, R0, c[14].x;
POW R1.w, c[17].w, R0.w;
MUL R0.w, fragment.texcoord[2], c[15].x;
MUL R0.w, R0, c[14].x;
POW R0.w, c[17].w, R0.w;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.xyz, R0, -R2;
MAD R1.xyz, R1, c[2].x, R2;
DP3 R2.y, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.z, R2.y;
MOV R0.x, c[10];
ADD R0.x, -R0, c[11];
RCP R0.y, R0.x;
MUL R1.xyz, R1, c[4];
ADD R0.x, fragment.texcoord[2].w, -c[10];
MUL_SAT R0.x, R0, R0.y;
MAD R0.y, -R0.x, c[17], c[17].z;
MUL R0.x, R0, R0;
MAD R2.w, -R0.x, R0.y, c[17].x;
ADD R1.xyz, R1, -c[5];
MAD R0.xyz, R2.w, R1, c[5];
ADD R1.xyz, -R0, c[13];
ADD R1.w, -R1, c[17].x;
MAD R1.xyz, R1.w, R1, R0;
MOV R2.x, c[17];
ADD R0.w, -R0, c[17].x;
MAD R2.z, -R2, fragment.texcoord[0], c[17].x;
MOV R0.y, c[18].x;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R0.xyz, R0, -R1;
MAD R0.xyz, R0.w, R0, R1;
ADD R0.w, R2.x, -c[16].x;
TXP R1, fragment.texcoord[4], texture[3], 2D;
MUL R2.y, R0.w, c[6].x;
MUL R1.w, R1, R2.y;
MUL R2.y, R2.z, c[8].x;
ADD R2.z, R2.x, -c[7].x;
POW R2.x, R2.y, c[9].x;
MAD R2.x, R2, R2.z, c[7];
ADD R3.x, R2, -c[17];
ADD R1.xyz, R1, fragment.texcoord[5];
MUL R2.xyz, R1, c[1];
MAD R2.w, R2, R3.x, c[17].x;
MUL R2.xyz, R1.w, R2;
MUL R0.w, R0, R2;
MAD result.color.xyz, R0, R1, R2;
MAD result.color.w, R1, c[1], R0;
END
# 63 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c17, 1.00000000, 2.00000000, 3.00000000, -1.00000000
def c18, 2.71828198, 0.50000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4
dcl_texcoord5 v5.xyz
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c12.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c2.x, r3
mul r0.xyz, r0, c4
add r1.xyz, r0, -c5
mul r0.y, v2.x, c15.x
mov r0.x, c11
mul r2.x, r0.y, c14
add r1.w, -c10.x, r0.x
pow r0, c18.x, r2.x
rcp r0.z, r1.w
add r0.y, v2.w, -c10.x
mul_sat r0.y, r0, r0.z
mad r0.z, -r0.y, c17.y, c17
mul r0.y, r0, r0
mad r1.w, -r0.y, r0.z, c17.x
mov r0.w, r0.x
mad r1.xyz, r1.w, r1, c5
add r0.xyz, -r1, c13
add r0.w, -r0, c17.x
mad r1.xyz, r0.w, r0, r1
mov r0.y, c18
mov r0.x, v3
texld r0.xyz, r0, s2
add r2.xyz, r0, -r1
mul r0.y, v2.w, c15.x
dp3 r0.x, v0, v0
rsq r0.x, r0.x
mul r3.x, r0.y, c14
mad r2.w, -r0.x, v0.z, c17.x
pow r0, c18.x, r3.x
mul r0.y, r2.w, c8.x
pow r3, r0.y, c9.x
mov r0.y, c7.x
mov r0.z, r3.x
add r0.y, c17.x, -r0
mad r0.y, r0.z, r0, c7.x
mov r0.z, r0.x
add r0.x, r0.y, c17.w
mad r2.w, r1, r0.x, c17.x
add r0.y, -r0.z, c17.x
mad r1.xyz, r0.y, r2, r1
texldp r0, v4, s3
add_pp r0.xyz, r0, v5
mov r1.w, c16.x
add r1.w, c17.x, -r1
mul r3.x, r1.w, c6
mul_pp r0.w, r0, r3.x
mul_pp r2.xyz, r0, c1
mul_pp r2.xyz, r0.w, r2
mul r1.w, r1, r2
mad_pp oC0.xyz, r1, r0, r2
mad_pp oC0.w, r0, c1, r1
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
ConstBuffer "$Globals" 320
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedaemomlfenibbliabopgoemedkhdeeplbabaaaaaaoaajaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
ahahaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefckiaiaaaaeaaaaaaackacaaaa
fjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaa
fibiaaaeaahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaa
fibiaaaeaahabaaaadaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaad
icbabaaaabaaaaaagcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaa
gcbaaaadlcbabaaaaeaaaaaagcbaaaadhcbabaaaafaaaaaagfaaaaadpccabaaa
aaaaaaaagiaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaa
adaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaa
adaaaaaafgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaa
abaaaaaaggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaah
hcaabaaaabaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaa
abaaaaaadcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaa
egacbaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaa
agbabaaaacaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaa
adaaaaaakgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaa
egacbaaaaaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaa
abaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaagiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaa
egiccaiaebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaia
ebaaaaaaaaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaa
aaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaa
aaaaaaajbcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaadicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaa
dcaaaaajbcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaa
aaaaeaeadiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaa
dcaaaaakicaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaa
abeaaaaaaaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaaegiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaa
mgbabaaaadaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaa
egaabaaaacaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaa
egaabaaaacaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaaf
dcaabaaaacaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaia
ebaaaaaaacaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaaj
hcaabaaaaaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaa
dgaaaaafbcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaa
abeaaaaaaaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
acaaaaaaaagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaa
egacbaaaabaaaaaaegacbaaaaaaaaaaaaoaaaaahdcaabaaaabaaaaaaegbabaaa
aeaaaaaapgbpbaaaaeaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaa
abaaaaaaegbcbaaaafaaaaaadiaaaaaihcaabaaaacaaaaaaegacbaaaabaaaaaa
egiccaaaaaaaaaaaacaaaaaaaaaaaaajicaabaaaacaaaaaaakiacaiaebaaaaaa
aaaaaaaabcaaaaaaabeaaaaaaaaaiadpdiaaaaaibcaabaaaadaaaaaadkaabaaa
acaaaaaabkiacaaaaaaaaaaaaiaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaa
abaaaaaaakaabaaaadaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaabaaaaaa
egacbaaaacaaaaaadiaaaaaiicaabaaaabaaaaaadkaabaaaabaaaaaadkiacaaa
aaaaaaaaacaaaaaadcaaaaajhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaabaaaaaa
egbcbaaaabaaaaaaeeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaak
bcaabaaaaaaaaaaackbabaiaebaaaaaaabaaaaaaakaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaaaaaaaaaa
amaaaaaacpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaaakaabaaaaaaaaaaackiacaaaaaaaaaaaamaaaaaabjaaaaafbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaaakiacaiaebaaaaaa
aaaaaaaaamaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajiccabaaa
aaaaaaaaakaabaaaaaaaaaaadkaabaaaacaaaaaadkaabaaaabaaaaaadoaaaaab
"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
Vector 17 [unity_LightmapFade]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"3.0-!!ARBfp1.0
PARAM c[20] = { program.local[0..17],
		{ 1, 2, 3, 2.718282 },
		{ 0.5, 8 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[12].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R1.xyz, R0, -R2;
MAD R1.xyz, R1, c[2].x, R2;
MOV R3.x, c[18];
MOV R0.x, c[10];
ADD R0.x, -R0, c[11];
RCP R0.y, R0.x;
ADD R0.x, fragment.texcoord[2].w, -c[10];
MUL R1.xyz, R1, c[4];
MUL_SAT R0.w, R0.x, R0.y;
ADD R0.xyz, R1, -c[5];
MUL R1.x, R0.w, R0.w;
MAD R0.w, -R0, c[18].y, c[18].z;
MAD R1.w, -R1.x, R0, c[18].x;
MUL R1.y, fragment.texcoord[2].x, c[15].x;
MUL R0.w, R1.y, c[14].x;
MAD R0.xyz, R1.w, R0, c[5];
POW R0.w, c[18].w, R0.w;
ADD R1.xyz, -R0, c[13];
ADD R0.w, -R0, c[18].x;
MAD R1.xyz, R0.w, R1, R0;
MUL R0.w, fragment.texcoord[2], c[15].x;
MUL R0.w, R0, c[14].x;
POW R2.w, c[18].w, R0.w;
ADD R2.w, -R2, c[18].x;
ADD R3.z, R3.x, -c[7].x;
MOV R0.y, c[19].x;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R2.xyz, R0, -R1;
MAD R1.xyz, R2.w, R2, R1;
TEX R0, fragment.texcoord[5], texture[4], 2D;
MUL R2.xyz, R0.w, R0;
TEX R0, fragment.texcoord[5], texture[5], 2D;
MUL R0.xyz, R0.w, R0;
MUL R0.xyz, R0, c[19].y;
DP4 R2.w, fragment.texcoord[6], fragment.texcoord[6];
RSQ R0.w, R2.w;
RCP R0.w, R0.w;
DP3 R2.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.w, R2.w;
MAD R3.y, -R2.w, fragment.texcoord[0].z, c[18].x;
MAD R2.xyz, R2, c[19].y, -R0;
MAD_SAT R0.w, R0, c[17].z, c[17];
MAD R2.xyz, R0.w, R2, R0;
TXP R0, fragment.texcoord[4], texture[3], 2D;
ADD R0.xyz, R0, R2;
ADD R2.w, R3.x, -c[16].x;
MUL R3.y, R3, c[8].x;
POW R3.x, R3.y, c[9].x;
MAD R3.y, R3.x, R3.z, c[7].x;
MUL R3.x, R2.w, c[6];
ADD R3.y, R3, -c[18].x;
MAD R1.w, R1, R3.y, c[18].x;
MUL R0.w, R0, R3.x;
MUL R2.xyz, R0, c[1];
MUL R2.xyz, R0.w, R2;
MUL R1.w, R2, R1;
MAD result.color.xyz, R1, R0, R2;
MAD result.color.w, R0, c[1], R1;
END
# 74 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Gloss]
Float 7 [_oceanOpacity]
Float 8 [_falloffPower]
Float 9 [_falloffExp]
Float 10 [_fadeStart]
Float 11 [_fadeEnd]
Float 12 [_tiling]
Vector 13 [_fogColor]
Float 14 [_heightDensityAtViewer]
Float 15 [_globalDensity]
Float 16 [_PlanetOpacity]
Vector 17 [unity_LightmapFade]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
def c18, 1.00000000, 2.00000000, 3.00000000, -1.00000000
def c19, 2.71828198, 0.50000000, 8.00000000, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
dcl_texcoord6 v6
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c12.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r1.xyz, r0, -r3
mad r1.xyz, r1, c2.x, r3
mov r0.x, c11
add r0.x, -c10, r0
rcp r0.y, r0.x
mul r1.xyz, r1, c4
add r0.x, v2.w, -c10
mul_sat r0.x, r0, r0.y
mad r0.y, -r0.x, c18, c18.z
mul r0.x, r0, r0
mad r2.w, -r0.x, r0.y, c18.x
mul r0.x, v2, c15
mul r3.x, r0, c14
add r1.xyz, r1, -c5
mad r1.xyz, r2.w, r1, c5
pow r0, c19.x, r3.x
mul r1.w, v2, c15.x
mul r0.y, r1.w, c14.x
pow r3, c19.x, r0.y
mov r0.w, r3.x
add r2.xyz, -r1, c13
add r0.x, -r0, c18
mad r1.xyz, r0.x, r2, r1
add r0.w, -r0, c18.x
mov r0.y, c19
mov r0.x, v3
texld r0.xyz, r0, s2
add r0.xyz, r0, -r1
mad r2.xyz, r0.w, r0, r1
texld r1, v5, s5
texld r0, v5, s4
mul_pp r1.xyz, r1.w, r1
mul_pp r0.xyz, r0.w, r0
mul_pp r1.xyz, r1, c19.z
dp4 r0.w, v6, v6
rsq r1.w, r0.w
dp3 r0.w, v0, v0
rcp r1.w, r1.w
rsq r0.w, r0.w
mad r0.w, -r0, v0.z, c18.x
mad_pp r0.xyz, r0, c19.z, -r1
mad_sat r1.w, r1, c17.z, c17
mad_pp r0.xyz, r1.w, r0, r1
texldp r1, v4, s3
mul r3.x, r0.w, c8
add_pp r1.xyz, r1, r0
pow r0, r3.x, c9.x
mov r0.z, r0.x
mov r0.x, c16
add r0.w, c18.x, -r0.x
mul r0.x, r0.w, c6
mov r0.y, c7.x
add r0.y, c18.x, -r0
mad r0.y, r0.z, r0, c7.x
add r0.y, r0, c18.w
mad r2.w, r2, r0.y, c18.x
mul_pp r1.w, r1, r0.x
mul_pp r3.xyz, r1, c1
mul_pp r0.xyz, r1.w, r3
mul r0.w, r0, r2
mad_pp oC0.xyz, r2, r1, r0
mad_pp oC0.w, r1, c1, r0
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "HDR_LIGHT_PREPASS_ON" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
ConstBuffer "$Globals" 352
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
Vector 320 [unity_LightmapFade]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedejlehencidcdcflmeddohfnfmajbbonmabaaaaaaiaalaaaaadaaaaaa
cmaaaaaabeabaaaaeiabaaaaejfdeheooaaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaaneaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaapalaaaaneaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
adadaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaagaaaaaaapapaaaafdfgfpfa
epfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaa
aiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfe
gbhcghgfheaaklklfdeieefcdaakaaaaeaaaaaaaimacaaaafjaaaaaeegiocaaa
aaaaaaaabfaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaadaagabaaaafaaaaaa
fibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaa
fibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaaadaaaaaaffffaaaa
fibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaaafaaaaaaffffaaaa
gcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaaabaaaaaagcbaaaadhcbabaaa
acaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadlcbabaaaaeaaaaaagcbaaaad
dcbabaaaafaaaaaagcbaaaadpcbabaaaagaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacaeaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaa
dkiacaaaabaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaa
fgifcaaaaaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaa
ggakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaa
egaabaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaaj
pcaabaaaacaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
dcaaaaajhcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaa
acaaaaaaefaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaa
kgbkbaaaacaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaa
aaaaaaaafgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaa
egacbaiaebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaa
agiacaaaaaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaaegiccaia
ebaaaaaaaaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaa
aaaaaaaaamaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaaaaaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaaj
bcaabaaaabaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaa
dicaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaaj
bcaabaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaea
diaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaak
icaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaiadpdcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaa
egiccaaaaaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaa
aaaaaaaaegiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaa
adaaaaaaagiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaa
acaaaaaafgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaa
acaaaaaaaceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaa
acaaaaaaegaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaa
acaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaaagaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
bcaabaaaabaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaa
aaaaaadpefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaa
aagabaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egacbaaaabaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaa
abaaaaaaegacbaaaaaaaaaaabbaaaaahbcaabaaaabaaaaaaegbobaaaagaaaaaa
egbobaaaagaaaaaaelaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadccaaaal
bcaabaaaabaaaaaaakaabaaaabaaaaaackiacaaaaaaaaaaabeaaaaaadkiacaaa
aaaaaaaabeaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaafaaaaaaeghobaaa
afaaaaaaaagabaaaafaaaaaadiaaaaahccaabaaaabaaaaaadkaabaaaacaaaaaa
abeaaaaaaaaaaaebdiaaaaahocaabaaaabaaaaaaagajbaaaacaaaaaafgafbaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaaafaaaaaaeghobaaaaeaaaaaa
aagabaaaaeaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaaabeaaaaa
aaaaaaebdcaaaaakhcaabaaaacaaaaaapgapbaaaacaaaaaaegacbaaaacaaaaaa
jgahbaiaebaaaaaaabaaaaaadcaaaaajhcaabaaaabaaaaaaagaabaaaabaaaaaa
egacbaaaacaaaaaajgahbaaaabaaaaaaaoaaaaahdcaabaaaacaaaaaaegbabaaa
aeaaaaaapgbpbaaaaeaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaacaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaadiaaaaaihcaabaaaacaaaaaaegacbaaaabaaaaaa
egiccaaaaaaaaaaaacaaaaaaaaaaaaajicaabaaaabaaaaaaakiacaiaebaaaaaa
aaaaaaaabcaaaaaaabeaaaaaaaaaiadpdiaaaaaibcaabaaaadaaaaaadkaabaaa
abaaaaaabkiacaaaaaaaaaaaaiaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaa
acaaaaaaakaabaaaadaaaaaadiaaaaahhcaabaaaacaaaaaapgapbaaaacaaaaaa
egacbaaaacaaaaaadiaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaadkiacaaa
aaaaaaaaacaaaaaadcaaaaajhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaa
abaaaaaaegacbaaaacaaaaaabaaaaaahbcaabaaaaaaaaaaaegbcbaaaabaaaaaa
egbcbaaaabaaaaaaeeaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaak
bcaabaaaaaaaaaaackbabaiaebaaaaaaabaaaaaaakaabaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaaibcaabaaaaaaaaaaaakaabaaaaaaaaaaabkiacaaaaaaaaaaa
amaaaaaacpaaaaafbcaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaibcaabaaa
aaaaaaaaakaabaaaaaaaaaaackiacaaaaaaaaaaaamaaaaaabjaaaaafbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaaaaaaaajccaabaaaaaaaaaaaakiacaiaebaaaaaa
aaaaaaaaamaaaaaaabeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaaakaabaaa
aaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaa
aaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaa
dkaabaaaaaaaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajiccabaaa
aaaaaaaaakaabaaaaaaaaaaadkaabaaaabaaaaaadkaabaaaacaaaaaadoaaaaab
"
}
SubProgram "opengl " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"3.0-!!ARBfp1.0
PARAM c[22] = { program.local[0..17],
		{ 0.57735026, 8, -0.40824828, -0.70710677 },
		{ 0.81649655, 0, 0.57735026, 128 },
		{ -0.40824831, 0.70710677, 0.57735026, 1 },
		{ 2, 3, 2.718282, 0.5 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
MOV R0.x, c[0].w;
MUL R0.x, R0, c[3];
MAD R3.xyz, fragment.texcoord[2], c[13].x, R0.x;
TEX R0.xyz, R3.zyzw, texture[0], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[0], 2D;
MAD R2.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[0], 2D;
MOV R0.w, c[11].x;
MAD R2.xyz, fragment.texcoord[1].y, R1, R2;
TEX R0.xyz, R3.zyzw, texture[1], 2D;
MUL R1.xyz, fragment.texcoord[1].x, R0;
TEX R0.xyz, R3, texture[1], 2D;
MAD R0.xyz, fragment.texcoord[1].z, R0, R1;
TEX R1.xyz, R3.zxzw, texture[1], 2D;
MAD R0.xyz, R1, fragment.texcoord[1].y, R0;
ADD R0.xyz, R0, -R2;
MAD R0.xyz, R0, c[2].x, R2;
ADD R0.w, -R0, c[12].x;
RCP R2.x, R0.w;
TEX R1, fragment.texcoord[5], texture[5], 2D;
MUL R1.xyz, R1.w, R1;
MUL R0.xyz, R0, c[4];
ADD R0.w, fragment.texcoord[2], -c[11].x;
MUL_SAT R0.w, R0, R2.x;
MUL R1.xyz, R1, c[18].y;
MAD R1.w, -R0, c[21].x, c[21].y;
MUL R2.xyz, R1.y, c[20];
MAD R2.xyz, R1.x, c[19], R2;
MUL R0.w, R0, R0;
MAD R0.w, -R0, R1, c[20];
ADD R0.xyz, R0, -c[5];
MAD R0.xyz, R0.w, R0, c[5];
MUL R1.w, fragment.texcoord[2].x, c[16].x;
MUL R1.w, R1, c[15].x;
POW R2.w, c[21].z, R1.w;
MAD R2.xyz, R1.z, c[18].zwxw, R2;
DP3 R1.w, R2, R2;
RSQ R1.w, R1.w;
MUL R2.xyz, R1.w, R2;
ADD R3.xyz, -R0, c[14];
ADD R2.w, -R2, c[20];
MAD R3.xyz, R2.w, R3, R0;
DP3 R2.w, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.w, R2.w;
MAD R2.xyz, R2.w, fragment.texcoord[0], R2;
MUL R1.w, fragment.texcoord[2], c[16].x;
MUL R2.w, R1, c[15].x;
DP3 R1.w, R2, R2;
POW R2.x, c[21].z, R2.w;
RSQ R1.w, R1.w;
MUL R1.w, R1, R2.z;
ADD R2.x, -R2, c[20].w;
MAX R1.w, R1, c[19].y;
MOV R0.y, c[21].w;
MOV R0.x, fragment.texcoord[3];
TEX R0.xyz, R0, texture[2], 2D;
ADD R0.xyz, R0, -R3;
MAD R0.xyz, R2.x, R0, R3;
TEX R2, fragment.texcoord[5], texture[4], 2D;
MUL R2.xyz, R2.w, R2;
MOV R2.w, c[19];
DP3 R1.x, R1, c[18].x;
MUL R1.xyz, R2, R1.x;
MUL R2.w, R2, c[6].x;
MUL R2.xyz, R1, c[18].y;
POW R2.w, R1.w, R2.w;
TXP R1, fragment.texcoord[4], texture[3], 2D;
ADD R1, R1, R2;
MOV R2.x, c[20].w;
DP3 R2.y, fragment.texcoord[0], fragment.texcoord[0];
RSQ R2.y, R2.y;
MAD R2.z, fragment.texcoord[0], -R2.y, c[20].w;
ADD R2.w, R2.x, -c[17].x;
MUL R2.y, R2.w, c[7].x;
ADD R3.x, R2, -c[8];
MUL R2.z, R2, c[9].x;
POW R2.x, R2.z, c[10].x;
MAD R2.x, R2, R3, c[8];
ADD R3.x, R2, -c[20].w;
MUL R1.w, R1, R2.y;
MUL R2.xyz, R1, c[1];
MAD R0.w, R0, R3.x, c[20];
MUL R2.xyz, R1.w, R2;
MUL R0.w, R2, R0;
MAD result.color.xyz, R1, R0, R2;
MAD result.color.w, R1, c[1], R0;
END
# 87 instructions, 4 R-regs
"
}
SubProgram "d3d9 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
Vector 0 [_SinTime]
Vector 1 [_SpecColor]
Float 2 [_Mix]
Float 3 [_displacement]
Vector 4 [_Color]
Vector 5 [_ColorFromSpace]
Float 6 [_Shininess]
Float 7 [_Gloss]
Float 8 [_oceanOpacity]
Float 9 [_falloffPower]
Float 10 [_falloffExp]
Float 11 [_fadeStart]
Float 12 [_fadeEnd]
Float 13 [_tiling]
Vector 14 [_fogColor]
Float 15 [_heightDensityAtViewer]
Float 16 [_globalDensity]
Float 17 [_PlanetOpacity]
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
def c18, 8.00000000, 0.57735026, -0.40824831, 0.70710677
def c19, 0.81649655, 0.00000000, 0.57735026, 128.00000000
def c20, -0.40824828, -0.70710677, 0.57735026, 1.00000000
def c21, 2.00000000, 3.00000000, -1.00000000, 2.71828198
def c22, 0.50000000, 0, 0, 0
dcl_texcoord0 v0.xyz
dcl_texcoord1 v1.xyz
dcl_texcoord2 v2
dcl_texcoord3 v3.x
dcl_texcoord4 v4
dcl_texcoord5 v5.xy
mov r0.x, c3
mul r0.x, c0.w, r0
mad r0.xyz, v2, c13.x, r0.x
texld r1.xyz, r0.zyzw, s0
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s0
mad r3.xyz, v1.z, r1, r2
texld r2.xyz, r0.zxzw, s0
mad r3.xyz, v1.y, r2, r3
texld r1.xyz, r0.zyzw, s1
mul r2.xyz, v1.x, r1
texld r1.xyz, r0, s1
mad r1.xyz, v1.z, r1, r2
texld r0.xyz, r0.zxzw, s1
mad r0.xyz, r0, v1.y, r1
add r0.xyz, r0, -r3
mad r0.xyz, r0, c2.x, r3
mul r1.xyz, r0, c4
texld r0, v5, s5
mul_pp r0.xyz, r0.w, r0
add r2.xyz, r1, -c5
mul_pp r1.xyz, r0, c18.x
mul r0.xyz, r1.y, c18.zwyw
mad r0.xyz, r1.x, c19, r0
mov r0.w, c12.x
add r0.w, -c11.x, r0
rcp r1.w, r0.w
add r0.w, v2, -c11.x
mul_sat r0.w, r0, r1
mad r1.w, -r0, c21.x, c21.y
mul r0.w, r0, r0
mad r1.w, -r0, r1, c20
mad r3.xyz, r1.w, r2, c5
mad r0.xyz, r1.z, c20, r0
dp3 r0.w, r0, r0
mul r2.x, v2, c16
add r4.xyz, -r3, c14
mul r2.w, r2.x, c15.x
rsq r0.w, r0.w
mul r2.xyz, r0.w, r0
pow r0, c21.w, r2.w
dp3_pp r0.y, v0, v0
rsq_pp r0.y, r0.y
mad_pp r0.yzw, r0.y, v0.xxyz, r2.xxyz
mov r2.x, r0
dp3_pp r0.x, r0.yzww, r0.yzww
add r0.y, -r2.x, c20.w
mad r2.xyz, r0.y, r4, r3
rsq_pp r2.w, r0.x
mov_pp r3.x, c6
mov r0.y, c22.x
mov r0.x, v3
texld r0.xyz, r0, s2
add r4.xyz, r0, -r2
mul_pp r0.x, r2.w, r0.w
mul r0.y, v2.w, c16.x
max_pp r2.w, r0.x, c19.y
mul r3.y, r0, c15.x
pow r0, c21.w, r3.y
mul_pp r0.y, c19.w, r3.x
pow r3, r2.w, r0.y
add r0.x, -r0, c20.w
mad r0.xyz, r0.x, r4, r2
texld r2, v5, s4
dp3 r0.w, v0, v0
mul_pp r2.xyz, r2.w, r2
rsq r0.w, r0.w
mad r2.w, v0.z, -r0, c20
dp3_pp r0.w, r1, c18.y
mul_pp r1.xyz, r2, r0.w
mul_pp r3.xyz, r1, c18.x
mul r2.w, r2, c9.x
pow r4, r2.w, c10.x
mov r1.x, c8
texldp r2, v4, s3
mov r0.w, c17.x
add_pp r2, r2, r3
mov r1.z, r4.x
add r1.y, c20.w, -r1.x
add r0.w, c20, -r0
mul r1.x, r0.w, c7
mad r1.y, r1.z, r1, c8.x
add r3.x, r1.y, c21.z
mul_pp r2.w, r2, r1.x
mul_pp r1.xyz, r2, c1
mad r1.w, r1, r3.x, c20
mul_pp r1.xyz, r2.w, r1
mul r0.w, r0, r1
mad_pp oC0.xyz, r2, r0, r1
mad_pp oC0.w, r2, c1, r0
"
}
SubProgram "d3d11 " {
Keywords { "LIGHTMAP_ON" "DIRLIGHTMAP_ON" "HDR_LIGHT_PREPASS_ON" }
SetTexture 0 [_WaterTex] 2D 0
SetTexture 1 [_WaterTex1] 2D 1
SetTexture 2 [_fogColorRamp] 2D 2
SetTexture 3 [_LightBuffer] 2D 3
SetTexture 4 [unity_Lightmap] 2D 4
SetTexture 5 [unity_LightmapInd] 2D 5
ConstBuffer "$Globals" 352
Vector 32 [_SpecColor]
Float 48 [_Mix]
Float 52 [_displacement]
Vector 80 [_Color]
Vector 96 [_ColorFromSpace]
Float 128 [_Shininess]
Float 132 [_Gloss]
Float 192 [_oceanOpacity]
Float 196 [_falloffPower]
Float 200 [_falloffExp]
Float 204 [_fadeStart]
Float 208 [_fadeEnd]
Float 212 [_tiling]
Vector 224 [_fogColor]
Float 244 [_heightDensityAtViewer]
Float 256 [_globalDensity]
Float 288 [_PlanetOpacity]
ConstBuffer "UnityPerCamera" 128
Vector 16 [_SinTime]
BindCB  "$Globals" 0
BindCB  "UnityPerCamera" 1
"ps_4_0
eefiecedadamhgmdhadkgggmgebeapjnobhahojdabaaaaaaniamaaaaadaaaaaa
cmaaaaaapmaaaaaadaabaaaaejfdeheomiaaaaaaahaaaaaaaiaaaaaalaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaalmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahahaaaalmaaaaaaadaaaaaaaaaaaaaaadaaaaaaabaaaaaa
aiaiaaaalmaaaaaaabaaaaaaaaaaaaaaadaaaaaaacaaaaaaapahaaaalmaaaaaa
acaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaalmaaaaaaaeaaaaaaaaaaaaaa
adaaaaaaaeaaaaaaapalaaaalmaaaaaaafaaaaaaaaaaaaaaadaaaaaaafaaaaaa
adadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefckaalaaaaeaaaaaaaoiacaaaa
fjaaaaaeegiocaaaaaaaaaaabdaaaaaafjaaaaaeegiocaaaabaaaaaaacaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaa
acaaaaaafkaaaaadaagabaaaadaaaaaafkaaaaadaagabaaaaeaaaaaafkaaaaad
aagabaaaafaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaafibiaaaeaahabaaaaeaaaaaaffffaaaafibiaaaeaahabaaa
afaaaaaaffffaaaagcbaaaadhcbabaaaabaaaaaagcbaaaadicbabaaaabaaaaaa
gcbaaaadhcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaadlcbabaaa
aeaaaaaagcbaaaaddcbabaaaafaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaac
afaaaaaadiaaaaajbcaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaadkiacaaa
abaaaaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaegbcbaaaadaaaaaafgifcaaa
aaaaaaaaanaaaaaaagaabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaggakbaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadiaaaaahhcaabaaaabaaaaaa
egacbaaaabaaaaaaagbabaaaacaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaa
aaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaacaaaaaakgbkbaaaacaaaaaaegacbaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaacgakbaaaaaaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaadcaaaaaj
hcaabaaaabaaaaaaegacbaaaacaaaaaafgbfbaaaacaaaaaaegacbaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaaggakbaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaacaaaaaaagbabaaaacaaaaaa
efaaaaajpcaabaaaadaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaefaaaaajpcaabaaaaaaaaaaacgakbaaaaaaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaajhcaabaaaacaaaaaaegacbaaaadaaaaaakgbkbaaa
acaaaaaaegacbaaaacaaaaaadcaaaaajhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
fgbfbaaaacaaaaaaegacbaaaacaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaaaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaaaaaaaaaagiacaaa
aaaaaaaaadaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadcaaaaamhcaabaaa
aaaaaaaaegacbaaaaaaaaaaaegiccaaaaaaaaaaaafaaaaaaegiccaiaebaaaaaa
aaaaaaaaagaaaaaaaaaaaaakicaabaaaaaaaaaaadkiacaiaebaaaaaaaaaaaaaa
amaaaaaaakiacaaaaaaaaaaaanaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaaaaaaaaajbcaabaaa
abaaaaaadkbabaaaadaaaaaadkiacaiaebaaaaaaaaaaaaaaamaaaaaadicaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaabaaaaaadcaaaaajbcaabaaa
abaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaaamaabeaaaaaaaaaeaeadiaaaaah
icaabaaaaaaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaa
aaaaaaaaakaabaiaebaaaaaaabaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadp
dcaaaaakhcaabaaaaaaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaaegiccaaa
aaaaaaaaagaaaaaaaaaaaaajhcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaa
egiccaaaaaaaaaaaaoaaaaaadiaaaaaidcaabaaaacaaaaaamgbabaaaadaaaaaa
agiacaaaaaaaaaaabaaaaaaadiaaaaaidcaabaaaacaaaaaaegaabaaaacaaaaaa
fgifcaaaaaaaaaaaapaaaaaadiaaaaakdcaabaaaacaaaaaaegaabaaaacaaaaaa
aceaaaaadlkklidpdlkklidpaaaaaaaaaaaaaaaabjaaaaafdcaabaaaacaaaaaa
egaabaaaacaaaaaaaaaaaaaldcaabaaaacaaaaaaegaabaiaebaaaaaaacaaaaaa
aceaaaaaaaaaiadpaaaaiadpaaaaaaaaaaaaaaaadcaaaaajhcaabaaaaaaaaaaa
agaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaafbcaabaaa
abaaaaaadkbabaaaabaaaaaadgaaaaafccaabaaaabaaaaaaabeaaaaaaaaaaadp
efaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaacaaaaaaaagabaaa
acaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaiaebaaaaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaajhcaabaaaaaaaaaaafgafbaaaacaaaaaaegacbaaaabaaaaaa
egacbaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaafaaaaaaeghobaaa
afaaaaaaaagabaaaafaaaaaadiaaaaahicaabaaaabaaaaaadkaabaaaabaaaaaa
abeaaaaaaaaaaaebdiaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaapgapbaaa
abaaaaaadiaaaaakhcaabaaaacaaaaaafgafbaaaabaaaaaaaceaaaaaomafnblo
pdaedfdpdkmnbddpaaaaaaaadcaaaaamhcaabaaaacaaaaaaagaabaaaabaaaaaa
aceaaaaaolaffbdpaaaaaaaadkmnbddpaaaaaaaaegacbaaaacaaaaaadcaaaaam
hcaabaaaacaaaaaakgakbaaaabaaaaaaaceaaaaaolafnblopdaedflpdkmnbddp
aaaaaaaaegacbaaaacaaaaaabaaaaaakbcaabaaaabaaaaaaaceaaaaadkmnbddp
dkmnbddpdkmnbddpaaaaaaaaegacbaaaabaaaaaabaaaaaahccaabaaaabaaaaaa
egacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaafccaabaaaabaaaaaabkaabaaa
abaaaaaabaaaaaahecaabaaaabaaaaaaegbcbaaaabaaaaaaegbcbaaaabaaaaaa
eeaaaaafecaabaaaabaaaaaackaabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaa
kgakbaaaabaaaaaaegbcbaaaabaaaaaadcaaaaakecaabaaaabaaaaaackbabaia
ebaaaaaaabaaaaaackaabaaaabaaaaaaabeaaaaaaaaaiadpdiaaaaaiecaabaaa
abaaaaaackaabaaaabaaaaaabkiacaaaaaaaaaaaamaaaaaacpaaaaafecaabaaa
abaaaaaackaabaaaabaaaaaadiaaaaaiecaabaaaabaaaaaackaabaaaabaaaaaa
ckiacaaaaaaaaaaaamaaaaaabjaaaaafecaabaaaabaaaaaackaabaaaabaaaaaa
dcaaaaajhcaabaaaacaaaaaaegacbaaaacaaaaaafgafbaaaabaaaaaaegacbaaa
adaaaaaabaaaaaahccaabaaaabaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaa
eeaaaaafccaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaahccaabaaaabaaaaaa
bkaabaaaabaaaaaackaabaaaacaaaaaadeaaaaahccaabaaaabaaaaaabkaabaaa
abaaaaaaabeaaaaaaaaaaaaacpaaaaafccaabaaaabaaaaaabkaabaaaabaaaaaa
diaaaaaiicaabaaaabaaaaaaakiacaaaaaaaaaaaaiaaaaaaabeaaaaaaaaaaaed
diaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaadkaabaaaabaaaaaabjaaaaaf
icaabaaaacaaaaaabkaabaaaabaaaaaaaoaaaaahkcaabaaaabaaaaaaagbebaaa
aeaaaaaapgbpbaaaaeaaaaaaefaaaaajpcaabaaaadaaaaaangafbaaaabaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaaefaaaaajpcaabaaaaeaaaaaaegbabaaa
afaaaaaaeghobaaaaeaaaaaaaagabaaaaeaaaaaadiaaaaahccaabaaaabaaaaaa
dkaabaaaaeaaaaaaabeaaaaaaaaaaaebdiaaaaahhcaabaaaaeaaaaaaegacbaaa
aeaaaaaafgafbaaaabaaaaaadiaaaaahhcaabaaaacaaaaaaagaabaaaabaaaaaa
egacbaaaaeaaaaaaaaaaaaahpcaabaaaacaaaaaaegaobaaaacaaaaaaegaobaaa
adaaaaaadiaaaaailcaabaaaabaaaaaaegaibaaaacaaaaaaegiicaaaaaaaaaaa
acaaaaaaaaaaaaajbcaabaaaadaaaaaaakiacaiaebaaaaaaaaaaaaaabcaaaaaa
abeaaaaaaaaaiadpdiaaaaaiccaabaaaadaaaaaaakaabaaaadaaaaaabkiacaaa
aaaaaaaaaiaaaaaadiaaaaahicaabaaaacaaaaaadkaabaaaacaaaaaabkaabaaa
adaaaaaadiaaaaahlcaabaaaabaaaaaaegambaaaabaaaaaapgapbaaaacaaaaaa
diaaaaaiicaabaaaacaaaaaadkaabaaaacaaaaaadkiacaaaaaaaaaaaacaaaaaa
dcaaaaajhccabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaacaaaaaaegadbaaa
abaaaaaaaaaaaaajbcaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaaamaaaaaa
abeaaaaaaaaaiadpdcaaaaakbcaabaaaaaaaaaaackaabaaaabaaaaaaakaabaaa
aaaaaaaaakiacaaaaaaaaaaaamaaaaaaaaaaaaahbcaabaaaaaaaaaaaakaabaaa
aaaaaaaaabeaaaaaaaaaialpdcaaaaajbcaabaaaaaaaaaaadkaabaaaaaaaaaaa
akaabaaaaaaaaaaaabeaaaaaaaaaiadpdcaaaaajiccabaaaaaaaaaaaakaabaaa
aaaaaaaaakaabaaaadaaaaaadkaabaaaacaaaaaadoaaaaab"
}
}
 }
}
Fallback "Reflective/Bumped Diffuse"}