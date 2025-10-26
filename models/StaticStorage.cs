using AdvShields.Behaviours;
using System.IO;
using System;
using UnityEngine;
using BrilliantSkies.Core.Logger;

namespace AdvShields.Models
{
    public static class StaticStorage
    {
        public static GameObject ShieldDomeObject { get; set; }

        public static GameObject HitEffectObject { get; set; }
        private static AssetBundle loadedBundle;
        public static bool HasLoaded = false;
        public static bool IsInBetaTest = false;

        public static void LoadAsset()
        {
            if (HasLoaded == true) return;
            AdvLogger.LogInfo("Dome shield is loading assets...", LogOptions._AlertDevInGame);
            HasLoaded = true;
            LoadAssetBundle();
            LoadPrefabs();
            /*
            string bundlePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                 "From The Depths", "Mods", "DomeShieldNewAge", "Assets", bundleName);
            //AssetBundle bundle = AssetBundle.LoadFromMemory(Properties.Resources.shielddome);
            /*
            GameObject objShield = bundle.LoadAsset<GameObject>("Assets/external/BasicShield.prefab");
            objShield.AddComponent<ShieldDomeBehaviour>();
            ShieldDomeObject = objShield;

            GameObject objEffect = bundle.LoadAsset<GameObject>("Assets/external/BasicShieldHitEffect.prefab");
            objEffect.AddComponent<HitEffectBehaviour>();
            HitEffectObject = objEffect;
            */
            /*
            if (File.Exists(bundlePath))
            {
                byte[] binary = File.ReadAllBytes(bundlePath); // Read the file into a byte array
                AssetBundle bundle = AssetBundle.LoadFromMemory(binary); // Load from memory

                if (bundle == null)
                {
                    AdvLogger.LogWarning("Failed to load AssetBundle from memory!", LogOptions._AlertDevInGame);
                    return;
                }

                GameObject objShield = bundle.LoadAsset<GameObject>("BasicShield"); // Adjust asset name if needed
                if (objShield != null)
                {
                    objShield.AddComponent<ShieldDomeBehaviour>();
                    ShieldDomeObject = objShield;
                }
                else
                {
                    AdvLogger.LogWarning("Failed to load BasicShield from memory!", LogOptions._AlertDevInGame);
                }
            }
            else
            {
                AdvLogger.LogWarning("AssetBundle file not found at: " + bundlePath, LogOptions._AlertDevInGame);
            }
            */
        }
        public static void LoadAssetBundle()
        {
            /*
            string[] files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "From The Depths", "Mods", "DomeShieldTwo", "Assets", "shielddome"));
            foreach (string file in files)
            {
                AdvLogger.LogWarning("Found file: " + file, LogOptions._AlertDevInGame);
            }
            */
            string modfoldername = "null";
            int DomeShieldsInstalled = 0;
            string pathcheck = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "From The Depths", "Mods");
            string[] subdirectories = Directory.GetDirectories(pathcheck);
            foreach (string subdir in subdirectories)
            {
                if (subdir.ToLower().EndsWith("domeshieldbetatest"))
                {
                    modfoldername = subdir;
                    IsInBetaTest = true;
                    DomeShieldsInstalled++;
                }
                if (subdir.ToLower().EndsWith("domeshieldtwo"))
                {
                    modfoldername = subdir;
                    IsInBetaTest = false;
                    DomeShieldsInstalled++;
                }
                if (subdir.ToLower().EndsWith("domedhield"))
                {
                    AdvLogger.LogError("WARNING: You have the pre-Unity6 version of the Dome Shield. Please navigate to Documents -> From The Depths -> Mods and delete all Dome Shield folders, then re-install this mod.", LogOptions._AlertDevAndCustomerInGame);
                    IsInBetaTest = false;
                    DomeShieldsInstalled++;
                }
            }
            if (DomeShieldsInstalled > 1) 
            {
                AdvLogger.LogError("WARNING: You seem to have more than one install of the Dome Shield. It is likely that you have the Beta test as well as the non-beta test. Please delete one of these at Documents -> From The Depths -> Mods. THE MOD WILL NOT WORK WITH A DOUBLE INSTALL.", LogOptions._AlertDevAndCustomerInGame);
            }
            if (DomeShieldsInstalled == 0) AdvLogger.LogError("ERROR: The Dome Shield mod folder was not located. Please do not manually change the name of the folder when you download it. (If you are sure didn't change the install name at all, please send me proof and I will fix this.", LogOptions._AlertDevAndCustomerInGame);
            if (IsInBetaTest) AdvLogger.LogInfo("NOT AN ERROR. You are using the BETA TEST version of the Dome Shield mod. You are likely to encounter bugs, unfinished pieces, and unbalanced mechanics. Please provide feedback in the discord server for this mod. Thank you for testing this mod!", LogOptions._AlertDevAndCustomerInGame);

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "From The Depths", "Mods", modfoldername, "Assets", "shielddome", "shielddomebundle");

            if (!File.Exists(path))
            {
                AdvLogger.LogError("AssetBundle not found at: " + path + ", might be in beta test. Trying alternate filepath.", LogOptions._AlertDevInGame);
                return;
            }

            loadedBundle = AssetBundle.LoadFromFile(path);
            if (loadedBundle == null)
            {
                AdvLogger.LogError("Failed to load AssetBundle!", LogOptions._AlertDevInGame);
            }
            else
            {
                AdvLogger.LogWarning("AssetBundle loaded successfully.", LogOptions._AlertDevInGame);
            }
        }

        public static void LoadPrefabs()
        {
            if (loadedBundle == null) {AdvLogger.LogError("loadedBundle = null", LogOptions._AlertDevInGame); return; }

            AdvLogger.LogWarning("Loading BasicShield Prefab...", LogOptions._AlertDevInGame);
            GameObject objShield = loadedBundle.LoadAsset<GameObject>("BasicShield");
            if (objShield != null)
            {
                AdvLogger.LogWarning("Got BasicShield Prefab", LogOptions._AlertDevInGame);
                objShield.AddComponent<ShieldDomeBehaviour>();
                ShieldDomeObject = objShield;
            }
            AdvLogger.LogWarning("Loading HitEffect Prefab...", LogOptions._AlertDevInGame);
            GameObject objEffect = loadedBundle.LoadAsset<GameObject>("BasicShieldHitEffect");
            if (objEffect != null)
            {
                AdvLogger.LogWarning("Got HitEffect Prefab", LogOptions._AlertDevInGame);
                objEffect.AddComponent<HitEffectBehaviour>();
                HitEffectObject = objEffect;
            }
        }
        /*
        public static void LoadAsset()
        {
            string modPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                          "From The Depths", "Mods", "DomeShieldNewAge", "Assets");

            string shieldPath = Path.Combine(modPath, "BasicShield.obj");
            string effectPath = Path.Combine(modPath, "BasicShieldHitEffect.obj");

            GameObject objShield = LoadObj(shieldPath);
            if (objShield != null)
            {
                objShield.AddComponent<ShieldDomeBehaviour>();
                ShieldDomeObject = objShield;
            }

            GameObject objEffect = LoadObj(effectPath);
            if (objEffect != null)
            {
                objEffect.AddComponent<HitEffectBehaviour>();
                HitEffectObject = objEffect;
            }
        }

        private static GameObject LoadObj(string path)
        {
            if (File.Exists(path))
            {
                Debug.Log("Loading OBJ file: " + path);
                return new GameObject(Path.GetFileNameWithoutExtension(path));
            }
            else
            {
                Debug.LogError("OBJ file not found: " + path);
                return null;
            }
        }
        */
    }
}
