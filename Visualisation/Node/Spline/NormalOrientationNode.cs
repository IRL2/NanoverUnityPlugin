using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Spline
{
    /// <summary>
    /// Rotates normals so within each sequence, the normals only rotate by up to 90 degrees.
    /// </summary>
    [Serializable]
    public class NormalOrientationNode : GenericOutputNode
    {
        /// <summary>
        /// The set of normals to rotate.
        /// </summary>
        [SerializeField]
        private Vector3ArrayProperty inputNormals = new Vector3ArrayProperty();

        /// <summary>
        /// The set of tangents, needed to calculate the binormals.
        /// </summary>
        [SerializeField]
        private Vector3ArrayProperty inputTangents = new Vector3ArrayProperty();

        /// <summary>
        /// A set of rotated normals that minimises the rotation of the normals.
        /// </summary>
        private Vector3ArrayProperty outputNormals = new Vector3ArrayProperty();

        /// <summary>
        /// Calculate rotated normals from an existing set of normals and tangents.
        /// </summary>
        public void RotateNormals(Vector3[] normals, Vector3[] tangents)
        {
            for (var j = 1; j < normals.Length; j++)
            {
                var a = normals[j] - Vector3.Project(normals[j], tangents[j]);
                var b = Vector3.Cross(tangents[j], normals[j]);
                a = a.normalized;
                b = b.normalized;
                var dot1 = Vector3.Dot(normals[j - 1], a);
                var dot2 = Vector3.Dot(normals[j - 1], b);

                if (dot1 * dot1 > dot2 * dot2)
                {
                    outputNormals[j] = a * Mathf.Sign(dot1);
                }
                else
                {
                    outputNormals[j] = b * Mathf.Sign(dot2);
                }
            }
        }

        /// <inheritdoc cref="GenericOutputNode.IsInputValid"/>
        protected override bool IsInputValid => inputNormals.HasNonNullValue()
                                             && inputTangents.HasNonNullValue();

        /// <inheritdoc cref="GenericOutputNode.IsInputDirty"/>
        protected override bool IsInputDirty => inputNormals.IsDirty
                                             || inputTangents.IsDirty;

        /// <inheritdoc cref="GenericOutputNode.ClearDirty"/>
        protected override void ClearDirty()
        {
            inputNormals.IsDirty = false;
            inputTangents.IsDirty = false;
        }

        /// <inheritdoc cref="GenericOutputNode.UpdateOutput"/>
        protected override void UpdateOutput()
        {
            outputNormals.Resize(inputNormals.Value.Length);
            Array.Copy(inputNormals, outputNormals, inputNormals.Value.Length);
            RotateNormals(outputNormals.Value, inputTangents.Value);
            outputNormals.MarkValueAsChanged();
        }

        /// <inheritdoc cref="GenericOutputNode.ClearOutput"/>
        protected override void ClearOutput()
        {
            outputNormals.UndefineValue();
        }
    }
}