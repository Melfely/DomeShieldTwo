using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldTransformer : DomeShieldCapacitorLinePiece
    {
        static DomeShieldTransformer()
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
            feeler.transformers++;
            feeler.ItemsFlownThrough++;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldTransformer._locFile.Get("SpecialName", "Dome Shield Transformer", true), DomeShieldTransformer._locFile.Get("SpecialDescription", "Increases the speed in which electricity can flow through the system, increasing regeneration capabilities at the cost of armour class. Connect to cavities or other connected cavity components." + this.GetConnectionText(), true));
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldTransformer._locFile.Get("Return_Connect", "Connect to dome shield couplers, dome shield cavities, or dome shield modifiers.", true);
        }
        public DomeShieldTransformer()
        {
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Transformer");
    }
}
