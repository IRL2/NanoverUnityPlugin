using System;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Adaptor
{
    /// <summary>
    /// Visualisation node which reads frames using <see cref="IFrameConsumer" /> and
    /// outputs the arrays and variables as <see cref="OutputProperty{TValue}" /> for
    /// other nodes.
    /// </summary>
    /// <remarks>
    /// This visualisation node acts as the bridge between the underlying trajectory
    /// and the visualisation system.
    /// </remarks>
    [Serializable]
    public class FrameAdaptor : IFrameConsumer
    {
        /// <summary>
        /// Array of elements of the provided frame.
        /// </summary>
        public IReadOnlyProperty<Element[]> ParticleElements => particleElements;

        private ElementArrayProperty particleElements = new ElementArrayProperty();

        /// <summary>
        /// Array of particle positions of the provided frame.
        /// </summary>
        public IReadOnlyProperty<Vector3[]> ParticlePositions => particlePositions;

        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        /// <summary>
        /// Array of bonds of the provided frame.
        /// </summary>
        public IReadOnlyProperty<BondPair[]> BondPairs => bondPairs;

        private BondArrayProperty bondPairs = new BondArrayProperty();


        /// <summary>
        /// Array of bond orders of the provided frame.
        /// </summary>
        public IReadOnlyProperty<int[]> BondOrders => bondOrders;

        private IntArrayProperty bondOrders = new IntArrayProperty();

        /// <summary>
        /// Callback for when the frame is changed. Updates the output properties
        /// selectively depending on if the field is marked as having changed.
        /// </summary>
        private void OnFrameUpdated(IFrame frame, FrameChanges changes = null)
        {
            if (frame == null)
                return;

            if (changes?.HaveParticlePositionsChanged ?? true)
                particlePositions.Value = FrameSource.CurrentFrame.ParticlePositions;

            if (changes?.HaveParticleElementsChanged ?? true)
                particleElements.Value = FrameSource.CurrentFrame.ParticleElements;

            if (changes?.HaveBondsChanged ?? true)
                bondPairs.Value = FrameSource.CurrentFrame.BondPairs;

            if (changes?.HaveBondOrdersChanged ?? true)
                bondOrders.Value = FrameSource.CurrentFrame.BondOrders;
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