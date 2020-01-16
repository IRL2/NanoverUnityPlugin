// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Narupa.Utility
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Enumerate over all pairs of adjacent items in a list, such that the set [A, B,
        /// C, D] yields the pairs (A, B), (B, C) and (C, D).
        /// </summary>
        public static IEnumerable<(TElement First, TElement Second)> GetPairs<TElement>(
            this IEnumerable<TElement> enumerable)
        {
            var started = false;
            var prev = default(TElement);
            foreach (var e in enumerable)
            {
                if (!started)
                {
                    started = true;
                    prev = e;
                }
                else
                {
                    yield return (prev, e);
                    prev = e;
                }
            }
        }

        /// <summary>
        /// Find the index of an item in an enumerable, returning -1 if the item is not
        /// present.
        /// </summary>
        public static int IndexOf<TElement>(this IEnumerable<TElement> list, TElement item)
        {
            var i = 0;
            foreach (var thing in list)
            {
                if (thing.Equals(item))
                    return i;
                i++;
            }

            return -1;
        }
    }
}