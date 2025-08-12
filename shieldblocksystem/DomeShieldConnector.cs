using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldConnector : DomeShieldComponent
    {
        protected override int ConnectionType
        {
            get
            {
                return 0;
            }
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            bool flag = !base.LinkedUp;
            if (!flag)
            {
                base.Node.GoverningBlock.AppendCavityStatsWithFirepower(tip, 400);
            }
        }
        public DomeShieldConnector()
        {
        }
    }
}
