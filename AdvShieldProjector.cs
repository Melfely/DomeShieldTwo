// Decompiled with JetBrains decompiler
// Type: ShieldProjector
// Assembly: Ftd, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BB640B63-E85B-4BC6-BAF1-78BE6814A0C2
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\From The Depths\From_The_Depths_Data\Managed\Ftd.dll

using AdvShields;
using AdvShields.Behaviours;
using AdvShields.Models;
using Assets.Scripts;
using BrilliantSkies.Blocks.BlockBaseClass;
using BrilliantSkies.Blocks.BreadBoards.GenericGetter;
using BrilliantSkies.Blocks.Decorative;
using BrilliantSkies.Blocks.Feet;
using BrilliantSkies.Blocks.Weapons;
using BrilliantSkies.Common.CarriedObjects;
using BrilliantSkies.Common.ChunkCreators.Chunks.Utilities;
using BrilliantSkies.Common.Colliders;
using BrilliantSkies.Common.Controls;
using BrilliantSkies.Common.Controls.AdvStimulii;
using BrilliantSkies.Common.Drag;
using BrilliantSkies.Common.Explosions;
using BrilliantSkies.Common.Masses;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Constructs.Blocks.Parts;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Core.Enumerations;
using BrilliantSkies.Core.Geometry;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Intersections;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Core.Pooling;
using BrilliantSkies.Core.Recursion;
using BrilliantSkies.Core.ResourceAccess;
using BrilliantSkies.Core.Returns;
using BrilliantSkies.Core.Returns.Interfaces;
using BrilliantSkies.Core.Returns.UniversePositions;
using BrilliantSkies.Core.Serialisation.AsDouble;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Core.Threading.Callbacks;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Core.Timing.Internal;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Units;
using BrilliantSkies.Core.Unity.MeshMaking;
using BrilliantSkies.Core.UniverseRepresentation.Positioning.Frames.Points;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.DataManagement.Vars;
using BrilliantSkies.Effects.Pools.Smashes;
using BrilliantSkies.Effects.Regulation;
using BrilliantSkies.Effects.SoundSystem;
using BrilliantSkies.Effects.SpecialSounds;
using BrilliantSkies.FromTheDepths.Game.UserInterfaces;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Ftd.Avatar.Repair;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using BrilliantSkies.Ftd.Constructs.Modules.All.Shell;
using BrilliantSkies.Ftd.Constructs.Modules.All.StandardExplosion;
using BrilliantSkies.Ftd.Constructs.Modules.Main.Scuttling;
using BrilliantSkies.Ftd.DamageLogging;
using BrilliantSkies.Ftd.DamageModels;
using BrilliantSkies.Ftd.Game.Pools;
using BrilliantSkies.GridCasts;
using BrilliantSkies.GridCasts.Interfaces;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Modding.Containers;
//using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Modding.Types.Helpful;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Special.ButtonsAndBars;
using BrilliantSkies.Ui.Tips;
using DomeShieldTwo;
using DomeShieldTwo.newshieldblocksystem;
using HarmonyLib;
using MoonSharp.Interpreter.CoreLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;



namespace AdvShields
{
    public class AdvShieldProjector : BlockWithControl, IGoverningBlock<DomeShieldNode>, IBlock, IAlive, IFlagState, IBlockWithNode<DomeShieldNode>, IBlockWithFirepowerStats
    {
        private ICarriedObjectReference carriedObject;

        private BlockModule_Hot module_Hot;

        private ActivateCallback activateCallback;

        public float CurrentDamageSustained { get; private set; }

        private VelocityMeasurement velocityMeasurement;

        private float reliabilityTimeCheck;

        private float currentStrength;

        public float PowerDrawDifference { get; set; }

        public float BasePowerDrawUI { get; set; }

        public float RPDForUI { get; set; }

        public float APDForUI { get; set; }

        public float RestingPDDFromPowerScale { get; set; }

        public float ActivePDDFromPowerScale { get; set; }

        public float ShieldCircleness { get; set; }

        //public float driveValues { get; set; }

        public virtual float SurfaceFactor { get; private set; } = 1; //surface / AdvShieldDome.BaseSurface;

        public ShieldDomeBehaviour ShieldDome { get; set; }

        public AdvShieldHandler ShieldHandler { get; set; }

