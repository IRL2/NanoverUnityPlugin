// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

namespace Narupa.Visualisation.Components.Color
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Color.GradientColor" />
    public sealed class GradientColor : VisualisationComponent<Node.Color.GradientColor>
    {
        private void Update()
        {
            node.Refresh();
        }
    }
}