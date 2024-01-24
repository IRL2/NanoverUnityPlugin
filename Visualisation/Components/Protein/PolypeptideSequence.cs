using NanoVer.Visualisation.Node.Protein;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="PolypeptideSequenceNode" />
    public class PolypeptideSequence : VisualisationComponent<PolypeptideSequenceNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}