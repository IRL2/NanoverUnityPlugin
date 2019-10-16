using Narupa.Visualisation.Components.Renderer;
using UnityEngine;

namespace Narupa.Visualisation.Components.Visualiser
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Renderer.ParticleSphereRenderer"/>
    public class ParticleSphereRenderer :
        VisualisationComponentRenderer<Node.Renderer.ParticleSphereRenderer>
    {
        private void Start()
        {
            node.Transform = transform;
        }

        private void OnDestroy()
        {
            node.Dispose();
        }

        protected override void Render(Camera camera)
        {
            node.Render(camera);
        }
        
        protected override void UpdateInEditor()
        {
            base.UpdateInEditor();
            node.ResetBuffers();
        }

    }
}