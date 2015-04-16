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
    /// <summary>
    /// An Extended version of the UnityEngine.MonoBehaviour Class
    /// Has some added functions to simplify repeated use and some defined overridable functions for common functions
    /// </summary>
    /// <remarks>
    /// You want to create instances of this using either the KSPAddOn Attribute or the gameObject.AddComponent. 
    /// 
    /// Using the simple contructor new MonobehaviourExtended() will NOT register the object to receive Unity events and you will wonder why no events are firing
    /// </remarks>
    /// <example title="Initialisation Examples">
    /// <code>
    /// [KSPAddon(KSPAddon.Startup.Flight,false)]
    /// class KSPAlternateResourcePanel : MonoBehaviourExtended
    /// {
    /// ...
    /// }
    /// 
    /// OR
    /// MonobehaviourExtended mbTemp = gameObject.AddComponent&lt;MonobehaviorExtended&gt;();
    /// </code>
    /// DONT DO THIS ONE!!!
    /// MonobehaviourExtended mbTemp = new MonobehaviourExtended();
    /// </example>
    public abstract class MonoBehaviourExtended : MonoBehaviour
    {
        #region Constructor
        ///// <summary>
        ///// This is marked private so you have to use the Factory Method to create any new instance. The Factory Method will add the new instance to a gameObject which is a requirement for Unity events to occur on the object
        ///// </summary>
        //private MonoBehaviourExtended()
        //{

        //}

        //internal static MonoBehaviourExtended CreateComponent(GameObject AttachTo)
        //{
        //    MonoBehaviourExtended monoReturn;
        //    monoReturn = AttachTo.AddComponent<MonoBehaviourExtended>();
        //    return monoReturn;
        //}
        static MonoBehaviourExtended()
        {
            UnityEngine.Random.seed = (int)(DateTime.Now - DateTime.Now.Date).TotalSeconds;
        }
        #endregion

        internal T AddComponent<T>() where T : UnityEngine.Component
        {
            return gameObject.AddComponent<T>();
        }

        #region RepeatingFunction Code
        private Boolean _RepeatRunning = false;
        /// <summary>
        /// Returns whether the RepeatingWorkerFunction is Running
        /// </summary>
        internal Boolean RepeatingWorkerRunning { get { return _RepeatRunning; } }

        //Storage Variables
        private Single _RepeatInitialWait;
        private Single _RepeatSecs;

        /// <summary>
        /// Get/Set the period in seconds that the repeatingfunction is triggered at
        /// Note: When setting this value if the repeating function is already running it restarts it to set the new period
        /// </summary>
        internal Single RepeatingWorkerRate
        {
            get { return _RepeatSecs; }
            private set
            {
                LogFormatted_DebugOnly("Setting RepeatSecs to {0}", value);
                _RepeatSecs = value;
                //If its running then restart it
                if (RepeatingWorkerRunning)
                {
                    StopRepeatingWorker();
                    StartRepeatingWorker();
                }
            }
        }

        /// <summary>
        /// Set the repeating period by how many times a second it should repeat
        ///    eg. if you set this to 4 then it will repeat every 0.25 secs
        /// </summary>
        /// <param name="NewTimesPerSecond">Number of times per second to repeat</param>
        /// <returns>The new RepeatSecs value (eg 0.25 from the example)</returns>
        internal Single SetRepeatTimesPerSecond(Int32 NewTimesPerSecond)
        {
            RepeatingWorkerRate = (Single)(1 / (Single)NewTimesPerSecond);
            return RepeatingWorkerRate;
        }
        /// <summary>
        /// Set the repeating period by how many times a second it should repeat
        ///    eg. if you set this to 4 then it will repeat every 0.25 secs
        /// </summary>
        /// <param name="NewTimesPerSecond">Number of times per second to repeat</param>
        /// <returns>The new RepeatSecs value (eg 0.25 from the example)</returns>
        internal Single SetRepeatTimesPerSecond(Single NewTimesPerSecond)
        {
            RepeatingWorkerRate = (Single)(1 / NewTimesPerSecond);
            return RepeatingWorkerRate;
        }
        /// <summary>
        /// Set the repeating rate in seconds for the repeating function
        ///    eg. if you set this to 0.1 then it will repeat 10 times every second
        /// </summary>
        /// <param name="NewSeconds">Number of times per second to repeat</param>
        /// <returns>The new RepeatSecs value</returns>
        internal Single SetRepeatRate(Single NewSeconds)
        {
            RepeatingWorkerRate = NewSeconds;
            return RepeatingWorkerRate;
        }

        /// <summary>
        /// Get/Set the value of the period that should be waited before the repeatingfunction begins
        /// eg. If you set this to 1 and then start the repeating function then the first time it fires will be in 1 second and then every RepeatSecs after that
        /// </summary>
        internal Single RepeatingWorkerInitialWait
        {
            get { return _RepeatInitialWait; }
            set { _RepeatInitialWait = value; }
        }

        #region Start/Stop Functions
        /// <summary>
        /// Starts the RepeatingWorker Function and sets the TimesPerSec variable
        /// </summary>
        /// <param name="TimesPerSec">How many times a second should the RepeatingWorker Function be run</param>
        /// <returns>The RunningState of the RepeatinWorker Function</returns>
        internal Boolean StartRepeatingWorker(Int32 TimesPerSec)
        {
            LogFormatted_DebugOnly("Starting the repeating function");
            //Stop it if its running
            StopRepeatingWorker();
            //Set the new value
            SetRepeatTimesPerSecond(TimesPerSec);
            //Start it and return the result
            return StartRepeatingWorker();
        }

        /// <summary>
        /// Starts the Repeating worker
        /// </summary>
        /// <returns>The RunningState of the RepeatinWorker Function</returns>
        internal Boolean StartRepeatingWorker()
        {
            try
            {
                LogFormatted_DebugOnly("Invoking the repeating function");
                this.InvokeRepeating("RepeatingWorkerWrapper", _RepeatInitialWait, RepeatingWorkerRate);
                _RepeatRunning = true;
            }
            catch (Exception)
            {
                LogFormatted("Unable to invoke the repeating function");
                //throw;
            }
            return _RepeatRunning;
        }

        /// <summary>
        /// Stop the RepeatingWorkerFunction
        /// </summary>
        /// <returns>The RunningState of the RepeatinWorker Function</returns>
        internal Boolean StopRepeatingWorker()
        {
            try
            {
                LogFormatted_DebugOnly("Cancelling the repeating function");
                this.CancelInvoke("RepeatingWorkerWrapper");
                _RepeatRunning = false;
            }
            catch (Exception)
            {
                LogFormatted("Unable to cancel the repeating function");
                //throw;
            }
            return _RepeatRunning;
        }
        #endregion

        /// <summary>
        /// Function that is repeated.
        /// You can monitor the duration of the execution of your RepeatingWorker using RepeatingWorkerDuration 
        /// You can see the game time that passes between repeats via RepeatingWorkerUTPeriod
        /// 
        /// No Need to run the base RepeatingWorker
        /// </summary>
        internal virtual void RepeatingWorker()
        {
            //LogFormatted_DebugOnly("WorkerBase");

        }

        /// <summary>
        /// Time that the last iteration of RepeatingWorkerFunction ran for. Can use this value to see how much impact your code is having
        /// </summary>
        internal TimeSpan RepeatingWorkerDuration { get; private set; }


        /// <summary>
        /// The Game Time that the Repeating Worker function last started
        /// </summary>
        private Double RepeatingWorkerUTLastStart { get; set; }
        /// <summary>
        /// The Game Time that the Repeating Worker function started this time
        /// </summary>
        private Double RepeatingWorkerUTStart { get; set; }
        /// <summary>
        /// The amount of UT that passed between the last two runs of the Repeating Worker Function
        /// 
        /// NOTE: Inside the RepeatingWorker Function this will be the UT that has passed since the last run of the RepeatingWorker
        /// </summary>
        internal Double RepeatingWorkerUTPeriod { get; private set; }

        /// <summary>
        /// This is the wrapper function that calls all the repeating function goodness
        /// </summary>
        private void RepeatingWorkerWrapper()
        {
            //record the start date
            DateTime Duration = DateTime.Now;

            //Do the math to work out how much game time passed since last time
            RepeatingWorkerUTLastStart = RepeatingWorkerUTStart;
            RepeatingWorkerUTStart = Planetarium.GetUniversalTime();
            RepeatingWorkerUTPeriod = RepeatingWorkerUTStart - RepeatingWorkerUTLastStart;

            //Now call the users code function as they will have overridden this
            RepeatingWorker();

            //Now calc the duration
            RepeatingWorkerDuration = (DateTime.Now - Duration);
        }
        #endregion

        #region Standard Monobehaviour definitions-for overriding
        //See this for info on order of execuction
        //  http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html

        /// <summary>
        /// Unity Help: Awake is called when the script instance is being loaded.
        ///
        /// Trigger: Override this for initialization Code - this is before the Start Event
        ///          See this for info on order of execuction: http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html
        /// </summary>
        internal virtual void Awake()
        {
            LogFormatted_DebugOnly("New MBExtended Awakened");
        }

        /// <summary>
        /// Unity: Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        ///
        /// Trigger: This is the last thing that happens before the scene starts doing stuff
        ///          See this for info on order of execuction: http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html
        /// </summary>
        internal virtual void Start()
        {
            LogFormatted_DebugOnly("New MBExtended Started");
        }

        /// <summary>
        /// Unity: This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
        ///
        /// Trigger: This Update is called at a fixed rate and usually where you do all your physics stuff for consistent results
        ///          See this for info on order of execuction: http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html
        /// </summary>
        internal virtual void FixedUpdate()
        { }

        /// <summary>
        /// Unity: LateUpdate is called every frame, if the MonoBehaviour is enabled.
        ///
        /// Trigger: This Update is called just before the rendering, and where you can adjust any graphical values/positions based on what has been updated in the physics, etc
        ///          See this for info on order of execuction: http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html
        /// </summary>
        internal virtual void LateUpdate()
        { }

        /// <summary>
        /// Unity: Update is called every frame, if the MonoBehaviour is enabled.
        ///
        /// Trigger: This is usually where you stick all your control inputs, keyboard handling, etc
        ///          See this for info on order of execuction: http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html
        /// </summary>
        internal virtual void Update()
        { }

        /// <summary>
        /// Unity Help: This function is called when the MonoBehaviour will be destroyed..
        ///
        /// Trigger: Override this for destruction and cleanup code
        ///          See this for info on order of execuction: http://docs.unity3d.com/Documentation/Manual/ExecutionOrder.html
        /// </summary>
        internal virtual void OnDestroy()
        {
            LogFormatted_DebugOnly("Destroying MBExtended");
        }

        #endregion

        #region OnGuiStuff
        //flag to mark when OnceOnly has been done
        private Boolean _OnGUIOnceOnlyHasRun = false;

        /// <summary>
        /// Unity: OnGUI is called for rendering and handling GUI events.
        ///
        /// Trigger: This is called multiple times per frame to lay stuff out etc. 
        ///          Code here ignores the F2 key that disables user interface. So if you are making something to be user hidable then use the RenderingManager.PostDrawQueue functions in here
        ///          Alternatively you could use the MonoBehaviourWindow Type and its DrawWindow Function
        /// </summary>
        private void OnGUI()
        {
            if (!_OnGUIOnceOnlyHasRun)
            {
                //set theflag so this only runs once
                _OnGUIOnceOnlyHasRun = true;
                //set up the skins library
                if (!SkinsLibrary._Initialized)
                    SkinsLibrary.InitSkinList();

                //then pass it on to the downstream derivatives
                OnGUIOnceOnly();
            }

            OnGUIEvery();
        }

        /// <summary>
        /// Extension Function - OnGUIEvery is wrapped in OnGUI with some stuff to facilitate the OnGUIOnceOnly functionality, basically this is the OnGUI function
        /// 
        /// Unity: OnGUI is called for rendering and handling GUI events.
        ///
        /// Trigger: This is called multiple times per frame to lay stuff out etc. 
        ///          Code here ignores the F2 key that disables user interface. So if you are making something to be user hidable then use the RenderingManager.PostDrawQueue functions in here
        ///          Alternatively you could use the MonoBehaviourWindow Type and its DrawWindow Function
        /// </summary>
        internal virtual void OnGUIEvery()
        {

        }

        /// <summary>
        /// Extension Function - this will run only once each time the monobehaviour is awakened
        /// 
        /// Added this so you can put your GUI initialisation code in here. Running GUI initialisation stuff in Awake/Start will throw an error
        /// </summary>
        internal virtual void OnGUIOnceOnly()
        {
            LogFormatted_DebugOnly("Running OnGUI OnceOnly Code");

        }
        #endregion

        #region Assembly/Class Information
        /// <summary>
        /// Name of the Assembly that is running this MonoBehaviour
        /// </summary>
        internal static String _AssemblyName
        { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }

        /// <summary>
        /// Name of the Class - including Derivations
        /// </summary>
        internal String _ClassName
        { get { return this.GetType().Name; } }
        #endregion

        #region Logging
        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void LogFormatted_DebugOnly(String Message, params object[] strParams)
        {
            LogFormatted("DEBUG: " + Message, strParams);
        }

        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(String Message, params object[] strParams)
        {
            Message = String.Format(Message, strParams);                  // This fills the params into the message
            String strMessageLine = String.Format("{0},{2},{1}",
                DateTime.Now, Message,
                _AssemblyName);                                           // This adds our standardised wrapper to each line
            UnityEngine.Debug.Log(strMessageLine);                        // And this puts it in the log
        }

        #endregion
    }
}