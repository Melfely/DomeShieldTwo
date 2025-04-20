using System;
using UnityEngine;
using AdvShields;
using System.Runtime;
using System.Collections;
using BrilliantSkies.Blocks.Lasers;
using BrilliantSkies.Core.JsonPlus.Converters;
using BrilliantSkies.Core.Units;
using BrilliantSkies.Modding.Types;
using AdvShields.Models;

namespace AdvShields
{
    public class AdvShieldStatus
    {
        private AdvShieldProjector controller;

        public AdvShieldHandler ShieldHandler { get; set; }

        private float maxEnergyFactor = 1f;
        private float armorClassFactor = 1f;
        private float passiveRegenFactor = 0f;

        public AdvShieldData ShieldData { get; set; } = new AdvShieldData(0u);

        public float Energy { get; private set; }

        public float MaxEnergy { get; private set; }

        public float Doublers { get; private set; }

        public float Destabilizers { get; private set; }

        public float EnergyBeforeMods { get; private set; }

        public float HealthLossFromMods {  get; private set; }

        public float ArmorClassDifference { get; private set; }

        public float PassiveRegenDifference { get; private set; }

        public float HealthFromPowerScale {  get; private set; }

        public float ACFromPowerScale { get; private set; }

        public float PRFromPowerScale { get; private set; }

        public float WTFromPowerScale { get; private set; }

        public float ArmorClass { get; private set; }

        public float ShieldEmpSusceptibility { get; set; }

        public float ShieldEmpResistivity { get; set; }

        public float ShieldEmpDamageFactor {  get; set; }

        public float BaseShieldArmorClass { get; private set; }

        public float BaseHealthModifier { get; private set; }

        public float WaitTimeModifier { get; private set; }

        public float PowerScaleACModifier { get; private set; }

        public float PowerScaleHealthModifier { get; private set; }

        public float PowerScalePRModifier { get; private set; }

        public float PowerScale { get; private set; }

        public float PowerScaleWaitTimeModifier { get; private set; }

        public float PowerScaleHealingEfficiencyModifier { get; private set; } //temporarily scrapped, might add. This would affect ACTIVE regen if implemented

        public float PassiveOnOffACChange { get; private set; }

        public float PassiveOnOffHChange { get; private set; }

        public string ShieldType { get; private set; }

        public float PassiveRegen { get; private set; }

        public float DoublersACFactor { get; private set; }

        public float DestbalisersACFactor { get; private set; }

        public float PassiveRegenModifier { get; private set; }

        public float PassiveRegenBeforeScaling { get; private set; }

        public float MinimumPassiveRegen { get; private set; }

        /*private static float GetAp(int doublers, int pumps, bool isContinuous, float totalEnergyCapacity)
        {
            float num = totalEnergyCapacity / 2500f;
            float b = (float)pumps + num;
            float num2 = 40f + (float)doublers * 100f / Mathf.Max(1f, b);
            return isContinuous ? (num2 * 1.5f) : num2;

            /*ORIGINAL: float num = totalEnergyCapacity / LaserConstants.PumpCavityCapacityEquivalent(10000f);
            float b = (float)pumps + num;
            float num2 = LaserConstants.PulsedBaseAp(40) + (float)doublers * LaserConstants.PulsedApPerDoubler(100f) / Mathf.Max(1f, b);
            return isContinuous ? (num2 * LaserConstants.ContinuousApMultiplier[1.5f]) : num2;*/
        //}*/
        //The game already knows doublers, pumps, isContinuous, and totalEnergyCapacity. It divides totalEnergy Capacity by LaserConstants.PumpCavityCapacityEquivalent (10000) to get "num". It then adds the meters of pumps to "num" to get "b". Then, the game adds LaserConstants.PulsedBaseAp (40) to the amount of frequency doublers, then multiplies this by LaserConstants.PulsedApPerDoubler (100), then divides this by Math.Max(1f, b), meaning it will be divided by 1 or by "b", whichever is higher, to get "num2" Finally, ifContinuous will apply a LaserConstants.ContinuousApMultiplier (1.5) if the laser is continuous. If not, it won't. A?x:y means that if A is true, x happens, otherwise y happens
        public float WaitTime { get; set; }
           
        private bool IsContinuous { get; set; }

        public float DoublersForUI { get; set; }

        public float DestabilizersForUI { get; set; }

