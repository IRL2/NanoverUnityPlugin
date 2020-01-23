using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Properties;

namespace Narupa.Visualisation.Components.Adaptor
{
    /// <inheritdoc cref="FilteredAdaptorNode"/>
    public class FilteredAdaptor : FrameAdaptorComponent<FilteredAdaptorNode>
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