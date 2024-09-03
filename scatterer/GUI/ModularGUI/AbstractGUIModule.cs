//Generic GUIModule, to be overriden by concrete classes that provide their own implementation of RenderGUI

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
    public abstract class AbstractGUIModule
    {
        static protected BindingFlags Flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        
        public AbstractGUIModule ()
        {
        }
        
        public abstract void RenderGUI ();
    }
}