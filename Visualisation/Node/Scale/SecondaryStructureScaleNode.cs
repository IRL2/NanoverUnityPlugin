using System;
using Narupa.Visualisation.Node.Calculator;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Node.Scale;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Scale
{
    /// <summary>
    /// Provides scales that depend on the secondary structure.
    /// </summary>
    [Serializable]
    public class SecondaryStructureScaleNode : VisualiserScale
    {
        [SerializeField]
        private SecondaryStructureArrayProperty secondaryStructure;

        [SerializeField]
        private float radius;

        [SerializeField]
        private float helixRadius;

        [SerializeField]
        private float turnRadius;

        [SerializeField]
        private float sheetRadius;

        private float GetSecondaryStructureScale(SecondaryStructureAssignment i)
        {
            switch (i)
            {
                case SecondaryStructureAssignment.ThreeTenHelix:
                    return helixRadius;
                case SecondaryStructureAssignment.AlphaHelix:
                    return helixRadius;
                case SecondaryStructureAssignment.PiHelix:
                    return helixRadius;
                case SecondaryStructureAssignment.Turn:
                    return turnRadius;
                case SecondaryStructureAssignment.Sheet:
                    return sheetRadius;
                default:
                    return radius;
            }
        }


        protected override bool IsInputValid => secondaryStructure.HasNonEmptyValue();

        protected override bool IsInputDirty => secondaryStructure.IsDirty;

        protected override void ClearDirty()
        {
            secondaryStructure.IsDirty = false;
        }

        protected override void ClearOutput()
        {
            scales.UndefineValue();
        }

        protected override void UpdateOutput()
        {
            var secondaryStructure = this.secondaryStructure.Value;
            var scaleArray = scales.Resize(secondaryStructure.Length);
            for (var i = 0; i < secondaryStructure.Length; i++)
                scaleArray[i] = GetSecondaryStructureScale(secondaryStructure[i]);

            scales.Value = scaleArray;
        }
    }
}