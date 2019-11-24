namespace Narupa.Visualisation.Components.Color
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Color.ResidueNameColor" />
    public class ResidueNameColor :
        VisualisationComponent<Node.Color.ResidueNameColor>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}