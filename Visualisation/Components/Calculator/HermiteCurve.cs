namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.HermiteCurveNode" />
    public class HermiteCurve : VisualisationComponent<Node.Calculator.HermiteCurveNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}