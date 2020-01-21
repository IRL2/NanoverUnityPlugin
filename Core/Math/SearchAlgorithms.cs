using System.Collections.Generic;
using UnityEngine;

namespace Narupa.Core.Math
{
    public class SearchAlgorithms
    {
        /// <summary>
        /// Binary search to find an index in a set of ordered indices.
        /// </summary>
        /// <param name="value">The value that is being searched for.</param>
        /// <param name="set">A set of integers ordered low to high.</param>
        public static bool BinarySearch(int value, IReadOnlyList<int> set)
        {
            var leftIndex = 0;
            var rightIndex = set.Count - 1;
            while (leftIndex <= rightIndex)
            {
                var midpointIndex = (leftIndex + rightIndex) / 2;
                var valueAtMidpoint = set[midpointIndex];
                if (valueAtMidpoint < value)
                    leftIndex = midpointIndex + 1;
                else if (valueAtMidpoint > value)
                    rightIndex = midpointIndex - 1;
                else
                    return true;
            }

            return false;
        }
        
        public static int BinarySearchIndex(int value, IReadOnlyList<int> set)
        {
            var leftIndex = 0;
            var rightIndex = set.Count - 1;
            while (leftIndex <= rightIndex)
            {
                var midpointIndex = (leftIndex + rightIndex) / 2;
                var valueAtMidpoint = set[midpointIndex];
                if (valueAtMidpoint < value)
                    leftIndex = midpointIndex + 1;
                else if (valueAtMidpoint > value)
                    rightIndex = midpointIndex - 1;
                else
                    return midpointIndex;
            }

            return -1;
        }
    }
}