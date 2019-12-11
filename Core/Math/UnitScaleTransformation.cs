using UnityEngine;

namespace Narupa.Core.Math
{
    /// <summary>
    /// A transformation consisting of a rotation followed by a translation.
    /// </summary>
    /// <remarks>
    /// Also known as a Rigid Motion or a Proper Rigid Transformation. These
    /// transformations preserve orientation, distances and angles.
    /// </remarks>
    public struct UnitScaleTransformation
    {
        public Vector3 position;

        public Quaternion rotation;

        public UnitScaleTransformation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public Matrix4x4 matrix => Matrix4x4.TRS(position, rotation, Vector3.one);

        public UnitScaleTransformation inverse
        {
            get
            {
                var inverseRotation =
                    new Quaternion(rotation.x, -rotation.y, -rotation.z, -rotation.w);
                return new UnitScaleTransformation(inverseRotation * -position, inverseRotation);
            }
        }

        public Matrix4x4 inverseMatrix => inverse.matrix;

        public Vector3 TransformPoint(Vector3 pt)
        {
            return rotation * pt + position;
        }

        public Transformation TransformPose(Transformation pose)
        {
            return new Transformation(TransformPoint(pose.Position),
                                      rotation * pose.Rotation,
                                      pose.Scale);
        }

        public Vector3 TransformDirection(Vector3 pt)
        {
            return rotation * pt;
        }

        public static UnitScaleTransformation LookAt(Vector3 from, Vector3 to, Vector3 up)
        {
            return new UnitScaleTransformation(from, Quaternion.LookRotation(to - from, up));
        }

        public Transformation InverseTransformPose(Transformation pose)
        {
            return inverse.TransformPose(pose);
        }
    }
}