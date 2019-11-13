// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace Narupa.Visualisation.Components.Renderer
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Renderer.ParticleBondRenderer"/>
    public class ParticleBondRenderer : VisualisationComponentRenderer<Node.Renderer.ParticleBondRenderer>
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