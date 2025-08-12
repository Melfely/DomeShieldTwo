using AdvShields.Models;
using BrilliantSkies.Common.Explosions;
using BrilliantSkies.Core.Pooling;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ftd.Constructs.Modules.All.EMP;
using BrilliantSkies.Ftd.Constructs.Modules.All.StandardExplosion;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.Ftd.Missiles;
using BrilliantSkies.Ftd.Missiles.Blueprints;
using BrilliantSkies.Ftd.Missiles.Components;
using BrilliantSkies.Blocks.MissileComponents;
using BrilliantSkies.GridCasts;
using HarmonyLib;
using BrilliantSkies.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Game.Pools.SpecialExplosionEffects;
using BrilliantSkies.Constructs;
using BrilliantSkies.Ftd.Planets.World.Distances;
using AdvShields;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Core.Id;
using System.Runtime.CompilerServices;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Modding;
using BrilliantSkies.Ftd.AdvancedCannons;
using BrilliantSkies.Blocks.Shields.Ui;
using BrilliantSkies.Ftd.Constructs.Modules.Main.Scuttling;
using BrilliantSkies.Ftd.Modes.MainMenu.Ui;
using DomeShieldTwo.shieldblocksystem;
namespace AdvShields
{
    public static class CIL_Control
    {
        public static int Search(List<CodeInstruction> codes, List<string> searchList)
        {
            AdvLogger.LogWarning("Entering Search", LogOptions._AlertDevInGame);
            int maxCount = codes.Count - searchList.Count;
            int targetIndex = -1;

            for (int i = 0; i < maxCount; i++)
            {
                int count = 0;

                foreach (string str in searchList)
                {
                    if (codes[i + count].ToString() == str)
                    {
                        ++count;
                    }
                    else
                    {
                        count = 0;
                        break;
                    }
                }

                if (count > 0)
                {
                    targetIndex = i;
                    break;
                }
            }
            AdvLogger.LogWarning("Exiting Search", LogOptions._AlertDevInGame);
            return targetIndex;
        }
    }
    [HarmonyPatch(typeof(MainMenuUi), "LateUpdateWhenActive", new Type[] {})]

    internal class PleaseWorkPatch
    {
        private static void Prefix()
        {
            if (!StaticStorage.HasLoaded)
            {
                StaticStorage.HasLoaded = true;
                StaticStorage.LoadAsset();
            }
        }
    }

    [HarmonyPatch(typeof(ExplosionExtras), "ExplodeNearbyObjects", new Type[] { typeof(Vector3), typeof(float), typeof(float), typeof(IDamageLogger), typeof(bool) })]
    internal class ExplosionOnMainConstructPatch
    {
        private static void Postfix(ref Vector3 position, ref float damage, ref float radius, ref IDamageLogger gunner, ref bool damageMissiles)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.IsShieldOn == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();


