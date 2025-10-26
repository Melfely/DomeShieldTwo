using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldHardener : DomeShieldCapacitorLinePiece
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
            feeler.ItemsFlownThrough++;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldHardener._locFile.Get("SpecialName", "Dome Shield Hardener", true), DomeShieldHardener._locFile.Get("SpecialDescription", "Increases the effect of diverting energy towards Armor", true));
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldHardener._locFile.Get("Return_Connect", "Connect to power links, capacitors, or modifiers.", true);
        }

        public override BlockTechInfo GetTechInfo()
        {
            //We definitely need to adjust this...
            return new BlockTechInfo().AddStatement(DomeShieldHardener._locFile.Format("TechInfo_ACModifier", "AC modifier: 3 idk"));
        }

        public DomeShieldHardener()
        {
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Hardener");
    }
}
