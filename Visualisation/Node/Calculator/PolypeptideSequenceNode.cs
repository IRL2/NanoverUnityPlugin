using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Calculate sequences of polypeptides.
    /// </summary>
    [Serializable]
    public class PolypeptideSequenceNode : GenericOutputNode
    {
        [SerializeField]
        private StringArrayProperty atomNames = new StringArrayProperty();

        [SerializeField]
        private IntArrayProperty atomResidues = new IntArrayProperty();

        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();

        [SerializeField]
        private IntArrayProperty residueEntities = new IntArrayProperty();

        private SelectionArrayProperty residueSequences = new SelectionArrayProperty();

        private SelectionArrayProperty alphaCarbonSequences = new SelectionArrayProperty();

        protected override bool IsInputValid => atomNames.HasNonNullValue()
                                             && atomResidues.HasNonNullValue()
                                             && residueNames.HasNonNullValue()
                                             && residueEntities.HasNonNullValue();

        protected override bool IsInputDirty => atomNames.IsDirty
                                             || atomResidues.IsDirty
                                             || residueNames.IsDirty
                                             || residueEntities.IsDirty;

        protected override void ClearDirty()
        {
            atomNames.IsDirty = false;
            atomResidues.IsDirty = false;
            residueNames.IsDirty = false;
            residueEntities.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var residueSequences = new List<List<int>>();
            var alphaCarbonSequences = new List<List<int>>();
            var residueNames = this.residueNames.Value;
            var atomNames = this.atomNames.Value;
            var atomResidues = this.atomResidues.Value;
            var residueEntities = this.residueEntities.Value;
            var currentResidues = new List<int>();
            var currentAlphaCarbons = new List<int>();
            int currentEntity = -1;
            for (var atom = 0; atom < atomNames.Length; atom++)
            {
                if (!string.Equals(atomNames[atom], "ca",
                                   StringComparison.InvariantCultureIgnoreCase))
                    continue;
                var residue = atomResidues[atom];
                if (!AminoAcid.IsStandardAminoAcid(residueNames[residue]))
                    continue;
                var entity = residueEntities[residue];
                if (currentEntity != entity && currentResidues.Any())
                {
                    residueSequences.Add(currentResidues);
                    alphaCarbonSequences.Add(currentAlphaCarbons);
                    currentResidues = new List<int>();
                    currentAlphaCarbons = new List<int>();
                }

                currentEntity = entity;
                currentResidues.Add(residue);
                currentAlphaCarbons.Add(atom);
            }

            if (currentResidues.Any())
            {
                residueSequences.Add(currentResidues);
                alphaCarbonSequences.Add(currentAlphaCarbons);
            }

            this.residueSequences.Value = residueSequences.ToArray();
            this.alphaCarbonSequences.Value = alphaCarbonSequences.ToArray();
        }

        protected override void ClearOutput()
        {
            residueSequences.UndefineValue();
            alphaCarbonSequences.UndefineValue();
        }
    }
}