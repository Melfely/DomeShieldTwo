using BrilliantSkies.Effects.Regulation;
using DomeShieldTwo.NewShieldBlockSystem;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            this.TotalEnergyInBeam = 0;
            this.hardeners = 0;
            this.transformers = 0;
            this.rectifiers = 0;
            this.ItemsFlownThrough = 0;
            this.Overchargers = 0;
            this.Spoofers = 0;
            this.CurrentDSPL = null;
            this.CurrentDSBeam = null;
        }
        public void ResetComputerParts()
        {
            this.CurrentDSMatrixComputer = null;
            this.CurrentDSMatrixComputer = null;
        }

        public int hardeners = 0;

        public int TotalCapacitorSize = 0;

        public int TotalEnergyInBeam = 0;

        public int transformers = 0;

        public int rectifiers = 0;

        public int Overchargers = 0;

        public int Spoofers = 0;

        public int ItemsFlownThrough = 0;

        public string? ConnectedCard;

        public DomeShieldPowerLink? CurrentDSPL;

        public DomeShieldBeamInfo? CurrentDSBeam;

        public DomeShieldMatrixComputer? CurrentDSMatrixComputer;


        //If we add equivalents to destabalizers, frequency doublers, or other similar things, we might want to add them here.
        //Done! :)
    }
}
