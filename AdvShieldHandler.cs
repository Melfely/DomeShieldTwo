using System;
using System.Linq;
using System.Threading.Tasks;
using AdvShields.Behaviours;
using AdvShields.Models;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools.SpecialExplosionEffects;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.GridCasts;
using BrilliantSkies.Modding.Types;
using HarmonyLib;
using UnityEngine;
using BrilliantSkies.Effects.Pools.DamageAndDebris;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Modding;
using BrilliantSkies.Core.Logger;
using UnityEngine.UIElements;
using BrilliantSkies.Ftd.Game.BattleOrchestration.AiPositioning;
using DomeShieldTwo.shieldblocksystem;


namespace AdvShields
{
    public class AdvShieldHandler : IDamageable
    {
        public const float BaseWaitTime = 48.0f;

        public const float PassiveRegenDelay = 1.0f;

        public const float BaseSurface = 1256;

        private AdvShieldProjector controller;

        public Rigidbody Rigidbody => controller.MainConstruct.PlatformPhysicsRestricted.GetRigidbody();

        public Transform transform => controller.GetConstructableOrSubConstructable().GameObject.myTransform;

        public bool RequireRaycastForExplosion => true;

        public float TimeSinceLastHit { get; private set; }

        public float CurrentDamageSustained { get; set; }

        public float AmountPassivelyRegenerated { get; set; }

        public string PassiveRegenText {  get; set; }

        public string ILoveNumbers {  get; set; }

        public float PassiveRegenOverchargeRegenModifier { get; set; }

        public float PassiveRegenOverchargeArmourModifier { get; set; }

        public float WaitTimeDelay {  get; set; }

        public Elipse Shape { get; set; }

        public Vector3 GridcastHit { get; set; }

        public bool ShieldDisabled = false;

        public float GetCurrentHealth()
        {
            return controller.ShieldStats.MaxEnergy - CurrentDamageSustained;
        }

        public AllConstruct GetC()
        {
            return controller.GetC();
        }

        public AdvShieldHandler(AdvShieldProjector controller)
        {
            this.controller = controller;
            Shape = new Elipse(controller);
        }

