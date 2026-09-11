using BDO.GWT.Common.Core.Exceptions;

namespace BDO.GWT.Common.Core.Extensions;

public static class ObjectExtensions
{
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);

        if (property == null)
        {
            throw new PropertyNotFoundException($"Property '{propertyName}' does not exist.");
        }

        return property.GetValue(obj);
    }
}
