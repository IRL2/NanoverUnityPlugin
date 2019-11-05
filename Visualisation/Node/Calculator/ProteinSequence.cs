using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class ProteinSequence
    {
        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();
        
        [SerializeField]
        private Vector3ArrayProperty residuePositions = new Vector3ArrayProperty();

        private SelectionArrayProperty peptideSequences = new SelectionArrayProperty();

        [SerializeField]
        private float cutoff = 1f;

        public void Refresh()
        {
            if ((residueNames.IsDirty || residuePositions.IsDirty) 
                && residueNames.HasNonEmptyValue() 
                && residuePositions.HasNonEmptyValue())
            {
                var all = new List<List<int>>();
                var current = new List<int>();
                for (var i = 0; i < residueNames.Value.Length; i++)
                {
                    if (AminoAcid.IsStandardAminoAcid(residueNames.Value[i]))
                    {
                        if (current.Any())
                        {
                            var p1 = residuePositions.Value[i];
                            var p2 = residuePositions.Value[current.Last()];
                            if (Vector3.Distance(p1, p2) < cutoff)
                            {
                                current.Add(i);
                            }
                            else
                            {
                                all.Add(current);
                                current = new List<int>();
                            }
                        }
                        else
                        {
                            current.Add(i);
                        }
                        
                    }
                        
                }
                all.Add(current);

                peptideSequences.Value = all.ToArray();
                residueNames.IsDirty = false;
                residuePositions.IsDirty = false;
            }
        }
    }
}