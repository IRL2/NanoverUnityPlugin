// Copyright (c) 2019 Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Narupa.Core.Math;
using Narupa.Grpc.Trajectory;
using Narupa.Session;
using UnityEngine;

namespace Narupa.Frontend.Manipulation
{
    /// <summary>
    /// Situates and IMD simulation in a Unity transform and allows grab
    /// manipulations to begin and end particle interactions.
    /// </summary>
    public class ManipulableParticles
    {
        /// <summary>
        /// The force multiplier.
        /// </summary>
        public float ForceScale { get; set; } = 100f;

        // TODO: this should be exposed in a cleaner way
        public IEnumerable<ActiveParticleGrab> ActiveGrabs => activeGrabs;

        private readonly Transform transform;
        private readonly TrajectorySession trajectorySession;
        private readonly ImdSession imdSession;

        private readonly HashSet<ActiveParticleGrab> activeGrabs
            = new HashSet<ActiveParticleGrab>();

        public ManipulableParticles(Transform transform,
                                    TrajectorySession trajectorySession,
                                    ImdSession imdSession)
        {
            this.transform = transform;
            this.trajectorySession = trajectorySession;
            this.imdSession = imdSession;
        }

        /// <summary>
        /// Start a particle grab on whatever particle falls close to the position
        /// of the given grabber. Return either the manipulation or null if there was
        /// nothing grabbable.
        /// </summary>
        public IActiveManipulation StartParticleGrab(Transformation grabberPose)
        {
            if (trajectorySession.CurrentFrame == null
             || trajectorySession.CurrentFrame.ParticlePositions.Length == 0)
                return null;

            uint particleIndex = GetNearestParticle(grabberPose.Position);

            return StartParticleGrab(grabberPose, particleIndex);
        }

        /// <summary>
        /// Start a particle grab on a particular particle. Return either the
        /// manipulation or null if it could not be grabbed.
        /// </summary>
        public IActiveManipulation StartParticleGrab(Transformation grabberPose,
                                                     uint particleIndex)
        {
            var grab = new ActiveParticleGrab(this, particleIndex);
            grab.UpdateManipulatorPose(grabberPose);
            OnParticleGrabUpdated(grab);

            activeGrabs.Add(grab);

            return grab;
        }

        private void OnParticleGrabUpdated(ActiveParticleGrab grab)
        {
            var position = transform.InverseTransformPoint(grab.GrabPosition);

            imdSession.SetInteraction(grab.Id,
                                      position,
                                      forceScale: ForceScale,
                                      particles: grab.ParticleIndex);
        }

        private void EndParticleGrab(ActiveParticleGrab grab)
        {
            activeGrabs.Remove(grab);

            imdSession.UnsetInteraction(grab.Id);
        }

        private uint GetNearestParticle(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);

            var frame = trajectorySession.CurrentFrame;

            var bestSqrDistance = Mathf.Infinity;
            var bestParticleIndex = 0;

            for (var i = 0; i < frame.ParticlePositions.Length; ++i)
            {
                var particlePosition = frame.ParticlePositions[i];
                var sqrDistance = Vector3.SqrMagnitude(position - particlePosition);

                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    bestParticleIndex = i;
                }
            }

            return (uint) bestParticleIndex;
        }

        /// <summary>
        /// Represents a grab between a particle and world-space position
        /// </summary>
        // TODO: this was exposed last minute and should be refactored into 
        // something cleaner, and not expose EndManipulation publically. See
        // issue #74
        public class ActiveParticleGrab : IActiveManipulation
        {
            public event Action ManipulationEnded;

            public string Id { get; }
            public uint ParticleIndex { get; }
            public Vector3 GrabPosition { get; private set; }

            private readonly ManipulableParticles imdSimulator;

            public ActiveParticleGrab(ManipulableParticles imdSimulator,
                                      uint particleIndex)
            {
                Id = Guid.NewGuid().ToString();
                this.imdSimulator = imdSimulator;
                ParticleIndex = particleIndex;
            }

            /// <inheritdoc />
            public void UpdateManipulatorPose(Transformation manipulatorPose)
            {
                GrabPosition = manipulatorPose.Position;
                imdSimulator.OnParticleGrabUpdated(this);
            }

            /// <inheritdoc />
            public void EndManipulation()
            {
                imdSimulator.EndParticleGrab(this);
                ManipulationEnded?.Invoke();
            }
        }
    }
}