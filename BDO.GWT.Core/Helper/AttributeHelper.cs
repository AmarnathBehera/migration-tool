using BDO.GWT.Common.Core.Extensions;
using System;
using System.Reflection;

namespace BDO.GWT.Common.Core.Helpers;

public static class AttributeHelper
{
    public static TField GetAttributeField<TAttribute, TField>(MemberInfo property, string field)
        where TAttribute : Attribute
    {
        var attribute = property.GetCustomAttribute<TAttribute>();
        if (attribute is null)
        {
            throw new InvalidOperationException($"The expected attribute of type '{nameof(TAttribute)}' wasn't found on the property '{property.Name}'.");
        }

        var propertyValue = attribute.GetPropertyValue(field);

        if (propertyValue is null)
        {
            throw new InvalidOperationException($"'{nameof(TAttribute)}.{field}' is missing for property '{property.DeclaringType?.Name}.{nameof(property.Name)}'");
        }

        return (TField)propertyValue;
    }
}
