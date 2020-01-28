// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Visualisation.Components.Renderer;
using Narupa.Visualisation.Node.Renderer;
using Narupa.Visualisation.Property;
using UnityEditor;
using UnityEngine;

namespace Narupa.Visualisation.Components.Visualiser
{
    /// <inheritdoc cref="ParticleSphereRendererNode" />
    public class ParticleSphereRenderer :
        VisualisationComponentRenderer<ParticleSphereRendererNode>
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