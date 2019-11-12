using Narupa.Visualisation;
using Narupa.Visualisation.Components.Renderer;
using UnityEngine;

namespace Plugins.Narupa.Visualisation.Components.Renderer
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Renderer"/>
    public class GoodsellRenderer : VisualisationComponentRenderer<global::Narupa.Visualisation.Node.Renderer.GoodsellRenderer>
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