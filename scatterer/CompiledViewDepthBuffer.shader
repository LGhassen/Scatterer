// Compiled shader for all platforms, uncompressed size: 8.4KB

Shader "Custom/DepthShader" {
SubShader { 
 Tags { "RenderType"="Opaque" }


 // Stats for Vertex shader:
 //       d3d11 : 7 math
 //        d3d9 : 9 math
 //      opengl : 9 math
 // Stats for Fragment shader:
 //       d3d11 : 4 math, 1 texture
 //        d3d9 : 4 math, 1 texture
 //      opengl : 5 math, 1 texture
 Pass {
  Tags { "RenderType"="Opaque" }
Program "vp" {
SubProgram "opengl " {
// Stats: 9 math
Bind "vertex" Vertex
Vector 5 [_ProjectionParams]
"3.0-!!ARBvp1.0
PARAM c[6] = { { 0.5 },
		state.matrix.mvp,
		program.local[5] };
TEMP R0;
TEMP R1;
DP4 R0.w, vertex.position, c[4];
DP4 R0.z, vertex.position, c[3];
DP4 R0.x, vertex.position, c[1];
DP4 R0.y, vertex.position, c[2];
MUL R1.xyz, R0.xyww, c[0].x;
MUL R1.y, R1, c[5].x;
ADD result.texcoord[1].xy, R1, R1.z;
MOV result.position, R0;
MOV result.texcoord[1].zw, R0;
END
# 9 instructions, 2 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 9 math
Bind "vertex" Vertex
Matrix 0 [glstate_matrix_mvp]
Vector 4 [_ProjectionParams]
Vector 5 [_ScreenParams]
"vs_3_0
dcl_position o0
dcl_texcoord1 o1
def c6, 0.50000000, 0, 0, 0
dcl_position0 v0
dp4 r0.w, v0, c3
dp4 r0.z, v0, c2
dp4 r0.x, v0, c0
dp4 r0.y, v0, c1
mul r1.xyz, r0.xyww, c6.x
mul r1.y, r1, c4.x
mad o1.xy, r1.z, c5.zwzw, r1
mov o0, r0
mov o1.zw, r0
"
}
SubProgram "d3d11 " {
// Stats: 7 math
Bind "vertex" Vertex
ConstBuffer "UnityPerCamera" 128
Vector 80 [_ProjectionParams]
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  "UnityPerCamera" 0
BindCB  "UnityPerDraw" 1
"vs_4_0
eefieceddidllfcgeoacanbaimfiidddihijppebabaaaaaaiaacaaaaadaaaaaa
cmaaaaaakaaaaaaapiaaaaaaejfdeheogmaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaafjaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahaaaaaagaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apaaaaaafaepfdejfeejepeoaaeoepfcenebemaafeeffiedepepfceeaaklklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaa
fdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefciaabaaaa
eaaaabaagaaaaaaafjaaaaaeegiocaaaaaaaaaaaagaaaaaafjaaaaaeegiocaaa
abaaaaaaaeaaaaaafpaaaaadpcbabaaaaaaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaadpccabaaaabaaaaaagiaaaaacacaaaaaadiaaaaaipcaabaaa
aaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaadcaaaaakpcaabaaa
aaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaacaaaaaakgbkbaaaaaaaaaaa
egaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaadaaaaaa
pgbpbaaaaaaaaaaaegaobaaaaaaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaa
aaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaa
afaaaaaadiaaaaakncaabaaaabaaaaaaagahbaaaaaaaaaaaaceaaaaaaaaaaadp
aaaaaaaaaaaaaadpaaaaaadpdgaaaaafmccabaaaabaaaaaakgaobaaaaaaaaaaa
aaaaaaahdccabaaaabaaaaaakgakbaaaabaaaaaamgaabaaaabaaaaaadoaaaaab
"
}
SubProgram "gles " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 glstate_matrix_mvp;
varying highp vec4 xlv_TEXCOORD1;
void main ()
{
  highp vec4 tmpvar_1;
  tmpvar_1 = (glstate_matrix_mvp * _glesVertex);
  highp vec4 o_2;
  highp vec4 tmpvar_3;
  tmpvar_3 = (tmpvar_1 * 0.5);
  highp vec2 tmpvar_4;
  tmpvar_4.x = tmpvar_3.x;
  tmpvar_4.y = (tmpvar_3.y * _ProjectionParams.x);
  o_2.xy = (tmpvar_4 + tmpvar_3.w);
  o_2.zw = tmpvar_1.zw;
  gl_Position = tmpvar_1;
  xlv_TEXCOORD1 = o_2;
}



#endif
#ifdef FRAGMENT

uniform highp vec4 _ZBufferParams;
uniform sampler2D _CameraDepthTexture;
varying highp vec4 xlv_TEXCOORD1;
void main ()
{
  mediump vec4 c_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2DProj (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float tmpvar_3;
  highp float z_4;
  z_4 = tmpvar_2.x;
  tmpvar_3 = (1.0/(((_ZBufferParams.x * z_4) + _ZBufferParams.y)));
  highp float tmpvar_5;
  tmpvar_5 = (tmpvar_3 * 10.0);
  c_1.x = tmpvar_5;
  highp float tmpvar_6;
  tmpvar_6 = (tmpvar_3 * 10.0);
  c_1.y = tmpvar_6;
  highp float tmpvar_7;
  tmpvar_7 = (tmpvar_3 * 10.0);
  c_1.z = tmpvar_7;
  c_1.w = 1.0;
  gl_FragData[0] = c_1;
}



#endif"
}
SubProgram "glesdesktop " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 glstate_matrix_mvp;
varying highp vec4 xlv_TEXCOORD1;
void main ()
{
  highp vec4 tmpvar_1;
  tmpvar_1 = (glstate_matrix_mvp * _glesVertex);
  highp vec4 o_2;
  highp vec4 tmpvar_3;
  tmpvar_3 = (tmpvar_1 * 0.5);
  highp vec2 tmpvar_4;
  tmpvar_4.x = tmpvar_3.x;
  tmpvar_4.y = (tmpvar_3.y * _ProjectionParams.x);
  o_2.xy = (tmpvar_4 + tmpvar_3.w);
  o_2.zw = tmpvar_1.zw;
  gl_Position = tmpvar_1;
  xlv_TEXCOORD1 = o_2;
}



#endif
#ifdef FRAGMENT

uniform highp vec4 _ZBufferParams;
uniform sampler2D _CameraDepthTexture;
varying highp vec4 xlv_TEXCOORD1;
void main ()
{
  mediump vec4 c_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = texture2DProj (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float tmpvar_3;
  highp float z_4;
  z_4 = tmpvar_2.x;
  tmpvar_3 = (1.0/(((_ZBufferParams.x * z_4) + _ZBufferParams.y)));
  highp float tmpvar_5;
  tmpvar_5 = (tmpvar_3 * 10.0);
  c_1.x = tmpvar_5;
  highp float tmpvar_6;
  tmpvar_6 = (tmpvar_3 * 10.0);
  c_1.y = tmpvar_6;
  highp float tmpvar_7;
  tmpvar_7 = (tmpvar_3 * 10.0);
  c_1.z = tmpvar_7;
  c_1.w = 1.0;
  gl_FragData[0] = c_1;
}



#endif"
}
SubProgram "gles3 " {
"!!GLES3#version 300 es


#ifdef VERTEX

in vec4 _glesVertex;
uniform highp vec4 _ProjectionParams;
uniform highp mat4 glstate_matrix_mvp;
out highp vec4 xlv_TEXCOORD1;
void main ()
{
  highp vec4 tmpvar_1;
  tmpvar_1 = (glstate_matrix_mvp * _glesVertex);
  highp vec4 o_2;
  highp vec4 tmpvar_3;
  tmpvar_3 = (tmpvar_1 * 0.5);
  highp vec2 tmpvar_4;
  tmpvar_4.x = tmpvar_3.x;
  tmpvar_4.y = (tmpvar_3.y * _ProjectionParams.x);
  o_2.xy = (tmpvar_4 + tmpvar_3.w);
  o_2.zw = tmpvar_1.zw;
  gl_Position = tmpvar_1;
  xlv_TEXCOORD1 = o_2;
}



#endif
#ifdef FRAGMENT

out mediump vec4 _glesFragData[4];
uniform highp vec4 _ZBufferParams;
uniform sampler2D _CameraDepthTexture;
in highp vec4 xlv_TEXCOORD1;
void main ()
{
  mediump vec4 c_1;
  lowp vec4 tmpvar_2;
  tmpvar_2 = textureProj (_CameraDepthTexture, xlv_TEXCOORD1);
  highp float tmpvar_3;
  highp float z_4;
  z_4 = tmpvar_2.x;
  tmpvar_3 = (1.0/(((_ZBufferParams.x * z_4) + _ZBufferParams.y)));
  highp float tmpvar_5;
  tmpvar_5 = (tmpvar_3 * 10.0);
  c_1.x = tmpvar_5;
  highp float tmpvar_6;
  tmpvar_6 = (tmpvar_3 * 10.0);
  c_1.y = tmpvar_6;
  highp float tmpvar_7;
  tmpvar_7 = (tmpvar_3 * 10.0);
  c_1.z = tmpvar_7;
  c_1.w = 1.0;
  _glesFragData[0] = c_1;
}



#endif"
}
}
Program "fp" {
SubProgram "opengl " {
// Stats: 5 math, 1 textures
Vector 0 [_ZBufferParams]
SetTexture 0 [_CameraDepthTexture] 2D 0
"3.0-!!ARBfp1.0
PARAM c[2] = { program.local[0],
		{ 10, 1 } };
TEMP R0;
TXP R0.x, fragment.texcoord[1], texture[0], 2D;
MAD R0.x, R0, c[0], c[0].y;
RCP R0.x, R0.x;
MUL result.color.xyz, R0.x, c[1].x;
MOV result.color.w, c[1].y;
END
# 5 instructions, 1 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 4 math, 1 textures
Vector 0 [_ZBufferParams]
SetTexture 0 [_CameraDepthTexture] 2D 0
"ps_3_0
dcl_2d s0
def c1, 10.00000000, 1.00000000, 0, 0
dcl_texcoord1 v0
texldp r0.x, v0, s0
mad r0.x, r0, c0, c0.y
rcp r0.x, r0.x
mul oC0.xyz, r0.x, c1.x
mov_pp oC0.w, c1.y
"
}
SubProgram "d3d11 " {
// Stats: 4 math, 1 textures
SetTexture 0 [_CameraDepthTexture] 2D 0
ConstBuffer "UnityPerCamera" 128
Vector 112 [_ZBufferParams]
BindCB  "UnityPerCamera" 0
"ps_4_0
eefiecednbiapfikpkgkflfnldinnbfodiejadpiabaaaaaaoiabaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaabaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapalaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcciabaaaa
eaaaaaaaekaaaaaafjaaaaaeegiocaaaaaaaaaaaaiaaaaaafkaaaaadaagabaaa
aaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaadlcbabaaaabaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacabaaaaaaaoaaaaahdcaabaaaaaaaaaaa
egbabaaaabaaaaaapgbpbaaaabaaaaaaefaaaaajpcaabaaaaaaaaaaaegaabaaa
aaaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaalbcaabaaaaaaaaaaa
akiacaaaaaaaaaaaahaaaaaaakaabaaaaaaaaaaabkiacaaaaaaaaaaaahaaaaaa
aoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
akaabaaaaaaaaaaadiaaaaakhccabaaaaaaaaaaaagaabaaaaaaaaaaaaceaaaaa
aaaacaebaaaacaebaaaacaebaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaa
aaaaiadpdoaaaaab"
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
Fallback "VertexLit"
}