using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    [Serializable]
    public class SecondaryStructureSpline : Spline
    {
        public float ArrowScale = 8;

        public float HelixScale = 4;

        [SerializeField]
        private SecondaryStructureArrayProperty secondaryStructure;

        public float SheetScale = 4;

        protected override void ModifyGeometry(HermiteCurve curve)
        {
            base.ModifyGeometry(curve);

            return;

            // Smooths out beta sheets

            var p0 = curve.Positions[0];
            var p1 = curve.Positions[1];

            var pointSequence = curve.Sequence;

            for (var j = 1; j < curve.Positions.Length - 1; j++)
            {
                var p2 = curve.Positions[j + 1];
                var residueIndex = pointSequence[j];
                if (secondaryStructure.Value[residueIndex]
                                      .HasFlag(SecondaryStructureAssignment.Sheet))
                    curve.Positions[j] = 0.5f * p1 + 0.25f * p0 + 0.25f * p2;

                p0 = p1;
                p1 = p2;
            }
        }

        protected override void GenerateSegments()
        {
            base.GenerateSegments();
            secondaryStructure.IsDirty = false;
        }

        protected override bool DoSegmentsNeedGenerating
            => base.DoSegmentsNeedGenerating || secondaryStructure.IsDirty;

        protected override bool CanSegmentsBeGenerated
            => base.CanSegmentsBeGenerated &&
               secondaryStructure.HasNonEmptyValue();
/*

        protected override void PreUpdateCurves()
        {
            base.PreUpdateCurves();
            sequenceArray = pointSequences.Value;
            secondaryStructureArray = residueSecondaryStructure.Value;
            colorArray = atomColors.Value;
        }
        
        */

        protected override void OnUpdateSegment(ref SplineSegment segment,
                                                int k,
                                                HermiteCurve curve)
        {
            var sequence = curve.Sequence;

            var res1 = sequence[k];
            var res2 = sequence[k + 1];

            var ss1 = secondaryStructure.Value[res1];
            var ss2 = secondaryStructure.Value[res2];

            var ss3 = k < sequence.Count - 2
                          ? secondaryStructure.Value[sequence[k + 2]]
                          : SecondaryStructureAssignment.None;

            var helix1 = (ss1 & SecondaryStructureAssignment.Helix) > 0;
            var helix2 = (ss2 & SecondaryStructureAssignment.Helix) > 0;

            var w1 = 1f;
            var w2 = 1f;

            if (helix1)
                w1 = HelixScale;
            if (helix2)
                w2 = HelixScale;

            var sheet1 = (ss1 & SecondaryStructureAssignment.Sheet) > 0;
            var sheet2 = (ss2 & SecondaryStructureAssignment.Sheet) > 0;
            var sheet = sheet1 && sheet2;
            var arrow = sheet && (ss3 & SecondaryStructureAssignment.Sheet) == 0;

            if (sheet1)
                w1 = SheetScale;
            if (sheet2)
                w2 = SheetScale;

            if (sheet)
            {
                //w1 = SheetScale;
                //w2 = SheetScale;
                if (arrow) // end of sheet
                {
                    //w1 = ArrowScale;
                    //w2 = 1f;
                }
            }

            var startColor = segment.StartColor;
            var endColor = segment.EndColor;

            // Ensures consistent yellow arrows
            //if (sheet1 && !sheet2) startColor = endColor;

            //if (sheet2 && !sheet1) endColor = startColor;

            var startTangent = curve.Tangents[k];
            var endTangent = curve.Tangents[k + 1];

            if (arrow)
            {
                //endTangent *= 0.1f;
                //startTangent *= 0.5f;
            }

            segment.StartColor = startColor;
            segment.EndColor = endColor;
            segment.StartTangent = startTangent;
            segment.EndTangent = endTangent;
            segment.StartScale = new Vector2(w1, w1);
            segment.EndScale = new Vector2(w2, w2);
        }

        private enum SegmentType
        {
            Arrow,
            Sheet
        }
    }
}