//A modular gui class to simplify writing a GUI
//Define the gui once and have render everything
//Submodules can be used to access and modify private fields of the main types, and hold their own local variables so I don't have to juggle them

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
    public class ModularGUI
    {

        List<AbstractGUIModule> Modules = new List<AbstractGUIModule>();

        public ModularGUI ()
        {

        }

        public void RenderGUI()
        {
            foreach (AbstractGUIModule module in Modules)
            {
                module.RenderGUI();
            }
        }

        public void AddModule(AbstractGUIModule module)
        {
            this.Modules.Add (module);
        }

        public void ClearModules()
        {
            Modules.Clear ();
        }
    }
}

