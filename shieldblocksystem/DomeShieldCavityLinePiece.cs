using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldCavityLinePiece : DomeShieldComponent
    {
        static DomeShieldCavityLinePiece()
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
        
        public DomeShieldCoupler Coupler { get; private set; }
        /*
    {
        [CompilerGenerated]
        get
        {
            return this.k__BackingFieldDSC;
        }
        [CompilerGenerated]
        private set
        {
            this.k__BackingFieldDSC = value;
        }
    }
        */

        //I don't think we can put off the coupler class much longer, by the way.
        public DomeShieldBeamInfo DomeShieldBeam { get; private set; }
            /*
        {
            [CompilerGenerated]
            get
            {
                return this.k__BackingFieldDSB;
            }
            [CompilerGenerated]
            private set
            {
                this.k__BackingFieldDSB = value;
            }
        }
            */
        protected override void AppendToolTip(ProTip tip)
        {
            
            base.AppendToolTip(tip);
            int num = 400;
            bool flag = this.DomeShieldBeam != null;
            if (flag)
            {
                bool flag2 = this.DomeShieldBeam.MaxEnergy > 0f;
                if (flag2)
                {
                    float num2 = this.DomeShieldBeam.Energy / this.DomeShieldBeam.MaxEnergy;
                    string text = DomeShieldCavityLinePiece._locFile.Format("Tip_BeamEnergy", "Beam energy: {0}/{1}", new object[]
                    {
                    Rounding.R0(this.DomeShieldBeam.Energy).ToString(),
                    Rounding.R0(this.DomeShieldBeam.MaxEnergy).ToString()
                    });
                    tip.Add(Position.Middle, new ProTipSegment_BarWithTextOnIt(num, text, num2, true));
                }
                bool flag3 = this.DomeShieldBeam.MaxEnergy > 0f;
                if (flag3)
                {
                    this.DomeShieldBeam.CalculateEnergyAvailable();
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCavityLinePiece._locFile.Format("Tip_EnergyRecharge", "Energy recharge/sec: <<{0}>>", new object[] { (float)this.DomeShieldBeam.CubicMetresOfPumping * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter })));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCavityLinePiece._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { this.DomeShieldBeam.PowerPerSec })));;
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCavityLinePiece._locFile.Format("Tip_CavityEnergy", "Cavity energy used per shot: <<{0}>>", new object[] { Rounding.R0(this.DomeShieldBeam.EnergyAvailablePerSecond / 40) })));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCavityLinePiece._locFile.Format("Tip_CavityEnergyPercent", "Cavity energy % used per shot: <<{0}%>>", new object[] { Rounding.R2(100f * this.DomeShieldBeam.EnergyFractionPerSec / 4) })));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCavityLinePiece._locFile.Format("Tip_Transformers", "Shield Transformers: <<{0}>>", new object[] { this.DomeShieldBeam.Transformers })));
                }
                tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldCavityLinePiece._locFile.Format("Tip_Hardeners", "Shield Hardeners: <<{0}>>", new object[] { this.DomeShieldBeam.Hardeners })));
            }
            
        }
        //Oh no.
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            //this.Coupler = feeler.CurrentCoupler;
            //Coupler class next.
            this.DomeShieldBeam = feeler.CurrentDSBeam;
        }

        public DomeShieldCavityLinePiece()
        {
        }
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_CavityLinePiece");
    }
}
