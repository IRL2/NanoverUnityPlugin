using Narupa.Visualisation.Node.Protein;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="SecondaryStructureNode" />
    public class
        SecondaryStructureCalculator : VisualisationComponent<SecondaryStructureNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}