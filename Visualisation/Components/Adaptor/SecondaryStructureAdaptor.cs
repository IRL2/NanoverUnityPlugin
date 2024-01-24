using NanoVer.Visualisation.Node.Adaptor;
using NanoVer.Visualisation.Properties;

namespace NanoVer.Visualisation.Components.Adaptor
{
    /// <inheritdoc cref="SecondaryStructureAdaptorNode"/>
    public class SecondaryStructureAdaptor : FrameAdaptorComponent<SecondaryStructureAdaptorNode>
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