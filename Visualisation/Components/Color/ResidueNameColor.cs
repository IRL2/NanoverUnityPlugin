using NanoVer.Visualisation.Node.Color;

namespace NanoVer.Visualisation.Components.Color
{
    /// <inheritdoc cref="ResidueNameColorNode" />
    public class ResidueNameColor :
        VisualisationComponent<ResidueNameColorNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}