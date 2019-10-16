// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using Narupa.Frame;

namespace Narupa.Visualisation.Components.Adaptor
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Adaptor.FrameAdaptor" />
    public sealed class FrameAdaptor : VisualisationComponent<Node.Adaptor.FrameAdaptor>,
                                       IFrameConsumer
    {
        /// <inheritdoc cref="IFrameConsumer.FrameSource" />
        public ITrajectorySnapshot FrameSource
        {
            set => node.FrameSource = value;
        }
    }
}