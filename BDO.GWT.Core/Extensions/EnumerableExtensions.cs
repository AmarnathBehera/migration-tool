using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Common.Core.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Executes the provided selector function on each item in the collection in parallel, awaiting the results of all operations before returning. This is useful for performing asynchronous operations on a collection of items concurrently, improving performance when the operations are independent and can be executed simultaneously.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<TResult[]> SelectInParallelAsync<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, Task<TResult>> selector)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        var tasks = items.Select(selector);

        var results = await Task.WhenAll(tasks);

        return results;
    }

    /// <summary>
    /// Executes the provided selector function on each item in the collection sequentially, awaiting the result of each operation before moving on to the next item. This is useful when you need to ensure that each asynchronous operation completes before starting the next one, such as when dealing with rate-limited APIs or when order of execution matters.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<TResult[]> SelectOneByOneAsync<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, Task<TResult>> selector)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        var results = new List<TResult>();

        foreach (var item in items)
        {
            var result = await selector(item);

            results.Add(result);
        }

        return results.ToArray();
    }

    /// <summary>
    /// Creates a dictionary from an IEnumerable source, overwriting existing keys with new values if they already exist in the dictionary.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <param name="elementSelector"></param>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> ToDictionaryOverwriteExisting<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TElement>();

        foreach (var sourceValue in source)
        {
            dictionary[keySelector(sourceValue)] = elementSelector(sourceValue);
        }

        return dictionary;
    }

    /// <summary>
    /// Creates a dictionary from an IEnumerable source, ignoring new values for keys that already exist in the dictionary. If a key already exists, the existing value is retained and the new value is discarded.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <param name="elementSelector"></param>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> ToDictionaryIgnoreExisting<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TElement>();

        foreach (var sourceValue in source)
        {
            if (!dictionary.ContainsKey(keySelector(sourceValue)))
            {
                dictionary[keySelector(sourceValue)] = elementSelector(sourceValue);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Creates a dictionary from an IEnumerable of groupings, ensuring that all specified keys are present in the resulting dictionary. If a key from the keysToEnsure collection does not exist in the groupings, it will be added to the dictionary with an empty collection as its value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="groupings"></param>
    /// <param name="keysToEnsure"></param>
    /// <returns></returns>
    public static Dictionary<TKey, IEnumerable<TElement>> ToDictionaryWithEnsuredKeys<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groupings, IEnumerable<TKey> keysToEnsure) where TKey: notnull
    {
        var elementsByOptionalKey = groupings.ToDictionary(
            x => x.Key, 
            x => (IEnumerable<TElement>)[..x]
        );

        var elementsByEnsuredKey = keysToEnsure.ToDictionary(
            key => key,
            key => elementsByOptionalKey.TryGetValue(key, out var grouping)
                ? grouping
                : []
        );

        return elementsByEnsuredKey;
    }

    /// <summary>
    /// Creates a nested dictionary from an IEnumerable source, where the first level of keys is determined by the firstKeySelector, the second level of keys is determined by the secondKeySelector, and the values are determined by the elementSelector. If a key already exists in the nested dictionary, its value will be overwritten with the new value.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="source"></param>
    /// <param name="firstKeySelector"></param>
    /// <param name="secondKeySelector"></param>
    /// <param name="elementSelector"></param>
    /// <returns></returns>
    public static IDictionary<TKey1, IDictionary<TKey2, TElement>> ToNestedDictionary<TSource, TKey1, TKey2, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey1> firstKeySelector, Func<TSource, TKey2> secondKeySelector, Func<TSource, TElement> elementSelector)
        where TKey1 : notnull
        where TKey2 : notnull
    {
        var nestedDictionary = new Dictionary<TKey1, IDictionary<TKey2, TElement>>();

        foreach (var sourceElement in source)
        {
            var key1 = firstKeySelector(sourceElement);

            nestedDictionary.TryAdd(key1, new Dictionary<TKey2, TElement>());

            var key2 = secondKeySelector(sourceElement);

            nestedDictionary[key1][key2] = elementSelector(sourceElement);
        }

        return nestedDictionary;
    }

    /// <summary>
    /// Enumerates the items in the given IEnumerable along with their corresponding index, returning a new IEnumerable of tuples containing each item and its index. This is useful for scenarios where you need to access both the item and its position in the collection during iteration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <returns></returns>
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Select((item, index) => (item, index));
    }
}
