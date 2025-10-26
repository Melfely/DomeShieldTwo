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
    public class AdvShieldTransformData : DataPackage
    {
        public AdvShieldTransformData(uint uniqueId) : base(uniqueId)
        {
        }

        [Slider(0, "Length {0}m", "The length that the shield is projected. Larger shields consume more engine power.", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Length { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(1, "Width {0}m", "The width of the shield. Larger shields consume more engine power.", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Width { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(2, "Height {0}m", "The height of the shield. Larger shields consume more engine power.", 10f, 1500f, 0.5f, 100f)]
        public Var<float> Height { get; set; } = new VarFloatClamp(15f, 10f, 1500f, NoLimitMode.None);

        [Slider(3, "Left/Right {0}", "The X position of the shield. Shields further away from the center consume slightly more engine power.", -100f, 100f, 0.5f, 200f)]
        public Var<float> LocalPosX { get; set; } = new VarFloatClamp(0f, -100f, 100f, NoLimitMode.None);

        [Slider(4, "Up/Down {0}", "The Y position of the shield. Shields further away from the center consume slightly more engine power.", -100f, 100f, 0.5f, 200f)]
        public Var<float> LocalPosY { get; set; } = new VarFloatClamp(0f, -100f, 100f, NoLimitMode.None);

        [Slider(5, "Forward/Back {0}", "The Z position of the shield. Shields further away from the center consume slightly more engine power.", -100f, 100f, 0.5f, 200f)]
        public Var<float> LocalPosZ { get; set; } = new VarFloatClamp(0f, -100f, 100f, NoLimitMode.None);
    }
}
