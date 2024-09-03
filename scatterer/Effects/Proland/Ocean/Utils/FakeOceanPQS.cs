using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Scatterer
{
    public class FakeOceanPQS : PQSMod
    {
        bool coroutineStarted = false;
        Material originalOceanMaterial;

        IEnumerator StopSphereCoroutine()
        {
            while (true)
            {
                for (int i=0; i<10; i++)
                    yield return new WaitForFixedUpdate ();

                sphere.StopAllCoroutines ();
                sphere.DeactivateSphere();
            }
        }
        
        public override void OnSphereStarted()
        {
            if (!coroutineStarted)
            {
                StartCoroutine (StopSphereCoroutine ());
                coroutineStarted = true;
            }
        }

        public void Apply(PQS pqs)
        {
            if (pqs != null)
            {
                this.sphere = pqs;
                this.transform.parent = pqs.transform;
                this.requirements = PQS.ModiferRequirements.Default;
                this.modEnabled = true;
                this.order = 0;

                originalOceanMaterial = pqs.surfaceMaterial;

                Material invisibleOceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
                invisibleOceanMaterial.SetOverrideTag ("IgnoreProjector", "True");
                invisibleOceanMaterial.SetOverrideTag ("ForceNoShadowCasting", "True");
                pqs.surfaceMaterial = invisibleOceanMaterial;

                sphere.StopAllCoroutines ();
                sphere.DeactivateSphere();
            }
        }

        public void Remove()
        {
            this.StopAllCoroutines ();
            sphere.surfaceMaterial = originalOceanMaterial;
            sphere.StartUpSphere ();

            gameObject.DestroyGameObject ();
        }
    }
}