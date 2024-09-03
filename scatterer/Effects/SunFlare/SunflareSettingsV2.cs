using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;

namespace Scatterer
{
    public class SunflareSettingsV2
    {
        [Persistent] public int syntaxVersion = 2;
        [Persistent] public Vector3 flareColor = Vector3.one;

        [Persistent] public List<FlareSettings> flares = new List<FlareSettings> {};
        [Persistent] public List<GhostSettings> ghosts = new List<GhostSettings> {};

        public SunflareSettingsV2 ()
        {
        }

        public void Load(ConfigNode node)
        {
            ConfigNode.LoadObjectFromConfig (this, node);

            if (node.HasNode ("flares") && node.GetNode("flares").HasNode("Item"))
            {
                ConfigNode[] flareNodes =  node.GetNode("flares").GetNodes("Item");
                for (int i = 0; i < flareNodes.Length; i++) { flares[i].Load(flareNodes[i]);}
            }

            if (node.HasNode ("ghosts") && node.GetNode("ghosts").HasNode("Item"))
            {
                ConfigNode[] ghostNodes =  node.GetNode("ghosts").GetNodes("Item");
                for (int i = 0; i< ghostNodes.Length; i++) {ghosts[i].Load(ghostNodes[i]);}
            }
        }
    }

    public class FlareSettings
    {
        [Persistent] public string texture;
        [Persistent] public float displayAspectRatio;
        [Persistent] public FloatCurve scaleCurve = new FloatCurve(new [] {new Keyframe(0, 1), new Keyframe(1, 1)});
        [Persistent] public FloatCurve intensityCurve = new FloatCurve(new [] {new Keyframe(0, 1), new Keyframe(1, 1)});

        public void Load(ConfigNode node)
        {
            if (node.HasNode ("scaleCurve"))
                scaleCurve.Load (node.GetNode ("scaleCurve"));

            if (node.HasNode ("intensityCurve"))
                intensityCurve.Load (node.GetNode ("intensityCurve"));
        }
    }

    public class GhostSettings
    {
        [Persistent] public string texture;
        [Persistent] public FloatCurve intensityCurve = new FloatCurve(new [] {new Keyframe(0, 1), new Keyframe(1, 1)});
        [Persistent] public List<GhostInstanceSettings> instances = new List<GhostInstanceSettings> {};

        public void Load(ConfigNode node)
        {
            if (node.HasNode ("intensityCurve"))
                intensityCurve.Load (node.GetNode ("intensityCurve"));
        }
    }

    //not sure this will serialize or reach the depth limit thing
    public class GhostInstanceSettings
    {
        [Persistent] public float intensityMultiplier;
        [Persistent] public float displayAspectRatio;
        [Persistent] public float scale;
        [Persistent] public float sunToScreenCenterPosition;
    }
}

