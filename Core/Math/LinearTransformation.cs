using System;
using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// A linear transformation between two 3D spaces, defined by where it maps the
    /// three axes of cartesian space. This can represent any combination of rotations,
    /// reflections, scaling and shears.
    /// </summary>
    /// <remarks>
    /// Every linear transformation can be represented as a 3x3 matrix (and vice
    /// versa), with the three axes representing the three columns of the matrix.
    /// </remarks>
    [Serializable]
    public struct LinearTransformation
    {
        /// <summary>
        /// The vector to which this transformation maps (1, 0, 0).
        /// </summary>
        public Vector3 xAxis;

        /// <summary>
        /// The vector to which this transformation maps (0, 1, 0).
        /// </summary>
        public Vector3 yAxis;

        /// <summary>
        /// The vector to which this transformation maps (0, 0, 1).
        /// </summary>
        public Vector3 zAxis;

        /// <summary>
        /// The identity linear transformation.
        /// </summary>
        public static LinearTransformation identity => new LinearTransformation(Vector3.right,
                                                                                Vector3.up,
                                                                                Vector3.forward);

        /// <summary>
        /// Create a linear transformation which maps the x, y and z axes to new vectors.
        /// </summary>
        public LinearTransformation(Vector3 xAxis,
                                    Vector3 yAxis,
                                    Vector3 zAxis)

        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.zAxis = zAxis;
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
        /// Convert the transformation to an <see cref="AffineTransformation" /> with no
        /// translation.
        /// </summary>
        public static implicit operator AffineTransformation(LinearTransformation transformation)
        {
            return new AffineTransformation(transformation.xAxis,
                                            transformation.yAxis,
                                            transformation.zAxis,
                                            Vector3.zero);
        }
    }
}