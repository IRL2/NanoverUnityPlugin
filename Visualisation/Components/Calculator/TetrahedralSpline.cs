using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.TetrahedralSplineNode" />
    public class TetrahedralSpline : VisualisationComponent<TetrahedralSplineNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}