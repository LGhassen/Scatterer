using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Scatterer
{
    public class ScattererCelestialBody
    {
        [Persistent] public string celestialBodyName;
        [Persistent] public string transformName;
        [Persistent] public float loadDistance;
        [Persistent] public float unloadDistance;
        [Persistent] public bool hasOcean;
        [Persistent] public bool usesCloudIntegration;
        [Persistent] public bool cloudIntegrationUsesScattererSunColors = false;
        [Persistent] public bool flatScaledSpaceModel;

        [Persistent] public string mainSunCelestialBody;
        [Persistent] public bool sunsUseIntensityCurves = false;
        [Persistent] public Color sunColor = Color.white;
        [Persistent] public List<string> eclipseCasters=new List<string> {};

        [Persistent] public List<SecondarySunConfig> secondarySuns=new List<SecondarySunConfig> {};

        [Persistent] public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};
            
        public CelestialBody celestialBody;
        public Transform transform;
        public bool isFound = false;
        public bool active;
        public ProlandManager prolandManager;
        
        public ScattererCelestialBody ()
        {
            
        }
    }

    public class SecondarySunConfig
    {
        [Persistent] public string celestialBodyName;
        [Persistent] public Color sunColor = Color.white;
        //[Persistent] public List<string> eclipseCasters=new List<string> {};    //maybe don't do this at the start? or at all, especially since we will half-ass it by not being able to eclipse light on terrain anyway
    }

    public class SecondarySun
    {
        public SecondarySunConfig config;
        public CelestialBody celestialBody;
        public Light sunLight = null;
        public Light scaledSunLight = null;
        //public List<CelestialBody> eclipseCasters=new List<CelestialBody> {};

        public SecondarySun (SecondarySunConfig inConfig, CelestialBody body)
        {
            config = inConfig;
            celestialBody = body;
        }

        public static SecondarySun FindSecondarySun (SecondarySunConfig inConfig)
        {
            var celestialBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == inConfig.celestialBodyName);
            if (celestialBody == null)
            {
                Utils.LogError ("Secondary sun " + inConfig.celestialBodyName + " not found, will be unavailable for atmo/ocean effects");
                return null;
            }
            else
            {
                SecondarySun secondarySun = new SecondarySun(inConfig, celestialBody);
                return secondarySun;
            }
        }
    }
}

