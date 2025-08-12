using BrilliantSkies.Blocks.BreadBoards.GenericGetter;
using BrilliantSkies.DataManagement.Attributes;
using BrilliantSkies.DataManagement.Packages;
using BrilliantSkies.DataManagement.Saving;
using BrilliantSkies.DataManagement.Serialisation;
using BrilliantSkies.DataManagement.Vars;
using System;
using System.Runtime.CompilerServices;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Tips;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldCavityEnergyData : DataPackage
    {
        public DomeShieldCavityEnergyData(uint uniqueId)
        : base(uniqueId)
        {
        }
        public float this[int i]
        {
            get
            {
                float num;
                switch (i)
                {
                    case 0:
                        num = this.DSCavity0.Us;
                        break;
                    case 1:
                        num = this.DSCavity1.Us;
                        break;
                    case 2:
                        num = this.DSCavity2.Us;
                        break;
                    case 3:
                        num = this.DSCavity3.Us;
                        break;
                    case 4:
                        num = this.DSCavity4.Us;
                        break;
                    case 5:
                        num = this.DSCavity5.Us;
                        break;
                    default:
                        num = 0f;
                        break;
                }
                return num;
            }
        }
        [UnreadableVariable]
        [Variable(0U, "", "")]
        public Var<float> DSCavity0 { get; set; } = new Var<float>(0f);
        [UnreadableVariable]
        [Variable(1U, "", "")]
        public Var<float> DSCavity1 { get; set; } = new Var<float>(0f);
        [UnreadableVariable]
        [Variable(2U, "", "")]
        public Var<float> DSCavity2 { get; set; } = new Var<float>(0f);
        [UnreadableVariable]
        [Variable(3U, "", "")]
        public Var<float> DSCavity3 { get; set; } = new Var<float>(0f);
        [UnreadableVariable]
        [Variable(4U, "", "")]
        public Var<float> DSCavity4 { get; set; } = new Var<float>(0f);
        [UnreadableVariable]
        [Variable(5U, "", "")]
        public Var<float> DSCavity5 { get; set; } = new Var<float>(0f);
        public override void Save(ISuperSaver s, SaveCriteria criteria)
        {
            bool flag = this.DSCoupler != null;
            if (flag)
            {
                this.DSCavity0.Us = this.DSCoupler.dSBeamInfo[0].Energy;
                this.DSCavity1.Us = this.DSCoupler.dSBeamInfo[1].Energy;
                this.DSCavity2.Us = this.DSCoupler.dSBeamInfo[2].Energy;
                this.DSCavity3.Us = this.DSCoupler.dSBeamInfo[3].Energy;
                this.DSCavity4.Us = this.DSCoupler.dSBeamInfo[4].Energy;
                this.DSCavity5.Us = this.DSCoupler.dSBeamInfo[5].Energy;
            }
            base.Save(s, criteria);
        }

        public DomeShieldCoupler DSCoupler;
    }
}
