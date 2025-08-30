using System;
using System.Collections.Generic;
using System.Text;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldActiveRectifier : DomeShieldCapacitorLinePiece
    {
        static DomeShieldActiveRectifier()
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
            feeler.rectifiers++;
            feeler.ItemsFlownThrough++;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldActiveRectifier._locFile.Get("SpecialName", "Dome Shield Active Rectifier", true), DomeShieldActiveRectifier._locFile.Get("SpecialDescription", "The Active Rectifier increases the efficiency at which the shield draws power from the vehicle's engines, resulting in a reduced power cost overall. Active Rectifiers have no downside, but a high upfront cost. Unlike most parts, they are not directly affected by most shield classes, except for the Regenerator. Connect to power links, capacitors or other connected components.", true));
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldActiveRectifier._locFile.Get("Return_Connect", "Connect to dome shield links, dome shield capacitors, or dome shield modifiers.", true);
        }


        public DomeShieldActiveRectifier()
        {
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_ActiveRectifier");
    }
}
