// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using Narupa.Protocol.Trajectory;

namespace Narupa.Frame.Event
{
    /// <summary>
    /// A record of known changes to a Narupa frame. Everything is assumed
    /// unchanged unless explicitly set.
    /// </summary>
    public class FrameChanges
    {
        private readonly HashSet<string> changed = new HashSet<string>();

        /// <summary>
        /// Indicates whether any keys have been changed in comparison to a
        /// previous frame.
        /// </summary>
        public bool HasAnythingChanged => changed.Count > 0;

        /// <summary>
        /// Indicates whether the bonds have been changed in comparison to a
        /// previous frame.
        /// </summary>
        public bool HaveBondsChanged
        {
            get => GetIsChanged(FrameData.BondArrayKey);
            set => SetIsChanged(FrameData.BondArrayKey, value);
        }

        /// <summary>
        /// Indicates whether the particle elements have been changed in
        /// comparison to a previous frame.
        /// </summary>
        public bool HaveParticleElementsChanged
        {
            get => GetIsChanged(FrameData.ParticleElementArrayKey);
            set => SetIsChanged(FrameData.ParticleElementArrayKey, value);
        }

        /// <summary>
        /// Indicates whether the particle positions have been changed in
        /// comparison to a previous frame.
        /// </summary>
        public bool HaveParticlePositionsChanged
        {
            get => GetIsChanged(FrameData.ParticlePositionArrayKey);
            set => SetIsChanged(FrameData.ParticlePositionArrayKey, value);
        }

        /// <summary>
        /// Indicates whether the bond orders have been changed in
        /// comparison to a previous frame.
        /// </summary>
        public bool HaveBondOrdersChanged
        {
            get => GetIsChanged(FrameData.BondOrderArrayKey);
            set => SetIsChanged(FrameData.BondOrderArrayKey, value);
        }
        
        public bool HaveParticleResiduesChanged
        {
            get => GetIsChanged(FrameData.ParticleResidueArrayKey);
            set => SetIsChanged(FrameData.ParticleResidueArrayKey, value);
        }
        
        public bool HaveParticleNamesChanged
        {
            get => GetIsChanged(FrameData.ParticleNameArrayKey);
            set => SetIsChanged(FrameData.ParticleNameArrayKey, value);
        }
        
        public bool HaveResidueNamesChanged
        {
            get => GetIsChanged(FrameData.ResidueNameArrayKey);
            set => SetIsChanged(FrameData.ResidueNameArrayKey, value);
        }

        /// <summary>
        /// Merge another <see cref="FrameChanges" />, such that the update
        /// state reflects the combination of the two.
        /// </summary>
        public void MergeChanges(FrameChanges otherChanges)
        {
            changed.UnionWith(otherChanges.changed);
        }

        /// <summary>
        /// Check if the field with the given id as having been changed from the previous
        /// frame.
        /// </summary>
        public bool GetIsChanged(string id)
        {
            return changed.Contains(id);
        }

        /// <summary>
        /// Mark the field with the given id as having been changed from the previous
        /// frame.
        /// </summary>
        public void SetIsChanged(string id, bool changedState)
        {
            if (changedState)
                changed.Add(id);
            else
                changed.Remove(id);
        }
    }
}