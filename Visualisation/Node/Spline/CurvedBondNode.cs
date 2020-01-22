using System;
using Narupa.Visualisation.Properties.Collections;
using UnityEngine;

namespace Narupa.Visualisation.Node.Spline
{
    [Serializable]
    public class CurvedBondNode : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty particlePositions = new Vector3ArrayProperty();

        [SerializeField]
        private ColorArrayProperty particleColors = new ColorArrayProperty();

        [SerializeField]
        private BondArrayProperty bondPairs = new BondArrayProperty();

        private SplineArrayProperty splineSegment = new SplineArrayProperty();

        protected override bool IsInputValid => particlePositions.HasValue && bondPairs.HasValue;

        protected override bool IsInputDirty =>
            particlePositions.IsDirty || bondPairs.IsDirty || particleColors.IsDirty;

        private Vector3[] offsetArray = new Vector3[0];
        private int[] offsetCount = new int[0];

        protected override void ClearDirty()
        {
            particlePositions.IsDirty = false;
            bondPairs.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            if (bondPairs.IsDirty)
            {
                Array.Resize(ref offsetArray, particlePositions.Value.Length);
                Array.Resize(ref offsetCount, particlePositions.Value.Length);
            }

            var bondcount = bondPairs.Value.Length;
            var particleCount = particlePositions.Value.Length;
            
            for (var i = 0; i < particleCount; i++)
            {
                offsetArray[i] = Vector3.zero;
                offsetCount[i] = 0;
            }

            foreach (var bond in bondPairs)
            {
                var a = bond.A;
                var b = bond.B;
                var dir = particlePositions.Value[b] - particlePositions.Value[a];
                offsetArray[a] += dir;
                offsetArray[b] -= dir;
                offsetCount[a]++;
                offsetCount[b]++;
            }

            splineSegment.Resize(bondcount);
            var l = 0;
            foreach (var bond in bondPairs)
            {
                var offset1 = offsetArray[bond.A];
                var offset2 = offsetArray[bond.B];
                var offsetC1 = offsetCount[bond.A];
                var offsetC2 = offsetCount[bond.B];

                var pos1 = particlePositions[bond.A];
                var pos2 = particlePositions[bond.B];
                var dir = pos2 - pos1;

                var tan1 = dir;
                var tan2 = dir;

                if (offsetC1 > 1)
                {
                    var mag = offset1.magnitude / offsetC1;
                    tan1 -= Mathf.Clamp01(15f * mag) * Vector3.Project(tan1, offset1 / offsetC1);
                }

                if (offsetC2 > 1)
                {
                    var mag = offset2.magnitude / offsetC2;
                    tan2 -= Mathf.Clamp01(15f * mag) *  Vector3.Project(tan2, offset2 / offsetC2);
                }

                var norm1 = Vector3.Cross(Vector3.up, tan1).normalized;
                var norm2 = Vector3.Cross(Vector3.up, tan2).normalized;

                var color1 = particleColors.HasValue
                                 ? particleColors[bond.A]
                                 : UnityEngine.Color.white;

                var color2 = particleColors.HasValue
                                 ? particleColors[bond.B]
                                 : UnityEngine.Color.white;

                splineSegment[l] = new SplineSegment
                {
                    StartPoint = pos1,
                    EndPoint = pos2,
                    StartTangent = tan1,
                    EndTangent = tan2,
                    StartNormal = norm1,
                    EndNormal = norm2,
                    StartColor = color1,
                    EndColor = color2,
                    StartScale = Vector3.one,
                    EndScale = Vector3.one
                };

                l++;
            }
            splineSegment.MarkValueAsChanged();
        }

        protected override void ClearOutput()
        {
            splineSegment.UndefineValue();
        }
    }
}