        public AdvShieldStatusTwo ShieldStats { get; set; }

        public AdvShieldSettingsData SettingsData { get; set; } = new AdvShieldSettingsData(0u);

        public AdvShieldTransformData TransformData { get; set; } = new AdvShieldTransformData(1u);

        public AdvShieldVisualData VisualData { get; set; } = new AdvShieldVisualData(2u);

        public DomeShieldNode Node { get; set; }

        //public ShieldNode ConnectShieldNode { get; set; }

        public IPowerRequestRecurring PowerUse { get; set; }

        public VarIntClamp Priority { get; set; } = new VarIntClamp(0, -50, 50, NoLimitMode.None);

        public PowerUserData PriorityData { get; set; } = new PowerUserData(34852u);

        public Transform ControllersTransform;

        public int DomeShieldsOnCraft;

        public float OverchargerPercent;

        public float ActiveRectifierPercent;

        public float ShieldSizePower = 0f;

        /*public bool IsActive
        {
            get
            {
                return ShieldData.Type != enumShieldDomeState.Off;
            }
        }*/

        public bool DoesConstructHaveOtherShields
        {
            get
            {
                return MainConstruct.NodeSetsRestricted.RingShieldNodes.NodeCount > 0 || DomeShieldsOnCraft > 1;
            }
        }

        public INode NodeInterface
        {
            get
            {
                return this.Node;
            }
        }
        public static bool HasCreatedTypeSet = false;
        public static bool HasAddedToTypeSet = false;

        
        public float LSRCurrentEnergy
        {
            get
            {
                return this.Node.MaximumEnergy;
                //What does this do?
            }
        }
        [Readable(500U, "Total energy in dome shield cavities ", "")]
        public float CavitySize
        {
            get
            {
                return this.Node.dSPLs.Sum((DomeShieldPowerLink t) => t.dSBeamInfo.Sum((DomeShieldBeamInfo tt) => tt.MaxEnergy));
                //This needs to be fixed when we decide what couplers are changed to.
            }
        }
        //These two seems to be for breadboard? Make sure the 500 series isn't taken yet.
        [Readable(501U, "Maximum energy in dome shield cavities ", "")]
        public float CavitySizeMax
        {
            get
            {
                return this.Node.MaximumEnergy;
            }
        }

        //Next three are for Firepower calcs, leave them be.
        public float GetAmmoPerSec()
        {
            return 0f;
        }
        public float GetFuelPerSec()
        {
            return 0f;
        }
        public float GetProjectileHealthPerSec()
        {
            return 0f;
        }
        public float GetPowerUsed()
        {
            int num = 0;
            /*
            foreach (DomeShieldPowerLink shieldCoupler in this.Node.dSPLs)
            {
                num += shieldCoupler.dSBeamInfo.Sum((DomeShieldBeamInfo beam) => beam.TotalCapacitorSize);
            }
            int num2 = num;
            */
            foreach (DomeShieldPowerLink dSPL in this.Node.dSPLs)
            {
                foreach (DomeShieldBeamInfo beam in dSPL.dSBeamInfo)
                {
                    num += beam.PowerPerSec;
                }
            }
            if (this.Node.matrixComputer != null)
            {
                num += this.Node.matrixComputer.PowerPerSec;
            }
            /*
            foreach (DomeShieldPowerLink domeShieldPowerLink in this.Node.dSPLs)
            {
                foreach (DomeShieldBeamInfo beam in domeShieldPowerLink.dSBeamInfo)
                {
                    int beampercent = CalculatePercentOfMaxHealth(beam, num2);

                }
            }
            */
            return (float)num;
            //This needs to be fixed when we decide what couplers are changed to. We also need to decide if this is how we will do energy.
            //Notice that this is very similar to PumpEnergyPerSecond...

        }
        public float GetNoModCapPowerUsed()
        {
            int num = 0;
            foreach (DomeShieldPowerLink dSPL in this.Node.dSPLs)
            {
                foreach (DomeShieldBeamInfo beam in dSPL.dSBeamInfo)
                {
                    num += beam.NoModCapPowerPerSec;
                }
            }
            return num;
        }
        public float TotalCapModDifference()
        {
            float num = 0;
            foreach (DomeShieldPowerLink dSPL in this.Node.dSPLs)
            {
                foreach (DomeShieldBeamInfo beam in dSPL.dSBeamInfo)
                {
                    num += beam.CapModDifference;
                }
            }
            return num;
        }

