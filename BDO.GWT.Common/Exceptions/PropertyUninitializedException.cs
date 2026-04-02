using System;

namespace BDO.GWT.Common.Exceptions;

public class PropertyUninitializedException : Exception
{
    public PropertyUninitializedException(string propertyName) : base($"Property '{propertyName}' is uninitialized.") { }
}
