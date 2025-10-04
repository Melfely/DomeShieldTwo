using AdvShields.Behaviours;
using AdvShields.Models;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Effects.Pools.DamageAndDebris;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using BrilliantSkies.Ftd.Constructs.Modules.All.FireDamage;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.BattleOrchestration.AiPositioning;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.Ftd.Game.Pools.SpecialExplosionEffects;
using BrilliantSkies.GridCasts;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Consoles.Getters;
using DomeShieldTwo;
using DomeShieldTwo.newshieldblocksystem;
using FtdRequirements.GameDesignDocuments.ForcesAndVehicles.Vehicles.ConnectionRules;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
//using BrilliantSkies.Core.Units;


namespace AdvShields
{
    public class AdvShieldHandler : IDamageable
    {
        public const float BaseWaitTime = 48.0f;

        public const float PassiveRegenDelay = 1.0f;

        public const float BaseSurface = 1256;

        private AdvShieldProjector controller;

        private AdvShieldStatusTwo stats;

        public Rigidbody Rigidbody => controller.MainConstruct.PlatformPhysicsRestricted.GetRigidbody();

        public Transform transform => controller.GetConstructableOrSubConstructable().GameObject.myTransform;

        public bool RequireRaycastForExplosion => true;

        public float TimeSinceLastHit = 0f;

        public float CurrentDamageSustained { get; set; }

        public float AmountPassivelyRegenerated { get; set; }

        public string PassiveRegenText {  get; set; }

        public string ILoveNumbers {  get; set; }

        public float PassiveRegenOverchargeRegenModifier { get; set; }

        public float PassiveRegenOverchargeArmourModifier { get; set; }

        public float TotalFireDamageThisTick = 0f;

        public float TotalOxidizerThisTick = 0f;

        public float ContinuousLaserDamageThisFrame = 0f;

        public bool isOnFire = false;

        public float WaitTimeDelay {  get; set; }

        public Elipse Shape { get; set; }

        public Vector3 GridcastHit { get; set; }

        public bool ShieldDisabled = false;

        public float NextShieldFireTick;

        public bool SufferingFromDisruptor = false;

        public float EMPDamageCachedForDisruption = 0;

        public float TimeDisrupted = 0; //There will be a couple seconds of no disruption recovery before it begins "healing"

        public bool TargettedByContLaser = false;

        //public float TimeSinceHitByContLaser = 0;

        public float ContLaserRegenFactor = 0; //Keep this between 0 and 1

        public float TimeAtFullHealth = 0;

        public bool isActiveRegen = true;

        public float TimeSinceModeSwap = 1;
        public float GetCurrentHealth()
        {
            return controller.ShieldStats.MaxHealth - CurrentDamageSustained;
        }

        public AllConstruct GetC()
        {
            return controller.GetC();
        }
        public struct FireInstance
        {
            public float Fuel;
            public float Intensity;
            public float Oxidizer;
            public float RemainingTime;

            public FireInstance(float fuel, float intensity, float oxidizer, float time)
            {
                Fuel = fuel;
                Intensity = intensity;
                Oxidizer = oxidizer;
                RemainingTime = time;
            }
        }

        public struct ContLaserInstance
        {
            public float Factor;
            public float Time;

            public ContLaserInstance (float factor, float time)
            {
                Factor = factor;
                Time = time;
            }
        }
        private List<FireInstance> fireStorage = new List<FireInstance>(256); // pre-allocated
        private List<ContLaserInstance> laserStorage = new List<ContLaserInstance>(1000); //pre-allocated

        public AdvShieldHandler(AdvShieldProjector controller)
        {
            this.controller = controller;
            Shape = new Elipse(controller);
            this.stats = controller.ShieldStats;
        }

        public void AdjustTimeSinceLastHit(float damage)
        {
            AdvShieldStatusTwo stats = controller.ShieldStats;
            if (damage > stats.MaxHealth / 100) TimeSinceLastHit = 0;
            else if (damage < 2000 && damage < stats.MaxHealth / 500)
            {
                return;
            }
            else
            {
                float percent = damage / (stats.MaxHealth / 25);
                TimeSinceLastHit -= percent * stats.ActualWaitTime * 2;
                if (TimeSinceLastHit < 0) TimeSinceLastHit = 0;
            }
            isActiveRegen = false;
        }

