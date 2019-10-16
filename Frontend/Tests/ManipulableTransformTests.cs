// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Narupa.Frontend.Tests
{
    internal class ManipulableTransformTests
    {
        private static IEnumerable<bool> GrabReleaseSequence =>
            RandomTestData.SeededRandom(() => Random.value > 0.5f).Take(64);

        [Test]
        public void StartGrabManipulation_WithNoGrabs_AllowsGrab()
        {
            var transform = new GameObject("Test Transform").transform;
            var manipulable = new ManipulableTransform(transform);

            Assert.IsNotNull(manipulable.StartGrabManipulation(Transformation.Identity));
        }

        [Test]
        public void StartGrabManipulation_WithOneGrab_AllowsGrab()
        {
            var transform = new GameObject("Test Transform").transform;
            var manipulable = new ManipulableTransform(transform);

            var pose1 = Transformation.Identity;
            var pose2 = Transformation.Identity;
            pose2.Position += Vector3.right;

            manipulable.StartGrabManipulation(pose1);

            Assert.IsNotNull(manipulable.StartGrabManipulation(pose2));
        }

        [Test]
        public void StartGrabManipulation_WithTwoGrabs_RejectsGrab()
        {
            var transform = new GameObject("Test Transform").transform;
            var manipulable = new ManipulableTransform(transform);

            var pose1 = Transformation.Identity;
            var pose2 = Transformation.Identity;
            var pose3 = Transformation.Identity;
            pose2.Position += Vector3.right;
            pose3.Position += Vector3.up;

            manipulable.StartGrabManipulation(pose1);
            manipulable.StartGrabManipulation(pose2);

            Assert.IsNull(manipulable.StartGrabManipulation(pose3));
        }

        [Test]
        public void StartGrabManipulation_WithReleasedGrabs_AllowsGrab()
        {
            var transform = new GameObject("Test Transform").transform;
            var manipulable = new ManipulableTransform(transform);

            var pose1 = Transformation.Identity;
            var pose2 = Transformation.Identity;
            var pose3 = Transformation.Identity;
            pose2.Position += Vector3.right;
            pose3.Position += Vector3.up;

            manipulable.StartGrabManipulation(pose1);
            var grab = manipulable.StartGrabManipulation(pose2);
            grab.EndManipulation();

            Assert.IsNotNull(manipulable.StartGrabManipulation(pose3));
        }

        [Test]
        public void
            StartGrabManipulation_WithRandomGrabReleaseSequence_NeverAllowsMoreThanTwoGrabs()
        {
            var transform = new GameObject("Test Transform").transform;
            var manipulable = new ManipulableTransform(transform);
            var grabs = new List<IActiveManipulation>();

            foreach (var grabMore in GrabReleaseSequence)
            {
                if (grabMore)
                {
                    var pose = SpatialTestData.GetRandomTransformation();
                    var grab = manipulable.StartGrabManipulation(pose);

                    if (grab != null)
                    {
                        grabs.Add(grab);
                    }
                }
                else if (grabs.Count > 0)
                {
                    grabs[0].EndManipulation();
                    grabs.RemoveAt(0);
                }

                Assert.That(grabs.Count, Is.LessThanOrEqualTo(2));
            }
        }
    }
}