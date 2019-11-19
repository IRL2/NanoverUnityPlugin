namespace Narupa.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Calculator.ParticleInSystemFraction" />
    public class ParticleInSystemFraction : VisualisationComponent<Node.Calculator.ParticleInSystemFraction>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}