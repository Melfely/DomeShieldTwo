using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using UnityEngine;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldRequestReturn
    {
        static DomeShieldRequestReturn()
        {

        }
        public float BaseHealing { get; set; }
        public float Healing { get; set; }
        public float ArmourClass {  get; set; }
        public bool WorthFiring
        {
            get
            {
                return this.Healing > 0f;
            }
        }
        public DomeShieldRequestReturn (float healing, float ac)
        {
            this.BaseHealing = healing;
            this.Healing = healing;
            this.ArmourClass = ac;
        }
        public static ILocFile _locFile = Loc.GetFile("DomeShield_RequestReturn");
    }
}
