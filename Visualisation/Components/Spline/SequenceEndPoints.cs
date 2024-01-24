using NanoVer.Visualisation.Node.Spline;

namespace NanoVer.Visualisation.Components.Spline
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