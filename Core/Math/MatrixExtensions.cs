// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// Extension methods for Unity's <see cref="Matrix4x4" />
    /// </summary>
    public static class MatrixExtensions
    {
        /// <summary>
        /// Extract the translation component of the given TRS matrix. This is
        /// the worldspace origin of matrix's coordinate space.
        /// </summary>
        public static Vector3 GetTranslation(this Matrix4x4 matrix)
        {
            return matrix.GetColumn(3);
        }

        /// <summary>
        /// Extract the rotation component of the given TRS matrix. This is
        /// the quaternion that rotates worldspace forward, up, right vectors
        /// into the matrix's coordinate space.
        /// </summary>
        public static Quaternion GetRotation(this Matrix4x4 matrix)
        {
            return matrix.rotation;
        }

        /// <summary>
        /// Extract the scale component of the given TRS matrix, assuming it is orthogonal.
        /// </summary>
        public static Vector3 GetScale(this Matrix4x4 matrix)
        {
            return matrix.lossyScale;
        }

        /// <summary>
        /// Get the matrix that transforms from this matrix to another.
        /// </summary>
        /// <remarks>
        /// In Unity transformations are from local-space to world-space, so
        /// the transformation is multiplied on the right-hand side.
        /// </remarks>
        public static Matrix4x4 GetTransformationTo(this Matrix4x4 from, Matrix4x4 to)
        {
            return from.inverse * to;
        }

        /// <summary>
        /// Return this matrix transformed by the given transformation matrix.
        /// </summary>
        /// <remarks>
        /// In Unity transformations are from local-space to world-space, so
        /// the transformation is multiplied on the right-hand side.
        /// </remarks>
        public static Matrix4x4 TransformedBy(this Matrix4x4 matrix, Matrix4x4 transformation)
        {
            return matrix * transformation;
        }

        /// <summary>
        /// Set the transform's position, rotation and scale relative to
        /// the world space from this TRS matrix.
        /// Invalid for compositions of rotation and non-uniform scales.
        /// </summary>
        public static void CopyTrsToTransformRelativeToWorld(this Matrix4x4 trs, Transform transform)
        {
            // we are not allowed to set global scale directly in Unity, so
            // instead we unparent the object, make local changes, then reparent
            var parent = transform.parent;

            transform.parent = null;

            transform.localPosition = trs.GetTranslation();
            transform.localRotation = trs.GetRotation();
            transform.localScale = trs.GetScale();

            transform.parent = parent;
        }
        
        /// <summary>
        /// Set the transform's position, rotation and scale relative to
        /// its parent from this TRS matrix.
        /// </summary>
        public static void CopyTrsToTransformRelativeToParent(this Matrix4x4 trs, Transform transform)
        {
            transform.localPosition = trs.GetTranslation();
            transform.localRotation = trs.GetRotation();
            transform.localScale = trs.GetScale();
        }
    }
}