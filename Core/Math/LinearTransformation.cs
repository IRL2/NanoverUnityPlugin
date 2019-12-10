using System;
using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// A linear transformation, defined by where it maps the three axes of cartesian space.
    /// </summary>
    [Serializable]
    public struct LinearTransformation
    {
        public Vector3 xAxis;
        public Vector3 yAxis;
        public Vector3 zAxis;

        public LinearTransformation(Vector3 xAxis,
                                    Vector3 yAxis,
                                    Vector3 zAxis)

        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.zAxis = zAxis;
        }

        public LinearTransformation(float xx,
                                    float xy,
                                    float xz,
                                    float yx,
                                    float yy,
                                    float yz,
                                    float zx,
                                    float zy,
                                    float zz)
        {
            xAxis = new Vector3(xx, xy, xz);
            yAxis = new Vector3(yx, yy, yz);
            zAxis = new Vector3(zx, zy, zz);
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Vector3 Dot(Vector3 factors, LinearTransformation transformation)
        {
            return factors.x * transformation.xAxis
                 + factors.y * transformation.yAxis
                 + factors.z * transformation.zAxis;
        }


        public static implicit operator AffineTransformation(LinearTransformation transformation)
        {
            return new AffineTransformation(transformation.xAxis,
                                            transformation.yAxis,
                                            transformation.zAxis,
                                            Vector3.zero);
        }
    }
}