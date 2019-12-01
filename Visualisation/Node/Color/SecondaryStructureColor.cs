using System;
using Narupa.Visualisation.Node.Protein;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Color
{
    [Serializable]
    public class SecondaryStructureColor : VisualiserColor
    {
        [SerializeField]
        private SecondaryStructureArrayProperty secondaryStructure;

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

        protected override bool IsInputValid => secondaryStructure.HasNonEmptyValue();

        protected override bool IsInputDirty => secondaryStructure.IsDirty;

        protected override void ClearDirty()
        {
            secondaryStructure.IsDirty = false;
        }

        protected override void ClearOutput()
        {
            colors.UndefineValue();
        }

        protected override void UpdateOutput()
        {
            var secondaryStructure = this.secondaryStructure.Value;
            var colorArray = colors.HasValue ? colors.Value : new UnityEngine.Color[0];
            Array.Resize(ref colorArray, secondaryStructure.Length);
            for (var i = 0; i < secondaryStructure.Length; i++)
                colorArray[i] = GetSecondaryStructureColor(secondaryStructure[i]);

            colors.Value = colorArray;
        }
    }
}