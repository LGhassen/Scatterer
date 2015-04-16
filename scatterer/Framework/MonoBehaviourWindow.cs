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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class WindowInitialsAttribute : Attribute
    {
        public Boolean Visible { get; set; }
        public Boolean DragEnabled { get; set; }
        public Boolean ClampToScreen { get; set; }
        public Boolean TooltipsEnabled { get; set; }
        public String Caption { get; set; }
    }

    /// <summary>
    /// An Extended version of the UnityEngine.MonoBehaviour Class
    /// Basically a template for a Window, has the MonoBehaviourExtended properties and extra bits to make drawing a window easier
    /// </summary>
    public abstract class MonoBehaviourWindow : MonoBehaviourExtended
    {
        #region "Constructors"
        internal MonoBehaviourWindow()
            : base()
        {
            //do the assembly name add so we get different windowIDs for multiple plugins
            this.WindowID = UnityEngine.Random.Range(1000, 2000000) + _AssemblyName.GetHashCode();
            this._Visible = false;
            LogFormatted_DebugOnly("WindowID:{0}", WindowID);

            //and look for any customattributes
            WindowInitialsAttribute[] attrs = (WindowInitialsAttribute[])Attribute.GetCustomAttributes(this.GetType(), typeof(WindowInitialsAttribute));
            foreach (WindowInitialsAttribute attr in attrs)
            {
                Visible = attr.Visible;
                DragEnabled = attr.DragEnabled;
                ClampToScreen = attr.ClampToScreen;
                TooltipsEnabled = attr.TooltipsEnabled;
                WindowCaption = attr.Caption;
            }
        }
        ///CANT USE THE ONES BELOW HERE AS WE NEED TO INSTANTIATE THE WINDOW USING AddComponent()
        //internal MonoBehaviourWindow(String Caption)
        //    : this()
        //{
        //    this.WindowCaption = Caption;
        //}
        //internal MonoBehaviourWindow(String Caption, Rect Position)
        //    : this(Caption)
        //{
        //    this.WindowRect = Position;
        //}

        //TODO: Look at using this
        //  http://answers.unity3d.com/questions/445444/add-component-in-one-line-with-parameters.html

        //internal static MonoBehaviourWindow CreateComponent(GameObject AttachTo)
        //{
        //    MonoBehaviourWindow monoReturn;
        //    monoReturn = AttachTo.AddComponent<MonoBehaviourWindow>();
        //    return monoReturn;
        //}

        #endregion

        internal override void Awake()
        {
            //just some debugging stuff here
            LogFormatted_DebugOnly("New MBWindow Awakened");

            //base.Awake();
        }

        /// <summary>
        /// WindowID variable - randomly set at window creation
        /// </summary>
        internal Int32 WindowID { get; private set; }
        /// <summary>
        /// Window position on screen, is fed in to the Window routine and the resulting position after GUILayout is what you read
        /// </summary>
        internal Rect WindowRect;

        /// <summary>
        /// Caption of the Window
        /// </summary>
        internal String WindowCaption = null;
        /// <summary>
        /// Style of the Window
        /// </summary>
        internal GUIStyle WindowStyle = null;
        /// <summary>
        /// Layout Options for the GUILayout.Window function
        /// </summary>
        internal GUILayoutOption[] WindowOptions = null;

        /// <summary>
        /// Whether the window is draggable by mouse
        /// </summary>
        internal Boolean DragEnabled = false;
        /// <summary>
        /// A defined area (like a handle) where you can drag from. This is from the top left corner of the window. Lets you make isso it can be only draggable from a certain point, icon, title bar, etc
        /// </summary>
        internal Rect DragRect;

        /// <summary>
        /// Whether the window can be moved off the visible screen
        /// </summary>
        internal Boolean ClampToScreen = true;
        /// <summary>
        /// How close to the edges it can get if clamping is enabled - this can be negative if you want to allow it to go off screen by a certain amount
        /// </summary>
        internal RectOffset ClampToScreenOffset = new RectOffset(0, 0, 0, 0);

        private Boolean _Visible;
        /// <summary>
        /// Whether the Window is visible or not. Changing this value will add/remove the window from the RenderingManager.PostDrawQueue
        /// </summary>
        internal Boolean Visible
        {
            get { return _Visible; }
            set
            {
                if (_Visible != value)
                {
                    if (value)
                    {
                        LogFormatted_DebugOnly("Adding Window to PostDrawQueue-{0}", WindowID);
                        RenderingManager.AddToPostDrawQueue(5, this.DrawGUI);
                    }
                    else
                    {
                        LogFormatted_DebugOnly("Removing Window from PostDrawQueue", WindowID);
                        RenderingManager.RemoveFromPostDrawQueue(5, this.DrawGUI);
                    }
                }
                _Visible = value;
            }
        }

        /// <summary>
        /// This is the Code that draws the window and sets the skin
        /// !!!! You have to set the skin before drawing the window or you will scratch your head for ever
        /// </summary>
        private void DrawGUI()
        {
            //this sets the skin on each draw loop
            GUI.skin = SkinsLibrary.CurrentSkin;

            //keep the window locked to the screen if its supposed to be
            if (ClampToScreen)
                WindowRect = WindowRect.ClampToScreen(ClampToScreenOffset);

            //Are we using a custom style of the skin style for the window
            if (WindowStyle == null)
            {
                WindowRect = GUILayout.Window(WindowID, WindowRect, DrawWindowInternal, WindowCaption, WindowOptions);
            }
            else
            {
                WindowRect = GUILayout.Window(WindowID, WindowRect, DrawWindowInternal, WindowCaption, WindowStyle, WindowOptions);
            }

            //Draw the tooltip of its there to be drawn
            if (TooltipsEnabled)
                DrawToolTip();
        }

        /// <summary>
        /// Time that the last iteration of RepeatingWorkerFunction ran for. Can use this value to see how much impact your code is having
        /// </summary>
        internal TimeSpan DrawWindowInternalDuration { get; private set; }

        /// <summary>
        /// Private function that handles wrapping the drawwindow functionality with the supplementary stuff - dragging, tooltips, etc
        /// </summary>
        /// <param name="id">The ID of the Window being drawn</param>
        private void DrawWindowInternal(Int32 id)
        {
            //record the start date
            DateTime Duration = DateTime.Now;

            DrawWindowPre(id);

            //This calls the must be overridden code
            DrawWindow(id);

            DrawWindowPost(id);

            //Set the Tooltip variable based on whats in this window
            if (TooltipsEnabled)
                SetTooltipText();

            //Are we allowing window drag
            if (DragEnabled)
                if (DragRect.height == 0 && DragRect.width == 0)
                    GUI.DragWindow();
                else
                    GUI.DragWindow(DragRect);

            //Now calc the duration
            DrawWindowInternalDuration = (DateTime.Now - Duration);
        }

        /// <summary>
        /// This is the optionally overridden function that runs before DrawWindow
        /// </summary>
        /// <param name="id">The ID of the Window being drawn</param>
        internal virtual void DrawWindowPre(Int32 id) {}

        /// <summary>
        /// This is the must be overridden function that equates to the content of the window
        /// </summary>
        /// <param name="id">The ID of the Window being drawn</param>
        internal abstract void DrawWindow(Int32 id);

        /// <summary>
        /// This is the optionally overridden function that runs after DrawWindow, but before tooltips, etc
        /// </summary>
        /// <param name="id">The ID of the Window being drawn</param>
        internal virtual void DrawWindowPost(Int32 id) { }

        #region Tooltip Work
        /// <summary>
        /// Whether tooltips should be displayed for this window
        /// </summary>
        internal Boolean TooltipsEnabled = false;

        /// <summary>
        /// Is a Tooltip currently showing
        /// </summary>
        internal Boolean TooltipShown { get; private set; }
        /// <summary>
        /// Whereis the tooltip positioned
        /// </summary>
        internal Rect TooltipPosition { get { return _TooltipPosition; } }
        private Rect _TooltipPosition = new Rect();

        /// <summary>
        /// An offset from the mouse position to put the top left of the tooltip. Use this to get the tooltip out from under the cursor
        /// </summary>
        internal Vector2d TooltipMouseOffset = new Vector2d();

        /// <summary>
        /// Whether the Tooltip should stay where first drawn or follow the mouse
        /// </summary>
        internal Boolean TooltipStatic = false;

        /// <summary>
        /// How long the tooltips should be displayed before auto-hiding
        /// 
        /// Set to 0 to have them last for ever
        /// </summary>
        internal Int32 TooltipDisplayForSecs = 15;

        /// <summary>
        /// Maximum width in pixels of the tooltip window. Text will wrap inside that width.
        /// 
        /// Set to 0 for unity default behaviour
        /// </summary>
        internal Int32 TooltipMaxWidth = 250;

        //Store the tooltip text from throughout the code
        private String strToolTipText = "";
        private String strLastTooltipText = "";

        //store how long the tooltip has been displayed for
        private Single fltTooltipTime = 0f;

        /// <summary>
        /// This is the meat of drawing the tooltip on screen
        /// </summary>
        private void DrawToolTip()
        {
            //Added drawing check to turn off tooltips when window hides
            if (TooltipsEnabled && Visible && (strToolTipText != "") && ((TooltipDisplayForSecs == 0) || (fltTooltipTime < (Single)TooltipDisplayForSecs)))
            {
                GUIContent contTooltip = new GUIContent(strToolTipText);
                GUIStyle styleTooltip = SkinsLibrary.CurrentTooltip;

                //if the content of the tooltip changes then reset the counter
                if (!TooltipShown || (strToolTipText != strLastTooltipText))
                    fltTooltipTime = 0f;

                //Calc the size of the Tooltip
                _TooltipPosition.x = Event.current.mousePosition.x + (Single)TooltipMouseOffset.x;
                _TooltipPosition.y = Event.current.mousePosition.y + (Single)TooltipMouseOffset.y;

                //do max width calc if needed
                if (TooltipMaxWidth > 0)
                {
                    //calc the new width and height
                    float minwidth, maxwidth;
                    SkinsLibrary.CurrentTooltip.CalcMinMaxWidth(contTooltip, out minwidth, out maxwidth); // figure out how wide one line would be
                    _TooltipPosition.width = Math.Min(TooltipMaxWidth - SkinsLibrary.CurrentTooltip.padding.horizontal, maxwidth); //then work out the height with a max width
                    _TooltipPosition.height = SkinsLibrary.CurrentTooltip.CalcHeight(contTooltip, TooltipPosition.width); // heres the result
                }
                else
                {
                    //calc the new width and height
                    Vector2 Size = SkinsLibrary.CurrentTooltip.CalcSize(contTooltip);
                    _TooltipPosition.width = Size.x;
                    _TooltipPosition.height = Size.y;

                }
                //set the style props for text layout
                styleTooltip.stretchHeight = !(TooltipMaxWidth > 0);
                styleTooltip.stretchWidth = !(TooltipMaxWidth > 0);
                styleTooltip.wordWrap = (TooltipMaxWidth > 0);

                //clamp it accordingly
                if (ClampToScreen)
                    _TooltipPosition = _TooltipPosition.ClampToScreen(ClampToScreenOffset);

                //Draw the Tooltip
                GUI.Label(TooltipPosition, contTooltip, styleTooltip);
                //On top of everything
                GUI.depth = 0;

                //update how long the tip has been on the screen and reset the flags
                fltTooltipTime += Time.deltaTime;
                TooltipShown = true;
            }
            else
            {
                //clear the flags
                TooltipShown = false;
            }

			//if we've moved to a diffn tooltip then reset the counter
			if (strToolTipText != strLastTooltipText) fltTooltipTime = 0f;
			
			//make sure the last text is correct
            strLastTooltipText = strToolTipText;
        }

        /// <summary>
        /// This function is run at the end of each draw loop to store the tooltiptext for later
        /// </summary>
        private void SetTooltipText()
        {
            if (Event.current.type == EventType.Repaint)
            {
                strToolTipText = GUI.tooltip;
            }
        }
        #endregion
    }
}