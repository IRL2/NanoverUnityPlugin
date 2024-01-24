using NanoVer.Visualisation.Components.Renderer;
using NanoVer.Visualisation.Node.Renderer;
using UnityEngine;

namespace NanoVer.Visualisation.Components.Renderer
{
    /// <inheritdoc cref="NanoVer.Visualisation.Node.Renderer" />
    public class GoodsellRenderer : VisualisationComponentRenderer<GoodsellRendererNode>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            node.Transform = transform;
            node.Dispose();
            node.Setup();
        }

        protected override void Render(Camera camera)
        {
            node.Render(camera);
        }

        protected override void UpdateInEditor()
        {
            base.UpdateInEditor();
            node.Dispose();
        }
    }
}