using BrilliantSkies.Core.Logger;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Tips;
using DomeShieldTwo.newshieldblocksystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.NewShieldBlockSystem
{
    public class DomeShieldMatrixCard : DomeShieldComponent
    {
        static DomeShieldMatrixCard()
        {
        }
        protected override int ConnectionType
        {
            get
            {
                return 4;
            }
        }
        private int cardType = 0;
        public string localCardName = "None";
        public override void ItemSet()
        {
            base.ItemSet();
            this.cardType = base.item.Code.Variables.GetInt("CardType");
            localCardName = DetermineCardType();
            //AdvLogger.LogInfo($"We just put on a {localCardName} card?", LogOptions._AlertDevInGame);
        }
        private string DetermineCardType()
        {
            switch (cardType)
            {
                case 1:
                    return "Pierce";
                case 2:
                    return "Thump";
                case 3:
                    return "Explosive";
                case 4:
                    return "Energy";
                case 5:
                    return "Fire";
                case 6:
                    return "EMP";
                case 7:
                    return "Particle";
                case 0:
                    return "None";
                default:
                    return "None";
            }
            //Switch, probably?
        }

        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            base.FeelerFlowDown(feeler);
            //AdvLogger.LogInfo($"Is this happening before ItemSet?", LogOptions._AlertDevInGame);
            feeler.ConnectedCard = localCardName;
            base.Node.ConnectedCard = localCardName;
        }

        public override void PrepForDelete()
        {
            if (base.Node != null) base.Node.ConnectedCard = "None";
            base.PrepForDelete();
        }

        public override void ScrapBlock()
        {
            if (base.Node != null) base.Node.ConnectedCard = "None";
            base.ScrapBlock();

        }

        protected override void AppendToolTip(ProTip tip)
        {

            base.AppendToolTip(tip);
            int num = 400;
            string card = base.Node.ConnectedCard;
            switch (localCardName)
            {
                case "Pierce":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "Incoming Pierce damage * 0.25" )));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Explosive damage * 1.3")));
                    break;
                case "Thump":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "Incoming Thump damage * 0.25")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Pierce damage * 1.2")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect3", "Incoming Plasma damage * 1.2")));
                    break;
                case "Explosive":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "Incoming Explosive damage * 0.25")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Fire damage * 1.3")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect3", "Incoming Laser damage * 1.15")));
                    break;
                case "Energy":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "Incoming Laser damage * 0.5")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Plasma damage * 0.5")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect3", "Incoming Pierce damage * 1.25")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect4", "Incoming Thump damage * 1.25")));
                    break;
                case "Fire":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "Incoming Fire damage * 0.25")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming EMP damage * 1.3")));
                    break;
                case "EMP":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "Incoming EMP damage * 0.2")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Disruption effect * 0.5")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Pierce damage * 1.35")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Thump damage * 1.35")));
                    break;
                case "Particle":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect1", "The shield can now be hit by particle cannons, protecting the craft from them (damage like normal)")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Laser damage * 1.1")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming EMP damage * 1.1")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardEffect2", "Incoming Plasma damage * 1.1")));
                    break;
                case "None":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_BrokenCard:", "ERROR: NO CARD TYPE FOUND. Report this to the developer of the mod!!!")));
                    break;
            }
            //Have fun rewriting this one.
            //I did, thank you very much.
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldMatrixCard._locFile.Get("Return_Connect", "Connect to the card slot on the Matrix Computer.", true);
        }
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_MatrixCard");
    }
}
