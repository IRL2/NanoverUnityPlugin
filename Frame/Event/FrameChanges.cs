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

        public bool this[string id]
        {
            get => GetIsChanged(id);
            set => SetIsChanged(id, value);
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