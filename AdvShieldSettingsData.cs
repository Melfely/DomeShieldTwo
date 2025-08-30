using AdvShields.Models;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.UserInterfaces;
using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.DataManagement.Packages;
using BrilliantSkies.DataManagement.Vars;
using BrilliantSkies.DataManagement.Attributes;

namespace AdvShields
{
    public class AdvShieldSettingsData : DataPackage
    {
        public AdvShieldSettingsData(uint uniqueId) : base(uniqueId)
        {
        }

        [Slider(0, "External drive factor {0}", "This is not controlled from the UI, but from the ACB", 1f, 10f, 0.01f)]
        public Var<float> ExternalDriveFactor { get; set; } = new VarFloatClamp(1f, 1f, 10f, NoLimitMode.None);

        [Slider(1, "Shield reactivates at {0}%", "The % of health that the shield will heal to before turning on after being disabled. A lower number will make the shield turn on quicker, but with less health", 10f, 100f, 1f, 300f)]
        public Var<float> ShieldReactivationPercent { get; set; } = new VarFloatClamp(40f, 0f, 100f, NoLimitMode.None);

        [Slider(2, "Shield strength {0}", "The strength of the Shield and how much power is used for the shield.", 1f, 10f, 0.1f, 1f)]
        public Var<float> ExcessDrive { get; set; } = new VarFloatClamp(1f, 1f, 10f, NoLimitMode.None);

        [Variable(3, "Shield type", "The type of the shield")]
        public Var<enumShieldDomeState> IsShieldOn { get; set; } = new Var<enumShieldDomeState>(enumShieldDomeState.On);

        [Variable(4, "Shield class", "The class of the shield")]
        public Var<enumShieldClassSelection> ShieldClass { get; set; } = new Var<enumShieldClassSelection>(enumShieldClassSelection.QH);

        [Slider(5, "Armour: {0}%", "The % of energy diverted from health into Armour class", 0f, 50f, 0.1f, 300f)]
        public Var<float> ArmourPercent { get; set; } = new VarFloatClamp(10, 0, 50, NoLimitMode.None);

        [Slider(6, "Regen: {0}%", "The % of energy diverted from health into Regeneration statistics", 0f, 50f, 0.1f, 300f)]
        public Var<float> RegenPercent { get; set; } = new VarFloatClamp(10, 0, 50, NoLimitMode.None);
    }
}
