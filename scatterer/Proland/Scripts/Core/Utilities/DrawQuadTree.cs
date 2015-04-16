using UnityEngine;
using System.Collections;

namespace scatterer
{

	/*
	 * Used to draw the outline of the bounding box on the terrain nodes quads.
	 * To draw a terrainNode the game object must be dragged onto the terrainNode array on this script
	 * This script must be attached to a camera to work.
	 */
	public class DrawQuadTree : MonoBehaviour 
	{
		public bool on = false;

		public GameObject[] terrainNode;

		TerrainNode[] node;
		Material lineMaterial;

		Color[] col = new Color[]{ Color.red, Color.magenta, Color.blue, Color.green, Color.yellow, Color.white };
		
		// Use this for initialization
		void Start () 
		{
			
			CreateLineMaterial();

			node = new TerrainNode[terrainNode.Length];

			for(int i = 0; i < terrainNode.Length; i++)
			{
				node[i] = terrainNode[i].GetComponent<TerrainNode>();
				
				if(node[i] == null)
				{
					Debug.Log("Proland::DrawQuadTree::Start - The game object at " + i + " you set does not have a Proland::TerrainNode script attached");
					return;
				}
			}
		
		}

		void Update()
		{
			if(Input.GetKeyUp(KeyCode.F1))
				on = !on;
		}
		
		void OnPostRender()  
		{
			if(!on) return;

			for(int i = 0; i < terrainNode.Length; i++)
			{
				if(!terrainNode[i].activeInHierarchy) continue;

				if(node[i] == null) continue;

				TerrainQuad root = node[i].GetRoot();
				
				if(root == null) continue;
				
				root.DrawQuadOutline(Camera.main.camera, lineMaterial, col[i%6]);
			}
		}
		
		void CreateLineMaterial() 
		{
			if( !lineMaterial ) 
			{
				lineMaterial = new Material( 	"Shader \"Lines/Colored Blended\" {" +
												"SubShader { Pass { " +
												"    Blend SrcAlpha OneMinusSrcAlpha " +
												"    ZWrite Off Cull Off Fog { Mode Off } " +
												"    BindChannels {" +
												"      Bind \"vertex\", vertex Bind \"color\", color }" +
												"} } }" );
				
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}
}
