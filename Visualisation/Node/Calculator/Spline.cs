using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class Spline
    {
        [SerializeField]
        private ColorArrayProperty pointColors = new ColorArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty pointPositions = new Vector3ArrayProperty();

        private HermiteCurve[] curves;

        private SplineArrayProperty splineSegments = new SplineArrayProperty();

        [SerializeField]
        private float scale = 1;

        protected SplineSegment[] segments;

        [SerializeField]
        private SelectionArrayProperty pointSequences = new SelectionArrayProperty();

        private bool areSplinesDirty = false;

        [SerializeField]
        private float shape = 0.5f;

        protected virtual bool DoSegmentsNeedGenerating =>
            areSplinesDirty || pointPositions.IsDirty;

        protected virtual bool CanSegmentsBeGenerated =>
            pointPositions.HasValue;

        public void Refresh()
        {
            if ((pointSequences.IsDirty || pointColors.IsDirty))
            {
                SetupHermiteSplines();
                pointSequences.IsDirty = false;
                areSplinesDirty = true;
                pointColors.IsDirty = false;
            }

            if (DoSegmentsNeedGenerating && CanSegmentsBeGenerated)
            {
                GenerateSegments();
            }
        }

        protected virtual void GenerateSegments()
        {
            PreUpdateCurves();

            foreach (var curve in curves)
                curve.Update(this);

            GenerateSegmentsFromSplines();

            splineSegments.Value = segments;
            pointPositions.IsDirty = false;
            areSplinesDirty = false;
        }

        private void GenerateSegmentsFromSplines()
        {
            var offset = 0;
            foreach (var curve in curves)
            {
                curve.UpdateSegments(ref segments, offset, this);
                offset += curve.PointCount - 1;
            }
        }

        protected virtual void PreUpdateCurves()
        {
        }

        protected virtual void ModifyGeometry(HermiteCurve curve)
        {
        }

        private void SetupHermiteSplines()
        {
            var segmentCount = pointSequences.HasNonNullValue()
                                   ? pointSequences.Value.Sum(sequence => sequence.Count - 1)
                                   : pointPositions.Value.Length - 1;

            var curveCount = pointSequences.HasNonNullValue()
                                 ? pointSequences.Value.Length
                                 : 1;

            var sequences = pointSequences.HasNonNullValue()
                                ? pointSequences.Value
                                : new[]
                                {
                                    Enumerable.Range(0, segmentCount + 1).ToList()
                                };

            Array.Resize(ref segments, segmentCount);
            Array.Resize(ref curves, curveCount);
            var i = 0;
            foreach (var sequence in sequences)
            {
                var count = sequence.Count;

                // Setup initial HermiteCurve
                curves[i] = new HermiteCurve
                {
                    Positions = new Vector3[count],
                    Normals = new Vector3[count],
                    Tangents = new Vector3[count],
                    Sequence = sequence,
                    Colors = new UnityEngine.Color[count],
                    PointCount = count,
                    SequenceIndex = i
                };
                i++;
            }
        }

        /// <summary>
        /// Represents a continuous curve.
        /// </summary>
        protected class HermiteCurve
        {
            public IReadOnlyList<int> Sequence;
            public int PointCount;
            public Vector3[] Normals;
            public Vector3[] Positions;
            public Vector3[] Tangents;
            public UnityEngine.Color[] Colors;
            public int SequenceIndex;

            public void Update(Spline generator)
            {
                // Get positions from a generator, using indices from PointIndices.
                // Also initialise normals to zero.
                for (var j = 0; j < PointCount; j++)
                {
                    Positions[j] = generator.pointPositions.Value[Sequence[j]];
                    Colors[j] = generator.pointColors.HasValue
                                    ? generator.pointColors.Value[Sequence[j]]
                                    : UnityEngine.Color.white;
                    Normals[j] = Vector3.zero;
                }

                // Allow the generator to modify the positions before calculating the spline.
                generator.ModifyGeometry(this);

                // Calculate tangents based offsets between positions.
                for (var j = 1; j < PointCount - 1; j++)
                    Tangents[j] = generator.shape * (Positions[j + 1] - Positions[j - 1]);

                // Compute normals by rejection of second derivative of curve from tangent
                for (var i = 1; i < PointCount - 2; i++)
                {
                    var p1 = Positions[i];
                    var p2 = Positions[i + 1];

                    var m1 = Tangents[i];
                    var m2 = Tangents[i + 1];

                    var n0 = -6 * p1 - 4 * m1 + 6 * p2 - 2 * m2;
                    var n1 = 6 * p1 + 2 * m1 - 6 * p2 + 4 * m2;

                    n0 -= Vector3.Project(n0, m1);
                    n1 -= Vector3.Project(n1, m2);

                    Normals[i] += n0;
                    Normals[i + 1] += n1;
                }

                // Normalise each normal
                Normals[0] = Normals[0].normalized;

                // Flip the normals
                for (var j = 1; j < Positions.Length; j++)
                {
                    var a = Normals[j] - Vector3.Project(Normals[j], Tangents[j]);
                    var b = Vector3.Cross(Tangents[j], Normals[j]);
                    a = a.normalized;
                    b = b.normalized;
                    var dot1 = Vector3.Dot(Normals[j - 1], a);
                    var dot2 = Vector3.Dot(Normals[j - 1], b);

                    if (dot1 * dot1 > dot2 * dot2)
                    {
                        Normals[j] = a * Mathf.Sign(dot1);
                    }
                    else
                    {
                        Normals[j] = b * Mathf.Sign(dot2);
                    }
                }

                // Set the first and last tangents
                Tangents[0] = 2 * Vector3.Dot(Tangents[1], Tangents[2]) * Tangents[1] - Tangents[2];
                Tangents[PointCount - 1] = 2 * Tangents[PointCount - 2] - Tangents[PointCount - 3];

                // Set the first normal
                Normals[0] = (2 * Normals[1] - Normals[2]).normalized;
            }

            public void UpdateSegment(ref SplineSegment segment, int k, Spline generator)
            {
                segment.StartColor = Colors[k];
                segment.EndColor = Colors[k + 1];
                segment.StartPoint = Positions[k];
                segment.EndPoint = Positions[k + 1];
                segment.StartTangent = Tangents[k];
                segment.EndTangent = Tangents[k + 1];
                segment.StartNormal = Normals[k];
                segment.EndNormal = Normals[k + 1];
                segment.StartScale = Vector2.one;
                segment.EndScale = Vector2.one;
                generator.OnUpdateSegment(ref segment, k, this);
            }

            public void UpdateSegments(ref SplineSegment[] segment, int offset, Spline generator)
            {
                for (var i = 0; i < PointCount - 1; i++)
                {
                    UpdateSegment(ref segment[offset + i], i, generator);
                }
            }
        }

        protected virtual void OnUpdateSegment(ref SplineSegment segment,
                                               int index,
                                               HermiteCurve curve)
        {
        }
    }
}