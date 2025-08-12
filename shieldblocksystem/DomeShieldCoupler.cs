using BrilliantSkies.Blocks.Separator;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using System.Diagnostics;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldCoupler : DomeShieldComponent, IExtraSeparatingBlockData
    {
        protected override int ConnectionType
        {
            get
            {
                return 1;
            }
        }
        public DomeShieldCavityEnergyData StoredDSEnergies { get; set; } = new DomeShieldCavityEnergyData(3313352U);
        public override bool NeedsInspectFeelerAfterDirection
        {
            get
            {
                return true;
                //What does this do?
            }
        }
        //Skipped "NbQSwitches"
        public override void ComponentStart()
        {
            this.StoredDSEnergies.DSCoupler = this;
            for (int i = 0; i < 6; i++)
            {
                this.dSBeamInfo[i] = new DomeShieldBeamInfo();
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            base.Node.dSCouplers.Add(this);
            feeler.CurrentDSCoupler = this;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldCoupler._locFile.Get("SpecialName", "Dome Shield Coupler", true), DomeShieldCoupler._locFile.Get("SpecialDescription", "Connects dome shield cavities to the dome shield multipurpose block.", true));
            int num = 400;
            float num2 = 0f;
            float num3 = 0f;
            float num4 = 0f;
            int num5 = 0;
            int num99 = 0;
            for (int i = 0; i < 6; i++)
            {
                DomeShieldBeamInfo beamInfo = this.dSBeamInfo[i];
                bool flag = beamInfo.MaxEnergy > 0f;
                if (flag)
                {
                    float num6 = beamInfo.Energy / beamInfo.MaxEnergy;
                    string text = DomeShieldCoupler._locFile.Format("String_Energy", "Beam {0} energy: {1}/{2}", new object[]
                    {
                    i.ToString(),
                    Mathf.Round(beamInfo.Energy).ToString(),
                    Mathf.Round(beamInfo.MaxEnergy).ToString()
                    });
                    beamInfo.CalculateEnergyAvailable();
                    tip.Add(Position.Middle, new ProTipSegment_BarWithTextOnIt(num, text, num6, true));
                    num2 += (float)beamInfo.PowerPerSec;
                    /*
                    num3 += beamInfo.DamagePerSec;
                    num4 += beamInfo.GetDamageThisFrame();
                    */
                    num5 += beamInfo.Hardeners;
                    num99 += beamInfo.Transformers;
                }
            }
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCoupler._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { num2 })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCoupler._locFile.Format("Tip_Hardeners", "Hardeners: <<{0}>>", new object[] { num5 })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCoupler._locFile.Format("Tip_Transformers", "Transformers: <<{0}>>", new object[] { num99 })));

            //Have fun rewriting this one.
            //I did, thank you very much.
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldCoupler._locFile.Get("Return_Connect", " Connect to a multipurpose dome shield block or another coupler.", true);
        }
        public override void InspectFeelerBeforeDirection(DomeShieldFeeler feeler, Vector3i localDirection, int index, int positionIndex)
        {
            feeler.CurrentDSCoupler = this;
            feeler.CurrentDSBeam = this.dSBeamInfo[index];
            feeler.CurrentDSBeam.FeelerVersion = feeler.ID;
        }
        public override void InspectFeelerAfterDirection(DomeShieldFeeler feeler, Vector3i localDirection, int index)
        {
            DomeShieldBeamInfo dSBeamInfo = this.dSBeamInfo[index];
            base.Node.MaximumEnergy += dSBeamInfo.SetParts(feeler.energyCapacity, feeler.hardeners, feeler.CubicMetresOfPumping, feeler.transformers, feeler.rectifiers);
            feeler.ResetPartsToZero();
            //This will need coming back to because of the replacement of FDs and DEs
            //We are calling FDs 'Shield Hardeners" and DEs "Shield Transformers"
        }
        public override void SeeFeelerAfterAllConnections(DomeShieldFeeler Feeler)
        {
            for (int i = 0; i < 6; i++)
            {
                bool flag = this.dSBeamInfo[i].FeelerVersion != Feeler.ID;
                if (flag)
                {
                    this.dSBeamInfo[i].Reset();
                }
            }
        }
        //Skipped "SeeConnections" as it seems to only be about qSwitches, which we don't use.
        public bool InitialCharge(ReloadType reloadType, ResourceStoreCollection resourceStores)
        {
            for (int i = 0; i < this.dSBeamInfo.Length; i++)
            {
                if (reloadType > ReloadType.UseMaterial)
                {
                    this.dSBeamInfo[i].Energy = Mathf.Clamp(this.StoredDSEnergies[i], 0f, this.dSBeamInfo[i].MaxEnergy);
                    this.dSBeamInfo[i].HadInitialCharge = true;
                }
                else
                {
                    float spawnEnergy = this.dSBeamInfo[i].GetSpawnEnergy();
                    bool flag = spawnEnergy == 0f;
                    if (flag)
                    {
                        this.dSBeamInfo[i].Energy = 0f;
                        this.dSBeamInfo[i].HadInitialCharge = true;
                    }
                    else
                    {
                        float num = spawnEnergy * DomeShieldConstants.DSPowerPerCavityEnergy / FuelConstants.BaseFuelToPower / GameConstants.MaterialToFuelRatio;
                        float num2 = resourceStores.TakeMaterialsWithTotalCheck(num, false);
                        this.dSBeamInfo[i].Energy = Mathf.Clamp(spawnEnergy * num2 / num, 0f, this.dSBeamInfo[i].MaxEnergy);
                        this.dSBeamInfo[i].HadInitialCharge = true;
                        bool flag2 = num2 != num;
                        if (flag2)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
            //This might not need any editing?
        }
        object IExtraSeparatingBlockData.GetExtraData()
        {
            DomeShieldCoupler.SeparatingData separatingData = new DomeShieldCoupler.SeparatingData();
            separatingData.DSBeamEnergy = new float[this.dSBeamInfo.Length];
            for (int i = 0; i < this.dSBeamInfo.Length; i++)
            {
                separatingData.DSBeamEnergy[i] = this.dSBeamInfo[i].Energy;
            }
            return separatingData;
        }
        void IExtraSeparatingBlockData.SetExtraData(object data, Separator.CoordinateTransformation coordinateTransformation)
        {
            DomeShieldCoupler.SeparatingData separatingData = data as DomeShieldCoupler.SeparatingData;
            bool flag = separatingData != null;
            if (flag)
            {
                for (int i = 0; i < this.dSBeamInfo.Length; i++)
                {
                    float num = separatingData.DSBeamEnergy[i];
                    float maxEnergy = this.dSBeamInfo[i].MaxEnergy;
                    bool flag2 = Math.Abs(num - maxEnergy) > 0.001f;
                    if (flag2)
                    {
                        this.dSBeamInfo[i].Energy = 0f;
                    }
                    else
                    {
                        this.dSBeamInfo[i].Energy = Mathf.Clamp(num, 0f, maxEnergy);
                    }
                }
            }
        }
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Coupler");

        public DomeShieldBeamInfo[] dSBeamInfo = new DomeShieldBeamInfo[6];
        private class SeparatingData
        {
            public float[] DSBeamEnergy;
        }
    }
}
