using System;
using System.Collections;
using UnityEngine;
using KSP.UI.Screens;

namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class ToolbarButton : MonoBehaviour
	{
		bool hasAddedButton = false;		
		
		Rect toolbarRect;
		float toolbarWidth = 280;
		float toolbarHeight = 0;
		float toolbarMargin = 6;
		float contentWidth;
		Vector2 toolbarPosition;
		public ApplicationLauncherButton button;

		private static ToolbarButton instance;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				UnityEngine.Object.Destroy (this);
			}
		}

		public static ToolbarButton Instance
		{
			get 
			{
				return instance;
			}
		}

		void Start()
		{
			toolbarPosition = new Vector2(Screen.width - toolbarWidth - 80, 39);
			toolbarRect = new Rect(toolbarPosition.x, toolbarPosition.y, toolbarWidth, toolbarHeight);
			contentWidth = toolbarWidth - (2 * toolbarMargin);
			
			AddToolbarButton();
		}

		void AddToolbarButton()
		{
			if(HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				if(!hasAddedButton)
				{
					Texture buttonTexture = GameDatabase.Instance.GetTexture("Scatterer/icon/icon", false);
					button = ApplicationLauncher.Instance.AddModApplication(ShowToolbarGUI, HideToolbarGUI, Dummy, Dummy, Dummy, Dummy, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
					hasAddedButton = true;
				}
			}
		}
		public void ShowToolbarGUI()
		{
			Core.Instance.visible = true;
		}
		
		public void HideToolbarGUI()
		{
			Core.Instance.visible = false;
		}

		void OnDestroy ()
		{
			if (button)
				ApplicationLauncher.Instance.RemoveModApplication (button);
		}

		void Dummy()
		{}
		
		public static bool MouseIsInRect(Rect rect)
		{
			return rect.Contains(MouseGUIPos());
		}
		
		public static Vector2 MouseGUIPos()
		{
			return new Vector3(Input.mousePosition.x, Screen.height-Input.mousePosition.y, 0);
		}
	}
}

