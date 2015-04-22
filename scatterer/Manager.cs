using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSP.IO;

namespace scatterer
{
	/*
	 * A manger to organise what order update functions are called, the running of tasks and the drawing of the terrain.
	 * Provides a location for common settings and allows the nodes to access each other.
	 * Also sets uniforms that are considered global.
	 * Must have a scheduler script attached to the same gameobject
	 * 
	 */
	public class Manager : MonoBehaviour 
	{


		Core m_core;
		//RenderTexture m_inscatter;
		int updateCnt=0;
		public enum DEFORM { PLANE, SPHERE };
		string managerState="not initialized";

		//[SerializeField]
		//ComputeShader m_writeData;

		//[SerializeField]
		//ComputeShader m_readData;
		//bool bindtocam1or0=true;
		int[] cam=new int[10];
		int transformCam=0;

		int globalsCam=1;

		[SerializeField]
		int m_gridResolution = 25;

		[SerializeField]
		float m_HDRExposure = 0.2f;

		//If the world is a flat plane or a sphere
		[SerializeField]
		DEFORM m_deformType = DEFORM.SPHERE;

		[SerializeField]
		//float m_radius = 63600.0f*4;
		//float m_radius = 600000.0f;
		float m_radius= 603000.0f;

		//Schedular m_schedular;

		//OceanNode m_oceanNode;
		SkyNode m_skyNode;
		SunNode m_sunNode;
/*		PlantsNode m_plantsNode;
		TerrainNode[] m_terrainNodes;
		Controller m_controller;
		List<TileSampler> m_samplers;
		Mesh m_quadMesh;
		MaterialPropertyBlock m_propertyBlock;*/
		Vector3 m_origin;
		CelestialBody parentCelestialBody,sunCelestialBody;

		public void saveSettings(){
			m_skyNode.saveSettings ();
		}

		public void loadSettings(){
			m_skyNode.loadSettings ();
		}

		public void enablePostprocess(){
			m_skyNode.enablePostprocess ();
		}

		public void disablePostprocess(){
			m_skyNode.disablePostprocess ();
		}


		public void toggleCam(int i){
			//bindtocam1or0 = !bindtocam1or0;
			if (cam[i] == 1)
				cam[i] = 0;
			else
				cam[i] = 1;
		}

		public void setCam(int i, int value){
			//bindtocam1or0 = !bindtocam1or0;
			cam [i] = value;
		}

		public void toggleCamTransform(){

			transformCam++;
			if (transformCam >= 7)
				transformCam = 0;
			print ("TRANSFORM CAM CHANGED TO");
			print (transformCam);
		}

		public void SetNearPlane(int NR) {
			m_skyNode.SetNearPlane (NR);
		}


		
		public void SetFarPlane(int FR) {
			m_skyNode.SetFarPlane (FR);
		}

		
		public void setGlobalsCam(int whatever) {
			globalsCam=whatever;
		}


		public void SetPostProcessScale(float postScale) {
			m_skyNode.SetPostProcessScale (postScale);
		}

		public void SetPostProcessExposure(float postExposure) {
			m_skyNode.SetPostProcessExposure (postExposure);
		}

		public void SetPostProcessDepth(float postDepth) {
			m_skyNode.SetPostProcessDepth (postDepth);
		}

		public void SetPostProcessAlpha(float postAlpha) {
			m_skyNode.SetPostProcessAlpha (postAlpha);
		}

		public void SetAtmosphereGlobalScale(float gScale){
			m_skyNode.SetAtmosphereGlobalScale (gScale);
		}


		public void SetAlphaCutoff(float cutoff) {
			m_skyNode.SetAlphaCutoff (cutoff);
		}

		
		public void SetAlphaGlobal(float glob) {
			m_skyNode.SetAlphaGlobal (glob);
		}

		public void SetExposure(float expo) {
			m_HDRExposure=expo;
		}

		public void setParentCelestialBody (CelestialBody parent)
		{
			parentCelestialBody = parent;
		}

		public CelestialBody getParentCelestialBody ()
		{
			return parentCelestialBody;
		}

