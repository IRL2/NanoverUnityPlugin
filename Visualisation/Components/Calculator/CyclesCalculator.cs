using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="CyclesCalculatorNode" />
    public class CyclesCalculator : VisualisationComponent<CyclesCalculatorNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}