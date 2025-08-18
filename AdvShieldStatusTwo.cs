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
using DomeShieldTwo.newshieldblocksystem;
using BrilliantSkies.Core.Logger;

namespace DomeShieldTwo
{
    public class AdvShieldStatusTwo
    {
        private AdvShieldProjector controller;

        public AdvShieldHandler ShieldHandler { get; set; }

        private float maxEnergyFactor = 1f;
        private float armorClassFactor = 1f;
        private float passiveRegenFactor = 0f;

        public AdvShieldSettingsData ShieldData;

        public float MaxHealth;
        public float HealthBeforePowerRouting;
        public float ArmourClass;
        public float PassiveRegen;
        public float BaseArmourClass = 20;
        public float BaseRegen;
        public float HealthLossFromRoutedPower;
        public float ArmourIncrease;
        public float RegenIncrease;
        public int Hardeners;
        public int Transformers;
        public float ShieldEmpSusceptibility = 1;
        public float ShieldEmpResistivity = 0;
        public float ShieldEmpDamageFactor = 2;
        public float ActiveRectifierSavingsPercent;
        public string ChosenMatrix = "default";
        public float PowerScale;
        public float WaitTime;
        public float BaseWateTime = 48;
        public float CombinedRoutedPowerPercent;

        public AdvShieldStatusTwo(AdvShieldProjector controller, float maxEnergyFactor, float armorClassFactor, float passiveRegenFactor)
        {
            this.controller = controller;
            this.maxEnergyFactor = maxEnergyFactor;
            this.armorClassFactor = armorClassFactor;
            this.passiveRegenFactor = passiveRegenFactor;
            //all of this can be seen in the lowest tab in the item selection in mods->create->AdvancedShields->Advanced Shield Controller. This is effectively so that the 1x1x1 dome shield can have worse stats.
        }
        public void UpdateShieldInformation(DomeShieldNode node)
        {
            //AdvLogger.LogInfo("Running UpdateShieldInformation");
            Hardeners = 0;
            Transformers = 0;
            ShieldData = controller.SettingsData;
            ShieldEmpSusceptibility = 1f;
            ShieldEmpResistivity = 0f;
            ShieldEmpDamageFactor = 2f; //base is 2 so that EMP doesn't deal less damage than other warheads.
            float mE = node.MaximumEnergy;
            HealthBeforePowerRouting = mE;
            BaseRegen = mE / 150;
            CombinedRoutedPowerPercent = (1f - ((ShieldData.ArmourPercent + ShieldData.RegenPercent) / 100f));
            PowerScale = this.controller.SettingsData.ExcessDrive;
            foreach (DomeShieldPowerLink dSCoupler in node.dSPLs)
            {
                foreach (DomeShieldBeamInfo beamInfo in dSCoupler.dSBeamInfo)
                {
                    Hardeners += beamInfo.Hardeners;
                    Transformers += beamInfo.Transformers;
                }
            }
            float baseHardenerIncrease = (Hardeners * 2f);
            float adjustedHardenerIncrease = baseHardenerIncrease - (float)Math.Pow(Hardeners * 0.3f, 1.42f);
            if (Hardeners == 0) adjustedHardenerIncrease = 1.5f;

            //if (Hardeners == 1) adjustedHardenerIncrease = 1.8f;
            MaxHealth = mE * (1f - ((ShieldData.ArmourPercent + ShieldData.RegenPercent) / 100f));

            float baseTransformerIncrease = (Transformers * (MaxHealth / 2500));
            float adjustedTransformerIncrease = baseTransformerIncrease - (float)Math.Pow(Transformers, 1.5f);
            if (Transformers == 0) adjustedTransformerIncrease = 0f;

            if (controller.SettingsData.ShieldClass == enumShieldClassSelection.HE) { MaxHealth *= 1.5f; HealthBeforePowerRouting *= 1.5f; }
            HealthLossFromRoutedPower = MaxHealth - HealthBeforePowerRouting;

            ArmourClass = 20 + ((ShieldData.ArmourPercent / 100) * (adjustedHardenerIncrease * 3.5f));
            if (controller.SettingsData.ShieldClass == enumShieldClassSelection.AC) ArmourClass *= 1.2f;
            Math.Round(ArmourClass, 0);
            ArmourIncrease = ArmourClass - BaseArmourClass;

            PassiveRegen = (MaxHealth / 750) * ((1f+(2*ShieldData.RegenPercent)) + ((adjustedTransformerIncrease / (10/ShieldData.RegenPercent))/2));
            if (controller.SettingsData.ShieldClass == enumShieldClassSelection.REG) PassiveRegen *= 1.5f;
            Math.Round(PassiveRegen, 1);
            RegenIncrease = PassiveRegen - BaseRegen;
        }
    }
}