        public float TotalCapMult()
        {
            float num = 0;
            int beams = 0;
            foreach (DomeShieldPowerLink dSPL in this.Node.dSPLs)
            {
                foreach (DomeShieldBeamInfo beam in dSPL.dSBeamInfo)
                {
                    num += beam.GetPowerMultiplier();
                    beams++;
                }
            }
            return num / beams;
            //Gonna need to double check that for sure
        }
        /*
        private int CalculatePercentOfMaxHealth(DomeShieldBeamInfo beam, int MaxPowerOfSystem)
        {
            beam.MaxEnergy
        }
        */
        public override float GetFirePower()
        {
            bool flag = this.Node == null;
            float num;
            if (flag)
            {
                num = 0f;
            }
            else
            {
                float num2 = 0f;
                for (int i = 0; i < 5; i++)
                {
                    float num3 = 0f;
                    float num4 = 0f;
                    int num5 = 0;
                    int num6 = 0;
                    foreach (DomeShieldPowerLink shieldCoupler in this.Node.dSPLs)
                    {
                        foreach (DomeShieldBeamInfo beamInfo2 in shieldCoupler.dSBeamInfo)
                        {
                            num4 += beamInfo2.MaxEnergy;
                            num5 += beamInfo2.Hardeners;
                            num3 += 1f; //beamInfo2.DamagePerSec;
                            //We need to find something to replace DamagePerSec for the sake of GetLaserPower!
                            //Or... see below
                            num6 += beamInfo2.TotalEnergyInBeam;
                        }
                    }
                    float ac = DomeShieldConstants.GetAC(num5, num6, i == 0, num4);
                    num2 += FirepowerHandler.GetLaserPower(num3, ac);
                    //We can totally write this ourselves. We're coming back to this another time.
                }
                num = num2;
            }
            return num;

            //Final firepower calc. //This needs to be fixed when we decide what couplers are changed to.
        }

        public override void ItemSet()
        {
            base.ItemSet();

            ShieldStats = new AdvShieldStatusTwo(this, base.item.Code.Variables.GetFloat("maxEnergyFactor"), base.item.Code.Variables.GetFloat("armorClassFactor"), base.item.Code.Variables.GetFloat("passiveRegenFactor"));
        }

        public override void BlockStart()
        {
            base.BlockStart();
            if (StaticStorage.HasLoaded == false) StaticStorage.LoadAsset();
            GameObject gameObject = GameObject.Instantiate<GameObject>(StaticStorage.ShieldDomeObject);
            gameObject.transform.position = GameWorldPosition;
            gameObject.transform.rotation = GameWorldRotation;
            gameObject.transform.localPosition = Transforms.LocalToGlobal(Vector3.zero, GameWorldPosition, GameWorldRotation);
            gameObject.transform.localRotation = Transforms.LocalRotationToGlobalRotation(Quaternion.identity, GameWorldRotation);
            ControllersTransform = gameObject.transform;

            carriedObject = CarryThisWithUs(gameObject, LevelOfDetail.Low);

            ShieldDome = gameObject.GetComponent<ShieldDomeBehaviour>();
            ShieldDome.Initialize(VisualData);

            /*ShieldStats = new AdvShieldStatus(this);*/
            ShieldHandler = new AdvShieldHandler(this);

            // yay
            velocityMeasurement = new VelocityMeasurement(new UniversePositionReturnBlockInMainFrame(this, PositionReturnBlockValidRequirement.Alive));

            //Added Get and Set priority
            //PoweredDecoy CurrentPower = base.TargetPower.PowerUse.Us;
            PowerUse = new PowerRequestRecurring(this, PowerRequestType.Shield, PriorityData.Get, PriorityData.Set)
            {
                fnSetRequestLevel = Allow,
                fnCalculateIdealPowerUse = IdealUse,
            };
            module_Hot = new BlockModule_Hot(this);
            module_Hot.TemperatureIncreaseUnderFullUsagePerSecond = 0.0f;
            module_Hot.CoolingFractionPerSecond = 0.15f;
            module_Hot.TotalTemperatureWeighting = 0.2f;
            module_Hot.SetWeightings(LocalForward);

            activateCallback = new ActivateCallback(this);
            ShieldDataSetChangeAction();
            VisualDataSetChangeAction();
            ShieldSettingsChangeAction();
            ShieldHandler.Shape.UpdateInfo();
            ShieldDome.UpdateSizeInfo(TransformData);
            carriedObject.ObjectItself.transform.localPosition = LocalPosition + new Vector3(TransformData.LocalPosX, TransformData.LocalPosY, TransformData.LocalPosZ);
        }

