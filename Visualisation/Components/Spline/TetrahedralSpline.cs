using NanoVer.Visualisation.Node.Spline;

namespace NanoVer.Visualisation.Components.Spline
{
    /// <inheritdoc cref="TetrahedralSplineNode" />
    public class TetrahedralSpline : VisualisationComponent<TetrahedralSplineNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}