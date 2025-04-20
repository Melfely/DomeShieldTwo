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
    public class AdvShieldData : DataPackage
    {
        public AdvShieldData(uint uniqueId) : base(uniqueId)
        {
        }

        [Slider(0, "External drive factor {0}", "This is not controlled from the UI, but from the ACB", 1f, 10f, 0.01f)]
        public Var<float> ExternalDriveFactor { get; set; } = new VarFloatClamp(1f, 1f, 10f, NoLimitMode.None);

        [Slider(1, "Length {0}m", "The length that the shield is projected. Larger shields consume more engine power.", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Length { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(2, "Width {0}m", "The width of the shield. Larger shields consume more engine power.", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Width { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(3, "Height {0}m", "The height of the shield. Larger shields consume more engine power.", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Height { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(4, "Left/Right {0}", "The X position of the shield. Shields further away from the center consume slightly more engine power.", -100f, 100f, 0.5f, 200f)]
        public Var<float> LocalPosX { get; set; } = new VarFloatClamp(0f, -100f, 100f, NoLimitMode.None);

        [Slider(5, "Up/Down {0}", "The Y position of the shield. Shields further away from the center consume slightly more engine power.", -100f, 100f, 0.5f, 200f)]
        public Var<float> LocalPosY { get; set; } = new VarFloatClamp(0f, -100f, 100f, NoLimitMode.None);

        [Slider(6, "Forward/Back {0}", "The Z position of the shield. Shields further away from the center consume slightly more engine power.", -100f, 100f, 0.5f, 200f)]
        public Var<float> LocalPosZ { get; set; } = new VarFloatClamp(0f, -100f, 100f, NoLimitMode.None);

        [Variable(7, "Passive Regen", "Set passive regen to the absolute minimum (if your engine cannot handle it)")]
        public Var<enumPassiveRegenState> IsPassiveOn { get; set; } = new Var<enumPassiveRegenState>(enumPassiveRegenState.On);

        [Slider(8, "Shield reactivates at {0}%", "The % of health that the shield will heal to before turning on after being disabled. A lower number will make the shield turn on quicker, but with less health", 10f, 100f, 1f, 300f)]
        public Var<float> ShieldReactivationPercent { get; set; } = new VarFloatClamp(40f, 0f, 100f, NoLimitMode.None);

        [Slider(9, "Shield strength {0}", "The strength of the Shield and how much power is used for the shield.", 1f, 10f, 0.1f, 1f)]
        public Var<float> ExcessDrive { get; set; } = new VarFloatClamp(1f, 1f, 10f, NoLimitMode.None);
        
        /*
        [Slider(8, "Azimuth angle {0}°", "The azimuth angle of the shield", -45f, 45f, 0.1f, 0.0f)]
        public Var<float> Azimuth { get; set; } = new VarFloatClamp(0.0f, -45f, 45f, NoLimitMode.None);

        [Slider(9, "Elevation angle {0}°", "The elevation angle of the shield", -45f, 45f, 0.1f, 0.0f)]
        public Var<float> Elevation { get; set; } = new VarFloatClamp(0.0f, -45f, 45f, NoLimitMode.None);
        */

        [Variable(10, "Shield type", "The type of the shield")]
        public Var<enumShieldDomeState> IsShieldOn { get; set; } = new Var<enumShieldDomeState>(enumShieldDomeState.On);

        [Variable(11, "Shield class", "The class of the shield")]
        public Var<enumShieldClassSelection> ShieldClass { get; set; } = new Var<enumShieldClassSelection>(enumShieldClassSelection.QH);
    }
}
