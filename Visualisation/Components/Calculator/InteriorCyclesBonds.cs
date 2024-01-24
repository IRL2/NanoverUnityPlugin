using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="InteriorCyclesBondsNode" />
    public class InteriorCyclesBonds : VisualisationComponent<InteriorCyclesBondsNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}