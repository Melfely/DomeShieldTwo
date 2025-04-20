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
                CurrentDamageSustained = maxEnergy;
                controller.ShieldData.IsShieldOn.Us = enumShieldDomeState.Off;
            }

            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
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
            }

            float remainingHealthFraction = Mathf.Clamp01((maxEnergy - CurrentDamageSustained) / maxEnergy);
            Color hitColor = Color.Lerp(Color.red, Color.green, remainingHealthFraction);
            CreateAnimation(hitPosition, Mathf.Max(magnitude, 1), hitColor);
        }
        public void CreateAnimation(Vector3 worldHit, float magnitude, Color color)
        {
            GameObject obj = UnityEngine.Object.Instantiate(StaticStorage.HitEffectObject, controller.ShieldDome.transform, false);
            HitEffectBehaviour behaviour = obj.GetComponent<HitEffectBehaviour>();
            behaviour.Initialize(worldHit, color, magnitude, 1.5f);
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
                CurrentDamageSustained -= ShieldStats.PassiveRegen / 70 * Time.timeScale;
                AmountPassivelyRegenerated += ShieldStats.PassiveRegen / 70 * Time.timeScale;
                PassiveRegenText = "Shield is using significant engine power to passively regenerate the shield";
            }
            if (Time.time - TimeSinceLastHit < ShieldStats.WaitTime) return;
            LaserNode laserNode = controller.ConnectLaserNode;
            if (laserNode == null) return;
            //            if (laserNode.HasToWaitForCharge()) return;
            if ((CurrentDamageSustained / ShieldStats.MaxEnergy >= (controller.ShieldData.ShieldReactivationPercent / 100)) && (controller.ShieldData.IsShieldOn.Us == enumShieldDomeState.Off))
            {
                controller.ShieldData.IsShieldOn.Us = enumShieldDomeState.On;
                CurrentDamageSustained = controller.ShieldData.ShieldReactivationPercent / 100;
            }
            /*if ((CurrentDamageSustained / ShieldStats.MaxEnergy <= 0.999f) && (controller.ShieldData.Type.Us == enumShieldDomeState.On))
            {
                CurrentDamageSustained -= ShieldStats.PassiveRegen;
            }
            */
            //Added this^^
            LaserRequestReturn continuousReturn = laserNode.GetCWEnergyAvailable(true);
            LaserRequestReturn pulsedReturn = laserNode.GetPulsedEnergyAvailable(true);
            if /*((CurrentDamageSustained>0.0f)&&(Time.time - TimeSinceLastHit < ShieldStats.WaitTime))*/(continuousReturn.WorthFiring)
            {
                CurrentDamageSustained -=continuousReturn.BaseDamage;
            }

            if /*((CurrentDamageSustained > 0.0f)&&(Time.time - TimeSinceLastHit < ShieldStats.WaitTime))*/(pulsedReturn.WorthFiring)
            {
                CurrentDamageSustained -=pulsedReturn.BaseDamage;
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
            }
            /*if (CurrentDamageSustained == 0.0f)
            {
                controller.ShieldData.Type.Us = enumShieldDomeState.On;
                CurrentDamageSustained = 0.0f;
            }*/
        }
    }
}