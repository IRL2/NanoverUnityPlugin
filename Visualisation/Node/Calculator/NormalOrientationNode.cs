using System;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Rotates normals by 90 degrees so there are no sharp turns.
    /// </summary>
    [Serializable]
    public class NormalOrientationNode : GenericOutputNode
    {
        [SerializeField]
        private Vector3ArrayProperty inputNormals = new Vector3ArrayProperty();

        [SerializeField]
        private Vector3ArrayProperty inputTangents = new Vector3ArrayProperty();

        private Vector3ArrayProperty outputNormals = new Vector3ArrayProperty();

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

        protected override bool IsInputValid => inputNormals.HasNonNullValue()
                                             && inputTangents.HasNonNullValue();

        protected override bool IsInputDirty => inputNormals.IsDirty
                                             || inputTangents.IsDirty;

        protected override void ClearDirty()
        {
            inputNormals.IsDirty = false;
            inputTangents.IsDirty = false;
        }

        protected override void UpdateOutput()
        {
            outputNormals.Resize(inputNormals.Value.Length);
            Array.Copy(inputNormals, outputNormals, inputNormals.Value.Length);
            RotateNormals(outputNormals.Value, inputTangents.Value);
            outputNormals.MarkValueAsChanged();
        }

        protected override void ClearOutput()
        {
            outputNormals.UndefineValue();
        }
    }
}