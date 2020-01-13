using System;
using System.Linq;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Spline
{
    [Serializable]
    public class SplineNode : GenericOutputNode
    {
        [SerializeField]
        private SelectionArrayProperty sequences = new SelectionArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty positions = new Vector3ArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty normals = new Vector3ArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty tangents = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty colors = new ColorArrayProperty();

        [SerializeField]
        private FloatArrayProperty scales = new FloatArrayProperty();

        [SerializeField]
        private ColorProperty color;

        [SerializeField]
        private FloatProperty radius;

        private SplineArrayProperty splineSegments = new SplineArrayProperty();

        protected override bool IsInputValid => sequences.HasNonNullValue()
                                             && positions.HasNonNullValue()
                                             && normals.HasNonNullValue()
                                             && tangents.HasNonNullValue()
                                             && color.HasNonNullValue()
                                             && radius.HasNonNullValue();

        protected override bool IsInputDirty => sequences.IsDirty
                                             || positions.IsDirty
                                             || normals.IsDirty
                                             || tangents.IsDirty
                                             || colors.IsDirty
                                             || color.IsDirty
                                             || radius.IsDirty
                                             || scales.IsDirty;

        protected override void ClearDirty()
        {
            sequences.IsDirty = false;
            positions.IsDirty = false;
            normals.IsDirty = false;
            tangents.IsDirty = false;
            colors.IsDirty = false;
            color.IsDirty = false;
            radius.IsDirty = false;
            scales.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            var segmentCount = sequences.Value.Sum(s => s.Count - 1);
            var splineSegments = this.splineSegments.HasValue
                                     ? this.splineSegments.Value
                                     : new SplineSegment[0];
            Array.Resize(ref splineSegments, segmentCount);
            var offset = 0;
            var color = this.color.HasValue ? this.color.Value : UnityEngine.Color.white;
            foreach (var sequence in sequences.Value)
            {
                if (sequence.Count == 0)
                    continue;
                var sequenceLength = sequence.Count;

                var startPosition = positions.Value[sequence[0]];
                var startNormal = normals.Value[offset];
                var startTangent = tangents.Value[offset];
                var startColor =
                    color * (colors.HasValue ? colors.Value[offset] : UnityEngine.Color.white);
                var startSize = radius * (scales.HasValue ? scales.Value[offset] : 1f);


                for (var i = 0; i < sequenceLength - 1; i++)
                {
                    var endPosition = positions.Value[sequence[i + 1]];
                    var endNormal = normals.Value[offset + i + 1];
                    var endTangent = tangents.Value[offset + i + 1];
                    var endColor = color * (colors.HasValue
                                                ? colors.Value[offset + i + 1]
                                                : UnityEngine.Color.white);
                    var endSize = radius * (scales.HasValue ? scales.Value[offset + i + 1] : 1f);

                    splineSegments[offset + i] = new SplineSegment
                    {
                        StartPoint = startPosition,
                        StartNormal = startNormal,
                        StartTangent = startTangent,
                        StartColor = startColor,
                        StartScale = Vector3.one * startSize,
                        EndPoint = endPosition,
                        EndNormal = endNormal,
                        EndTangent = endTangent,
                        EndColor = endColor,
                        EndScale = Vector3.one * endSize
                    };

                    startPosition = endPosition;
                    startNormal = endNormal;
                    startTangent = endTangent;
                    startColor = endColor;
                    startSize = endSize;
                }

                offset += sequenceLength - 1;
            }

            this.splineSegments.Value = splineSegments;
        }

        protected override void ClearOutput()
        {
            splineSegments.UndefineValue();
        }
    }
}