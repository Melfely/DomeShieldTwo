using BrilliantSkies.Core.Timing;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldNode : GovernedNode<DomeShieldMultipurpose>
    {
        public DomeShieldNode(AllConstruct C, DomeShieldMultipurpose B)
        : base(C, 10000, B) //Make sure this has no errors later?
        {
            this._powerUse = new PowerRequestRecurring(this, PowerRequestType.Shield, new Func<int>(base.GoverningBlock.PriorityData.Get), new Action<int>(base.GoverningBlock.PriorityData.Set))
            {
                fnCalculateIdealPowerUse = new Action<IPowerRequestRecurring>(this.IdealUsage),
                fnUseAllocatedPower = new Action<IPowerRequestRecurring>(this.Use)
            };
            this.MainConstruct.PowerUsageCreationAndFuelRestricted.AddRecurringPowerUser(this._powerUse);
        }
        private void IdealUsage(IPowerRequestRecurring request)
        {
            
            bool munitionsWaitingToBeLoaded = this.MainConstruct.MunitionsWaitingToBeLoaded;
            if (munitionsWaitingToBeLoaded)
            {
                request.IdealCalculationValue = 0f;
                request.IdealPower = 0f;
            }
            else
            {
                float num = 0f;
                for (int i = 0; i < this.dSCouplers.Count; i++)
                {
                    DomeShieldCoupler shieldCoupler = this.dSCouplers[i];
                    for (int j = 0; j < shieldCoupler.dSBeamInfo.Length; j++)
                    {
                        DomeShieldBeamInfo beamInfo = shieldCoupler.dSBeamInfo[j];
                        float energyShortage = beamInfo.EnergyShortage;
                        num += Mathf.Min((float)beamInfo.CubicMetresOfPumping * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter * GameTimer.Instance.FixedDeltaTimeCache, energyShortage);
                    }
                }
                request.IdealCalculationValue = num;
                request.IdealPower = num * DomeShieldConstants.DSPowerPerCavityEnergy / GameTimer.Instance.FixedDeltaTimeCache;
            }
            
            //Read this when you aren't sick
        }
        private void Use(IPowerRequestRecurring request)
        {
            
            float num = request.IdealCalculationValue * request.FractionOfPowerRequestedThatWasProvided;
            for (int i = 0; i < this.dSCouplers.Count; i++)
            {
                DomeShieldCoupler shieldCoupler = this.dSCouplers[i];
                for (int j = 0; j < shieldCoupler.dSBeamInfo.Length; j++)
                {
                    DomeShieldBeamInfo beamInfo = shieldCoupler.dSBeamInfo[j];
                    float energyShortage = beamInfo.EnergyShortage;
                    float num2 = (float)beamInfo.CubicMetresOfPumping * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter * request.DeltaTime;
                    num2 = Mathf.Min(num2, num);
                    num2 = Mathf.Min(num2, energyShortage);
                    beamInfo.GiveEnergy(num2);
                    num -= num2;
                }
            }
            
            //Come back to this when we decide how the shield will use power.
        }
        protected override void OnNodeDestroy()
        {
            this.MainConstruct.PowerUsageCreationAndFuelRestricted.RemoveRecurringPowerUser(this._powerUse);
        }

        public override void PriorToSendingOutFeelers()
        {
            
            this.dSCouplers.Clear();
            //Will we need to clear anything else? Probably not...
        }
        /*
        public override void AfterSendingOutFeelers()
        {
            
            foreach (DomeShieldCoupler shieldCoupler in this.dSCouplers)
            {
                foreach (DomeShieldBeamInfo beamInfo2 in shieldCoupler.beamInfo)
                {
                    beamInfo2.Regulator = this.Regulator;
                }
            }
            
            //Whatever we utilize in place of a coupler, aka whatever block fills the role of the coupler, come back to this
        }
        */
        //^^Skipped this, because it turns out the only thing this does is search for the Regulator. We aren't using a regulator.
        
        public DomeShieldRequestReturn GetCWEnergyAvailable(bool takeIt)
        {
            float num = 0f;
            int num2 = 0;
            int num3 = 0;
            float num4 = 0f;
            this.LastCWEnergySum = float.MaxValue;
            if (takeIt)
            {
                this.LastCWEnergyLeft = 0f;
            }
            for (int i = 0; i < this.dSCouplers.Count; i++)
            {
                DomeShieldCoupler dSCoupler = this.dSCouplers[i];
                bool flag = dSCoupler == null || !dSCoupler.IsAlive;
                if (!flag)
                {
                    for (int j = 0; j < dSCoupler.dSBeamInfo.Length; j++)
                    {
                        DomeShieldBeamInfo beamInfo = dSCoupler.dSBeamInfo[j];
                        bool flag3 = beamInfo.CubicMetresOfPumping > 0;
                        if (flag3)
                        {
                            this.LastCWEnergySum = 0f;
                        }
                        num2 += beamInfo.Hardeners;
                        num3 += beamInfo.CubicMetresOfPumping;
                        num4 += beamInfo.MaxEnergy;
                        bool flag4 = beamInfo.Energy > 0f;
                        if (flag4)
                        {
                            num += beamInfo.GetHealthThisFrame();
                            if (takeIt)
                            {
                                beamInfo.UseEnergyAvailableThisFrame();
                                this.LastCWEnergyLeft += beamInfo.Energy;
                                this.CurrentEnergyDirty = true;
                            }
                        }
                    }
                }
            }
            float ap = DomeShieldConstants.GetAC(num2, num3, true, num4);
            this.LastCWEnergySum = ((this.LastCWEnergySum == float.MaxValue) ? 0f : this.LastCWEnergySum);
            this.LastCWEnergySum = Mathf.Max(this.LastCWEnergySum, num);
            return new DomeShieldRequestReturn(num, ap);
        
        //Come back to this when you have more things done, this looks like a later thing. You will need to change "LaserRequestReturn" to something, probably "DomeShieldRequestReturn"
        //This seems to be the constant firing version of the laser? There's another method for pulsed, we can skip that here.
        //Let's change the name of this method later.
        //This references LaserRequestReturn, which might not be needed here at all? What calls this method exactly?
        }
        public float GetTotalEnergyAvailable()
        {
            
            float num = 0f;
            foreach (DomeShieldCoupler dSCoupler in this.dSCouplers)
            {
                foreach (DomeShieldBeamInfo beamInfo2 in dSCoupler.dSBeamInfo)
                {
                    num += beamInfo2.Energy;
                }
            }
            return num;
            //Once we get the equivalent of the LaserCoupler set up, come back to this. Notice that it's a float.
        }
        public float GetMaximumEnergy()
        {
            
            float num = 0f;
            for (int i = 0; i < this.dSCouplers.Count; i++)
            {
                DomeShieldCoupler dSCoupler = this.dSCouplers[i];
                for (int j = 0; j < dSCoupler.dSBeamInfo.Length; j++)
                {
                    DomeShieldBeamInfo beamInfo = dSCoupler.dSBeamInfo[j];
                    num += beamInfo.MaxEnergy;
                }
            }
            return num;
            //Read above note
        }
        //Skipped HasToWaitForCharge
        public float TotalEnergyFraction()
        {
            
            bool maximumEnergyDirty = this.MaximumEnergyDirty;
            if (maximumEnergyDirty)
            {
                this.MaximumEnergy = this.GetMaximumEnergy();
                this.MaximumEnergyDirty = false;
            }
            bool currentEnergyDirty = this.CurrentEnergyDirty;
            if (currentEnergyDirty)
            {
                this.CurrentEnergy = this.GetTotalEnergyAvailable();
                this.CurrentEnergyDirty = false;
            }
            return this.CurrentEnergy / this.MaximumEnergy;
            
            //Come back to this when energy is figured out.
        }
        //Skipping HasEnoughShotEnergy
        //^^THIS IS THE ONLY THING THAT WOULD USE GET CW ENERGY AVAILABLE!
        public float GetFirePower()
        {
            return base.GoverningBlock.GetFirePower();
            //Don't forget to make sure we know what this does.
        }
        //Variables below here
        private IPowerRequestRecurring _powerUse;

        public List<DomeShieldCoupler> dSCouplers = new List<DomeShieldCoupler>();

        public bool WaitingForCharge;

        public bool MaximumEnergyDirty;

        public bool CurrentEnergyDirty;

        public float MaximumEnergy;

        public float CurrentEnergy;

        //Learn what these do.

        protected float LastCWEnergySum;

        protected float LastCWEnergyLeft;

        //Reword these when we reword the method they appear in.
    }
}
