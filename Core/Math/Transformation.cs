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
        /// <remarks>
        /// This is only works if the matrix involves uniform scaling. 
        /// </remarks>
        public static Transformation FromMatrix(Matrix4x4 matrix)
        {
            return new Transformation(matrix.GetTranslation(),
                                      matrix.GetRotation(),
                                      matrix.GetScale());
        }

        /// <summary>
        /// Construct a transformation from the translation, rotation, and
        /// scale of a Unity <see cref="Transform"/> relative to world space.
        /// </summary>
        /// <remarks>
        /// The scale is inherently lossy, as the composition of multiple
        /// transforms is not necessarily a transform. 
        /// </remarks>
        public static Transformation FromTransformRelativeToWorld(Transform transform)
        {
            return new Transformation(transform.position,
                                      transform.rotation,
                                      transform.lossyScale);
        }
        
        /// <summary>
        /// Construct a transformation from the translation, rotation, and
        /// scale of a Unity <see cref="Transform"/> relative to world space.
        /// </summary>
        public static Transformation FromTransformRelativeToParent(Transform transform)
        {
            return new Transformation(transform.localPosition,
                                      transform.localRotation,
                                      transform.localScale);
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
        /// Set the transform's position, rotation and scale relative to its parent from this transformation.
        /// </summary>
        public void CopyToTransformRelativeToParent(Transform transform)
        {
            transform.localPosition = Position;
            transform.localRotation = Rotation;
            transform.localScale = Scale;
        }
        
        /// <summary>
        /// Set the transform's position, rotation and scale relative to the world space from this transformation.
        /// </summary>
        public void CopyToTransformRelativeToWorld(Transform transform)
        {
            // we are not allowed to set global scale directly in Unity, so
            // instead we unparent the object, make local changes, then reparent
            var parent = transform.parent;

            transform.parent = null;

            transform.localPosition = Position;
            transform.localRotation = Rotation;
            transform.localScale = Scale;

            transform.parent = parent;
        }

        public override string ToString()
        {
            var pos = Position;
            var rot = Rotation.eulerAngles;
            var scale = Scale;
            return $"Transformation(Position: ({pos.x}, {pos.y}, {pos.z}), Rotation: ({rot.x}, {rot.y}, {rot.z}), Scale: ({scale.x}, {scale.y}, {scale.z}))";
        }

        /// <summary>
        /// Convert to a transformation with unit scale, discarding any scaling associated with this transformation.
        /// </summary>
        public UnitScaleTransformation AsUnitTransformWithoutScale()
        {
            return new UnitScaleTransformation(Position, Rotation);
        }
    }
}