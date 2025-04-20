// Decompiled with JetBrains decompiler
// Type: ShieldProjector
// Assembly: Ftd, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BB640B63-E85B-4BC6-BAF1-78BE6814A0C2
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\From The Depths\From_The_Depths_Data\Managed\Ftd.dll

using BrilliantSkies.Common.Controls.AdvStimulii;
using BrilliantSkies.Common.CarriedObjects;
using BrilliantSkies.Common.StatusChecking;
using BrilliantSkies.Core;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Pooling;
using BrilliantSkies.Core.Returns;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Returns.UniversePositions;
using BrilliantSkies.Core.Serialisation.AsDouble;
using BrilliantSkies.Core.Threading;
using BrilliantSkies.Core.Threading.Callbacks;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Effects.SoundSystem;
using BrilliantSkies.Effects.SpecialSounds;
//using BrilliantSkies.Modding;
using BrilliantSkies.Modding.Types;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Tips;
using System;
using System.Collections.Generic;
using UnityEngine;
using AdvShields.Models;
using AdvShields.Behaviours;
using Assets.Scripts;
using BrilliantSkies.Blocks.BlockBaseClass;
using BrilliantSkies.Blocks.Decorative;
using BrilliantSkies.Blocks.Feet;
using BrilliantSkies.Common.ChunkCreators.Chunks.Utilities;
using BrilliantSkies.Common.Colliders;
using BrilliantSkies.Common.Controls;
using BrilliantSkies.Common.Drag;
using BrilliantSkies.Common.Explosions;
using BrilliantSkies.Common.Masses;
using BrilliantSkies.Constructs.Blocks.Parts;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Enumerations;
using BrilliantSkies.Core.Geometry;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Intersections;
using BrilliantSkies.Core.Logger;
using BrilliantSkies.Core.Maths;
using BrilliantSkies.Core.Recursion;
using BrilliantSkies.Core.ResourceAccess;
using BrilliantSkies.Core.Returns.Interfaces;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.Units;
using BrilliantSkies.Core.Unity.MeshMaking;
using BrilliantSkies.Core.UniverseRepresentation.Positioning.Frames.Points;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.Effects.Pools.Smashes;
using BrilliantSkies.Effects.Regulation;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Ftd.Avatar.Repair;
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
using BrilliantSkies.Modding.Types.Helpful;
using BrilliantSkies.Ui.Consoles.Styles;
using BrilliantSkies.Ui.Special.ButtonsAndBars;
using BrilliantSkies.FromTheDepths.Game.UserInterfaces;
using AdvShields;
using BrilliantSkies.Ftd.Constructs.Modules.All.DebugAnnotations;
using System.Runtime.CompilerServices;
using BrilliantSkies.DataManagement.Vars;



namespace AdvShields
{
    public class AdvShieldProjector : BlockWithControl
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

        public AdvShieldStatus ShieldStats { get; set; }

        public AdvShieldData ShieldData { get; set; } = new AdvShieldData(0u);

        public AdvShieldVisualData VisualData { get; set; } = new AdvShieldVisualData(1u);

        public LaserNode ConnectLaserNode { get; set; }

        //public ShieldNode ConnectShieldNode { get; set; }

        public IPowerRequestRecurring PowerUse { get; set; }

        public VarIntClamp Priority { get; set; } = new VarIntClamp(0, -50, 50, NoLimitMode.None);

