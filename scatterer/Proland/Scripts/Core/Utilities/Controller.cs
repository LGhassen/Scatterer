
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
using System;
using System.Collections;

namespace scatterer
{
	/*
	* Controller used to collect user input and move the view (TerrainView or PlanetView)
	* Provides smooth interpolation from the views current to new position
	*/
	public class Controller : MonoBehaviour 
	{
		//Speed settings for the different typs of movement
		[SerializeField]
		double m_moveSpeed = 1e-3;

		[SerializeField]
		double m_turnSpeed = 5e-3;

		[SerializeField]
		 double m_zoomSpeed = 1.0;

		[SerializeField]
		double m_rotateSpeed = 0.1;

		[SerializeField]
		double m_dragSpeed = 0.01;

		//True to use exponential damping to go to target positions, false to go to target positions directly.
		[SerializeField]
		bool m_smooth = true;
		//True if the PAGE_DOWN key is currently pressed.
		bool m_near;
		//True if the PAGE_UP key is currently pressed.
		bool m_far;
		//True if the UP key is currently pressed.
		bool m_forward;
		//True if the DOWN key is currently pressed.
		bool m_backward;
		//True if the LEFT key is currently pressed.
		bool m_left;
		//True if the RIGHT key is currently pressed.
		bool m_right;
		//True if the target position target is initialized.
		bool m_initialized;
		//The target position manipulated by the user via the mouse and keyboard.
		TerrainView.Position m_target;
		//Start position for an animation between two positions.
		TerrainView.Position m_start;
		//End position for an animation between two positions.
		TerrainView.Position m_end;

		Vector3d2 m_previousMousePos;
		
		/*
		* Animation status. Negative values mean no animation.
		* 0 corresponds to the start position, 1 to the end position,
		* and values between 0 and 1 to intermediate positions between
		* the start and end positions.
		*/
		double m_animation = -1.0;

		TerrainView m_view;

		public TerrainView GetView() {
			return m_view;
		}

		// Use this for initialization
		void Start() 
		{
			m_view = GetComponent<TerrainView>();

			m_target = new TerrainView.Position();
			m_start = new TerrainView.Position();
			m_end = new TerrainView.Position();
			m_previousMousePos = new Vector3d2(Input.mousePosition);
		}

		// Update is called once per frame
		public void UpdateController() 
		{
			if (!m_initialized) {
				GetPosition(m_target);
				m_initialized = true;
			}

			//Check for input
			KeyDown();
			MouseWheel();
			MouseMotion();

			double dt = Time.deltaTime * 1000.0;

			//If animation requried interpolate from start to end position
			//NOTE - has not been tested and not currently used
			if(m_animation >= 0.0) 
			{
				m_animation = m_view.Interpolate(m_start.x0, m_start.y0, m_start.theta, m_start.phi, m_start.distance,
				                                 m_end.x0, m_end.y0, m_end.theta, m_end.phi, m_end.distance, m_animation);
				
				if (m_animation == 1.0) {
					GetPosition(m_target);
					m_animation = -1.0;
				}
			} 
			else {
				UpdateController(dt);
			}

			//Update the view so the new positions are relected in the matrices
			m_view.UpdateView();
		}