        public void HandleGenericAPSHit (ShellModel shell, Vector3 position, IDamageLogger gunner)
        {
            //AdvLogger.LogInfo($"Running a test, does this shell have {shell.ExplosiveCharges.GetExplosionDamage()} HE damage?");
            //It does!!! Oh thank goodness. We are getting things fixed!
            foreach (var part in shell.PartsAndMesh.AllParts)
            {
                if (part.Type == ShellModuleType.ShieldBuster) { ShellHasDisruptor(shell.ExplosiveCharges.GetEmpDamage()); break; }
            }
            if (shell.ExplosiveCharges.GetExplosionDamage() > 0) ApplyHEDamage(shell.ExplosiveCharges.GetExplosionDamage(), position);
            if (shell.ExplosiveCharges.GetFlakExplosionDamage() > 0) ApplyFlakDamage(shell.ExplosiveCharges.GetFlakExplosionDamage(), position);
            if (shell.ExplosiveCharges.GetEmpDamage() > 0) ApplyEmpDamage(shell.ExplosiveCharges.GetEmpDamage(), position);
            if (shell.ExplosiveCharges.GetIncendiaryFuel() > 0) HandleNonFlamethrowerFireHit(shell.ExplosiveCharges.GetIncendiaryFuel(), shell.ExplosiveCharges.GetIncendiaryIntensity(), shell.ExplosiveCharges.GetIncendiaryOxidizer());
            if (shell.ExplosiveCharges.GetFragCount() > 0) { float angle = shell.ExplosiveCharges.GetFragAngle(); HandleFrag(shell.ExplosiveCharges.GetFragDamage() * FragGenerator.GetAngleDamageMultiplier(angle), shell.ExplosiveCharges.GetFragCount(), angle, position); }
            if (shell.KineticDamage.ApplyAsMeleeDamage()) ApplyThumpDamage(shell.KineticDamage.GetKineticDamage(), shell.ArmourPierce.GetArmourPiercing(), position);
            else ApplyPierceDamage(shell.KineticDamage.GetKineticDamage(), shell.ArmourPierce.GetArmourPiercing(), position);
        }

        public void HandleGenericCRAMAndSimpleHit(ProjectileImpactState state, Vector3 position)
        {
            //This should work for cram. Weird that it doesn't for APS, but, whatever x-x
            if (state.ExplosiveDamage > 0) ApplyHEDamage(state.ExplosiveDamage, position);
            if (state.EmpDamage > 0) ApplyEmpDamage(state.EmpDamage, position);
            if (state.FireFuel > 0) HandleNonFlamethrowerFireHit(state.FireFuel, state.FireIntensity, state.FireOxidizer);
            if (state.FragCount > 0) HandleFrag(state.FragDamage, state.FragCount, state.FragAngle, position);
            if (state.ApplyAsMeleeDamage) ApplyThumpDamage(state.KineticDamage, state.ArmourPiercing, position);
            else ApplyPierceDamage(state.KineticDamage, state.ArmourPiercing, position);
        }

        public void HandleStrayFragmentHit(ProjectileImpactState state, Vector3 position)
        {
            ApplyPierceDamage(state.KineticDamage, 4, position);
        }
        public void ApplyLaserDamage(LaserDamageDescription LDD, Vector3 position, bool isContinuous)
        {
            //We got lasers to hit the shield. Just gotta actually code it now.
            float realDamage = LDD.CalculateDamage(LDD.DamagePotential, GetCurrentHealth(), position);
            realDamage *= GetCardMult("Laser");
            //AdvLogger.LogInfo($"Laser damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            if (isContinuous) HandleContLaser(realDamage);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Laser, GAME_STATE.MyTeam);
            if (isContinuous) AdjustTimeSinceLastHit(realDamage * 5);
            else AdjustTimeSinceLastHit(realDamage);
                float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            //Don't forget to include the position
            CreateAnimation(position, Mathf.Max(realDamage, 1), hitColor);
        }

