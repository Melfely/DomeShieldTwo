using BrilliantSkies.Blocks.Separator;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Tips;
using DomeShieldTwo.newshieldblocksystem;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.NewShieldBlockSystem
{
    public class DomeShieldMatrixComputer : DomeShieldComponent, IExtraSeparatingBlockData
    {
        static DomeShieldMatrixComputer()
        {
        }
        private bool CompAlreadyExists = false;
        protected override int ConnectionType
        {
            get
            {
                return 3;
            }
        }
        public int PowerPerSec = 500;
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {

            if (base.Node.matrixComputer != null && base.Node.matrixComputer != this)
            {
                CompAlreadyExists = true;
                PowerPerSec = 0;
                return;
            }
            CompAlreadyExists = false;
            base.FeelerFlowDown(feeler);
            base.Node.matrixComputer = this;
            PowerPerSec = (int)base.Node.MaximumEnergy / 500;
            if (PowerPerSec < 500) PowerPerSec = 500;
        }
        
        protected override void AppendToolTip(ProTip tip)
        {

            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldMatrixComputer._locFile.Get("SpecialName", "Dome Shield Matrix Computer", true), DomeShieldMatrixComputer._locFile.Get("SpecialDescription", "A massive structure of wires, processing cards, and Scarlet Dawn technology, able to hack into and modify the very energy the dome shield emits, changing how it reacts to various weapons. Consumes significant engine power to operate.", true));
            if (base.Node == null) return;
            int num = 400;
            tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixComputer._locFile.Format("Tip_PowerUse", "Power use: <<{0}>>", new object[] { this.PowerPerSec })));
            if (CompAlreadyExists) { tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixComputer._locFile.Format("Tip_MultipleComputers", "YOU CANNOT HAVE MORE THAN ONE COMPUTER CONNECTED TO THE DOME SHIELD. PLEASE REMOVE ALL BUT ONE."))); return; }
            if (base.Node.ConnectedCard == "None") tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixComputer._locFile.Format("Tip_NoConnectedCard", "This computer has no card, and therefore is doing nothing but taking up space (and energy). How sad :(")));
            else tip.Add(Position.Middle, new ProTipSegment_Text(num, DomeShieldMatrixComputer._locFile.Format("Tip_ConnectedCard", "This computer is equipped with a <<{0}>> card. Look at the placed card to see what it does.", new object[] { base.Node.ConnectedCard })));

            //Have fun rewriting this one.
            //I did, thank you very much.
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldMatrixComputer._locFile.Get("Return_Connect", "Connect directly to the Dome Shield Controller.", true);
        }
        public DomeShieldMatrixComputer()
        {
        }

        object IExtraSeparatingBlockData.GetExtraData()
        {
            DomeShieldMatrixComputer.SeparatingData separatingData = new DomeShieldMatrixComputer.SeparatingData();
            separatingData.ConnectedCard = new string(base.Node.ConnectedCard);
            return separatingData;
        }
        void IExtraSeparatingBlockData.SetExtraData(object data, Separator.CoordinateTransformation coordinateTransformation)
        {
            DomeShieldMatrixComputer.SeparatingData separatingData = data as DomeShieldMatrixComputer.SeparatingData;
            bool flag = separatingData != null;
            if (flag)
            {
                if (separatingData.ConnectedCard != base.Node.ConnectedCard)
                {
                    base.Node.ConnectedCard = "None";
                }
            }
        }

        private class SeparatingData
        {
            public string ConnectedCard;
        }

        public new static ILocFile _locFile = Loc.GetFile("DomeShield_MatrixComputer");
        //This is gonna be fun to code, isn't it! 
    }
}
