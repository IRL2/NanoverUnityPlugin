using System;
using System.Collections;
using System.Collections.Generic;
using Narupa.Visualisation.Property;

namespace Narupa.Visualisation.Node.Filter
{
    public abstract class VisualiserFilter : GenericOutputNode
    {
        protected readonly IntArrayProperty particleFilter = new IntArrayProperty();

        private int[] filter = new int[0];

        protected abstract int MaximumFilterCount { get; }
        
        protected override void UpdateOutput()
        {
            Array.Resize(ref filter, MaximumFilterCount);
            var i = 0;
            foreach (var f in GetFilteredIndices())
            {
                filter[i++] = f;
            }
            Array.Resize(ref filter, i);
            particleFilter.Value = filter;
        }
        
        protected override void ClearOutput()
        {
            Array.Resize(ref filter, 0);
            particleFilter.Value = filter;
        }


        protected abstract IEnumerable<int> GetFilteredIndices();

        /// <summary>
        /// List of particle indices parsed by this filter.
        /// </summary>
        public IReadOnlyProperty<int[]> ParticleFilter => particleFilter;
    }
}