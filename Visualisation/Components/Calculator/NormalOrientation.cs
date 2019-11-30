using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.NormalOrientationNode" />
    public class NormalOrientation : VisualisationComponent<NormalOrientationNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}