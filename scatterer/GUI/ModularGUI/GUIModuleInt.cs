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
	public class GUIModuleInt  : AbstractGUIModule
	{
		int localVariable = 0;
		string label = "";

		object targetObject;
		FieldInfo targetField;

		public GUIModuleInt (string label, object targetObject, string fieldName)
		{
			this.label = label;
			this.targetObject = targetObject;

			Type targetType = targetObject.GetType ();

			this.targetField = targetType.GetField(fieldName, Flags);
			this.localVariable = (int) targetField.GetValue(targetObject);
		}

		public override void RenderGUI()
		{

			GUILayout.BeginHorizontal ();

			GUILayout.Label (label);
			localVariable = (Int32)(Convert.ToInt32 (GUILayout.TextField (localVariable.ToString ())));
			if (GUILayout.Button ("Set"))
			{
				targetField.SetValue(targetObject, localVariable);
			}
			GUILayout.EndHorizontal ();
		}
	}
}