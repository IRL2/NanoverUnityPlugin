using NanoVer.Visualisation.Node.Protein;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="ByEntitySequenceNode" />
    public class ByEntitySequence : VisualisationComponent<ByEntitySequenceNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}