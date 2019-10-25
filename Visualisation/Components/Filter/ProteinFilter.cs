// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

namespace Narupa.Visualisation.Components.Filter
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Filter.ProteinFilter" />
    public sealed class ProteinFilter : VisualisationComponent<global::Narupa.Visualisation.Node.Filter.ProteinFilter>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}