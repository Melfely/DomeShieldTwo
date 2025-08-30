using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldSpoofer : DomeShieldCapacitorLinePiece
    {
        static DomeShieldSpoofer()
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
            feeler.Spoofers++;
            feeler.ItemsFlownThrough++;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldSpoofer._locFile.Get("SpecialName", "Dome Shield Spoofer", true), DomeShieldSpoofer._locFile.Get("SpecialDescription", "A unique modifier that tricks the system into believing there are three less components attatched than there really are, allowing Active Regeneration to begin quicker. Unlike other modifers, this one consumes engine power to work. Connect to couplers, cavities or other connected cavity components.", true));
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldSpoofer._locFile.Get("Return_Connect", "Connect to dome shield couplers, dome shield cavities, or dome shield modifiers.", true);
        }

        public override BlockTechInfo GetTechInfo()
        {
            //We definitely need to adjust this...
            return new BlockTechInfo().AddStatement(DomeShieldSpoofer._locFile.Format("TechInfo_Spoofer", "Reduces the block count for Active Regen calculations by 3 (includes this block)"));
        }

        public DomeShieldSpoofer()
        {
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Spoofer");
    }
}