using Narupa.Visualisation.Node.Calculator;
using UnityEngine;

namespace Narupa.Visualisation.Components.Renderer
{
    public class SplineRenderer : VisualisationComponentRenderer<Node.Renderer.SplineRenderer>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
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