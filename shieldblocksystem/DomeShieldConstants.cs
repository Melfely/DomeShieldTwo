using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldConstants
    {
        //Skipped GetSuperheaterFireFuelFactor, though we might use something similar.
        public static float GetAC(int hardeners, int pumps, bool isContinuous, float totalEnergyCapacity /* We might need to add arguments */)
        {
            
            float num = totalEnergyCapacity / DomeShieldConstants.DSPumpCavityCapacityEquivalent;
            float num2 = (float)pumps + num;
            float num3 = DomeShieldConstants.BaseAC + (float)hardeners * DomeShieldConstants.ACPerHardener / Mathf.Max(1f, num2);
            return num3;
            
            //We will need to edit this a bit. Probably call it GetAc instead of Ap, and adjust the formula later on. We won't be having a Pulsed AP. In fact, ShieldClass will factor in here!
        }
        public DomeShieldConstants()
        {
        }
        static DomeShieldConstants()
        {
        }
        //Notice that the following are all static. I have deleted all that won't be a factor, so you will see less.
        //THESE ARE ALL PUBLIC STATICS. THE NAMES MUST BE CHANGED SLIGHTLY.

        public static float DSEnergyPumpRatePerCubicMeter = 12f;

        public static float DSPowerPerCavityEnergy = 2.5f;

        public static float DSCavitySpawnPumpingSeconds = 30f;

        public static float BaseDSCavitySize = 125f;

        public static float BaseAC = 20;

        public static float ACPerHardener = 50f;

        public const float BaseDSCavityDischargeRate = 0.1f;

        public static float HealthPerEnergy = 1f;

        public static float DSPumpCavityCapacityEquivalent = 10000f;
    }
}
