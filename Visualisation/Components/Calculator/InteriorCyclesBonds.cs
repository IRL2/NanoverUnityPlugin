namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.InteriorCyclesBonds" />
    public class InteriorCyclesBonds : VisualisationComponent<Node.Calculator.InteriorCyclesBonds>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}