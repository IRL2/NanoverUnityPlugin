using System.Collections.Generic;
using Narupa.Core.Math;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Narupa.Core.Tests.Math
{
    public class UnitScaleTransformationTests
    {
        public static Quaternion GetRandomRotation()
        {
            return Random.rotationUniform;
        }

        public static Vector3 GetRandomPosition()
        {
            return Random.insideUnitSphere * 100f;
        }

        public static float GetRandomNonZeroScale()
        {
            return Random.value * 100f + 1e-6f * (Random.value > 0.5f ? 1f : -1f);
        }

        public static UnitScaleTransformation GetRandomTransformation()
        {
            return new UnitScaleTransformation(GetRandomPosition(),
                                               GetRandomRotation());
        }

        public static IEnumerable<UnitScaleTransformation> GetTransformations()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return GetRandomTransformation();
            }
        }

        public static IEnumerable<(UnitScaleTransformation, UnitScaleTransformation)>
            GetTransformationPairs()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomTransformation(), GetRandomTransformation());
            }
        }

        public static IEnumerable<(UnitScaleTransformation, Vector3)>
            GetTransformationAndVectors()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomTransformation(), GetRandomPosition());
            }
        }

        public static IEnumerable<(Vector3, Quaternion)> GetTranslationAndRotations()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomPosition(), GetRandomRotation());
            }
        }
        
        #region Constructors

        [Test]
        public void Constructor(
            [ValueSource(nameof(GetTranslationAndRotations))]
            (Vector3, Quaternion) input)
        {
            var (position, rotation) = input;
            var transformation = new UnitScaleTransformation(position, rotation);
            MathAssert.AreEqual(position, transformation.position);
            MathAssert.AreEqual(rotation, transformation.rotation);
        }

        #endregion


        #region Constants

        [Test]
        public void Identity()
        {
            Assert.AreEqual(Matrix4x4.identity, UnitScaleTransformation.identity.matrix);
        }

        #endregion


        #region Inverse

        [Test]
        public void Inverse(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            var matrix = transformation.matrix;
            MathAssert.AreEqual(matrix.inverse, transformation.inverse.matrix);
        }

        [Test]
        public void LeftInverseMultiplication(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            var identity = transformation.inverse * transformation;
            MathAssert.AreEqual(Matrix4x4.identity, identity.matrix);
        }

        [Test]
        public void RightInverseMultiplication(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            var identity = transformation * transformation.inverse;
            MathAssert.AreEqual(Matrix4x4.identity, identity.matrix);
        }

        #endregion


        #region Matrices

        [Test]
        public void PositionFromMatrix(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            var position = GetRandomPosition();
            transformation.position = position;
            MathAssert.AreEqual(position, transformation.matrix.GetTranslation());
        }

        [Test]
        public void RotationFromMatrix(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            var rotation = GetRandomRotation();
            transformation.rotation = rotation;
            MathAssert.AreEqual(rotation, transformation.matrix.GetRotation());
        }

        [Test]
        public void InverseMatrix(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            var matrix = transformation.matrix;
            MathAssert.AreEqual(matrix.inverse, transformation.inverseMatrix);
        }

        #endregion


        #region Conversions

        [Test]
        public void AsMatrix(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            MathAssert.AreEqual(transformation.matrix, (Matrix4x4) transformation);
        }

        [Test]
        public void AsTransformation(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            MathAssert.AreEqual(transformation.matrix, ((Transformation) transformation).Matrix);
        }

        [Test]
        public void AsUniformScaleTransformation(
            [ValueSource(nameof(GetTransformations))] UnitScaleTransformation transformation)
        {
            MathAssert.AreEqual(transformation.matrix,
                                ((UniformScaleTransformation) transformation).matrix);
        }

        #endregion


        #region Multiplication

        [Test]
        public void Multiplication(
            [ValueSource(nameof(GetTransformationPairs))]
            (UnitScaleTransformation, UnitScaleTransformation) input)
        {
            var (transformation1, transformation2) = input;
            var matrix1 = transformation1.matrix;
            var matrix2 = transformation2.matrix;
            MathAssert.AreEqual(matrix1 * matrix2, (transformation1 * transformation2).matrix);
        }

        #endregion


        #region Transformation of Points

        [Test]
        public void TransformPoint(
            [ValueSource(nameof(GetTransformationAndVectors))]
            (UnitScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.MultiplyPoint3x4(vector),
                                transformation.TransformPoint(vector));
        }

        [Test]
        public void InverseTransformPoint(
            [ValueSource(nameof(GetTransformationAndVectors))]
            (UnitScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.inverse.MultiplyPoint3x4(vector),
                                transformation.InverseTransformPoint(vector));
        }

        #endregion


        #region Transformation of Directions

        [Test]
        public void TransformDirection(
            [ValueSource(nameof(GetTransformationAndVectors))]
            (UnitScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.MultiplyVector(vector),
                                transformation.TransformDirection(vector));
        }

        [Test]
        public void InverseTransformDirection(
            [ValueSource(nameof(GetTransformationAndVectors))]
            (UnitScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.inverse.MultiplyVector(vector),
                                transformation.InverseTransformDirection(vector));
        }

        #endregion
    }
}