using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.newshieldblocksystem
{
    public class DomeShieldFeeler : GenericFeeler<DomeShieldNode>
    {
        public DomeShieldFeeler(DomeShieldNode sender)
        : base(sender)
        {
        }
        public void ResetPartsToZero()
        {
            this.TotalCapacitorSize = 0;
            this.hardeners = 0;
            this.transformers = 0;
            this.rectifiers = 0;
            this.ItemsFlownThrough = 0;
            this.CurrentDSPL = null;
            this.CurrentDSBeam = null;
        }

        public int hardeners = 0;

        public int TotalCapacitorSize = 0;

        public int transformers = 0;

        public int rectifiers = 0;

        public int ItemsFlownThrough = 0;

        public DomeShieldPowerLink? CurrentDSPL;

        public DomeShieldBeamInfo? CurrentDSBeam;


        //If we add equivalents to destabalizers, frequency doublers, or other similar things, we might want to add them here.
        //Done! :)
    }
}
