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
            /*
            int num = 400;
            string card = base.Node.ConnectedCard;
            switch (localCardName)
            {
                case "Pierce":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Consolidator" )));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Upon an impact, rushes all of the surrounding energy of the dome to that exact spot, greatly reducing the effectiveness of piercing weapons (and fragments). However, the shield becomes significantly weaker to explosive weapons, as the much wider area of effect overloads the computer's consolidation programming.")));
                    break;
                case "Thump":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Net")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Changes the dome to have a consistency almost like a net, making hollow point ammunition almost worthless against it. Piercing weapons and Plasma have a much easier time slicing through the weaker structure, however.")));
                    break;
                case "Explosive":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Manipulator")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Allows the dome to intentionally cave from explosions before quickly returning its shape, causing much less energy loss than normal. However, the heat from incendiary weapons also triggers this reaction, causing significantly more damage.")));
                    break;
                case "Energy":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Redirector")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Adjusts the reaction the dome's energy has to energy weapons, making it significantly more resilient to lasers as well as plasma than it already is. Unfortunately, this comes at the cost of the shield's ability to handle kinetic options.")));
                    break;
                case "Fire":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Seperator")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Completely reworks the way the shield controls energy, almost seperating it from the controller block. As a result, incendiary weapaons become unable to overheat and therefore damage the shield. However, electronic attacks will obliterate the shield, since there's no surge protections.")));
                    break;
                case "EMP":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Rescinder")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Grants the dome the ability to rescind energy from a spot instantaneously, rending all electric weapons (including disruptors) nearly useless against the shield as there becomes no electricity to overload. Kinetic weapons love this fact, however, and will decimate the shield. Interestingly, normal energy weapons seem unaffected.")));
                    break;
                case "Particle":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardName:", "Energy Disruptor")));
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_CardDescription", "Specifically tunes the energy around the dome to intercept particle cannon shots, allowing the shield to block them with zero overkill potential. Laser weaponry becomes enhanced against a shield like this, however, and EMP will be even more effective than usual.")));
                    break;
                case "None":
                    tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixCard._locFile.Format("Tip_BrokenCard:", "ERROR: NO CARD TYPE FOUND. Report this to the developer of the mod!!!")));
                    break;
            }
            */
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
