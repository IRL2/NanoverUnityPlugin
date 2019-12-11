using Narupa.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Narupa.Core.Tests.Math
{
    public class MathAssert
    {
        public static void AreEqual(Matrix4x4 expected, Matrix4x4 actual)
        {
            Assert.That(actual,
                        Is.EqualTo(expected)
                          .Using(Matrix4x4RowEqualityComparer.Instance));
        }

        public static void AreEqual(Quaternion expected, Quaternion actual)
        {
            Assert.That(actual,
                        Is.EqualTo(expected)
                          .Using(QuaternionEqualityComparer.Instance));
        }
        
        public static void AreEqual(Vector3 expected, Vector3 actual)
        {
            Assert.That(actual,
                        Is.EqualTo(expected)
                          .Using(Vector3EqualityComparer.Instance));
        }
        
        public static void AreEqual(float expected, float actual)
        {
            Assert.That(actual,
                        Is.EqualTo(expected)
                          .Using(FloatEqualityComparer.Instance));
        }
    }
}