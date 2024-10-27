using System;
using System.Reflection;
using UnityEngine;

namespace Scatterer
{
    public class DeferredReflectionHandler
    {
        private static DeferredReflectionHandler instance;

        public static DeferredReflectionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DeferredReflectionHandler();
                    instance.Initialize();
                }

                return instance;
            }
        }

        bool deferredInstalled = false;
        bool deferredSSRInstalled = false;

        public bool DeferredInstalled { get => deferredInstalled; }
        public bool DeferredSSRInstalled { get => deferredSSRInstalled; }

        private Func<Camera, bool, bool> registerOceanSSRDelegate;
        private Func<Camera, bool> unregisterOceanSSRDelegate;

        public bool RegisterScattererOceanForCamera(Camera cam, out bool halfResolution)
        {
            halfResolution = false; // This isn't working correctly, always false, the delegate out param isn't working

            if (deferredSSRInstalled)
            { 
                bool returnValue = registerOceanSSRDelegate(cam, halfResolution);
                return returnValue;
            }

            return false;
        }

        public bool UnregisterScattererOceanForCamera(Camera cam)
        {
            if (deferredSSRInstalled)
            { 
                return unregisterOceanSSRDelegate(cam);
            }

            return false;
        }

        private void Initialize()
        {
            var deferredType = ReflectionUtils.getType("Deferred.Deferred");

            if (deferredType == null)
            {
                return;
            }

            deferredInstalled = true;

            var deferredSSRType = ReflectionUtils.getType("Deferred.ScreenSpaceReflections");

            if (deferredSSRType == null)
            {
                return;
            }

            deferredSSRInstalled = true;

            MethodInfo registerMethodInfo = deferredSSRType.GetMethod("RegisterScattererOceanForCamera");
            MethodInfo unregisterMethodInfo = deferredSSRType.GetMethod("UnregisterScattererOceanForCamera");

            if (registerMethodInfo != null && unregisterMethodInfo != null)
            {
                registerOceanSSRDelegate = (cam, outValue) =>
                {
                    object[] parameters = { cam, false };
                    bool result = (bool)registerMethodInfo.Invoke(null, parameters);
                    outValue = (bool)parameters[1]; // Capture the out parameter
                    return result;
                };

                unregisterOceanSSRDelegate = (Func<Camera, bool>)Delegate.CreateDelegate(typeof(Func<Camera, bool>), unregisterMethodInfo);
            }
        }
    }
}