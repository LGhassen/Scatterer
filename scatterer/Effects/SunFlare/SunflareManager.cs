using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scatterer
{
    public class SunflareManager : MonoBehaviour
    {
        public Dictionary<String, SunFlare> scattererSunFlares = new Dictionary<String, SunFlare>();

        public SunflareManager ()
        {
        }

        public void Init()
        {
            StartCoroutine (InitCoroutine());
        }


        IEnumerator InitCoroutine()
        {
            foreach (ConfigNode _sunflareConfigs in Scatterer.Instance.planetsConfigsReader.sunflareConfigs)
            {
                foreach (ConfigNode _cn in _sunflareConfigs.GetNodes())
                {
                    if (scattererSunFlares.ContainsKey(_cn.name))
                        continue;

                    SunFlare customSunFlare = (SunFlare)Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SunFlare));
                    try
                    {
                        customSunFlare.Configure(FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == _cn.name),
                                                 _cn.name,Utils.GetScaledTransform (_cn.name), _cn);
                        customSunFlare.start ();
                        scattererSunFlares.Add (_cn.name, customSunFlare);
                    }
                    catch (Exception exception)
                    {
                        Utils.LogDebug ("Custom sunflare cannot be added to " + _cn.name + " " + exception.ToString ());
                        Component.Destroy (customSunFlare);
                        UnityEngine.Object.Destroy (customSunFlare);
                        continue;
                    }
                    yield return new WaitForFixedUpdate ();
                }
            }
            DisableStockSunflares ();
        }
        
        //TODO: decouple and let every sunflare update itself, based on the GameObject it is linked to?
        public void UpdateFlares()
        {
            foreach (SunFlare customSunFlare in scattererSunFlares.Values)
            {
                customSunFlare.Update();
            }
        }

        public void OnDestroy()
        {
            if (this)
                StopAllCoroutines();

            ReenableStockSunflares ();

            foreach (SunFlare customSunFlare in scattererSunFlares.Values)
            {
                Component.Destroy (customSunFlare);
            }
        }
        
        void DisableStockSunflares ()
        {
            global::SunFlare[] stockFlares = (global::SunFlare[])global::SunFlare.FindObjectsOfType (typeof(global::SunFlare));
            foreach (global::SunFlare _flare in stockFlares)
            {
                if (scattererSunFlares.ContainsKey (_flare.sun.name))
                {
                    Utils.LogDebug ("Disabling stock sunflare for " + _flare.sun.name);
                    _flare.sunFlare.enabled = false;
                    _flare.enabled = false;
                    _flare.gameObject.SetActive (false);
                }
            }
        }
        
        void ReenableStockSunflares ()
        {
            global::SunFlare[] stockFlares = (global::SunFlare[]) global::SunFlare.FindObjectsOfType(typeof( global::SunFlare));
            foreach(global::SunFlare _flare in stockFlares)
            {                        
                if (scattererSunFlares.ContainsKey (_flare.sun.name))
                {
                    _flare.sunFlare.enabled=true;
                }
            }
        }
    }
}

