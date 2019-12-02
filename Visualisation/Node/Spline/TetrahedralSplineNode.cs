﻿using System;
using System.Linq;
using Narupa.Frame;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Spline
{
    [Serializable]
    public class TetrahedralSplineNode : GenericOutputNode
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
        private BondArrayProperty interiorBonds = new BondArrayProperty();

        [SerializeField]
        private ColorProperty color;

        [SerializeField]
        private FloatProperty radius;

        private Vector3ArrayProperty outputPositions = new Vector3ArrayProperty();

        private BondArrayProperty outputBonds = new BondArrayProperty();

        private SelectionArrayProperty outputFaces = new SelectionArrayProperty();

        private ColorArrayProperty outputColors = new ColorArrayProperty();

        private BondArrayProperty outputInteriorBonds = new BondArrayProperty();

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
                                             || interiorBonds.IsDirty
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
            interiorBonds.IsDirty = false;
            scales.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            if (sequences.IsDirty || !outputPositions.HasValue)
            {
                var pointCount = sequences.Value.Sum(i => i.Count);
                var segmentCount = sequences.Value.Sum(i => i.Count - 1);
                var bondcount = pointCount + 4 * segmentCount;

                outputPositions.Resize(pointCount * 2);
                outputBonds.Resize(bondcount);
                outputColors.Resize(pointCount * 2);
                outputFaces.Resize(segmentCount * 4);


                // Generate bonds between each pair of points representing a point of the spline.
                var bondIndex = 0;
                foreach (var sequence in sequences)
                {
                    var sequenceLength = sequence.Count;
                    for (var i = 0; i < sequenceLength; i++)
                    {
                        outputBonds[bondIndex] = new BondPair(bondIndex * 2, bondIndex * 2 + 1);
                        bondIndex++;
                    }
                }

                var pointIndex = 0;
                var faceIndex = 0;
                foreach (var sequence in sequences)
                {
                    var sequenceLength = sequence.Count;
                    for (var segment = 0; segment < sequenceLength - 1; segment++)
                    {
                        outputBonds[bondIndex] = new BondPair(pointIndex, pointIndex + 2);
                        outputBonds[bondIndex + 1] = new BondPair(pointIndex, pointIndex + 3);
                        outputBonds[bondIndex + 2] = new BondPair(pointIndex + 1, pointIndex + 2);
                        outputBonds[bondIndex + 3] = new BondPair(pointIndex + 1, pointIndex + 3);

                        outputFaces[faceIndex] = new[]
                            { pointIndex, pointIndex + 1, pointIndex + 2 };
                        outputFaces[faceIndex + 1] = new[]
                            { pointIndex, pointIndex + 1, pointIndex + 3 };
                        outputFaces[faceIndex + 2] = new[]
                            { pointIndex, pointIndex + 2, pointIndex + 3 };
                        outputFaces[faceIndex + 3] = new[]
                            { pointIndex + 1, pointIndex + 2, pointIndex + 3 };

                        bondIndex += 4;
                        faceIndex += 4;
                        pointIndex += 2;
                    }

                    pointIndex += 2;
                }

                outputBonds.MarkValueAsChanged();
                outputFaces.MarkValueAsChanged();

                interiorBonds.IsDirty = true;
            }

            if (interiorBonds.IsDirty)
            {
                if (interiorBonds.HasNonNullValue())
                {
                    outputInteriorBonds.Resize(interiorBonds.Value.Length);

                    for (var i = 0; i < interiorBonds.Value.Length; i++)
                    {
                        outputInteriorBonds[i] = new BondPair(interiorBonds[i].A * 2,
                                                              interiorBonds[i].B * 2);
                    }

                    outputInteriorBonds.MarkValueAsChanged();
                }
                else
                {
                    outputInteriorBonds.UndefineValue();
                }
            }

            var index = 0;
            foreach (var sequence in sequences)
            {
                var sequenceLength = sequence.Count;
                for (var vertex = 0; vertex < sequenceLength; vertex++)
                {
                    var position = positions[sequence[vertex]];
                    var tangent = tangents[index];
                    var normal = normals[index];
                    var binormal = Vector3.Cross(tangent, normal);
                    var color = this.color.Value * (colors.HasValue
                                                        ? colors.Value[index]
                                                        : UnityEngine.Color.white);
                    var radius = this.radius.Value * (scales.HasValue
                                                          ? scales.Value[index]
                                                          : 1f);

                    outputPositions[index * 2] = position + radius * binormal;
                    outputPositions[index * 2 + 1] = position - radius * binormal;
                    outputColors[index * 2] = color;
                    outputColors[index * 2 + 1] = color;
                    index++;
                }
            }

            outputPositions.MarkValueAsChanged();
            outputColors.MarkValueAsChanged();
        }

        protected override void ClearOutput()
        {
            outputBonds.UndefineValue();
            outputColors.UndefineValue();
            outputFaces.UndefineValue();
            outputPositions.UndefineValue();
            outputInteriorBonds.UndefineValue();
        }
    }
}