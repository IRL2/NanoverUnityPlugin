using UnityEngine;

namespace Narupa.Visualisation.Components.Renderer
{
    public class CyclesRenderer : VisualisationComponentRenderer<Node.Renderer.CyclesRenderer>
    {
        private void Start()
        {
            node.Transform = transform;
        }

        protected override void Render(Camera camera)
        {
            if (camera.name == "Preview Scene Camera")
                return;
            node.Render(camera);
        }
    }
}