using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using UnityEngine;

namespace Scatterer
{
    public class GUIModuleVector4  : AbstractGUIModule
    {
        Vector4 localVariable = new Vector4(0f,0f,0f,0f);
        string label = "";

        object targetObject;
        FieldInfo targetField;

        public GUIModuleVector4 (string label, object targetObject, string fieldName)
        {
            this.label = label;
            this.targetObject = targetObject;

            Type targetType = targetObject.GetType ();

            this.targetField = targetType.GetField(fieldName, Flags);
            this.localVariable = (Vector4) targetField.GetValue(targetObject);
        }

        public override void RenderGUI()
        {

            GUILayout.BeginHorizontal ();

            GUILayout.Label (label);

            localVariable.x = float.Parse (GUILayout.TextField (localVariable.x.ToString ()));
            localVariable.y = float.Parse (GUILayout.TextField (localVariable.y.ToString ()));
            localVariable.z = float.Parse (GUILayout.TextField (localVariable.z.ToString ()));
            localVariable.w = float.Parse (GUILayout.TextField (localVariable.w.ToString ()));

            if (GUILayout.Button ("Set"))
            {
                targetField.SetValue(targetObject, localVariable);
            }
            GUILayout.EndHorizontal ();
        }
    }
}