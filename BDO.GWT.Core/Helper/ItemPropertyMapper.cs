using System.Collections.Concurrent;
using System.Reflection;

namespace BDO.GWT.Core.Helpers;

/// <summary>
/// Provides cached, reflection-based access to common item properties (<c>Name</c>,
/// <c>CodeName</c> and <c>SystemFields.ContentItemName</c>) so that callers can read
/// these values uniformly across any content item / page type without each call
/// site having to special-case individual classes.
/// </summary>
public static class ItemPropertyMapper
{
    private const string NameProperty = "Name";
    private const string CodeNameProperty = "CodeName";
    private const string SystemFieldsProperty = "SystemFields";
    private const string ContentItemNameProperty = "ContentItemName";

    private static readonly ConcurrentDictionary<Type, ItemPropertyAccessors> _accessorsByType = new();

    /// <summary>
    /// Returns the cached <see cref="ItemPropertyAccessors"/> for the supplied type.
    /// </summary>
    public static ItemPropertyAccessors GetAccessors(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _accessorsByType.GetOrAdd(type, BuildAccessors);
    }

    /// <summary>
    /// Returns the cached <see cref="ItemPropertyAccessors"/> for <typeparamref name="T"/>.
    /// </summary>
    public static ItemPropertyAccessors GetAccessors<T>() => GetAccessors(typeof(T));

    /// <summary>
    /// Reads the <c>Name</c> property from the supplied item if available.
    /// </summary>
    public static string? GetName(object? item)
    {
        if (item is null)
        {
            return null;
        }

        return GetAccessors(item.GetType()).GetName?.Invoke(item);
    }

    /// <summary>
    /// Reads the <c>CodeName</c> property from the supplied item if available.
    /// </summary>
    public static string? GetCodeName(object? item)
    {
        if (item is null)
        {
            return null;
        }

        return GetAccessors(item.GetType()).GetCodeName?.Invoke(item);
    }

    /// <summary>
    /// Reads the <c>SystemFields.ContentItemName</c> property from the supplied item if available.
    /// </summary>
    public static string? GetContentItemName(object? item)
    {
        if (item is null)
        {
            return null;
        }

        return GetAccessors(item.GetType()).GetContentItemName?.Invoke(item);
    }

    /// <summary>
    /// Returns the best available display label for the supplied item, falling back
    /// from <c>Name</c> to <c>SystemFields.ContentItemName</c> to <c>CodeName</c>.
    /// </summary>
    public static string? GetDisplayLabel(object? item)
    {
        if (item is null)
        {
            return null;
        }

        var accessors = GetAccessors(item.GetType());
        return accessors.GetName?.Invoke(item)
            ?? accessors.GetContentItemName?.Invoke(item)
            ?? accessors.GetCodeName?.Invoke(item);
    }

    private static ItemPropertyAccessors BuildAccessors(Type type)
    {
        var nameProp = type.GetProperty(NameProperty, BindingFlags.Public | BindingFlags.Instance);
        var codeProp = type.GetProperty(CodeNameProperty, BindingFlags.Public | BindingFlags.Instance);
        var sysProp = type.GetProperty(SystemFieldsProperty, BindingFlags.Public | BindingFlags.Instance);

        Func<object, string?>? getName = nameProp is null
            ? null
            : x => nameProp.GetValue(x) as string;

        Func<object, string?>? getCodeName = codeProp is null
            ? null
            : x => codeProp.GetValue(x) as string;

        Func<object, string?>? getContentItemName = null;
        if (sysProp is not null)
        {
            var contentItemNameProp = sysProp.PropertyType
                .GetProperty(ContentItemNameProperty, BindingFlags.Public | BindingFlags.Instance);
            if (contentItemNameProp is not null)
            {
                getContentItemName = x =>
                {
                    var sys = sysProp.GetValue(x);
                    return sys is null ? null : contentItemNameProp.GetValue(sys) as string;
                };
            }
        }

        return new ItemPropertyAccessors(getName, getCodeName, getContentItemName);
    }
}

/// <summary>
/// A cached set of delegates for reading common item properties from any content
/// item / page type. Use <see cref="ItemPropertyMapper.GetAccessors(Type)"/> to obtain
/// an instance so the underlying reflection lookups are performed once per type.
/// </summary>
public readonly record struct ItemPropertyAccessors(
    Func<object, string?>? GetName,
    Func<object, string?>? GetCodeName,
    Func<object, string?>? GetContentItemName);
