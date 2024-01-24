// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using NanoVer.Visualisation.Components.Renderer;
using NanoVer.Visualisation.Node.Renderer;
using UnityEngine;

namespace NanoVer.Visualisation.Components.Visualiser
{
    /// <inheritdoc cref="ParticleSphereRendererNode" />
    public class ParticleSphereRenderer :
        VisualisationComponentRenderer<ParticleSphereRendererNode>
    {
        private void Start()
        {
            node.Transform = transform;
        }

        protected override void OnDestroy()
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