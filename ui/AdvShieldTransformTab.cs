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
using BrilliantSkies.DataManagement.CopyPasting;

namespace AdvShields.UI
{ 
    public class AdvShieldTransformTab : SuperScreen<AdvShieldProjector>
    {

        public AdvShieldTransformTab(ConsoleWindow window, AdvShieldProjector focus) : base(window, focus)
        {
            Name = new Content("ShieldSettings", new ToolTip("Adjust the core values of the shield", 200f), "shieldz");
        }

        public override void Build()
        {
            ScreenSegmentStandard standardSegment1 = CreateStandardSegment(InsertPosition.OnCursor);
            ScreenSegmentStandard standardSegment2 = CreateStandardSegment(InsertPosition.OnCursor);
            StringDisplay stringDisplay2 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Select length, width and height of the shield:</i>"));
            standardSegment1.AddInterpretter(Quick.SliderNub(_focus.TransformData, t => "Length", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(_focus.TransformData, t => "Width", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(_focus.TransformData, (t => "Height"), null));
            standardSegment1.AddInterpretter(new Blank(30f));
            StringDisplay stringDisplay3 = standardSegment1.AddInterpretter(StringDisplay.Quick("<i>Select the offset of the shield:</i>"));
            standardSegment1.AddInterpretter(Quick.SliderNub(_focus.TransformData, t => "LocalPosX", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(_focus.TransformData, t => "LocalPosY", null));
            standardSegment1.AddInterpretter(Quick.SliderNub(_focus.TransformData, t => "LocalPosZ", null));
            standardSegment1.AddInterpretter(new Blank(30f));
            standardSegment1.SpaceBelow = 40f;
            standardSegment1.SpaceAbove = 40f;
            stringDisplay2.Justify = new TextAnchor?(TextAnchor.UpperLeft);
            stringDisplay3.Justify = new TextAnchor?(TextAnchor.UpperLeft);
            CreateSpace(0);
            ScreenSegmentStandardHorizontal horizontalSegment2 = CreateStandardHorizontalSegment();
            horizontalSegment2.SpaceBelow = 30f;
            horizontalSegment2.AddInterpretter(SubjectiveButton<AdvShieldProjector>.Quick(_focus, "Copy to clipboard", new ToolTip("Copy the shield settings to the clipboard", 200f), I => CopyPaster.Copy(I.SettingsData)));
            horizontalSegment2.AddInterpretter(SubjectiveButton<AdvShieldProjector>.Quick(_focus, "Paste from clipboard", new ToolTip("Paste shield settings from the clipboard", 200f), I => CopyPaster.Paste(I.SettingsData))).FadeOut = M.m((Func<AdvShieldProjector, bool>)(I => !CopyPaster.ReadyToPaste(I.SettingsData)));
        }
    }
}
