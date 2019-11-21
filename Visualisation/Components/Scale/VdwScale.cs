// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

namespace Narupa.Visualisation.Components.Scale
{
    /// <inheritdoc cref="Node.Scale.VdwScale" />
    public sealed class VdwScale : VisualisationComponent<Node.Scale.VdwScale>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            node.Refresh();
        }

        private void Update()
        {
            node.Refresh();
        }
    }
}