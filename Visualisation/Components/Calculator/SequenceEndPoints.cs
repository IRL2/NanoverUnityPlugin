using Narupa.Visualisation.Node.Calculator;

namespace Narupa.Visualisation.Components.Calculator
{
    public class SequenceEndPoints : VisualisationComponent<SequenceEndPointsNode>
    {
        public void Update()
        {
            node.Refresh();
        }
    }
}