		public void setLayernCam(int inLayer, int inCam)
		{
			m_skyNode.setLayernCam (inLayer, inCam);
		}

		public void setSunCelestialBody (CelestialBody sun)
		{
			sunCelestialBody = sun;
		}

		public Vector3 getDirectionToSun()
		{
			return((sunCelestialBody.GetTransform ().position-parentCelestialBody.GetTransform ().position));
		}

		public int GetGridResolution() {
			return m_gridResolution;
		}

		public string getManagerState() {
			return managerState;
		}
		
		public bool IsDeformed() {
			return (m_deformType == DEFORM.SPHERE);
		}
		
		public float GetRadius() {
			return m_radius;
		}

		public void SetRadius(float rad) {
			if (rad == 0) {
				print("RAD NULL");
				print("RAD NULL");
				print("RAD NULL");
				print("RAD NULL");
			}

			m_radius=rad;
			m_skyNode.SetRadius (rad);
		}

	/*	public Schedular GetSchedular() {
			return m_schedular;
		}
		
		public ComputeShader GetWriteData() {
			return new ComputeShader();
		}
		
		public ComputeShader GetReadData() {
			return new ComputeShader();
		}*/
		
		public SkyNode GetSkyNode() {
			return m_skyNode;
		}
		
		public SunNode GetSunNode() {
			return m_sunNode;
		}

		public Vector3 GetSunNodeDirection() {
			return m_sunNode.GetDirection();
		}

		public Matrix4x4 GetSunWorldToLocalRotation(){

			return m_sunNode.GetWorldToLocalRotation();
		}
		
/*		public OceanNode GetOceanNode() {
			return m_oceanNode;
		}

		public PlantsNode GetPlantsNode() {
			return m_plantsNode;
		}

		public Controller GetController() {
			return m_controller;
		}*/

		public void SetSunNodeUniforms(Material mat){
			m_sunNode.SetUniforms (mat);
		}


		public void SetCore(Core core){
				m_core=core;
		}



		public RenderTexture getInscatter()
		{
			return m_skyNode.getInscatter ();
		}
		
	
		// Use this for initialization
		public void Awake() 
		{
			managerState = "waking up";

			/*if(IsDeformed())
				m_origin = Vector3.zero;
			else
				m_origin = new Vector3(0.0f, 0.0f, m_radius);

			m_schedular = new Schedular();
			m_controller = new Controller ();

			//if planet view is being use set the radius
			if( m_controller.GetView() is PlanetView)
				((PlanetView)m_controller.GetView()).SetRadius(m_radius);*/

			//Get the nodes that are children of the manager
			//m_oceanNode = GetComponentInChildren<OceanNode>();
			m_sunNode = new SunNode();
			m_sunNode.Start ();
			m_skyNode = new SkyNode();
			m_skyNode.setManager (this);
			m_skyNode.SetParentCelestialBody (parentCelestialBody);
			m_skyNode.loadSettings ();
			m_skyNode.Start ();


			/*//m_plantsNode = GetComponentInChildren<PlantsNode>();
			//m_terrainNodes = GetComponentsInChildren<TerrainNode>();

			m_samplers = new List<TileSampler>(GetComponentsInChildren<TileSampler>());
			m_samplers.Sort(new TileSampler.Sort());

			m_propertyBlock = new MaterialPropertyBlock();
			//make the mesh used to draw the terrain quads
			m_quadMesh = MeshFactory.MakePlane(m_gridResolution,m_gridResolution);
			m_quadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));*/
			for (int i=0;i<10;i++){
				cam[i]=1;
			}

			managerState = "awake";



			m_radius = (float)parentCelestialBody.Radius;
		}

