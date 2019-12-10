using System;
using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// An affine transformation, defined by how it transforms the three axes of cartesian space and offsets the origin.
    /// </summary>
    [Serializable]
    public struct AffineTransformation
    {
        public Vector3 xAxis;
        public Vector3 yAxis;
        public Vector3 zAxis;
        public Vector3 origin;

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

        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return xAxis;
                    case 1:
                        return yAxis;
                    case 2:
                        return zAxis;
                    case 3:
                        return origin;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        xAxis = value;
                        break;
                    case 1:
                        yAxis = value;
                        break;
                    case 2:
                        zAxis = value;
                        break;
                    case 3:
                        origin = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public LinearTransformation LinearTransformation =>
            new LinearTransformation(xAxis, yAxis, zAxis);

        /// <summary>
        /// The magnitudes of the three axes that define this linear transformation.
        /// </summary>
        public Vector3 AxesMagnitudes => new Vector3(xAxis.magnitude,
                                                     yAxis.magnitude,
                                                     zAxis.magnitude);

        public Matrix4x4 AsMatrix()
        {
            return new Matrix4x4(xAxis, yAxis, zAxis, new Vector4(origin.x, origin.y, origin.z, 1));
        }
    }
}