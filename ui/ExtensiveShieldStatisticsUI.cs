using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Core;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Ui.Consoles;
using BrilliantSkies.Ui.Consoles.Builders;
using BrilliantSkies.Ui.Consoles.Getters;
using BrilliantSkies.Ui.Consoles.Interpretters;
using BrilliantSkies.Ui.Consoles.Interpretters.Simple;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Buttons;
using BrilliantSkies.Ui.Consoles.Interpretters.Subjective.Numbers;
using BrilliantSkies.Ui.Consoles.Segments;
using BrilliantSkies.Ui.Layouts.DropDowns;
using BrilliantSkies.Ui.Tips;
using UnityEngine;
using AdvShields.Models;

namespace AdvShields.UI
{
    internal class ExtensiveShieldStatisticsUI : SuperScreen<AdvShieldProjector>
    {
        public ExtensiveShieldStatisticsUI(ConsoleWindow window, AdvShieldProjector focus) : base(window, focus)
        {
            Name = new Content("Shield Dome Statistics", new ToolTip("View all of the statitics of this dome shield", 200f), "What is this");
        }
        public override void Build()
        {
            ScreenSegmentStandard standardSegment1 = CreateStandardSegment(InsertPosition.OnCursor);
            //ScreenSegmentStandard standardSegment2 = CreateStandardSegment(InsertPosition.OnCursor);
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m((Func<AdvShieldProjector, string>)(I =>
            {
                enumShieldClassSelection type = I.SettingsData.ShieldClass;
                string str = "The class of this shield is <color=yellow>QuickHeal</color>. This class has the quickest active regen time, but all other stats suffer slight penalties. Best used on evasive craft that will rarely be hit.";
                switch (type)
                {
                    case enumShieldClassSelection.QH:
                        str = "The class of this shield is <color=yellow>QuickHeal</color>. This class has the quickest active regen time, but all other stats suffer slight penalties. Best used on evasive craft that will rarely be hit.";
                        break;
                    case enumShieldClassSelection.AC:
                        str = "The class of this shield is <color=red>Armored</color>. This class has by far the highest base armor class of all shields, but suffers greatly in regeneration. Best used when the shield is not expected to regenerate after going down.";
                        break;
                    case enumShieldClassSelection.HE:
                        str = "The class of this shield is <color=purple>Healthy</color>. This class has significantly higher base health than any other shields, and takes less emp damage than the other shields. However, other stats suffer noticably. Best used against weapons with high armor penetration, as their penetrating capabilities will be wasted.";
                        break;
                    case enumShieldClassSelection.GEN:
                        str = "The class of this shield is <color=green>Generalist</color>. This class has decent buffs to health and armor class, and no debuffs to passive regen. None of the stats by themselves are the best, and power scaling is less useful. Best used as a balanced option when the threat is unknown or varied.";
                        break;
                    case enumShieldClassSelection.REG:
                        str = "The class of this shield is <color=cyan>Regenerator</color>. This class has incredible passive regen that can be brought to extreme numbers if powered properly. When used against craft with low DPS, you can become almost invincible. Beware, though; 20% reduced health, lowest base armor of all classes, and increased emp damage taken means that a strong burst can disable the shield instantly!!!";
                        break;
                }
                return str;
            }))));
            StringDisplay stringDisplay1 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Basic Shield Stats:</i>"));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The maximum health of this shield is {0}", (I.ShieldStats.MaxHealth)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The armor class of this shield is {0}", (I.ShieldStats.ArmourClass)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The passive regeneration of this shield is {0} health per second.", (I.ShieldStats.PassiveRegen)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The delay before active regeneration of this shield is {0} seconds. This means that the shield must not take damage (usually while off) for {0} seconds before the laser system will start 'firing', restoring the shield.", (I.ShieldStats.ActualWaitTime)))));
            CreateSpace(0);
            StringDisplay stringDisplay2 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Shield modifiers:</i>"));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("Before any modifiers, the base health of the shield is <color=red>{0}</color>, equal to the max energy of the system. A total of {1}% of the power is being routed to AC or regen, resulting in a health loss of {2}.", (I.Node.MaximumEnergy), (I.ShieldStats.CombinedRoutedPowerPercent), (I.ShieldStats.HealthLossFromRoutedPower)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("You currently have {0}% of the power being utilized for Armor Class. This, combined with your Hardeners, is resulting in a increase of <color=green>{1}</color> AC to the shield, up from the base of {2}.", (I.SettingsData.ArmourPercent), (I.ShieldStats.ArmourIncrease), (I.ShieldStats.BaseArmourClass)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("You currently have {0}% of the power being utilized for Regeneration. This, combined with your Transformers, is resulting in a increase of <color=green>{1}</color> Passive regen to the shield, up from the base of {2}. During active regen, this number is multiplied by 10x.", (I.SettingsData.RegenPercent), (I.ShieldStats.RegenIncrease), (I.ShieldStats.BaseRegen)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("After the bonus from overchargers, you effectively have {0} Hardeners and {1} Transformers.", I.ShieldStats.Hardeners, I.ShieldStats.Transformers))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("Together, these lower the health of the shield by <color=purple>{0}</color>.", (I.ShieldStats.HealthLossFromRoutedPower)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("There are {0} blocks connected to this shield (does not include conduit blocks). This number * 0.3 means that active regeneration has a base wait time of {1}. You have {2} spoofers attatched, reducing the wait time down to a final number of <color=green>{3}</color> (Total difference: {4})", I.ShieldStats.TotalBlocks, I.ShieldStats.BaseWaitTime, I.ShieldStats.Spoofers, I.ShieldStats.ActualWaitTime, ((float)Math.Round(I.ShieldStats.ActualWaitTime - I.ShieldStats.BaseWaitTime), 1)))));
            //standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("Additionally, the current power scale of the shield is <color=red>{0}</color>. AFTER the above, this increases health by <color=green>{1}</color>, armor class by <color=green>{2}</color>, and passive regen by <color=green>{3}</color>. However, this increases the active regen wait time by <color=purple>{4}</color>, and increases the engine draw by <color=purple>{5}</color> while resting and <color=purple>{6}</color> during passive regeneration. Power scaling will favor the stats that the shield class already enhances; for example, power scaling the Armored class will mostly increase armor, while providing little benefit to health and passive regen.", (I.ShieldStats.PowerScale), (I.ShieldStats.HealthFromPowerScale), (I.ShieldStats.ACFromPowerScale), (I.ShieldStats.PRFromPowerScale), (I.ShieldStats.WTFromPowerScale), (I.RestingPDDFromPowerScale), (I.ActivePDDFromPowerScale)))));
            /*
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m((Func<AdvShieldProjector, string>)(I =>
            {
                enumShieldClassSelection type = I.SettingsData.ShieldClass;
                string str = "However, as a QuickHeal shield, the power scale<color= green> DECREASES </color> active wait time instead.";
                switch (type)
                {
                    case enumShieldClassSelection.QH:
                        str = "However, as a QuickHeal shield, the power scale <color=green>DECREASES</color> active wait time instead.";
                        break;
                    case enumShieldClassSelection.AC:
                        str = "The QuickHeal shield would have decreased wait time, instead of increased.";
                        break;
                    case enumShieldClassSelection.HE:
                        str = "The QuickHeal shield would have decreased wait time, instead of increased.";
                        break;
                    case enumShieldClassSelection.GEN:
                        str = "The QuickHeal shield would have decreased wait time, instead of increased.";
                        break;
                    case enumShieldClassSelection.REG:
                        str = "The QuickHeal shield would have decreased wait time, instead of increased.";
                        break;
                }
                return str;
            }))));
            */
            CreateSpace(0);
            StringDisplay stringDisplay3 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Engine / Power draw:</i>"));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The size of the shield creates a base power draw of <color=red>{0}</color>.", (I.BasePowerDrawUI)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("Power scale is increasing resting power draw by <color=purple>{0}</color>, and power draw while passively regenerating by <color=purple>{1}</color>.", (I.RestingPDDFromPowerScale), (I.ActivePDDFromPowerScale)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("While resting, the shield uses <color=red>{0}</color> engine power.", (I.RPDForUI)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("While passive regeneration is occuring, the shield uses <color=red>{0}</color> engine power.", (I.APDForUI)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The shield is benefitting from a <color=green>{0}</color>% reduction in power use due to the circular shape of the shield.", (I.ShieldCircleness * 100) - 100))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The shield is also benefitting from a <color=green>{0}</color>% reduction in power use due to <color=red>{1}%</color> of the energy being affected by Active Rectifiers. This bonus is increased by 25% if you are using the Regenerator type shield (you would see this bonus reflected in this stat page.", (I.ShieldStats.ActiveRectifierSavingsPercent),(I.ActiveRectifierPercent)))));
            CreateSpace(0);
            standardSegment1.SpaceBelow = 40f;
            standardSegment1.SpaceAbove = 40f;
            stringDisplay1.Justify = new TextAnchor?(TextAnchor.MiddleCenter);
            stringDisplay2.Justify = new TextAnchor?(TextAnchor.MiddleCenter);
            stringDisplay3.Justify = new TextAnchor?(TextAnchor.MiddleCenter);

        }
    }
}
