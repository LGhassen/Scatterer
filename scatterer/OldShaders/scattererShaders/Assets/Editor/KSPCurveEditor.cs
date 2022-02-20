using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuMech
{

    public class KSPCurveEditor : EditorWindow
    {
        [MenuItem("KSP/Curve Editor")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(KSPCurveEditor));
        }

        AnimationCurve curve = new AnimationCurve();
        Vector2 scrollPos = new Vector2();
        string textVersion;
        List<FloatString4> points = new List<FloatString4>();
        bool curveNeedsUpdate = false, textChanged = false;
        float lastCurve = 0;

        void OnGUI()
        {
            textChanged = false;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(50));
            EditorGUILayout.LabelField("Click curve to edit ->", GUILayout.ExpandWidth(false));
            curve = EditorGUILayout.CurveField(curve, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndHorizontal();

            string newT = EditorGUILayout.TextArea(textVersion, GUILayout.ExpandWidth(true), GUILayout.Height(100));
            if (newT != textVersion)
            {
                textVersion = newT;
                textChanged = true;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            GUILayout.BeginVertical();
            GUILayout.Label("X", GUILayout.ExpandWidth(true));
            foreach (FloatString4 p in points)
            {
                string ns = EditorGUILayout.TextField(p.strings[0]);
                if (ns != p.strings[0])
                {
                    p.strings[0] = ns;
                    p.UpdateFloats();
                    curveNeedsUpdate = true;
                }

            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Y", GUILayout.ExpandWidth(true));
            foreach (FloatString4 p in points)
            {
                string ns = EditorGUILayout.TextField(p.strings[1]);
                if (ns != p.strings[1])
                {
                    p.strings[1] = ns;
                    p.UpdateFloats();
                    curveNeedsUpdate = true;
                }

            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Auto", GUILayout.ExpandWidth(true));
            foreach (FloatString4 p in points)
            {
                bool a = EditorGUILayout.Toggle(p.twoMode);
                if (a != p.twoMode)
                {
                    p.twoMode = a;
                    curveNeedsUpdate = true;
                }

            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("In Tangent", GUILayout.ExpandWidth(true));
            foreach (FloatString4 p in points)
            {
                if (!p.twoMode)
                {
                    string ns = EditorGUILayout.TextField(p.strings[2]);
                    if (ns != p.strings[2])
                    {
                        p.strings[2] = ns;
                        p.UpdateFloats();
                        curveNeedsUpdate = true;
                    }
                }
                else
                {
                    EditorGUILayout.TextField("");
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Out Tangent", GUILayout.ExpandWidth(true));
            foreach (FloatString4 p in points)
            {
                if (!p.twoMode)
                {
                    string ns = EditorGUILayout.TextField(p.strings[3]);
                    if (ns != p.strings[3])
                    {
                        p.strings[3] = ns;
                        p.UpdateFloats();
                        curveNeedsUpdate = true;
                    }
                }
                else
                {
                    EditorGUILayout.TextField("");
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        static float HashAnimationCurve(AnimationCurve c)
        {
            float h = 0;

            foreach (Keyframe k in c.keys)
            {
                h += k.time + k.value + k.inTangent + k.outTangent + k.tangentMode;
            }

            return h;
        }

        void Update()
        {
            if (textChanged)
            {
                StringToCurve(textVersion);
            }

            if (curveNeedsUpdate)
            {
                UpdateCurve();
            }

            float newCurve = HashAnimationCurve(curve);
            if (lastCurve != newCurve)
            {
                points = new List<FloatString4>();

                for (int i = 0; i < curve.keys.Length; i++)
                {
                    FloatString4 nv = new FloatString4(curve.keys[i].time, curve.keys[i].value, curve.keys[i].inTangent, curve.keys[i].outTangent, (curve.keys[i].tangentMode == 10));
                    nv.UpdateStrings();
                    points.Add(nv);
                }

                if (!textChanged)
                {
                    textVersion = CurveToString();
                }

                lastCurve = newCurve;
            }
        }

        string CurveToString()
        {
            string buff = "";
            foreach (FloatString4 p in points)
            {
                if (p.twoMode)
                {
                    buff += "key = " + p.floats.x + " " + p.floats.y + "\n";
                }
                else
                {
                    buff += "key = " + p.floats.x + " " + p.floats.y + " " + p.floats.z + " " + p.floats.w + "\n";
                }
            }
            return buff;
        }

        void StringToCurve(string data)
        {
            points = new List<FloatString4>();

            string[] lines = data.Split('\n');
            foreach (string line in lines)
            {
                string[] pcs = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if ((pcs.Length >= 3) && (pcs[0] == "key"))
                {
                    FloatString4 nv = new FloatString4();
                    if (pcs.Length >= 5)
                    {
                        nv.strings = new string[] { pcs[1], pcs[2], pcs[3], pcs[4] };
                        nv.twoMode = false;
                    }
                    else
                    {
                        nv.strings = new string[] { pcs[1], pcs[2], "0", "0" };
                        nv.twoMode = true;
                    }
                    nv.UpdateFloats();
                    points.Add(nv);
                }
            }

            if (!textChanged)
            {
                textVersion = CurveToString();
            }

            curveNeedsUpdate = true;
        }

        void UpdateCurve()
        {
            points.Sort();

            curve = new AnimationCurve();

            foreach (FloatString4 v in points)
            {
                Keyframe k = new Keyframe(v.floats.x, v.floats.y, v.floats.z, v.floats.w);
                if (v.twoMode)
                {
                    k.tangentMode = 10;
                }
                else
                {
                    k.tangentMode = 1;
                }
                curve.AddKey(k);
            }

            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (points[i].twoMode)
                {
                    curve.SmoothTangents(i, 0);
                }
            }

            if (!textChanged)
            {
                textVersion = CurveToString();
            }

            lastCurve = HashAnimationCurve(curve);
            curveNeedsUpdate = false;
        }
    }

    public class FloatString4 : IComparable<FloatString4>
    {
        public Vector4 floats;
        public string[] strings;
        public bool twoMode;

        public int CompareTo(FloatString4 other)
        {
            if (other == null) return 1;
            return floats.x.CompareTo(other.floats.x);
        }

        public FloatString4()
        {
            floats = new Vector4();
            strings = new string[] { "0", "0", "0", "0" };
            twoMode = false;
        }

        public FloatString4(float x, float y, float z = 0, float w = 0, bool twoMode = true)
        {
            floats = new Vector4(x, y, z, w);
            this.twoMode = twoMode;
            UpdateStrings();
        }

        public void UpdateFloats()
        {
            float x, y, z, w;
            float.TryParse(strings[0], out x);
            float.TryParse(strings[1], out y);
            float.TryParse(strings[2], out z);
            float.TryParse(strings[3], out w);
            floats = new Vector4(x, y, z, w);
        }

        public void UpdateStrings()
        {
            strings = new string[] { floats.x.ToString(), floats.y.ToString(), floats.z.ToString(), floats.w.ToString() };
        }
    }
}
