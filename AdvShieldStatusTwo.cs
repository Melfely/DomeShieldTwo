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
using BrilliantSkies.Core.Help;

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
        public float ArmourClass;
        public float PassiveRegen;
        public float TotalBlocks;
        public float EffectiveBlocks;
        public float HealthBeforePowerRouting;
        public float BaseArmourClass = 10;
        public float BaseRegen;
        public float HealthLossFromRoutedPower;
        public float CurrentMaxEnergy = 0;
        public float ArmourIncrease;
        public float RegenIncrease;
        public float Hardeners;
        public float Overchargers;
        public float Transformers;
        public float Spoofers;
        public float ShieldEmpSusceptibility = 1;
        public float ShieldEmpResistivity = 0;
        public float ShieldEmpDamageFactor = 2;
        public float ActiveRectifierSavingsPercent;
        public string ChosenMatrix = "None";
        public float BaseWaitTime;
        public float ActualWaitTime;
        public float CombinedRoutedPowerPercent;
        public bool NotEnoughEnergy = true;
        public float LastArmourReduction = 0;
        public float LastDisruptorStrength = 0;
        public float DisruptionFactor;
        public float EnergyPercentForArmour;
        public float BaseEnergyPercentForArmour;
        //public enumShieldClassSelection currentClass;

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
            //"We are doing away with the class system.";
            if (ShieldHandler == null) ShieldHandler = controller.ShieldHandler;
            Hardeners = 0;
            Transformers = 0;
            TotalBlocks = 0;
            Spoofers = 0;
            Overchargers = 0;
            ShieldData = controller.SettingsData;
            //currentClass = ShieldData.ShieldClass;
            ShieldEmpSusceptibility = 1f;
            ShieldEmpResistivity = 0f;
            ShieldEmpDamageFactor = 2f; //base is 2 so that EMP doesn't deal less damage than other warheads.
            CurrentMaxEnergy = SetShieldNumbers(node);
            HealthBeforePowerRouting = CurrentMaxEnergy;
            CalculateEnergyUsedForAC();
            BaseRegen = CurrentMaxEnergy / 700;
            MaxHealth = CurrentMaxEnergy * (1f - ((EnergyPercentForArmour + ShieldData.RegenPercent) / 100f));
            if (MaxHealth == 0 && CurrentMaxEnergy > 0) MaxHealth = 1f;

            CombinedRoutedPowerPercent = EnergyPercentForArmour + ShieldData.RegenPercent;
            /*
            float baseHardenerIncrease = (Hardeners * (1.3f - Math.Min(MaxHealth / 500000, 0.25f)));
            float adjustedHardenerIncrease = baseHardenerIncrease - Mathf.Min((float)Math.Pow(Hardeners * 0.15f, 1.20f), (Hardeners));
            if (Hardeners == 0) adjustedHardenerIncrease = 1f;
            */
            //if (Hardeners == 1) adjustedHardenerIncrease = 1.8f;

            float baseTransformerIncrease = (Transformers * ((CurrentMaxEnergy * (.1f+(MaxHealth / CurrentMaxEnergy))) / 8000f));
            //float adjustedTransformerIncrease = baseTransformerIncrease - (float)Math.Pow(Transformers, 1.3f);
            //float adjustedTransformerIncrease = baseTransformerIncrease - Mathf.Min((float)(Math.Pow(Transformers, 1.02f) * 1.6f), (float)Math.Pow(Transformers, 1.3f));
            float adjustedTransformerIncrease = baseTransformerIncrease * Mathf.Pow((Mathf.Max((Transformers / Mathf.Pow(Transformers, 1.1f)), 0.5f)), .95f);
            adjustedTransformerIncrease /= 2f;
            if (Transformers == 0 || adjustedTransformerIncrease < 1f) adjustedTransformerIncrease = 1f;

            //if (currentClass == enumShieldClassSelection.HE) { MaxHealth *= 1.5f; HealthBeforePowerRouting *= 1.5f; } "We will want to use these numbers";
            HealthLossFromRoutedPower = MaxHealth - HealthBeforePowerRouting;

            ArmourClass = ShieldData.ArmourSet;
            if (ShieldHandler.isOnFire) ArmourClass -= LastArmourReduction;
            if (ShieldHandler.SufferingFromDisruptor) ArmourClass *= (1f - DisruptionFactor);
            Math.Round(ArmourClass, 0);
            if (ArmourClass < 2) ArmourClass = 2;
            ArmourIncrease = ArmourClass - BaseArmourClass;

            float PassiveRegenBeforePercent = (CurrentMaxEnergy / 2000) + (adjustedTransformerIncrease - 1);
            PassiveRegenBeforePercent *= 2;
            if (PassiveRegenBeforePercent > (MaxHealth / 20)) PassiveRegenBeforePercent = MaxHealth / 20;
            PassiveRegen = PassiveRegenBeforePercent * (1+(Mathf.Pow(ShieldData.RegenPercent, 1.2f)/(6f - (ShieldData.RegenPercent / 40))));
            //WHAT WE CURRENTLY HAVE IS PRETTY GOOD. DON'T MAKE SIGNIFICANT CHANGES WITHOUT KNOWING WHAT YOU ARE DOING AND PRESERVING THIS^
            if (ShieldHandler.TargettedByContLaser) PassiveRegen *= (1f - (ShieldHandler.ContLaserRegenFactor * UnityEngine.Time.timeScale));
            if (ShieldHandler.SufferingFromDisruptor) PassiveRegen *= (1f - DisruptionFactor);
            Math.Round(PassiveRegen, 1);
            RegenIncrease = PassiveRegen - BaseRegen;

            if (ShieldData.IsShieldOn.Us == enumShieldDomeState.On) AffectNumbersByAvailableEnginePower();
            BaseWaitTime = (float)Math.Round(TotalBlocks * 0.25f, 1);
            ActualWaitTime = (float)Math.Round(EffectiveBlocks * 0.25f, 1);
            if (ActualWaitTime < 3f) ActualWaitTime = 3f;
        }

        public void ShieldIsDisrupted(float totalEMPDamageStored)
        {
            float factor = (totalEMPDamageStored * 4) / MaxHealth;
            Math.Clamp(factor, 0.001, 0.8);
            DisruptionFactor = factor;
        }
        public void CalculateEnergyUsedForAC()
        {
            BaseEnergyPercentForArmour = Mathf.Clamp01((ShieldData.ArmourSet - 10f) / (60f - 10f)) * 0.8f;
            if (BaseEnergyPercentForArmour == 0) { EnergyPercentForArmour = 0; return; }
            float hardenerMod = (Hardeners / Mathf.Pow(Hardeners, 1.05f)) * Mathf.Clamp(300000 / CurrentMaxEnergy, 0, 1);
            EnergyPercentForArmour = BaseEnergyPercentForArmour * hardenerMod;
        }

        private void AffectNumbersByAvailableEnginePower()
        {
            float EPP = controller.PowerUse.FractionOfPowerRequestedThatWasProvided;
            float EPPModeMulti = 0.0f;
            if (EPP < 1)
            { 
                NotEnoughEnergy = true;

                if (ShieldHandler.CurrentDamageSustained <= 0)
                {
                    EPPModeMulti = 1.0f; //Base cost of keeping the shield *stable*
                } else if (ShieldHandler.isActiveRegen)
                {
                    EPPModeMulti = DomeShieldConstants.ActiveRegenEPPMulti / DomeShieldConstants.IdleEPPMulti;
                } else
                {
                    EPPModeMulti = DomeShieldConstants.PassiveRegenEPPMulti / DomeShieldConstants.IdleEPPMulti;
                }

                float mainEPP = Math.Min(1.0f, EPP * EPPModeMulti);
                MaxHealth *= mainEPP;
                ArmourClass *= mainEPP;

                PassiveRegen *= EPP;
            }
            else NotEnoughEnergy = false;
        }

        public void AdjustArmourNow(float oxidizer)
        {
            float reduction = (ShieldHandler.TotalFireDamageThisTick * 10 / MaxHealth);
            if (ShieldHandler.TotalOxidizerThisTick != 0)
            {
                bool fulloxi = (ShieldHandler.TotalOxidizerThisTick > ShieldHandler.TotalFireDamageThisTick / 4);
                if (fulloxi) reduction *= 2;
                else reduction *= 1 + (ShieldHandler.TotalOxidizerThisTick / (ShieldHandler.TotalFireDamageThisTick / 4));
            }
            ArmourClass -= reduction;
            LastArmourReduction = reduction;
        }

        public int SetShieldNumbers(DomeShieldNode node)
        {
            float realEnergy = 0;
            float effectiveHardeners = 0;
            float effectiveTransformers = 0;
            float effectiveBlocks = 0;
            float effectiveSpoofers = 0;
            foreach (DomeShieldPowerLink dSCoupler in node.dSPLs)
            {
                float localHardeners = 0;
                float localTransformers = 0;
                float localOverchargers = 0;
                float localEnergy = 0;
                float localBlocks = 0;
                float localSpoofers = 0;
                foreach (DomeShieldBeamInfo beamInfo in dSCoupler.dSBeamInfo)
                {
                    localHardeners += beamInfo.Hardeners;
                    localTransformers += beamInfo.Transformers;
                    localOverchargers += beamInfo.Overchargers;
                    localEnergy += beamInfo.MaxEnergy;
                    localBlocks += beamInfo.TotalBlocks;
                    localSpoofers += beamInfo.Spoofers;
                    TotalBlocks += beamInfo.TotalBlocks;
                }
                float baseOverchargerIncrease = (localOverchargers * 1.1f);
                float penalty = ((baseOverchargerIncrease * 1.83f) - localOverchargers) - 1;
                float adjustedOverchargerIncrease = baseOverchargerIncrease - penalty;
                if (localOverchargers == 1) adjustedOverchargerIncrease = 1.1f;
                if (localOverchargers != 0)
                {
                    effectiveHardeners += (localHardeners * adjustedOverchargerIncrease);
                    effectiveTransformers += (localTransformers * adjustedOverchargerIncrease);
                    realEnergy += (localEnergy * adjustedOverchargerIncrease);
                    effectiveSpoofers += (localSpoofers * adjustedOverchargerIncrease);
                    if ((localBlocks - ((localSpoofers * adjustedOverchargerIncrease) * 3) > 0))
                    {
                        effectiveBlocks += (localBlocks - ((localSpoofers * adjustedOverchargerIncrease) * 3));
                    }
                    //No reason to add 0, we can just ignore otherwise.
                }
                else
                {
                    effectiveHardeners += localHardeners;
                    effectiveTransformers += localTransformers;
                    realEnergy += localEnergy;
                    effectiveSpoofers += localSpoofers;
                    if ((localBlocks - (localSpoofers * 3) > 0))
                    {
                        effectiveBlocks += (localBlocks - (localSpoofers * 3));
                    }
                }
            }
            Hardeners = effectiveHardeners;
            Transformers = effectiveTransformers;
            EffectiveBlocks = effectiveBlocks;
            Spoofers = effectiveSpoofers;
            MaxHealth = realEnergy;
            int en = Rounding.FloatToInt(realEnergy);
            return en;
        }
    }
}
