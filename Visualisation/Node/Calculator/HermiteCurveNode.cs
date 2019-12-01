using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Calculates normals and tangents for splines going through a set of points.
    /// </summary>
    [Serializable]
    public class HermiteCurveNode : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty positions = new Vector3ArrayProperty();

        [SerializeField]
        private SelectionArrayProperty sequences = new SelectionArrayProperty();


        private Vector3ArrayProperty normals = new Vector3ArrayProperty();

        private Vector3ArrayProperty tangents = new Vector3ArrayProperty();

        [SerializeField]
        private FloatProperty shape = new FloatProperty
        {
            Value = 1f
        };

        public void CalculatePositions(IReadOnlyList<int> sequence,
                                       IReadOnlyList<Vector3> positions,
                                       int offset,
                                       ref Vector3[] normals,
                                       ref Vector3[] tangents,
                                       float shape)
        {
            var count = sequence.Count;

            // Calculate tangents based offsets between positions.
            for (var j = 1; j < count - 1; j++)
                tangents[offset + j] =
                    shape * (positions[sequence[j + 1]] - positions[sequence[j - 1]]);

            tangents[offset] = tangents[offset + 1];
            tangents[offset + count - 1] = tangents[offset + count - 2];
/*
            // Set the first and last tangents
            tangents[offset] =
                2 * Vector3.Dot(tangents[offset + 1], tangents[offset + 2]) * tangents[offset + 1] -
                tangents[offset + 2];
            tangents[offset + count - 1] =
                2 * tangents[offset + count - 2] - tangents[offset + count - 3];
                */

            // Compute normals by rejection of second derivative of curve from tangent
            for (var i = 1; i < count - 2; i++)
            {
                var p1 = positions[sequence[i]];
                var p2 = positions[sequence[i + 1]];

                var m1 = tangents[offset + i];
                var m2 = tangents[offset + i + 1];

                var n0 = -6 * p1 - 4 * m1 + 6 * p2 - 2 * m2;
                var n1 = 6 * p1 + 2 * m1 - 6 * p2 + 4 * m2;

                n0 -= Vector3.Project(n0, m1);
                n1 -= Vector3.Project(n1, m2);

                normals[offset + i] += n0;
                normals[offset + i + 1] += n1;
            }

            for (var i = 1; i < count - 1; i++)
            {
                normals[offset + i] = normals[offset + i].normalized;
            }

            // Set the first normal
            normals[offset] = (2 * normals[offset + 1] - normals[offset + 2]).normalized;
        }

        protected override bool IsInputValid => sequences.HasNonNullValue()
                                             && positions.HasNonNullValue()
                                             && shape.HasNonNullValue();

        protected override bool IsInputDirty => sequences.IsDirty
                                             || positions.IsDirty
                                             || shape.IsDirty;

        protected override void ClearDirty()
        {
            sequences.IsDirty = false;
            positions.IsDirty = false;
            shape.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            if (sequences.IsDirty)
            {
                var totalCount = sequences.Value.Sum(a => a.Count);
                var normals = this.normals.HasValue ? this.normals.Value : new Vector3[0];
                var tangents = this.tangents.HasValue ? this.tangents.Value : new Vector3[0];
                Array.Resize(ref normals, totalCount);
                Array.Resize(ref tangents, totalCount);
                this.normals.Value = normals;
                this.tangents.Value = tangents;
                positions.IsDirty = true;
            }

            if (positions.IsDirty)
            {
                var offset = 0;
                var normals = this.normals.Value;
                var tangents = this.tangents.Value;
                foreach (var sequence in sequences.Value)
                {
                    CalculatePositions(sequence,
                                       positions.Value,
                                       offset,
                                       ref normals,
                                       ref tangents,
                                       shape);
                    offset += sequence.Count - 1;
                }
            }
        }

        protected override void ClearOutput()
        {
            normals.UndefineValue();
            tangents.UndefineValue();
        }
    }
}