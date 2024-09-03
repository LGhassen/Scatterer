using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering;

namespace Scatterer
{
    public class GPUWaveInteractionHandler
    {
        private ComputeShader findHeightsShader;
        private List<PartBuoyancy> partsBuoyancies = new List<PartBuoyancy>();
        private bool heightsRequestInProgress = false;
        private bool cameraHeightRequested = false;
        private float[] heights = { };
        private ComputeBuffer positionsBuffer, heightsBuffer;
        private int frameLatencyCounter = 1;

        private bool paused = false;
        private KSP.UI.Screens.AltimeterSliderButtons altimeterRecoveryButton;
        private bool altimeterRecoveryButtonOverriden = false;
        private MethodInfo recoveryButtonSetUnlockMethod;
        private object[] setUnlockParametersArray;

        private float maxWaveInteractionShipAltitude = 500.0f;
        private bool isHomeworld = false;

        public GPUWaveInteractionHandler(float inMaxWaveInteractionShipAltitude, bool inIsHomeworld)
        {
            maxWaveInteractionShipAltitude = inMaxWaveInteractionShipAltitude;
            isHomeworld = inIsHomeworld;

            findHeightsShader = ShaderReplacer.Instance.LoadedComputeShaders["FindHeights"];

            if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideWaterCrashTolerance)
            {
                PhysicsGlobals.BuoyancyCrashToleranceMult = Scatterer.Instance.mainSettings.buoyancyCrashToleranceMultOverride;
            }

            if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideDrag)
            {
                PhysicsGlobals.BuoyancyWaterDragTimer = double.NegativeInfinity; // this is needed to avoid the excessive drag that is applied when first hitting the water and which basically kills seaplanes with the smallest waves
            }

