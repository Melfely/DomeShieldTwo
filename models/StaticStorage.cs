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

        public static void LoadAsset()
        {
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
            string[] files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "From The Depths", "Mods", "DomeShieldTwo", "Assets", "shielddome"));
            foreach (string file in files)
            {
                AdvLogger.LogWarning("Found file: " + file, LogOptions._AlertDevInGame);
            }
            
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "From The Depths", "Mods", "DomeShieldTwo", "Assets", "shielddome", "shielddomebundle");

            if (!File.Exists(path))
            {
                AdvLogger.LogError("AssetBundle not found at: " + path, LogOptions._AlertDevInGame);
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
