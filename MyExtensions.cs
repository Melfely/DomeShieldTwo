using AdvShields;
using AdvShields.Behaviours;
using AdvShields.Models;
using BrilliantSkies.Blocks.AdvCannonComponents.Gui.AmmoCustomising;
using BrilliantSkies.Blocks.MissileComponents;
using BrilliantSkies.Blocks.Shields.Ui;
using BrilliantSkies.Common.Explosions;
using BrilliantSkies.Constructs;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Core.Pooling;
using BrilliantSkies.Core.Threading.Pools;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Types;
//using BrilliantSkies.Core.Units;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Effects.Explosions;
using BrilliantSkies.Effects.Pools.DamageAndDebris;
using BrilliantSkies.Effects.SoundSystem;
using BrilliantSkies.Ftd.AdvancedCannons;
using BrilliantSkies.Ftd.Avatar.Interact;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using BrilliantSkies.Ftd.Constructs.Modules.All.EMP;
using BrilliantSkies.Ftd.Constructs.Modules.All.PlasmaExplosion;
using BrilliantSkies.Ftd.Constructs.Modules.All.StandardExplosion;
using BrilliantSkies.Ftd.Constructs.Modules.Main.Scuttling;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.Ftd.Game.Pools.SpecialExplosionEffects;
using BrilliantSkies.Ftd.Missiles;
using BrilliantSkies.Ftd.Missiles.Blueprints;
using BrilliantSkies.Ftd.Missiles.Components;
using BrilliantSkies.Ftd.Missiles.Editor;
using BrilliantSkies.Ftd.Modes.MainMenu.Ui;
using BrilliantSkies.Ftd.Planets.Instances.SpawnPoints;
using BrilliantSkies.Ftd.Planets.World.Distances;
using BrilliantSkies.GridCasts;
using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Tips;
using DomeShieldTwo.newshieldblocksystem;
using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
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
    [HarmonyPatch(typeof(ExplosionExtras), "DoSingleBlockDamage", new Type[] { typeof(Vector3), typeof(float), typeof(float), typeof(float) })]
    internal class ReworkedExplosionPatch
    {
        private static void Postfix(ref Vector3 position, ref float sqrRadius, ref float radius, ref float damage)
        {
            //AdvLogger.LogInfo($"Are you seeing this? Logging all the things in order (position, sqrRadius, radius, damage): {position}, {sqrRadius}, {radius}, {damage}");
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();


                if (elipse.CheckIntersection(position, radius))
                {
                    //AdvLogger.LogInfo("The dome shield should be hit by this explosion. Is that correct?");
                    //We are seeing this properly!!! Fantastic. Let's try applying damage now.
                    item.ShieldHandler.ApplyHEDamage(damage, position);
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
    [HarmonyPatch(typeof(LaserDamageHelper), "DamageSection")]
    internal class LaserDamageShieldPatch
    {
        private static bool Prefix(LaserDamageHelper __instance, ref Vector3 __result, ref Vector3 p1, ref Vector3 dir, ref LaserRequestReturn info, ref IDamageLogger gunner, ref int surfacesToCheck, out bool shouldContinue)
        {
            shouldContinue = true;
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                /*
                RaycastHit hit;
                Ray ray = new Ray(currentPosition, normalisedDirection);
                */
                Vector3 hitPosition;
                Vector3 hitNormal;
                if (elipse.CheckIntersection(p1, dir, out hitPosition, out hitNormal))
                {
                    float num = 0;
                    shouldContinue = false;
                    float num2 = LaserDamageHelper.TotalSmokeBetweenPoints(p1, hitPosition, ((gunner != null) ? gunner.FactionId : null) ?? ObjectId.AnIdWithNoLinkage);
                    if (num2 > 0)
                    {
                        info.SetSmoke(num2);
                        num = num2;
                    }
                    LaserDamageDescription laserDamageDescription = new LaserDamageDescription(gunner, info.GetDamage(p1, hitPosition), info.Intensity);
                    item.ShieldHandler.ApplyLaserDamage(laserDamageDescription, hitPosition, info.Continuous);
                    __result = hitPosition;
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(StatPanelBase<AmmoCustomise, AmmoCustomise>), "AddTableRow")]
    internal class InterceptAPSDisruptorUI_Patch
    {
        private static float LastEMPValue = 0;
        private static void Prefix(StatPanelBase<AmmoCustomise, AmmoCustomise> __instance, ref ScreenSegmentTable table, ref string name, ref int row, ref string toolTip, ref Func<AmmoCustomise, string> getter)
        {
            if (name.Contains("EMP"))
            {
                var thing = M.m<AmmoCustomise>(getter);
                //AdvLogger.LogInfo(getter.ToString(), LogOptions._AlertDevInGame);
            }
            if (name.Contains("Shield"))
            {
                name = "Planar shield strength reduction";
                toolTip = "A multiplicatively stacking debuff on current planar shield strength. Dome shields are instead affected based on shell EMP damage.";
                string bonusToolTip = "EMP damage is matched against the CURRENT health of a hit dome shield, and applies Disruption based on that ratio. Disruption significantly lowers armor class and passive regen, and blocks the active regen timer from beginning.";
                StringDisplay stringDisplay2 = new StringDisplay(M.m(string.Format("<b>{0}:</b>", "Dome Shield Disruption")), M.m(new ToolTip(bonusToolTip, 500f)));
                StringDisplay stringDisplay3 = new StringDisplay(M.m(string.Format("<b>{0}</b>", "^See EMP damage^")), M.m(new ToolTip(bonusToolTip, 500f)));
                table.Resize(2, 2);
                table.AddInterpretter<StringDisplay>(stringDisplay2, row + 1, 0);
                table.AddInterpretter<StringDisplay>(stringDisplay3, row + 1, 1);
            }
        }
    }
    [HarmonyPatch(typeof(ParticleCannon), "MainThreadClientAndServerFire")]
    internal class ParticleCannonIntercept_Patch
    {
        private static bool Prefix(ParticleCannon __instance, ref int armNumber, ref Vector3 direction, ref float energy, ref int seed, ref IDamageLogger damageLogger)
        {
            //AdvLogger.LogInfo("Are you seeing this?", LogOptions._AlertDevInGame);
            //This all works on a technical side... however, we still see no beam. Tragic.
            /*
            Vector3 vector = BoresightErrors.AdjustForAccuracy(direction, __instance.StabilityExtraInaccuracy);
            bool flag = particleCannonArm.HasPort || particleCannonArm.HasTerminator;
            ParticleCannonEffect particleCannonEffect;
            if (flag)
            {
                particleCannonEffect = R_Blocks.ParticleCannonParticle.InstantiateACopy(__instance.GetFirePoint(0f), Quaternion.LookRotation(vector, __instance.GameWorldUp));
                particleCannonEffect.InaccuracyErrorAt1m = (direction - vector).magnitude;
            }
            else
            {
                ParticleCannonPipe lastPipe = particleCannonArm.LastPipe;
                particleCannonEffect = R_Blocks.ParticleCannonParticle.InstantiateACopy(lastPipe.GameWorldPosition + lastPipe.GameWorldForwards * (float)lastPipe.item.SizeInfo.ArrayPositionsUsed, Quaternion.LookRotation(lastPipe.GameWorldForwards));
            }
            __instance.effectScale = Mathf.Clamp(Interp.TwoPoints(__instance.particleScaleStart, __instance.particleScaleEnd, energy), 0f, 3.5f) * Mathf.Pow(__instance.ReloadTime, 0.2f);
            particleCannonEffect._lineRenderer.widthMultiplier = __instance.effectScale;
            particleCannonEffect.m_BaseColor = __instance.ParticleData.Color;
            particleCannonEffect._secondaryEffectRenderer.widthMultiplier = __instance.effectScale;
            particleCannonEffect._light.range = __instance.effectScale * 10f;
            particleCannonEffect._fireEffects.transform.localScale *= __instance.effectScale;
            particleCannonEffect.HorizontalFocus = __instance.ParticleData.Focus.Us * __instance.HorizontalFocusFactor * ParticleCannonConstants.FocusFactor;
            particleCannonEffect.VerticalFocus = __instance.ParticleData.Focus.Us * __instance.VerticalFocusFactor * ParticleCannonConstants.FocusFactor;
            particleCannonEffect.Attenuation = __instance.Attenuation * particleCannonArm.AttenuationFactor;
            particleCannonEffect.ParticleType = __instance.ParticleData.ParticleType;
            particleCannonEffect.Range0Damage = __instance.GetDamage(energy, armNumber, false);
            particleCannonEffect.DistancePerSegment = ParticleCannonConstants.Range / 100f;
            */
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;
                /*
                if (item.Node.ConnectedCard == null) AdvLogger.LogWarning("Card was null (HOW?!?)", LogOptions._AlertDevInGame);
                AdvLogger.LogInfo($"{item.Node.ConnectedCard}", LogOptions._AlertDevInGame);
                */
                //WHYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
                if (item.Node.ConnectedCard != "Particle") continue;
                //if (item.Node.ConnectedCard != "Particle") continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                /*
                RaycastHit hit;
                Ray ray = new Ray(currentPosition, normalisedDirection);
                */
                Vector3 hitPosition;
                Vector3 hitNormal;
                if (elipse.CheckIntersection(__instance.GetFirePoint(0), direction, out hitPosition, out hitNormal))
                {
                    //AdvLogger.LogInfo("Did we hit a shield?");
                    //Yes, this is amazingly working out.
                    ParticleCannonArm particleCannonArm = __instance.Node.Arms.Arms[armNumber];
                    Vector3 vector = BoresightErrors.AdjustForAccuracy(direction, __instance.StabilityExtraInaccuracy);
                    bool flag = particleCannonArm.HasPort || particleCannonArm.HasTerminator;
                    ParticleCannonEffect particleCannonEffect;
                    if (flag)
                    {
                        particleCannonEffect = R_Blocks.ParticleCannonParticle.InstantiateACopy(__instance.GetFirePoint(0f), Quaternion.LookRotation(vector, __instance.GameWorldUp));
                        particleCannonEffect.InaccuracyErrorAt1m = (direction - vector).magnitude;
                    }
                    else
                    {
                        ParticleCannonPipe lastPipe = particleCannonArm.LastPipe;
                        particleCannonEffect = R_Blocks.ParticleCannonParticle.InstantiateACopy(lastPipe.GameWorldPosition + lastPipe.GameWorldForwards * (float)lastPipe.item.SizeInfo.ArrayPositionsUsed, Quaternion.LookRotation(lastPipe.GameWorldForwards));
                    }
                    particleCannonEffect.Attenuation = __instance.Attenuation * particleCannonArm.AttenuationFactor;
                    /*
                    __instance.effectScale = Mathf.Clamp(Interp.TwoPoints(__instance.particleScaleStart, __instance.particleScaleEnd, energy), 0f, 3.5f) * Mathf.Pow(__instance.ReloadTime, 0.2f);
                    particleCannonEffect._lineRenderer.widthMultiplier = __instance.effectScale;
                    particleCannonEffect.m_BaseColor = __instance.ParticleData.Color;
                    particleCannonEffect._secondaryEffectRenderer.widthMultiplier = __instance.effectScale;
                    particleCannonEffect._light.range = __instance.effectScale * 10f;
                    particleCannonEffect._fireEffects.transform.localScale *= __instance.effectScale;
                    particleCannonEffect.HorizontalFocus = __instance.ParticleData.Focus.Us * __instance.HorizontalFocusFactor * ParticleCannonConstants.FocusFactor;
                    particleCannonEffect.VerticalFocus = __instance.ParticleData.Focus.Us * __instance.VerticalFocusFactor * ParticleCannonConstants.FocusFactor;
                    */
                    particleCannonEffect.Attenuation = __instance.Attenuation * particleCannonArm.AttenuationFactor;
                    particleCannonEffect.ParticleType = __instance.ParticleData.ParticleType;
                    particleCannonEffect.Range0Damage = __instance.GetDamage(energy, armNumber, false);
                    particleCannonEffect.DistancePerSegment = ParticleCannonConstants.Range / 100f;
                    float num2 = ParticleCannonConstants.DamageAtRange(particleCannonEffect.Attenuation, particleCannonEffect.DistancePerSegment);
                    float num3 = particleCannonEffect.Range0Damage * num2;
                    switch (particleCannonEffect.ParticleType)
                    {
                        case ParticleType.Piercing:
                            ParticleDamageDescription particleDamageDescriptionP = new ParticleDamageDescription(damageLogger, num3, ParticleCannonConstants.PiercingAp);
                            item.ShieldHandler.ApplyPierceParticleDamage(particleDamageDescriptionP, hitPosition);
                            break;
                        case ParticleType.Explosive:
                            ExplosionDamageDescription explosionDamageDescription = new ExplosionDamageDescription(damageLogger, num3, PayloadDerivedValues.GetExplosionRadius(num3), hitPosition, true, true);
                            item.ShieldHandler.ApplyExplosiveParticleDamage(explosionDamageDescription, hitPosition);
                            break;
                        case ParticleType.Emp:
                            EmpDamageDescription emD = new EmpDamageDescription(damageLogger, num3);
                            item.ShieldHandler.ApplyEmpParticleDamage(emD, hitPosition);
                            break;
                        case ParticleType.Impact:
                            KineticDamageDescription kineticDamageDescription = new KineticDamageDescription(damageLogger, num3, ParticleCannonConstants.ImpactAp, true);
                            item.ShieldHandler.ApplyThumpParticleDamage(num3, ParticleCannonConstants.ImpactAp, hitPosition);
                            break;
                    }
                    return false;
                    //We will have to come back to this another day, if we so desire. Sadge.
                    LocalRenderAndRun(particleCannonEffect, hitPosition, damageLogger, seed);
                    int beamCount = __instance.BeamCount;
                    int hitCount = __instance.HitCount;
                    Traverse.Create(particleCannonEffect).Field("BeamCount").SetValue(beamCount + 1);
                    Traverse.Create(particleCannonEffect).Field("HitCount").SetValue(hitCount + 1);
                    float error5 = __instance.TotalError500 + particleCannonEffect.DeviationAt500;
                    float error10 = __instance.TotalError1000 + particleCannonEffect.DeviationAt1000;
                    float error20 = __instance.TotalError2000 + particleCannonEffect.DeviationAt2000;
                    float error40 = __instance.TotalError4000 + particleCannonEffect.DeviationAt4000;
                    Traverse.Create(particleCannonEffect).Field("TotalError500").SetValue(error5);
                    Traverse.Create(particleCannonEffect).Field("TotalError1000").SetValue(error10);
                    Traverse.Create(particleCannonEffect).Field("TotalError2000").SetValue(error20);
                    Traverse.Create(particleCannonEffect).Field("TotalError4000").SetValue(error40);

                    /*
                    particleCannonEffect._lineRenderer.positionCount = seed;
                    particleCannonEffect._secondaryEffectRenderer.positionCount = seed;
                    particleCannonEffect._lineRenderer.SetPosition(seed - 1, particleCannonEffect.transform.InverseTransformPoint(hitPosition));
                    particleCannonEffect._secondaryEffectRenderer.SetPosition(seed - 1, particleCannonEffect.transform.InverseTransformPoint(hitPosition));
                    Traverse.Create(particleCannonEffect).Field("terminatesAtTarget").SetValue(true);
                    particleCannonEffect._lineRenderer.startWidth = 1f;
                    particleCannonEffect._lineRenderer.endWidth = 1f;
                    AdvLogger.LogInfo("Did we actually reach the end of all that?", LogOptions._AlertDevInGame);
                    */
                    /*
                    Vector3 vector3 = Vector3.zero;
                    particleCannonEffect._lineRenderer.SetPosition(seed, vector3);
                    particleCannonEffect._secondaryEffectRenderer.SetPosition(seed, vector3);


                    particleCannonEffect._lineRenderer.positionCount = seed;
                    particleCannonEffect._secondaryEffectRenderer.positionCount = seed;
                    particleCannonEffect._lineRenderer.SetPosition(seed - 1, particleCannonEffect.transform.InverseTransformPoint(hitPosition));
                    particleCannonEffect._secondaryEffectRenderer.SetPosition(seed - 1, particleCannonEffect.transform.InverseTransformPoint(hitPosition));
                    Traverse.Create(particleCannonEffect).Field("terminatesAtTarget").SetValue(true);
                    particleCannonEffect._lineRenderer.startWidth = 1f;
                    particleCannonEffect._lineRenderer.endWidth = 1f;
                    */
                    //TerminateParticleEffectPatch.HandleParticleCannonHitShield(particleCannonEffect, hitPosition, seed)
                    //Let's see what happens if we do this.
                    //Well, that was more intense than expected.
                    return false;
                }
            }
            return true;
        }
        private static void LocalRenderAndRun(ParticleCannonEffect particleCannonEffect, Vector3 hitPosition, IDamageLogger damageLogger, int seed)
        {
            particleCannonEffect._positionOffsets = new ParticlePositionOffsets(100, seed, new Vector3(1f / particleCannonEffect.HorizontalFocus, 1f / particleCannonEffect.VerticalFocus, 0f));
            particleCannonEffect._lineRenderer.positionCount = 100;
            particleCannonEffect._secondaryEffectRenderer.positionCount = 100;
            float num = 0.04f;
            Vector3[] array = new Vector3[100];
            Vector3 vector4 = Vector3.zero;
            for (int i = 0; i < 100; i++)
            {
                bool flag7 = particleCannonEffect.DeviationAt500 < 0f && (float)i * particleCannonEffect.DistancePerSegment > 500f;
                if (flag7)
                {
                    float devAt5 = new Vector3(vector4.x, vector4.y, 0f).magnitude + particleCannonEffect.InaccuracyErrorAt1m * 500f;
                    Traverse.Create(particleCannonEffect).Field("DeviationAt500").SetValue(devAt5);
                    AdvLogger.LogInfo("Adjusted devAt5, are you seeing this?", LogOptions._AlertDevInGame);
                }
                else
                {
                    bool flag2 = particleCannonEffect.DeviationAt1000 < 0f && (float)i * particleCannonEffect.DistancePerSegment > 1000f;
                    if (flag2)
                    {
                        float devAt10 = new Vector3(vector4.x, vector4.y, 0f).magnitude + particleCannonEffect.InaccuracyErrorAt1m * 1000f;
                        Traverse.Create(particleCannonEffect).Field("DeviationAt1000").SetValue(devAt10);
                        AdvLogger.LogInfo("Adjusted devAt10, are you seeing this?", LogOptions._AlertDevInGame);
                    }
                    else
                    {
                        bool flag3 = particleCannonEffect.DeviationAt2000 < 0f && (float)i * particleCannonEffect.DistancePerSegment > 2000f;
                        if (flag3)
                        {
                            float devAt20 = new Vector3(vector4.x, vector4.y, 0f).magnitude + particleCannonEffect.InaccuracyErrorAt1m * 2000f;
                            Traverse.Create(particleCannonEffect).Field("DeviationAt2000").SetValue(devAt20);
                            AdvLogger.LogInfo("Adjusted devAt20, are you seeing this?", LogOptions._AlertDevInGame);
                        }
                        else
                        {
                            bool flag4 = particleCannonEffect.DeviationAt4000 < 0f && (float)i * particleCannonEffect.DistancePerSegment > 4000f;
                            if (flag4)
                            {
                                float devAt40 = new Vector3(vector4.x, vector4.y, 0f).magnitude + particleCannonEffect.InaccuracyErrorAt1m * 4000f;
                                Traverse.Create(particleCannonEffect).Field("DeviationAt4000").SetValue(devAt40);
                                AdvLogger.LogInfo("Adjusted devAt40, are you seeing this?", LogOptions._AlertDevInGame);
                            }
                        }
                    }
                }
                if (i >= 99) continue;
                particleCannonEffect._lineRenderer.SetPosition(i, vector4);
                particleCannonEffect._secondaryEffectRenderer.SetPosition(i, vector4);
                array[i] = particleCannonEffect.transform.TransformPoint(vector4);
                vector4 += new Vector3(0f, 0f, particleCannonEffect.DistancePerSegment);
                vector4 += particleCannonEffect._positionOffsets.GetNextOffset() * particleCannonEffect.DistancePerSegment * Mathf.Clamp01(0.1f + (float)i * num);
                RunEvenMoreThings(particleCannonEffect, damageLogger, hitPosition, array);
            }
        }
        private static void RunEvenMoreThings(ParticleCannonEffect particleCannonEffect, IDamageLogger GR, Vector3 hitPosition, Vector3[]worldPositions)
        {
            int num = 1 + ((particleCannonEffect.ParticleType != ParticleType.Piercing) ? 1 : ((int)particleCannonEffect.DistancePerSegment));
            GridCastReturn gCReturn = new GridCastReturn();
            GridCasting gCaster = new GridCasting(gCReturn);
            for (int i = 1; i < 100; i++)
            {
                if (i >= 99) continue;
                gCReturn.Setup(worldPositions[i - 1], worldPositions[i] - worldPositions[i - 1], (worldPositions[i] - worldPositions[i - 1]).magnitude, num, true);
                gCaster.AllConstructs(StaticConstructablesManager.Constructables, null);
                FinalTermination(particleCannonEffect, gCReturn.FirstHit.InPointGlobal, i);
                gCReturn.FinishedWithForNow();
            }
        }

        private static void FinalTermination (ParticleCannonEffect particleCannonEffect, Vector3 hitPosition, int i)
        {
            particleCannonEffect._lineRenderer.positionCount = i;
            particleCannonEffect._secondaryEffectRenderer.positionCount = i;
            particleCannonEffect._lineRenderer.SetPosition(i - 1, particleCannonEffect.transform.InverseTransformPoint(hitPosition));
            particleCannonEffect._secondaryEffectRenderer.SetPosition(i - 1, particleCannonEffect.transform.InverseTransformPoint(hitPosition));
            Traverse.Create(particleCannonEffect).Field("terminatesAtTarget").SetValue(true);
            particleCannonEffect._lineRenderer.startWidth = 1f;
            particleCannonEffect._lineRenderer.endWidth = 1f;
        }
    }
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ParticleCannonEffect), "TerminateAtPoint")]
    public class TerminateParticleEffectPatch
    {
        public static void HandleParticleCannonHitShield(ParticleCannonEffect effect, Vector3 gameWorldPosition, int indexOfTermination)
        {
            throw new NotImplementedException("It's a stub");
        }
    }
    /*
    [HarmonyPatch(typeof(ProjectileCastingSystem), "CastMe", new Type[] { typeof(ProjectileImpactState), typeof(ISettablePositionAndRotation), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]
    internal class ProjectileCastingSystem_Cast_Patch
    {
        private static void Postfix (ProjectileCastingSystem __instance, ref ProjectileImpactState pState, ref ISettablePositionAndRotation myTransform, ref Vector3 newPosition, ref Vector3 currentPosition, ref Vector3 normalisedDirection, ref float distance, ref Vector3 velocity, ref int debugId, ref Color projectileDebugColor, ref ProjectileUpdateType __result)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                Vector3 hitPosition;
                Vector3 hitNormal;
                if (elipse.CheckIntersection(currentPosition, normalisedDirection, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, currentPosition);
                    if (distance > rayDistance)
                    {
                        
                        item.ShieldHandler.HandleGenericShellHit(pState, currentPosition);
                        __result = ProjectileUpdateType.Deactivate;
                        //Yes, this is working perfectly. Would be awesome to play a vision explosion, though. Need to look into that.
                    }
                }
            }
        }
    }
    */
    [HarmonyPatch(typeof(AdvPooledProjectile), "MoveProjectile")]
    internal class APSMove_Patch
    {
        private static bool Prefix(AdvPooledProjectile __instance)
        {
            Vector3 safePosition = __instance.SafePosition;
            Vector3 gravityForAltitude = StaticPhysics.GetGravityForAltitude(safePosition.y);
            gravityForAltitude.y = Mathf.Min(gravityForAltitude.y, 0);
            float fixedDeltaTimeCache = GameTimer.Instance.FixedDeltaTimeCache;
            Vector3 vector = __instance._pState.FrameStartVelocity * fixedDeltaTimeCache;
            Vector3 vector2 = safePosition + vector;
            float magnitude = vector.magnitude;
            Vector3 vector3 = vector / magnitude;
            Vector3 hitPosition;
            Vector3 hitNormal;
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                if (elipse.CheckIntersection(safePosition, vector3, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, __instance.SafePosition);
                    if (magnitude > rayDistance)
                    {
                        //AdvLogger.LogInfo("APS shell just hit the shield?");
                        ShellModel model = Traverse.Create(__instance).Field("_shellModel").GetValue<ShellModel>();
                        item.ShieldHandler.HandleGenericAPSHit(model, hitPosition, __instance.Gunner);
                        if (model.ExplosiveCharges.GetExplosionRadius() > 0)
                        {
                            GameEvents.Callbacks.DispatchToMainThread(delegate
                            {
                                float size = (6f * (model.ExplosiveCharges.GetExplosionDamage() / 3000));
                                //AdvLogger.LogInfo(size.ToString(), LogOptions._AlertDevInGame);
                                ExplosionVisualiser.Instance.MakeExplosion(size, hitPosition, null, false);
                            }, false);
                        }
                        else if (model.ExplosiveCharges.GetFlakExplosionDamage() > 0)
                        {
                            GameEvents.Callbacks.DispatchToMainThread(delegate
                            {
                                float size = (6f * (model.ExplosiveCharges.GetFlakExplosionDamage() / 500));
                                //AdvLogger.LogInfo(size.ToString(), LogOptions._AlertDevInGame);
                                ExplosionVisualiser.Instance.MakeExplosion(size, hitPosition, null, false);
                            }, false);
                        }
                            __instance.Deactivate(false);
                    }
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PooledCramProjectile), "MoveProjectile")]
    internal class CRAMMove_Patch
    {
        private static bool Prefix(PooledCramProjectile __instance)
        {
            Vector3 safePosition = __instance.SafePosition;
            Vector3 gravityForAltitude = StaticPhysics.GetGravityForAltitude(safePosition.y);
            gravityForAltitude.y = Mathf.Min(gravityForAltitude.y, 0);
            float fixedDeltaTimeCache = GameTimer.Instance.FixedDeltaTimeCache;
            Vector3 vector = __instance._pState.FrameStartVelocity * fixedDeltaTimeCache;
            Vector3 vector2 = safePosition + vector;
            float magnitude = vector.magnitude;
            Vector3 vector3 = vector / magnitude;
            Vector3 hitPosition;
            Vector3 hitNormal;
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                if (elipse.CheckIntersection(safePosition, vector3, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, __instance.SafePosition);
                    if (magnitude > rayDistance)
                    {
                        //AdvLogger.LogInfo("CRAM shell just hit the shield?");
                        item.ShieldHandler.HandleGenericCRAMAndSimpleHit(__instance._pState, hitPosition);
                        if (__instance._pState.ExplosiveDamage > 0)
                        {
                            GameEvents.Callbacks.DispatchToMainThread(delegate
                            {
                                float size = (6f * __instance._pState.ExplosiveDamage / 10000);
                                //AdvLogger.LogInfo(size.ToString(), LogOptions._AlertDevInGame);
                                ExplosionVisualiser.Instance.MakeExplosion(size, hitPosition, null, false);
                            }, false);
                        }
                        __instance.Deactivate(false);
                    }
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PooledProjectile), "MoveProjectile")]
    internal class SimpleProjectileMove_Patch
    {
        private static bool Prefix(PooledProjectile __instance)
        {
            Vector3 safePosition = __instance.SafePosition;
            Vector3 gravityForAltitude = StaticPhysics.GetGravityForAltitude(safePosition.y);
            gravityForAltitude.y = Mathf.Min(gravityForAltitude.y, 0);
            float fixedDeltaTimeCache = GameTimer.Instance.FixedDeltaTimeCache;
            Vector3 vector = __instance._pState.FrameStartVelocity * fixedDeltaTimeCache;
            Vector3 vector2 = safePosition + vector;
            float magnitude = vector.magnitude;
            Vector3 vector3 = vector / magnitude;
            Vector3 hitPosition;
            Vector3 hitNormal;
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                if (elipse.CheckIntersection(safePosition, vector3, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, __instance.SafePosition);
                    if (magnitude > rayDistance)
                    {
                        //AdvLogger.LogInfo("SimpleWeapon shell just hit the shield?");
                        item.ShieldHandler.HandleGenericCRAMAndSimpleHit(__instance._pState, hitPosition);
                        if (__instance._pState.ExplosiveDamage > 0)
                        {
                            GameEvents.Callbacks.DispatchToMainThread(delegate
                            {
                                float size = (1f);
                                ExplosionVisualiser.Instance.MakeExplosion(size, hitPosition, null, false);
                            }, false);
                        }
                        __instance.Deactivate(false);
                    }
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PooledFragment), "MoveProjectile")]
    internal class FragmentMove_Patch
    {
        private static bool Prefix(PooledFragment __instance)
        {
            Vector3 safePosition = __instance.SafePosition;
            Vector3 gravityForAltitude = StaticPhysics.GetGravityForAltitude(safePosition.y);
            gravityForAltitude.y = Mathf.Min(gravityForAltitude.y, 0);
            float fixedDeltaTimeCache = GameTimer.Instance.FixedDeltaTimeCache;
            Vector3 vector = __instance.PState.FrameStartVelocity * fixedDeltaTimeCache;
            Vector3 vector2 = safePosition + vector;
            float magnitude = vector.magnitude;
            Vector3 vector3 = vector / magnitude;
            Vector3 hitPosition;
            Vector3 hitNormal;
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;
                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                if (elipse.CheckIntersection(safePosition, vector3, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, __instance.SafePosition);
                    if (magnitude > rayDistance)
                    {
                        //AdvLogger.LogInfo("Stray fragment just hit the shield?");
                        item.ShieldHandler.HandleStrayFragmentHit(__instance.PState, hitPosition);
                        __instance.Deactivate();
                    }
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlasmaProjectileCastingSystem), "DoCast", new Type[] { typeof(PooledPlasmaProjectile), typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float), typeof(Vector3), typeof(int), typeof(Color) })]

    internal class PlasmaProjectilePoolPatch
    {
        //private static Type __instance;

        private static void Postfix(ref PooledPlasmaProjectile projectile, ref Vector3 newPosition, ref Vector3 currentPosition, ref Vector3 normalisedDirection, ref float distance, ref Vector3 velocity, ref int debugId, ref Color projectileDebugColor)
        {
            //PooledPlasmaProjectile _pooledPlasmaProjectile = Traverse.Create(__instance).Field("_pooledPlasmaProjectile").GetValue<PooledPlasmaProjectile>();

            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();
                /*
                RaycastHit hit;
                Ray ray = new Ray(currentPosition, normalisedDirection);
                */
                Vector3 hitPosition;
                Vector3 hitNormal;
                if (elipse.CheckIntersection(currentPosition, normalisedDirection, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, currentPosition);
                    if (distance > rayDistance)
                    {
                        //AdvLogger.LogInfo("Code worked", LogOptions._AlertDevInGame);
                        //The code does, in fact, work.
                        item.ShieldHandler.ApplyPlasmaDamage(new PlasmaDamageDescription(projectile.Gunner, projectile.GetCurrentDamage(), projectile.ArmorPiercing, currentPosition), hitPosition);
                        projectile.Deactivate(false);
                        //projectile.DestroyYourself();
                    }
                }
                /*
                bool hitShield = elipse.CheckIntersection(newPosition, 30);
                if (!hitShield) continue;
                */
            }
        }
    } // Modified code, reference above existing code...

     //Let's use a very similar method for fire.

    [HarmonyPatch(typeof(FlamerProjectileCastingSystem), "DoCast", new Type[] { typeof(PooledFlamerProjectile), typeof(Vector3), typeof(Vector3), typeof(float) })]

    internal class FlamerProjectilePoolPatch
    {
        private static void Postfix(ref PooledFlamerProjectile projectile, ref Vector3 startPosition, ref Vector3 normalisedDirection, ref float distance)
        {
            foreach (AdvShieldProjector item in TypeStorage.GetObjects())
            {
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;

                Elipse elipse = item.ShieldHandler.Shape;
                elipse.UpdateInfo();

                Vector3 hitPosition;
                Vector3 hitNormal;
                if (elipse.CheckIntersection(startPosition, normalisedDirection, out hitPosition, out hitNormal))
                {
                    float rayDistance = Vector3.Distance(hitPosition, startPosition);
                    if (distance > rayDistance)
                    {
                        //AdvLogger.LogInfo("Code worked", LogOptions._AlertDevInGame);
                        //The code does, in fact, work.
                        item.ShieldHandler.HandleFlamethrowerHit(projectile);
                        projectile.Deactivate(false);
                        //projectile.DestroyYourself();
                    }
                }
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
                if (item.SettingsData.IsShieldOn == enumShieldDomeState.Off) continue;

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
                Vector3 position = _missile.NosePosition;
                float speed = (_missile.Velocity - allConstructBlock.Velocity).magnitude;
                float damage = _missile.Blueprint.Warheads.ThumpPerMs * speed * _missile.GetHealthDependency(HealthDependency.Damage);
                float thumpAP = _missile.Blueprint.Warheads.ThumpAP;
                bool hasEMP = _missile.Blueprint.Warheads.EMPDamage > 0;
                bool hasFire = _missile.Blueprint.Warheads.FireFuel > 0;
                bool hasFrag = _missile.Blueprint.Warheads.FragmentCount > 0;
                bool hasHE = _missile.Blueprint.Warheads.ExplosiveDamage > 0;
                KineticDamageDescription missileDD = new KineticDamageDescription(_missile.Gunner, damage, thumpAP, true);
                item.ShieldHandler.ApplyThumpDamage(damage, thumpAP, position);
                //item.ShieldHandler.ApplyDamage(new KineticDamageDescription(_missile.Gunner, damage, thumpAP, true));
                //item.ShieldHandler.ApplyDamage(new EmpDamageDescription(_missile.Gunner, empDamage));
                if (hasEMP)
                {
                    item.ShieldHandler.ApplyEmpDamage(new EmpDamageDescription(_missile.Gunner, _missile.Blueprint.Warheads.EMPDamage), position);
                }

                if (hasFire)
                {
                    item.ShieldHandler.HandleNonFlamethrowerFireHit(_missile.Blueprint.Warheads.FireFuel, _missile.Blueprint.Warheads.FireIntensity, _missile.Blueprint.Warheads.FireOxidizer);
                }

                if (hasFrag)
                {
                    item.ShieldHandler.HandleFrag(_missile.Blueprint.Warheads.FragmentDamage, _missile.Blueprint.Warheads.FragmentCount, _missile.Blueprint.Warheads.FragmentAngle, position);
                }

                if (hasHE)
                {
                    item.ShieldHandler.ApplyHEDamage(_missile.Blueprint.Warheads.ExplosiveDamage, position);
                }

                bool safetyOn = !__instance.CheckSafety(item) || !_missile.HasClearedVehicle;
                _missile.MoveNoseIntoPosition(hitPointIn);
                _missile.DestroyYourself();
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
