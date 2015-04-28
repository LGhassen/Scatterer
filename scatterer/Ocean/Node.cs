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

		

		
		protected virtual void Start () {

		}
		
		protected virtual void OnDestroy()
		{
			
		}
		
		/*
		 * Used if the node has data that nees to be drawn by a camera in the OnPostRender function
		 * See the PostRender.cs script for more info
		 */
		public virtual void PostRender()
		{
			
		}

		
	}
	
}
