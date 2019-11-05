using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Calculate residue positions based on alpha carbon backbone.
    /// </summary>
    [Serializable]
    public class AminoAcidBackbone
    {
        [SerializeField]
        private StringArrayProperty atomNames = new StringArrayProperty();

        [SerializeField]
        private IntArrayProperty atomResidues = new IntArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty atomPositions = new Vector3ArrayProperty();

        [SerializeField]
        private IntProperty residueCount = new IntProperty();

        private Vector3ArrayProperty residuePositions = new Vector3ArrayProperty();

        private Vector3[] cachedResiduePositions = new Vector3[0];

        private int[] alphaCarbonIndices = new int[0];

        private int[] residueSize = new int[0];

        public void Refresh()
        {
            if (residueCount.IsDirty && residueCount.HasValue)
            {
                if (cachedResiduePositions.Length != residueCount.Value)
                {
                    Array.Resize(ref cachedResiduePositions, residueCount.Value);
                    Array.Resize(ref alphaCarbonIndices, residueCount.Value);
                    Array.Resize(ref residueSize, residueCount.Value);
                }
            }

            var count = cachedResiduePositions.Length;

            if (count > 0 && (atomNames.IsDirty || atomResidues.IsDirty))
            {
                for (var i = 0; i < count; i++)
                {
                    alphaCarbonIndices[i] = -1;
                    residueSize[i] = 0;
                    cachedResiduePositions[i] = Vector3.zero;
                }

                for (var a = 0; a < atomNames.Value.Length; a++)
                {
                    var name = atomNames.Value[a];
                    if (name.Equals("ca", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var res = atomResidues.Value[a];
                        alphaCarbonIndices[res] = a;
                    }
                }

                atomNames.IsDirty = false;
                atomResidues.IsDirty = false;
            }

            if (count > 0 && atomPositions.IsDirty)
            {
                for (var res = 0; res < count; res++)
                {
                    var alpha = alphaCarbonIndices[res];
                    if (alpha >= 0)
                        cachedResiduePositions[res] = atomPositions.Value[alpha];
                }

                atomPositions.IsDirty = false;
                residuePositions.Value = cachedResiduePositions;
            }
        }
    }
}