            if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideRecoveryVelocity)
            {
                GameEvents.onGamePause.Add(ForcePauseMenuSaving);
                GameEvents.onGameUnpause.Add(UnPause);

                if (inIsHomeworld)
                {
                    KSP.UI.Screens.AltimeterSliderButtons[] sliderButtons = Resources.FindObjectsOfTypeAll<KSP.UI.Screens.AltimeterSliderButtons>();
                    if (sliderButtons.Length > 0)
                    {
                        altimeterRecoveryButton = sliderButtons[0];

                        BindingFlags Flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                        recoveryButtonSetUnlockMethod = altimeterRecoveryButton.GetType().GetMethod("setUnlock", Flags);
                        if (recoveryButtonSetUnlockMethod == null)
                        {
                            Utils.LogError("No setUnlock method found in AltimeterSliderButtons");
                            altimeterRecoveryButton = null;
                            return;
                        }

                        setUnlockParametersArray = new object[] { 2 }; //The state for unlocking the altimeterSliderButtons
                    }
                }
            }
        }

        public void SetMaterialProperties(Vector4 choppyness, Vector4 gridSizes, RenderTexture map0, RenderTexture map3, RenderTexture map4)
        {
            findHeightsShader.SetVector(ShaderProperties._Ocean_Choppyness_PROPERTY, choppyness);
            findHeightsShader.SetVector(ShaderProperties._Ocean_GridSizes_PROPERTY, gridSizes);

            findHeightsShader.SetTexture(0, ShaderProperties._Ocean_Map0_PROPERTY, map0);
            findHeightsShader.SetTexture(0, ShaderProperties._Ocean_Map3_PROPERTY, map3);
            findHeightsShader.SetTexture(0, ShaderProperties._Ocean_Map4_PROPERTY, map4);
        }

        public float UpdateInteractions(double cameraHeight, float waterHeightAtCameraPosition, Vector3 ux, Vector3 uy, Vector3 offsetVector3)
        {
            UpdateRecoveryButton();
            return UpdateHeights(cameraHeight, waterHeightAtCameraPosition, ux, uy, offsetVector3);
        }

        private float UpdateHeights(double cameraHeight, float waterHeightAtCameraPosition, Vector3 ux, Vector3 uy, Vector3 offsetVector3)
        {
            if (!heightsRequestInProgress)
            {
                waterHeightAtCameraPosition = ApplyWaterLevelHeights(waterHeightAtCameraPosition);

                List<Vector2> positionsList = new List<Vector2>();
                BuildPartsPositionsList(positionsList, partsBuoyancies, ux, uy, offsetVector3);
                AddCameraPosition(positionsList, cameraHeight, new Vector2(offsetVector3.x, offsetVector3.y));
                RequestAsyncWaterLevelHeights(positionsList);
            }
            else
            {
                frameLatencyCounter++;
            }

            return waterHeightAtCameraPosition;
        }

        private void UpdateRecoveryButton()
        {
            if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideRecoveryVelocity && !paused && FlightGlobals.ActiveVessel != null)
            {
                if ((altimeterRecoveryButton != null) && (FlightGlobals.ActiveVessel.altitude <= Mathf.Abs(maxWaveInteractionShipAltitude))
                    && (FlightGlobals.ActiveVessel.IsClearToSave() == ClearToSaveStatus.CLEAR) && (FlightGlobals.ActiveVessel.situation == Vessel.Situations.SPLASHED)
                    && (FlightGlobals.ActiveVessel.srf_velocity.sqrMagnitude < Scatterer.Instance.mainSettings.waterMaxRecoveryVelocity * Scatterer.Instance.mainSettings.waterMaxRecoveryVelocity))
                {
                    FlightGlobals.ActiveVessel.srf_velocity = Vector3d.zero;

                    if (!altimeterRecoveryButton.led.IsOn || (altimeterRecoveryButton.led.color != KSP.UI.Screens.LED.colorIndices.green))
                    {
                        recoveryButtonSetUnlockMethod.Invoke(altimeterRecoveryButton, setUnlockParametersArray);
                        altimeterRecoveryButton.StopAllCoroutines();
                        altimeterRecoveryButtonOverriden = true;
                    }
                }
                else if (altimeterRecoveryButtonOverriden)
                {
                    altimeterRecoveryButton.StartCoroutine("UnlockRecovery", FlightGlobals.ActiveVessel);
                    altimeterRecoveryButton.StartCoroutine("UnlockReturnToKSC", FlightGlobals.ActiveVessel);
                    altimeterRecoveryButtonOverriden = false;
                }
            }
        }

        void BuildPartsPositionsList(List<Vector2> positionsList, List<PartBuoyancy> partsBuoyanciesList, Vector3 ux, Vector3 uy, Vector3 offsetVector3)
        {
            foreach (Vessel vessel in FlightGlobals.VesselsLoaded)
            {
                if (vessel.altitude <= Mathf.Abs(maxWaveInteractionShipAltitude))
                {
                    foreach (Part part in vessel.parts)
                    {
                        if (part.partBuoyancy)
                        {
                            //To be more accurate I'd need to take ceter of buoyancy and transform it to worldPos but that would add a matrix multiply for every part, which I'm not about to do
                            Vector3 relativePartPos = part.transform.position - Scatterer.Instance.nearCamera.transform.position;
                            Vector2 oceanPos = new Vector2(Vector3.Dot(relativePartPos, ux) + offsetVector3.x, Vector3.Dot(relativePartPos, uy) + offsetVector3.y);
                            positionsList.Add(oceanPos);
                            partsBuoyanciesList.Add(part.partBuoyancy);
                        }
                    }
                }
            }
        }

        void AddCameraPosition(List<Vector2> positionsList, double cameraHeight, Vector2 cameraOceanPosition)
        {
            if (cameraHeight <= Mathf.Abs(maxWaveInteractionShipAltitude))
            {
                positionsList.Add(cameraOceanPosition);
                cameraHeightRequested = true;
            }
        }

        float ApplyWaterLevelHeights(float waterHeightAtCameraPosition)
        {
            //partBuoyancy has no way to override the force direction (it uses -g direction directly) however, maybe do a parent addforce at position with the buoyancy effective position with the normal? will always be hacky probably

            if (cameraHeightRequested)
            {
                waterHeightAtCameraPosition = heights[heights.Length - 1];
            }

            if (partsBuoyancies!=null)
            {
                for (int i = 0; i < partsBuoyancies.Count; i++)
                {
                    if (partsBuoyancies[i] != null)
                    {
                        partsBuoyancies[i].waterLevel = heights[i];

                        if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideDrag)
                        { 
                            partsBuoyancies[i].wasSplashed = true;            // changing these is also need so the game doesn't apply excessive drag on splashes which makes seaplanes unusable
                            partsBuoyancies[i].splashed = true;
                        }
                    }
                }
            }

            partsBuoyancies.Clear();
            cameraHeightRequested = false;

            return waterHeightAtCameraPosition;
        }

        void RequestAsyncWaterLevelHeights(List<Vector2> positionsList)
        {
            int size = positionsList.Count;
            if (size > 0)
            {
                positionsBuffer = new ComputeBuffer(size, 2 * sizeof(float));
                positionsBuffer.SetData(positionsList);
                findHeightsShader.SetBuffer(0, "positions", positionsBuffer);
                heightsBuffer = new ComputeBuffer(size, sizeof(float));
                findHeightsShader.SetBuffer(0, "result", heightsBuffer);

                findHeightsShader.Dispatch(0, size, 1, 1); //worry about figuring out threads and groups later

                AsyncGPUReadback.Request(heightsBuffer, OnCompletePartHeightsReadback);
                frameLatencyCounter = 1;
                heightsRequestInProgress = true;
            }
        }

        void OnCompletePartHeightsReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Utils.LogError("GPU readback error detected.");
                return;
            }

            heights = request.GetData<float>().ToArray();
            heightsRequestInProgress = false;

            positionsBuffer.Dispose();
            heightsBuffer.Dispose();
        }

        public void ForcePauseMenuSaving()
        {
            if (!paused && FlightGlobals.ActiveVessel != null && (FlightGlobals.ActiveVessel.altitude <= Mathf.Abs(maxWaveInteractionShipAltitude))
                && (FlightGlobals.ActiveVessel.IsClearToSave() == ClearToSaveStatus.CLEAR) && (FlightGlobals.ActiveVessel.situation == Vessel.Situations.SPLASHED)
                && (FlightGlobals.ActiveVessel.srf_velocity.sqrMagnitude < Scatterer.Instance.mainSettings.waterMaxRecoveryVelocity * Scatterer.Instance.mainSettings.waterMaxRecoveryVelocity))
            {
                Utils.LogInfo("Overriding pause menu recovery and save options");
                paused = true;
                FlightGlobals.ActiveVessel.srf_velocity = Vector3d.zero;

                PauseMenu.Display();
            }
        }

        public void UnPause()
        {
            paused = false;
        }

        public void Cleanup()
        {
            GameEvents.onGamePause.Remove(ForcePauseMenuSaving);
            GameEvents.onGameUnpause.Remove(UnPause);
        }
    }
}

