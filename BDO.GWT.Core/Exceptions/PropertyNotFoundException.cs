using System;

namespace BDO.GWT.Common.Core.Exceptions;

public class PropertyNotFoundException : Exception
{
    public PropertyNotFoundException(string propertyName) : base($"Property '{propertyName}' not found.") { }
}
