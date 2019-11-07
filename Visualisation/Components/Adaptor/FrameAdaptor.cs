// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Core;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Protocol.Trajectory;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Components.Adaptor
{
    /// <inheritdoc cref="Narupa.Visualisation.Node.Adaptor.FrameAdaptor" />
    public sealed class FrameAdaptor : VisualisationComponent<Node.Adaptor.FrameAdaptor>,
                                       IFrameConsumer,
                                       IPropertyProvider
    {
        /// <inheritdoc cref="IFrameConsumer.FrameSource" />
        public ITrajectorySnapshot FrameSource
        {
            set => node.FrameSource = value;
        }

        /// <summary>
        /// The wrapped <see cref="FrameAdaptor"/>.
        /// </summary>
        public Node.Adaptor.FrameAdaptor Adaptor => node;

        private void Update()
        {
            node.Refresh();
        }

        public IEnumerable<(string name, Type type)> GetPotentialProperties()
        {
            yield return (FrameData.BondArrayKey, typeof(BondPair[]));
            yield return (FrameData.BondOrderArrayKey, typeof(int[]));
            yield return (FrameData.ChainCountValueKey, typeof(int));
            yield return (FrameData.ChainNameArrayKey, typeof(string[]));
            yield return (FrameData.KineticEnergyValueKey, typeof(float));
            yield return (FrameData.ParticleCountValueKey, typeof(int));
            yield return (FrameData.ParticleElementArrayKey, typeof(Element[]));
            yield return (FrameData.ParticleNameArrayKey, typeof(string[]));
            yield return (FrameData.ParticlePositionArrayKey, typeof(Vector3[]));
            yield return (FrameData.ParticleResidueArrayKey, typeof(int[]));
            yield return (FrameData.ParticleTypeArrayKey, typeof(string[]));
            yield return (FrameData.PotentialEnergyValueKey, typeof(float));
            yield return (FrameData.ResidueChainArrayKey, typeof(int[]));
            yield return (FrameData.ResidueCountValueKey, typeof(int));
            yield return (FrameData.ResidueNameArrayKey, typeof(string[]));
        }

        public override IEnumerable<(string name, Property.Property property)> GetProperties()
        {
            foreach (var existing in base.GetProperties())
                yield return existing;
            foreach (var (key, property) in node.properties)
                yield return (key, property);
        }

        public override Property.Property GetProperty(string name)
        {
            return base.GetProperty(name) ?? node.GetProperty(name);
        }

        public override IReadOnlyProperty<T> GetOrCreateProperty<T>(string name)
        {
            if (GetProperty(name) is IReadOnlyProperty<T> property)
                return property;
            return node.GetOrCreateProperty<T>(name);
        }
    }
}