using NanoVer.Visualisation.Node.Spline;

namespace NanoVer.Visualisation.Components.Spline
{
    /// <inheritdoc cref="NormalOrientationNode" />
    public class NormalOrientation : VisualisationComponent<NormalOrientationNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}