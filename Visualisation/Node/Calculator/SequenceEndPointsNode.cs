using System;
using System.Collections.Generic;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class SequenceEndPointsNode : GenericOutputNode
    {
        [SerializeField]
        private SelectionArrayProperty sequences = new SelectionArrayProperty();

        private IntArrayProperty filters = new IntArrayProperty();

        protected override bool IsInputValid => sequences.HasValue;

        protected override bool IsInputDirty => sequences.IsDirty;

        protected override void ClearDirty()
        {
            sequences.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var list = new List<int>();
            foreach (var sequence in sequences.Value)
            {
                list.Add(sequence[0]);
                if (sequence.Count > 1)
                    list.Add(sequence[sequence.Count - 1]);
            }

            filters.Value = list.ToArray();
        }

        protected override void ClearOutput()
        {
            filters.UndefineValue();
        }
    }
}