        [ExtraThread("Should be callable from extra thread")]
        public void ApplyDamage(IDamageDescription DD)
        {
            TimeSinceLastHit = Time.time;

            //Console.WriteLine(DD.GetType().ToString());

            AdvShieldStatus stats = controller.ShieldStats;

            float damage = DD.CalculateDamage(stats.ArmorClass, GetCurrentHealth(), controller.GameWorldPosition);
            CurrentDamageSustained += damage * controller.SurfaceFactor;

            float magnitude;
            Vector3 hitPosition;

            if (DD is ExplosionDamageDescription)
            {
                ExplosionDamageDescription expDD = DD as ExplosionDamageDescription;
                magnitude = expDD.Radius;
                hitPosition = expDD.Position - controller.GameWorldPosition;
            }
            else if (DD is ApplyDamageCallback)
            {
                ApplyDamageCallback adc = (ApplyDamageCallback)DD;
                magnitude = Traverse.Create(adc).Field("_radius").GetValue<float>();
                hitPosition = GridcastHit - controller.GameWorldPosition;
            }
            else
            {
                magnitude = damage / 300;
                hitPosition = GridcastHit - controller.GameWorldPosition;
            }
            float maxEnergy = stats.MaxEnergy;
            float Energy = stats.Energy;
            if (CurrentDamageSustained >= maxEnergy)
            {
                ShieldDisabled = true;
                CurrentDamageSustained = maxEnergy;
                controller.ShieldData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            //CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
        }
        public void ApplyEmpDamage(EmpDamageDescription dd)
        {
            TimeSinceLastHit = Time.time;

            AdvShieldStatus stats = controller.ShieldStats;
            float empdamage = dd.CalculateEmpDamage(GetCurrentHealth(), controller.ShieldStats.ShieldEmpSusceptibility, controller.ShieldStats.ShieldEmpResistivity, controller.ShieldStats.ShieldEmpDamageFactor);
            CurrentDamageSustained += empdamage * controller.SurfaceFactor;

            float magnitude;
            Vector3 hitPosition;

            magnitude = empdamage / 300;
            hitPosition = GridcastHit - controller.GameWorldPosition;

            float maxEnergy = stats.MaxEnergy;
            float Energy = stats.Energy;
            if (CurrentDamageSustained >= maxEnergy)
            {
                CurrentDamageSustained = maxEnergy;
                controller.ShieldData.IsShieldOn.Us = enumShieldDomeState.Off;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            //CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
        }
        public void CreateAnimation(Vector3 worldHit, float magnitude, Color color)
        {
            AdvLogger.LogInfo("Get ready to crash the game!");
            //AdvLogger.LogInfo($"{controller.GameWorldPosition}");
            //We need to re-write this if possible
            //Vector3 shieldSpawnLocation = new Vector3(controller.ShieldDome.transform);
            GameObject obj = UnityEngine.Object.Instantiate(StaticStorage.HitEffectObject, controller.ShieldDome.transform, false);
            AdvLogger.LogInfo("0");
            //obj.transform.position = controller.GameWorldPosition;
            AdvLogger.LogInfo("1");
            //obj.transform.rotation = controller.GameWorldRotation;
            AdvLogger.LogInfo("2");
            //obj.transform.localPosition = Transforms.LocalToGlobal(Vector3.zero, controller.GameWorldPosition, controller.GameWorldRotation);
            AdvLogger.LogInfo("3");
            //obj.transform.localRotation = Transforms.LocalRotationToGlobalRotation(Quaternion.identity, controller.GameWorldRotation);
            AdvLogger.LogInfo("4");
            AdvLogger.LogInfo($"Shield's position is {controller.ShieldDome.transform.position}");
            HitEffectBehaviour behaviour = obj.GetComponent<HitEffectBehaviour>();
            behaviour.Initialize(worldHit, color, magnitude, 1.5f, controller.ShieldDome.transform);
        }

            // Existing code...
            public void Update(AdvShieldStatus ShieldStats)
            {
            if (CurrentDamageSustained == 0.0f) return;
            if ((CurrentDamageSustained <= 0.0f) && (controller.ShieldData.IsShieldOn.Us == enumShieldDomeState.On))
            {
                PassiveRegenText = "The shield is at full health, it is not regenerating.";
                CurrentDamageSustained = 0.0f;
            }

            if ((CurrentDamageSustained > 0.0f) && (controller.ShieldData.IsShieldOn.Us == enumShieldDomeState.On))
            {
                if (ShieldDisabled) return;
                CurrentDamageSustained -= ShieldStats.PassiveRegen / 70 * Time.timeScale;
                AmountPassivelyRegenerated += ShieldStats.PassiveRegen / 70 * Time.timeScale;
                PassiveRegenText = "Shield is using significant engine power to passively regenerate the shield";
            }
            if (Time.time - TimeSinceLastHit < ShieldStats.WaitTime) return;
            DomeShieldNode shieldNode = controller.ConnectShieldNode;
            if (shieldNode == null) return;
            //            if (laserNode.HasToWaitForCharge()) return;
            if ((CurrentDamageSustained / ShieldStats.MaxEnergy >= (controller.ShieldData.ShieldReactivationPercent / 100)) && (controller.ShieldData.IsShieldOn.Us == enumShieldDomeState.Off))
            {
                controller.ShieldData.IsShieldOn.Us = enumShieldDomeState.On;
                CurrentDamageSustained = controller.ShieldData.ShieldReactivationPercent / 100;
                AmountPassivelyRegenerated = 0;
                ShieldDisabled = false;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            /*if ((CurrentDamageSustained / ShieldStats.MaxEnergy <= 0.999f) && (controller.ShieldData.Type.Us == enumShieldDomeState.On))
            {
                CurrentDamageSustained -= ShieldStats.PassiveRegen;
            }
            */
            //Added this^^
            DomeShieldRequestReturn continuousReturn = shieldNode.GetCWEnergyAvailable(true);
            //We need to change this to be exclusively continuous. Pulsed will no longer happen
            if /*((CurrentDamageSustained>0.0f)&&(Time.time - TimeSinceLastHit < ShieldStats.WaitTime))*/(continuousReturn.WorthFiring)
            {
                CurrentDamageSustained -=continuousReturn.BaseHealing;
            }

            /*bool AllowPassiveRegen = (CurrentDamageSustained > 0.0f);

            if (AllowPassiveRegen == true)
            {
                CurrentDamageSustained -=ShieldStats.PassiveRegen;
            }*/
            if (CurrentDamageSustained <= 0.0f)
            {
                controller.ShieldData.IsShieldOn.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
                ShieldDisabled = false;
                controller.ShieldDome.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
            /*if (CurrentDamageSustained == 0.0f)
            {
                controller.ShieldData.Type.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
            }*/
        }
    }
}