using BrilliantSkies.Core.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldNodeSet : GovernedSetManager<DomeShieldNode, DomeShieldFeeler, DomeShieldConnectedTypeInfo, DomeShieldMultipurpose>
    {
        public DomeShieldNodeSet(MainConstruct C)
        : base(C)
        {
            base.HookUpRefreshFunctionToAChangeListener(new Guid("63f06e55-dec1-4f6a-a8d3-cfd0ba89a9fa"), 1f);
            /*
            if (!DomeShieldMultipurpose.HasCreatedTypeSet)
            {
                DomeShieldMultipurpose.HasCreatedTypeSet = true;
                DomeShieldMultipurpose.nodeSetToAdd = this;
                AdvLogger.LogInfo("Created a DomeShieldNodeSet");
            }
            */
        }
        public override DomeShieldFeeler MakeFeeler(DomeShieldNode node)
        {
            return new DomeShieldFeeler(node);
        }

        public override DomeShieldNode MakeNode(AllConstruct construct, DomeShieldMultipurpose block)
        {
            return new DomeShieldNode(construct, block);
        }

        public static DomeShieldNodeSet GetNode(MainConstruct construct)
        {
            return new DomeShieldNodeSet(construct);
        }
    }
}
