namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.AminoAcidBackbone" />
    public class AminoAcidBackbone : VisualisationComponent<Node.Calculator.AminoAcidBackbone>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}