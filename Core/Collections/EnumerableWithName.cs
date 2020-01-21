using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Narupa.Core.Collections
{
    /// <summary>
    /// Wrapper around an <see cref="IEnumerable" /> that overrides the
    /// <see cref="ToString()" /> method to provide a list of its contents, optionally
    /// with a custom function to generate the names of each element.
    /// </summary>
    public class EnumerableWithName<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> enumerable;
        private readonly Func<T, string> namer;

        public EnumerableWithName(IEnumerable<T> enumerable) : this(
            enumerable, t => t.ToString())
        {
        }

        public EnumerableWithName(IEnumerable<T> enumerable, Func<T, string> namer)
        {
            this.enumerable = enumerable;
            this.namer = namer;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(", ", enumerable.Select(namer));
        }
    }
}