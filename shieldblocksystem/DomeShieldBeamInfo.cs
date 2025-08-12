using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldBeamInfo
    {
        public float EnergyShortage
        {
            get
            {
                return this.MaxEnergy - this.Energy;
            }
        }
        public float Energy { get; set; }
        public float MaxEnergy { get; private set; }

        public float EnergyAvailablePerSecond { get; private set; }
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
                return this.CubicMetresOfPumping * (int)Rounding.R0(DomeShieldPump.CubicMeterPowerNeeded);
            }
            
            //Will need some adjusting later on when we get "pumps" figured out.
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
        public void GiveEnergy(float n)
        {
            bool flag = n < 0f;
            if (flag)
            {
                AdvLogger.LogWarning("Why is energy given to dome shield beam negative?", LogOptions._AlertDevInGame);
            }
            else
            {
                bool flag2 = n > 0f;
                if (flag2)
                {
                    this.Energy += n;
                    this.Energy = Mathf.Min(this.Energy, this.MaxEnergy);
                    this.CalculateEnergyAvailable();
                }
            }
        }

        public void TakeEnergy(float n)
        {
            bool flag = n < 0f;
            if (flag)
            {
                AdvLogger.LogWarning("Why is energy taken from dome shield beam negative?", LogOptions._AlertDevInGame);
            }
            else
            {
                bool flag2 = n > 0f;
                if (flag2)
                {
                    this.Energy -= n;
                    this.Energy = Mathf.Max(0f, this.Energy);
                    this.CalculateEnergyAvailable();
                }
            }
        }

        // Token: 0x06001860 RID: 6240 RVA: 0x0007A224 File Offset: 0x00078424
        public float SetParts(float cavityCapacity, int hardeners, int cubicMetresOfPumping, int transformers, int rectifiers)
        {
            
            bool flag = false;
            float num = cavityCapacity - this.MaxEnergy;
            bool hadInitialCharge = this.HadInitialCharge;
            if (hadInitialCharge)
            {
                bool flag2 = this.Hardeners != hardeners || this.CubicMetresOfPumping != cubicMetresOfPumping || this.Transformers != transformers;
                if (flag2)
                {
                    flag = true;
                }
            }
            this.Hardeners = hardeners;
            this.CubicMetresOfPumping = cubicMetresOfPumping;
            this.Transformers = transformers;
            this.Rectifiers = rectifiers;
            this.MaxEnergy = cavityCapacity;
            bool flag3 = !flag;
            if (flag3)
            {
                this.Energy = Mathf.Clamp(this.Energy, 0f, this.MaxEnergy);
            }
            else
            {
                this.Energy = 0f;
            }
            this.CalculateEnergyAvailable();
            return num;
            
            //Yeaaahhh... this one's gonna take some work. FrequencyDoublers and Destabalisers won't be used, but we can easily swap those out for int 'other thing'.
            //Let's see if this plays nicely.
        }

        public void Reset()
        {
            this.Hardeners = 0;
            this.CubicMetresOfPumping = 0;
            this.Transformers = 0;
            this.Rectifiers = 0;
            this.MaxEnergy = 0f;
            this.Energy = 0f;
            this.EnergyAvailablePerSecond = 0f;
            //We can add the new modifer blocks (what is replacing FD's and DE's) here.
        }
        //Skipped SetQSwitch, not relavent here. Though, should shield class be referenced here...?
        public float GetSpawnEnergy()
        {
            return Mathf.Min(this.MaxEnergy, (float)this.CubicMetresOfPumping * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter * DomeShieldConstants.DSCavitySpawnPumpingSeconds);
            //This should be easy to fix, just some class name changes.
        }
        //Skipped GetReadyToFire, it might not be relevant.
        //Skipped IsPulsed and IsCW. This will always be "CW".
        public float GetHealthThisFrame()
        {
            bool flag = this.EnergyAvailablePerSecond == 0f && this.Energy > 0f;
            if (flag)
            {
                this.CalculateEnergyAvailable();
            }
            return Mathf.Min(this.EnergyAvailablePerSecond / 40f, this.Energy) * this.HealthPerEnergy;
        }
        public void UseEnergyAvailableThisFrame()
        {
            this.TakeEnergy(this.EnergyAvailablePerSecond / 40);
        }
        public void CalculateEnergyAvailable()
        {
            this.EnergyAvailablePerSecond = this.EnergyFractionPerSec * this.Energy;
            //this.EnergyAvailablePerSecond = this.EnergyFractionPerSec * ((this.Regulator != null) ? this.MaxEnergy : this.Energy);
            //Will need a bit of tinkering. Kept the original for reference in comments
        }
        public DomeShieldBeamInfo()
        {
        }
        //Is adding this important...?
        public int Hardeners;

        public int CubicMetresOfPumping;

        public int Transformers;

        public int Rectifiers;

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
