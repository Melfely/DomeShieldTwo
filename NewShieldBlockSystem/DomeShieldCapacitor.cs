using BrilliantSkies.Core.Types;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldCapacitor : DomeShieldCapacitorLinePiece
    {
        protected override int ConnectionType
        {
            get
            {
                return 2;
            }
        }
        public override void ItemSet()
        {
            base.ItemSet();
            this.energyPerDSCapacitor *= base.item.Code.Variables.GetFloat("CapacitorSize", 1f);
            this.ThisCapacitorSize = base.item.Code.Variables.GetFloat("CapacitorSize", 1f);
        }
        public override void TagFeelerConnectRules(IConnectionTypes feeler)
        {
            base.TagFeelerConnectRules(feeler);
            bool flag = feeler.LocalOutDirection == Vector3i.forward || feeler.LocalOutDirection == Vector3i.back;
            if (flag)
            {
                feeler.SetConnection(2);
                feeler.SetNoConnection(4);
            }
            else
            {
                feeler.SetNoConnection(2);
                feeler.SetConnection(4);
            }
        }
        public override void FeelerFlowDown(DomeShieldFeeler feeler)
        {
            base.FeelerFlowDown(feeler);
            feeler.ItemsFlownThrough++;
            feeler.TotalCapacitorSize += (int)this.ThisCapacitorSize;
            feeler.TotalEnergyInBeam += (int)this.energyPerDSCapacitor;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldCapacitor._locFile.Get("SpecialName", "Dome Shield Capacitor", true), DomeShieldCapacitor._locFile.Get("SpecialDescription", "Where the power for the dome shield is generated and stored.", true));
        }
        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddSpec(DomeShieldCapacitor._locFile.Get("TechInfo_EnergyStorageCapacity", "Energy storage capacity", true), this.energyPerDSCapacitor);
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldCapacitor._locFile.Get("Return_Connect", "Connect to power links or other capacitors", true);
        }
        public new static ILocFile _locFile = Loc.GetFile("Dome_Shield_Capacitor");

        public float energyPerDSCapacitor = DomeShieldConstants.BaseDSCapacitorSize;
        public float ThisCapacitorSize;
        public int AddCapacitorPlaced;
    }
}
