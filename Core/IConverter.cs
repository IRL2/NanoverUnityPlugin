using System;
using System.Collections.Generic;

namespace Narupa.Core
{
    /// <summary>
    /// Defines an interface which provides a mapping between two types. It is
    /// essentially a <see cref="Converter{TInput,TOutput}" /> as an interface.
    /// </summary>
    public interface IConverter<in TFrom, out TTo>
    {
        /// <summary>
        /// Map an input value to an output value.
        /// </summary>
        TTo Convert(TFrom from);
    }

    internal class DictionaryAsConverter<TFrom, TTo> : IConverter<TFrom, TTo>
    {
        private IReadOnlyDictionary<TFrom, TTo> dictionary;

        private TTo defaultValue;

        internal DictionaryAsConverter(IReadOnlyDictionary<TFrom, TTo> dict,
                                     TTo defaultValue = default)
        {
            dictionary = dict;
            this.defaultValue = defaultValue;
        }

        /// <inheritdoc cref="IConverter{TFrom,TTo}.Convert"/>
        public TTo Convert(TFrom from)
        {
            return dictionary.TryGetValue(from, out var value) ? value : defaultValue;
        }
    }

    /// <summary>
    /// Extensions methods for <see cref="IConverter{TFrom,TTo}"/>.
    /// </summary>
    public static class ConverterExtensions
    {
        /// <summary>
        /// Convert a dictionary into an <see cref="IConverter{TFrom,TTo}"/>.
        /// </summary>
        public static IConverter<TFrom, TTo> AsMapping<TFrom, TTo>(
            this IReadOnlyDictionary<TFrom, TTo> dict,
            TTo defaultValue = default)
        {
            return new DictionaryAsConverter<TFrom, TTo>(dict, defaultValue);
        }
    }
}