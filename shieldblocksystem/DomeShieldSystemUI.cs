using BrilliantSkies.Blocks.MissileComponents;
using BrilliantSkies.Ui.Consoles;
using System;
using System.Collections.Generic;
using System.Text;
using BrilliantSkies.Blocks.Weapons.Uis;
using BrilliantSkies.Localisation.Runtime.FileManagers.Files;
using BrilliantSkies.Localisation;

namespace DomeShieldTwo.shieldblocksystem
{
    public class DomeShieldSystemUI : ConsoleUi<DomeShieldMultipurpose>
    {
        static DomeShieldSystemUI()
        {
        }
        public DomeShieldSystemUI(DomeShieldMultipurpose focus) : base(focus)
        {
        }
        public override bool StillHasAValidSubject()
        {
            return this._focus.IsAlive;
        }

        protected override ConsoleWindow BuildInterface(string suggestedName = "")
        {
            ConsoleWindow consoleWindow = base.NewWindow(0, string.Format("{0}", this._focus.Name), WindowSizing.GetLhs());
            consoleWindow.DisplayTextPrompt = false;
            consoleWindow.AllScreens.Clear();
            consoleWindow.AllScreens.Add(new StatsTab(consoleWindow, this._focus, null));
            consoleWindow.Screen = consoleWindow.AllScreens[0];
            return consoleWindow;
        }
        public static ILocFile _locFile = Loc.GetFile("DomeShield_OutputRegulatorUI");
    }
}
