using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldPump : DomeShieldCavityLinePiece
    {
        static DomeShieldPump()
        {
        }
        protected override int ConnectionType
        {
            get
            {
                return 4;
            }
        }
        public static float CubicMeterPowerNeeded
        {
            get
            {
                return DomeShieldConstants.DSPowerPerCavityEnergy * DomeShieldConstants.DSEnergyPumpRatePerCubicMeter;
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            base.FeelerFlowDown(feeler);
            feeler.CubicMetresOfPumping += base.item.SizeInfo.ArrayPositionsUsed;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldPump._locFile.Get("SpecialName", "Dome Shield Pump", true), DomeShieldPump._locFile.Get("SpecialDescription", "Pumps energy in to the dome shield cavity.  Connects to the dome shield cavity." + this.GetConnectionText(), true));
        }
        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddSpec(DomeShieldPump._locFile.Get("TechInfo_DomeShieldEnergy", "Dome shield energy created per second", true), DomeShieldConstants.DSEnergyPumpRatePerCubicMeter * (float)base.item.SizeInfo.ArrayPositionsUsed).AddSpec(LaserPump._locFile.Get("TechInfo_DSPowerNeeded", "Power needed", true), DomeShieldPump.CubicMeterPowerNeeded * (float)base.item.SizeInfo.ArrayPositionsUsed);
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldPump._locFile.Get("Return_Connect", "Connect to a dome shield cavity.", true);
        }

        // Token: 0x06001936 RID: 6454 RVA: 0x0007E483 File Offset: 0x0007C683
        public DomeShieldPump()
        {
        }

        // Token: 0x04000AEB RID: 2795
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Pump");
    }
}
