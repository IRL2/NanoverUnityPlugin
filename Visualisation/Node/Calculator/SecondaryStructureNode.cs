using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Frame;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class SecondaryStructureNode
    {
        [SerializeField]
        private Vector3ArrayProperty atomPositions;

        [SerializeField]
        private IntArrayProperty atomResidues;

        [SerializeField]
        private StringArrayProperty atomNames;

        [SerializeField]
        private SelectionArrayProperty peptideChains;

        [SerializeField]
        private DsspOptions dsspOptions = new DsspOptions();

        private bool needRecalculate = true;

        private SecondaryStructureArrayProperty residueSecondaryStructure =
            new SecondaryStructureArrayProperty();

        private BondArrayProperty hydrogenBonds =
            new BondArrayProperty();

        public List<ProteinResidueData[]> sequenceResidueData =
            new List<ProteinResidueData[]>();

        public bool IsInputValid => peptideChains.HasNonNullValue();

        public bool AreResiduesDirty
            => atomResidues.IsDirty || peptideChains.IsDirty || atomNames.IsDirty;

        public bool AreResiduesValid => atomResidues.HasNonEmptyValue() &&
                                        peptideChains.HasNonEmptyValue() &&
                                        atomNames.HasNonEmptyValue();

        public void Refresh()
        {
            if (IsInputValid)
            {
                if (AreResiduesDirty)
                {
                    if (AreResiduesValid)
                        UpdateResidues();
                }


                if (atomPositions.IsDirty)
                    UpdatePositions();

                if (needRecalculate || Time.frameCount % 30 == 0)
                    CalculateSecondaryStructure();
            }
        }

        private void UpdateResidues()
        {
            sequenceResidueData.Clear();
            foreach (var sequence in peptideChains.Value)
                sequenceResidueData.Add(
                    DsspAlgorithm.GetResidueData(sequence, atomResidues, atomNames));

            needRecalculate = true;
        }

        private void CalculateSecondaryStructure()
        {
            foreach (var peptideSequence in sequenceResidueData)
                DsspAlgorithm.CalculateSecondaryStructure(peptideSequence, dsspOptions);

            var peptideCount = peptideChains.Value.Sum(s => s.Count);

            var secondaryStruct = new SecondaryStructureAssignment[peptideCount];

            var index = 0;
            foreach (var sequence in sequenceResidueData)
            foreach (var data in sequence)
                secondaryStruct[index++] = data.SecondaryStructure;

            residueSecondaryStructure.Value = secondaryStruct;

            needRecalculate = false;

            var bonds = new List<BondPair>();
            var seqOffset = 0;
            foreach (var sequence in sequenceResidueData)
            {
                foreach (var data in sequence)
                    if (data.DonorHydrogenBondResidue != null)
                        bonds.Add(new BondPair(seqOffset + data.ordinal,
                                               seqOffset + data.DonorHydrogenBondResidue.ordinal));
                seqOffset += sequence.Length;
            }

            hydrogenBonds.Value = bonds.ToArray();
        }

        private void UpdatePositions()
        {
            foreach (var t in sequenceResidueData)
                DsspAlgorithm.UpdateResidueAtomPositions(atomPositions.Value, t);

            needRecalculate = true;
        }
    }
}