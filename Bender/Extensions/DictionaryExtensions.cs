using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bender.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Finds a value in the dictionary by the given key and concatenates it with the specified string values separating them with the specified separator.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="dictionary">Should implements <see>
        ///         <cref>IDictionary</cref>
        ///     </see>
        /// </param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <param name="equalityComparer"></param>
        public static IDictionary<TKey, string> Extend<TKey>(
            this IDictionary<TKey, string> dictionary,
            TKey key,
            char separator,
            IEnumerable<string> values,
            IEqualityComparer<string>? equalityComparer = null
            )
        {
            string? existingValue;
            dictionary.TryGetValue(key, out existingValue);
            dictionary[key] =
                (existingValue ?? string.Empty)
                    .JoinDistinct(
                        separator,
                        values,
                        equalityComparer,
                        StringSplitOptions.RemoveEmptyEntries);
            return dictionary;
        }

        public static IDictionary<string, TValue> ToCaseInsensitiveDictionary<TValue>(this IReadOnlyDictionary<string, TValue> dictionary)
        {
            var destination = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, TValue> de in dictionary)
            {
                destination[de.Key] = de.Value;
            }

            return destination;
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey: notnull
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

    }
}
