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
        #region Fields

        /// <summary>
        /// The translation this transformation applies. When considered as a
        /// transformation from an object's local space to world space, describes the
        /// position of the object.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// The rotation this transformation applies. When considered as a transformation
        /// from an object's local space to world space, describes the rotation of the
        /// object.
        /// </summary>
        public Quaternion rotation;

        #endregion


        #region Constructors

        /// <summary>
        /// Create a transformation from its two actions.
        /// </summary>
        /// <param name="position">The translation this transformation applies.</param>
        /// <param name="rotation">The rotation this transformation applies.</param>
        public UnitScaleTransformation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        #endregion


        #region Constants

        /// <summary>
        /// The identity transformation.
        /// </summary>
        public static UnitScaleTransformation identity =>
            new UnitScaleTransformation(Vector3.zero, Quaternion.identity);

        #endregion


        #region Inverse

        /// <summary>
        /// The inverse of this transformation, which undoes this transformation.
        /// </summary>
        public UnitScaleTransformation inverse
        {
            get
            {
                var inverseRotation = Quaternion.Inverse(rotation);
                return new UnitScaleTransformation(
                    inverseRotation * -position,
                    inverseRotation);
            }
        }

        #endregion


        #region Matrices

        /// <summary>
        /// The 4x4 augmented matrix representing this transformation as it acts upon
        /// vectors and directions in homogeneous coordinates.
        /// </summary>
        public Matrix4x4 matrix => Matrix4x4.TRS(position, rotation, Vector3.one);

        /// <summary>
        /// The 4x4 augmented matrix representing the inverse of this transformation as it
        /// acts upon vectors and directions in homogeneous coordinates.
        /// </summary>
        public Matrix4x4 inverseMatrix => inverse.matrix;

        #endregion


        #region Conversions

        public static implicit operator Matrix4x4(UnitScaleTransformation transformation)
        {
            return transformation.matrix;
        }

        public static implicit operator Transformation(UnitScaleTransformation transformation)
        {
            return new Transformation(transformation.position,
                                      transformation.rotation,
                                      Vector3.one);
        }

        public static implicit operator UniformScaleTransformation(
            UnitScaleTransformation transformation)
        {
            return new UniformScaleTransformation(transformation.position,
                                                  transformation.rotation,
                                                  1);
        }

        #endregion


        #region Multiplication

        public static UnitScaleTransformation operator *(UnitScaleTransformation a,
                                                         UnitScaleTransformation b)
        {
            return new UnitScaleTransformation(a.position + (a.rotation * b.position),
                                               a.rotation * b.rotation);
        }

        public static Transformation operator *(UnitScaleTransformation a,
                                                         Transformation b)
        {
            return new Transformation(a.position + (a.rotation * b.Position),
                                               a.rotation * b.Rotation,
                                               b.Scale);
        }

        #endregion


        #region Transformation of Points

        /// <summary>
        /// Transform a point in space using this transformation. When considered as a
        /// transformation from an object's local space to world space, this takes points
        /// in the object's local space to world space.
        /// </summary>
        public Vector3 TransformPoint(Vector3 pt)
        {
            return rotation * pt + position;
        }

        /// <summary>
        /// Transform a point in space using the inverse of this transformation. When
        /// considered as a transformation from an object's local space to world space,
        /// this takes points in world space to the object's local space.
        /// </summary>
        public Vector3 InverseTransformPoint(Vector3 pt)
        {
            return inverse.TransformPoint(pt);
        }

        #endregion


        #region Transformation of Directions

        /// <summary>
        /// Transform a direction in space using this transformation. When considered as a
        /// transformation from an object's local space to world space, this takes
        /// directions in world space to the object's local space.
        /// </summary>
        public Vector3 TransformDirection(Vector3 direction)
        {
            return rotation * direction;
        }

        /// <summary>
        /// Transform a direction in space using the inverse of this transformation. When
        /// considered as a transformation from an object's local space to world space,
        /// this takes directions in world space to the object's local space.
        /// </summary>
        public Vector3 InverseTransformDirection(Vector3 pt)
        {
            return inverse.TransformDirection(pt);
        }
        
        public override string ToString()
        {
            var pos = position;
            var rot = rotation.eulerAngles;
            return $"UnitTransformation(Position: ({pos.x}, {pos.y}, {pos.z}), Rotation: ({rot.x}, {rot.y}, {rot.z}))";
        }

        #endregion
    }
}