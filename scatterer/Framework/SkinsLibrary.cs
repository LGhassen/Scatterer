/* Part of KSPPluginFramework
Version 1.2

Forum Thread:http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework
Author: TriggerAu, 2014
License: The MIT License (MIT)
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace KSPPluginFramework
{
    internal static class SkinsLibrary
    {
        #region Constructor
        static SkinsLibrary()
        {
            _Initialized = false;
            List = new Dictionary<string, GUISkin>();
        }
        #endregion

        /// <summary>
        /// Returns whether the Init Routine has been run
        /// </summary>
        internal static Boolean _Initialized { get; private set; }

        /// <summary>
        /// This is a copy of the default Unity skin
        /// </summary>
        internal static GUISkin DefUnitySkin { get; private set; }
        /// <summary>
        /// This is a copy of the default KSP skin
        /// </summary>
        internal static GUISkin DefKSPSkin { get; private set; }

        private static GUISkin _CurrentSkin;
        /// <summary>
        /// Will return the current Skin as controlled by the SetSkin() Methods
        /// </summary>
        internal static GUISkin CurrentSkin { get { return _CurrentSkin; } }

        private static GUIStyle _CurrentTooltip;
        /// <summary>
        /// Will return the current Tooltip style.
        /// 
        /// The Tooltip style will be the custom style called "Tooltip" or in the KSP and Unity Default skins I have created a tooltip style, in a custom skin it will default back to the label style from the skin
        /// </summary>
        internal static GUIStyle CurrentTooltip { get { return _CurrentTooltip; } }

        /// <summary>
        /// Heres where we store the list of custom skins
        /// </summary>
        internal static Dictionary<String, GUISkin> List { get; set; }

        /// <summary>
        /// Sets up the default skins and associated bits - only needs to run once in OnGUI code
        /// </summary>
        internal static void InitSkinList()
        {
            if (!_Initialized)
            {
                DefUnitySkin = GUI.skin;
                DefKSPSkin = HighLogic.Skin;

                SetCurrent(DefSkinType.KSP);
                _Initialized = true;
            }
        }

        /// <summary>
        /// Choices for pre-configured skins
        /// </summary>
        internal enum DefSkinType
        {
            Unity,
            KSP
            //,None
        }

        /// <summary>
        /// Sets the current Skin to one of the Pre-Defined types
        /// </summary>
        /// <param name="DefaultSkin">Which predefined skin to use</param>
        internal static void SetCurrent(DefSkinType DefaultSkin)
        {
            MonoBehaviourExtended.LogFormatted_DebugOnly("Setting GUISkin to {0}", DefaultSkin);
            GUISkin OldSkin = _CurrentSkin;
            switch (DefaultSkin)
            {
                case DefSkinType.Unity: _CurrentSkin = DefUnitySkin; break;
                case DefSkinType.KSP: _CurrentSkin = DefKSPSkin; break;
                //case DefSkinType.None: _CurrentSkin = new GUISkin(); break;
                default: _CurrentSkin = DefKSPSkin; break;
            }
            //Now set the tooltip style as well
            SetCurrentTooltip();

            if (OldSkin != CurrentSkin && OnSkinChanged!=null)
                OnSkinChanged();
        }

        public delegate void SkinChangedEvent();
        public static event SkinChangedEvent OnSkinChanged;

        /// <summary>
        /// Sets the current skin to one of the custom skins
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        internal static void SetCurrent(String SkinID)
        {
            MonoBehaviourExtended.LogFormatted_DebugOnly("Setting GUISkin to {0}", SkinID);
            GUISkin OldSkin = _CurrentSkin;

            //check the skin exists, and throw a log line if it doesnt
            if (List.ContainsKey(SkinID))
                _CurrentSkin = List[SkinID];
            else
                MonoBehaviourExtended.LogFormatted("Unable to change GUISkin to {0}, GUISkin not found", SkinID);

            //Now set the tooltip style as well
            SetCurrentTooltip();

            if (OldSkin != CurrentSkin && OnSkinChanged != null)
                OnSkinChanged();
        }

        /// <summary>
        /// Private routine that sets the tooltip based on the Current skin
        /// </summary>
        private static void SetCurrentTooltip()
        {
            //Use the custom skin if it exists
            if (StyleExists(_CurrentSkin, "Tooltip"))
            {
                _CurrentTooltip = GetStyle(_CurrentSkin, "Tooltip");
            }
            else
            {
                //otherwise lets build a style for the defaults or take the label style otherwise
                if (_CurrentSkin == DefUnitySkin)
                    _CurrentTooltip = new GUIStyle(DefUnitySkin.box);
                else if (_CurrentSkin == DefKSPSkin)
                    _CurrentTooltip = GenDefKSPTooltip();
                else
                    _CurrentTooltip = _CurrentSkin.label;
            }
        }

        /// <summary>
        /// private function that creates a tooltip style for KSP
        /// </summary>
        /// <returns>New Default Tooltip style for KSP</returns>
        private static GUIStyle GenDefKSPTooltip()
        {
            //build a new style to return
            GUIStyle retStyle = new GUIStyle(DefKSPSkin.label);
            //build the background
            Texture2D texBack = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            texBack.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.95f));
            texBack.Apply();
            retStyle.normal.background = texBack;
            //set some text defaults
            retStyle.normal.textColor = new Color32(224, 224, 224, 255);
            retStyle.padding = new RectOffset(3, 3, 3, 3);
            retStyle.alignment = TextAnchor.MiddleCenter;
            return retStyle;
        }

        /// <summary>
        /// Copies a skin so you can make a new custom skin from an already defined one
        /// </summary>
        /// <param name="DefaultSkin">Which predefined skin to use</param>
        /// <returns>The new copy of the skin</returns>
        internal static GUISkin CopySkin(DefSkinType DefaultSkin)
        {
            switch (DefaultSkin)
            {
                case DefSkinType.Unity: return (GUISkin)MonoBehaviourExtended.Instantiate(DefUnitySkin);
                case DefSkinType.KSP: return (GUISkin)MonoBehaviourExtended.Instantiate(DefKSPSkin);
                //case DefSkinType.None: return new GUISkin();
                default: return (GUISkin)MonoBehaviourExtended.Instantiate(DefKSPSkin);
            }
        }
        /// <summary>
        /// Copies a skin so you can make a new custom skin from an already defined one
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <returns>The new copy of the skin</returns>
        internal static GUISkin CopySkin(String SkinID)
        {
            if (List.ContainsKey(SkinID))
                return (GUISkin)MonoBehaviourExtended.Instantiate(List[SkinID]);
            else
            {
                MonoBehaviourExtended.LogFormatted("Unable to copy GUISkin to {0}, GUISkin not found", SkinID);
                throw new SystemException(String.Format("Unable to copy GUISkin to {0}, GUISkin not found", SkinID));
            }
        }

        /// <summary>
        /// Add a new GUISkin to the list of Skins
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <param name="NewSkin">The new GUISkin to add</param>
        /// <param name="SetAsCurrent">Whether we should immediately set this GUISkin to the current active skin</param>
        internal static void AddSkin(String SkinID, GUISkin NewSkin, Boolean SetAsCurrent = false)
        {
            NewSkin.name = SkinID;
            if (List.ContainsKey(SkinID))
                List[SkinID] = NewSkin;
            else
                List.Add(SkinID, NewSkin);

            if (SetAsCurrent)
                SetCurrent(SkinID);
        }
        /// <summary>
        /// Remove the GUISkin with the specified ID from the Skins List
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <returns>Whether it was removed</returns>
        internal static Boolean RemoveSkin(String SkinID)
        {
            return List.Remove(SkinID);
        }
        /// <summary>
        /// Check whether a skin exists
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <returns>The ID of the skin to look for</returns>
        internal static Boolean SkinExists(String SkinID)
        {
            return List.ContainsKey(SkinID);
        }

        /// <summary>
        /// Add a style to a skin, if the styleID already exists it will update the style
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <param name="NewStyle">The New GUIStyle to Add to the Skin</param>
        internal static void AddStyle(String SkinId, String StyleID, GUIStyle NewStyle)
        {
            NewStyle.name = StyleID;
            AddStyle(SkinId, NewStyle);
        }

        /// <summary>
        /// Add a style to a skin, if the styleID already exists it will update the style
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <param name="NewStyle">The New GUIStyle to Add to the Skin</param>
        internal static void AddStyle(String SkinId, GUIStyle NewStyle)
        {
            if (SkinExists(SkinId))
            {
                GUISkin skinTemp = List[SkinId];

                AddStyle(ref skinTemp, NewStyle);
            }
        }

        /// <summary>
        /// Add a style to a skin, if the styleID already exists it will update the style
        /// </summary>
        /// <param name="DefaultSkin">Which predefined skin to use</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <param name="NewStyle">The New GUIStyle to Add to the Skin</param>
        internal static void AddStyle(DefSkinType DefaultSkin, String StyleID, GUIStyle NewStyle)
        {
            NewStyle.name = StyleID;
            AddStyle(DefaultSkin, NewStyle);
        }
        /// <summary>
        /// Add a style to a skin, if the styleID already exists it will update the style
        /// </summary>
        /// <param name="DefaultSkin">Which predefined skin to use</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <param name="NewStyle">The New GUIStyle to Add to the Skin</param>
        internal static void AddStyle(DefSkinType DefaultSkin, GUIStyle NewStyle)
        {
            GUISkin skinTemp;
            if (DefaultSkin == DefSkinType.KSP)
                skinTemp = DefKSPSkin;
            else if (DefaultSkin == DefSkinType.Unity)
                skinTemp = DefUnitySkin;
            else return;

            AddStyle(ref skinTemp, NewStyle);
        }

        /// <summary>
        /// Remove a style from a skins CustomStyles list
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        internal static void RemoveStyle(String SkinID, String StyleID)
        {
            if (SkinExists(SkinID))
            {
                GUISkin skinTemp = List[SkinID];
                RemoveStyle(ref skinTemp, StyleID);
            }
        }
        /// <summary>
        /// Remove a style from a skins CustomStyles list
        /// </summary>
        /// <param name="DefaultSkin">Which predefined skin to use</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        internal static void RemoveStyle(DefSkinType DefaultSkin, String StyleID)
        {
            GUISkin skinTemp;
            if (DefaultSkin == DefSkinType.KSP)
                skinTemp = DefKSPSkin;
            else if (DefaultSkin == DefSkinType.Unity)
                skinTemp = DefKSPSkin;
            else return;

            RemoveStyle(ref skinTemp, StyleID);
        }
        /// <summary>
        /// Check whether a custom style exists in one of the custom skins customstyles list
        /// </summary>
        /// <param name="SkinID">The string ID of the custom skin</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <returns>whether it exists or not</returns>
        internal static Boolean StyleExists(String SkinID, String StyleID)
        {
            return (SkinExists(SkinID) && StyleExists(List[SkinID], StyleID));
        }
        /// <summary>
        /// Check whether a custom style exists in one of the default skins customstyles list
        /// </summary>
        /// <param name="DefaultSkin">Which predefined skin to use</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <returns>Whether the Style exists</returns>
        internal static Boolean StyleExists(DefSkinType DefaultSkin, String StyleID)
        {
            if (DefaultSkin == DefSkinType.KSP)
                return (StyleExists(DefKSPSkin, StyleID));
            else if (DefaultSkin == DefSkinType.Unity)
                return (StyleExists(DefUnitySkin, StyleID));
            else return false;
        }

        /// <summary>
        /// Add a style to a skin, if the styleID already exists it will update the style
        /// </summary>
        /// <param name="SkinToAction">The GUISkin we are going to adjust. The name of the Style is its ID</param>
        /// <param name="StyleID">String Identifier for the Style</param>
        /// <param name="NewStyle">The GUIStyle to add or update</param>
        internal static void AddStyle(ref GUISkin SkinToAction, String StyleID, GUIStyle NewStyle)
        {
            //set the name of the style and push to next method
            NewStyle.name = StyleID;
            AddStyle(ref SkinToAction, NewStyle);
        }

        /// <summary>
        /// Add a style to a skin, if the styleID already exists it will update the style
        /// </summary>
        /// <param name="SkinToAction">The GUISkin we are going to adjust. The name of the Style is its ID</param>
        /// <param name="NewStyle">The GUIStyle to add or update</param>
        internal static void AddStyle(ref GUISkin SkinToAction, GUIStyle NewStyle)
        {
            if (NewStyle.name == null || NewStyle.name == "")
            {
                MonoBehaviourExtended.LogFormatted("No Name Provided in the Style to add to {0}. Cannot add this.", SkinToAction.name);
                return;
            }

            //Convert to a list
            List<GUIStyle> lstTemp = SkinToAction.customStyles.ToList<GUIStyle>();

            //Add or edit the customstyle
            if (lstTemp.Any(x => x.name == NewStyle.name))
            {
                //if itexists then remove it first
                GUIStyle styleTemp = lstTemp.First(x => x.name == NewStyle.name);
                lstTemp.Remove(styleTemp);
            }
            //add the new style
            lstTemp.Add(NewStyle);

            //Write the list back to the array
            SkinToAction.customStyles = lstTemp.ToArray<GUIStyle>();
        }
        /// <summary>
        /// Remove a style from a skins CustomStyles list
        /// </summary>
        /// <param name="SkinToAction">The GUISkin we are going to adjust</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        internal static void RemoveStyle(ref GUISkin SkinToAction, String StyleID)
        {
            if (StyleExists(SkinToAction, StyleID))
            {
                //Convert to a list
                List<GUIStyle> lstTemp = SkinToAction.customStyles.ToList<GUIStyle>();

                //find and remove the style
                GUIStyle styleTemp = lstTemp.First(x => x.name == StyleID);
                lstTemp.Remove(styleTemp);

                //Write the list back to the array
                SkinToAction.customStyles = lstTemp.ToArray<GUIStyle>();
            }
        }

        /// <summary>
        /// Get a GUIStyle from a GUISkin custom list
        /// </summary>
        /// <param name="SkinToAction">The GUISkin we are going to adjust</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <returns>The style you are after, or null if the ID is not there</returns>
        internal static GUIStyle GetStyle(GUISkin SkinToAction, String StyleID)
        {
            if (StyleExists(SkinToAction, StyleID))
                return SkinToAction.customStyles.First(x => x.name == StyleID);
            else
                return null;
        }

        /// <summary>
        /// Check whether a custom style exists in one of the custom skins customstyles list
        /// </summary>
        /// <param name="SkinToAction">The GUISkin we are going to adjust</param>
        /// <param name="StyleID">The string ID of the custom style</param>
        /// <returns>whether it exists or not</returns>
        internal static Boolean StyleExists(GUISkin SkinToAction, String StyleID)
        {
            if (SkinToAction.customStyles.Any(x => x.name == StyleID))
                return true;
            else
            {
                //MonoBehaviourExtended.LogFormatted("Unable to find Style: {0} in Skin: {1}", StyleID, SkinToAction.name);
                return false;
            }
            //return (SkinToAction.customStyles.Any(x => x.name == StyleID));
        }
    }
}