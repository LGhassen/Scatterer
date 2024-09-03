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
    public class GUIModuleLabel : AbstractGUIModule
    {
        string label = "";
        
        public GUIModuleLabel (string label)
        {
            this.label = label;
        }
        
        public override void RenderGUI()
        {
            GUILayout.BeginHorizontal ();
            GUILayout.Label (label);
            GUILayout.EndHorizontal ();
        }
    }
}