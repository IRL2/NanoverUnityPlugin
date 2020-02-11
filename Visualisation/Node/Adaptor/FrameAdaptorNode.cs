// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Core;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// Visualisation node which reads frames using <see cref="IFrameConsumer" /> and
    /// dynamically provides the frame's data as properties for the visualisation
    /// system.
    /// </summary>
    /// <remarks>
    /// This visualisation node acts as the bridge between the underlying trajectory
    /// and the visualisation system.
    /// </remarks>
    [Serializable]
    public class FrameAdaptorNode : BaseAdaptorNode, IFrameConsumer
    {
        private Dictionary<string, IProperty> propertyOverrides =
            new Dictionary<string, IProperty>();

        /// <summary>
        /// Add a property with the given type and name to this adaptor that is not
        /// affected by the frame.
        /// </summary>
        public IProperty<TValue> AddCustomProperty<TValue>(string name)
        {
            var prop = new SerializableProperty<TValue>();
            propertyOverrides[name] = prop;
            return prop;
        }

        /// <inheritdoc cref="BaseAdaptorNode.GetProperty" />
        public override IReadOnlyProperty GetProperty(string key)
        {
            if (propertyOverrides.TryGetValue(key, out var value))
                return value;
            return base.GetProperty(key);
        }

        /// <inheritdoc cref="BaseAdaptorNode.GetProperties" />
        public override IEnumerable<(string name, IReadOnlyProperty property)> GetProperties()
        {
            foreach (var (key, prop) in propertyOverrides)
                yield return (key, prop);
            foreach (var (key, prop) in base.GetProperties())
                yield return (key, prop);
        }

        /// <inheritdoc cref="BaseAdaptorNode.GetOrCreateProperty{T}" />
        public override IReadOnlyProperty<T> GetOrCreateProperty<T>(string name)
        {
            foreach (var (key, prop) in propertyOverrides)
            {
                if (name == key && prop is IReadOnlyProperty<T> propAsT)
                    return propAsT;
            }

            return base.GetOrCreateProperty<T>(name);
        }

        /// <inheritdoc cref="BaseAdaptorNode.OnCreateProperty{T}" />
        protected override void OnCreateProperty<T>(string key, IProperty<T> property)
        {
            if (propertyOverrides.ContainsKey(key))
                return;
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
        private void OnFrameUpdated(IFrame frame, FrameChanges changes = null)
        {
            if (frame == null)
                return;

            foreach (var (key, property) in Properties)
            {
                if (propertyOverrides.ContainsKey(key))
                    return;
                if ((changes?.GetIsChanged(key) ?? true)
                 && FrameSource.CurrentFrame.Data.TryGetValue(key, out var value))
                    property.TrySetValue(value);
            }
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
                    OnFrameUpdated(source.CurrentFrame);
                }
            }
        }
    }
}