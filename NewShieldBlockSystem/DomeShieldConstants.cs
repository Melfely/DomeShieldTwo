using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldConstants
    {
        //Skipped GetSuperheaterFireFuelFactor, though we might use something similar.
        public static float GetAC(int hardeners, int pumps, bool isContinuous, float totalEnergyCapacity /* We might need to add arguments */)
        {
            return BaseAC;
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

        public static float BaseDSCapacitorSize = 750f;

        public static float BaseAC = 20;

        public static float ACPerHardener = 50f;

        public static float HealthPerEnergy = 1f;
    }
}
