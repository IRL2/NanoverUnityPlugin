using Nanover.Visualisation.Node.Adaptor;
using Nanover.Visualisation.Properties;

namespace Nanover.Visualisation.Components.Adaptor
{
    /// <inheritdoc cref="ExtendedSecondaryStructureAdaptorNode"/>
    public class ExtendedSecondaryStructureAdaptor : FrameAdaptorComponent<ExtendedSecondaryStructureAdaptorNode>
    {
        protected override void OnDisable()
        {
            base.OnDisable();
            
            // Unlink the adaptor, preventing memory leaks
            node.ParentAdaptor.UndefineValue();
            node.Refresh();
        }
    }
}