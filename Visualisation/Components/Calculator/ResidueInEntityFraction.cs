namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.ResidueInEntityFraction" />
    public class ResidueInEntityFraction : VisualisationComponent<Node.Calculator.ResidueInEntityFraction>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}