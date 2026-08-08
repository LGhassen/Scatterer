using UnityEngine;

namespace Scatterer
{
    public class ScaledDepthBufferScatteringContainer
    {
        GameObject scaledDepthScatteringGO;
        MeshRenderer scaledDepthScatteringMR;

        public ScaledDepthBufferScatteringContainer(Material material, Transform parentScaledTransform)
        {
            string goName = "Scatterer scaled depth-buffer atmo";

            var existingGoTransform = parentScaledTransform.FindChild(goName);

            if (existingGoTransform != null)
            {
                GameObject.Destroy(existingGoTransform.gameObject);
            }

            scaledDepthScatteringGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            scaledDepthScatteringGO.name = goName;
            GameObject.Destroy(scaledDepthScatteringGO.GetComponent<Collider>());
            scaledDepthScatteringGO.transform.SetParent(parentScaledTransform, false);
            scaledDepthScatteringGO.transform.localPosition = Vector3.zero;
            scaledDepthScatteringGO.transform.localRotation = Quaternion.identity;
            scaledDepthScatteringGO.transform.localScale = Vector3.one;
            scaledDepthScatteringGO.layer = HighLogic.LoadedScene == GameScenes.MAINMENU ? 15 : 10;

            scaledDepthScatteringGO.GetComponent<MeshFilter>().mesh.bounds = new Bounds(Vector3.zero, new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));
            scaledDepthScatteringGO.AddComponent<ScaledDepthBufferScatteringScreenCopy>();

            scaledDepthScatteringMR = scaledDepthScatteringGO.GetComponent<MeshRenderer>();
            scaledDepthScatteringMR.sharedMaterial = material;
            scaledDepthScatteringMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            scaledDepthScatteringMR.receiveShadows = false;
            scaledDepthScatteringMR.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            scaledDepthScatteringMR.enabled = true;
        }

        public void SetEnabled(bool value)
        {
            scaledDepthScatteringMR.enabled = value;
        }
        
        public void Cleanup()
        {
            if (scaledDepthScatteringGO != null)
            {
                scaledDepthScatteringGO.SetActive(false);
                scaledDepthScatteringMR.enabled = false;
                UnityEngine.Object.Destroy(scaledDepthScatteringGO);
            }
        }
    }

    public class ScaledDepthBufferScatteringScreenCopy : MonoBehaviour
    {
        void OnWillRenderObject()
        {
            Camera cam = Camera.current;
            
            if (!cam)
                return;

            ScreenCopyCommandBuffer.EnableScreenCopyForFrame (cam);
        }
    }
}