        public PowerUserData PriorityData { get; set; } = new PowerUserData(34852u);

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
                return MainConstruct.NodeSetsRestricted.RingShieldNodes.NodeCount > 0 || MainConstruct.iBlockTypeStorage.ShieldProjectorStore.Count > 0;
            }
        }

        public override void ItemSet()
        {
            base.ItemSet();

            ShieldStats = new AdvShieldStatus(this, base.item.Code.Variables.GetFloat("maxEnergyFactor"), base.item.Code.Variables.GetFloat("armorClassFactor"), base.item.Code.Variables.GetFloat("passiveRegenFactor"));
        }

        public override void BlockStart()
        {
            base.BlockStart();
            GameObject gameObject = GameObject.Instantiate<GameObject>(StaticStorage.ShieldDomeObject);
            gameObject.transform.position = GameWorldPosition;
            gameObject.transform.rotation = GameWorldRotation;
            gameObject.transform.localPosition = Transforms.LocalToGlobal(Vector3.zero, GameWorldPosition, GameWorldRotation);
            gameObject.transform.localRotation = Transforms.LocalRotationToGlobalRotation(Quaternion.identity, GameWorldRotation);

            carriedObject = CarryThisWithUs(gameObject, LevelOfDetail.Low);

            ShieldDome = gameObject.GetComponent<ShieldDomeBehaviour>();
            ShieldDome.Initialize();

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
        }

        public override void StateChanged(IBlockStateChange change)
        {
            base.StateChanged(change);

            if (change.IsAvailableToConstruct)
            {
                TypeStorage.AddProjector(this);
                MainConstruct.PowerUsageCreationAndFuelRestricted.AddRecurringPowerUser(PowerUse);
                MainConstruct.HotObjectsRestricted.AddHotObject(module_Hot);
                MainConstruct.ShieldsChanged();
                MainConstruct.SchedulerRestricted.RegisterForLateUpdate(Update);
            }

            if (change.IsLostToConstructOrConstructLost)
            {
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

            ShieldData.IsShieldOn.Us = enumShieldDomeState.On;
        }
        public override void CheckStatus(IStatusUpdate updater)
        {
            base.CheckStatus(updater);

            if (ShieldData.Width * ShieldData.Height < 1.00999999046326)
            {
                updater.FlagWarning(this, "Should be larger than 1x1");
            }

            if (DoesConstructHaveOtherShields)
            {
                updater.FlagError(this, "Shield domes don't work if there are shield rings or shield projectors on the vehicle");
            }

            ConnectLaserNode = LaserComponentSearch();
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
                ShieldData.ExcessDrive.Us = Mathf.Clamp(ShieldData.ExcessDrive + 2f * UnityEngine.Time.timeScale, 1.0001f, 10f);
            }
            else if (stimDirection == StimulusDirection.Negative)
            {
                ShieldData.ExcessDrive.Us = Mathf.Clamp(ShieldData.ExcessDrive - 2f * UnityEngine.Time.timeScale, 1.0001f, 10f);
            }
        }

        protected override void RunControlFromDrive(StimulusDirection stimDirection, float driveValue)
        {
            if (stimDirection == StimulusDirection.None) return;

            driveValue = Mathf.Clamp(driveValue, 1f, 10f);
            ShieldData.ExcessDrive.Us = driveValue;
            //driveValues = driveValue;
        }

        public override BlockTechInfo GetTechInfo()
        {
            return new BlockTechInfo().AddStatement("Shields have a reduction in reflect effectiveness when moving at high speeds").AddStatement("Shield Domes cannot run when Shield Rings or Shield Projectors are present on your vehicle");
        }
        protected override void AppendToolTip(ProTip tip)
        {
            base.AppendToolTip(tip);

            float driveAfterFactoring = GetExcessDriveAfterFactoring();
            bool flag_0 = currentStrength < driveAfterFactoring;
            string text_0 = "This shield turned off and is therefore inactive";

            if (ShieldData.IsShieldOn.Us == enumShieldDomeState.On)
            {
                text_0 = "This shield is turned on";
            }

            float currentHealth = ShieldHandler.GetCurrentHealth();
            string text_1 = "Shield is fully charged";
            float progress = 1.0f;
            /*string text_2 = $"This shield dome has {(int)currentHealth}/{(int)ShieldStats.MaxEnergy} health";*/
            if (ShieldHandler.CurrentDamageSustained > 0.0f)
            {
                float secondsSinceLastHit = UnityEngine.Time.time - ShieldHandler.TimeSinceLastHit;
                float timeRemaining = ShieldStats.WaitTime - secondsSinceLastHit;
                if (timeRemaining <= 0.0f)
                {
                    text_1 = $"Shield is recharging, {currentHealth / ShieldStats.MaxEnergy * 100:F1} % complete.";
                }
                else
                {
                    text_1 = $"Time until recharge: {timeRemaining:F1}s";
                    progress = Mathf.Clamp01(Mathf.SmoothStep(0, 1, secondsSinceLastHit / ShieldStats.WaitTime));
                }
            }

            tip.SetSpecial(UniqueTipType.Name, new ProTipSegment_TitleSubTitle("Shield dome", "Projects a defensive shield around itself"));
            tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Total drive {0} (basic drive {1} and an external factor of {2})", driveAfterFactoring, ShieldData.ExcessDrive, ShieldData.ExternalDriveFactor)), Position.Middle);
            if (flag_0) tip.Add(new ProTipSegment_TextAdjustable(500, string.Format("Charging, effective drive: {0}", Rounding.R2(currentStrength))), Position.Middle);
            tip.Add(new ProTipSegment_TextAdjustable(500, text_0), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"SHIELD CLASS: {ShieldStats.ShieldType}"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"Surface area {(int)ShieldHandler.Shape.SurfaceArea()} m2"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {(int)currentHealth}/{(int)ShieldStats.MaxEnergy} health"), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {ShieldStats.ArmorClass} armor class (minimum 2)."), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has a passive regen of {ShieldStats.PassiveRegen} each second (Minimum 50, maximum 500000). Active regeneration takes {ShieldStats.WaitTime} to begin."), Position.Middle);
            tip.Add(new ProTipSegment_Text(400, $"This shield dome has {ShieldStats.Doublers} Frequency Doublers and {ShieldStats.Destabilizers} Destabilizers attatched. See the stats page for more info."), Position.Middle);

            tip.Add(new ProTipSegment_BarWithTextOnIt(400, text_1, progress));
            /*tip.Add(new ProTipSegment_BarWithTextOnIt(400, text_2, progress));*/
            tip.SetSpecial(UniqueTipType.Interaction, new ProTipSegment_TextAdjustableRight(500, "Press <<Q>> to modify shield settings"));
        }

        public override void Secondary(Transform T)
        {
            new UI.AdvShieldUi(this).ActivateGui(GuiActivateType.Stack);
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
                module_Hot.TemperatureIncreaseUnderFullUsagePerSecond = (float)(ShieldData.Width * (double)ShieldData.Height * driveAfterFactoring * 0.100000001490116);
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
            float num1 = ShieldData.Length / ShieldData.Width;
            float num2 = ShieldData.Width / ShieldData.Height;
            float num3 = ShieldData.Height / ShieldData.Length;
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

            BasePowerDrawUI = (float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f);
            RPDForUI = (float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness;
            APDForUI = (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness);
            RestingPDDFromPowerScale = ((float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness - (float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) / ShieldCircleness);
            ActivePDDFromPowerScale = ((float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness) - (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) / ShieldCircleness));

            if (DoesConstructHaveOtherShields)
            {
                request.IdealPower = 0f;
            }

            else if (ShieldData.IsShieldOn == enumShieldDomeState.Off)
            {
                request.IdealPower = 0f;
                PowerDrawDifference = (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) +200f) + (ShieldStats.PassiveRegen * 1.5f)) - (float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.00499999988824129) + 200f);
                //these "PowerDrawDifference"s are how the UI shows you the difference in power draw that passive regeneration costs
            }
            else if (ShieldHandler.CurrentDamageSustained <= 0)
            {
                float driveAfterFactoring = GetExcessDriveAfterFactoring();
                request.IdealPower = (float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness;
                PowerDrawDifference = (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f * ShieldCircleness) + (ShieldStats.PassiveRegen * 1.5f)) - (float)((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.00499999988824129) + 200f * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) * ShieldCircleness);
            }
            else
            {
                float driveAfterFactoring = GetExcessDriveAfterFactoring();
                request.IdealPower = (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) / ShieldCircleness);
                PowerDrawDifference = (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) + (ShieldStats.PassiveRegen * 1.5f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) * ShieldCircleness - (float)(((ShieldData.Length * ShieldData.Width * ShieldData.Height * 0.006f) + 200f) * (float)Math.Round(ShieldData.ExcessDrive / 2.25f + 0.5555f, 1) * ShieldCircleness));            }
        }



        [MainThread("Has an RPC and sets an enable flag- must be called from main thread")]
        private void SetShieldState(bool b, bool sync)
        {
            if (!ShieldDome.SetState(b) || !sync || !Net.IsServer) return;
        }

        public float GetExcessDriveAfterFactoring()
        {
            return Mathf.Clamp(ShieldData.ExcessDrive * ShieldData.ExternalDriveFactor, 1.0001f, 10f);
        }
        public void Update()
        {
            ShieldStats.Update();
            ShieldHandler.Update(ShieldStats);
        }

        private void ShieldDataSetChangeAction()
        {
            ShieldData.SetChangeAction(
            () =>
            {
                ShieldHandler.Shape.UpdateInfo();
                ShieldDome.UpdateSizeInfo(ShieldData);
                carriedObject.ObjectItself.transform.localPosition = LocalPosition + new Vector3(ShieldData.LocalPosX, ShieldData.LocalPosY, ShieldData.LocalPosZ);
            });
        }

        private void VisualDataSetChangeAction()
        {
            Material _material = carriedObject.ObjectItself.GetComponent<MeshRenderer>().material;

            VisualData.AssembleSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_AssembleSpeed", newValue));
            VisualData.Edge.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_Edge", newValue));
            VisualData.Fresnel.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_Fresnel", newValue));
            VisualData.SinWaveFactor.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveFactor", newValue));
            VisualData.SinWaveSpeed.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveSpeed", newValue));
            VisualData.SinWaveSize.SetChangeAction((newValue, oldValue, type) => _material.SetFloat("_SinWaveSize", newValue));
            VisualData.BaseColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_Color", newValue));
            VisualData.GridColor.SetChangeAction((newValue, oldValue, type) => _material.SetColor("_GridColor", newValue));
        }

        private LaserNode LaserComponentSearch()
        {
            Vector3i[] verificationPosition = SetVerificationPosition();
            LaserNode ln = null;

            foreach (Vector3i vp in verificationPosition)
            {
                Block b = GetConstructableOrSubConstructable().AllBasicsRestricted.GetAliveBlockViaLocalPosition(vp);

                if (b is LaserConnector || b is LaserTransceiver)
                {
                    LaserComponent lc = b as LaserComponent;
                    ln = lc.Node;
                    break;
                }
                else if (b is LaserMultipurpose)
                {
                    LaserMultipurpose lm = b as LaserMultipurpose;
                    ln = lm.Node;
                    break;
                }
            }

            return ln;
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