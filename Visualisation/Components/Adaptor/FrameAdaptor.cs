// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Core;
using Narupa.Frame;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Property;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            node.Refresh();
        }

        private void Update()
        {
            node.Refresh();
        }

        /// <inheritdoc cref="IPropertyProvider.GetPotentialProperties" />
        public override IEnumerable<(string name, Type type)> GetPotentialProperties()
        {
            return StandardFrameProperties.All;
        }

        /// <inheritdoc cref="IPropertyProvider.GetProperties" />
        public override IEnumerable<(string name, IReadOnlyProperty property)> GetProperties()
        {
            foreach (var existing in base.GetProperties())
                yield return existing;
            foreach (var (key, property) in node.properties)
                yield return (key, property);
        }

        /// <inheritdoc cref="IPropertyProvider.GetProperty" />
        public override IReadOnlyProperty GetProperty(string name)
        {
            return base.GetProperty(name) ?? node.GetExistingProperty(name);
        }

        /// <inheritdoc cref="IPropertyProvider.GetOrCreateProperty{T}" />
        public override IReadOnlyProperty<T> GetOrCreateProperty<T>(string name)
        {
            if (GetProperty(name) is IReadOnlyProperty<T> property)
                return property;
            return node.GetOrCreateProperty<T>(name);
        }

        /// <inheritdoc cref="IPropertyProvider.CanProvideProperty{T}" />
        public override bool CanProvideProperty<T>(string name)
        {
            return true;
        }
    }
}