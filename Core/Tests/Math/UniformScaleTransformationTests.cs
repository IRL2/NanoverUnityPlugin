using System.Collections.Generic;
using Narupa.Core.Math;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Narupa.Core.Tests.Math
{
    public class UniformScaleTransformationTests
    {
        public static Quaternion GetRandomRotation()
        {
            return Random.rotationUniform;
        }

        public static Vector3 GetRandomPosition()
        {
            return Random.insideUnitSphere * 100f;
        }

        public static float GetRandomPositiveScale()
        {
            return Random.value * 100f + 1e-6f;
        }


        public static Vector3 GetRandomScale()
        {
            return new Vector3(GetRandomNonZeroScale(),
                               GetRandomNonZeroScale(),
                               GetRandomNonZeroScale());
        }

        public static Transformation GetRandomTRS()
        {
            return new Transformation(GetRandomPosition(),
                                      GetRandomRotation(),
                                      GetRandomScale());
        }


        public static IEnumerable<(UniformScaleTransformation, Transformation)>
            GetTranslationAndTRSs()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomTransformation(), GetRandomTRS());
            }
        }


        public static float GetRandomNonZeroScale()
        {
            return Random.value * 100f + 1e-6f * (Random.value > 0.5f ? 1f : -1f);
        }

        public static UniformScaleTransformation GetRandomTransformation()
        {
            return new UniformScaleTransformation(GetRandomPosition(),
                                                  GetRandomRotation(),
                                                  GetRandomNonZeroScale());
        }

        public static IEnumerable<UniformScaleTransformation> GetTransformations()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return GetRandomTransformation();
            }
        }

        public static IEnumerable<(UniformScaleTransformation, UniformScaleTransformation)>
            GetTransformationPairs()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomTransformation(), GetRandomTransformation());
            }
        }

        public static IEnumerable<(UniformScaleTransformation, Vector3)>
            GetTransformationAndVectors()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomTransformation(), GetRandomPosition());
            }
        }

        public static IEnumerable<(Vector3, Quaternion, float)> GetTranslationRotationAndScales()
        {
            for (var i = 0; i < 128; i++)
            {
                yield return (GetRandomPosition(), GetRandomRotation(), GetRandomNonZeroScale());
            }
        }

        #region Constructors

        [Test]
        public void Constructor(
            [ValueSource(nameof(GetTranslationRotationAndScales))]
            (Vector3, Quaternion, float) input)
        {
            var (position, rotation, scale) = input;
            var transformation = new UniformScaleTransformation(position, rotation, scale);
            MathAssert.AreEqual(position, transformation.position);
            MathAssert.AreEqual(rotation, transformation.rotation);
            MathAssert.AreEqual(scale, transformation.scale);
        }

        #endregion


        #region Constants

        [Test]
        public void Identity()
        {
            Assert.AreEqual(Matrix4x4.identity, UniformScaleTransformation.identity.matrix);
        }

        #endregion


        #region Inverse

        [Test]
        public void Inverse(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var matrix = transformation.matrix;
            MathAssert.AreEqual(matrix.inverse, transformation.inverse.matrix);
        }

        [Test]
        public void LeftInverseMultiplication(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var identity = transformation.inverse * transformation;
            MathAssert.AreEqual(Matrix4x4.identity, identity.matrix);
        }

        [Test]
        public void RightInverseMultiplication(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var identity = transformation * transformation.inverse;
            MathAssert.AreEqual(Matrix4x4.identity, identity.matrix);
        }

        #endregion


        #region Matrices

        [Test]
        public void PositionFromMatrix(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var position = GetRandomPosition();
            transformation.position = position;
            MathAssert.AreEqual(position, transformation.matrix.GetTranslation());
        }

        [Test]
        public void RotationFromMatrix(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var rotation = GetRandomRotation();
            transformation.rotation = rotation;
            MathAssert.AreEqual(rotation, transformation.matrix.GetRotation());
        }

        [Test]
        public void ScaleFromMatrix(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var scale = GetRandomNonZeroScale();
            transformation.scale = scale;
            MathAssert.AreEqual(scale * Vector3.one, transformation.matrix.GetScale());
        }

        [Test]
        public void InverseMatrix(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            var matrix = transformation.matrix;
            MathAssert.AreEqual(matrix.inverse, transformation.inverseMatrix);
        }

        #endregion


        #region Conversions

        [Test]
        public void AsMatrix(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            MathAssert.AreEqual(transformation.matrix, (Matrix4x4) transformation);
        }

        [Test]
        public void AsTransformation(
            [ValueSource(nameof(GetTransformations))] UniformScaleTransformation transformation)
        {
            MathAssert.AreEqual(transformation.matrix, ((Transformation) transformation).Matrix);
        }

        #endregion


        #region Multiplication

        [Test]
        public void Multiplication(
            [ValueSource(nameof(GetTransformationPairs))]
            (UniformScaleTransformation, UniformScaleTransformation) input)
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
            (UniformScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.MultiplyPoint3x4(vector),
                                transformation.TransformPoint(vector));
        }

        [Test]
        public void InverseTransformPoint(
            [ValueSource(nameof(GetTransformationAndVectors))]
            (UniformScaleTransformation, Vector3) input)
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
            (UniformScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.MultiplyVector(vector),
                                transformation.TransformDirection(vector));
        }

        [Test]
        public void InverseTransformDirection(
            [ValueSource(nameof(GetTransformationAndVectors))]
            (UniformScaleTransformation, Vector3) input)
        {
            var (transformation, vector) = input;
            MathAssert.AreEqual(transformation.matrix.inverse.MultiplyVector(vector),
                                transformation.InverseTransformDirection(vector));
        }

        #endregion


        #region TransformPose

        [Test]
        public void TransformationTo(
            [ValueSource(nameof(GetTranslationAndTRSs))]
            (UniformScaleTransformation, Transformation) input)
        {
            var (transformation, trs) = input;
            var conversion = transformation.TransformationTo(trs);
            MathAssert.AreEqual(transformation.matrix.inverse * trs.Matrix, conversion.Matrix);
        }
        
        [Test]
        public void TransformBy(
            [ValueSource(nameof(GetTranslationAndTRSs))]
            (UniformScaleTransformation, Transformation) input)
        {
            var (transformation, trs) = input;
            var conversion = transformation.TransformationTo(trs);
            MathAssert.AreEqual(trs.Matrix, transformation.TransformBy(conversion).Matrix);
        }


        #endregion
    }
}