using AdvShields.Models;
using BrilliantSkies.Core.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo
{
    internal class AssetLoader : MonoBehaviour
    {
        void Start()
        {
            StaticStorage.LoadAsset();
            AdvLogger.LogInfo("Dome Shield Assets were loaded");
        }
    }
}
