using Narupa.Visualisation.Components.Renderer;
using Narupa.Visualisation.Node.Renderer;
using UnityEngine;

namespace Plugins.Narupa.Visualisation.Components.Renderer
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Renderer" />
    public class GoodsellRenderer : VisualisationComponentRenderer<GoodsellRendererNode>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            node.Transform = transform;
            node.Cleanup();
            node.Setup();
        }

        private void OnDestroy()
        {
            node.Cleanup();
        }

        protected override void Render(Camera camera)
        {
            node.Render(camera);
        }

        protected override void UpdateInEditor()
        {
            base.UpdateInEditor();
            node.Cleanup();
        }
    }
}