using NanoVer.Visualisation.Node.Spline;

namespace NanoVer.Visualisation.Components.Spline
{
    /// <inheritdoc cref="PolypeptideCurveNode" />
    public class PolypeptideCurve : VisualisationComponent<PolypeptideCurveNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}