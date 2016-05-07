
using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{
	
	public class SunFlare : MonoBehaviour
	{	
		public Material sunglareMaterial;
		
		Texture2D sunSpikes;
		Texture2D sunFlare;

		Texture2D sunGhost1;
		Texture2D sunGhost2;
		Texture2D sunGhost3;

		Manager lastActivePQS=null; //manager with last active PQS, for atmosphere transmittance data

		bool transmittanceSet= false;

		public Core inCore;

		Vector3 sunViewPortPos=Vector3.zero;

		RaycastHit hit;
		bool hitStatus=false;
		bool eclipse=false;

		float sunGlareScale=1;
		float sunGlareFade=1;

		Mesh screenMesh;

		[Persistent]
		float sunGlareFadeDistance = 1000000;

		//input settings
		[Persistent]
		Vector3 flareSettings = Vector3.zero;
		[Persistent]
		Vector3 spikesSettings = Vector3.zero;
	
		[Persistent]
		List<Vector4> ghost1SettingsList1=new List<Vector4>{};
		[Persistent]
		List<Vector4> ghost1SettingsList2=new List<Vector4>{};

		[Persistent]
		List<Vector4> ghost2SettingsList1=new List<Vector4>{};
		[Persistent]
		List<Vector4> ghost2SettingsList2=new List<Vector4>{};

		[Persistent]
		List<Vector4> ghost3SettingsList1=new List<Vector4>{};
		[Persistent]
		List<Vector4> ghost3SettingsList2=new List<Vector4>{};


		public void start()
		{
			sunglareMaterial = new Material (ShaderTool.GetMatFromShader2 ("CompiledSunGlare.shader"));

			//Size is loaded automatically from the files
			sunSpikes = new Texture2D (1, 1);
			sunFlare = new Texture2D (1, 1);
			sunGhost1 = new Texture2D (1, 1);
			sunGhost2 = new Texture2D (1, 1);
			sunGhost3 = new Texture2D (1, 1);
			
			
			sunSpikes.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", inCore.path+"/sunflare", "sunSpikes.png")));
			sunSpikes.wrapMode = TextureWrapMode.Clamp;
			sunFlare.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", inCore.path+"/sunflare", "sunFlare.png")));
			sunFlare.wrapMode = TextureWrapMode.Clamp;
			sunGhost1.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", inCore.path+"/sunflare", "Ghost1.png")));
			sunGhost1.wrapMode = TextureWrapMode.Clamp;
			sunGhost2.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", inCore.path+"/sunflare", "Ghost2.png")));
			sunGhost2.wrapMode = TextureWrapMode.Clamp;
			sunGhost3.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", inCore.path+"/sunflare", "Ghost3.png")));
			sunGhost3.wrapMode = TextureWrapMode.Clamp;

//			sunglareMaterial.SetTexture ("_Sun_Glare", sunGlare);
			sunglareMaterial.SetTexture ("sunSpikes", sunSpikes);
			sunglareMaterial.SetTexture ("sunFlare", sunFlare);
			sunglareMaterial.SetTexture ("sunGhost1", sunGhost1);
			sunglareMaterial.SetTexture ("sunGhost2", sunGhost2);
			sunglareMaterial.SetTexture ("sunGhost3", sunGhost2);

			sunglareMaterial.SetTexture ("_customDepthTexture", inCore.customDepthBufferTexture);

			sunglareMaterial.renderQueue = 3100;

			screenMesh = MeshFactory.MakePlane (2, 2, MeshFactory.PLANE.XY, false, false);
			screenMesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));

			Sun.Instance.sunFlare.enabled = false;


			loadConfigNode ();

			//didn't want to serialize the matrices directly as the result is pretty unreadable
			//sorry about the mess, I'll make a cleaner way later
			//ghost 1
			Matrix4x4 ghost1Settings1 = Matrix4x4.zero;
			for (int i=0; i<ghost1SettingsList1.Count; i++)
			{
				ghost1Settings1.SetRow(i,ghost1SettingsList1[i]);
			}
			Matrix4x4 ghost1Settings2 = Matrix4x4.zero;
			for (int i=0; i<ghost1SettingsList2.Count; i++)
			{
				ghost1Settings2.SetRow(i,ghost1SettingsList2[i]);
			}

			//ghost 2
			Matrix4x4 ghost2Settings1 = Matrix4x4.zero;
			for (int i=0; i<ghost2SettingsList1.Count; i++)
			{
				ghost2Settings1.SetRow(i,ghost2SettingsList1[i]);
			}
			Matrix4x4 ghost2Settings2 = Matrix4x4.zero;
			for (int i=0; i<ghost2SettingsList2.Count; i++)
			{
				ghost2Settings2.SetRow(i,ghost2SettingsList2[i]);
			}

			//ghost 3
			Matrix4x4 ghost3Settings1 = Matrix4x4.zero;
			for (int i=0; i<ghost3SettingsList1.Count; i++)
			{
				ghost3Settings1.SetRow(i,ghost3SettingsList1[i]);
			}
			Matrix4x4 ghost3Settings2 = Matrix4x4.zero;
			for (int i=0; i<ghost3SettingsList2.Count; i++)
			{
				ghost3Settings2.SetRow(i,ghost3SettingsList2[i]);
			}


			sunglareMaterial.SetVector ("flareSettings", flareSettings);
			sunglareMaterial.SetVector ("spikesSettings", spikesSettings);

			sunglareMaterial.SetMatrix ("ghost1Settings1", ghost1Settings1);
			sunglareMaterial.SetMatrix ("ghost1Settings2", ghost1Settings2);

			sunglareMaterial.SetMatrix ("ghost2Settings1", ghost2Settings1);
			sunglareMaterial.SetMatrix ("ghost2Settings2", ghost2Settings2);

			sunglareMaterial.SetMatrix ("ghost3Settings1", ghost3Settings1);
			sunglareMaterial.SetMatrix ("ghost3Settings2", ghost3Settings2);

			Debug.Log ("[Scatterer] added custom sun flare");

		}
		
		
		public void OnPreRender()
		{

			sunViewPortPos = inCore.scaledSpaceCamera.WorldToViewportPoint
				(ScaledSpace.LocalToScaledSpace(inCore.sunCelestialBody.transform.position));

			float dist = (float) (inCore.scaledSpaceCamera.transform.position - ScaledSpace.LocalToScaledSpace (inCore.sunCelestialBody.transform.position))
				.magnitude;

			sunGlareScale = dist / 2266660f;
			

			//if dist> 0.75 sunglarefadedistance -->1
			//if dist < 0.25*sunglarefadedistance -->0
			//else values smoothstepped in between
			sunGlareFade = Mathf.SmoothStep(0,1,(dist/sunGlareFadeDistance)-0.25f);




			hitStatus=false;
//			if (!MapView.MapIsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			if (!MapView.MapIsEnabled)
			{
	
				hitStatus = Physics.Raycast (inCore.farCamera.transform.position,
				                             (inCore.sunCelestialBody.transform.position- inCore.farCamera.transform.position).normalized,
				                             out hit, Mathf.Infinity, (int)((1 << 15) + (1 << 0)));
				
				if(!hitStatus)
				{
					hitStatus = Physics.Raycast (inCore.scaledSpaceCamera.transform.position,
					                             (ScaledSpace.LocalToScaledSpace(inCore.sunCelestialBody.transform.position)- inCore.scaledSpaceCamera.transform.position)
					                             .normalized,out hit, Mathf.Infinity, (int)((1 << 10)));
				}
			}
			else
			{
				hitStatus = Physics.Raycast (inCore.scaledSpaceCamera.transform.position, (ScaledSpace.LocalToScaledSpace(inCore.sunCelestialBody.transform.position)
				                                                                           - inCore.transform.position).normalized,out hit, Mathf.Infinity, (int)((1 << 10)));
			}

			eclipse = hitStatus;


			sunglareMaterial.SetVector ("sunViewPortPos", sunViewPortPos);
			sunglareMaterial.SetFloat ("aspectRatio", inCore.scaledSpaceCamera.aspect);
			sunglareMaterial.SetFloat ("sunGlareScale", sunGlareScale);
			sunglareMaterial.SetFloat ("sunGlareFade", sunGlareFade);


			//check for active PQS
//			if (!transmittanceSet && inCore.pqsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			if (!transmittanceSet && inCore.pqsEnabled)
			{
				if ((lastActivePQS != inCore.managerWactivePQS) || !(lastActivePQS))
				{
					sunglareMaterial.SetFloat ("Rg", inCore.managerWactivePQS.m_skyNode.Rg);
					sunglareMaterial.SetFloat ("Rt", inCore.managerWactivePQS.m_skyNode.Rt);
					sunglareMaterial.SetTexture ("_Sky_Transmittance", inCore.managerWactivePQS.m_skyNode.m_transmit);

					lastActivePQS = inCore.managerWactivePQS;

					Debug.Log("[Scatterer] Sunflare: loaded new transmittance table");
				}

				sunglareMaterial.SetFloat("useTransmittance",1f);
				transmittanceSet=true;
			}
			if (transmittanceSet)
			{
				if (!inCore.pqsEnabled)
				{
					sunglareMaterial.SetFloat ("useTransmittance", 0f);
					transmittanceSet = false;
				}
				else
				{


					if (!MapView.MapIsEnabled)
						sunglareMaterial.SetVector ("_Globals_WorldCameraPos", inCore.farCamera.transform.position - lastActivePQS.parentCelestialBody.transform.position);
					else
						sunglareMaterial.SetVector ("_Globals_WorldCameraPos", (Vector3) ScaledSpace.ScaledToLocalSpace(inCore.scaledSpaceCamera.transform.position) - lastActivePQS.parentCelestialBody.transform.position);

					sunglareMaterial.SetVector ("_Sun_WorldSunDir", lastActivePQS.getDirectionToSun ().normalized);
				}
			}
		}	

		public void updateNode()
		{
			//drawmesh calls have to be made in update()
			//if they're done on prerender or anywhere else they don't work as far as I know
//			if (!MapView.MapIsEnabled && !eclipse && (sunViewPortPos.z > 0) && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			if (!MapView.MapIsEnabled && !eclipse && (sunViewPortPos.z > 0))
			{
				Graphics.DrawMesh (screenMesh, Vector3.zero, Quaternion.identity, sunglareMaterial, 15,
				                   inCore.nearCamera, 0, null, false, false);
			}
			
			else if (!eclipse && (sunViewPortPos.z > 0))
			{
				Graphics.DrawMesh (screenMesh, Vector3.zero, Quaternion.identity, sunglareMaterial, 10,
				                   inCore.scaledSpaceCamera, 0, null, false, false);

			}
		}



//		public void saveToConfigNode ()
//		{
//			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
//			cnTemp.Save (inCore.path + "/sunflare/sunflare.cfg");
//		}

		public void loadConfigNode ()
		{
			ConfigNode cnToLoad = ConfigNode.Load (inCore.path + "/sunflare/sunflare.cfg");
			ConfigNode.LoadObjectFromConfig (this, cnToLoad);
		}


	}
}