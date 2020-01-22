using System;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    [Serializable]
    public class SecondaryStructureColor : VisualiserColorNode
    {
        [SerializeField]
        private SecondaryStructureArrayProperty residueSecondaryStructure;

        [SerializeField]
        private IntArrayProperty particleResidues;

        private UnityEngine.Color GetSecondaryStructureColor(SecondaryStructureAssignment i)
        {
            switch (i)
            {
                case SecondaryStructureAssignment.ThreeTenHelix:
                    return UnityEngine.Color.blue;
                case SecondaryStructureAssignment.AlphaHelix:
                    return UnityEngine.Color.magenta;
                case SecondaryStructureAssignment.PiHelix:
                    return UnityEngine.Color.red;
                case SecondaryStructureAssignment.Turn:
                    return UnityEngine.Color.cyan;
                case SecondaryStructureAssignment.Sheet:
                    return UnityEngine.Color.yellow;
                default:
                    return UnityEngine.Color.white;
            }
        }

        protected override bool IsInputValid => residueSecondaryStructure.HasNonEmptyValue() 
                                             && particleResidues.HasNonNullValue();

        protected override bool IsInputDirty => residueSecondaryStructure.IsDirty
                                             || particleResidues.IsDirty;

        protected override void ClearDirty()
        {
            residueSecondaryStructure.IsDirty = false;
            particleResidues.IsDirty = false;
        }

        protected override void ClearOutput()
        {
            colors.UndefineValue();
        }

        protected override void UpdateOutput()
        {
            var secondaryStructure = this.residueSecondaryStructure.Value;
            var residues = this.particleResidues.Value;
            var colorArray = colors.HasValue ? colors.Value : new UnityEngine.Color[0];
            Array.Resize(ref colorArray, residues.Length);
            for (var i = 0; i < residues.Length; i++)
                colorArray[i] = GetSecondaryStructureColor(secondaryStructure[residues[i]]);
            colors.Value = colorArray;
        }
    }
}