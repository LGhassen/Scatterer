using UnityEngine;
using System.Collections;

namespace scatterer
{

	/*
	 * Provides a common interface for nodes (ie terrain node, ocean node etc)
	 * Also for tile samplers and producers. Provides access to the manager so
	 * common data can be shared.
	 */
	public abstract class Node : MonoBehaviour 
	{
		public Manager m_manager;

		public TerrainView GetView() {
			return m_manager.GetController().GetView();
		}

		public virtual void Awake() {
			FindManger();
		}

		public virtual void Start () {
			if(m_manager == null) FindManger();
		}

		public virtual void OnDestroy()
		{

		}

		/*
		 * Used if the node has data that nees to be drawn by a camera in the OnPostRender function
		 * See the PostRender.cs script for more info
		 */
		public virtual void PostRender()
		{

		}

		void FindManger()
		{
			Transform t = transform;
	
			while(t != null) {
				Manager manager = t.GetComponent<Manager>();

				if(manager != null) {
					m_manager = manager;
					break;
				}

				t = t.parent;
			}

			if(m_manager == null) {
				Debug.Log("Proland::Node - Could not find manager. This gameObject must be a child of the manager");
				Debug.Break();
			}

		}

	}

}
