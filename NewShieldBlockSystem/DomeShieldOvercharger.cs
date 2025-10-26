using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Tips;
using DomeShieldTwo.newshieldblocksystem;
using System;
using System.Collections.Generic;
using System.Text;
namespace DomeShieldTwo.NewShieldBlockSystem
{
    public class DomeShieldOvercharger : DomeShieldCapacitorLinePiece
    {
        static DomeShieldOvercharger()
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
            feeler.Overchargers++;
            feeler.ItemsFlownThrough++;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldOvercharger._locFile.Get("SpecialName", "Dome Shield Overcharger", true), DomeShieldOvercharger._locFile.Get("SpecialDescription", "The Overcharger increases the amount of power used in a power link to increase the effects of all other pieces attatched to that link, even those on different sides of the link. Connect to power links, capacitors or other connected cavity components.", true));
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldOvercharger._locFile.Get("Return_Connect", "Connect to power links, capacitors, or modifiers.", true);
        }


        public DomeShieldOvercharger()
        {
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Overcharger");
    }
}
