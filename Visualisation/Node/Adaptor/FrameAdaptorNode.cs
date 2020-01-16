// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Protocol.Trajectory;
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
    public class FrameAdaptorNode : IFrameConsumer
    {
        /// <summary>
        /// Dynamic properties created by the system, with the keys corresponding to the keys in the frame's data.
        /// </summary>
        internal readonly Dictionary<string, Property.Property> properties =
            new Dictionary<string, Property.Property>();

        /// <summary>
        /// Get a property which has already been setup.
        /// </summary>
        /// <remarks>
        /// Returns null if a property with this key has not been defined yet.
        /// </remarks>
        public Property.Property GetExistingProperty(string key)
        {
            return properties.TryGetValue(key, out var value)
                       ? value
                       : null;
        }

        /// <summary>
        /// Get a frame property, or create one if it has not been defined yet.
        /// </summary>
        /// <remarks>
        /// Returns null if there is already a property with this name, but it's the wrong type.
        /// </remarks>
        public IReadOnlyProperty<T> GetOrCreateProperty<T>(string name)
        {
            if (properties.TryGetValue(name, out var existing))
                return existing as IReadOnlyProperty<T>;
            var property = new SerializableProperty<T>();
            properties[name] = property;
            OnCreateProperty<T>(name, property);
            return property;
        }

        /// <summary>
        /// Array of elements of the provided frame.
        /// </summary>
        public IReadOnlyProperty<Element[]> ParticleElements =>
            GetOrCreateProperty<Element[]>(FrameData.ParticleElementArrayKey);

        /// <summary>
        /// Array of particle positions of the provided frame.
        /// </summary>
        public IReadOnlyProperty<Vector3[]> ParticlePositions =>
            GetOrCreateProperty<Vector3[]>(FrameData.ParticlePositionArrayKey);

        /// <summary>
        /// Array of bonds of the provided frame.
        /// </summary>
        public IReadOnlyProperty<BondPair[]> BondPairs =>
            GetOrCreateProperty<BondPair[]>(FrameData.BondArrayKey);

        /// <summary>
        /// Array of bond orders of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int[]> BondOrders =>
            GetOrCreateProperty<int[]>(FrameData.BondOrderArrayKey);

        /// <summary>
        /// Array of particle residues of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int[]> ParticleResidues =>
            GetOrCreateProperty<int[]>(FrameData.ParticleResidueArrayKey);

        /// <summary>
        /// Array of particle names of the provided frame.
        /// </summary>
        public IReadOnlyProperty<string[]> ParticleNames =>
            GetOrCreateProperty<string[]>(FrameData.ParticleNameArrayKey);

        /// <summary>
        /// Array of residue names of the provided frame.
        /// </summary>
        public IReadOnlyProperty<string[]> ResidueNames =>
            GetOrCreateProperty<string[]>(FrameData.ResidueNameArrayKey);

        [SerializeField]
        private FrameAdaptorProperty parentAdaptor = new FrameAdaptorProperty();

        private void OnCreateProperty<T>(string key, SerializableProperty<T> property)
        {
            // Link to the parent adaptor
            if (parentAdaptor.HasNonNullValue())
            {
                property.LinkedProperty = parentAdaptor.Value
                                                           .Adaptor
                                                           .GetOrCreateProperty<T>(key);
            }
            else if (FrameSource?.CurrentFrame != null
             && FrameSource.CurrentFrame.Data.TryGetValue(key, out var value))
            {
                property.TrySetValue(value);
            }
        }

        public void Refresh()
        {
            if (parentAdaptor.IsDirty)
            {
                if (parentAdaptor.HasNonNullValue())
                {
                    foreach (var (key, property) in properties)
                        property.TrySetLinkedProperty(parentAdaptor.Value
                                                                   .Adaptor
                                                                   .GetExistingProperty(key));
                }
                else
                {
                    foreach (var (key, property) in properties)
                        property.TrySetLinkedProperty(null);
                }

                parentAdaptor.IsDirty = false;
            }
        }

        /// <summary>
        /// Callback for when the frame is changed. Updates the output properties
        /// selectively depending on if the field is marked as having changed.
        /// </summary>
        private void OnFrameUpdated(IFrame frame, FrameChanges changes = null)
        {
            if (parentAdaptor.HasNonNullValue())
                return;

            if (frame == null)
                return;

            foreach (var (key, property) in properties)
                if ((changes?.GetIsChanged(key) ?? true)
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
                    OnFrameUpdated(source.CurrentFrame);
                }
            }
        }
    }
}