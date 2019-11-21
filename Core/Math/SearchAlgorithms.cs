using System.Collections.Generic;
using UnityEngine;

namespace Narupa.Core.Math
{
    public class SearchAlgorithms
    {
        public static bool BinarySearch(int value, IReadOnlyList<int> set)
        {
            var L = 0;
            var R = set.Count - 1;
            while (L <= R)
            {
                var m = (L + R) / 2;
                var v = set[m];
                if (v < value)
                    L = m + 1;
                else if (v > value)
                    R = m - 1;
                else
                    return true;
            }

            return false;
        }
    }
}