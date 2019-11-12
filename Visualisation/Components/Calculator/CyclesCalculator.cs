namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.CyclesCalculator" />
    public class CyclesCalculator : VisualisationComponent<Node.Calculator.CyclesCalculator>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}