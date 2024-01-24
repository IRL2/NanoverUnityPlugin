// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using NanoVer.Visualisation.Node.Filter;

namespace NanoVer.Visualisation.Components.Filter
{
    /// <inheritdoc cref="ResidueNameFilterNode" />
    public sealed class ResidueNameFilter : VisualisationComponent<ResidueNameFilterNode>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}