using System;
using System.Reflection;
using AdvShields.Models;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Modding;
using HarmonyLib;
using UnityEngine;

namespace DomeShieldTwo
{
    public class Plugin : GamePlugin_PostLoad
    {
        /// <summary>Used in FtD's log to indicate the name of the loaded plugin</summary>
        public String name { get { return "DomeShield2"; } }
        /// <summary>Not used in FtD</summary>
        public Version version { get { return new Version(2, 0); } }
        /// <summary>
        /// Called by FtD when the plugin is loaded
        /// </summary>
        public void OnLoad()
        {
            AdvLogger.LogInfo("Loading Dome Shield Beta Test, Update 2. Does this match the the most recent version?");
            AdvLogger.LogWarning("Attempting to load the assets", LogOptions._AlertDevInGame);
            GameObject loaderObj = new GameObject("AssetLoader");
            loaderObj.AddComponent<AssetLoader>();
            //StaticStorage.LoadAsset();
            AdvLogger.LogWarning("Assets loaded!", LogOptions._AlertDevInGame);
            

            AdvLogger.LogWarning("Attempting to setup harmony", LogOptions._AlertDevInGame);
            Harmony harmony = new Harmony("AdvShields_Patch");
            AdvLogger.LogWarning("Created a harmony, running PatchAll", LogOptions._AlertDevInGame);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            AdvLogger.LogWarning("Harmony successfully loaded", LogOptions._AlertDevInGame);
        }
        /// <summary>
        /// Called when all the 'OnLoad()' methods of all the plugins have been called
        ///
        /// Specific from 'GamePlugin_PostLoad'
        /// </summary>
        /// <returns>'true' if no problem, 'false' if any problem</returns>
        public Boolean AfterAllPluginsLoaded()
        {
            return true;
        }
        /// <summary>
        /// Not called by FtD
        /// </summary>
        public void OnSave()
        {
        }
    }
}
