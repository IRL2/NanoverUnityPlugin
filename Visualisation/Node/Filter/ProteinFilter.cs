// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Filter
{
    /// <summary>
    /// Filters particles by if they're in a standard amino acid, optionally excluding hydrogens.
    /// </summary>
    [Serializable]
    public class ProteinFilter : VisualiserFilter
    {
        [SerializeField]
        private BoolProperty includeHydrogens = new BoolProperty();
        
        [SerializeField]
        private IntArrayProperty particleResidues = new IntArrayProperty();
        
        [SerializeField]
        private ElementArrayProperty particleElements = new ElementArrayProperty();

        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();

        /// <summary>
        /// Should hydrogens be included?
        /// </summary>
        public IProperty<bool> IncludeHydrogens => includeHydrogens;

        /// <summary>
        /// The indices of the residue for each particle.
        /// </summary>
        public IProperty<int[]> ParticleResidues => particleResidues;

        /// <summary>
        /// The residue names.
        /// </summary>
        public IProperty<string[]> ResidueNames => residueNames;
        
        /// <summary>
        /// The particle elements.
        /// </summary>
        public IProperty<Element[]> ParticleElements => particleElements;

        /// <inheritdoc cref="GenericOutputNode.IsInputValid"/>
        protected override bool IsInputValid => particleResidues.HasNonEmptyValue()
                                             && residueNames.HasNonEmptyValue()
                                                && particleElements.HasNonEmptyValue()
        && includeHydrogens.HasNonNullValue();

        /// <inheritdoc cref="GenericOutputNode.IsInputDirty"/>
        protected override bool IsInputDirty => particleResidues.IsDirty
                                                || particleElements.IsDirty
                                             || residueNames.IsDirty
        || includeHydrogens.IsDirty;

        /// <inheritdoc cref="GenericOutputNode.ClearDirty"/>
        protected override void ClearDirty()
        {
            particleResidues.IsDirty = false;
            residueNames.IsDirty = false;
            includeHydrogens.IsDirty = false;
            particleElements.IsDirty = false;
        }

        /// <inheritdoc cref="VisualiserFilter.MaximumFilterCount"/>
        protected override int MaximumFilterCount => particleResidues.Value.Length;

        /// <inheritdoc cref="VisualiserFilter.GetFilteredIndices"/>
        protected override IEnumerable<int> GetFilteredIndices()
        {
            for (var i = 0; i < particleResidues.Value.Length; i++)
            {
                if (!includeHydrogens.Value && particleElements.Value[i] == Element.Hydrogen)
                    continue;
                if (AminoAcid.IsStandardAminoAcid(residueNames.Value[particleResidues.Value[i]]))
                    yield return i;
            }
        }
    }
}