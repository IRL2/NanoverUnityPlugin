using System.Collections.Generic;

namespace Narupa.Core
{
    /// <summary>
    /// Defines an interface which provides a mapping between two types.
    /// </summary>
    public interface IMapping<in TFrom, out TTo>
    {
        TTo Map(TFrom from);
    }

    internal class DictionaryAsMapping<TFrom, TTo> : IMapping<TFrom, TTo>
    {
        private IReadOnlyDictionary<TFrom, TTo> dictionary;

        private TTo defaultValue;

        internal DictionaryAsMapping(IReadOnlyDictionary<TFrom, TTo> dict,
                                     TTo defaultValue = default)
        {
            this.dictionary = dict;
            this.defaultValue = defaultValue;
        }

        public TTo Map(TFrom from)
        {
            return dictionary.TryGetValue(from, out var value) ? value : defaultValue;
        }
    }

    public static class MappingExtensions
    {
        public static IMapping<TFrom, TTo> AsMapping<TFrom, TTo>(
            this IReadOnlyDictionary<TFrom, TTo> dict,
            TTo defaultValue = default)
        {
            return new DictionaryAsMapping<TFrom, TTo>(dict, defaultValue);
        }
    }
}