		public void SetUniforms(Material mat)
		{



//			if(mat == null) return;
//			Camera[] cams = Camera.allCameras;
//
//			mat.SetMatrix ("_Globals_WorldToCamera", cams [transformCam].worldToCameraMatrix);
//			mat.SetMatrix ("_Globals_CameraToWorld", cams [transformCam].worldToCameraMatrix.inverse);
//
////			mat.SetMatrix ("_Globals_WorldToCamera", cams [globalsCam].worldToCameraMatrix);
////			mat.SetMatrix ("_Globals_CameraToWorld", cams [globalsCam].worldToCameraMatrix.inverse);
//
//			Matrix4x4 p = cams [cam [1]].projectionMatrix;
//
////			Matrix4x4 p = cams [globalsCam].projectionMatrix;
//
//				/*bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
//			
//			if(d3d) 
//			{
//			if(cams[1].actualRenderingPath == RenderingPath.DeferredLighting)
//				{
//					// Invert Y for rendering to a render texture
//					for (int i = 0; i < 4; i++) {
//						p[1,i] = -p[1,i];
//					}
//				}
//				
//				// Scale and bias depth range
//				for (int i = 0; i < 4; i++) {
//					p[2,i] = p[2,i]*0.5f + p[3,i]*0.5f;
//				}
//			}*/
//
//
//			
//				Matrix4x4d m_cameraToScreenMatrix = new Matrix4x4d (p);
//
//				if (cam [2] == 1) {
//
//					mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
//					mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
//				} else {
//
//					mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
//					mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.ToMatrix4x4 ());
//				}
//
//			//	if (cam [6] == 1) {
//				mat.SetVector ("_Globals_WorldCameraPos", cams [cam [3]].transform.position);
//
////				mat.SetVector ("_Globals_WorldCameraPos", cams [globalsCam].transform.position);
//
////				}
////				else{
////					mat.SetVector ("_Globals_WorldCameraPos",Vector3.zero- cams [cam [3]].transform.position);
////				}
//
//				if (cam [4] == 1) {
//					//Vector3 ayd=parentCelestialBody.transform.position;
//					mat.SetVector ("_Globals_Origin", Vector3.zero-parentCelestialBody.transform.position);	
//				} else {
//					mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
//				}
//				//Vessel playerVessel=FlightGlobals.ActiveVessel;
//				//mat.SetVector("_Globals_Origin", Vector3.zero-parentCelestialBody.transform.position);
//				//mat.SetVector ("_Globals_Origin", Vector3.zero);
				


			//NEW TESTING BLOCK /////////////////////////////////


						if(mat == null) return;
						Camera[] cams = Camera.allCameras;
			
//						mat.SetMatrix ("_Globals_WorldToCamera", cams [cam [0]].cameraToWorldMatrix.inverse);
//						mat.SetMatrix ("_Globals_CameraToWorld", cams [cam [0]].cameraToWorldMatrix);
			
			//			mat.SetMatrix ("_Globals_WorldToCamera", cams [globalsCam].worldToCameraMatrix);
			//			mat.SetMatrix ("_Globals_CameraToWorld", cams [globalsCam].worldToCameraMatrix.inverse);
			
						Matrix4x4 p = cams [cam [1]].projectionMatrix;
			
			//			Matrix4x4 p = cams [globalsCam].projectionMatrix;
			
							/*bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
						
						if(d3d) 
						{
						if(cams[1].actualRenderingPath == RenderingPath.DeferredLighting)
							{
								// Invert Y for rendering to a render texture
								for (int i = 0; i < 4; i++) {
									p[1,i] = -p[1,i];
								}
							}
							
							// Scale and bias depth range
							for (int i = 0; i < 4; i++) {
								p[2,i] = p[2,i]*0.5f + p[3,i]*0.5f;
							}
						}*/
			
			
						
							Matrix4x4d m_cameraToScreenMatrix = new Matrix4x4d (p);
			
							if (cam [2] == 1) {
			
								mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
								mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
							} else {
			
								mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
								mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.ToMatrix4x4 ());
							}
			
						//	if (cam [6] == 1) {
						mat.SetVector ("_Globals_WorldCameraPos", cams [cam [2]].transform.position);
			
			//				mat.SetVector ("_Globals_WorldCameraPos", cams [globalsCam].transform.position);
			
			//				}
			//				else{
			//					mat.SetVector ("_Globals_WorldCameraPos",Vector3.zero- cams [cam [3]].transform.position);
			//				}
			
