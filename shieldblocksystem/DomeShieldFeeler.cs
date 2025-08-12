using System;
using System.Collections.Generic;
using System.Text;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldFeeler : GenericFeeler<DomeShieldNode>
    {
        public DomeShieldFeeler(DomeShieldNode sender)
        : base(sender)
        {
        }
        public void ResetPartsToZero()
        {
            this.energyCapacity = 0f;
            this.CubicMetresOfPumping = 0;
            this.hardeners = 0;
            this.transformers = 0;
            this.rectifiers = 0;
            this.CurrentDSCoupler = null;
            this.CurrentDSBeam = null;
        }

        public float energyCapacity = 0f;

        public int hardeners = 0;

        public int CubicMetresOfPumping = 0;

        public int transformers = 0;

        public int rectifiers = 0;

        public DomeShieldCoupler CurrentDSCoupler;

        public DomeShieldBeamInfo CurrentDSBeam;


        //If we add equivalents to destabalizers, frequency doublers, or other similar things, we might want to add them here.
        //Done! :)
    }
}
