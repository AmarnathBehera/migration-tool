using System.Collections;

namespace BDO.GWT.GLOBAL.API.Extensions;

public static class DictionaryExtensions
{
    public static void AddOrEnrichEntry(this IDictionary<string, object> dictionary, string key, string value, char? separator = ' ')
    {
        if (!dictionary.TryAdd(key, value))
        {
            if (dictionary[key] is string)
            {
                dictionary[key] += $"{separator}{value}";
            }
        }
    }

    public static bool TryGetValue(this IDictionary dictionary, object key, out object? value)
    {
        if (dictionary.Contains(key))
        {
            value = dictionary[key];
            return true;
        }

        value = default;
        return false;
    }

    public static Dictionary<TValue, TKey> InvertKeyAndListValueDictionary<TKey, TValue>(this Dictionary<TKey, IEnumerable<TValue>> dictionary)
        where TValue : notnull
        where TKey : notnull
    {
        var result = new Dictionary<TValue, TKey>();
        foreach (var (key, value) in dictionary)
        {
            foreach (var item in value)
            {
                result.Add(item, key);
            }
        }

        return result;
    }
}
