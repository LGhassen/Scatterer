// Compiled shader for all platforms, uncompressed size: 9.3KB

Shader "EncodeFloat/DecodeToFloat" {
SubShader { 


 // Stats for Vertex shader:
 //       d3d11 : 4 math
 //        d3d9 : 5 math
 //      opengl : 5 math
 // Stats for Fragment shader:
 //       d3d11 : 5 math, 4 texture
 //        d3d9 : 6 math, 4 texture
 //      opengl : 10 math, 4 texture
 Pass {
  ZTest Always
  ZWrite Off
  Cull Off
  Fog { Mode Off }
Program "vp" {
SubProgram "opengl " {
// Stats: 5 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
"3.0-!!ARBvp1.0
PARAM c[5] = { program.local[0],
		state.matrix.mvp };
MOV result.texcoord[0].xy, vertex.texcoord[0];
DP4 result.position.w, vertex.position, c[4];
DP4 result.position.z, vertex.position, c[3];
DP4 result.position.y, vertex.position, c[2];
DP4 result.position.x, vertex.position, c[1];
END
# 5 instructions, 0 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 5 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
"vs_3_0
dcl_position o0
dcl_texcoord0 o1
dcl_position0 v0
dcl_texcoord0 v1
mov o1.xy, v1
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}
SubProgram "d3d11 " {
// Stats: 4 math
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "UnityPerDraw" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  "UnityPerDraw" 0
"vs_4_0
eefieceddolmmcahcgjmjpiinclfhjokihhgamkaabaaaaaaaeacaaaaadaaaaaa
cmaaaaaakaaaaaaapiaaaaaaejfdeheogmaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaafjaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaahaaaaaagaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apadaaaafaepfdejfeejepeoaaeoepfcenebemaafeeffiedepepfceeaaklklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefcaeabaaaa
eaaaabaaebaaaaaafjaaaaaeegiocaaaaaaaaaaaaeaaaaaafpaaaaadpcbabaaa
aaaaaaaafpaaaaaddcbabaaaacaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaaddccabaaaabaaaaaagiaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaaaaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaaaaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaaaaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaaaaaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdccabaaaabaaaaaaegbabaaaacaaaaaa
doaaaaab"
}
SubProgram "gles " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
varying highp vec2 xlv_TEXCOORD0;
void main ()
{
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
}



#endif
#ifdef FRAGMENT

uniform sampler2D _TexR;
uniform sampler2D _TexG;
uniform sampler2D _TexB;
uniform sampler2D _TexA;
uniform highp float _Max;
uniform highp float _Min;
varying highp vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_TexR, xlv_TEXCOORD0);
  highp vec4 rgba_2;
  rgba_2 = tmpvar_1;
  lowp vec4 tmpvar_3;
  tmpvar_3 = texture2D (_TexG, xlv_TEXCOORD0);
  highp vec4 rgba_4;
  rgba_4 = tmpvar_3;
  lowp vec4 tmpvar_5;
  tmpvar_5 = texture2D (_TexB, xlv_TEXCOORD0);
  highp vec4 rgba_6;
  rgba_6 = tmpvar_5;
  lowp vec4 tmpvar_7;
  tmpvar_7 = texture2D (_TexA, xlv_TEXCOORD0);
  highp vec4 rgba_8;
  rgba_8 = tmpvar_7;
  highp vec4 tmpvar_9;
  tmpvar_9.x = dot (rgba_2, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.y = dot (rgba_4, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.z = dot (rgba_6, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.w = dot (rgba_8, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  highp vec4 tmpvar_10;
  tmpvar_10 = ((tmpvar_9 * _Max) - _Min);
  gl_FragData[0] = tmpvar_10;
}



#endif"
}
SubProgram "glesdesktop " {
"!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
varying highp vec2 xlv_TEXCOORD0;
void main ()
{
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
}



#endif
#ifdef FRAGMENT

uniform sampler2D _TexR;
uniform sampler2D _TexG;
uniform sampler2D _TexB;
uniform sampler2D _TexA;
uniform highp float _Max;
uniform highp float _Min;
varying highp vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture2D (_TexR, xlv_TEXCOORD0);
  highp vec4 rgba_2;
  rgba_2 = tmpvar_1;
  lowp vec4 tmpvar_3;
  tmpvar_3 = texture2D (_TexG, xlv_TEXCOORD0);
  highp vec4 rgba_4;
  rgba_4 = tmpvar_3;
  lowp vec4 tmpvar_5;
  tmpvar_5 = texture2D (_TexB, xlv_TEXCOORD0);
  highp vec4 rgba_6;
  rgba_6 = tmpvar_5;
  lowp vec4 tmpvar_7;
  tmpvar_7 = texture2D (_TexA, xlv_TEXCOORD0);
  highp vec4 rgba_8;
  rgba_8 = tmpvar_7;
  highp vec4 tmpvar_9;
  tmpvar_9.x = dot (rgba_2, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.y = dot (rgba_4, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.z = dot (rgba_6, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.w = dot (rgba_8, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  highp vec4 tmpvar_10;
  tmpvar_10 = ((tmpvar_9 * _Max) - _Min);
  gl_FragData[0] = tmpvar_10;
}



#endif"
}
SubProgram "gles3 " {
"!!GLES3#version 300 es


#ifdef VERTEX

in vec4 _glesVertex;
in vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
out highp vec2 xlv_TEXCOORD0;
void main ()
{
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = _glesMultiTexCoord0.xy;
}



#endif
#ifdef FRAGMENT

out mediump vec4 _glesFragData[4];
uniform sampler2D _TexR;
uniform sampler2D _TexG;
uniform sampler2D _TexB;
uniform sampler2D _TexA;
uniform highp float _Max;
uniform highp float _Min;
in highp vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 tmpvar_1;
  tmpvar_1 = texture (_TexR, xlv_TEXCOORD0);
  highp vec4 rgba_2;
  rgba_2 = tmpvar_1;
  lowp vec4 tmpvar_3;
  tmpvar_3 = texture (_TexG, xlv_TEXCOORD0);
  highp vec4 rgba_4;
  rgba_4 = tmpvar_3;
  lowp vec4 tmpvar_5;
  tmpvar_5 = texture (_TexB, xlv_TEXCOORD0);
  highp vec4 rgba_6;
  rgba_6 = tmpvar_5;
  lowp vec4 tmpvar_7;
  tmpvar_7 = texture (_TexA, xlv_TEXCOORD0);
  highp vec4 rgba_8;
  rgba_8 = tmpvar_7;
  highp vec4 tmpvar_9;
  tmpvar_9.x = dot (rgba_2, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.y = dot (rgba_4, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.z = dot (rgba_6, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  tmpvar_9.w = dot (rgba_8, vec4(1.0, 0.00392157, 1.53787e-05, 6.22737e-09));
  highp vec4 tmpvar_10;
  tmpvar_10 = ((tmpvar_9 * _Max) - _Min);
  _glesFragData[0] = tmpvar_10;
}



#endif"
}
}
Program "fp" {
SubProgram "opengl " {
// Stats: 10 math, 4 textures
Float 0 [_Max]
Float 1 [_Min]
SetTexture 0 [_TexR] 2D 0
SetTexture 1 [_TexG] 2D 1
SetTexture 2 [_TexB] 2D 2
SetTexture 3 [_TexA] 2D 3
"3.0-!!ARBfp1.0
PARAM c[3] = { program.local[0..1],
		{ 1, 0.0039215689, 1.53787e-005, 6.2273724e-009 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEX R1, fragment.texcoord[0], texture[0], 2D;
TEX R0, fragment.texcoord[0], texture[1], 2D;
DP4 R2.x, R1, c[2];
DP4 R2.y, R0, c[2];
TEX R0, fragment.texcoord[0], texture[2], 2D;
TEX R1, fragment.texcoord[0], texture[3], 2D;
DP4 R2.w, R1, c[2];
DP4 R2.z, R0, c[2];
MUL R0, R2, c[0].x;
ADD result.color, R0, -c[1].x;
END
# 10 instructions, 3 R-regs
"
}
SubProgram "d3d9 " {
// Stats: 6 math, 4 textures
Float 0 [_Max]
Float 1 [_Min]
SetTexture 0 [_TexR] 2D 0
SetTexture 1 [_TexG] 2D 1
SetTexture 2 [_TexB] 2D 2
SetTexture 3 [_TexA] 2D 3
"ps_3_0
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c2, 1.00000000, 0.00392157, 0.00001538, 0.00000001
dcl_texcoord0 v0.xy
texld r1, v0, s0
texld r0, v0, s1
dp4 r2.x, r1, c2
dp4 r2.y, r0, c2
texld r0, v0, s2
texld r1, v0, s3
dp4 r2.w, r1, c2
dp4 r2.z, r0, c2
mul r0, r2, c0.x
add oC0, r0, -c1.x
"
}
SubProgram "d3d11 " {
// Stats: 5 math, 4 textures
SetTexture 0 [_TexR] 2D 0
SetTexture 1 [_TexG] 2D 1
SetTexture 2 [_TexB] 2D 2
SetTexture 3 [_TexA] 2D 3
ConstBuffer "$Globals" 32
Float 16 [_Max]
Float 20 [_Min]
BindCB  "$Globals" 0
"ps_4_0
eefiecedcjhpjimpnndbbpoonnidhepjeadimaejabaaaaaammacaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcamacaaaa
eaaaaaaaidaaaaaafjaaaaaeegiocaaaaaaaaaaaacaaaaaafkaaaaadaagabaaa
aaaaaaaafkaaaaadaagabaaaabaaaaaafkaaaaadaagabaaaacaaaaaafkaaaaad
aagabaaaadaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaafibiaaaeaahabaaa
adaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacacaaaaaaefaaaaajpcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaabbaaaaakbcaabaaaaaaaaaaaegaobaaaaaaaaaaa
aceaaaaaaaaaiadpibiaiadlicabibdhimpinfdbefaaaaajpcaabaaaabaaaaaa
egbabaaaabaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaabbaaaaakccaabaaa
aaaaaaaaegaobaaaabaaaaaaaceaaaaaaaaaiadpibiaiadlicabibdhimpinfdb
efaaaaajpcaabaaaabaaaaaaegbabaaaabaaaaaaeghobaaaacaaaaaaaagabaaa
acaaaaaabbaaaaakecaabaaaaaaaaaaaegaobaaaabaaaaaaaceaaaaaaaaaiadp
ibiaiadlicabibdhimpinfdbefaaaaajpcaabaaaabaaaaaaegbabaaaabaaaaaa
eghobaaaadaaaaaaaagabaaaadaaaaaabbaaaaakicaabaaaaaaaaaaaegaobaaa
abaaaaaaaceaaaaaaaaaiadpibiaiadlicabibdhimpinfdbdcaaaaampccabaaa
aaaaaaaaegaobaaaaaaaaaaaagiacaaaaaaaaaaaabaaaaaafgifcaiaebaaaaaa
aaaaaaaaabaaaaaadoaaaaab"
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