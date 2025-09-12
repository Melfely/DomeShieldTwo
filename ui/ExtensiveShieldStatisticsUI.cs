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
            /*
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m((Func<AdvShieldProjector, string>)(I =>
            {
                enumShieldClassSelection type = I.SettingsData.ShieldClass;
                string str = "The class of this shield is <color=yellow>QuickHeal</color>. This class has the quickest active regen time, and the minimum time is lowered to 1 second (down from 5).";
                switch (type)
                {
                    case enumShieldClassSelection.QH:
                        str = "The class of this shield is <color=yellow>QuickHeal</color>. This class has the quickest active regen time, and the minimum time is lowered to 1 second (down from 5).";
                        break;
                    case enumShieldClassSelection.AC:
                        str = "The class of this shield is <color=red>Armored</color>. This class has a higher armor class than the other shields, effectively reducing incoming damage.";
                        break;
                    case enumShieldClassSelection.HE:
                        str = "The class of this shield is <color=purple>Healthy</color>. This class has double the base health of other shields (being factored in after ac / regen is calcualted).";
                        break;
                    case enumShieldClassSelection.GEN:
                        str = "The class of this shield is <color=green>Generalist</color>. This class has minor buffs to health, armor class, and regeneration.";
                        break;
                    case enumShieldClassSelection.REG:
                        str = "The class of this shield is <color=cyan>Regenerator</color>. This class has stronger regeneration capabilities";
                        break;
                }
                return str;
            }))));
            */
            StringDisplay stringDisplay0 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i> THESE STATS ASSUME YOU ARE NOT IN COMBAT </i>"));
            StringDisplay stringDisplay1 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Basic Shield Stats:</i>"));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The maximum health of this shield is {0}", (I.ShieldStats.MaxHealth)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The armor class of this shield is {0}", (I.ShieldStats.ArmourClass)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The passive regeneration of this shield is {0} health per second.", (I.ShieldStats.PassiveRegen)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The delay before active regeneration of this shield is {0} seconds. This means that the shield must not take damage (usually while off) for {0} seconds before the laser system will start 'firing', restoring the shield.", (I.ShieldStats.ActualWaitTime)))));
            CreateSpace(0);
            StringDisplay stringDisplay2 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Shield modifiers:</i>"));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("Before any modifiers, the base health of the shield is <color=red>{0}</color>, equal to the max energy of the system. A total of {1}% of the power is being routed to AC and regen, resulting in a health loss of {2}.", (I.Node.MaximumEnergy), (I.ShieldStats.CombinedRoutedPowerPercent), (I.ShieldStats.HealthLossFromRoutedPower)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("You currently have Armor Class set to {0}. Before hardeners, this would cost {1}% of the energy in the system. With your {2} effective hardeners, the actual energy cost is instead {3}", (I.SettingsData.ArmourSet), (I.ShieldStats.EnergyPercentForArmour), (I.ShieldStats.Hardeners), I.ShieldStats.BaseEnergyPercentForArmour))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("You currently have {0}% of the power being utilized for Regeneration. This, combined with your Transformers, is resulting in a increase of <color=green>{1}</color> Passive regen to the shield, up from the base of {2}. During active regen, this number is multiplied by 10x.", (I.ShieldStats.ActualRegenPercent), (I.ShieldStats.RegenIncrease), (I.ShieldStats.BaseRegen)))));
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
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The capacitors on this system create a base power draw of {0}.", (I.GetNoModCapPowerUsed())))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("Your placement of active rectifiers and overchargers adjusts the base power draw by a multiplier of <color=yellow>{0}</color>, meaning an actual difference of <color=yellow>{1}</color>", I.TotalCapMult(), I.TotalCapModDifference()))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The size of the dome adds {0} power draw, calculated by (L+W+H) * 3.", (I.ShieldSizePower)))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I =>
            {
                bool hasMatrix = I.Node.matrixComputer != null;
                if (hasMatrix) return string.Format("You have a matrix computer installed, adding {0} power cost (max energy / 250, minimum power draw of 500).", I.Node.matrixComputer.PowerPerSec);
                else return "You do not have a matrix computer installed, so there is no power draw from that";
            })));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I =>
            {
                bool hasSpoofer = I.ShieldStats.Spoofers > 0;
                if (hasSpoofer) return string.Format("You have {0} spoofers, for a total power increase of {1} (spoofers * (300 * power mult)).", I.ShieldStats.Spoofers, (I.ShieldStats.Spoofers * (300 * I.TotalCapMult())));
                else return "You do not have any spoofers attached, so there is no power draw from that";
            })));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("The shield is benefitting from a <color=green>{0}</color>% reduction in power use due to the circular shape of the shield.", (I.ShieldCircleness * 100) - 100))));
            standardSegment1.AddInterpretter(SubjectiveDisplay<AdvShieldProjector>.Quick(_focus, M.m<AdvShieldProjector>(I => string.Format("This comes to a final power draw of <color=purple>{0}</color> while resting. During passive regen, this is multiplied by 1.2 to become {1}, and during active regen it's multiplied by 2 to become {2}. Be sure your engine can handle it!", (I.GetPowerUsed() + I.ShieldSizePower / I.ShieldCircleness), ((I.GetPowerUsed() + I.ShieldSizePower / I.ShieldCircleness)) * 1.2f, ((I.GetPowerUsed() + I.ShieldSizePower / I.ShieldCircleness) * 2)))));
            CreateSpace(0);
            standardSegment1.SpaceBelow = 40f;
            standardSegment1.SpaceAbove = 40f;
            stringDisplay1.Justify = new TextAnchor?(TextAnchor.MiddleCenter);
            stringDisplay2.Justify = new TextAnchor?(TextAnchor.MiddleCenter);
            stringDisplay3.Justify = new TextAnchor?(TextAnchor.MiddleCenter);

        }
    }
}
