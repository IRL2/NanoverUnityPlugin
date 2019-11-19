namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.ResidueInSystemFraction" />
    public class ResidueInSystemFraction : VisualisationComponent<Node.Calculator.ResidueInSystemFraction>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}