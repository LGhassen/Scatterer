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

        public void UpdateScreenBounds(Camera targetCamera, Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix,
                                       Vector3 atmosphereCenter, float atmosphereRadius)
        {
            Vector3 viewCenter = viewMatrix.MultiplyPoint3x4(atmosphereCenter);
            Vector4 screenBounds;

            if (viewCenter.z - atmosphereRadius >= 0f)
            {
                screenBounds = new Vector4(2f, 2f, 2f, 2f);
            }
            else if (viewCenter.sqrMagnitude <= atmosphereRadius * atmosphereRadius || viewCenter.z + atmosphereRadius >= -targetCamera.nearClipPlane)
            {
                screenBounds = new Vector4(-1f, -1f, 1f, 1f);
            }
            else
            {
                Vector2 horizontalTangents = GetProjectedTangentBounds(
                    new Vector2(viewCenter.x, viewCenter.z), viewCenter, atmosphereRadius, projectionMatrix, true);
                Vector2 verticalTangents = GetProjectedTangentBounds(
                    new Vector2(viewCenter.y, viewCenter.z), viewCenter, atmosphereRadius, projectionMatrix, false);

                float horizontalPadding = 2f / Mathf.Max(1, targetCamera.pixelWidth);
                float verticalPadding = 2f / Mathf.Max(1, targetCamera.pixelHeight);
                screenBounds = new Vector4(horizontalTangents.x - horizontalPadding,
                                           verticalTangents.x - verticalPadding,
                                           horizontalTangents.y + horizontalPadding,
                                           verticalTangents.y + verticalPadding);
            }

            scaledDepthScatteringMR.sharedMaterial.SetVector(ShaderProperties._scaledAtmosphereScreenBounds_PROPERTY, screenBounds);
        }

        private static Vector2 GetProjectedTangentBounds(Vector2 circleCenter, Vector3 viewCenter, float radius,
                                                          Matrix4x4 projectionMatrix, bool horizontal)
        {
            float centerDistanceSquared = circleCenter.sqrMagnitude;
            float tangentDistance = Mathf.Sqrt(centerDistanceSquared - radius * radius);
            Vector2 tangentCenter = circleCenter * ((centerDistanceSquared - radius * radius) / centerDistanceSquared);
            Vector2 tangentOffset = new Vector2(-circleCenter.y, circleCenter.x)
                * (radius * tangentDistance / centerDistanceSquared);

            Vector2 tangentA = tangentCenter + tangentOffset;
            Vector2 tangentB = tangentCenter - tangentOffset;
            Vector3 pointA = horizontal
                ? new Vector3(tangentA.x, viewCenter.y, tangentA.y)
                : new Vector3(viewCenter.x, tangentA.x, tangentA.y);
            Vector3 pointB = horizontal
                ? new Vector3(tangentB.x, viewCenter.y, tangentB.y)
                : new Vector3(viewCenter.x, tangentB.x, tangentB.y);

            Vector4 clipA = projectionMatrix * new Vector4(pointA.x, pointA.y, pointA.z, 1f);
            Vector4 clipB = projectionMatrix * new Vector4(pointB.x, pointB.y, pointB.z, 1f);
            float projectedA = horizontal ? clipA.x / clipA.w : clipA.y / clipA.w;
            float projectedB = horizontal ? clipB.x / clipB.w : clipB.y / clipB.w;

            return new Vector2(Mathf.Min(projectedA, projectedB), Mathf.Max(projectedA, projectedB));
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
