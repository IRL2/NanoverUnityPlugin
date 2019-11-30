namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.PolypeptideSequenceNode" />
    public class PolypeptideSequence : VisualisationComponent<Node.Calculator.PolypeptideSequenceNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}