							if (cam [4] == 1) {
								//Vector3 ayd=parentCelestialBody.transform.position;
								mat.SetVector ("_Globals_Origin", Vector3.zero-parentCelestialBody.transform.position);	
							} else {
								mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
							}
							//Vessel playerVessel=FlightGlobals.ActiveVessel;
							//mat.SetVector("_Globals_Origin", Vector3.zero-parentCelestialBody.transform.position);
							//mat.SetVector ("_Globals_Origin", Vector3.zero);


			mat.SetFloat ("_Exposure", m_HDRExposure);

		}

		public void setPostprocessUniforms(Material mat)
		{

		}

		public void SetInscatteringCoeff(float inCoeff) {
			m_skyNode.SetInscatteringCoeff(inCoeff);
		}
		
		public void SetExtinctionCoeff(float exCoeff) {
			m_skyNode.SetExtinctionCoeff(exCoeff);
		}

		
		// Update is called once per frame
		public void Update () 
		{
			managerState = "updating";


			//Update the sky, sun and controller. These node are presumed to always be present
			m_sunNode.setDirectionToSun (getDirectionToSun ());
			m_sunNode.UpdateNode();
			m_radius = (float)parentCelestialBody.Radius;

			m_skyNode.UpdateNode();




			/*m_controller.UpdateController();
			m_sunNode.UpdateNode();


			//Uppdate ocean if used
			if(m_oceanNode != null)
				m_oceanNode.UpdateNode();

			//Update all the terrain nodes used and active
			foreach(TerrainNode node in m_terrainNodes)
			{
				if(node.gameObject.activeInHierarchy) 
					node.UpdateNode();
			}

			//Update the plants node if used
			if(m_plantsNode != null)
				m_plantsNode.UpdateNode();

			//Update all the samplers used and active
			foreach(TileSampler sampler in m_samplers)
			{
				if(sampler.gameObject.activeInHierarchy)
					sampler.UpdateSampler();
			}

			//Run any tasks generated by updating the samplers
			m_schedular.Run();

			//Draw the terrain quads of each terrain node if active
			foreach(TerrainNode node in m_terrainNodes)
			{
				if(node.gameObject.activeInHierarchy)
					DrawTerrain(node);
			}*/

			updateCnt++;
			managerState = "update done "+updateCnt.ToString();

		}

		public void lateUdpate()
		{
			m_skyNode.drawSky();
		}

		/*void DrawTerrain(TerrainNode node)
		{
			//Get all the samplers attached to the terrain node. The samples contain the data need to draw the quad
			TileSampler[] allSamplers = node.transform.GetComponentsInChildren<TileSampler>();
			List<TileSampler> samplers = new List<TileSampler>();
			//Only use sample if enabled
			foreach(TileSampler sampler in allSamplers)
			{
				if(sampler.enabled && sampler.GetStoreLeaf())
					samplers.Add(sampler);
			}

			if(samplers.Count == 0) return;

			//Find all the quads in the terrain node that need to be drawn
			FindDrawableQuads(node.GetRoot(), samplers);
			//The draw them
			DrawQuad(node, node.GetRoot(), samplers);

		}*/