                if (elipse.CheckIntersection(position, radius))
                {
                    new ApplyDamageCallback(item.ShieldHandler, position, radius, damage * 10, gunner).Enqueue();
                }

            }
        }
    }

    [HarmonyPatch(typeof(ConstructSets), "LinkUpExternallyPriorToBlocksInitialising", new Type[] {})]
    internal class ConstructSetsPatch
    {
        private static void Postfix(ConstructSets __instance)
        {
            DomeShieldNodeSet DSNodeSetToAdd = new DomeShieldNodeSet(__instance._construct);
            __instance.DictionaryOfAllSets.Add<DomeShieldNodeSet>(DSNodeSetToAdd);
        }
    }

    [HarmonyPatch(typeof(ObjectCasting), "ShieldsQuick", new Type[] { typeof(GridCastReturn), typeof(Func<ShieldProjector, bool>) })]
    internal class ShieldsQuickPatch
    {
        private static void Postfix(ref GridCastReturn results)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.IsShieldOn == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                //elipse.UpdateInfo();

                bool hitSomething = elipse.CheckIntersection(results.Position, results.Direction, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitSomething) continue;
                float range = (results.Position - hitPointIn).magnitude;
                if (range > results.Range) continue;


                item.ShieldHandler.GridcastHit = hitPointIn;
                IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
                Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);

                GridCastHit hit = GridHitPool.Pool.Acquire();
                hit.Setup(hitPointInLocal, allConstructBlock.GameObject, range, HitSource.Block, results.Direction);
                hit.DamageableObject = item.ShieldHandler;
                hit.OutPointGlobal = hitPointIn;
                hit.From = BarrierCondition.Unknown;
                //item.PlayShieldHit(hitPointIn);
                //commented out
                results.AddAndSort(hit);
            }
        }
    }
    //original one
    [HarmonyPatch(typeof(ProjectileCastingSystem), "Cast", new Type[] { typeof(ProjectileImpactState), typeof(ISettablePositionAndRotation), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]
    internal class ProjectileCastingSystem_Cast_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<string> searchList = new List<string>()
            {
                "call static ActivePlayAreaCalculator ActivePlayAreaCalculator::get_Instance()",
                "ldarg.s 4",
                "ldarg.s 5",
                "ldarg.s 6",
                "ldc.r4 4",
            };

            List<CodeInstruction> codes = instructions.ToList();
            int targetIndex = CIL_Control.Search(codes, searchList);

            if (targetIndex != -1)
            {
                codes[targetIndex + 4] = new CodeInstruction(OpCodes.Ldc_R4, 1500f);
            }
            return codes.AsEnumerable();
        }
    }
    /*
     //original
    [HarmonyPatch(typeof(ProjectileCastingSystem), "Cast", new Type[] { typeof(ProjectileImpactState), typeof(ISettablePositionAndRotation), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]
    internal class ProjectileCastingSystem_Cast_CramPostfix
    {
        private static void Postfix(ref ProjectileImpactState pState, ref Vector3 newPosition, ref Vector3 currentPosition, ref Vector3 normalisedDirection, ref float distance, ref Vector3 velocity)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.IsShieldOn == enumShieldDomeState.Off) continue;
                ///float num21 = _advPooledProjectile._shellModel.ExplosiveCharges.GetEmpDamage();
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                float empDamage = pState.EmpDamage;
                //    float explosiveDamage = _shellModel.ExplosiveCharges.GetExplosionDamage();
                //    float explosiveRadius = _shellModel.ExplosiveCharges.GetExplosionRadius();
                float fireFuel = pState.FireFuel;
                float fireIntensity = pState.FireIntensity;
                float fireDamage = fireFuel * fireIntensity;
                bool hitShield = elipse.CheckIntersection(currentPosition, normalisedDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;

                float range = (currentPosition - hitPointIn).magnitude;
                if (range > distance + 1) continue;

                //DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Plasma, GAME_STATE.MyTeam);

                if (fireDamage > 0)
                {
                    item.ShieldHandler.ApplyDamage(new FireDamageDescription(pState.Gunner, fireDamage, fireFuel));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage), DamageType.Fire, GAME_STATE.MyTeam);
                }
                if (empDamage > 0)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(pState.Gunner, empDamage/2));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage/2), DamageType.Emp, GAME_STATE.MyTeam);
                }
            }
        }
    }
    */
    /*
    [HarmonyPatch(typeof(ProjectileCastingSystem), "CastMe", new Type[] { typeof(ProjectileImpactState), typeof(ISettablePositionAndRotation), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]
    internal class ProjectileCastingSystem_CastMe_APSPostfix
    {
        private static void Postfix(ref ProjectileImpactState pState, ref Vector3 newPosition, ref Vector3 currentPosition, ref Vector3 normalisedDirection, ref float distance, ref Vector3 velocity)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;
                ///float num21 = _advPooledProjectile._shellModel.ExplosiveCharges.GetEmpDamage();
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                float empDamage = pState.EmpDamage;
                //    float explosiveDamage = _shellModel.ExplosiveCharges.GetExplosionDamage();
                //    float explosiveRadius = _shellModel.ExplosiveCharges.GetExplosionRadius();
                float fireFuel = pState.FireFuel;
                float fireIntensity = pState.FireIntensity;
                float fireDamage = fireFuel * fireIntensity;
                bool hitShield = elipse.CheckIntersection(currentPosition, normalisedDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;

                float range = (currentPosition - hitPointIn).magnitude;
                if (range > distance + 1) continue;

                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Plasma, GAME_STATE.MyTeam);

                if (fireDamage > 0)
                {
                    item.ShieldHandler.ApplyDamage(new FireDamageDescription(pState.Gunner, fireDamage, fireFuel));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage), DamageType.Fire, GAME_STATE.MyTeam);
                }
                if (empDamage > 0)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(pState.Gunner, empDamage / 2));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage / 2), DamageType.Emp, GAME_STATE.MyTeam);
                }
            }
        }
    }
    */
    /*
    [HarmonyPatch(typeof(ProjectileCastingSystem), "Cast", new Type[] { typeof(ProjectileImpactState), typeof(ISettablePositionAndRotation), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]
    internal class ProjectileCastingSystem_Cast_APSPostfix
    {
        private static void Postfix(ref ProjectileImpactState pState, ref Vector3 newPosition, ref Vector3 currentPosition, ref Vector3 normalisedDirection, ref float distance, ref Vector3 velocity)
        {

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;
                ///float num21 = _advPooledProjectile._shellModel.ExplosiveCharges.GetEmpDamage();
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                float empDamage = pState.EmpDamage;  //_shellModel.ExplosiveCharges.GetEmpDamage();
                //    float explosiveDamage = _shellModel.ExplosiveCharges.GetExplosionDamage();
                //    float explosiveRadius = _shellModel.ExplosiveCharges.GetExplosionRadius();
                float fireFuel = pState.FireFuel;  //_shellModel.ExplosiveCharges.GetIncendiaryFuel();
                float fireIntensity = pState.FireIntensity; //_shellModel.ExplosiveCharges.GetIncendiaryIntensity();
                float fireDamage = fireFuel * fireIntensity;
                bool hitShield = elipse.CheckIntersection(currentPosition, normalisedDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;

                float range = (currentPosition - hitPointIn).magnitude;
                if (range > distance + 1) continue;

                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Plasma, GAME_STATE.MyTeam);

                if (fireDamage > 0)
                {
                    item.ShieldHandler.ApplyDamage(new FireDamageDescription(pState.Gunner, fireDamage, fireFuel));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage), DamageType.Fire, GAME_STATE.MyTeam);
                }
                if (empDamage > 0)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(pState.Gunner, empDamage / 2));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage / 2), DamageType.Emp, GAME_STATE.MyTeam);
                }
            }
        }
    }
    */
    /*
    [HarmonyPatch(typeof(AdvPooledProjectile), "ShieldHit", new Type[] { typeof(GridCastHit) })]

    internal class AdvPooledProjectileShieldHitPatch
    {
        private static void PostFix(AdvPooledProjectile __instance, ShellModel __instances, ref GridCastHit hit)
        {
            AdvPooledProjectile _advPooledProjectile = Traverse.Create(__instance).Field("_advPooledProjectile").GetValue<AdvPooledProjectile>();
            ShellModel _shellModel = Traverse.Create(__instances).Field("_shellModel").GetValue<ShellModel>();

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;
                ///float num21 = _advPooledProjectile._shellModel.ExplosiveCharges.GetEmpDamage();
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                Vector3 NewDirection = _advPooledProjectile.Velocity * Time.fixedDeltaTime;
                AdvShieldProjector advShieldProjector = hit.BlockHit as AdvShieldProjector;
                float empDamage = _shellModel.ExplosiveCharges.GetEmpDamage();
                //    float explosiveDamage = _shellModel.ExplosiveCharges.GetExplosionDamage();
                //    float explosiveRadius = _shellModel.ExplosiveCharges.GetExplosionRadius();
                float fireFuel = _shellModel.ExplosiveCharges.GetIncendiaryFuel();
                float fireIntensity = _shellModel.ExplosiveCharges.GetIncendiaryIntensity();
                float fireDamage = fireFuel * fireIntensity;
                bool hitSomething = advShieldProjector != null;
                if (!hitSomething) continue;
                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Particle, GAME_STATE.MyTeam);
                bool hitShield = elipse.CheckIntersection(_advPooledProjectile.Position, NewDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;

                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Laser, GAME_STATE.MyTeam);

                if (fireDamage > 0)
                {
                    item.ShieldHandler.ApplyDamage(new FireDamageDescription(_advPooledProjectile.Gunner, fireDamage, fireFuel));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage + 10000), DamageType.Fire, GAME_STATE.MyTeam);
                }
                if (empDamage > 0)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(_advPooledProjectile.Gunner, empDamage));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage + 10000), DamageType.Emp, GAME_STATE.MyTeam);
                }
            }
        }
    }
    */
    /*
    [HarmonyPatch(typeof(AdvPooledProjectile), "ActivateHere", new Type[] { typeof(Vector3), typeof(Vector3), typeof(ShellModel) })]

    internal class AdvPooledProjectileActivatedPatch
    {
        private static void PostFix(AdvPooledProjectile __instance, ref Vector3 position, ref Vector3 velocityVector, ref ShellModel shellModel)
        {
            AdvPooledProjectile _advPooledProjectile = Traverse.Create(__instance).Field("_advPooledProjectile").GetValue<AdvPooledProjectile>();
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                Vector3 Direction = velocityVector * Time.fixedDeltaTime;

                float empDamage = shellModel.ExplosiveCharges.GetEmpDamage();
                float fireFuel = shellModel.ExplosiveCharges.GetIncendiaryFuel();
                float fireIntensity = shellModel.ExplosiveCharges.GetIncendiaryIntensity();
                float fireDamage = fireFuel * fireIntensity;
                float magnitude = velocityVector.magnitude;
                bool hitShield = elipse.CheckIntersection(position, Direction, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;

                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Laser, GAME_STATE.MyTeam);

                float range = (position - hitPointIn).magnitude;
                if (range > Direction.magnitude * 2) continue;

                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Particle, GAME_STATE.MyTeam);
                _advPooledProjectile.Deactivate(true);
            }
        }
    }
    */
    /*
    [HarmonyPatch(typeof(AdvPooledProjectile), "Explode", new Type[] { typeof(IAllConstructBlock), typeof(Block), typeof(Func<Vector3>), typeof(ExplosionReason), typeof(GridCastHit) })]

    internal class AdvPooledProjectilePatch
    {
        private static void Postfix(AdvPooledProjectile __instance, ShellModel __instances)
        {
            AdvPooledProjectile _advPooledProjectile = Traverse.Create(__instance).Field("_advPooledProjectile").GetValue<AdvPooledProjectile>();
            ShellModel _shellModel = Traverse.Create(__instances).Field("_shellModel").GetValue<ShellModel>();

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
                {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;
                ///float num21 = _advPooledProjectile._shellModel.ExplosiveCharges.GetEmpDamage();
                Elipse elipse = item.ShieldHandler.Shape;
                Vector3 NewDirection = _advPooledProjectile.Velocity * Time.fixedDeltaTime;
                elipse.UpdateInfo();
                float empDamage = _shellModel.ExplosiveCharges.GetEmpDamage();
                //    float explosiveDamage = _shellModel.ExplosiveCharges.GetExplosionDamage();
                //    float explosiveRadius = _shellModel.ExplosiveCharges.GetExplosionRadius();
                float fireFuel = _shellModel.ExplosiveCharges.GetIncendiaryFuel();
                float fireIntensity = _shellModel.ExplosiveCharges.GetIncendiaryIntensity();
                float fireDamage = fireFuel * fireIntensity;
                bool hitShield = elipse.CheckIntersection(_advPooledProjectile.FastPosition, NewDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;
                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Plasma, GAME_STATE.MyTeam);
                /*   if (explosiveDamage > 0 && hitShield)
                   {
                       item.ShieldHandler.ApplyDamage(new ExplosionDamageDescription(_advPooledProjectile.Gunner, explosiveDamage, explosiveRadius, _advPooledProjectile.Position, false, true));
                       DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(explosiveDamage), DamageType.Explosive, GAME_STATE.MyTeam);
                   }
                */
    /*
                if (fireDamage > 0)
                    {
                        item.ShieldHandler.ApplyDamage(new FireDamageDescription(_advPooledProjectile.Gunner, fireDamage, fireFuel));
                        DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage + 10000), DamageType.Fire, GAME_STATE.MyTeam);
                    }
                    if (empDamage > 0)
                    {
                        item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(_advPooledProjectile.Gunner, empDamage));
                        DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage + 10000), DamageType.Emp, GAME_STATE.MyTeam);
                    }
                }
        }
    }
    */
    /*
    [HarmonyPatch(typeof(PooledCramProjectile), "Explode", new Type[] { typeof(IAllConstructBlock), typeof(Block), typeof(Func<Vector3>), typeof(ExplosionReason), typeof(GridCastHit) })]

    internal class PooledCramProjectilePatch
    {
        private static void Postfix(PooledCramProjectile __instance)
        {

            PooledCramProjectile _pooledCramProjectile = Traverse.Create(__instance).Field("_pooledCramProjectile").GetValue<PooledCramProjectile>();
            //ShellModel _shellModel = Traverse.Create(__instance).Field("_ShellModel").GetValue<ShellModel>();

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                float empDamage = _pooledCramProjectile._pState.EmpDamage;
                float explosiveDamage = _pooledCramProjectile._pState.ExplosiveDamage;
                float explosiveRadius = _pooledCramProjectile._pState.ExplosiveRadius;
                float fireDamage = _pooledCramProjectile._pState.FireFuel * _pooledCramProjectile._pState.FireIntensity;
                IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
                bool hitShield = elipse.CheckIntersection(_pooledCramProjectile.Position, _pooledCramProjectile.Velocity, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitShield) continue;
                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(10000f), DamageType.Plasma, GAME_STATE.MyTeam);
                if (explosiveDamage > 0)
                {
                    item.ShieldHandler.ApplyDamage(new ExplosionDamageDescription(_pooledCramProjectile.Gunner, explosiveDamage, explosiveRadius, _pooledCramProjectile.Position, false, true));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(explosiveDamage), DamageType.Explosive, GAME_STATE.MyTeam);
                }
                if (fireDamage > 0)
                {
                    item.ShieldHandler.ApplyDamage(new FireDamageDescription(_pooledCramProjectile.Gunner, fireDamage, _pooledCramProjectile._pState.FireFuel));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage+10000), DamageType.Fire, GAME_STATE.MyTeam);
                    //C.iMain.FireRestricted.DelayedCreateFire(base.Gunner, C, localCell, num2, incendiaryIntensity, num4, 10);
                }
                if (empDamage > 0)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(_pooledCramProjectile.Gunner, empDamage));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage+10000), DamageType.Emp, GAME_STATE.MyTeam);
                    
                }
            }
        }
    }
    */
    [HarmonyPatch(typeof(PlasmaProjectileCastingSystem), "DoCast", new Type[] { typeof(PooledPlasmaProjectile), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]

        internal class PlasmaProjectilePoolPatch
        {
            //private static Type __instance;

            private static void Postfix(ref PooledPlasmaProjectile projectile, ref Vector3 newPosition, ref Vector3 currentPosition, ref Vector3 normalisedDirection, ref float distance, ref Vector3 velocity, ref int debugId, ref Color projectileDebugColor)
            {
                //PooledPlasmaProjectile _pooledPlasmaProjectile = Traverse.Create(__instance).Field("_pooledPlasmaProjectile").GetValue<PooledPlasmaProjectile>();

                foreach (AdvShieldProjector item in TypeStorage.GetObjects())
                {
                    if (item.ShieldData.IsShieldOn == enumShieldDomeState.Off) continue;

                    Elipse elipse = item.ShieldHandler.Shape;
                    elipse.UpdateInfo();

                    bool hitSomething = elipse.CheckIntersection(currentPosition, normalisedDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
                    if (!hitSomething) continue;

                    float range = (currentPosition - hitPointIn).magnitude;
                    if (range > distance + 50) continue;

                    item.ShieldHandler.ApplyDamage(new PlasmaDamageDescription(projectile.Gunner, projectile.GetCurrentDamage(), projectile.ArmorPiercing, hitPointIn));
                    projectile.Deactivate(false);

                    item.ShieldHandler.GridcastHit = hitPointIn;

                    IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
                    Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);



                }
            }
        }
        // Existing code...

   [HarmonyPatch(typeof(FlamerProjectileCastingSystem), "DoCast", new Type[] { typeof(PooledFlamerProjectile), typeof(Vector3), typeof(Vector3), typeof(float) })]

   internal class FlamerProjectilePoolPatch
   {
       private static void Postfix(ref PooledFlamerProjectile projectile, ref Vector3 startPosition, ref Vector3 normalisedDirection, ref float distance)
       { 
       foreach (AdvShieldProjector item in TypeStorage.GetObjects())
           {
               if (item.ShieldData.IsShieldOn == enumShieldDomeState.Off) continue;

               Elipse elipse = item.ShieldHandler.Shape;
               elipse.UpdateInfo();

               bool hitSomething = elipse.CheckIntersection(startPosition, normalisedDirection, out Vector3 hitPointIn, out Vector3 hitNormal);
               if (!hitSomething) continue;

               float range = (startPosition - hitPointIn).magnitude;
               if (range > distance + 50) continue;

        item.ShieldHandler.ApplyDamage(new FireDamageDescription(projectile.Gunner, projectile.Fuel, projectile.Intensity*4));
                //IDamageable damageableObject = (IDamageable)item.ShieldHandler.Shape;
                projectile.Deactivate(false);
                DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(projectile.Fuel * projectile.Intensity * 4), DamageType.Fire, GAME_STATE.MyTeam);

               item.ShieldHandler.GridcastHit = hitPointIn;

               IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
               Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);
           }
        }
   }

    [HarmonyPatch(typeof(MissileImpactAndTriggering), "HandleHits", new Type[] { typeof(Vector3) })]
        internal class MissileImpactAndTriggering_HandleHits_Patch
        {
            private static void Postfix(MissileImpactAndTriggering __instance)
            {
                Missile _missile = Traverse.Create(__instance).Field("_missile").GetValue<Missile>();

                foreach (AdvShieldProjector item in TypeStorage.GetObjects())
                {
                    if (item.ShieldData.IsShieldOn == enumShieldDomeState.Off) continue;

                    Elipse elipse = item.ShieldHandler.Shape;
                    elipse.UpdateInfo();

                    Vector3 Direction = _missile.Velocity * Time.fixedDeltaTime;

                    bool hitSomething = elipse.CheckIntersection(_missile.NosePosition, Direction, out Vector3 hitPointIn, out Vector3 hitNormal);
                    if (!hitSomething) continue;

                    float range = (_missile.NosePosition - hitPointIn).magnitude;
                    if (range > Direction.magnitude * 2) continue;

                    // =============== Safety Fuse and Shields by Nicholas Zonenberg ================================

                    // Bool for checking if the missile has a safety fuse
                    bool hasSafetyFuse = false;

                    // go through the whole missile checking each component
                    foreach (MissileComponent missileComponent in _missile.Blueprint.Components)
                    {
                        //enum holding all the comonents
                        enumMissileComponentType componentType = missileComponent.componentType;
                        // 91 is the enum for the safety fuse
                        if (componentType == (enumMissileComponentType)91)
                        {
                            hasSafetyFuse = true;
                            break;
                        }
                    }

                    //if there's a safety fuse check that this vehicle launched it
                    if (hasSafetyFuse)
                    {
                        // the construct the missile is hitting. ( This might need to also check sub constructs but I don't even know if you can put a shield on a sub construct and if you can I have no idea why you would)
                        IMainConstructBlock constructable = item.GetConstructable();
                        //Missile missile = .Missile;
                        // obj that the shield belongs too
                        object obj;
                        if (_missile == null)
                        {
                            obj = null;
                        }
                        else
                        {
                            // get the missilenode ( missile launcher ) the missile came from
                            MissileNode missileNode = _missile.MissileNode;
                            // check if the missile node is on this construct
                            obj = ((missileNode != null) ? missileNode.MainConstruct : null);
                        }
                        // if the missile launcher is on the construct do not hit the shield
                        if (constructable == obj) continue;
                    }

                    // =============== End Safety Fuses and Shields

                    item.ShieldHandler.GridcastHit = hitPointIn;

                    IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
                    Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);

                    float speed = (_missile.Velocity - allConstructBlock.Velocity).magnitude;
                    float damage = _missile.Blueprint.Warheads.ThumpPerMs * speed * _missile.GetHealthDependency(HealthDependency.Damage);
                    float thumpAP = _missile.Blueprint.Warheads.ThumpAP;
                    float empDamage = _missile.Blueprint.Warheads.EMPDamage;
                    float fireFuel =   _missile.Blueprint.Warheads.FireFuel;
                    float fireIntensity = _missile.Blueprint.Warheads.FireIntensity;
                    float fireOxidizer = _missile.Blueprint.Warheads.FireOxidizer;
                    float fireDamage = fireFuel * (fireIntensity/20);
                item.ShieldHandler.ApplyDamage(new KineticDamageDescription(_missile.Gunner, damage, thumpAP, true));
                    //item.ShieldHandler.ApplyDamage(new EmpDamageDescription(_missile.Gunner, empDamage));
                    if (empDamage > 0)
                    {
                        item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(_missile.Gunner, empDamage));
                        DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage), DamageType.Emp, GAME_STATE.MyTeam);
                    }
                

                    if (fireFuel > 0)
                    {
                        item.ShieldHandler.ApplyDamage(new FireDamageDescription(_missile.Gunner, fireDamage, fireFuel));
                        DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage), DamageType.Fire, GAME_STATE.MyTeam);
                    }
                    bool safetyOn = !__instance.CheckSafety(item) || !_missile.HasClearedVehicle;
                    _missile.MoveNoseIntoPosition(hitPointIn);
                    _missile.ExplosionHandler.ExplodeNow(allConstructBlock, hitPointInLocal, hitPointIn, safetyOn);
                }
            }
        }
    /*
    [HarmonyPatch(typeof(PooledProjectile), "Explode", new Type[] { typeof(IAllConstructBlock), typeof(Block), typeof(Func<Vector3>), typeof(ExplosionReason), typeof(GridCastHit) })]

    internal class PooledProjectilePatch
    {
        private static void Postfix(PooledProjectile __instance)
        {

            PooledProjectile _pooledProjectile = Traverse.Create(__instance).Field("_pooledProjectile").GetValue<PooledProjectile>();

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                float empDamage = _pooledProjectile._pState.EmpDamage;
                float explosiveDamage = _pooledProjectile._pState.ExplosiveDamage;
                float explosiveRadius = _pooledProjectile._pState.ExplosiveRadius;
                float fireDamage = _pooledProjectile._pState.FireFuel * _pooledProjectile._pState.FireIntensity;
                bool hitShield = elipse.CheckIntersection(_pooledProjectile.Position, _pooledProjectile.Velocity, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (explosiveDamage > 0 && hitShield)
                {
                    item.ShieldHandler.ApplyDamage(new ExplosionDamageDescription(_pooledProjectile.Gunner, explosiveDamage, explosiveRadius, _pooledProjectile.Position, false, true));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(explosiveDamage), DamageType.Explosive, GAME_STATE.MyTeam);
                }
                if (fireDamage > 0 && hitShield)
                {
                    item.ShieldHandler.ApplyDamage(new FireDamageDescription(_pooledProjectile.Gunner, fireDamage, fireDamage));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(fireDamage), DamageType.Fire, GAME_STATE.MyTeam);
                }
                if (empDamage > 0 && hitShield)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(_pooledProjectile.Gunner, empDamage));
                    DamageHelp.DisplayDamageMarker(Rounding.FloatToInt(empDamage), DamageType.Emp, GAME_STATE.MyTeam);
                }
                }
            }
        }
*/
    /* [HarmonyPatch(typeof(EmpDamageDescription), "CalculateEmpDamage", new Type[] { typeof(Vector3) })]
     internal class EmpDamageDescription_CalculateEmpDamage_Patch
     { private static void PostFix(EmpDamageDescription __instance)
         {
             foreach (AdvShieldProjector item in TypeStorage.GetObjects())
             {
                 if (item.ShieldData.Type == enumShieldDomeState.On)
                 { empSusceptibility = 1;
                     DamageFactor = 4;
                 }
                 Maybe try having the game figure out what the emp damage of a shell is, and do that x4 to the shield health? And maybe have it remove that much laser energy as well from the 
                 laser system, to lower armour class and force pump recharging (engine power). 
             } */
    /*[HarmonyPatch(typeof(ParticleCannonEffect), "ApplyDamage", new Type[] { typeof(IDamageLogger GR,) typeof(Vector3[] worldPositions) })]
    internal class ParticleCannonEffect_ApplyDamage_Patch
    {
        private static void Postfix(IDamageLogger GR, Vector3[] worldPositions) ;
        {

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())   
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

                if (elipse.CheckIntersection(position, radius))
            {
            bool flag = !ParticleCannonEffect._ourReturn.HitSomething;
            }
        }
    }*/

    /*   [HarmonyPatch(typeof(AdvPooledProjectile), "Expode", new Type[] { typeof(IAllConstructBlock), typeof(Block), typeof(Func<Vector3>), typeof(ExplosionReason), typeof(GridCastHit) })]

       internal class AdvPooledProjectilePatch
       {
           private static void Postfix (ref IAllConstructBlock C, ref Block B, ref Func<Vector3> dynamicPoint, ref ExplosionReason reason, ref GridCastHit hit)
           {
               foreach (AdvShieldProjector item in TypeStorage.GetObjects())
               {
                   if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                   Elipse elipse = item.ShieldHandler.Shape;
                   elipse.UpdateInfo();

                   {
                       // Existing code...

                       if (_shellModel.ExplosiveCharges.GetEmpDamage() > 0f)
                       {
                           float num2 = _shellModel.ExplosiveCharges.GetEmpDamage() * num;
                           ProjectileImpactFusing fusing = _pState.Fusing;
                           if (C2 == null && (fusing.UsePenetrationDepth || fusing.UseTimeFromFirstSurface) && fusing.LastBlockHit != null)
                           {
                               C2 = fusing.LastBlockHit;
                           }
                           if (C2 != null)
                           {
                               Vector3 globalExplosionPoint = ((B == null) ? dynamicPoint() : B.GameWorldPosition);
                               Block block = ProjectileContactEffectHandler.ApplyEmp(num2, C2, globalExplosionPoint, base.Gunner, B);
                               if (block != null && C2.iMain.SettingsRestricted.ShowProjectiles)
                               {
                                   C2.DebugDisplayRestricted.AddDebug(new DebugBoundingBox(B, DebugColors.EmpBlock, base.UniqueId, _locFile.Format("Debug_EmpDeliveryBlockDamage", "EMP delivery block ({0} damage)", num2.ToLargeReadable()))
                                   {
                                       CornerSize = 0.45f,
                                       BoxOffset = -0.1f
                                   });
                               }

                               // **Begin Added Code for Shield Disruption on EMP Hit**
                               // Check if the hit block is a shield
                               ShieldProjector shieldProjector = B as ShieldProjector;
                               float shieldDisruptionMultiplier = _shellModel.ExplosiveCharges.GetShieldDisruptionMultiplier();
                               if (shieldProjector != null && shieldDisruptionMultiplier < 1f)
                               {
                                   shieldProjector.ApplyDisruption(shieldDisruptionMultiplier);
                               }
                               // **End Added Code**
                           }
                           exploded = true;
                       }
                   }
       }
    */
    /*
    [HarmonyPatch(typeof(ProjectileContactEffectHandler), "ApplyEmp", new Type[] { typeof(float), typeof(IAllConstructBlock), typeof(IDamageLogger), typeof(Vector3), typeof(Block), typeof(float) })]

    internal class ProjectileContactEffectHandlerApplyEmpPatch
    {
        private static void Postfix(ref float empDamage, ref IAllConstructBlock construct, ref Vector3 globalExplosionPoint, ref IDamageLogger gunner, ref Block block, ref float maxJumpDistance)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.ShieldData.Type == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

              // item.ShieldHandler.GridcastHit = hitPointIn;

                IAllConstructBlock allConstructBlock = item.GetConstructableOrSubConstructable();
              //         Vector3 hitPointInLocal = allConstructBlock.SafeGlobalToLocal(hitPointIn);

                /*bool hitSomething = elipse.CheckIntersection(globalExplosionPoint, globalExplosionPoint, out Vector3 hitPointIn, out Vector3 hitNormal);
                if (!hitSomething) continue;*/
    /*
                    block = ProjectileContactEffectHandler.GetNearestBlock(maxJumpDistance, allConstructBlock, globalExplosionPoint, item);
                    bool flag4 = item;
                    if (flag4)
                    {
                        construct.EmpRestricted.NewEmpSource(block.LocalPosition, empDamage, gunner);
                        //item.ShieldHandler.ApplyDamage(new EmpDamageDescription(gunner, empDamage * 20));
                    }
    
                    item.ShieldHandler.ApplyDamage(new EmpDamageDescription(gunner, empDamage * 2));
                    return;
    
                }
            }
        }
    */
    /*
[HarmonyPatch(typeof(EmpDamageDescription), "CalculateEmpDamage", new Type[] { typeof(float), typeof(float), typeof(float), typeof(float), })]
internal class EmpDamageDescription_CalculateEmpDamage_Patch
{
    private static void PostFix(ref float currentHealth, ref float empSusceptibility, ref float empResistivity, ref float damageFactor)
    {
        foreach (AdvShieldProjector item in TypeStorage.GetObjects())
        {
            if (item.ShieldData.Type == enumShieldDomeState.On)
            {
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

                currentHealth = 1f;
                empSusceptibility = 1f;
                empResistivity = 0f;
                damageFactor = 4f;
            }
        }
    }
}
*/
}
