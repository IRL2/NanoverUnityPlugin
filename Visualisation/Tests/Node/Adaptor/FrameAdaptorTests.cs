using System;
using Narupa.Core.Science;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Protocol.Trajectory;
using Narupa.Visualisation.Node.Adaptor;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Visualisation.Tests.Node.Adaptor
{
    public class FrameAdaptorTests
    {
        public class FrameSnapshot : ITrajectorySnapshot
        {
            public void Update(Frame.Frame frame, FrameChanges changes)
            {
                CurrentFrame = frame;
                FrameChanged?.Invoke(frame, changes);
            }

            public Frame.Frame CurrentFrame { get; private set; }
            public event FrameChanged FrameChanged;
        }

        private FrameSnapshot source;

        private FrameAdaptorNode adaptor;

        private Action positionsChangedHandler;
        private Action elementsChangedHandler;
        private Action bondsChangedHandler;

        [SetUp]
        public void Setup()
        {
            source = new FrameSnapshot();
            adaptor = new FrameAdaptorNode { FrameSource = source };
            positionsChangedHandler = Substitute.For<Action>();
            adaptor.ParticlePositions.ValueChanged += positionsChangedHandler;
            elementsChangedHandler = Substitute.For<Action>();
            adaptor.ParticleElements.ValueChanged += elementsChangedHandler;
            bondsChangedHandler = Substitute.For<Action>();
            adaptor.BondPairs.ValueChanged += bondsChangedHandler;
        }

        [Test]
        public void PositionUpdated()
        {
            var positionArray = new[] { Vector3.up, Vector3.down };
            source.Update(new Frame.Frame
                          {
                              ParticlePositions = positionArray
                          },
                          new FrameChanges
                          {
                              [FrameData.ParticlePositionArrayKey] = true
                          });

            positionsChangedHandler.ReceivedWithAnyArgs(1).Invoke();
            elementsChangedHandler.ReceivedWithAnyArgs(0).Invoke();
            bondsChangedHandler.ReceivedWithAnyArgs(0).Invoke();

            CollectionAssert.AreEqual(positionArray, adaptor.ParticlePositions.Value);
        }

        [Test]
        public void ElementsUpdated()
        {
            var elementArray = new[] { Element.Hydrogen, Element.Carbon };
            source.Update(new Frame.Frame
                          {
                              ParticleElements = elementArray
                          },
                          new FrameChanges
                          {
                              [FrameData.ParticleElementArrayKey] = true
                          });

            positionsChangedHandler.ReceivedWithAnyArgs(0).Invoke();
            elementsChangedHandler.ReceivedWithAnyArgs(1).Invoke();
            bondsChangedHandler.ReceivedWithAnyArgs(0).Invoke();

            CollectionAssert.AreEqual(elementArray, adaptor.ParticleElements.Value);
        }

        [Test]
        public void BondsUpdated()
        {
            var bondArray = new[] { new BondPair(0, 1), new BondPair(1, 2) };
            source.Update(new Frame.Frame
                          {
                              BondPairs = bondArray
                          },
                          new FrameChanges
                          {
                              [FrameData.BondArrayKey] = true
                          });

            positionsChangedHandler.ReceivedWithAnyArgs(0).Invoke();
            elementsChangedHandler.ReceivedWithAnyArgs(0).Invoke();
            bondsChangedHandler.ReceivedWithAnyArgs(1).Invoke();

            CollectionAssert.AreEqual(bondArray, adaptor.BondPairs.Value);
        }
    }
}