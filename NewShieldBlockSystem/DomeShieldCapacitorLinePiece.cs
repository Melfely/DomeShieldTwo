using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;

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
                    string text = DomeShieldCapacitorLinePiece._locFile.Format("Tip_BeamEnergy", "Energy in this line: <<{0}>>", new object[]
                    {
                    Rounding.R0(this.DomeShieldBeam.MaxEnergy).ToString()
                    });
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, text));
                }
                bool flag3 = this.DomeShieldBeam.MaxEnergy > 0f;
                if (flag3)
                {
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCapacitorLinePiece._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { this.PowerLink.PowerPerSec })));;
                }
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            this.DomeShieldBeam = feeler.CurrentDSBeam;
        }

        public DomeShieldCapacitorLinePiece()
        {
        }
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_CavityLinePiece");
    }
}
