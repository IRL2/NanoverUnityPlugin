using NanoVer.Visualisation.Node.Color;

namespace NanoVer.Visualisation.Components.Color
{
    /// <inheritdoc cref="Node.Color.SecondaryStructureColor" />
    public class SecondaryStructureColor :
        VisualisationComponent<Node.Color.SecondaryStructureColor>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}