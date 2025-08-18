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

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldPowerLink : DomeShieldComponent, IExtraSeparatingBlockData
    {
        protected override int ConnectionType
        {
            get
            {
                return 1;
            }
        }
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
            for (int i = 0; i < 6; i++)
            {
                this.dSBeamInfo[i] = new DomeShieldBeamInfo();
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            base.Node.dSPLs.Add(this);
            feeler.CurrentDSPL = this;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldPowerLink._locFile.Get("SpecialName", "Dome Shield Power Link", true), DomeShieldPowerLink._locFile.Get("SpecialDescription", "Connects dome shield cavities and modifiers to the Hardpoint. Components attatched to this link will use Engine power.", true));
            int num = 400;
            float num2 = 0f;
            int num5 = 0;
            int num99 = 0;
            for (int i = 0; i < 6; i++)
            {
                DomeShieldBeamInfo beamInfo = this.dSBeamInfo[i];
                bool flag = beamInfo.MaxEnergy > 0f;
                if (flag)
                {
                    string text = DomeShieldPowerLink._locFile.Format("String_Energy", "Beam {0} energy: {1}", new object[]
                    {
                    i.ToString(),
                    Mathf.Round(beamInfo.MaxEnergy).ToString()
                    });
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, text));
                    num2 += (float)beamInfo.PowerPerSec;
                    /*
                    num3 += beamInfo.DamagePerSec;
                    num4 += beamInfo.GetDamageThisFrame();
                    */
                    num5 += beamInfo.Hardeners;
                    num99 += beamInfo.Transformers;
                }
            }
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { num2 })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_Hardeners", "Hardeners: <<{0}>>", new object[] { num5 })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_Transformers", "Transformers: <<{0}>>", new object[] { num99 })));

            //Have fun rewriting this one.
            //I did, thank you very much.
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldPowerLink._locFile.Get("Return_Connect", " Connect to a multipurpose dome shield block or another power link.", true);
        }
        public override void InspectFeelerBeforeDirection(DomeShieldFeeler feeler, Vector3i localDirection, int index, int positionIndex)
        {
            feeler.CurrentDSPL = this;
            feeler.CurrentDSBeam = this.dSBeamInfo[index];
            feeler.CurrentDSBeam.FeelerVersion = feeler.ID;
        }
        public override void InspectFeelerAfterDirection(DomeShieldFeeler feeler, Vector3i localDirection, int index)
        {
            DomeShieldBeamInfo dSBeamInfo = this.dSBeamInfo[index];
            base.Node.MaximumEnergy += dSBeamInfo.SetParts(feeler.TotalCapacitorSize, feeler.hardeners, feeler.transformers, feeler.rectifiers);
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
        
        object IExtraSeparatingBlockData.GetExtraData()
        {
            DomeShieldPowerLink.SeparatingData separatingData = new DomeShieldPowerLink.SeparatingData();
            separatingData.DSBeamEnergy = new float[this.dSBeamInfo.Length];
            for (int i = 0; i < this.dSBeamInfo.Length; i++)
            {
                separatingData.DSBeamEnergy[i] = this.dSBeamInfo[i].MaxEnergy;
            }
            return separatingData;
        }
        void IExtraSeparatingBlockData.SetExtraData(object data, Separator.CoordinateTransformation coordinateTransformation)
        {
            DomeShieldPowerLink.SeparatingData separatingData = data as DomeShieldPowerLink.SeparatingData;
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
                        this.dSBeamInfo[i].MaxEnergy = 0f;
                    }
                    else
                    {
                        this.dSBeamInfo[i].MaxEnergy = Mathf.Clamp(num, 0f, maxEnergy);
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
