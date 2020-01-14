using System;
using System.Linq;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
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
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty splineNormals = new Vector3ArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty splineTangents = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty splineColors = new ColorArrayProperty();

        [SerializeField]
        private FloatArrayProperty splineScales = new FloatArrayProperty();

        [SerializeField]
        private ColorProperty color;

        [SerializeField]
        private FloatProperty radius;

        private SplineArrayProperty splineSegments = new SplineArrayProperty();

        /// <inheritdoc cref="GenericOutputNode.IsInputValid"/>
        protected override bool IsInputValid => sequences.HasNonNullValue()
                                             && particlePositions.HasNonNullValue()
                                             && splineNormals.HasNonNullValue()
                                             && splineTangents.HasNonNullValue()
                                             && color.HasNonNullValue()
                                             && radius.HasNonNullValue();

        /// <inheritdoc cref="GenericOutputNode.IsInputDirty"/>
        protected override bool IsInputDirty => sequences.IsDirty
                                             || particlePositions.IsDirty
                                             || splineNormals.IsDirty
                                             || splineTangents.IsDirty
                                             || splineColors.IsDirty
                                             || color.IsDirty
                                             || radius.IsDirty
                                             || splineScales.IsDirty;

        /// <inheritdoc cref="GenericOutputNode.ClearDirty"/>
        protected override void ClearDirty()
        {
            sequences.IsDirty = false;
            particlePositions.IsDirty = false;
            splineNormals.IsDirty = false;
            splineTangents.IsDirty = false;
            splineColors.IsDirty = false;
            color.IsDirty = false;
            radius.IsDirty = false;
            splineScales.IsDirty = false;
        }

        /// <inheritdoc cref="GenericOutputNode.UpdateOutput"/>
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

                var startPosition = particlePositions.Value[sequence[0]];
                var startNormal = splineNormals.Value[offset];
                var startTangent = splineTangents.Value[offset];
                var startColor =
                    color * (splineColors.HasValue ? splineColors.Value[offset] : UnityEngine.Color.white);
                var startSize = radius * (splineScales.HasValue ? splineScales.Value[offset] : 1f);


                for (var i = 0; i < sequenceLength - 1; i++)
                {
                    var endPosition = particlePositions.Value[sequence[i + 1]];
                    var endNormal = splineNormals.Value[offset + i + 1];
                    var endTangent = splineTangents.Value[offset + i + 1];
                    var endColor = color * (splineColors.HasValue
                                                ? splineColors.Value[offset + i + 1]
                                                : UnityEngine.Color.white);
                    var endSize = radius * (splineScales.HasValue ? splineScales.Value[offset + i + 1] : 1f);

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

        /// <inheritdoc cref="GenericOutputNode.ClearOutput"/>
        protected override void ClearOutput()
        {
            splineSegments.UndefineValue();
        }
    }
}