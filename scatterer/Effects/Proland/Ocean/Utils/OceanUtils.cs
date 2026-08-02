using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scatterer
{
    public static class OceanUtils
    {
        public static bool oceanRemoved = false;

        private sealed class DisabledOceanState
        {
            public PQS ocean;
            public string bodyName;
            public bool wasActiveSelf;
            public bool wasDisabled;
        }

        private static readonly List<DisabledOceanState> disabledOceans = new List<DisabledOceanState>();

        public static void RemoveStockOceansIfNotDone()
        {
            // Always scan so bodies that were unavailable during an earlier scene can be retried.
            RemoveStockOceans();
        }

        private static void RemoveStockOceans()
        {
            disabledOceans.RemoveAll(state => state.ocean == null);

            foreach (ScattererCelestialBody sctBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
            {
                if (sctBody.hasOcean)
                {
                    bool disabled = false;
                    var celBody = FlightGlobals.Bodies.SingleOrDefault(_cb => _cb.bodyName == sctBody.celestialBodyName);
                    if (celBody == null)
                    {
                        celBody = FlightGlobals.Bodies.SingleOrDefault(_cb => _cb.bodyName == sctBody.transformName);
                    }

                    if (celBody != null)
                    {
                        PQS pqs = celBody.pqsController;
                        if (pqs != null)
                        {
                            // Accessing ChildSpheres before deactivating the ocean preserves KSP's
                            // cached child reference for code that reads ocean PQS metadata.
                            PQS ocean = FindOceanPQS(celBody, pqs.ChildSpheres);
                            disabled = DisableStockOcean(celBody, ocean);
                        }
                    }

                    if (!disabled)
                    {
                        Utils.LogDebug("Couldn't disable stock ocean for " + sctBody.celestialBodyName);
                    }
                }
            }

            oceanRemoved = disabledOceans.Count != 0;
            Utils.LogDebug("Disabled " + disabledOceans.Count + " stock ocean(s)");
        }

        private static PQS FindOceanPQS(CelestialBody celBody, PQS[] childSpheres)
        {
            if (childSpheres == null)
                return null;

            PQS[] children = childSpheres.Where(child => child != null).ToArray();
            if (children.Length == 0)
                return null;

            PQS[] oceanFxCandidates = children.Where(HasOceanFx).ToArray();
            if (oceanFxCandidates.Length == 1)
                return oceanFxCandidates[0];

            if (oceanFxCandidates.Length > 1)
            {
                LogAmbiguousOceanCandidates(celBody, oceanFxCandidates, "PQSMod_OceanFX");
                return null;
            }

            PQS[] namedCandidates = children.Where(child => child.gameObject.name.EndsWith("Ocean", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (namedCandidates.Length == 1)
                return namedCandidates[0];

            if (namedCandidates.Length > 1)
            {
                LogAmbiguousOceanCandidates(celBody, namedCandidates, "name");
                return null;
            }

            if (celBody.ocean && children.Length == 1)
                return children[0];

            LogAmbiguousOceanCandidates(celBody, children, "child sphere hierarchy");
            return null;
        }

        private static bool HasOceanFx(PQS sphere)
        {
            if (sphere.GetComponent<PQSMod_OceanFX>() != null)
                return true;

            return HasOceanFxBelow(sphere.transform);
        }

        private static bool HasOceanFxBelow(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // Match PQS.GetChildMods(): a nested PQS owns its own modifiers.
                if (child.GetComponent<PQS>() != null)
                    continue;

                if (child.GetComponent<PQSMod_OceanFX>() != null || HasOceanFxBelow(child))
                    return true;
            }

            return false;
        }

        private static void LogAmbiguousOceanCandidates(CelestialBody celBody, PQS[] candidates, string matchedBy)
        {
            string childNames = String.Join(", ", candidates.Select(candidate => candidate.gameObject.name).ToArray());
            Utils.LogDebug("Couldn't uniquely identify stock ocean for " + celBody.bodyName +
                           " using " + matchedBy + ". Candidates: " + childNames);
        }

        private static bool DisableStockOcean(CelestialBody celBody, PQS ocean)
        {
            if (ocean == null)
                return false;

            DisabledOceanState existingState = disabledOceans.Find(state => state.ocean == ocean);
            if (existingState != null)
            {
                SuppressStockOcean(ocean);
                return true;
            }

            DisabledOceanState newState = new DisabledOceanState
            {
                ocean = ocean,
                bodyName = celBody.bodyName,
                wasActiveSelf = ocean.gameObject.activeSelf,
                wasDisabled = ocean.isDisabled
            };

            SuppressStockOcean(ocean);
            disabledOceans.Add(newState);

            Utils.LogDebug("Disabled stock ocean PQS " + ocean.gameObject.name + " for " + celBody.bodyName);
            return true;
        }

        private static void SuppressStockOcean(PQS ocean)
        {
            // Keep the hierarchy active: KSP and BurstPQS can invoke PQS lifecycle methods
            // directly and expect active modifier objects while SetupMods runs.
            if (!ocean.gameObject.activeInHierarchy)
                return;

            if (ocean.isActive && ocean.quads != null && ocean.quads.Length == 6)
                ocean.DeactivateSphere();

            // The stock UpdateSphere coroutine remains alive but skips all PQS work while this
            // flag is set. LateUpdate reasserts it after any stock lifecycle reactivation.
            ocean.isDisabled = true;
        }

        public static void EnforceStockOceanSuppression()
        {
            if (!oceanRemoved)
                return;

            disabledOceans.RemoveAll(state => state.ocean == null);
            foreach (DisabledOceanState state in disabledOceans)
                SuppressStockOcean(state.ocean);

            oceanRemoved = disabledOceans.Count != 0;
        }

        // We can disable Scatterer oceans and re-enable stock oceans without restarting the game.
        // Rebuilding an already-started stock ocean still creates its normal PQS objects and cost.
        public static void RestoreOceansIfNeeded()
        {
            if (!oceanRemoved && disabledOceans.Count == 0)
                return;

            foreach (DisabledOceanState state in disabledOceans.ToArray())
            {
                PQS ocean = state.ocean;
                if (ocean == null)
                    continue;

                if (state.wasActiveSelf)
                {
                    if (!ocean.gameObject.activeSelf)
                        ocean.gameObject.SetActive(true);

                    ocean.isDisabled = state.wasDisabled;
                    if (!state.wasDisabled)
                        ocean.RebuildSphere();
                }
                else
                {
                    ocean.gameObject.SetActive(false);
                    ocean.isDisabled = state.wasDisabled;
                }

                Utils.LogDebug("Restored stock ocean PQS " + ocean.gameObject.name + " for " + state.bodyName);
            }

            disabledOceans.Clear();
            oceanRemoved = false;

            Utils.LogDebug("Stock oceans restored");
        }
    }
}