		void UpdateController(double dt)
		{

			double dzFactor = Math.Pow(1.02, Math.Min(dt, 1.0));

			if(m_near) {
				m_target.distance = m_target.distance / (dzFactor * m_zoomSpeed);
			} 
			else if(m_far) {
				m_target.distance = m_target.distance * dzFactor * m_zoomSpeed;
			}

			TerrainView.Position p = new TerrainView.Position();
			GetPosition(p);
			SetPosition(m_target);

			if(m_forward) 
			{
				double speed = Math.Max(m_view.GetHeight(), 1.0);
				m_view.MoveForward(speed * dt * m_moveSpeed);
			} 
			else if(m_backward) 
			{
				double speed = Math.Max(m_view.GetHeight(), 1.0);
				m_view.MoveForward(-speed * dt * m_moveSpeed);
			}

			if(m_left) {
				m_view.Turn(dt * m_turnSpeed);
			} 
			else if(m_right) {
				m_view.Turn(-dt * m_turnSpeed);
			}

			GetPosition(m_target);
					
			if(m_smooth) 
			{
				double lerp = 1.0 - Math.Exp(-dt * 2.301e-3);
				double x0 = 0.0;
				double y0 = 0.0;
				m_view.InterpolatePos(p.x0, p.y0, m_target.x0, m_target.y0, lerp, ref x0, ref y0);
				p.x0 = x0;
				p.y0 = y0;
				p.theta = Mix2(p.theta, m_target.theta, lerp);
				p.phi = Mix2(p.phi, m_target.phi, lerp);
				p.distance = Mix2(p.distance, m_target.distance, lerp);
				SetPosition(p);
			} 
			else {
				SetPosition(m_target);
			}

		}

		double Mix2(double x, double y, double t) {
			return Math.Abs(x - y) < Math.Max(x, y) * 1e-5 ? y : x*(1.0-t) + y*t;
		}

		void GetPosition(TerrainView.Position p)
		{
			p.x0 = m_view.GetPos().x0;
			p.y0 = m_view.GetPos().y0;
			p.theta = m_view.GetPos().theta;
			p.phi = m_view.GetPos().phi;
			p.distance = m_view.GetPos().distance;
		}

		void SetPosition(TerrainView.Position p)
		{
			m_view.GetPos().x0 = p.x0;
			m_view.GetPos().y0 = p.y0;
			m_view.GetPos().theta = p.theta;
			m_view.GetPos().phi = p.phi;
			m_view.GetPos().distance = p.distance;
			m_animation = -1.0;
		}

		void GoToPosition(TerrainView.Position p)
		{
			GetPosition(m_start);
			m_end = p;
			m_animation = 0.0;
		}
		
		void JumpToPosition(TerrainView.Position p)
		{
			SetPosition(p);
			m_target = p;
		}

		void MouseWheel()
		{
			m_near = false;
			m_far = false;

			if (Input.GetAxis("Mouse ScrollWheel") < 0.0f || Input.GetKey(KeyCode.PageUp)) {
				m_far = true;
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0.0f || Input.GetKey(KeyCode.PageDown)) {
				m_near = true;
			}
		}

		void KeyDown()
		{
			m_forward = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
			m_backward = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
			m_left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
			m_right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		}

		void MouseMotion()
		{

			if(Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl)) 
			{
				m_target.phi -= Input.GetAxis("Mouse X") * m_rotateSpeed;
				m_target.theta += Input.GetAxis("Mouse Y") * m_rotateSpeed;
			} 
			else if(Input.GetMouseButton(0)) 
			{

				Vector3d2 mousePos = new Vector3d2();
				mousePos.x = Input.mousePosition.x;
				mousePos.y = Input.mousePosition.y;
				mousePos.z = 0.0;

				Vector3d2 preMousePos = new Vector3d2();
				preMousePos.x = m_previousMousePos.x;
				preMousePos.y = m_previousMousePos.y;
				preMousePos.z = 0.0;

				Vector3d2 oldp = m_view.GetCameraToWorld() * preMousePos;
				Vector3d2 p = m_view.GetCameraToWorld() * mousePos;

				if (!(double.IsNaN(oldp.x) || double.IsNaN(oldp.y) || double.IsNaN(oldp.z) || double.IsNaN(p.x) || double.IsNaN(p.y) || double.IsNaN(p.z))) 
				{
					TerrainView.Position current = new TerrainView.Position();
					GetPosition(current);
					SetPosition(m_target);
				
					m_view.Move(new Vector3d2(oldp), new Vector3d2(p), m_dragSpeed);
					GetPosition(m_target);
					SetPosition(current);
				}
			} 

			m_previousMousePos = new Vector3d2(Input.mousePosition);
		}
	}

}
























