using BrilliantSkies.Blocks.BreadBoards.GenericGetter;
using BrilliantSkies.Blocks.Weapons;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using AdvShields;
using BrilliantSkies.Core.Logger;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldMultipurpose : Block, IGoverningBlock<DomeShieldNode>, IBlock, IAlive, IFlagState, IBlockWithNode<DomeShieldNode>, IBlockWithFirepowerStats
    {
        public INode NodeInterface
        {
            get
            {
                return this.Node;
            }
        }
        public static bool HasCreatedTypeSet = false;
        public static bool HasAddedToTypeSet = false;
        public static DomeShieldNodeSet nodeSetToAdd;

        public DomeShieldNode Node { get; set; }

        public PowerUserData PriorityData { get; set; } = new PowerUserData(37852U);
        public float LSRCurrentEnergy
        {
            get
            {
                return this.Node.CurrentEnergy;
                //What does this do?
            }
        }
        [Readable(500U, "Total energy in dome shield cavities ", "")]
        public float CavitySize
        {
            get
            {
                return this.Node.dSCouplers.Sum((DomeShieldCoupler t) => t.dSBeamInfo.Sum((DomeShieldBeamInfo tt) => tt.Energy));
                //This needs to be fixed when we decide what couplers are changed to.
            }
        }
        //These two seems to be for breadboard? Make sure the 500 series isn't taken yet.
        [Readable(501U, "Maximum energy in dome shield cavities ", "")]
        public float CavitySizeMax
        {
            get
            {
                return this.Node.MaximumEnergy;
            }
        }

        public float StoredEnergyMaterialCost()
        {
            return this.Node.dSCouplers.Sum((DomeShieldCoupler C) => C.GetContentMaterialCostForBlueprint());
            //This needs to be fixed when we decide what couplers are changed to.
        }

        public float PumpEnergyPerSec()
        {
            
            int num = 0;
            foreach (DomeShieldCoupler shieldCoupler in this.Node.dSCouplers)
            {
                num += shieldCoupler.dSBeamInfo.Sum((DomeShieldBeamInfo beam) => beam.CubicMetresOfPumping);
            }
            return (float)num * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter;
            
            //This needs to be fixed when we decide what couplers are changed to. We also need to decide if this is how we will do energy.
        }
        //Next three are for Firepower calcs, leave them be.
        public float GetAmmoPerSec()
        {
            return 0f;
        }
        public float GetFuelPerSec()
        {
            return 0f;
        }
        public float GetProjectileHealthPerSec()
        {
            return 0f;
        }
        public float GetPowerUsed()
        {
            
            int num = 0;
            foreach (DomeShieldCoupler shieldCoupler in this.Node.dSCouplers)
            {
                num += shieldCoupler.dSBeamInfo.Sum((DomeShieldBeamInfo beam) => beam.CubicMetresOfPumping);
            }
            return (float)num * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter * DomeShieldConstants.DSPowerPerCavityEnergy;
            //This needs to be fixed when we decide what couplers are changed to. We also need to decide if this is how we will do energy.
            //Notice that this is very similar to PumpEnergyPerSecond...
            
        }
        public override float GetFirePower()
        {
            
            bool flag = this.Node == null;
            float num;
            if (flag)
            {
                num = 0f;
            }
            else
            {
                float num2 = 0f;
                for (int i = 0; i < 5; i++)
                {
                    float num3 = 0f;
                    float num4 = 0f;
                    int num5 = 0;
                    int num6 = 0;
                    foreach (DomeShieldCoupler shieldCoupler in this.Node.dSCouplers)
                    {
                        foreach (DomeShieldBeamInfo beamInfo2 in shieldCoupler.dSBeamInfo)
                        {
                            beamInfo2.CalculateEnergyAvailable();
                            num4 += beamInfo2.MaxEnergy;
                            num5 += beamInfo2.Hardeners;
                            num3 += 1f; //beamInfo2.DamagePerSec;
                            //We need to find something to replace DamagePerSec for the sake of GetLaserPower!
                            //Or... see below
                            num6 += beamInfo2.CubicMetresOfPumping;
                        }
                    }
                    float ac = DomeShieldConstants.GetAC(num5, num6, i == 0, num4);
                    num2 += FirepowerHandler.GetLaserPower(num3, ac);
                    //We can totally write this ourselves. We're coming back to this another time.
                }
                num = num2;
            }
            return num;
            
            //Final firepower calc. //This needs to be fixed when we decide what couplers are changed to.
        }
        public override void CheckStatus(IStatusUpdate updater)
        {
            base.CheckStatus(updater);
            bool flag = this.Node == null;
            if (!flag)
            {
                bool flag2 = 0f == this.Node.GetTotalEnergyAvailable();
                if (flag2)
                {
                    updater.FlagError(this, DomeShieldMultipurpose._locFile.Get("Error_NoEnergy", "No energy available", true));
                }
            }
        }
        private void UpdateCharge(ITimeStep dt)
        {
            this.Node.MaximumEnergyDirty = true;
            this.Node.CurrentEnergyDirty = true;
        }
        /*
        public override void BlockStart()
        {
            base.BlockStart();
            MainConstruct construct = (MainConstruct)base.MainConstruct;
            if (!DomeShieldMultipurpose.HasAddedToTypeSet && !DomeShieldMultipurpose.HasCreatedTypeSet)
            {
                DomeShieldMultipurpose.HasAddedToTypeSet = true;
                DomeShieldNodeSet addSet = DomeShieldNodeSet.GetNode(construct);
                base.MainConstruct.NodeSetsRestricted.DictionaryOfAllSets.Add<DomeShieldNodeSet>(addSet);
                AdvLogger.LogInfo("Added DomeShieldNodeSet");
            }
        }
        */
        public override void StateChanged(IBlockStateChange change)
        {
            
            base.StateChanged(change);
            bool isAvailableToConstruct = change.IsAvailableToConstruct;
            if (isAvailableToConstruct)
            {
                /*
                if (!DomeShieldMultipurpose.HasAddedToTypeSet && DomeShieldMultipurpose.HasCreatedTypeSet)
                {
                    DomeShieldMultipurpose.HasAddedToTypeSet = true;
                    base.MainConstruct.NodeSetsRestricted.DictionaryOfAllSets.Add<DomeShieldNodeSet>(nodeSetToAdd);

                }
                */
                base.MainConstruct.NodeSetsRestricted.DictionaryOfAllSets.Get<DomeShieldNodeSet>().AddSender(this);
                TypeStorage.AddHardpoint(this);
                //Use Static Storage instead. You had it working in the old mod.
                base.MainConstruct.SchedulerRestricted.RegisterForFixedUpdate(new Action<ITimeStep>(this.UpdateCharge));
            }
            else
            {
                bool isLostToConstructOrConstructLost = change.IsLostToConstructOrConstructLost;
                if (isLostToConstructOrConstructLost)
                {
                    base.MainConstruct.NodeSetsRestricted.DictionaryOfAllSets.Get<DomeShieldNodeSet>().RemoveSender(this);
                    TypeStorage.AddHardpoint(this);
                    //Use Static Storage instead. You had it working in the old mod.
                    base.MainConstruct.SchedulerRestricted.UnregisterForFixedUpdate(new Action<ITimeStep>(this.UpdateCharge));
                }
            }
            
            //Heyyyyy... Remember this? You should look at your original solution for getting around this. I know you can find it somewhere. It might need updating.
        }
        public bool ChargeCavities(ReloadType reloadType, ResourceStoreCollection resourceStores)
        {
            
            for (int i = 0; i < this.Node.dSCouplers.Count; i++)
            {
                DomeShieldCoupler shieldCoupler = this.Node.dSCouplers[i];
                bool flag = !shieldCoupler.InitialCharge(reloadType, resourceStores);
                if (flag)
                {
                    return false;
                }
            }
            return true;
            
            //Might not need this down the line.
        }
        public override void Secondary(Transform T)
        {
            new DomeShieldSystemUI(this).ActivateGui(GuiActivateType.Stack);
            //A new class to write! Yay.
        }
        protected override void AppendToolTip(ProTip tip)
        {
            
            int num = 500;
            tip.SetSpecial_Name(DomeShieldMultipurpose._locFile.Format("SpecialName", "{0} <color=white>{1}</color>", new object[]
            {
            this.Name,
            base.IdSet.ParseName(false)
            }), DomeShieldMultipurpose._locFile.Get("SpecialDescriptionNoRegulator", "The Dome Shield's version of the LaserMultipurpose block. Connect various dome shield pieces here.", true));
            this.AppendCavityStatsWithFirepower(tip, num);
            tip.SetSpecial_Interaction(DomeShieldMultipurpose._locFile.Get("Tip_QToSeeDomeShieldStats", "Press <<Q>> for dome shield system stats", true));
            
            //This will need to be edited since we do not have a regulator!
        }
        public void AppendCavityStatsWithFirepower(ProTip tip, int width = 400)
        {
            float firePower = this.GetFirePower();
            bool flag = firePower > 0f;
            if (flag)
            {
                tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_DSFirepower", "Firepower: <<{0}>>", new object[] { Rounding.R2(firePower) })));
            }
            this.AppendCavityStats(tip, width, 1f, 1f);
        }
        public void AppendCavityStats(ProTip tip, int width = 400, float damageMultiplier = 1f, float apMultiplier = 1f)
        {
            
            bool flag = true;
            for (int i = 0; i < 1; i++)
            {
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = 0f;
                int num6 = 0;
                int num7 = 0;
                bool flag2 = this.Node != null;
                if (flag2)
                {
                    foreach (DomeShieldCoupler shieldCoupler in this.Node.dSCouplers)
                    {
                        foreach (DomeShieldBeamInfo beamInfo2 in shieldCoupler.dSBeamInfo)
                        {
                            beamInfo2.CalculateEnergyAvailable();
                            num += beamInfo2.MaxEnergy;
                            num2 += beamInfo2.Energy;
                            num6 += beamInfo2.Hardeners;
                            //num3 += beamInfo2.DamagePerSec;
                            num4 += (float)beamInfo2.PowerPerSec;
                            //num5 += beamInfo2.GetHealthThisFrame();
                            num7 += beamInfo2.CubicMetresOfPumping;
                        }
                    }
                }
                bool flag4 = num > 0f;
                if (flag4)
                {
                    flag = false;
                    float num8 = DomeShieldConstants.GetAC(num6, num7, i == 0, num) * apMultiplier;
                    float num9 = num2 / num;
                    string text = DomeShieldMultipurpose._locFile.Format("String_TotalEnergy", "Total energy available: {1}/{2}", new object[]
                    {
                    i,
                    Mathf.Round(num2).ToString(),
                    Mathf.Round(num).ToString()
                    });
                    float num10 = ((i != 0) ? 1f : GameSpeedManager.Instance.gameSpeedFactor);
                    tip.Add(Position.Middle, new ProTipSegment_BarWithTextOnIt(width, text, num9, true));
                    tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_DSPowerNeeded", "See the Dome Shield Controller for detailed power use statistics", new object[] { })));
                    /*
                    tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_SustainedDamage", "Sustained damage/sec:  <<{0}>> (<<{1} intensity>>)", new object[]
                    {
                    Rounding.R0(num3 * damageMultiplier),
                    Rounding.R1(num8)
                    })));
                    */
                    //tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_DamageOfNextShot", "Damage of next shot:    <<{0}>>", new object[] { Rounding.R0(num5 * damageMultiplier / num10) })));
                    //tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_InitialChargeCost", "Initial charge cost:         <<{0}>> materials", new object[] { Rounding.R1(this.StoredEnergyMaterialCost()) })));
                }
            }
            bool flag5 = flag;
            if (flag5)
            {
                tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Get("Tip_NoCavities", "No cavities attached", true)));
            }
            
            //This is a big one. Take this slowly.
        }
        public new static ILocFile _locFile = Loc.GetFile("Dome_Shield_Multipurpose");
    }
}
