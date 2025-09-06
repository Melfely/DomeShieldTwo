using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DomeShieldTwo.newshieldblocksystem
{
    public abstract class DomeShieldComponent : BlockComponent<DomeShieldNode, DomeShieldFeeler, DomeShieldConnectedTypeInfo>
    {
        //I assume this is important?
        protected virtual string GetConnectionText()
        {
            return base.LinkedUp ? DomeShieldComponent._locFile.Format("Return_Connected", " Connected to the Dome Shield system. <color=white>{0}</color>", new object[] { base.Node.GoverningBlock.IdSet.ParseName(false) }) : "";
        }
        public override void Secondary(Transform T)
        {
            DomeShieldNode node = base.Node;
            if (node != null)
            {
                node.GoverningBlock.Secondary(T);
            }
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);
            tip.SetSpecial_Name(base.item.DisplayName.ToString(), base.item.Description.ToString());
            bool flag = !base.LinkedUp;
            if (!flag)
            {
                tip.SetSpecial_Interaction(DomeShieldComponent._locFile.Get("Tip_QToSeeDomeShieldStats", "Press <<Q>> for dome shield system stats", true));
            }
        }
        public new static ILocFile _locFile = Loc.GetFile("Dome_Shield_Component");
    }
}
