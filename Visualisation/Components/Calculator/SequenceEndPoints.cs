using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="SequenceEndPointsNode"/>
    public class SequenceEndPoints : VisualisationComponent<SequenceEndPointsNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}