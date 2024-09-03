using UnityEngine;
using System.Collections.Generic;

namespace Scatterer
{
    public class SunlightModulatorsManager
    {
        private static SunlightModulatorsManager instance = null;

        public static SunlightModulatorsManager Instance
        {
            get
            {
                if (instance == null)
                { 
                    instance = new SunlightModulatorsManager();
                }

                return instance;
            }
        }

        private static Dictionary<Light, SunlightModulator> modulatorsDictionary = new Dictionary<Light, SunlightModulator>();

        public static void AddRenderingHookToCamera(Camera camera)
        {
            if (camera != null)
            {
                var hook = camera.GetComponent<SunlightModulatorCameraRenderingHook>();

                if (hook == null)
                {
                    camera.gameObject.AddComponent<SunlightModulatorCameraRenderingHook>();
                }
            }
        }

        public static void AddResetHookToCamera(Camera camera)
        {
            if (camera != null)
            {
                var hook = camera.GetComponent<SunlightModulatorResetHook>();

                if (hook == null)
                {
                    camera.gameObject.AddComponent<SunlightModulatorResetHook>();
                }
            }
        }

        private SunlightModulator FindOrCreateModulator(Light light)
        {
            if (modulatorsDictionary.ContainsKey(light))
            {
                return modulatorsDictionary[light];
            }
            else
            {
                modulatorsDictionary[light] = new SunlightModulator();
                modulatorsDictionary[light].Init(light);

                return modulatorsDictionary[light];
            }
        }

        public void ModulateByAttenuation(Light light, float inAttenuation)
        {
            FindOrCreateModulator (light).ModulateByAttenuation (inAttenuation);
        }
        
        public void ModulateByColor(Light light, Color inColor)
        {
            FindOrCreateModulator (light).ModulateByColor (inColor);
        }
        
        public Color GetLastModulationColor(Light light)
        {
            return FindOrCreateModulator (light).LastModulationColor;
        }

        public void CamereOnPrecull()
        {
            foreach(var modulator in modulatorsDictionary.Values)
            {
                modulator.StoreOriginalColor();
                modulator.ApplyColorModulation();
            }
        }

        public void CameraOnPostRender()
        {
            foreach (var modulator in modulatorsDictionary.Values)
            {
                modulator.RestoreOriginalColor();
            }
        }

        public void ResetModulation()
        {
            foreach (var modulator in modulatorsDictionary.Values)
            {
                modulator.ResetModulation();
            }
        }
    }

    public class SunlightModulatorResetHook : MonoBehaviour
    {
        void OnPreCull()
        {
            SunlightModulatorsManager.Instance.ResetModulation();
        }
    }

    public class SunlightModulatorCameraRenderingHook : MonoBehaviour
    {
        void OnPreCull()
        {
            SunlightModulatorsManager.Instance.CamereOnPrecull();
        }

        void OnPostRender()
        {
            SunlightModulatorsManager.Instance.CameraOnPostRender();
        }
    }

    public class SunlightModulator
    {
        private Color originalColor = Color.white;
        private Color modulationColor = Color.white;
        private Color lastModulationColor;

        Light sunLight;
        bool applyModulation = false;
        bool originalColorStored = false;

        public Color LastModulationColor { get => lastModulationColor; }

        public void Init(Light light)
        {
            sunLight = light;
        }

        public void StoreOriginalColor()
        {
            if (sunLight != null && sunLight.color != Color.black)
            {
                originalColor = sunLight.color;
                originalColorStored = true;
            }
        }

        public void ModulateByAttenuation(float inAttenuation)
        {
            modulationColor *= inAttenuation;
            applyModulation = true;
        }

        public void ModulateByColor(Color inColor)
        {
            modulationColor *= inColor;
            applyModulation = true;
        }

        public void ApplyColorModulation()
        {
            if (sunLight != null && applyModulation && originalColorStored)
            {
                sunLight.color = modulationColor * originalColor;
                lastModulationColor = sunLight.color;
            }
        }
        
        public void RestoreOriginalColor()
        {
            if (sunLight != null && applyModulation && originalColorStored)
            {
                sunLight.color = originalColor;
            }
        }

        public void ResetModulation()
        {
            applyModulation = false;
            modulationColor = Color.white;
        }
    }
}

