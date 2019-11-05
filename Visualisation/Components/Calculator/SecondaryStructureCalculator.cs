namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.SecondaryStructureCalculator" />
    public class
        SecondaryStructureCalculator : VisualisationComponent<
            Node.Calculator.SecondaryStructureCalculator>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}