        public override void StateChanged(IBlockStateChange change)
        {
            base.StateChanged(change);

            if (change.IsAvailableToConstruct)
            {
                base.MainConstruct.NodeSetsRestricted.DictionaryOfAllSets.Get<DomeShieldNodeSet>().AddSender(this);
                TypeStorage.AddProjector(this);
                MainConstruct.PowerUsageCreationAndFuelRestricted.AddRecurringPowerUser(PowerUse);
                MainConstruct.HotObjectsRestricted.AddHotObject(module_Hot);
                MainConstruct.ShieldsChanged();
                MainConstruct.SchedulerRestricted.RegisterForLateUpdate(Update);
            }

            if (change.IsLostToConstructOrConstructLost)
            {
                base.MainConstruct.NodeSetsRestricted.DictionaryOfAllSets.Get<DomeShieldNodeSet>().RemoveSender(this);
                TypeStorage.RemoveProjector(this);
                MainConstruct.PowerUsageCreationAndFuelRestricted.RemoveRecurringPowerUser(PowerUse);
                MainConstruct.HotObjectsRestricted.RemoveHotObject(module_Hot);
                MainConstruct.ShieldsChanged();
                MainConstruct.SchedulerRestricted.UnregisterForLateUpdate(Update);
            }
        }
        public override void FinalOptionalInitialisationStage()
        {
            base.FinalOptionalInitialisationStage();

            SettingsData.IsShieldOn.Us = enumShieldDomeState.On;
        }
        public override void CheckStatus(IStatusUpdate updater)
        {
            base.CheckStatus(updater);
            if (TransformData.Width * TransformData.Height < 1.00999999046326)
            {
                updater.FlagWarning(this, "Should be larger than 1x1");
            }

            if (DoesConstructHaveOtherShields)
            {
                updater.FlagError(this, "Shield domes don't work if there are shield rings or shield projectors on the vehicle");
            }
        }

        public override void PrepForDelete()
        {
            //ShieldClass.CleanUp();
        }

        public override void SyncroniseUpdate(bool b1)
        {
            SetShieldState(b1, false);
        }

        protected override void RunControl(StimulusDirection stimDirection)
        {
            if (stimDirection == StimulusDirection.Positive)
            {
                SettingsData.ExcessDrive.Us = Mathf.Clamp(SettingsData.ExcessDrive + 2f * UnityEngine.Time.timeScale, 1.0001f, 10f);
            }
            else if (stimDirection == StimulusDirection.Negative)
            {
                SettingsData.ExcessDrive.Us = Mathf.Clamp(SettingsData.ExcessDrive - 2f * UnityEngine.Time.timeScale, 1.0001f, 10f);
            }
        }

        protected override void RunControlFromDrive(StimulusDirection stimDirection, float driveValue)
        {
            if (stimDirection == StimulusDirection.None) return;

            driveValue = Mathf.Clamp(driveValue, 1f, 10f);
            SettingsData.ExcessDrive.Us = driveValue;
            //driveValues = driveValue;
        }
        
        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddStatement("Only one shield dome is allowed per vehicle.");
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);

            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            bool flag_0 = currentStrength < driveAfterFactoring;
            string text_0 = "This shield turned off and is therefore inactive";

            if (SettingsData.IsShieldOn.Us == enumShieldDomeState.On)
            {
                text_0 = "This shield is turned on";
            }

