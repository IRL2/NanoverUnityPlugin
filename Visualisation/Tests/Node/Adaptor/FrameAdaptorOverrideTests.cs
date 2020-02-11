using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Collections;
using Narupa.Frame;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Property;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Visualisation.Tests.Node.Adaptor
{
    public class FrameAdaptorOverrideTests
    {
        private FrameSnapshot source;
        private FrameAdaptorNode adaptor;
        private Frame.Frame frame;
        private IReadOnlyProperty<Vector3[]> property;

        private static readonly Vector3[] FramePositions =
        {
            Vector3.up, Vector3.left, Vector3.down
        };
        
        private static readonly Vector3[] OverridePositions = 
        {
            Vector3.right, Vector3.zero, Vector3.back
        };

        [SetUp]
        public void Setup()
        {
            source = new FrameSnapshot();
            adaptor = new FrameAdaptorNode
            {
                FrameSource = source
            };
            frame = new Frame.Frame
            {
                ParticlePositions = FramePositions
            };
        }

        [Test]
        public void NoOverrideGetPropertyFirst()
        {
            var prop = adaptor.ParticlePositions;
            source.Update(frame);
            CollectionAssert.AreEqual(FramePositions, prop.Value);
        }
        
        [Test]
        public void NoOverrideGetPropertyAfter()
        {
            source.Update(frame);
            var prop = adaptor.ParticlePositions;
            CollectionAssert.AreEqual(FramePositions, prop.Value);
        }

        private static IEnumerable<IEnumerable<Action<FrameAdaptorOverrideTests>>> GetActions()
        {
            var set = new Action<FrameAdaptorOverrideTests>[]
            {
                GetProperty, UpdateFrame, AddOverride
            };
            return set.GetPermutations().Select(s => s.AsPretty(t => t.Method.Name));
        }

        private static void GetProperty(FrameAdaptorOverrideTests test)
        {
            test.property = test.adaptor.ParticlePositions;
        }

        private static void UpdateFrame(FrameAdaptorOverrideTests test)
        {
            test.source.Update(test.frame);
        }
        
        private static void AddOverride(FrameAdaptorOverrideTests test)
        {
            var @override = test.adaptor.AddCustomProperty<Vector3[]>(StandardFrameProperties.ParticlePositions.Key);
            @override.Value = OverridePositions;
        }

        [Test]
        public void OverrideBeforeGetPropertyFirst([ValueSource(nameof(GetActions))] IEnumerable<Action<FrameAdaptorOverrideTests>> actions)
        {
            foreach (var action in actions)
                action(this);
            
            CollectionAssert.AreEqual(OverridePositions, property.Value);
        }
    }
}