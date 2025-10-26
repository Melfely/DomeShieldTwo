using AdvShields;
using BrilliantSkies.Core.Timing;
using DomeShieldTwo.NewShieldBlockSystem;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldNode : GovernedNode<AdvShieldProjector>
    {
        public AdvShieldProjector projector;
        public DomeShieldNode(AllConstruct C, AdvShieldProjector B)
        : base(C, 10000, B) //Make sure this has no errors later?
        {
            projector = B;
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
                float num2 = 0f;
                for (int i = 0; i < this.dSPLs.Count; i++)
                {
                    DomeShieldPowerLink sPL = this.dSPLs[i];
                    for (int j = 0; j < sPL.dSBeamInfo.Length; j++)
                    {
                        DomeShieldBeamInfo beamInfo = sPL.dSBeamInfo[j];
                        num += Mathf.Min((float)beamInfo.TotalEnergyInBeam * 0.05f * GameTimer.Instance.FixedDeltaTimeCache, 1);
                        num2 += beamInfo.MaxEnergy;
                    }
                }
                MaximumEnergy = num2;
                request.IdealCalculationValue = num;
                request.IdealPower = num * 0.05f / GameTimer.Instance.FixedDeltaTimeCache;
            }
        }
        
        private void Use(IPowerRequestRecurring request)
        {
            
            float num = request.IdealCalculationValue * request.FractionOfPowerRequestedThatWasProvided;
            for (int i = 0; i < this.dSPLs.Count; i++)
            {
                DomeShieldPowerLink sPL = this.dSPLs[i];
                for (int j = 0; j < sPL.dSBeamInfo.Length; j++)
                {
                    DomeShieldBeamInfo beamInfo = sPL.dSBeamInfo[j];
                    float energyShortage = 1;
                    float num2 = (float)beamInfo.TotalEnergyInBeam * 0.05f * request.DeltaTime;
                    num2 = Mathf.Min(num2, num);
                    num2 = Mathf.Min(num2, energyShortage);
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
            this.dSPLs.Clear();
            this.matrixComputer = null;
        }
        public override void AfterSendingOutFeelers()
        {
            foreach (DomeShieldPowerLink link in this.dSPLs)
            {
                link.CalculateActualEnergyAndPowerModifier();
            }
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
            for (int i = 0; i < this.dSPLs.Count; i++)
            {
                DomeShieldPowerLink dSPL = this.dSPLs[i];
                bool flag = dSPL == null || !dSPL.IsAlive;
                if (!flag)
                {
                    for (int j = 0; j < dSPL.dSBeamInfo.Length; j++)
                    {
                        DomeShieldBeamInfo beamInfo = dSPL.dSBeamInfo[j];
                        bool flag3 = beamInfo.TotalEnergyInBeam > 0;
                        if (flag3)
                        {
                            this.LastCWEnergySum = 0f;
                        }
                        num2 += beamInfo.Hardeners;
                        num3 += beamInfo.TotalEnergyInBeam;
                        num4 += beamInfo.MaxEnergy;
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
        public float GetMaximumEnergy()
        {
            
            float num = 0f;
            for (int i = 0; i < this.dSPLs.Count; i++)
            {
                DomeShieldPowerLink dSPL = this.dSPLs[i];
                for (int j = 0; j < dSPL.dSBeamInfo.Length; j++)
                {
                    DomeShieldBeamInfo beamInfo = dSPL.dSBeamInfo[j];
                    num = beamInfo.MaxEnergy;
                    MaximumEnergy = num;
                }
            }
            return num;
            //Read above note
        }
        //Skipped HasToWaitForCharge
        //Skipping HasEnoughShotEnergy
        //^^THIS IS THE ONLY THING THAT WOULD USE GET CW ENERGY AVAILABLE!
        public float GetFirePower()
        {
            return base.GoverningBlock.GetFirePower();
            //Don't forget to make sure we know what this does.
        }
        //Variables below here
        private IPowerRequestRecurring _powerUse;

        public List<DomeShieldPowerLink> dSPLs = new List<DomeShieldPowerLink>();

        public DomeShieldMatrixComputer? matrixComputer;

        public string ConnectedCard = "None";

        //public List<DomeShieldEnergyLink> dSELs = new List<DomeShieldEnergyLink>();

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
