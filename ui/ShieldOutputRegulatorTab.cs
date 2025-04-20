/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvShields.ShieldBlocks;
using System.Runtime.CompilerServices;
using BrilliantSkies.Localisation;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Tips;
using BrilliantSkies.Blocks.MissileComponents;

namespace AdvShields.UI
{
    internal class ShieldOutputRegulatorTab : SuperScreen<ShieldOutputRegulator>
    {
        // Token: 0x17002951 RID: 10577
        // (get) Token: 0x06009F20 RID: 40736 RVA: 0x0034DF22 File Offset: 0x0034C122
        public override Content Name
        {
            get
            {
                return new Content(ShieldOutputRegulatorTab._locFile.Get("Tab_ShieldOutputRegulator", "Shield output regulator configuration", true), new ToolTip(ShieldOutputRegulatorTab._locFile.Get("Tab_ShieldOutputRegulator_Tip", "Set up the shield output regulator block attached to this shield hardpoint", true), 200f), "regulator");
            }
        }

        // Token: 0x06009F21 RID: 40737 RVA: 0x0034DF62 File Offset: 0x0034C162
        public ShieldOutputRegulatorTab(ConsoleWindow window, ShieldOutputRegulator focus) : base(window, focus)
        {
        }

        // Token: 0x06009F22 RID: 40738 RVA: 0x0034DF6E File Offset: 0x0034C16E
        public override void Build()
        {
            this.AddShieldRegulatorSection(base.Window);
        }

        // Token: 0x06009F23 RID: 40739 RVA: 0x0034DF80 File Offset: 0x0034C180
        public void AddShieldRegulatorSection(ConsoleWindow window)
        {
            window.Screen.CreateHeader(ShieldOutputRegulatorTab._locFile.Get("Header_ShieldOutputRegulatorConfig", "Shield output regulator config", true), new ToolTip(ShieldOutputRegulatorTab._locFile.Get("Tab_ShieldOutputRegulator_Tip", "Set up the shield output regulator block attached to this shield hardpoint", true), 200f), InsertPosition.OnCursor);
            ScreenSegmentStandard screenSegmentStandard = window.Screen.CreateStandardSegment(InsertPosition.OnCursor);
            screenSegmentStandard.SpaceAbove = 5f;
            screenSegmentStandard.AddInterpretter<SubjectiveFloatClampedWithBar<ShieldOutputRegulator>>(new SubjectiveFloatClampedWithBar<ShieldOutputRegulator>(M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.DischargeRate.Min), M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.DischargeRate.Max), M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.DischargeRate), M.m<ShieldOutputRegulator>(0.1f), this._focus, M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => ShieldOutputRegulatorTab._locFile.Format("Option_EnergyDischargeRate", "Energy discharge rate: {0}%/sec", new object[]
            {
                I.Data.DischargeRate
            })), delegate (ShieldOutputRegulator I, float f)
            {
                I.Data.DischargeRate.Us = f;
            }, (ShieldOutputRegulator I, float f) => ShieldOutputRegulator._locFile.Format("Option_EnergyDischargeRate_Change", "Change discharge rate from {0}% to {1}%", new object[]
            {
                I.Data.DischargeRate,
                f
            }), M.m<ShieldOutputRegulator>(new ToolTip(ShieldOutputRegulatorTab._locFile.Get("Option_EnergyDischargeRate_Tip", "Percentage of total cavity energy used each second by a single combiner or LAMS node. Modified by destabilizers on the cavity lines they are placed on.", true), 200f)), new string[]
            {
                "energydischarge"
            }));
            screenSegmentStandard.AddInterpretter<SubjectiveFloatClampedWithBar<ShieldOutputRegulator>>(new SubjectiveFloatClampedWithBar<ShieldOutputRegulator>(M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.BurstSize.Min), M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.BurstSize.Max), M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.BurstSize), M.m<ShieldOutputRegulator>(0.1f), this._focus, M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => ShieldOutputRegulatorTab._locFile.Format("Option_ChargeBeforeFiring", "Minimum charge % before firing burst: {0}%", new object[]
            {
                I.Data.BurstSize
            })), delegate (ShieldOutputRegulator I, float f)
            {
                I.Data.BurstSize.Us = f;
            }, (ShieldOutputRegulator I, float f) => ShieldOutputRegulator._locFile.Format("Option_ChargeBeforeFiring_Change", "Change burst size from {0}% to {1}%", new object[]
            {
                I.Data.BurstSize,
                f
            }), M.m<ShieldOutputRegulator>(new ToolTip(ShieldOutputRegulatorTab._locFile.Get("Option_ChargeBeforeFiring_Tip", "Once the laser drops below minimum shot damage, it won't fire again until total cavity energy gets above this percentage. Tracked separately for pulsed and continuous couplers on the same system.", true), 200f)), new string[]
            {
                "burstsize"
            }));
            screenSegmentStandard.AddInterpretter<SubjectiveFloatClampedWithBar<ShieldOutputRegulator>>(new SubjectiveFloatClampedWithBar<ShieldOutputRegulator>(M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.MinimumShotDamage.Min), M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.MinimumShotDamage.Max), M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => I.Data.MinimumShotDamage), M.m<ShieldOutputRegulator>(1f), this._focus, M.m<ShieldOutputRegulator>((ShieldOutputRegulator I) => ShieldOutputRegulatorTab._locFile.Format("Option_MinShotDamage", "Minimum shot damage: {0}", new object[]
            {
                I.Data.MinimumShotDamage
            })), delegate (ShieldOutputRegulator I, float f)
            {
                I.Data.MinimumShotDamage.Us = f;
            }, (ShieldOutputRegulator I, float f) => ShieldOutputRegulatorTab._locFile.Format("Option_MinShotDamage_Change", "Change minimum shot damage from {0} to {1}", new object[]
            {
                I.Data.MinimumShotDamage,
                f
            }), M.m<ShieldOutputRegulator>(new ToolTip(ShieldOutputRegulatorTab._locFile.Get("Option_MinShotDamage_Tip", "Stop firing to charge if a shot would have less than this amount of damage. In mixed Q switch systems only the weakest shot counts.", true), 200f)), new string[]
            {
                "minshotenergy"
            }));
        }

        // Token: 0x0400470D RID: 18189
        public new static ILocFile _locFile = Loc.GetFile("I_CANTFUCKINGTAKEITANYMMORE");
    }
}
*/