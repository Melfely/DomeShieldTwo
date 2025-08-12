using BrilliantSkies.Core.Help;
using BrilliantSkies.Core.Serialisation.Parameters.Prototypes;
using BrilliantSkies.Core.Widgets;
using BrilliantSkies.DataManagement.Attributes;
using BrilliantSkies.DataManagement.Packages;
using BrilliantSkies.DataManagement.Vars;
using UnityEngine;

namespace AdvShields
{
    public class AdvShieldVisualData : DataPackage
    {
        public AdvShieldVisualData(uint uniqueId) : base(uniqueId)
        {
        }

        [Slider(0, "Edge", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp Edge { get; set; } = new VarFloatClamp(9, 0, 100, NoLimitMode.Max);

        [Slider(1, "Fresnel", "A higher value increases center transparency", 0, 100, 0.05f)]
        public VarFloatClamp Fresnel { get; set; } = new VarFloatClamp(3, 0, 100, NoLimitMode.Max);

        [Slider(2, "Assemble Speed", "Makes the grid color more intense", 0, 3, 0.01f)]
        public VarFloatClamp AssembleSpeed { get; set; } = new VarFloatClamp(0.25f, 0, 3, NoLimitMode.None);

        [Slider(3, "Noise Factor", "What does this do?", 0, 100, 0.05f)]
        public VarFloatClamp NoiseFactor { get; set; } = new VarFloatClamp(20, 0, 100, NoLimitMode.Max);

        [Slider(4, "Static Flicker Speed", "What does this do?", 0, 100, 0.05f)]
        public VarFloatClamp StaticFlickerSpeed { get; set; } = new VarFloatClamp(1, 0, 10, NoLimitMode.Max);

        [Slider(5, "Wave Factor", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp SinWaveFactor { get; set; } = new VarFloatClamp(1, 0, 100, NoLimitMode.Max);

        [Slider(6, "Wave Speed", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp SinWaveSpeed { get; set; } = new VarFloatClamp(0.5f, 0, 100, NoLimitMode.Max);

        [Slider(7, "Wave Size", "Makes the grid color more intense", 0, 100, 0.05f)]
        public VarFloatClamp SinWaveSize { get; set; } = new VarFloatClamp(0.2f, 0, 100, NoLimitMode.Max);

        [Variable(8, "Base Color", "Makes the grid color more intense")]
        public VarColor BaseColor { get; set; } = new VarColor(Color.blue);

        [Variable(9, "Grid Color", "Makes the grid color more intense")]
        public VarColor GridColor { get; set; } = new VarColor(Color.yellow);

    }
}