        public AdvShieldStatus(AdvShieldProjector controller, float maxEnergyFactor, float armorClassFactor, float passiveRegenFactor)
        {
            this.controller = controller;
            this.maxEnergyFactor = maxEnergyFactor;
            this.armorClassFactor = armorClassFactor;
            this.passiveRegenFactor = passiveRegenFactor;
            Update();
            //all of this can be seen in the lowest tab in the item selection in mods->create->AdvancedShields->Advanced Shield Controller. This is effectively so that the 1x1x1 dome shield can have worse stats.
        }

        public void Update()
        {
            Energy = 0;
            ArmorClass = 1;



            LaserNode laserNode = controller.ConnectLaserNode;

            if (laserNode != null)
            {
                int doublers = 0;
                int pumps = 0;
                //int totalEnergyCapacity = 0;
                int destabilisers = 0;
                int allQSwitches = 0;
                PowerScale = this.controller.ShieldData.ExcessDrive;
                foreach (LaserCoupler laserCoupler in laserNode.couplers)
                {
                    allQSwitches = laserCoupler.NbQSwitches;
                    foreach (BeamInfo beamInfo in laserCoupler.beamInfo)
                    {
                        doublers += beamInfo.FrequencyDoublers;
                        pumps += beamInfo.CubicMetresOfPumping;
                        destabilisers += beamInfo.Destabilisers; //these 3 are grabbing the amount of doublers, pumps, and destabs in the laser system. Pumps are no longer needed and will probably be removed eventually.
                        Doublers = doublers;
                        Destabilizers = destabilisers; //these 2 are effectively converting int to float
                    }
                }

                if (controller.ShieldData.ShieldClass == enumShieldClassSelection.QH)
                {
                    WaitTimeModifier = 40;
                    BaseShieldArmorClass = 15;
                    BaseHealthModifier = 1;
                    PassiveRegenModifier = 0.95f;
                    IsContinuous = true;
                    ShieldEmpSusceptibility = 1f;
                    ShieldEmpResistivity = 0f;
                    ShieldEmpDamageFactor = 2f; //base is 2 so that EMP doesn't deal less damage than other warheads.
                    PowerScaleHealthModifier = (float)Math.Round(((PowerScale / 18f) + 0.94444f), 2); //Max increase = 1.5x. The reason all of these look like this (Powerscale / num + num) is so that there is no change at 1x (default) shield drive.
                    PowerScaleACModifier = (float)Math.Round(1 - ((PowerScale / 3) + 0.66667f), 2); //Max increase = +3. Notice, unlike health and pr, that this AC modifier is ADDED, not MULTIPLIED
                    PowerScalePRModifier = (float)Math.Round(((PowerScale / 18f) + 0.94444f), 2); //Max increase = 1.5x
                    PowerScaleWaitTimeModifier = (float)Math.Round(1 - ((PowerScale / 2.25f) + 0.5556f), 2); //Max decrease = 4 seconds, meaning that the new wait time is 4 (48-40-4=4).
                    ShieldType = "QUICKHEAL"; //<^these are sent to AdvShieldProjector to show up in the UI
                    //QuickHeal
                }
                else if (controller.ShieldData.ShieldClass == enumShieldClassSelection.AC)
                {
                    WaitTimeModifier = 2;
                    BaseShieldArmorClass = 50;
                    BaseHealthModifier = 0.95f;
                    PassiveRegenModifier = 0.5f;
                    IsContinuous = false;
                    ShieldEmpSusceptibility = 1f;
                    ShieldEmpResistivity = 0f;
                    ShieldEmpDamageFactor = 2f; //base is 2 so that EMP doesn't deal less damage than other warheads.
                    PowerScaleHealthModifier = (float)Math.Round(((PowerScale / 9f) + 0.88888f), 2); //Max increase = 2x
                    PowerScaleACModifier = (float)Math.Round((PowerScale * 1.11) - 1.11f, 2); //Max increase = +10
                    PowerScalePRModifier = (float)Math.Round(((PowerScale / 36f) + 0.975f), 2); //Max increase = 1.25x
                    PowerScaleWaitTimeModifier = PowerScale - 1; //Max increase (THIS IS A DEBUFF) = +9
                    ShieldType = "ARMORED"; //<^these are sent to AdvShieldProjector to show up in the UI
                    //Armoured
                }

                else if (controller.ShieldData.ShieldClass == enumShieldClassSelection.HE)
                {
                    WaitTimeModifier = 5;
                    BaseShieldArmorClass = 15;
                    BaseHealthModifier = 1.5f;
                    PassiveRegenModifier = 0.6f;
                    IsContinuous = false;
                    ShieldEmpSusceptibility = 1f;
                    ShieldEmpResistivity = 0f;
                    ShieldEmpDamageFactor = 1.75f; //base is 2 so that EMP doesn't deal less damage than other warheads.
                    PowerScaleHealthModifier = (float)Math.Round(((PowerScale / 4.5f) + 0.77777f), 2); //Max increase = 3x
                    PowerScaleACModifier = (float)Math.Round(((PowerScale / 1.8) + 0.55555f), 2); //Max increase = +5
                    PowerScalePRModifier = (float)Math.Round(((PowerScale / 27f) + 0.9629f), 2); //Max increase = 1.33x
                    PowerScaleWaitTimeModifier = (PowerScale / 2) - 0.5f; //Max increase (THIS IS A DEBUFF) = +5
                    ShieldType = "HEALTHY"; //<^these are sent to AdvShieldProjector to show up in the UI
                    //Healthy
                }

                else if (controller.ShieldData.ShieldClass == enumShieldClassSelection.GEN)
                {
                    WaitTimeModifier = 15;
                    BaseShieldArmorClass = 25;
                    BaseHealthModifier = 1.1f;
                    PassiveRegenModifier = 1f;
                    IsContinuous = false;
                    ShieldEmpSusceptibility = 1f;
                    ShieldEmpResistivity = 0f;
                    ShieldEmpDamageFactor = 2f; //base is 2 so that EMP doesn't deal less damage than other warheads.
                    PowerScaleHealthModifier = (float)Math.Round(((PowerScale / 9f) + 0.88888f), 2); //Max increase = 2x
                    PowerScaleACModifier = (float)Math.Round(((PowerScale / 1.8) + 0.55555f), 2); //Max increase = +5
                    PowerScalePRModifier = (float)Math.Round(((PowerScale / 18f) + 0.94444f), 2); //Max increase = 1.5x
                    PowerScaleWaitTimeModifier = (float)(Math.Round(PowerScale / 3 - 0.33f, 2)); //Max increase (THIS IS A DEBUFF) = +3
                    ShieldType = "GENERALIST"; //<^these are sent to AdvShieldProjector to show up in the UI
                    //Generalist
                }
                else if (controller.ShieldData.ShieldClass == enumShieldClassSelection.REG)
                {
                    WaitTimeModifier = 15;
                    BaseShieldArmorClass = 12;
                    BaseHealthModifier = 0.8f;
                    PassiveRegenModifier = 2f;
                    IsContinuous = false;
                    ShieldEmpSusceptibility = 1f;
                    ShieldEmpResistivity = 0f;
                    ShieldEmpDamageFactor = 3f; //base is 2 so that EMP doesn't deal less damage than other warheads.
                    PowerScaleHealthModifier = (float)Math.Round(((PowerScale / 18f) + 0.94444f), 2); //Max increase = 1.5x
                    PowerScaleACModifier = (float)Math.Round(((PowerScale / 4) + 0.75f), 2); //Max increase = +2.25
                    PowerScalePRModifier = (float)Math.Round(((PowerScale / 9f) + 0.8888f), 2); //Max increase = 2x
                    PowerScaleWaitTimeModifier = PowerScale / 4 - 0.25f; //Max increase (THIS IS A DEBUFF) = +2.25
                    ShieldType = "REGENERATOR"; //<^these are sent to AdvShieldProjector to show up in the UI
                    //Regenerator
                }
                else
                {
                    WaitTimeModifier = 1;
                    BaseShieldArmorClass = 2;
                    BaseHealthModifier = 1f;
                    PassiveRegenModifier = 1f;
                    IsContinuous = false;
                    ShieldEmpSusceptibility = 1f;
                    ShieldEmpResistivity = 0f;
                    ShieldEmpDamageFactor = 4f;
                    PowerScaleHealthModifier = (float)Math.Round(((PowerScale / 18f) + 0.94444f), 2);
                    PowerScaleACModifier = (float)Math.Round(((PowerScale / 4) + 0.75f), 2);
                    PowerScalePRModifier = (float)Math.Round(((PowerScale / 9f) + 0.8888f), 2);
                    PowerScaleWaitTimeModifier = PowerScale / 4 - 0.25f;
                    ShieldType = "ERROR: NO SHIELD CLASS SELECTED. PLEASE REPORT THIS ERROR";
                    //Error
                }
                DoublersACFactor = (doublers) +0.01f;
                if (DoublersACFactor > 1.3f)
                {
                    DoublersACFactor = (float)Math.Pow(DoublersACFactor = (doublers) + 0.01f, 0.90f)+0.1f;
                    if (DoublersACFactor < 1.3f)
                    {
                        DoublersACFactor = 1.3f;
                    }
                }
                if (DoublersACFactor > 10f)
                {
                    DoublersACFactor = (float)Math.Pow(DoublersACFactor = (doublers) + 0.01f, 0.60f) + 4.1f;
                    if (DoublersACFactor < 10f)
                    {
                        DoublersACFactor = 10f;
                    }
                }
                DestbalisersACFactor = (destabilisers / 3f) + 0.005f;
                if (DestbalisersACFactor > 1.29f)
                {
                    DestbalisersACFactor = (float)Math.Pow(DestbalisersACFactor = (destabilisers / 3f) + 0.01f, 0.85f) + 1f;
                    if (DestbalisersACFactor < 1.29f)
                    {
                        DestbalisersACFactor = 1.29f;
                    }
                }
                if (DestbalisersACFactor > 10f)
                {
                    DestbalisersACFactor = (float)Math.Pow(DestbalisersACFactor = (destabilisers / 3f + 0.01f), 0.60f) + 4.1f;
                    if (DestbalisersACFactor < 10f)
                    {
                        DestbalisersACFactor = 10f;
                    }
                }

                if (controller.ShieldData.IsPassiveOn == enumPassiveRegenState.On)
                {
                    PassiveOnOffACChange = 1f;
                    PassiveOnOffHChange = 1f;
                }
                //provides very small bonuses to the shield when passive regen has been set to minimum
                else
                {
                    PassiveOnOffACChange = 1.05f;
                    PassiveOnOffHChange = 1.02f;
                }
                DestabilizersForUI = destabilisers;
                DoublersForUI = doublers;
                //float ap = AdvShieldStatus.GetAp(doublers, pumps, IsContinuous, totalEnergyCapacity);
                MaxEnergy = (float)Math.Round((((laserNode.GetMaximumEnergy() * maxEnergyFactor / ((float)Math.Pow(destabilisers, 1.1134) / 900f + 1f) / ((float)Math.Pow(doublers, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2);
                Energy = (float)Math.Round((((laserNode.GetTotalEnergyAvailable() * maxEnergyFactor / ((float)Math.Pow(destabilisers, 1.1134) / 900f + 1f) / ((float)Math.Pow(doublers, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2);
                ArmorClass = (float)Math.Round(((BaseShieldArmorClass + (DoublersACFactor / 2) - DestbalisersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1);
                PassiveRegen = (float)Math.Round((float)(((((MaxEnergy * 0.01f) + 1f) * ((float)Math.Pow(1.01312f, Math.Min(destabilisers, 300)) + (Math.Max(destabilisers - 300, 0) * 1.2f) + 1f) / ((float)Math.Pow(1.01078f, Math.Min(doublers, 300)) + (Math.Max(doublers - 300, 0) * .05f) + 1f) * PassiveRegenModifier) * passiveRegenFactor) * PowerScalePRModifier), 2);
                MinimumPassiveRegen = MaxEnergy * 0.005f;
                
                //If the current passive regen is below what the minimum passive regen is, set it to the minimum
                if (PassiveRegen < MinimumPassiveRegen)
                {
                    PassiveRegen = MinimumPassiveRegen;
                }
                //If the new passive regen is under 50, set it to 50
                if (PassiveRegen < 50)
                {
                    PassiveRegen = 50;
                }
                //If the new passive regen is over 500K, set it to 500K
                if (PassiveRegen > 500000)
                {
                    PassiveRegen = 500000;
                }
                //If passive regen has been set to minimum in the shield settings (called 'off' in code), set the passive regen to 50
                if (controller.ShieldData.IsPassiveOn == enumPassiveRegenState.Off)
                {
                    PassiveRegen = 50;
                }
                EnergyBeforeMods = (float)Math.Abs((float)Math.Round((((laserNode.GetMaximumEnergy() * maxEnergyFactor * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2));
                HealthLossFromMods = EnergyBeforeMods - Energy; //this is how the UI shows how much health has been lost through doublers and destabs. I'll fix the redundancy later :P
                ArmorClassDifference = ((float)Math.Round(ArmorClass - (float)Math.Round((BaseShieldArmorClass / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 1)); //Same, for armour class
                PassiveRegenDifference = ((float)Math.Round(PassiveRegen - (float)Math.Round((float)((((MaxEnergy * 0.01f) + 1f) * PassiveRegenModifier) * passiveRegenFactor) * PowerScalePRModifier, 2), 2)); //Same, for passive regen
                HealthFromPowerScale = ((float)Math.Round((((laserNode.GetMaximumEnergy() * maxEnergyFactor / ((float)Math.Pow(destabilisers, 1.1134) / 900f + 1f) / ((float)Math.Pow(doublers, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2) - (float)Math.Round((((laserNode.GetMaximumEnergy() * maxEnergyFactor / ((float)Math.Pow(destabilisers, 1.1134) / 900f + 1f) / ((float)Math.Pow(doublers, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier)), 2)); //This figures out how much health is being added by the power scale specifically, for UI.
                ACFromPowerScale = (float)Math.Abs(((float)Math.Round(((BaseShieldArmorClass + (DoublersACFactor / 2) - DestbalisersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1) - (float)Math.Round(((BaseShieldArmorClass + (DoublersACFactor / 2) - DestbalisersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor), 1))); //This figures out how much armor class is being added by the power scale specifically, for UI.
                PRFromPowerScale = (float)Math.Abs(((float)Math.Round((float)(((((MaxEnergy * 0.01f) + 1f) * ((float)Math.Pow(1.01312f, Math.Min(destabilisers, 300)) + (Math.Max(destabilisers - 300, 0) * 1.2f) + 1f) / ((float)Math.Pow(1.01078f, Math.Min(doublers, 300)) + (Math.Max(doublers - 300, 0) * .05f) + 1f) * PassiveRegenModifier) * passiveRegenFactor) * PowerScalePRModifier), 2) - (float)Math.Round((float)(((((MaxEnergy * 0.01f) + 1f) * ((float)Math.Pow(1.01312f, Math.Min(destabilisers, 300)) + (Math.Max(destabilisers - 300, 0) * 1.2f) + 1f) / ((float)Math.Pow(1.01078f, Math.Min(doublers, 300)) + (Math.Max(doublers - 300, 0) * .05f) + 1f) * PassiveRegenModifier) * passiveRegenFactor)), 2))); //This figures out how much passive regen is being added by the power scale specifically, for UI.

                //next two factors are outdated UI things
                if ((PassiveRegen <= 1000) & (PassiveRegenDifference <= 50))
                {
                    PassiveRegenDifference = 0;
                }
                if ((PassiveRegen >= 500000) & (PassiveRegenDifference <= 200))
                {
                    PassiveRegenDifference = 0;
                }
                //if armour class ie below 2, set it to 2
                if (ArmorClass < 2f)
                {
                    ArmorClass = 2f;
                }
                //the following 3 are small scaling effects, taking effect at 40 / 50 / 60. They have been tested to ensure that you are not losing any armour class at each step.
                if (ArmorClass>40f)
                {
                    ArmorClass = (float)Math.Pow((float)Math.Round(((BaseShieldArmorClass + (DoublersACFactor / 2) - DestbalisersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 0.97f)+4.2f;
                    if (ArmorClass < 40f)
                    {
                        ArmorClass = 40f;
                    }
                }
                if (ArmorClass>50)
                {
                    ArmorClass = (float)Math.Pow((float)Math.Round(((BaseShieldArmorClass + (DoublersACFactor / 2) - DestbalisersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 0.91f)+14.84f;
                    if (ArmorClass < 50)
                    {
                        ArmorClass = 50;
                    }
                }
                if (ArmorClass > 60)
                {
                    ArmorClass = (float)Math.Pow((float)Math.Round(((BaseShieldArmorClass + (DoublersACFactor / 2) - DestbalisersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 0.85f) + 27.54f;
                     if (ArmorClass < 60)
                    {
                        ArmorClass = 60;
                    }
                }
                WaitTime = (AdvShieldHandler.BaseWaitTime - WaitTimeModifier) + PowerScaleWaitTimeModifier;
                WTFromPowerScale = PowerScaleWaitTimeModifier;
            }

            /*
            public float GetCurrentHealth(float sustainedUnfactoredDamage) => (Energy - sustainedUnfactoredDamage) / SurfaceFactor;
            public float GetFactoredDamage(float unfactoredDamage) => unfactoredDamage / 2 * SurfaceFactor;
            */
        }
    }
}
