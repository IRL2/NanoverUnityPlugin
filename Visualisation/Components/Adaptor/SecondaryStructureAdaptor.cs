using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Properties;

namespace Narupa.Visualisation.Components.Adaptor
{
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