		/*
		 * Find all the quads in a terrain that need to be drawn. If a quad is a leaf and is visible it should
		 * be drawn. If that quads tile is not ready the first ready parent is drawn
		 * NOTE - because of the current set up all task are run on the frame they are generated so 
		 * the leaf quads will always have tiles that are ready to be drawn
		 */
//		void FindDrawableQuads(TerrainQuad quad, List<TileSampler> samplers)
//		{
//			quad.SetDrawable(false);
//			
//			if (!quad.IsVisible()) {
//				quad.SetDrawable(true);
//				return;
//			}
//			
//			if (quad.IsLeaf()) 
//			{
//				for ( int i = 0; i < samplers.Count; ++i)
//				{
//					TileProducer p = samplers[i].GetProducer();
//					int l = quad.GetLevel();
//					int tx = quad.GetTX();
//					int ty = quad.GetTY();
//
//					if (p.HasTile(l, tx, ty) && p.FindTile(l, tx, ty, false, true) == null) {
//						return;
//					}
//				}
//			} 
//			else 
//			{
//				int nDrawable = 0;
//				for (int i = 0; i < 4; ++i) 
//				{
//					FindDrawableQuads(quad.GetChild(i), samplers);
//					if (quad.GetChild(i).GetDrawable()) {
//						++nDrawable;
//					}
//				}
//
//				if (nDrawable < 4) 
//				{
//					for (int i = 0; i < samplers.Count; ++i) 
//					{
//						TileProducer p = samplers[i].GetProducer();
//						int l = quad.GetLevel();
//						int tx = quad.GetTX();
//						int ty = quad.GetTY();
//						
//						if (p.HasTile(l, tx, ty) && p.FindTile(l, tx, ty, false, true) == null) {
//							return;
//						}
//					}
//				}
//			}
//			
//			quad.SetDrawable(true);
//		}
//
//		void DrawQuad(TerrainNode node, TerrainQuad quad, List<TileSampler> samplers)
//		{
//			if (!quad.IsVisible()) {
//				return;
//			}
//
//			if (!quad.GetDrawable()) {
//				return;
//			}
//
//			if (quad.IsLeaf()) 
//			{
//				m_propertyBlock.Clear();
//
//				for (int i = 0; i < samplers.Count; ++i) {
//					//Set the unifroms needed to draw the texture for this sampler
//					samplers[i].SetTile(m_propertyBlock, quad.GetLevel(), quad.GetTX(), quad.GetTY());
//				}
//
//				//Set the uniforms unique to each quad
//				node.SetPerQuadUniforms(quad, m_propertyBlock);
//
//				Graphics.DrawMesh(m_quadMesh, Matrix4x4.identity, node.GetMaterial(), 0, Camera.main, 0, m_propertyBlock);
//			} 
//			else 
//			{
//				//draw quads in a order based on distance to camera
//				int[] order = new int[4];
//				double ox = node.GetLocalCameraPos().x;
//				double oy = node.GetLocalCameraPos().y;
//				
//				double cx = quad.GetOX() + quad.GetLength() / 2.0;
//				double cy = quad.GetOY() + quad.GetLength() / 2.0;
//
//				if (oy < cy) 
//				{
//					if (ox < cx) {
//						order[0] = 0;
//						order[1] = 1;
//						order[2] = 2;
//						order[3] = 3;
//					} else {
//						order[0] = 1;
//						order[1] = 0;
//						order[2] = 3;
//						order[3] = 2;
//					}
//				} 
//				else 
//				{
//					if (ox < cx) {
//						order[0] = 2;
//						order[1] = 0;
//						order[2] = 3;
//						order[3] = 1;
//					} else {
//						order[0] = 3;
//						order[1] = 1;
//						order[2] = 2;
//						order[3] = 0;
//					}
//				}
//				
//				int done = 0;
//				for (int i = 0; i < 4; ++i) 
//				{
//					if (quad.GetChild(order[i]).GetVisible() == Frustum.VISIBILTY.INVISIBLE) {
//						done |= (1 << order[i]);
//					} 
//					else if (quad.GetChild(order[i]).GetDrawable()) {
//						DrawQuad(node, quad.GetChild(order[i]), samplers);
//						done |= (1 << order[i]);
//					}
//				}
//
//				if (done < 15) 
//				{
//					//If the a leaf quad needs to be drawn but its tiles are not ready then this 
//					//will draw the next parent tile instead that is ready.
//					//Because of the current set up all tiles always have there tasks run on the frame they are generated
//					//so this section of code is never reached
//
//					m_propertyBlock.Clear();
//					
//					for (int i = 0; i < samplers.Count; ++i) {
//						//Set the unifroms needed to draw the texture for this sampler
//						samplers[i].SetTile(m_propertyBlock, quad.GetLevel(), quad.GetTX(), quad.GetTY());
//					}
//					
//					//Set the uniforms unique to each quad
//					node.SetPerQuadUniforms(quad, m_propertyBlock);
//					//TODO - use mesh of appropriate resolution for non-leaf quads
//					Graphics.DrawMesh(m_quadMesh, Matrix4x4.identity, node.GetMaterial(), 0, Camera.main, 0, m_propertyBlock);
//				}
//			}
//		}
	}
}











































