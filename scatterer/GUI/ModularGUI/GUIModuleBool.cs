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
	public class GUIModuleBool  : AbstractGUIModule
	{
		bool localVariable = false;
		string label = "";

		object targetObject;
		FieldInfo targetField;

		public GUIModuleBool (string label, object targetObject, string fieldName)
		{
			this.label = label;
			this.targetObject = targetObject;

			Type targetType = targetObject.GetType ();

			this.targetField = targetType.GetField(fieldName, Flags);
			this.localVariable = (bool) targetField.GetValue(targetObject);
		}

		public override void RenderGUI()
		{

			GUILayout.BeginHorizontal ();

			GUILayout.Label (label);
			GUILayout.TextField (targetField.GetValue(targetObject).ToString ());
			if (GUILayout.Button ("Toggle"))
			{
				targetField.SetValue(targetObject, !(bool)targetField.GetValue(targetObject));
			}
			GUILayout.EndHorizontal ();
		}
	}
}