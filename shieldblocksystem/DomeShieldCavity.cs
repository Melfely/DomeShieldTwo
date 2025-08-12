using BrilliantSkies.Core.Types;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldCavity : DomeShieldCavityLinePiece
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
            this.energyPerDSCavity *= base.item.Code.Variables.GetFloat("CavitySize", 1f);
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
            feeler.energyCapacity += this.energyPerDSCavity;
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(DomeShieldCavity._locFile.Get("SpecialName", "Dome Shield Cavity", true), DomeShieldCavity._locFile.Get("SpecialDescription", "Where the power for the dome shield is generated based on connected pumps, storage, transformers and hardeners.", true));
        }
        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddSpec(DomeShieldCavity._locFile.Get("TechInfo_EnergyStorageCapacity", "Energy storage capacity", true), this.energyPerDSCavity);
        }
        public override string GetConnectionInstructions()
        {
            return DomeShieldCavity._locFile.Get("Return_Connect", " Connect to a Dome Shield coupler or another cavity.", true);
        }
        public new static ILocFile _locFile = Loc.GetFile("DomeShield_Cavity");

        // Token: 0x04000A8B RID: 2699
        public float energyPerDSCavity = DomeShieldConstants.BaseDSCavitySize;
    }
}
