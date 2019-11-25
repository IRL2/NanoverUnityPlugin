using System;
using System.Collections.Generic;
using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Frontend.Manipulation
{
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
        public IReadOnlyList<int> ParticleIndices => particleIndices;

        private readonly List<int> particleIndices = new List<int>();
        public Vector3 GrabPosition { get; private set; }

        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public event Action ParticleGrabUpdated;

        public ActiveParticleGrab(IEnumerable<int> particleIndices)
        {
            Id = Guid.NewGuid().ToString();
            this.particleIndices.AddRange(particleIndices);
        }

        /// <inheritdoc />
        public void UpdateManipulatorPose(Transformation manipulatorPose)
        {
            GrabPosition = manipulatorPose.Position;
            ParticleGrabUpdated?.Invoke();
        }

        /// <inheritdoc />
        public void EndManipulation()
        {
            ManipulationEnded?.Invoke();
        }

        private const string KeyResetVelocities = "reset_velocities";

        public bool ResetVelocities
        {
            set => Properties[KeyResetVelocities] = value;
        }
    }
}