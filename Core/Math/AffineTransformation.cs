using System;
using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// An affine transformation between two 3D spaces, defined by where it maps the
    /// three axes of cartesian space and an offset. This can represent any combination
    /// of rotations,
    /// reflections, translations, scaling and shears.
    /// </summary>
    /// <remarks>
    /// Every affine transformation can be represented as an augmented 4x4 matrix, with
    /// the three axes (with the 4th component 0) and the origin (with the 4th
    /// component 1) representing the three columns of the matrix.
    /// </remarks>
    [Serializable]
    public struct AffineTransformation
    {
        /// <summary>
        /// The vector to which this transformation maps the direction (1, 0, 0).
        /// </summary>
        public Vector3 xAxis;

        /// <summary>
        /// The vector to which this transformation maps the direction (0, 1, 0).
        /// </summary>
        public Vector3 yAxis;

        /// <summary>
        /// The vector to which this transformation maps the direction (0, 0, 1).
        /// </summary>
        public Vector3 zAxis;

        /// <summary>
        /// The translation that this transformation applies.
        /// </summary>
        public Vector3 origin;

        /// <summary>
        /// The identity affine transformation.
        /// </summary>
        public static AffineTransformation identity => new AffineTransformation(Vector3.right,
                                                                                Vector3.up,
                                                                                Vector3.forward,
                                                                                Vector3.zero);

        /// <summary>
        /// Create an affine transformation which maps the x, y and z directions to new
        /// vectors, and translates to a new origin.
        /// </summary>
        public AffineTransformation(Vector3 xAxis,
                                    Vector3 yAxis,
                                    Vector3 zAxis,
                                    Vector3 origin)

        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.zAxis = zAxis;
            this.origin = origin;
        }

        /// <summary>
        /// The magnitudes of the three axes that define this linear transformation.
        /// </summary>
        public Vector3 axesMagnitudes => new Vector3(xAxis.magnitude,
                                                     yAxis.magnitude,
                                                     zAxis.magnitude);

        /// <summary>
        /// Get this transformation as a <see cref="Matrix4x4" />.
        /// </summary>
        public Matrix4x4 AsMatrix()
        {
            return new Matrix4x4(xAxis, yAxis, zAxis, new Vector4(origin.x, origin.y, origin.z, 1));
        }

        /// <summary>
        /// Transform a direction using this transformation.
        /// </summary>
        public Vector3 TransformDirection(Vector3 vec)
        {
            return vec.x * xAxis
                 + vec.y * yAxis
                 + vec.z * zAxis;
        }

        /// <summary>
        /// Transform a point using this transformation.
        /// </summary>
        public Vector3 TransformPoint(Vector3 vec)
        {
            return vec.x * xAxis
                 + vec.y * yAxis
                 + vec.z * zAxis
                 + origin;
        }
    }
}