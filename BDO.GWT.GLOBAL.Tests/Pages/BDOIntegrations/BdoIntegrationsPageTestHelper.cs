using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;

internal static class BdoIntegrationsPageTestHelper
{
    public static T CreateUninitialized<T>() where T : class =>
        (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    public static void PreparePageConfigurationForConfigurePage(object page)
    {
        var pageConfiguration = GetPageConfiguration(page);

        EnsureListProperty(pageConfiguration, "HeaderActions");
        EnsureListProperty(pageConfiguration, "TableActions");
        EnsureListProperty(pageConfiguration, "ColumnConfigurations");
        EnsureListProperty(pageConfiguration, "MassActions");
        EnsureListProperty(pageConfiguration, "QueryModifiers");
    }

    public static object GetPageConfiguration(object page)
    {
        var type = page.GetType();

        while (type is not null)
        {
            var property = type.GetProperty("PageConfiguration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property is not null)
            {
                var existing = property.GetValue(page);
                if (existing is not null)
                {
                    return existing;
                }

                var pageConfiguration = Activator.CreateInstance(property.PropertyType)
                    ?? RuntimeHelpers.GetUninitializedObject(property.PropertyType);

                if (property.SetMethod is not null)
                {
                    property.SetValue(page, pageConfiguration);
                    return pageConfiguration;
                }

                var backingField = FindMatchingField(page.GetType(), property.Name, property.PropertyType);
                backingField?.SetValue(page, pageConfiguration);

                var updated = property.GetValue(page);
                return updated ?? pageConfiguration;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("PageConfiguration property was not found.");
    }

    public static int GetEnumerableCount(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var value = property?.GetValue(target);

        return value switch
        {
            System.Collections.ICollection c => c.Count,
            System.Collections.IEnumerable e => e.Cast<object>().Count(),
            _ => 0,
        };
    }

    public static string? GetStringProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return property?.GetValue(target)?.ToString();
    }

    public static void SetPrivateField(object target, string fieldName, object? value)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var field = type.GetField(fieldName, flags);
            if (field is not null)
            {
                field.SetValue(target, value);
                return;
            }
        }

        throw new InvalidOperationException($"Field {fieldName} was not found.");
    }

    private static FieldInfo? FindMatchingField(Type type, string propertyName, Type fieldType)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        for (var current = type; current is not null; current = current.BaseType)
        {
            var field = current.GetField($"<{propertyName}>k__BackingField", flags)
                ?? current.GetField(propertyName, flags)
                ?? current.GetFields(flags)
                    .FirstOrDefault(f => f.FieldType == fieldType && f.Name.Contains(propertyName, StringComparison.OrdinalIgnoreCase));

            if (field is not null)
            {
                return field;
            }
        }

        return null;
    }

    private static void EnsureListProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property == null)
        {
            return;
        }

        var propertyType = property.PropertyType;
        if (!propertyType.IsGenericType)
        {
            return;
        }

        var elementType = propertyType.GetGenericArguments()[0];
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = Activator.CreateInstance(listType)!;

        if (property.SetMethod != null)
        {
            property.SetValue(target, list);
            return;
        }

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var backingField = target.GetType().GetField($"<{propertyName}>k__BackingField", flags)
            ?? target.GetType().GetField(propertyName, flags)
            ?? target.GetType().GetFields(flags)
                .FirstOrDefault(f => f.FieldType == propertyType && f.Name.Contains(propertyName, StringComparison.OrdinalIgnoreCase));

        backingField?.SetValue(target, list);
    }
}
