// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Nanover.Visualisation.Node.Filter;

namespace Nanover.Visualisation.Components.Filter
{
    /// <inheritdoc cref="ProteinFilterNode" />
    public sealed class ProteinFilter : VisualisationComponent<ProteinFilterNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}