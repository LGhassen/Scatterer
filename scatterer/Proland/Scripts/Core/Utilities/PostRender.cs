using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{

	/*
	 * Some nodes need to have data draw using DrawProcedural (like the PlantsNode) which can only be done in a OnPostRender 
	 * function when attached to a camera. 
	 * Attach this script to a camera and bind the node to this component (in the m_postRendersGO array)
	 * All bound nodes will have there PostRender function called where they can do any rendering they require
	 */
	[RequireComponent(typeof(Camera))]
	public class PostRender : MonoBehaviour 
	{

		[SerializeField]
		GameObject[] m_postRendersGO;
		List<Node> m_postRenders = new List<Node>();

		void Start()
		{
			//Find all the nodes in the m_postRendersGO array
			foreach(GameObject go in m_postRendersGO) 
			{
				Node n = go.GetComponent<Node>();
				if(n != null)
					m_postRenders.Add(n);
				else
					Debug.Log("Proland::PostRender::Start - Attached game object does not contain a Node component");
			}
		}

		void OnPostRender()
		{
			//FOr each node call its post render function
			foreach(Node n in m_postRenders) 
			{
				if(n.gameObject.activeInHierarchy)
					n.PostRender();
			}
		}
	}

}
