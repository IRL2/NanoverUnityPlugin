using NanoVer.Visualisation.Node.Protein;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="SecondaryStructureNode" />
    public class SecondaryStructure : VisualisationComponent<SecondaryStructureNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}