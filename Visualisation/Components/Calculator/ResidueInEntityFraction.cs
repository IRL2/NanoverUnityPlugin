using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="ResidueInEntityFractionNode" />
    public class ResidueInEntityFraction : VisualisationComponent<ResidueInEntityFractionNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}