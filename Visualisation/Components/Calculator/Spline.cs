namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.SplineNode" />
    public class Spline : VisualisationComponent<Node.Calculator.SplineNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}