using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Filter
{
    [Serializable]
    public class ResidueNameFilter : VisualiserFilter
    {
        [SerializeField]
        StringProperty pattern = new StringProperty();

        [SerializeField]
        IntArrayProperty particleResidues = new IntArrayProperty();

        [SerializeField]
        StringArrayProperty residueNames = new StringArrayProperty();

        protected override bool IsInputValid => pattern.HasNonNullValue()
                                             && particleResidues.HasNonEmptyValue()
                                             && residueNames.HasNonEmptyValue();

        protected override bool IsInputDirty => pattern.IsDirty
                                             || particleResidues.IsDirty
                                             || residueNames.IsDirty;

        protected override void ClearDirty()
        {
            pattern.IsDirty = false;
            particleResidues.IsDirty = false;
            residueNames.IsDirty = false;
        }

        protected override int MaximumFilterCount => particleResidues.Value.Length;

        protected override IEnumerable<int> GetFilteredIndices()
        {
            var regex = new Regex(pattern.Value);
            for (var i = 0; i < particleResidues.Value.Length; i++)
            {
                if (regex.IsMatch(residueNames.Value[particleResidues.Value[i]]))
                    yield return i;
            }
        }
    }
}