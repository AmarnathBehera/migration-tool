using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Common.Core.Extensions;

public static class EnumerableExtensions
{
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

    public static Dictionary<TKey, TElement> ToDictionaryOverwriteExisting<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TElement>();

        foreach (var sourceValue in source)
        {
            dictionary[keySelector(sourceValue)] = elementSelector(sourceValue);
        }

        return dictionary;
    }

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

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Select((item, index) => (item, index));
    }
}
