using BrilliantSkies.Blocks.Separator;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

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
            feeler.ItemsFlownThrough++;
        }
        public int PowerPerSec
        {
            get
            {
                return CalculateActualEnergyAndPowerModifier();
            }
            //We can fine tune this number
        }

        public int CalculateActualEnergyAndPowerModifier()
        {
            int hardeners = 0;
            float eng = 0;
            int transformers = 0;
            int overchargers = 0;
            int ARs = 0;
            int spoofers = 0;
            float totalCapSize = 0;
            int LBs = 0;
            for (int i = 0; i < 6; i++)
            {
                DomeShieldBeamInfo beamInfo = this.dSBeamInfo[i];
                /*
                num3 += beamInfo.DamagePerSec;
                num4 += beamInfo.GetDamageThisFrame();
                */
                eng += beamInfo.MaxEnergy;
                hardeners += beamInfo.Hardeners;
                transformers += beamInfo.Transformers;
                overchargers += beamInfo.Overchargers;
                ARs += beamInfo.Rectifiers;
                spoofers += beamInfo.Spoofers;
                LBs += beamInfo.TotalBlocks;
                totalCapSize += beamInfo.TotalCapacitorSize;
            }
            OverchargersOnLink = overchargers;
            RectifiersOnLink = ARs;
            float mult = GetPowerMultiplier();
            float num = totalCapSize * (20 * mult);
            num += (spoofers * (125 * mult)); //original was 300 * mult, we've halved it to 150 for now.
            int num2 = Rounding.FloatToInt(num);
            ActualEnergy = eng * mult;
            return num2;
        }
        public float GetPowerMultiplier()
        {
            float multiplier = ((1 * GetOverchargerPowerMultiplier()) * GetRegulatorPowerMultiplier());
            return multiplier;
        }

        private float GetOverchargerPowerMultiplier()
        {
            float baseIncrease = (float)OverchargersOnLink * 1.1f;
            float penalty = ((baseIncrease * 1.83f) - OverchargersOnLink) - 1;
            float adjustedIncrease = baseIncrease - penalty;
            if (OverchargersOnLink == 0) adjustedIncrease = 1;
            if (OverchargersOnLink == 1) adjustedIncrease = 1.1f;
            return adjustedIncrease;
            //return 1f + ((overchargers * 1.1f) * (0.1f - ((float)Math.Pow(100 / overchargers, 0.1))));
            //What on earth was I doing with this formula LMAO
        }
        private float GetRegulatorPowerMultiplier()
        {
            float baseReduction = (RectifiersOnLink * 0.05f);
            float adjustedReduction = baseReduction - (RectifiersOnLink * 0.006f);
            float finalReduction = 1f - adjustedReduction;
            if (RectifiersOnLink == 1) finalReduction = 0.95f;
            if (RectifiersOnLink == 0) finalReduction = 1f;
            return finalReduction;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldPowerLink._locFile.Get("SpecialName", "Dome Shield Power Link", true), DomeShieldPowerLink._locFile.Get("SpecialDescription", "Connects dome shield cavities and modifiers to the controller block. Components attatched to this link will use Engine power.", true));
            int num = 400;
            int hardeners = 0;
            int transformers = 0;
            int overchargers = 0;
            int ARs = 0;
            int spoofers = 0;
            int LBs = 0;
            for (int i = 0; i < 6; i++)
            {
                DomeShieldBeamInfo beamInfo = this.dSBeamInfo[i];
                bool flag = beamInfo.MaxEnergy > 0f;
                if (flag)
                {
                    string text = DomeShieldPowerLink._locFile.Format("String_Energy", "Beam {0} energy: {1}", new object[]
                    {
                    i.ToString(),
                    Mathf.Round(beamInfo.MaxEnergy * GetPowerMultiplier()).ToString()
                    });
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, text));
                }
                else if (beamInfo.Spoofers > 0 || beamInfo.Rectifiers > 0 || beamInfo.Hardeners > 0 || beamInfo.Overchargers > 0 || beamInfo.Transformers > 0)
                {
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_BeamWithModsNoCaps", "beam {0} has no capacitors but has modifiers attached", new object[] { i.ToString() })));
                }
                /*
                num3 += beamInfo.DamagePerSec;
                num4 += beamInfo.GetDamageThisFrame();
                */
                hardeners += beamInfo.Hardeners;
                transformers += beamInfo.Transformers;
                overchargers += beamInfo.Overchargers;
                ARs += beamInfo.Rectifiers;
                spoofers += beamInfo.Spoofers;
                LBs += beamInfo.TotalBlocks;
            }
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { PowerPerSec })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_Hardeners", "Hardeners: <<{0}>>", new object[] { hardeners })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_Transformers", "Transformers: <<{0}>>", new object[] { transformers })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_Overchargers", "Overchargers: <<{0}>>", new object[] { overchargers })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_ARs", "Active Rectifiers: <<{0}>>", new object[] { ARs })));
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_Overchargers", "Spoofers: <<{0}>>", new object[] { spoofers })));
            if (LBs / 3 < spoofers) tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldPowerLink._locFile.Format("Tip_too many spoofs", "<color=yellow>You have more spoofers connected to this power link than blocks they would effect, wasting energy!</color>")));


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
            LocalEnergyBeforeMods += dSBeamInfo.SetParts(feeler);
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
        public float LocalEnergyBeforeMods;
        public int OverchargersOnLink;
        public int RectifiersOnLink;
        public float ActualEnergy;
    }
}
