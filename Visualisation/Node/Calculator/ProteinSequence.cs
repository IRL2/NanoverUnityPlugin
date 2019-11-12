using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Science;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Find protein sequences of standard amino acids.
    /// </summary>
    [Serializable]
    public class ProteinSequence : GenericOutputNode
    {
        [SerializeField]
        private StringArrayProperty residueNames = new StringArrayProperty();
        
        [SerializeField]
        private IntArrayProperty residueEntities = new IntArrayProperty();

        private SelectionArrayProperty peptideSequences = new SelectionArrayProperty();

        protected override bool IsInputDirty => residueNames.IsDirty 
                                             || residueEntities.IsDirty;

        protected override bool IsInputValid => residueNames.HasNonEmptyValue()
                                             && residueEntities.HasNonEmptyValue();

        protected override void ClearDirty()
        {
            residueNames.IsDirty = false;
            residueEntities.IsDirty = false;
        }

        protected override void ClearOutput()
        {
            peptideSequences.UndefineValue();
        }

        protected override void UpdateOutput()
        {
            var all = new List<List<int>>();
            var current = new List<int>();
            for (var i = 0; i < residueNames.Value.Length; i++)
            {
                if (AminoAcid.IsStandardAminoAcid(residueNames.Value[i]))
                {
                    if (current.Any())
                    {
                        var p1 = residueEntities.Value[i];
                        var p2 = residueEntities.Value[current.Last()];
                        if (p1 == p2)
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
        }
    }
}