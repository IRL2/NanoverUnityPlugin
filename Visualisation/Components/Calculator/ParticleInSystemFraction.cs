using NanoVer.Visualisation.Node.Calculator;

namespace NanoVer.Visualisation.Components.Calculator
{
    /// <inheritdoc cref="ParticleInSystemFractionNode" />
    public class ParticleInSystemFraction : VisualisationComponent<ParticleInSystemFractionNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}