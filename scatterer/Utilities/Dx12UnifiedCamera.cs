//Enables unified camera mode on Directx12, Dx12 still runs slower than Dx11 on both CPU and GPU for some reason (higher loads and lower framerates), also ocean shader causes crash
using System;
using UnityEngine;
using System.Reflection;

namespace Scatterer
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class Dx12UnifiedCamera : MonoBehaviour
    {
        public void Start()
        {
            if (SystemInfo.graphicsDeviceVersion.Contains ("Direct3D 12"))
            {
                Type t = typeof(GameSettings);
                BindingFlags Flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                t.GetField ("graphicsVersion", Flags).SetValue (null, GameSettings.GraphicsType.D3D11);
            }
        }
    }
}