        private void HandleContLaser(float realDamage)
        {
            TargettedByContLaser = true;
            float laserPercentForRegen = (realDamage * 50) / stats.MaxHealth;
            Math.Clamp(laserPercentForRegen, 0, 1);
            ContLaserRegenFactor += laserPercentForRegen;
            laserStorage.Add(new ContLaserInstance(laserPercentForRegen, 0.1f));
            if (TimeSinceLastHit > stats.ActualWaitTime - 1) ; TimeSinceLastHit = stats.ActualWaitTime = 1;
        }
        public void ApplyThumpDamage(float damage, float AP, Vector3 position)
        {
            float armorMod = AP / stats.ArmourClass;
            float realDamage = damage;
            if (armorMod < 1) realDamage *= armorMod;
            realDamage *= 1.1f; //A very minor buff to thump damage since it's kind of kinetic
            realDamage *= GetCardMult("Thump");
            //AdvLogger.LogInfo($"Thump damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Crash, GAME_STATE.MyTeam);
            AdjustTimeSinceLastHit(realDamage);
            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            //Don't forget to include the position
            CreateAnimation(position, Mathf.Max(realDamage, 1), hitColor);
        }

        public void ApplyPierceDamage(float damage, float AP, Vector3 position, bool isParticle = false)
        {
            float armorMod = AP / stats.ArmourClass;
            float realDamage = damage;
            if (armorMod < 1) realDamage *= armorMod;
            realDamage *= 1.2f; //Slightly increasing pierce damage to the shield because it's pierce
            realDamage *= GetCardMult("Pierce");
            //AdvLogger.LogInfo($"Pierce damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) { if (!isParticle) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Kinetic, GAME_STATE.MyTeam); }
            AdjustTimeSinceLastHit(realDamage);
        }

        public void ApplyPlasmaDamage(PlasmaDamageDescription PDD, Vector3 position)
        {
            float realDamage = PDD.OriginalDamage;
            float armorMod = PDD.ArmourPierce / stats.ArmourClass;
            realDamage *= armorMod;
            realDamage *= GetCardMult("Plasma");
            //AdvLogger.LogInfo($"Plasma damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Plasma, GAME_STATE.MyTeam);
            AdjustTimeSinceLastHit(realDamage);
        }

        public void HandleFrag(float damage, int count, float angle, Vector3 position)
        {
            AdvLogger.LogInfo($"{damage}, {count}, {angle}");
            float realDamage = CalculateActualFragDamage(damage, count, angle);
            float armorMod = 4 / stats.ArmourClass;
            realDamage *= armorMod;
            realDamage /= 1.25f; //Was 1.5, now 1.25 for the slight kinetic damage buff

            realDamage *= GetCardMult("Pierce");
            //AdvLogger.LogInfo($"Frag damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Kinetic, GAME_STATE.MyTeam);
            AdjustTimeSinceLastHit(realDamage);
        }

        private float CalculateActualFragDamage(float damage, int count, float angle)
        {
            float actualCount = count * RandomCountMultiplier(angle);
            return damage * actualCount;

        }
        float RandomCountMultiplier(float angle)
        {
            // Normalize angle between 15 and 180
            float t = Mathf.InverseLerp(15f, 180f, angle); // 0 at 15°, 1 at 180°

            // Base average multiplier: linearly scale from 1 → 0.5
            float avg = Mathf.Lerp(1f, 0.5f, t);

            // Add randomness: random factor around avg (± some fraction)
            float noise = UnityEngine.Random.Range(-0.2f, 0.2f) * (1f - avg);
            // smaller randomness when angle is narrow, bigger when angle is wide

            return Mathf.Clamp01(avg + noise);
            //Thanks ChatGPT!
        }

