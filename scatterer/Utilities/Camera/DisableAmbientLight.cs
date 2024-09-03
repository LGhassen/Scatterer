using UnityEngine;

namespace Scatterer
{

    public class DisableAmbientLight : MonoBehaviour
    {
        Color ambientLight, originalAmbientLight;

        private void Awake()
        {
            ambientLight = Color.black;
        }

        public void OnPreRender()
        {
            originalAmbientLight = RenderSettings.ambientLight;
            RenderSettings.ambientLight = ambientLight;
        }

        public void OnPostRender()
        {
            RestoreLight ();
        }

        public void RestoreLight()
        {
            RenderSettings.ambientLight = originalAmbientLight;
        }

        public void OnDestroy()
        {
            RestoreLight();
        }
    }
}

