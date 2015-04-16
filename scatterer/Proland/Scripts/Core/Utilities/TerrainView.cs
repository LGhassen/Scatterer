
/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 */

using UnityEngine;
using System.Collections;
using System;

namespace scatterer
{
	/**
	* A view for flat terrains. The camera position is specified
	* from a "look at" position (x0,y0) on ground, with a distance d between
	* camera and this position, and two angles (theta,phi) for the direction
	* of this vector.
	*/
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(Controller))]
	public class TerrainView : MonoBehaviour 
	{
		[System.Serializable]
		public class Position
		{
			//The x and y coordinate of the point the camera is looking at on the ground.
			//For a planet view these are the longitudes and latitudes
			public double x0, y0;
			//The zenith angle of the vector between the "look at" point and the camera.
			public double theta;
			//The azimuth angle of the vector between the "look at" point and the camera.
			public double phi;
			//The distance between the "look at" point and the camera.
			public double distance;

			public override string ToString ()
			{
				return "Pos(x,y) = " + x0 + "," + y0 + " Theta = " + theta + " Phi = " + phi + " distance = " + distance;
			}
		};

		[SerializeField]
		bool m_printPositionOnClose = false;

		//The x0,y0,theta,phi and distance parameters
		[SerializeField]
		protected Position m_position;

		//The localToWorld matrix in double precision
		protected Matrix4x4d m_worldToCameraMatrix;
		//The inverse world to camera matrix
		protected Matrix4x4d m_cameraToWorldMatrix;
		//The projectionMatrix in double precision
		protected Matrix4x4d m_cameraToScreenMatrix;
		//inverse projection matrix
		protected Matrix4x4d m_screenToCameraMatrix;
		//The world camera pos
		protected Vector3d2 m_worldCameraPos;
		//The camera direction
		protected Vector3d2 m_cameraDir;
		//the height below the camera of the ground (if its been read back from gpu)
		protected double m_groundHeight = 0.0;
	    //The camera position in world space resulting from the x0,y0,theta,phi and distance parameters.
		protected Vector3d2 m_worldPos;

		public double GetGroundHeight() {
			return m_groundHeight;
		}

		public void SetGroundHeight(double ht) {
			m_groundHeight = ht;
		}

		public Position GetPos() {
			return m_position;
		}

		public Matrix4x4d GetWorldToCamera() {
			return m_worldToCameraMatrix;
		}

		public Matrix4x4d GetCameraToWorld() {
			return m_cameraToWorldMatrix;
		}

		public Matrix4x4d GetCameraToScreen() {
			return m_cameraToScreenMatrix;
		}

		public Matrix4x4d GetScreenToCamera() {
			return m_screenToCameraMatrix;
		}

		public Vector3d2 GetWorldCameraPos() {
			return m_worldCameraPos;
		}

		public Vector3d2 GetCameraDir() {
			return m_cameraDir;
		}

		//returns the position the camera is currently looking at
		public virtual Vector3d2 GetLookAtPos() {
			return new Vector3d2(m_position.x0, m_position.y0, 0.0);
		}

		public virtual double GetHeight() {
			return m_worldPos.z;
		}

		//Any contraints you need on the position are applied here
		public virtual void Constrain() {
			m_position.theta = Math.Max(0.0001, Math.Min(Math.PI, m_position.theta));
			m_position.distance = Math.Max(0.1, m_position.distance);
		}
		
		// Use this for initialization
		protected virtual void Start() 
		{
			m_worldToCameraMatrix = Matrix4x4d.Identity();
			m_cameraToWorldMatrix = Matrix4x4d.Identity();
			m_cameraToScreenMatrix = Matrix4x4d.Identity();
			m_screenToCameraMatrix = Matrix4x4d.Identity();
			m_worldCameraPos = new Vector3d2();
			m_cameraDir = new Vector3d2();
			m_worldPos = new Vector3d2();

			Constrain();
		}

		protected virtual void OnDestroy()
		{
			//On destroy print out the camera position. This can useful when tring to position
			//the camera in a certain spot
			if(m_printPositionOnClose)
				Debug.Log("Proland::TerrainView::OnDestroy - " + m_position.ToString());
		}
		
		// Update is called once per frame
		public virtual void UpdateView() 
		{
			Constrain();

			SetWorldToCameraMatrix();
			SetProjectionMatrix();

			m_worldCameraPos = m_worldPos;
			m_cameraDir = (m_worldPos - GetLookAtPos()).Normalized();
		}
		
		/*
		* Computes the world to camera matrix using double precision
		* and applies it to the camera.
		*/
		protected virtual void SetWorldToCameraMatrix()
		{

			Vector3d2 po = new Vector3d2(m_position.x0, m_position.y0, 0.0);
		    Vector3d2 px = new Vector3d2(1.0, 0.0, 0.0);
		    Vector3d2 py = new Vector3d2(0.0, 1.0, 0.0);
		    Vector3d2 pz = new Vector3d2(0.0, 0.0, 1.0);
		
			double ct = Math.Cos(m_position.theta);
			double st = Math.Sin(m_position.theta);
			double cp = Math.Cos(m_position.phi);
			double sp = Math.Sin(m_position.phi);
			
		    Vector3d2 cx = px * cp + py * sp;
		    Vector3d2 cy = (px*-1.0) * sp*ct + py * cp*ct + pz * st;
		    Vector3d2 cz = px * sp*st - py * cp*st + pz * ct;
			
			m_worldPos = po + cz * m_position.distance;
		
			if (m_worldPos.z < m_groundHeight + 10.0) {
				m_worldPos.z = m_groundHeight + 10.0;
		    }
		
		    Matrix4x4d view = new Matrix4x4d(	cx.x, cx.y, cx.z, 0.0,
		            							cy.x, cy.y, cy.z, 0.0,
		            							cz.x, cz.y, cz.z, 0.0,
		            							0.0, 0.0, 0.0, 1.0);

			m_worldToCameraMatrix = view * Matrix4x4d.Translate(m_worldPos * -1.0);

			m_worldToCameraMatrix.m[0,0] *= -1.0;
			m_worldToCameraMatrix.m[0,1] *= -1.0;
			m_worldToCameraMatrix.m[0,2] *= -1.0;
			m_worldToCameraMatrix.m[0,3] *= -1.0;

			m_cameraToWorldMatrix = m_worldToCameraMatrix.Inverse();

			camera.worldToCameraMatrix = m_worldToCameraMatrix.ToMatrix4x4();
			camera.transform.position = m_worldPos.ToVector3();

		}
		
		/*
		* Get a copy of the projection matrix and convert in to double precision
		* and apply the bias if using dx11 and flip Y if deferred rendering is used 
		*/
		protected virtual void SetProjectionMatrix()
		{ 

			float h = (float)(GetHeight() - m_groundHeight);
			camera.nearClipPlane = 0.1f * h;
			camera.farClipPlane = 1e6f * h;

			camera.ResetProjectionMatrix();

			Matrix4x4 p = camera.projectionMatrix;
			bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;

			if(d3d) 
			{
				if(camera.actualRenderingPath == RenderingPath.DeferredLighting)
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
			}

			m_cameraToScreenMatrix = new Matrix4x4d(p);
			m_screenToCameraMatrix = m_cameraToScreenMatrix.Inverse();
		}

		/**
		* Moves the "look at" point so that "oldp" appears at the position of "p"
		* on screen.
		*/
		public virtual void Move(Vector3d2 oldp, Vector3d2 p, double speed)
		{
			m_position.x0 -= (p.x - oldp.x) * speed * Math.Max (1.0, GetHeight());
			m_position.y0 -= (p.y - oldp.y) * speed * Math.Max (1.0, GetHeight());
		}

		public virtual void MoveForward(double distance)
		{
			m_position.x0 -= Math.Sin(m_position.phi) * distance;
			m_position.y0 += Math.Cos(m_position.phi) * distance;
		}

		public virtual void Turn(double angle) {
			m_position.phi += angle;
		}

		/**
		* Sets the position as the interpolation of the two given positions with
		* the interpolation parameter t (between 0 and 1). The source position is
		* sx0,sy0,stheta,sphi,sd, the destination is dx0,dy0,dtheta,dphi,dd.
		*/
		public virtual double Interpolate(	double sx0, double sy0, double stheta, double sphi, double sd,
		                           			double dx0, double dy0, double dtheta, double dphi, double dd, double t)
		{
			// TODO interpolation
			m_position.x0 = dx0;
			m_position.y0 = dy0;
			m_position.theta = dtheta;
			m_position.phi = dphi;
			m_position.distance = dd;
			return 1.0;
		}

		public virtual void InterpolatePos(double sx0, double sy0, double dx0, double dy0, double t, ref double x0, ref double y0)
		{
			x0 = sx0 * (1.0 - t) + dx0 * t;
			y0 = sy0 * (1.0 - t) + dy0 * t;
		}

		/**
		* Returns a direction interpolated between the two given direction.
		*
		* param slon start longitude.
		* param slat start latitude.
		* param elon end longitude.
		* param elat end latitude.
		* param t interpolation parameter between 0 and 1.
		* param[out] lon interpolated longitude.
		* param[out] lat interpolated latitude.
		*/
		public virtual void InterpolateDirection(double slon, double slat, double elon, double elat, double t, ref double lon, ref double lat)
		{
			Vector3d2 s = new Vector3d2(Math.Cos(slon) * Math.Cos(slat), Math.Sin(slon) * Math.Cos(slat), Math.Sin(slat));
			Vector3d2 e = new Vector3d2(Math.Cos(elon) * Math.Cos(elat), Math.Sin(elon) * Math.Cos(elat), Math.Sin(elat));
			Vector3d2 v = (s * (1.0 - t) + e * t).Normalized();
			lat = MathUtility.Safe_Asin(v.z);
			lon = Math.Atan2(v.y, v.x);
		}
		
	}
}
