        public void ApplyHEDamage(float damage, Vector3 position)
        {
            AdvShieldStatusTwo stats = controller.ShieldStats;
            float realDamage = CalculateActualHEDamage(damage, stats);
            realDamage *= GetCardMult("Explosive");
            //AdvLogger.LogInfo($"HE damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Explosive, GAME_STATE.MyTeam);
            AdjustTimeSinceLastHit(realDamage);
            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            //AdvLogger.LogInfo($"Hit position was {position}");
            //Don't forget to include the position
            CreateAnimation(position, realDamage / maxEnergy, hitColor);
        }
        private float CalculateActualHEDamage(float damage, AdvShieldStatusTwo stats)
        {
            float realDamage = damage;
            //realDamage *= 0.8f;
            float divisor = (2f / 59f) * stats.ArmourClass + (57f / 59f);
            realDamage /= divisor;
            //This feels a bit low?
            return realDamage;
        }

        public void ApplyFlakDamage (float damage, Vector3 position)
        {
            ApplyHEDamage (damage/2, position);
        }
        public void ApplyEmpDamage(float empdamage, Vector3 position)
        {
            //This looks a little outdated...
            AdvShieldStatusTwo stats = controller.ShieldStats;
            //Adding card mult, still think the method is outdated
            empdamage *= GetCardMult("EMP");
            //AdvLogger.LogInfo($"EMP damage after all mults: {empdamage}", LogOptions._AlertDevInGame);
            //card mult
            empdamage *= 1.5f;
            CurrentDamageSustained += empdamage;

            float magnitude;
            Vector3 hitPosition;

            magnitude = empdamage / 300;
            hitPosition = GridcastHit - controller.GameWorldPosition;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empdamage), DamageType.Emp, GAME_STATE.MyTeam);
            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            AdjustTimeSinceLastHit(empdamage);
            CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
        }
        public void ShellHasDisruptor(float EMP)
        {
            if (!SufferingFromDisruptor)
            {
                SufferingFromDisruptor = true;
                TimeDisrupted = 0;
                AdvLogger.LogInfo($"EMP damage is showing as {EMP}");
                EMP *= GetCardMult("EMP");
                EMPDamageCachedForDisruption += EMP;
                stats.ShieldIsDisrupted(EMPDamageCachedForDisruption);
            }
            else
            {
                EMP *= GetCardMult("EMP");
                EMPDamageCachedForDisruption += EMP;
                stats.ShieldIsDisrupted(EMPDamageCachedForDisruption);
            }
            //How will we code disruption?
            //We need to go off of emp damage, not the "shield piercing" amount. We might need to do some tooltip adjusting in the cannon UI.
        }

        public void HandleFlamethrowerHit (PooledFlamerProjectile flame)
        {
            if (!isOnFire)
            {
                isOnFire = true;
                fireStorage.Add(new FireInstance(flame.Fuel, flame.Intensity, flame.Oxidizer, 5));
                NextShieldFireTick = 0;
                return;
            }
            fireStorage.Add(new FireInstance(flame.Fuel, flame.Intensity, flame.Oxidizer, 5));
        }

        public void HandleNonFlamethrowerFireHit (float fuel, float intensity, float oxidizer)
        {
            if (!isOnFire)
            {
                isOnFire = true;
                fireStorage.Add(new FireInstance(fuel, intensity, oxidizer, 5));
                NextShieldFireTick = 0;
                return;
            }
            fireStorage.Add(new FireInstance(fuel, intensity, oxidizer, 5));
        }

        private void TakeFireDamage()
        {
            NextShieldFireTick = 0.5f;
            float totalFuel = 0f;
            float totalIntensity = 0f;
            float totalOxidizer = 0f;
            int count = 0;
            for (int i = fireStorage.Count - 1; i >= 0; i--)
            {
                var fire = fireStorage[i];
                fire.RemainingTime -= 0.5f;

                if (fire.RemainingTime <= 0)
                {
                    fireStorage.RemoveAt(i); // cheap O(1) removal at end
                    continue;
                }

                fireStorage[i] = fire; // reassign since it's a struct

                totalFuel += fire.Fuel;
                totalIntensity += fire.Intensity;
                totalOxidizer += fire.Oxidizer;
                count++;
            }
            if (count == 0)
            {
                isOnFire = false;
                return;
            }
            float averageIntensity = totalIntensity / count;
            AdvShieldStatusTwo stats = controller.ShieldStats;
            stats.AdjustArmourNow(totalOxidizer);
            float realDamage = CalculateActualFireDamage(totalFuel, averageIntensity, stats.ArmourClass);
            realDamage *= GetCardMult("Fire");
            //AdvLogger.LogInfo($"Fire damage after all mults: {realDamage}", LogOptions._AlertDevInGame);
            CurrentDamageSustained += realDamage;
            float maxEnergy = stats.MaxHealth;
            if (CurrentDamageSustained >= maxEnergy)
            {
                //CurrentDamageSustained = maxEnergy;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.Off;
                fireStorage.Clear();
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            if (!this.controller.OnPlayerTeam) DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(realDamage), DamageType.Fire, GAME_STATE.MyTeam);
            AdjustTimeSinceLastHit(realDamage);
        }
        private float CalculateActualFireDamage(float fuel, float intensity, float AC)
        {
            float mult = intensity / AC;
            Math.Clamp(mult, 0.5, 2);
            return (fuel / 10) * mult;
        }

        public void ApplyPierceParticleDamage (ParticleDamageDescription pD, Vector3 position)
        {
            ApplyPierceDamage(pD.CalculateDamage(stats.ArmourClass, GetCurrentHealth(), position), pD.ArmourPierce, position, true);
        }

        public void ApplyExplosiveParticleDamage (ExplosionDamageDescription eD, Vector3 position)
        {
            ApplyHEDamage(eD.DamagePotential, position);
        }

        public void ApplyEmpParticleDamage(float num3, Vector3 position)
        {
            ApplyEmpDamage(num3, position);
        }

        public void ApplyThumpParticleDamage (float num3, float ap, Vector3 position)
        {
            ApplyThumpDamage(num3, ap, position);
        }

        public float GetCardMult(string damType)
        {
            //We want the card mult to happen at the very end, right before damage is applied. AKA after all other calculations
            string card = controller.Node.ConnectedCard;
            if (card.ToLower() == "none") return 1;
            //We will try to check if we have a card installed before calling this method, but it's good to have this.
            //Remember that this is cards, not damage types
            switch (card)
            {
                default:
                    AdvLogger.LogInfo($"Invalid card type (registered card was {card})", LogOptions._AlertDevInGame);
                    return 1;
                case "Pierce":
                    return CombinePierce(damType);
                case "Thump":
                    return CombineThump(damType);
                case "Explosive":
                    return CombineExplosive(damType);
                case "Energy":
                    return CombineEnergy(damType);
                case "Fire":
                    return CombineFire(damType);
                case "EMP":
                    return CombineEMP(damType);
                case "Particle":
                    return CombineParticle(damType);
                    //We still need a CombineParticle because other weapons deal more damage.
            }
        }

        //We can use a default multiplier for damage types that would be unaffected
        private float CombinePierce (string damType)
        {
            //Remember that this is damage types, not cards
            switch (damType)
            {
                case "Pierce":
                    return 0.25f;
                case "Explosive":
                    return 1.3f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }

        private float CombineThump(string damType)
        {
            //Remember that this is damage types, not cards
            switch (damType)
            {
                case "Pierce":
                    return 1.2f;
                case "Thump":
                    return 0.25f;
                case "Plasma":
                    return 1.2f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }

        private float CombineExplosive(string damType)
        {
            switch (damType)
            {
                case "Explosive":
                    return 0.25f;
                case "Fire":
                    return 1.3f;
                case "Laser":
                    return 1.15f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }
        private float CombineEnergy(string damType)
        {
            switch (damType)
            {
                case "Laser":
                    return 0.5f;
                case "Plasma":
                    return 0.5f;
                case "Pierce":
                    return 1.25f;
                case "Thump":
                    return 1.25f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }
        private float CombineFire(string damType)
        {
            switch (damType)
            {
                case "Fire":
                    return 0.25f;
                case "EMP":
                    return 1.3f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }
        private float CombineEMP(string damType)
        {
            switch (damType)
            {
                case "EMP":
                    return 0.2f;
                case "Pierce":
                    return 1.35f;
                case "Thump":
                    return 1.35f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }
        private float CombineParticle(string damType)
        {
            switch (damType)
            {
                case "Particle":
                    AdvLogger.LogInfo("You shouldn't be running GetCardMult for particle damage...");
                    return 1;
                case "Laser":
                    return 1.1f;
                case "EMP":
                    return 1.1f;
                case "Plamsa":
                    return 1.1f;
                default:
                    return 1f;
                    //Default means it wasn't any other case.
            }
        }
        public void CreateAnimation(Vector3 worldHit, float magnitude, Color color)
        {
            //We'll have to work on this later.
            return;
            //AdvLogger.LogInfo("Get ready to crash the game!");
            //AdvLogger.LogInfo($"{controller.GameWorldPosition}");
            //We need to re-write this if possible
            //Vector3 shieldSpawnLocation = new Vector3(controller.ShieldDome.transform);
            GameObject obj = UnityEngine.Object.Instantiate(StaticStorage.HitEffectObject);

            // Match the shield dome transform exactly (world space)
            Transform target = controller.ShieldDome.transform;
            obj.transform.position = target.position;
            obj.transform.rotation = target.rotation;
            Vector3 scale = target.lossyScale;
            scale.x += 0.5f;
            scale.y += 0.5f;
            scale.z += 0.5f; //Making the hit effect a tiny bit larger to ensure shows
            obj.transform.localScale = scale; // world scale copy

            // Now parent it so it follows the shield
            obj.transform.SetParent(target);

            //obj.transform.position = controller.GameWorldPosition;
            //obj.transform.rotation = controller.GameWorldRotation;
            //obj.transform.localPosition = Transforms.LocalToGlobal(Vector3.zero, controller.GameWorldPosition, controller.GameWorldRotation);
            //obj.transform.localRotation = Transforms.LocalRotationToGlobalRotation(Quaternion.identity, controller.GameWorldRotation);
            //AdvLogger.LogInfo($"Shield's position is {controller.ShieldDome.transform.position}");
            HitEffectBehaviour behaviour = obj.GetComponent<HitEffectBehaviour>();
            behaviour.Initialize(worldHit, color, magnitude, 3f, controller.ShieldDome.transform);
        }

        private void HandleUpdateModifiers()
        {
            if (isOnFire)
            {
                NextShieldFireTick -= Time.deltaTime * Time.timeScale;
                if (NextShieldFireTick <= 0) TakeFireDamage();
            }
            if (TargettedByContLaser)
            {
                for (int i = laserStorage.Count - 1; i >= 0; i--)
                {
                    var laser = laserStorage[i];
                    laser.Time -= Time.deltaTime * Time.timeScale;
                    if (laser.Time <= 0)
                    {
                        laserStorage.RemoveAt(i); // cheap O(1) removal at end
                        ContLaserRegenFactor -= laser.Factor;
                        continue;
                    }

                    laserStorage[i] = laser; // reassign since it's a struct
                }
                if (laserStorage.Count == 0) TargettedByContLaser = false;
            }
            if (SufferingFromDisruptor)
            {
                //AdvLogger.LogInfo($"Current EMP cached is {EMPDamageCachedForDisruption}");
                //AdvLogger.LogInfo($"Current time since first disrupted is cached is {TimeDisrupted}");

                //We need to decide how we will work disruptors, eh?
                if (EMPDamageCachedForDisruption <= 0)
                {
                    SufferingFromDisruptor = false;
                    TimeDisrupted = 0;
                    EMPDamageCachedForDisruption = 0;
                    return;
                }
                stats.ShieldIsDisrupted(EMPDamageCachedForDisruption);
                if (TimeDisrupted > Math.Clamp(stats.ActualWaitTime / 2, 2, 30))
                {
                    EMPDamageCachedForDisruption -= ((stats.MaxHealth / 100) + ((stats.PassiveRegen/stats.MaxHealth) * 15) * Time.deltaTime * Time.timeScale);
                }
                TimeDisrupted += Time.deltaTime * Time.timeScale;
            }
        }

        // Existing code...
        public void Update(AdvShieldStatusTwo ShieldStats)
        {
            HandleUpdateModifiers();
            float timeThisTick = Time.deltaTime;
            TimeSinceModeSwap += timeThisTick;
            if (!isActiveRegen)
            {
                TimeSinceLastHit += timeThisTick;
                if (TimeSinceLastHit > ShieldStats.ActualWaitTime) isActiveRegen = true;
            }
            if ((CurrentDamageSustained <= 0.0f) && (controller.SettingsData.IsShieldOn.Us == enumShieldDomeState.On) && TimeAtFullHealth < DomeShieldConstants.SHIELDREGENSWAPDELAY)
            {
                PassiveRegenText = "The shield is at full health, will move to idle mode soon";
                CurrentDamageSustained = 0.0f;
                TimeAtFullHealth += timeThisTick;
                return;
            }
            if ((CurrentDamageSustained <= 0.0f) && (controller.SettingsData.IsShieldOn.Us == enumShieldDomeState.On) && TimeAtFullHealth >= DomeShieldConstants.SHIELDREGENSWAPDELAY)
            {
                PassiveRegenText = "The shield is at full health, it is not regenerating.";
                CurrentDamageSustained = 0.0f;
                TimeAtFullHealth += timeThisTick;
                return;
            }

            if ((CurrentDamageSustained > 0.0f) && (controller.SettingsData.IsShieldOn.Us == enumShieldDomeState.On) && (!isActiveRegen))
            {
                if (ShieldDisabled) return;
                if (TimeSinceModeSwap <= 0.5f) return;
                CurrentDamageSustained -= (ShieldStats.PassiveRegen * Time.deltaTime);
                //AmountPassivelyRegenerated += (ShieldStats.PassiveRegen * Time.deltaTime);
                //We are dividing by 70 to account for how often this method runs.
                PassiveRegenText = "Shield is using increased engine power to passively regenerate the shield";
                TimeAtFullHealth = 0;
                return;
            }
            if (!isActiveRegen) return;
            if ((CurrentDamageSustained / ShieldStats.MaxHealth <= (1-(controller.SettingsData.ShieldReactivationPercent / 100))) && (controller.SettingsData.IsShieldOn.Us == enumShieldDomeState.Off))
            {
                if (TimeSinceModeSwap <= 0.5f) return;
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.On;
                CurrentDamageSustained = ShieldStats.MaxHealth * (controller.SettingsData.ShieldReactivationPercent / 100);
                AmountPassivelyRegenerated = 0;
                TimeAtFullHealth = 0;
                ShieldDisabled = false;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            else if (CurrentDamageSustained > 0.0f)
            {
                if (TimeSinceModeSwap <= 0.5f) return;
                CurrentDamageSustained -= ((ShieldStats.PassiveRegen * 10) * Time.deltaTime);
                AmountPassivelyRegenerated = 0;
            }
            /*if ((CurrentDamageSustained / ShieldStats.MaxEnergy <= 0.999f) && (controller.ShieldData.Type.Us == enumShieldDomeState.On))
            {
                CurrentDamageSustained -= ShieldStats.PassiveRegen;
            }
            */
            //Added this^^

            /*bool AllowPassiveRegen = (CurrentDamageSustained > 0.0f);

            if (AllowPassiveRegen == true)
            {
                CurrentDamageSustained -=ShieldStats.PassiveRegen;
            }*/
            if (CurrentDamageSustained <= 0.0f)
            {
                controller.SettingsData.IsShieldOn.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
                ShieldDisabled = false;
                //controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            /*if (CurrentDamageSustained == 0.0f)
            {
                controller.ShieldData.Type.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
            }*/
        }

        public void ApplyDamage(IDamageDescription DD)
        {
            return;
            //So that's why ApplyDamage exists
        }
    }
}
