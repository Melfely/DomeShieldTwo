using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldBeamInfo
    {
        public float MaxEnergy { get; set; }

        public float EnergyAvailablePerSecond { get; set; }
        public float CavityDischargeRate
        {
            
            get
            {
                return 0.1f;
            }
            
            //This will need rewriten to function without a regulator, as we won't be using it. In fact, it might even be affected by the shield class...
        }
        public float EnergyFractionPerSec
        {
            
            get
            {
                return 0.1f;
            }
            
            //This will need rewritten, as the fraction of energy used to actively regen the shield will be different.
        }
        //Skipped ShotsPerSec, not relevant here.
        public int PowerPerSec
        {
            
            get
            {
                return this.TotalCapacitorSize * (20 * GetPowerMultiplier());
            }
            //We can fine tune this number
        }
        public float HealthPerEnergy
        {
            
            get
            {
                return DomeShieldConstants.HealthPerEnergy;
                //Do we want to edit this later? Perhaps make it slightly more or less efficient based on shield size, or other things?
            }
            
        }
        //Skipped DamagePerSec, not relevant here.

        public int GetPowerMultiplier()
        {
            float multiplier = 1 * (GetOverchargerPowerMultiplier() * GetRegulatorPowerMultiplier());
            Math.Round(multiplier, 0);
            return (int)multiplier;
        }

        private float GetOverchargerPowerMultiplier()
        {
            float overchargers = (float)Overchargers;
            if (Overchargers == 0) return 1;
            else return 1f + ((overchargers * 1.15f) * (0.1f - ((float)Math.Pow(100 / overchargers, 0.1))));
        }
        private float GetRegulatorPowerMultiplier()
        {
            float baseReduction = (Overchargers * 0.02f);
            float adjustedReduction = baseReduction - (Overchargers * 0.003f);
            if (Overchargers == 1) adjustedReduction = 0.02f;
            return adjustedReduction;
        }
        public float SetParts(float cavityCapacity, int hardeners, int transformers, int rectifiers)
        {
            float num = cavityCapacity - this.MaxEnergy;
            this.Hardeners = hardeners;
            this.TotalCapacitorSize = (int)cavityCapacity;
            this.Transformers = transformers;
            this.Rectifiers = rectifiers;
            this.MaxEnergy = cavityCapacity;
            return num;
            
            //Yeaaahhh... this one's gonna take some work. FrequencyDoublers and Destabalisers won't be used, but we can easily swap those out for int 'other thing'.
            //Let's see if this plays nicely.
        }

        public void Reset()
        {
            this.Hardeners = 0;
            this.TotalCapacitorSize = 0;
            this.Transformers = 0;
            this.Rectifiers = 0;
            this.MaxEnergy = 0f;
            this.EnergyAvailablePerSecond = 0f;
            this.Overchargers = 0;
            this.Regulators = 0;
            //We can add the new modifer blocks (what is replacing FD's and DE's) here.
        }
        //Skipped SetQSwitch, not relavent here. Though, should shield class be referenced here...?

        //Skipped GetReadyToFire, it might not be relevant.
        //Skipped IsPulsed and IsCW. This will always be "CW".

        public DomeShieldBeamInfo()
        {
        }
        //Is adding this important...?
        public int Hardeners;

        public int TotalCapacitorSize;

        public int Transformers;

        public int Rectifiers;

        public int Overchargers;

        public int Regulators;

        //public LaserOutputRegulator Regulator;
        //Will we need a regulator?

        [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private float k__BackingFieldE;

	    [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private float k__BackingFieldME;

	    [CompilerGenerated]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private float k__BackingFieldEPS;

	    public bool HadInitialCharge = false;

        private float _fireTime = -1f;

        private readonly GameTime _lastFireTime = new GameTime();

        public int FeelerVersion;

        private readonly GameTime _lastFireTimeTemp = new GameTime();
    }
}
