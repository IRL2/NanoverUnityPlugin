using NanoVer.Visualisation.Node.Spline;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="SplineNode" />
    public class Spline : VisualisationComponent<SplineNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}