            float currentHealth = ShieldHandler.GetCurrentHealth();
            string text_1 = "Shield is fully charged";
            float progress = 1.0f;
            /*string text_2 = $"This shield dome has {(int)currentHealth}/{(int)ShieldStats.MaxEnergy} health";*/
            if (ShieldHandler.CurrentDamageSustained > 0.0f)
            {
                float secondsSinceLastHit = ShieldHandler.TimeSinceLastHit;
                float timeRemaining = ShieldStats.ActualWaitTime - secondsSinceLastHit;
                if (timeRemaining <= 0.0f)
                {
                    text_1 = $"Shield is recharging, {currentHealth / ShieldStats.MaxHealth * 100:F1} % complete.";
                }
                else
                {
                    text_1 = $"Time until recharge: {timeRemaining:F1}s";
                    progress = Mathf.Clamp01(Mathf.SmoothStep(0, 1, secondsSinceLastHit / ShieldStats.ActualWaitTime));
                }
            }
            int num = 500;
            tip.SetSpecial(UniqueTipType.Name, new ProTipSegment_TitleSubTitle("Shield dome", "Projects a defensive shield around itself. Press Q for a lot of options."));
            this.AppendCavityStatsWithFirepower(tip, num);
            tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Total drive {0} (basic drive {1} and an external factor of {2})", driveAfterFactoring, SettingsData.ExcessDrive, SettingsData.ExternalDriveFactor)), BrilliantSkies.Ui.Tips.Position.Middle);
            if (flag_0) tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Charging, effective drive: {0}", Rounding.R2(currentStrength))), BrilliantSkies.Ui.Tips.Position.Middle);
            tip.Add(new ProTipSegment_TextAdjustable(500, text_0), BrilliantSkies.Ui.Tips.Position.Middle);
            //tip.Add(new ProTipSegment_Text(400, $"SHIELD CLASS: {SettingsData.ShieldClass}"), BrilliantSkies.Ui.Tips.Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"Surface area {(int)ShieldHandler.Shape.SurfaceArea()} m2"), BrilliantSkies.Ui.Tips.Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {(int)currentHealth}/{(int)ShieldStats.MaxHealth} health"), BrilliantSkies.Ui.Tips.Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {ShieldStats.ArmourClass} armor class (minimum 2)."), BrilliantSkies.Ui.Tips.Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has a passive regen of {ShieldStats.PassiveRegen} each second. " /* (Minimum 50, maximum 500000).*/ + $"Active regeneration takes {ShieldStats.ActualWaitTime} to begin."), BrilliantSkies.Ui.Tips.Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {ShieldStats.Hardeners} Hardeners and {ShieldStats.Transformers} Transformers attatched. See the stats page for more info."), BrilliantSkies.Ui.Tips.Position.Middle);
            if (ShieldHandler.TimeAtFullHealth > 45) tip.Add(new ProTipSegment_Text(400, "<color=green>Shield has not taken any damage for a considerable time. Power use is divided by 10 until damage is taken again (this is a buff).</color>"), BrilliantSkies.Ui.Tips.Position.Middle);
            if (Node.ConnectedCard.ToLower() != "none") tip.Add(new ProTipSegment_Text(400, $"This shield has a matrix computer with a {Node.ConnectedCard} card attached."), BrilliantSkies.Ui.Tips.Position.Middle);
            if (ShieldHandler.TargettedByContLaser) tip.Add(new ProTipSegment_Text(400, "<color=yellow>Shield is currently being attacked by a continuous laser. Regen capabilities are negatively affected.</color>"), BrilliantSkies.Ui.Tips.Position.Middle);
            if (ShieldHandler.SufferingFromDisruptor) tip.Add(new ProTipSegment_Text(400, $"<color=red>Shield is currently suffering from disruption! This is severely impacting regeneration and armor class! They are multiplied by {1f-ShieldStats.DisruptionFactor}.</color>"), BrilliantSkies.Ui.Tips.Position.Middle);

            //tip.Add(new ProTipSegment_Text(400, $"This shield dome has {ActiveRectifierPercent}% of its energy affected by Active Rectifiers. This is resulting in a {ShieldStats.ActiveRectifierSavingsPercent}% decrease in power usage (50% effective during active regen and full health)"), BrilliantSkies.Ui.Tips.Position.Middle);

