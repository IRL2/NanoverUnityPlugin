using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="ResidueInSystemFractionNode" />
    public class ResidueInSystemFraction : VisualisationComponent<ResidueInSystemFractionNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}