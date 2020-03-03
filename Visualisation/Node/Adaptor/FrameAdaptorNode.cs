// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Protocol.Trajectory;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// Visualisation node which reads frames using <see cref="IFrameConsumer" /> and
    /// dynamically provides the frame's data as properties for the visualisation system.
    /// </summary>
    /// <remarks>
    /// This visualisation node acts as the bridge between the underlying trajectory
    /// and the visualisation system.
    /// </remarks>
    [Serializable]
    public class FrameAdaptorNode : BaseAdaptorNode, IFrameConsumer
    {
        protected override void OnCreateProperty<T>(string key, IProperty<T> property)
        {
            if (FrameSource?.CurrentFrame != null
                  && FrameSource.CurrentFrame.Data.TryGetValue(key, out var value))
            {
                property.TrySetValue(value);
            }
        }

        /// <summary>
        /// Callback for when the frame is changed. Updates the output properties
        /// selectively depending on if the field is marked as having changed.
        /// </summary>
        private void OnFrameUpdated(IFrame frame, FrameChanges changes)
        {
            if (frame == null)
                return;

            foreach (var (key, property) in Properties)
                if (changes.HasChanged(key)
                 && FrameSource.CurrentFrame.Data.TryGetValue(key, out var value))
                    property.TrySetValue(value);
        }

        private ITrajectorySnapshot source;

        /// <inheritdoc cref="IFrameConsumer.FrameSource" />
        public ITrajectorySnapshot FrameSource
        {
            get => source;
            set
            {
                if (source != null)
                    source.FrameChanged -= OnFrameUpdated;
                source = value;
                if (source != null)
                {
                    source.FrameChanged += OnFrameUpdated;
                    OnFrameUpdated(source.CurrentFrame, FrameChanges.All);
                }
            }
        }
    }
}