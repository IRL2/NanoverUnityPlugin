// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using Narupa.Core.Math;
using UnityEngine;

namespace Narupa.Testing
{
    public static class SpatialTestData
    {
        /// <summary>
        /// Maximum displacement along any axis to use in positional test data. The
        /// larger the coordinate, the fewer decimal places of precision remain
        /// accurate.
        /// </summary>
        public const float MaximumCoordinate = 10000;

        /// <summary>
        /// Maximum shrinking/enlargement factor to use in scaling test data. The
        /// larger the factor, the fewer decimal places of precision remain accurate.
        /// </summary>
        public const float MaximumScaleFactor = 100;

        /// <summary>
        /// Get a random position within the supported range of coordinates.
        /// Coordinates outside this range are too imprecise to be worth supporting.
        /// </summary>
        public static Vector3 GetRandomPosition()
        {
            return new Vector3(Random.Range(-MaximumCoordinate, MaximumCoordinate),
                               Random.Range(-MaximumCoordinate, MaximumCoordinate),
                               Random.Range(-MaximumCoordinate, MaximumCoordinate));
        }

        /// <summary>
        /// Get a uniformly random rotation.
        /// </summary>
        public static Quaternion GetRandomRotation()
        {
            return Random.rotation;
        }

        /// <summary>
        /// Get a random positive scale within the supported range of scales. Scales
        /// outside this range are too imprecise to be worth supporting, and not
        /// practically useful anyway.
        /// </summary>
        public static Vector3 GetRandomPositiveScale()
        {
            return new Vector3(GetRandomPositiveScaleFactor(),
                               GetRandomPositiveScaleFactor(),
                               GetRandomPositiveScaleFactor());
        }

        public static Vector3 GetRandomPositiveUniformScale()
        {
            return Vector3.one * GetRandomPositiveScaleFactor();
        }

        /// <summary>
        /// Get a random scale factor, scaled evenly between above 1 and below 1
        /// </summary>
        public static float GetRandomPositiveScaleFactor()
        {
            var range = Mathf.Log(MaximumScaleFactor, 2);

            return Mathf.Pow(2, Random.Range(-range, range));
        }

        public static Transformation GetRandomTransformation()
        {
            var components = new Transformation
            {
                Position = GetRandomPosition(),
                Rotation = GetRandomRotation(),
                Scale = GetRandomPositiveScale(),
            };

            return components;
        }

        public static Transformation GetRandomTransformationUniformScale()
        {
            var transformation = new Transformation
            {
                Position = GetRandomPosition(),
                Rotation = GetRandomRotation(),
                Scale = GetRandomPositiveUniformScale(),
            };

            return transformation;
        }

        public static IEnumerable<Transformation> RandomTransformation
            => RandomTestData.SeededRandom(GetRandomTransformation);
    }
}