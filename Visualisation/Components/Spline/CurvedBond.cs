using NanoVer.Visualisation.Node.Spline;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="CurvedBondNode" />
    public class CurvedBond : VisualisationComponent<CurvedBondNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}