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
using DomeShieldTwo.shieldblocksystem;

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

        public float Hardeners { get; private set; }

        public float Transformers { get; private set; }

        public float Rectifiers { get; private set; }

        public float EnergyBeforeMods { get; private set; }

        public float HealthLossFromMods {  get; private set; }

        public float ArmorClassDifference { get; private set; }

        public float PassiveRegenDifference { get; private set; }

        public float HealthFromPowerScale {  get; private set; }

        public float ACFromPowerScale { get; private set; }
                //Armour Class

        public float PRFromPowerScale { get; private set; }
                //Passive Regeneration

        public float WTFromPowerScale { get; private set; }
                //Wait Time

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

        public float HardenersACFactor { get; private set; }

        public float TransformersACFactor { get; private set; }

        public float PassiveRegenModifier { get; private set; }

        public float PassiveRegenBeforeScaling { get; private set; }

        public float MinimumPassiveRegen { get; private set; }

        public float PowerReductionFromRectifiers { get; private set; }

        public float WaitTime { get; set; }
           
        private bool IsContinuous { get; set; }

        public float HardenersForUI { get; set; }

        public float TransformersForUI { get; set; }

        public float RectifiersForUI { get; set; }

        public float PowerSavingFromRectifiersForUI { get; set; }

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
            //Why on God's green earth is this Update. Dear Lord Jesus we need to make this not every frame. I can't believe the game doesn't run slower.
            Energy = 0;
            ArmorClass = 1;



            DomeShieldNode dSNode = controller.ConnectShieldNode;

            if (dSNode != null)
            {
                int hardeners = 0;
                int pumps = 0;
                //int totalEnergyCapacity = 0;
                int transformers = 0;
                int rectifiers = 0;
                PowerScale = this.controller.ShieldData.ExcessDrive;
                foreach (DomeShieldCoupler dSCoupler in dSNode.dSCouplers)
                {
                    foreach (DomeShieldBeamInfo beamInfo in dSCoupler.dSBeamInfo)
                    {
                        hardeners += beamInfo.Hardeners;
                        pumps += beamInfo.CubicMetresOfPumping;
                        transformers += beamInfo.Transformers; //these 3 are grabbing the amount of doublers, pumps, and destabs in the laser system.
                        rectifiers += beamInfo.Rectifiers;
                        Hardeners = hardeners;
                        Transformers = transformers; //these 2 are effectively converting int to float
                        Rectifiers = rectifiers;
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
                HardenersACFactor = (hardeners) +0.01f;
                if (HardenersACFactor > 1.3f)
                {
                    HardenersACFactor = (float)Math.Pow(HardenersACFactor = (hardeners) + 0.01f, 0.90f)+0.1f;
                    if (HardenersACFactor < 1.3f)
                    {
                        HardenersACFactor = 1.3f;
                    }
                }
                if (HardenersACFactor > 10f)
                {
                    HardenersACFactor = (float)Math.Pow(HardenersACFactor = (hardeners) + 0.01f, 0.60f) + 4.1f;
                    if (HardenersACFactor < 10f)
                    {
                        HardenersACFactor = 10f;
                    }
                }
                TransformersACFactor = (transformers / 3f) + 0.005f;
                if (TransformersACFactor > 1.29f)
                {
                    TransformersACFactor = (float)Math.Pow(TransformersACFactor = (transformers / 3f) + 0.01f, 0.85f) + 1f;
                    if (TransformersACFactor < 1.29f)
                    {
                        TransformersACFactor = 1.29f;
                    }
                }
                if (TransformersACFactor > 10f)
                {
                    TransformersACFactor = (float)Math.Pow(TransformersACFactor = (transformers / 3f + 0.01f), 0.60f) + 4.1f;
                    if (TransformersACFactor < 10f)
                    {
                        TransformersACFactor = 10f;
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
                TransformersForUI = transformers;
                HardenersForUI = hardeners;
                RectifiersForUI = rectifiers;
                //float ap = AdvShieldStatus.GetAp(doublers, pumps, IsContinuous, totalEnergyCapacity);
                MaxEnergy = (float)Math.Round((((dSNode.GetMaximumEnergy() * maxEnergyFactor / ((float)Math.Pow(transformers, 1.1134) / 900f + 1f) / ((float)Math.Pow(hardeners, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2);
                Energy = (float)Math.Round((((dSNode.GetTotalEnergyAvailable() * maxEnergyFactor / ((float)Math.Pow(transformers, 1.1134) / 900f + 1f) / ((float)Math.Pow(hardeners, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2);
                ArmorClass = (float)Math.Round(((BaseShieldArmorClass + (HardenersACFactor / 2) - TransformersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1);
                PassiveRegen = (float)Math.Round((float)(((((MaxEnergy * 0.01f) + 1f) * ((float)Math.Pow(1.01312f, Math.Min(transformers, 300)) + (Math.Max(transformers - 300, 0) * 1.2f) + 1f) / ((float)Math.Pow(1.01078f, Math.Min(hardeners, 300)) + (Math.Max(hardeners - 300, 0) * .05f) + 1f) * PassiveRegenModifier) * passiveRegenFactor) * PowerScalePRModifier), 2);
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
                EnergyBeforeMods = (float)Math.Abs((float)Math.Round((((dSNode.GetMaximumEnergy() * maxEnergyFactor * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2));
                HealthLossFromMods = EnergyBeforeMods - Energy; //this is how the UI shows how much health has been lost through doublers and destabs. I'll fix the redundancy later :P
                ArmorClassDifference = ((float)Math.Round(ArmorClass - (float)Math.Round((BaseShieldArmorClass / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 1)); //Same, for armour class
                PassiveRegenDifference = ((float)Math.Round(PassiveRegen - (float)Math.Round((float)((((MaxEnergy * 0.01f) + 1f) * PassiveRegenModifier) * passiveRegenFactor) * PowerScalePRModifier, 2), 2)); //Same, for passive regen
                HealthFromPowerScale = ((float)Math.Round((((dSNode.GetMaximumEnergy() * maxEnergyFactor / ((float)Math.Pow(transformers, 1.1134) / 900f + 1f) / ((float)Math.Pow(hardeners, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier) * PowerScaleHealthModifier), 2) - (float)Math.Round((((dSNode.GetMaximumEnergy() * maxEnergyFactor / ((float)Math.Pow(transformers, 1.1134) / 900f + 1f) / ((float)Math.Pow(hardeners, 1.1134) / 900f + 1f) * PassiveOnOffHChange) * BaseHealthModifier)), 2)); //This figures out how much health is being added by the power scale specifically, for UI.
                ACFromPowerScale = (float)Math.Abs(((float)Math.Round(((BaseShieldArmorClass + (HardenersACFactor / 2) - TransformersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1) - (float)Math.Round(((BaseShieldArmorClass + (HardenersACFactor / 2) - TransformersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor), 1))); //This figures out how much armor class is being added by the power scale specifically, for UI.
                PRFromPowerScale = (float)Math.Abs(((float)Math.Round((float)(((((MaxEnergy * 0.01f) + 1f) * ((float)Math.Pow(1.01312f, Math.Min(transformers, 300)) + (Math.Max(transformers - 300, 0) * 1.2f) + 1f) / ((float)Math.Pow(1.01078f, Math.Min(hardeners, 300)) + (Math.Max(hardeners - 300, 0) * .05f) + 1f) * PassiveRegenModifier) * passiveRegenFactor) * PowerScalePRModifier), 2) - (float)Math.Round((float)(((((MaxEnergy * 0.01f) + 1f) * ((float)Math.Pow(1.01312f, Math.Min(transformers, 300)) + (Math.Max(transformers - 300, 0) * 1.2f) + 1f) / ((float)Math.Pow(1.01078f, Math.Min(hardeners, 300)) + (Math.Max(hardeners - 300, 0) * .05f) + 1f) * PassiveRegenModifier) * passiveRegenFactor)), 2))); //This figures out how much passive regen is being added by the power scale specifically, for UI.

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
                    ArmorClass = (float)Math.Pow((float)Math.Round(((BaseShieldArmorClass + (HardenersACFactor / 2) - TransformersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 0.97f)+4.2f;
                    if (ArmorClass < 40f)
                    {
                        ArmorClass = 40f;
                    }
                }
                if (ArmorClass>50)
                {
                    ArmorClass = (float)Math.Pow((float)Math.Round(((BaseShieldArmorClass + (HardenersACFactor / 2) - TransformersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 0.91f)+14.84f;
                    if (ArmorClass < 50)
                    {
                        ArmorClass = 50;
                    }
                }
                if (ArmorClass > 60)
                {
                    ArmorClass = (float)Math.Pow((float)Math.Round(((BaseShieldArmorClass + (HardenersACFactor / 2) - TransformersACFactor) / (MaxEnergy / Energy) * PassiveOnOffACChange * armorClassFactor) + PowerScaleACModifier, 1), 0.85f) + 27.54f;
                     if (ArmorClass < 60)
                    {
                        ArmorClass = 60;
                    }
                }
                WaitTime = (AdvShieldHandler.BaseWaitTime - WaitTimeModifier) + PowerScaleWaitTimeModifier;
                WTFromPowerScale = PowerScaleWaitTimeModifier;

                PowerReductionFromRectifiers = CalculatePowerReductionFromRectifiers(rectifiers, MaxEnergy);

            }

            /*
            public float GetCurrentHealth(float sustainedUnfactoredDamage) => (Energy - sustainedUnfactoredDamage) / SurfaceFactor;
            public float GetFactoredDamage(float unfactoredDamage) => unfactoredDamage / 2 * SurfaceFactor;
            */
        }
        private float CalculatePowerReductionFromRectifiers(int rectifiers, float maxEnergy)
        {
            float maxReduction = 0.5f;
            float baseReduction = (rectifiers * 0.01f);
            float adjustedReduction = baseReduction - (rectifiers * 0.003f);
            if (rectifiers == 1) adjustedReduction = 0.01f;

            // Scaling penalty for large systems
            float penaltyStrength = 0.2f; // 20% less effective at maxEnergy = 3,000,000
            float penaltyFactor = 1f;

            if (maxEnergy > 50000f)
            {
                float t = Mathf.Clamp01((maxEnergy - 50000f) / (3000000f - 50000f));
                penaltyFactor = 1f - t * penaltyStrength;
            }

            // Apply penalty
            float finalReduction = adjustedReduction * penaltyFactor;
            if (finalReduction > maxReduction) finalReduction = maxReduction;
            if (ShieldType == "REGENERATOR") PowerReductionFromRectifiers = 1f - ((1f - PowerReductionFromRectifiers) * 1.25f);
            PowerSavingFromRectifiersForUI = finalReduction * 100;
            // Convert to multiplier (invert)
            float powerReductionFromRectifiers = 1f - finalReduction;

            return powerReductionFromRectifiers;
        }
    }
}
