using UnityEngine;

namespace Scatterer
{
    public class SunflareCameraHook : MonoBehaviour
    {
        public SunFlare flare;
        public float useDbufferOnCamera;

        public SunflareCameraHook ()
        {
        }

        public void OnPreRender()
        {
            if(flare)
            {
                flare.UpdateProperties ();
                flare.sunglareMaterial.SetFloat(ShaderProperties.renderOnCurrentCamera_PROPERTY,1.0f);
                flare.sunglareMaterial.SetFloat(ShaderProperties.useDbufferOnCamera_PROPERTY,useDbufferOnCamera);
            }
        }

        public void OnPostRender()
        {
            if(flare)
            {
                flare.ClearExtinction ();
                flare.sunglareMaterial.SetFloat(ShaderProperties.renderOnCurrentCamera_PROPERTY,0.0f);
                flare.sunglareMaterial.SetFloat(ShaderProperties.useDbufferOnCamera_PROPERTY,useDbufferOnCamera);
            }
        }
    }
}

