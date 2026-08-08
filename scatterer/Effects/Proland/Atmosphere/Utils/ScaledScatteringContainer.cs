using UnityEngine;

namespace Scatterer
{
    public class ScaledScatteringContainer
    {
        GameObject scaledScatteringGO;
        MeshRenderer scaledScatteringMR;

        public MeshRenderer MeshRenderer { get { return scaledScatteringMR; } }

        Transform parentLocalTransform;
        Transform parentScaledTransform;

        public ScaledScatteringContainer(Mesh planetMesh, Material material, Transform parentLocalTransform, Transform parentScaledTransform)
        {
            this.parentLocalTransform = parentLocalTransform;
            this.parentScaledTransform = parentScaledTransform;

            string goName = "Scatterer scaled atmo fallback";
            var existingGoTransform = parentScaledTransform.FindChild(goName);

            if (existingGoTransform != null)
            {
                GameObject.Destroy(existingGoTransform.gameObject);
            }

            scaledScatteringGO = new GameObject(goName);
            scaledScatteringGO.transform.SetParent(parentScaledTransform, false);

            MeshFilter meshFilter = scaledScatteringGO.AddComponent<MeshFilter>();
            meshFilter.mesh = (Mesh)Mesh.Instantiate(planetMesh);

            scaledScatteringMR = scaledScatteringGO.AddComponent<MeshRenderer>();
            scaledScatteringMR.sharedMaterial = material;
            Utils.EnableOrDisableShaderKeywords(scaledScatteringMR.sharedMaterial, "LOCAL_MODE_ON", "LOCAL_MODE_OFF", false);
            scaledScatteringMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            scaledScatteringMR.receiveShadows = false;
            scaledScatteringMR.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;

            scaledScatteringGO.layer = HighLogic.LoadedScene == GameScenes.MAINMENU ? 15 : 10;
            scaledScatteringGO.AddComponent<ScaledScatteringScreenCopy>();
        }

        public void ApplyNewMesh(Mesh planetMesh)
        {
            MeshFilter meshFilter = scaledScatteringGO.GetComponent<MeshFilter>();
            meshFilter.mesh.Clear();
            meshFilter.mesh = (Mesh)Mesh.Instantiate(planetMesh);
        }

        public void SwitchLocalMode()
        {
            scaledScatteringGO.layer = 15;
            scaledScatteringGO.transform.localScale = parentScaledTransform.localScale * ScaledSpace.ScaleFactor;
            scaledScatteringGO.transform.localPosition = Vector3.zero;
            scaledScatteringGO.transform.localRotation = Quaternion.identity;
            scaledScatteringGO.transform.SetParent(parentLocalTransform, false);

            Utils.EnableOrDisableShaderKeywords(scaledScatteringMR.sharedMaterial, "LOCAL_MODE_ON", "LOCAL_MODE_OFF", true);
        }

        public void SetEnabled(bool value)
        {
            scaledScatteringMR.enabled = value;
        }

        public void Cleanup()
        {
            if (scaledScatteringGO != null)
            {
                scaledScatteringGO.SetActive(false);
                scaledScatteringMR.enabled = false;
                UnityEngine.Object.Destroy(scaledScatteringGO);
            }
        }
    }

    public class ScaledScatteringScreenCopy : MonoBehaviour
    {
        void OnWillRenderObject()
        {
            Camera cam = Camera.current;

            if (!cam)
                return;

            ScreenCopyCommandBuffer.EnableScreenCopyForFrame(cam);
        }
    }
}
