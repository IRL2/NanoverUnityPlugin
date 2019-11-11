// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// Bundles position, rotation, and scale of a transformation.
    /// </summary>
    public struct Transformation
    {
        /// <summary>
        /// Construct a transformation from the translation, rotation, and
        /// scale of a TRS matrix.
        /// </summary>
        public static Transformation FromMatrix(Matrix4x4 matrix)
        {
            return new Transformation(matrix.GetTranslation(),
                                      matrix.GetRotation(),
                                      matrix.GetScale());
        }

        /// <summary>
        /// Construct a transformation from the translation, rotation, and
        /// scale of a Unity Transform.
        /// </summary>
        public static Transformation FromTransform(Transform transform)
        {
            return new Transformation(transform.position,
                                      transform.rotation,
                                      transform.lossyScale);
        }

        /// <summary>
        /// The identity transformation.
        /// </summary>
        public static Transformation Identity =>
            new Transformation(Vector3.zero, Quaternion.identity, Vector3.one);

        /// <summary>
        /// Position of this transformation.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Rotation of this transformation.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Scale of this transformation.
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// <see cref="Matrix4x4" /> representation of this transformation.
        /// </summary>
        public Matrix4x4 Matrix => Matrix4x4.TRS(Position, Rotation, Scale);

        public Transformation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        /// <summary>
        /// Set the transform's position, rotation, scale from this transformation.
        /// </summary>
        public void CopyToTransform(Transform transform)
        {
            Matrix.CopyTrsToTransform(transform);
        }
    }
}