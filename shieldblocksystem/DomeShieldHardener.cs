using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldHardener : DomeShieldCavityLinePiece
    {
        static DomeShieldHardener()
        {
        }
        protected override int ConnectionType
        {
            get
            {
                return 2;
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            base.FeelerFlowDown(feeler);
            feeler.hardeners++;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldHardener._locFile.Get("SpecialName", "Dome Shield Hardener", true), DomeShieldHardener._locFile.Get("SpecialDescription", "Increases the density of the shield, which increases the armour class. Negatively impacts regeneration speed as the flow of electricity is affected by the density.  Connect to couplers, cavities or other connected cavity components.", true));
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldHardener._locFile.Get("Return_Connect", "Connect to dome shield couplers, dome shield cavities, or dome shield modifiers.", true);
        }

        public override BlockTechInfo GetTechInfo()
        {
            //We definitely need to adjust this...
            return new BlockTechInfo().AddStatement(DomeShieldHardener._locFile.Format("TechInfo_ACModifier", "AC modifier: pump volume + total energy capacity/{0}", new object[] { DomeShieldConstants.DSPumpCavityCapacityEquivalent })).AddStatement(DomeShieldHardener._locFile.Format("TechInfo_Increases", "Armour class increase for each hardener: {0}/intensity modifier for continuous lasers.", new object[]
            {
            DomeShieldConstants.ACPerHardener
            //We need to adjust this.
            }));
        }

        public DomeShieldHardener()
        {
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Hardener");
    }
}