            tip.Add(new ProTipSegment_BarWithTextOnIt(400, text_1, progress));
            /*tip.Add(new ProTipSegment_BarWithTextOnIt(400, text_2, progress));*/
            tip.SetSpecial(UniqueTipType.Interaction, new ProTipSegment_TextAdjustableRight(500, "Press <<Q>> to modify shield settings"));
        }
        public void AppendCavityStatsWithFirepower(ProTip tip, int width = 400)
        {
            float firePower = this.GetFirePower();
            bool flag = firePower > 0f;
            if (flag)
            {
                tip.Add(BrilliantSkies.Ui.Tips.Position.Middle, new ProTipSegment_Text(width, AdvShieldProjector._locFile.Format("Tip_DSFirepower", "Firepower: <<{0}>>", new object[] { Rounding.R2(firePower) })));
            }
            this.AppendCavityStats(tip, width, 1f, 1f);
        }
        public void AppendCavityStats(ProTip tip, int width = 400, float damageMultiplier = 1f, float apMultiplier = 1f)
        {
            bool flag = true;
            for (int i = 0; i < 1; i++)
            {
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                float num4 = 0f;
                float num5 = 0f;
                int num6 = 0;
                int num7 = 0;
                bool flag2 = this.Node != null;
                if (flag2)
                {
                    foreach (DomeShieldPowerLink shieldCoupler in this.Node.dSPLs)
                    {
                        foreach (DomeShieldBeamInfo beamInfo2 in shieldCoupler.dSBeamInfo)
                        {
                            num += beamInfo2.MaxEnergy;
                            num6 += beamInfo2.Hardeners;
                            //num3 += beamInfo2.DamagePerSec;
                            num4 += (float)beamInfo2.PowerPerSec;
                            //num5 += beamInfo2.GetHealthThisFrame();
                            num7 += beamInfo2.TotalEnergyInBeam;
                        }
                    }
                }
                bool flag4 = num > 0f;
                if (flag4) flag = false;
                /*
                bool flag4 = num > 0f;
                if (flag4)
                {
                    flag = false;
                    float num8 = DomeShieldConstants.GetAC(num6, num7, i == 0, num) * apMultiplier;
                    float num9 = num2 / num;
                    string text = AdvShieldProjector._locFile.Format("String_TotalEnergy", "Total energy available: {1}/{2}", new object[]
                    {
                    i,
                    Mathf.Round(num2).ToString(),
                    Mathf.Round(num).ToString()
                    });
                    float num10 = ((i != 0) ? 1f : GameSpeedManager.Instance.gameSpeedFactor);
                    tip.Add(BrilliantSkies.Ui.Tips.Position.Middle, new ProTipSegment_BarWithTextOnIt(width, text, num9, true));
                */
                    /*
                    tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_SustainedDamage", "Sustained damage/sec:  <<{0}>> (<<{1} intensity>>)", new object[]
                    {
                    Rounding.R0(num3 * damageMultiplier),
                    Rounding.R1(num8)
                    })));
                    */
                    //tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_DamageOfNextShot", "Damage of next shot:    <<{0}>>", new object[] { Rounding.R0(num5 * damageMultiplier / num10) })));
                    //tip.Add(Position.Middle, new ProTipSegment_Text(width, DomeShieldMultipurpose._locFile.Format("Tip_InitialChargeCost", "Initial charge cost:         <<{0}>> materials", new object[] { Rounding.R1(this.StoredEnergyMaterialCost()) })));
                //}
            }
            bool flag5 = flag;
            if (flag5)
            {
                tip.Add(BrilliantSkies.Ui.Tips.Position.Middle, new ProTipSegment_Text(width, AdvShieldProjector._locFile.Get("Tip_NoCavities", "No cavities attached", true)));
                Node.MaximumEnergy = 0;
            }
            //This is a big one. Take this slowly.
        }

        public override void Secondary(Transform T)
        {
            new UI.AdvShieldUi(this).ActivateGui(GuiActivateType.Stack);
            //new DomeShieldSystemUI(this).ActivateGui(GuiActivateType.Stack);
        }

        public virtual Vector3i[] SetVerificationPosition()
        {
            Vector3i p = LocalPosition;
            Vector3i fv = LocalForward;
            Vector3i rv = LocalRight;
            Vector3i uv = LocalUp;

            Vector3i[] verificationPosition = new Vector3i[12]
            {
                p + 2 * fv,
                p - 2 * fv,
                p + 2 * rv,
                p - 2 * rv,
                p + 2 * uv,
                p - 2 * uv,
                p + 1 * fv,
                p - 1 * fv,
                p + 1 * rv,
                p - 1 * rv,
                p + 1 * uv,
                p - 1 * uv
            };

            return verificationPosition;
        }



        private void Allow(IPowerRequestRecurring request)
        {
            velocityMeasurement.Measure();
            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            //RegenerateFromDisruption(request.PowerUsed, request.DeltaTime);
            currentStrength = Mathf.Min(currentStrength + ShieldProjector.GetDisruptionRegenerationRate(request.PowerUsed) * request.DeltaTime, driveAfterFactoring);

            if (IsOnSubConstructable)
            {
                module_Hot.SetWeightings(LocalForwardInMainConstruct);
            }

            //int num = (IsActive && !DoesConstructHaveOtherShields) ? 1 : 0;
            //request.InitialRequestLevel = num;

            if (request.InitialRequestLevel == 1f)
            {
                module_Hot.TemperatureIncreaseUnderFullUsagePerSecond = (float)(TransformData.Width * (double)TransformData.Height * driveAfterFactoring * 0.100000001490116);
                module_Hot.AddUsage(PowerUse.FractionOfPowerRequestedThatWasProvided);
                //ShieldSound.me.NoiseHere(GameWorldPosition, driveAfterFactoring, 1f);

                if (Net.NetworkType == NetworkType.Client || (double)GameTimer.Instance.TimeCache <= reliabilityTimeCheck)
                {
                    return;
                }

                reliabilityTimeCheck = GameTimer.Instance.TimeCache + Aux.RandomRange(0.1f, 1f);

                bool flag_0 = Aux.Rnd.NextFloat(0.0f, 1f) > PowerUse.FractionOfPowerRequestedThatWasProvided;
                activateCallback.Enqueue(!flag_0, true);
            }
            else if (Net.NetworkType != NetworkType.Client)
            {
                activateCallback.Enqueue(false, true);
            }
        }

        private void IdealUse(IPowerRequestRecurring request)
        {
            ShieldCircleness = 1f;
            float num1 = TransformData.Length / TransformData.Width;
            float num2 = TransformData.Width / TransformData.Height;
            float num3 = TransformData.Height / TransformData.Length;
            float num4 = (num1 + num2 + num3) / 3;
            if ((num4 >= 0.75f) && (num4 <= 1.25f))
            {
                if ((num4 >= 0.75f) && (num4 < 1))
                {
                    ShieldCircleness = (num4 - 0.75f) + 1;
                }
                if (num4 == 1)
                {
                    ShieldCircleness = 1.25f;
                }
                if ((num4 > 1f) && (num4 <= 1.25f))
                {
                    ShieldCircleness = (float)Math.Abs(num4 - 1.25f) + 1;
                }
            }
            float power = GetPowerUsed();
            ShieldSizePower = ((TransformData.Length + TransformData.Width + TransformData.Height) * 3);
            BasePowerDrawUI = (float)((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f);
            RPDForUI = (float)((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness;
            APDForUI = (float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness);
            RestingPDDFromPowerScale = ((float)((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness - (float)((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) / ShieldCircleness);
            ActivePDDFromPowerScale = ((float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness) - (float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) / ShieldCircleness));
            if (DoesConstructHaveOtherShields)
            {
                request.IdealPower = 0f;
                //We never figured this out. Whoopsies :P
            }

            else if (SettingsData.IsShieldOn == enumShieldDomeState.Off)
            {
                request.IdealPower = 0f;
                //these "PowerDrawDifference"s are how the UI shows you the difference in power draw that passive regeneration costs
            }
            else if (ShieldHandler.CurrentDamageSustained <= 0)
            {
                /*
                request.IdealPower = ((float)((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness) * (1f - ((1f - ShieldStats.ActiveRectifierSavingsPercent * 0.5f)));
                PowerDrawDifference = (float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f * ShieldCircleness) + (ShieldStats.PassiveRegen * 1.5f)) - (float)((TransformData.Length * TransformData.Width * TransformData.Height * 0.00499999988824129) + 200f * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) * ShieldCircleness) * (1f - ((1f - ShieldStats.ActiveRectifierSavingsPercent * 0.5f)));
                */
                request.IdealPower = power + ShieldSizePower / ShieldCircleness;
                if (ShieldHandler.TimeAtFullHealth > 45) request.IdealPower /= 10;
            }
            else if (ShieldHandler.isActiveRegen)
            {
                request.IdealPower = power + ShieldSizePower * 2f / ShieldCircleness;
            }
            else
            {
                request.IdealPower = power + ShieldSizePower * 1.2f / ShieldCircleness;
                /*
                request.IdealPower = ((float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness)) * ShieldStats.ActiveRectifierSavingsPercent;
                PowerDrawDifference = ((float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) * ShieldCircleness - (float)(((TransformData.Length * TransformData.Width * TransformData.Height * 0.006f) + 200f) * (float)Math.Round(SettingsData.ExcessDrive / 2.25f + 0.5555f, 1) * ShieldCircleness))) * ShieldStats.ActiveRectifierSavingsPercent;
                */
            }
        }



        [MainThread("Has an RPC and sets an enable flag- must be called from main thread")]
        private void SetShieldState(bool b, bool sync)
        {
            if (!ShieldDome.SetState(b) || !sync || !Net.IsServer) return;
        }

        public float GetExcessDriveAfterFactoring()
        {
            return Mathf.Clamp(SettingsData.ExcessDrive * SettingsData.ExternalDriveFactor, 1.0001f, 10f);
        }
        public void Update()
        {
            ShieldStats.UpdateShieldInformation(Node);
            ShieldHandler.Update(ShieldStats);
            ChangeShieldVisualsBasedOnStats();
        }

        private void ShieldDataSetChangeAction()
        {
            TransformData.SetChangeAction(
            () =>
            {
                ShieldHandler.Shape.UpdateInfo();
                ShieldDome.UpdateSizeInfo(TransformData);
                carriedObject.ObjectItself.transform.localPosition = LocalPosition + new Vector3(TransformData.LocalPosX, TransformData.LocalPosY, TransformData.LocalPosZ);
            });
        }

        private void ShieldSettingsChangeAction()
        {
            SettingsData.SetChangeAction(
                () =>
                {
                    ShieldStats.UpdateShieldInformation(Node);
                });
        }

        private void VisualDataSetChangeAction()
        {
            Material _material = carriedObject.ObjectItself.GetComponent<MeshRenderer>().material;

            VisualData.AssembleSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_ScrollSpeed", newValue));
            VisualData.Edge.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_EdgeIntensity", newValue));
            VisualData.Fresnel.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_FresnelPower", newValue));
            VisualData.NoiseFactor.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_NoiseScale", newValue));
            VisualData.StaticFlickerSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_StaticFlickerSpeed", newValue));
            VisualData.SinWaveFactor.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_WaveFactor", newValue));
            VisualData.SinWaveSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_WaveSpeed", newValue));
            VisualData.SinWaveSize.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_WaveSize", newValue));
            VisualData.BaseColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_GridColor", newValue));
            VisualData.GridColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_WaveColor", newValue));
        }

        private void ChangeShieldVisualsBasedOnStats()
        {
            Material _material = carriedObject.ObjectItself.GetComponent<MeshRenderer>().material;

            float FractionOfHealth = ShieldHandler.CurrentDamageSustained / ShieldStats.MaxHealth;
            //float InvertFraction = 1.0f - FractionOfHealth;
            _material.SetFloat("_ShieldIntegrity", FractionOfHealth);
        }

        public void PlayShieldHit(Vector3 location)
        {
            /*
            AudioClipDefinition byCollectionName = Configured.i.AudioCollections.GetRandomClipByCollectionName("Shield Hit");
            if (byCollectionName == null) return;
            */
            /*Pooler.GetPool<AdvSoundManager>().PlaySound(new SoundRequest(byCollectionName, location)
            {
                Priority = SoundPriority.ShouldHear,
                Pitch = Aux.Rnd.NextFloat(0.9f, 1.1f),
                MinDistance = 0.5f,
                Volume = 0.6f
            });*/
        }

        public new static ILocFile _locFile = Loc.GetFile("AdvShield_Projector");

        public class ActivateCallback : CallbackWithObjects<AdvShieldProjector, bool, bool>
        {
            public ActivateCallback(AdvShieldProjector obj) : base(obj)
            {
            }

            protected override void ApplyTo(AdvShieldProjector obj, bool toApply, bool sync)
            {
                obj.SetShieldState(toApply, sync);
            }
        }
    }
}