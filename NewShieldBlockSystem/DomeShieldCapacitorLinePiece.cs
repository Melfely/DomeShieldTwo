using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;
using DomeShieldTwo.NewShieldBlockSystem;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldCapacitorLinePiece : DomeShieldComponent
    {
        static DomeShieldCapacitorLinePiece()
        {
        }
        //Why is this important?
        protected override int ConnectionType
        {
            get
            {
                return 2;
                //What is this???
            }
        }
        
        public DomeShieldPowerLink PowerLink { get; private set; }

        public DomeShieldBeamInfo DomeShieldBeam { get; private set; }

        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            int num = 400;
            bool flag = this.PowerLink != null;
            if (flag)
            {
                bool flag2 = this.PowerLink.ActualEnergy > 0f;
                if (flag2)
                {
                    float num2 = 1f;
                    string text = DomeShieldCapacitorLinePiece._locFile.Format("Tip_BeamEnergy", "Energy in the power link: <<{0}>>", new object[]
                    {
                    Rounding.R0(this.PowerLink.ActualEnergy).ToString()
                    });
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, text));
                }
                int power = this.PowerLink.PowerPerSec;
                if (power > 0)
                {
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { power})));;
                }
                if (this is DomeShieldCapacitor) tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_CapEnergy", "Energy in the system: <<{0}>>", new object[] { this.PowerLink.Node.projector.ShieldStats.CurrentMaxEnergy})));
                else if (this is DomeShieldHardener) tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_HardenerAC", "System AC: <<{0}>>", new object[] { this.PowerLink.Node.projector.ShieldStats.ArmourClass})));
                else if (this is DomeShieldTransformer) tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_TransformerReg", "System Passive Regen: <<{0}>>", new object[] { this.PowerLink.Node.projector.ShieldStats.PassiveRegen })));
                else if (this is DomeShieldOvercharger || this is DomeShieldActiveRectifier) tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_PowerModMult", "Power Link's total power modifier: <<{0}>>", new object[] { this.PowerLink.GetPowerMultiplier() })));
                else if (this is DomeShieldSpoofer) tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_TransformerReg", "System Active Wait Time: <<{0}>>", new object[] { this.PowerLink.Node.projector.ShieldStats.ActualWaitTime })));
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            this.DomeShieldBeam = feeler.CurrentDSBeam;
            this.PowerLink = feeler.CurrentDSPL;
        }

        public DomeShieldCapacitorLinePiece()
        {
        }
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_CavityLinePiece");
    }
}
